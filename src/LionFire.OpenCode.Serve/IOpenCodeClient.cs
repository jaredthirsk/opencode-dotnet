using LionFire.OpenCode.Serve.Models;
using LionFire.OpenCode.Serve.Models.Events;

namespace LionFire.OpenCode.Serve;

/// <summary>
/// Interface for the OpenCode client, providing access to all OpenCode serve API operations.
/// </summary>
/// <remarks>
/// <para>
/// This interface defines the contract for communicating with an OpenCode serve API
/// instance. It provides comprehensive access to all 67+ endpoints including:
/// sessions, messages, files, PTYs, MCP servers, permissions, events, and more.
/// </para>
/// <para>
/// Implementations should be thread-safe and support concurrent operations.
/// Most operations support an optional directory parameter that scopes the operation
/// to a specific instance/directory context.
/// </para>
/// <para>
/// Based on OpenCode serve API specification version 0.0.3.
/// </para>
/// </remarks>
/// <example>
/// Basic usage:
/// <code>
/// await using var client = new OpenCodeClient();
///
/// // Create and interact with a session
/// var session = await client.CreateSessionAsync(directory: "/my/project");
/// var message = await client.PromptAsync(session.Id, new SendMessageRequest
/// {
///     Parts = new List&lt;PartInput&gt; { new TextPartInput { Type = "text", Text = "Hello!" } }
/// });
///
/// // Stream events
/// await foreach (var ev in client.SubscribeToEventsAsync(directory: "/my/project"))
/// {
///     Console.WriteLine($"Event: {ev.Type}");
/// }
/// </code>
/// </example>
public interface IOpenCodeClient : IAsyncDisposable
{
    #region Global Events

    /// <summary>
    /// Subscribes to global events from the OpenCode system using server-sent events.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An async enumerable of <see cref="GlobalEvent"/> instances.</returns>
    IAsyncEnumerable<GlobalEvent> SubscribeToGlobalEventsAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Projects

    /// <summary>
    /// Lists all projects that have been opened with OpenCode.
    /// </summary>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of <see cref="Project"/> instances.</returns>
    Task<List<Project>> ListProjectsAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current project for the specified directory.
    /// </summary>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The current <see cref="Project"/>.</returns>
    Task<Project> GetCurrentProjectAsync(string? directory = null, CancellationToken cancellationToken = default);

    #endregion

    #region Sessions

