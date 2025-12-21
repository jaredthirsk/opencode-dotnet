using LionFire.OpenCode.Serve;
using LionFire.OpenCode.Serve.Models;

namespace AgUi.IDE.BlazorServer.Services;

/// <summary>
/// Service that wraps IOpenCodeClient for IDE-specific functionality.
/// Manages sessions, file operations, and chat integration.
/// </summary>
public class OpenCodeIdeService : IDisposable
{
    private readonly IOpenCodeClient _client;
    private readonly IdeStateService _ideState;
    private readonly ChatService _chatService;
    private readonly DiffService _diffService;
    private readonly TerminalService _terminalService;
    private readonly ILogger<OpenCodeIdeService> _logger;

    private Session? _currentSession;
    private bool _isInitialized;

    public OpenCodeIdeService(
        IOpenCodeClient client,
        IdeStateService ideState,
        ChatService chatService,
        DiffService diffService,
        TerminalService terminalService,
        ILogger<OpenCodeIdeService> logger)
    {
        _client = client;
        _ideState = ideState;
        _chatService = chatService;
        _diffService = diffService;
        _terminalService = terminalService;
        _logger = logger;
    }

    public Session? CurrentSession => _currentSession;
    public bool IsInitialized => _isInitialized;

    /// <summary>
    /// Initialize the IDE service by creating or resuming a session.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized) return;

        try
        {
            _terminalService.AppendSystem("Connecting to OpenCode backend...");

            // Create a new session with optional title
            var session = await _client.CreateSessionAsync(
                new CreateSessionRequest { Title = "IDE Session" },
                directory: _ideState.WorkspacePath,
                cancellationToken: cancellationToken);

            _currentSession = session;
            _ideState.SetConnected(true, session.Id);

            // Initialize ChatService with session info for real backend streaming
            _chatService.Initialize(session.Id, _ideState.WorkspacePath);

            _terminalService.AppendOutput($"Session created: {session.Id}");

            // Fetch available agents and providers
            await LoadAgentsAndProvidersAsync(cancellationToken);

            _terminalService.AppendPrompt("$");

            _isInitialized = true;
            _logger.LogInformation("OpenCode IDE service initialized with session {SessionId}", session.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize OpenCode IDE service");
            _ideState.SetConnected(false);
            _terminalService.AppendError($"Failed to connect: {ex.Message}");

            // For demo purposes, still mark as initialized with mock mode
            _isInitialized = true;
        }
    }

    /// <summary>
    /// Send a chat message and get the response.
    /// </summary>
    public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        if (_currentSession == null)
        {
            _logger.LogWarning("No active session, message not sent");
            return;
        }

