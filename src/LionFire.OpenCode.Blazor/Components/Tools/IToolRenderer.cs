namespace LionFire.OpenCode.Blazor.Components.Tools;

/// <summary>
/// Interface for tool-specific renderers.
/// </summary>
public interface IToolRenderer
{
    /// <summary>
    /// Tool name(s) this renderer handles.
    /// </summary>
    IEnumerable<string> SupportedTools { get; }

    /// <summary>
    /// Check if this renderer can handle a specific tool.
    /// </summary>
    bool CanHandle(string toolName);

    /// <summary>
    /// Get the display icon for this tool.
    /// </summary>
    string GetIcon(string toolName);

    /// <summary>
    /// Get a short summary description for the tool call.
    /// </summary>
    string GetSummary(string toolName, object? input);
}

/// <summary>
/// Tool execution state.
/// </summary>
public enum ToolState
{
    Pending,
    Running,
    Completed,
    Error
}

/// <summary>
/// Tool call data passed to renderers.
/// </summary>
public class ToolCallData
{
    public string ToolName { get; set; } = "";
    public string? CallId { get; set; }
    public ToolState State { get; set; } = ToolState.Pending;
    public object? Input { get; set; }
    public object? Output { get; set; }
    public bool IsExpanded { get; set; }
    public bool HasPendingPermission { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
}