    /// <summary>
    /// Lists all sessions.
    /// </summary>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of <see cref="Session"/> instances.</returns>
    Task<List<Session>> ListSessionsAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new session.
    /// </summary>
    /// <param name="request">Optional request with parentID and title.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created <see cref="Session"/>.</returns>
    Task<Session> CreateSessionAsync(CreateSessionRequest? request = null, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the session status for the specified directory.
    /// </summary>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="SessionStatus"/>.</returns>
    Task<SessionStatus> GetSessionStatusAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a session by ID.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="Session"/>.</returns>
    Task<Session> GetSessionAsync(string sessionId, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a session.
    /// </summary>
    /// <param name="sessionId">The session ID to delete.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task DeleteSessionAsync(string sessionId, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a session (e.g., changes title).
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="request">The update request.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated <see cref="Session"/>.</returns>
    Task<Session> UpdateSessionAsync(string sessionId, UpdateSessionRequest request, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aborts a running session.
    /// </summary>
    /// <param name="sessionId">The session ID to abort.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AbortSessionAsync(string sessionId, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets child sessions of a session.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of child <see cref="Session"/> instances.</returns>
    Task<List<Session>> GetSessionChildrenAsync(string sessionId, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a command in a session.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="request">The command execution request.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task ExecuteSessionCommandAsync(string sessionId, ExecuteCommandRequest request, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the diff for a session (or up to a specific message).
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="messageId">Optional message ID to diff up to.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of <see cref="FileDiff"/> instances.</returns>
    Task<List<FileDiff>> GetSessionDiffAsync(string sessionId, string? messageId = null, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Forks a session, creating a new session from an existing one.
    /// </summary>
    /// <param name="sessionId">The session ID to fork.</param>
    /// <param name="request">Optional fork request with messageID.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The forked <see cref="Session"/>.</returns>
    Task<Session> ForkSessionAsync(string sessionId, ForkSessionRequest? request = null, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes a session with agent and model settings.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="request">The initialization request.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task InitializeSessionAsync(string sessionId, InitSessionRequest request, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reverts a session to a previous message state.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="request">The revert request specifying which message to revert to.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task RevertSessionAsync(string sessionId, RevertSessionRequest request, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unrests a previously reverted session.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task UnrevertSessionAsync(string sessionId, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Shares a session, generating a shareable URL.
    /// </summary>
    /// <param name="sessionId">The session ID to share.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="Session"/> with share information.</returns>
    Task<Session> ShareSessionAsync(string sessionId, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unshares a session, removing the shareable URL.
    /// </summary>
    /// <param name="sessionId">The session ID to unshare.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task UnshareSessionAsync(string sessionId, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a shell command in a session.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="request">The shell command execution request.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task ExecuteSessionShellAsync(string sessionId, ExecuteShellRequest request, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Summarizes a session (creates a compact summary).
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="request">Optional summarize request with messageID.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SummarizeSessionAsync(string sessionId, SummarizeSessionRequest? request = null, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the todo list for a session.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of <see cref="Todo"/> items.</returns>
    Task<List<Todo>> GetSessionTodosAsync(string sessionId, string? directory = null, CancellationToken cancellationToken = default);

    #endregion

    #region Messages

    /// <summary>
    /// Lists messages in a session.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="limit">Optional limit on number of messages to return.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of <see cref="MessageWithParts"/> instances.</returns>
    Task<List<MessageWithParts>> ListMessagesAsync(string sessionId, int? limit = null, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a prompt to a session and waits for the complete response.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="request">The message request.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="MessageWithParts"/> response.</returns>
    Task<MessageWithParts> PromptAsync(string sessionId, SendMessageRequest request, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a prompt to a session asynchronously (non-blocking).
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="request">The message request.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task PromptAsyncNonBlocking(string sessionId, SendMessageRequest request, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific message from a session.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="messageId">The message ID.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="MessageWithParts"/>.</returns>
    Task<MessageWithParts> GetMessageAsync(string sessionId, string messageId, string? directory = null, CancellationToken cancellationToken = default);

    #endregion

    #region Permissions

    /// <summary>
    /// Responds to a permission request.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="permissionId">The permission ID.</param>
    /// <param name="response">The permission response (allow/deny).</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task RespondToPermissionAsync(string sessionId, string permissionId, PermissionResponse response, string? directory = null, CancellationToken cancellationToken = default);

    #endregion

    #region Files

    /// <summary>
    /// Lists files in a directory.
    /// </summary>
    /// <param name="path">The path to list.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of <see cref="FileNode"/> instances.</returns>
    Task<List<FileNode>> ListFilesAsync(string path, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the content of a file.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="FileContent"/>.</returns>
    Task<FileContent> ReadFileAsync(string path, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets file status (modified, added, deleted files).
    /// </summary>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="FileStatus"/>.</returns>
    Task<FileStatus> GetFileStatusAsync(string? directory = null, CancellationToken cancellationToken = default);

    #endregion

    #region Find

    /// <summary>
    /// Searches for text in files.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Search results as a list of strings.</returns>
    Task<List<string>> FindTextAsync(string query, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for files by name/pattern.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="dirs">Optional directories to search in.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of matching file paths.</returns>
    Task<List<string>> FindFilesAsync(string query, string? dirs = null, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for symbols in code.
    /// </summary>
    /// <param name="query">The symbol search query.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of <see cref="Symbol"/> instances.</returns>
    Task<List<Symbol>> FindSymbolsAsync(string query, string? directory = null, CancellationToken cancellationToken = default);

    #endregion

    #region PTY (Pseudo-Terminal)

    /// <summary>
    /// Lists all PTY instances.
    /// </summary>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of <see cref="Pty"/> instances.</returns>
    Task<List<Pty>> ListPtysAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new PTY.
    /// </summary>
    /// <param name="request">The PTY creation request.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created <see cref="Pty"/>.</returns>
    Task<Pty> CreatePtyAsync(CreatePtyRequest request, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a PTY by ID.
    /// </summary>
    /// <param name="ptyId">The PTY ID.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="Pty"/>.</returns>
    Task<Pty> GetPtyAsync(string ptyId, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a PTY (e.g., changes title).
    /// </summary>
    /// <param name="ptyId">The PTY ID.</param>
    /// <param name="request">The update request.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated <see cref="Pty"/>.</returns>
    Task<Pty> UpdatePtyAsync(string ptyId, UpdatePtyRequest request, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a PTY.
    /// </summary>
    /// <param name="ptyId">The PTY ID to delete.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task DeletePtyAsync(string ptyId, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Connects to a PTY for reading/writing (returns a WebSocket-like stream).
    /// </summary>
    /// <param name="ptyId">The PTY ID.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An async enumerable of output data.</returns>
    IAsyncEnumerable<string> ConnectToPtyAsync(string ptyId, string? directory = null, CancellationToken cancellationToken = default);

    #endregion

    #region MCP (Model Context Protocol)

    /// <summary>
    /// Gets the status of all MCP servers.
    /// </summary>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of <see cref="McpStatus"/> instances.</returns>
    Task<List<McpStatus>> GetMcpStatusAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new MCP server.
    /// </summary>
    /// <param name="request">The MCP server configuration.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AddMcpServerAsync(AddMcpServerRequest request, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts authentication for an MCP server.
    /// </summary>
    /// <param name="name">The MCP server name.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task StartMcpAuthAsync(string name, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes authentication for an MCP server.
    /// </summary>
    /// <param name="name">The MCP server name.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task RemoveMcpAuthAsync(string name, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates an MCP server.
    /// </summary>
    /// <param name="name">The MCP server name.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AuthenticateMcpAsync(string name, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles OAuth callback for MCP server authentication.
    /// </summary>
    /// <param name="name">The MCP server name.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task HandleMcpAuthCallbackAsync(string name, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Connects to an MCP server.
    /// </summary>
    /// <param name="name">The MCP server name.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task ConnectMcpAsync(string name, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects from an MCP server.
    /// </summary>
    /// <param name="name">The MCP server name.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task DisconnectMcpAsync(string name, string? directory = null, CancellationToken cancellationToken = default);

    #endregion

    #region Configuration

    /// <summary>
    /// Gets the OpenCode configuration.
    /// </summary>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The configuration as a dictionary.</returns>
    Task<Dictionary<string, object>> GetConfigAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the OpenCode configuration.
    /// </summary>
    /// <param name="config">The configuration updates.</param>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task UpdateConfigAsync(Dictionary<string, object> config, string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available provider configurations.
    /// </summary>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A dictionary of provider configurations.</returns>
    Task<Dictionary<string, object>> GetConfigProvidersAsync(string? directory = null, CancellationToken cancellationToken = default);

    #endregion

    #region Providers

    /// <summary>
    /// Lists all available AI providers.
    /// </summary>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of <see cref="Provider"/> instances.</returns>
    Task<List<Provider>> ListProvidersAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets provider authentication information.
    /// </summary>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of <see cref="ProviderAuth"/> instances.</returns>
    Task<List<ProviderAuth>> GetProviderAuthAsync(string? directory = null, CancellationToken cancellationToken = default);

    #endregion

    #region Other Operations

    /// <summary>
    /// Gets a list of available agents.
    /// </summary>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of <see cref="Agent"/> instances.</returns>
    Task<List<Agent>> ListAgentsAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists available slash commands.
    /// </summary>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of <see cref="Command"/> instances.</returns>
    Task<List<Command>> ListCommandsAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the LSP (Language Server Protocol) status.
    /// </summary>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="LspStatus"/>.</returns>
    Task<LspStatus> GetLspStatusAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the formatter status.
    /// </summary>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="FormatterStatus"/>.</returns>
    Task<FormatterStatus> GetFormatterStatusAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets path information (cwd and root).
    /// </summary>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="PathInfo"/>.</returns>
    Task<PathInfo> GetPathAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets version control system information.
    /// </summary>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="VcsInfo"/>.</returns>
    Task<VcsInfo> GetVcsInfoAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to events for a specific directory/instance.
    /// </summary>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An async enumerable of <see cref="Event"/> instances.</returns>
    IAsyncEnumerable<Event> SubscribeToEventsAsync(string? directory = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disposes the current instance.
    /// </summary>
    /// <param name="directory">Optional directory context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task DisposeInstanceAsync(string? directory = null, CancellationToken cancellationToken = default);

    #endregion
}
