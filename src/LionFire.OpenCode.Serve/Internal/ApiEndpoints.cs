namespace LionFire.OpenCode.Serve.Internal;

/// <summary>
/// Contains the API endpoint paths for the OpenCode serve API.
/// Based on the official OpenCode serve API specification version 0.0.3.
/// </summary>
internal static class ApiEndpoints
{
    // Global events
    public const string GlobalEvent = "/global/event";

    // Projects
    public const string Projects = "/project";
    public const string ProjectCurrent = "/project/current";

    // Sessions
    public const string Sessions = "/session";
    public const string SessionStatus = "/session/status";
    public static string Session(string sessionId) => $"/session/{sessionId}";
    public static string SessionAbort(string sessionId) => $"/session/{sessionId}/abort";
    public static string SessionChildren(string sessionId) => $"/session/{sessionId}/children";
    public static string SessionCommand(string sessionId) => $"/session/{sessionId}/command";
    public static string SessionDiff(string sessionId) => $"/session/{sessionId}/diff";
    public static string SessionFork(string sessionId) => $"/session/{sessionId}/fork";
    public static string SessionInit(string sessionId) => $"/session/{sessionId}/init";
    public static string SessionMessages(string sessionId) => $"/session/{sessionId}/message";
    public static string SessionMessage(string sessionId, string messageId) => $"/session/{sessionId}/message/{messageId}";
    public static string SessionPrompt(string sessionId) => $"/session/{sessionId}/message";
    public static string SessionPromptAsync(string sessionId) => $"/session/{sessionId}/prompt_async";
    public static string SessionRevert(string sessionId) => $"/session/{sessionId}/revert";
    public static string SessionUnrevert(string sessionId) => $"/session/{sessionId}/unrevert";
    public static string SessionShare(string sessionId) => $"/session/{sessionId}/share";
    public static string SessionUnshare(string sessionId) => $"/session/{sessionId}/share";
    public static string SessionShell(string sessionId) => $"/session/{sessionId}/shell";
    public static string SessionSummarize(string sessionId) => $"/session/{sessionId}/summarize";
    public static string SessionTodo(string sessionId) => $"/session/{sessionId}/todo";
    public static string SessionPermission(string sessionId, string permissionId) => $"/session/{sessionId}/permissions/{permissionId}";

    // Files
    public const string Files = "/file";
    public const string FileContent = "/file/content";
    public const string FileStatus = "/file/status";

    // Find
    public const string FindText = "/find";
    public const string FindFiles = "/find/file";
    public const string FindSymbols = "/find/symbol";

    // PTY
    public const string Ptys = "/pty";
    public static string Pty(string ptyId) => $"/pty/{ptyId}";
    public static string PtyConnect(string ptyId) => $"/pty/{ptyId}/connect";

    // MCP
    public const string Mcp = "/mcp";
    public static string McpAuth(string name) => $"/mcp/{name}/auth";
    public static string McpAuthAuthenticate(string name) => $"/mcp/{name}/auth/authenticate";
    public static string McpAuthCallback(string name) => $"/mcp/{name}/auth/callback";
    public static string McpConnect(string name) => $"/mcp/{name}/connect";
    public static string McpDisconnect(string name) => $"/mcp/{name}/disconnect";

    // Config
    public const string Config = "/config";
    public const string ConfigProviders = "/config/providers";

    // Providers
    public const string Providers = "/provider";
    public const string ProviderAuth = "/provider/auth";
    public static string ProviderOAuthAuthorize(string providerId) => $"/provider/{providerId}/oauth/authorize";
    public static string ProviderOAuthCallback(string providerId) => $"/provider/{providerId}/oauth/callback";

    // Auth
    public static string Auth(string providerId) => $"/auth/{providerId}";

    // Agents
    public const string Agents = "/agent";

    // Commands
    public const string Commands = "/command";

    // Tools (experimental)
    public const string Tools = "/experimental/tool";
    public const string ToolIds = "/experimental/tool/ids";

    // LSP
    public const string Lsp = "/lsp";

    // Formatter
    public const string Formatter = "/formatter";

    // Path
    public const string Path = "/path";

    // VCS
    public const string Vcs = "/vcs";

    // Events
    public const string Events = "/event";

    // Instance
    public const string InstanceDispose = "/instance/dispose";

    // Logging
    public const string Log = "/log";

    // TUI (Terminal UI)
    public const string TuiAppendPrompt = "/tui/append-prompt";
    public const string TuiClearPrompt = "/tui/clear-prompt";
    public const string TuiSubmitPrompt = "/tui/submit-prompt";
    public const string TuiExecuteCommand = "/tui/execute-command";
    public const string TuiOpenHelp = "/tui/open-help";
    public const string TuiOpenModels = "/tui/open-models";
    public const string TuiOpenSessions = "/tui/open-sessions";
    public const string TuiOpenThemes = "/tui/open-themes";
    public const string TuiShowToast = "/tui/show-toast";
    public const string TuiPublish = "/tui/publish";
    public const string TuiControlNext = "/tui/control/next";
    public const string TuiControlResponse = "/tui/control/response";
}
