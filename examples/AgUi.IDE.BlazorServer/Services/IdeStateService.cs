using LionFire.OpenCode.Serve.Models;

namespace AgUi.IDE.BlazorServer.Services;

/// <summary>
/// Manages the state of the IDE including selected files, active panels, etc.
/// </summary>
public class IdeStateService
{
    /// <summary>
    /// Currently selected file path in the file tree.
    /// </summary>
    public string? SelectedFilePath { get; private set; }

    /// <summary>
    /// Currently selected file name.
    /// </summary>
    public string? SelectedFileName { get; private set; }

    /// <summary>
    /// Current workspace/project root path.
    /// </summary>
    public string? WorkspacePath { get; private set; }

    /// <summary>
    /// Whether the OpenCode backend is connected.
    /// </summary>
    public bool IsConnected { get; private set; } = true; // Demo: assume connected

    /// <summary>
    /// Current session ID if connected.
    /// </summary>
    public string? SessionId { get; private set; }

    /// <summary>
    /// Current model ID.
    /// </summary>
    public string? CurrentModelId { get; private set; }

    /// <summary>
    /// Current model name (display friendly).
    /// </summary>
    public string? CurrentModelName { get; private set; }

    /// <summary>
    /// Current provider ID.
    /// </summary>
    public string? CurrentProviderId { get; private set; }

    /// <summary>
    /// Current provider name (display friendly).
    /// </summary>
    public string? CurrentProviderName { get; private set; }

    /// <summary>
    /// Current agent ID (null for default).
    /// </summary>
    public string? CurrentAgentId { get; private set; }

    /// <summary>
    /// Current agent name.
    /// </summary>
    public string? CurrentAgentName { get; private set; }

    /// <summary>
    /// Available agents.
    /// </summary>
    public List<Agent> AvailableAgents { get; private set; } = new();

    /// <summary>
    /// Available providers with models.
    /// </summary>
    public List<Provider> AvailableProviders { get; private set; } = new();

    /// <summary>
    /// Files currently added to chat context.
    /// </summary>
    public List<string> ContextFiles { get; } = new();

    /// <summary>
    /// Event raised when a file is selected.
    /// </summary>
    public event Action<string>? OnFileSelected;

    /// <summary>
    /// Event raised when the workspace changes.
    /// </summary>
    public event Action<string>? OnWorkspaceChanged;

    /// <summary>
    /// Event raised when connection status changes.
    /// </summary>
    public event Action<bool>? OnConnectionChanged;

    /// <summary>
    /// Event raised when context files change.
    /// </summary>
    public event Action? OnContextFilesChanged;

    /// <summary>
    /// Event raised when state changes (general).
    /// </summary>
    public event Action? OnStateChanged;

    /// <summary>
    /// Select a file in the IDE.
    /// </summary>
    public void SelectFile(string filePath, string? fileName = null)
    {
        SelectedFilePath = filePath;
        SelectedFileName = fileName ?? Path.GetFileName(filePath);
        OnFileSelected?.Invoke(filePath);
        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Set the workspace root path.
    /// </summary>
    public void SetWorkspace(string workspacePath)
    {
        WorkspacePath = workspacePath;
        OnWorkspaceChanged?.Invoke(workspacePath);
        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Add a file to the chat context.
    /// </summary>
    public void AddToContext(string filePath)
    {
        if (!ContextFiles.Contains(filePath))
        {
            ContextFiles.Add(filePath);
            OnContextFilesChanged?.Invoke();
            OnStateChanged?.Invoke();
        }
    }

    /// <summary>
    /// Remove a file from the chat context.
    /// </summary>
    public void RemoveFromContext(string filePath)
    {
        if (ContextFiles.Remove(filePath))
        {
            OnContextFilesChanged?.Invoke();
            OnStateChanged?.Invoke();
        }
    }

    /// <summary>
    /// Clear all context files.
    /// </summary>
    public void ClearContext()
    {
        ContextFiles.Clear();
        OnContextFilesChanged?.Invoke();
        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Set connection status.
    /// </summary>
    public void SetConnected(bool connected, string? sessionId = null)
    {
        IsConnected = connected;
        SessionId = sessionId;
        OnConnectionChanged?.Invoke(connected);
        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Set the current model and provider.
    /// </summary>
    public void SetModel(string? providerId, string? providerName, string? modelId, string? modelName)
    {
        CurrentProviderId = providerId;
        CurrentProviderName = providerName;
        CurrentModelId = modelId;
        CurrentModelName = modelName;
        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Set the current agent.
    /// </summary>
    public void SetAgent(string? agentId, string? agentName)
    {
        CurrentAgentId = agentId;
        CurrentAgentName = agentName;
        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Set available agents.
    /// </summary>
    public void SetAvailableAgents(List<Agent> agents)
    {
        AvailableAgents = agents;
        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Set available providers.
    /// </summary>
    public void SetAvailableProviders(List<Provider> providers)
    {
        AvailableProviders = providers;
        OnStateChanged?.Invoke();
    }
}
