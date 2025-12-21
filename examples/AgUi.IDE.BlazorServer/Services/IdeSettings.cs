namespace AgUi.IDE.BlazorServer.Services;

/// <summary>
/// Configuration settings for the IDE application.
/// </summary>
public class IdeSettings
{
    /// <summary>
    /// Default workspace path to open on startup.
    /// </summary>
    public string DefaultWorkspacePath { get; set; } = "";
}