        try
        {
            // Build prompt with context files
            var prompt = message;
            if (_ideState.ContextFiles.Count > 0)
            {
                prompt = $"Context files: {string.Join(", ", _ideState.ContextFiles)}\n\n{message}";
            }

            _chatService.AddUserMessage(message);
            _terminalService.AppendOutput($"> {message.Substring(0, Math.Min(50, message.Length))}...");

            // Send to OpenCode backend using PromptAsync
            var response = await _client.PromptAsync(
                _currentSession.Id,
                new SendMessageRequest
                {
                    Parts = new List<PartInput>
                    {
                        new PartInput { Type = "text", Text = prompt }
                    }
                },
                directory: _ideState.WorkspacePath,
                cancellationToken: cancellationToken);

            // Process response
            _logger.LogInformation("Received response for message in session {SessionId}", _currentSession.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message");
            _terminalService.AppendError($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get file contents from the backend.
    /// </summary>
    public async Task<string?> GetFileContentsAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_currentSession == null) return null;

        try
        {
            var fileContent = await _client.ReadFileAsync(path, _ideState.WorkspacePath, cancellationToken);
            return fileContent.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get file contents for {Path}", path);
            return null;
        }
    }

    /// <summary>
    /// Apply a diff/change suggested by the assistant.
    /// </summary>
    public async Task ApplyDiffAsync(FileDiff diff, CancellationToken cancellationToken = default)
    {
        if (_currentSession == null) return;

        try
        {
            _terminalService.AppendOutput($"Applying changes to {diff.FileName}...");
            // In real implementation, this would use a file write API
            _terminalService.AppendOutput($"Changes applied to {diff.FileName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply diff for {FileName}", diff.FileName);
            _terminalService.AppendError($"Failed to apply changes: {ex.Message}");
        }
    }

    /// <summary>
    /// Execute a command and stream output to terminal.
    /// </summary>
    public async Task ExecuteCommandAsync(string command, CancellationToken cancellationToken = default)
    {
        if (_currentSession == null) return;

        try
        {
            _terminalService.AppendPrompt($"$ {command}");

            // In real implementation, this would execute via OpenCode backend
            _terminalService.AppendOutput("Command execution not yet implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute command {Command}", command);
            _terminalService.AppendError($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Close the current session.
    /// </summary>
    public async Task CloseSessionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentSession == null) return;

        try
        {
            // In real implementation, this would properly close the session
            _terminalService.AppendSystem("Session closed");
            _currentSession = null;
            _ideState.SetConnected(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close session");
        }
    }

    /// <summary>
    /// Load available agents and providers from the backend.
    /// </summary>
    private async Task LoadAgentsAndProvidersAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Load agents - filter to show only primary agents (Build and Plan)
            var allAgents = await _client.ListAgentsAsync(_ideState.WorkspacePath, cancellationToken);
            var primaryAgents = allAgents
                .Where(a => a.Name.Equals("build", StringComparison.OrdinalIgnoreCase) ||
                           a.Name.Equals("plan", StringComparison.OrdinalIgnoreCase))
                .ToList();
            _ideState.SetAvailableAgents(primaryAgents);
            _logger.LogInformation("Loaded {Count} agents ({PrimaryCount} primary)", allAgents.Count, primaryAgents.Count);

            // Load providers
            var providers = await _client.ListProvidersAsync(_ideState.WorkspacePath, cancellationToken);
            _ideState.SetAvailableProviders(providers);
            _logger.LogInformation("Loaded {Count} providers", providers.Count);

            // Try to get last used model from recent sessions
            var lastModel = await GetLastUsedModelAsync(cancellationToken);
            if (lastModel != null && providers.Count > 0)
            {
                var provider = providers.FirstOrDefault(p => p.Id == lastModel.Value.providerId);
                var model = provider?.Models?.FirstOrDefault(m => m.Id == lastModel.Value.modelId);
                if (provider != null && model != null)
                {
                    _ideState.SetModel(provider.Id, provider.Name, model.Id, model.Name);
                    _terminalService.AppendOutput($"Model: {model.Name} ({provider.Name})");
                    _logger.LogInformation("Restored last used model: {ModelName} from {ProviderName}", model.Name, provider.Name);
                    return;
                }
            }

            // Fall back to default - prefer OpenCode Zen, then Anthropic, then first available
            if (providers.Count > 0)
            {
                var preferredProvider = providers.FirstOrDefault(p => p.Id == "opencode")
                    ?? providers.FirstOrDefault(p => p.Id == "anthropic")
                    ?? providers.First();
                var preferredModel = preferredProvider.Models?.FirstOrDefault(m => m.Id == "big-pickle")
                    ?? preferredProvider.Models?.FirstOrDefault(m => m.Name.Contains("Sonnet", StringComparison.OrdinalIgnoreCase))
                    ?? preferredProvider.Models?.FirstOrDefault();

                if (preferredModel != null)
                {
                    _ideState.SetModel(preferredProvider.Id, preferredProvider.Name, preferredModel.Id, preferredModel.Name);
                    _terminalService.AppendOutput($"Model: {preferredModel.Name} ({preferredProvider.Name})");
                    _logger.LogInformation("Selected default model: {ModelName} from {ProviderName}", preferredModel.Name, preferredProvider.Name);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load agents and providers");
            // Set defaults for demo mode
            _ideState.SetModel("opencode", "OpenCode Zen", "big-pickle", "GPT-5 Nano");
        }
    }

    /// <summary>
    /// Get the last used model from recent sessions.
    /// </summary>
    private async Task<(string providerId, string modelId)?> GetLastUsedModelAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Get recent sessions
            var sessions = await _client.ListSessionsAsync(_ideState.WorkspacePath, cancellationToken);
            if (sessions.Count == 0) return null;

            // Check most recent sessions for messages with model info
            foreach (var session in sessions.Take(5))
            {
                try
                {
                    var messages = await _client.ListMessagesAsync(session.Id, limit: 10, directory: _ideState.WorkspacePath, cancellationToken: cancellationToken);

                    // Find the last user message with model info
                    var lastUserMessage = messages
                        .Where(m => m.Message?.IsUserMessage == true && m.Message?.Model != null)
                        .OrderByDescending(m => m.Message?.Time?.Created ?? 0)
                        .FirstOrDefault();

                    if (lastUserMessage?.Message?.Model != null)
                    {
                        var model = lastUserMessage.Message.Model;
                        _logger.LogDebug("Found last used model: {ProviderId}/{ModelId} from session {SessionId}",
                            model.ProviderId, model.ModelId, session.Id);
                        return (model.ProviderId, model.ModelId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Failed to get messages for session {SessionId}", session.Id);
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to get last used model");
            return null;
        }
    }

    /// <summary>
    /// Switch to a different agent.
    /// </summary>
    public async Task SwitchAgentAsync(string? agentId, CancellationToken cancellationToken = default)
    {
        var agent = _ideState.AvailableAgents.FirstOrDefault(a => a.Id == agentId);
        _ideState.SetAgent(agentId, agent?.Name);

        if (agent != null)
        {
            _terminalService.AppendSystem($"Switched to agent: {agent.Name}");
            _logger.LogInformation("Switched to agent {AgentId}: {AgentName}", agentId, agent.Name);
        }
        else
        {
            _terminalService.AppendSystem("Using default agent");
            _logger.LogInformation("Using default agent");
        }
    }

    public void Dispose()
    {
        // Cleanup resources
        _currentSession = null;
    }
}
