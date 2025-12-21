using LionFire.OpenCode.Serve;
using LionFire.OpenCode.Serve.Models;

namespace AgUi.IDE.BlazorServer.Services;

/// <summary>
/// Service for managing permission requests.
/// Tracks pending permissions and handles user responses.
/// </summary>
public class PermissionService
{
    private readonly IOpenCodeClient? _client;
    private readonly ILogger<PermissionService> _logger;
    private readonly Dictionary<string, List<Permission>> _pendingPermissions = new();
    private readonly object _lock = new();

    public PermissionService(IOpenCodeClient? client = null, ILogger<PermissionService>? logger = null)
    {
        _client = client;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<PermissionService>.Instance;
    }

    /// <summary>
    /// Event fired when permissions change.
    /// </summary>
    public event Action? PermissionsChanged;

    /// <summary>
    /// Get pending permissions for a session.
    /// </summary>
    public IReadOnlyList<Permission> GetPendingPermissions(string sessionId)
    {
        lock (_lock)
        {
            if (_pendingPermissions.TryGetValue(sessionId, out var permissions))
            {
                return permissions.AsReadOnly();
            }
            return Array.Empty<Permission>();
        }
    }

    /// <summary>
    /// Get all pending permissions across all sessions.
    /// </summary>
    public IReadOnlyList<Permission> GetAllPendingPermissions()
    {
        lock (_lock)
        {
            return _pendingPermissions.Values.SelectMany(p => p).ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Check if a tool call has a pending permission.
    /// </summary>
    public bool HasPendingPermission(string sessionId, string? callId)
    {
        if (string.IsNullOrEmpty(callId)) return false;

        lock (_lock)
        {
            if (_pendingPermissions.TryGetValue(sessionId, out var permissions))
            {
                return permissions.Any(p => p.CallId == callId);
            }
            return false;
        }
    }

    /// <summary>
    /// Add a permission request (from SSE event).
    /// </summary>
    public void AddPermission(Permission permission)
    {
        lock (_lock)
        {
            if (!_pendingPermissions.TryGetValue(permission.SessionId, out var permissions))
            {
                permissions = new List<Permission>();
                _pendingPermissions[permission.SessionId] = permissions;
            }

            // Avoid duplicates
            if (!permissions.Any(p => p.Id == permission.Id))
            {
                permissions.Add(permission);
                _logger.LogInformation("Permission request added: {PermissionId} for {Title}", permission.Id, permission.Title);
            }
        }

        PermissionsChanged?.Invoke();
    }

    /// <summary>
    /// Remove a permission (after response or cancellation).
    /// </summary>
    public void RemovePermission(string sessionId, string permissionId)
    {
        lock (_lock)
        {
            if (_pendingPermissions.TryGetValue(sessionId, out var permissions))
            {
                var removed = permissions.RemoveAll(p => p.Id == permissionId);
                if (removed > 0)
                {
                    _logger.LogInformation("Permission removed: {PermissionId}", permissionId);
                }
            }
        }

        PermissionsChanged?.Invoke();
    }

    /// <summary>
    /// Respond to a permission request.
    /// </summary>
    public async Task RespondAsync(string sessionId, string permissionId, bool allow, bool remember = false, string? directory = null, CancellationToken cancellationToken = default)
    {
        if (_client == null)
        {
            _logger.LogWarning("No OpenCode client available to respond to permission");
            RemovePermission(sessionId, permissionId);
            return;
        }

        try
        {
            var response = new PermissionResponse
            {
                Allow = allow,
                Remember = remember ? true : null
            };

            await _client.RespondToPermissionAsync(sessionId, permissionId, response, directory, cancellationToken);
            _logger.LogInformation("Permission response sent: {PermissionId} allow={Allow} remember={Remember}", permissionId, allow, remember);

            // Remove from pending (the SSE event will also trigger removal, but we do it here too for responsiveness)
            RemovePermission(sessionId, permissionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to respond to permission {PermissionId}", permissionId);
            throw;
        }
    }

    /// <summary>
    /// Approve a permission once.
    /// </summary>
    public Task ApproveOnceAsync(string sessionId, string permissionId, string? directory = null, CancellationToken cancellationToken = default)
        => RespondAsync(sessionId, permissionId, allow: true, remember: false, directory, cancellationToken);

    /// <summary>
    /// Approve a permission and remember for session.
    /// </summary>
    public Task ApproveAlwaysAsync(string sessionId, string permissionId, string? directory = null, CancellationToken cancellationToken = default)
        => RespondAsync(sessionId, permissionId, allow: true, remember: true, directory, cancellationToken);

    /// <summary>
    /// Deny a permission.
    /// </summary>
    public Task DenyAsync(string sessionId, string permissionId, string? directory = null, CancellationToken cancellationToken = default)
        => RespondAsync(sessionId, permissionId, allow: false, remember: false, directory, cancellationToken);

    /// <summary>
    /// Clear all permissions for a session.
    /// </summary>
    public void ClearSession(string sessionId)
    {
        lock (_lock)
        {
            _pendingPermissions.Remove(sessionId);
        }

        PermissionsChanged?.Invoke();
    }
}
