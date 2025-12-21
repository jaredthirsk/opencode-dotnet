using System.Text.Json.Serialization;

namespace LionFire.OpenCode.Serve.Models;

/// <summary>
/// Represents an OpenCode session.
/// </summary>
public record Session
{
    /// <summary>
    /// The unique identifier of the session (pattern: ^ses.*).
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// The project ID this session belongs to.
    /// </summary>
    [JsonPropertyName("projectID")]
    public required string ProjectId { get; init; }

    /// <summary>
    /// The working directory for the session.
    /// </summary>
    [JsonPropertyName("directory")]
    public required string Directory { get; init; }

    /// <summary>
    /// The parent session ID if this is a forked session (pattern: ^ses.*).
    /// </summary>
    [JsonPropertyName("parentID")]
    public string? ParentId { get; init; }

    /// <summary>
    /// Summary statistics for the session.
    /// </summary>
    [JsonPropertyName("summary")]
    public SessionSummary? Summary { get; init; }

    /// <summary>
    /// Share information if the session is shared.
    /// </summary>
    [JsonPropertyName("share")]
    public SessionShare? Share { get; init; }

    /// <summary>
    /// The title of the session.
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    /// <summary>
    /// The version of the session.
    /// </summary>
    [JsonPropertyName("version")]
    public required string Version { get; init; }

    /// <summary>
    /// Timestamps associated with the session.
    /// </summary>
    [JsonPropertyName("time")]
    public required SessionTime Time { get; init; }

    /// <summary>
    /// Revert information if the session has been reverted.
    /// </summary>
    [JsonPropertyName("revert")]
    public SessionRevert? Revert { get; init; }
}

/// <summary>
/// Summary statistics for a session.
/// </summary>
public record SessionSummary
{
    /// <summary>
    /// Number of additions in the session.
    /// </summary>
    [JsonPropertyName("additions")]
    public required int Additions { get; init; }

    /// <summary>
    /// Number of deletions in the session.
    /// </summary>
    [JsonPropertyName("deletions")]
    public required int Deletions { get; init; }

    /// <summary>
    /// Number of files changed in the session.
    /// </summary>
    [JsonPropertyName("files")]
    public required int Files { get; init; }

    /// <summary>
    /// File diffs for the session.
    /// </summary>
    [JsonPropertyName("diffs")]
    public List<FileDiff>? Diffs { get; init; }
}

/// <summary>
/// Share information for a session.
/// </summary>
public record SessionShare
{
    /// <summary>
    /// The public URL for the shared session.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; init; }
}

/// <summary>
/// Timestamps for a session.
/// </summary>
public record SessionTime
{
    /// <summary>
    /// When the session was created (Unix timestamp in milliseconds).
    /// </summary>
    [JsonPropertyName("created")]
    public required long Created { get; init; }

    /// <summary>
    /// When the session was last updated (Unix timestamp in milliseconds).
    /// </summary>
    [JsonPropertyName("updated")]
    public required long Updated { get; init; }

    /// <summary>
    /// When the session started compacting (Unix timestamp in milliseconds).
    /// </summary>
    [JsonPropertyName("compacting")]
    public long? Compacting { get; init; }
}

/// <summary>
/// Revert information for a session.
/// </summary>
public record SessionRevert
{
    /// <summary>
    /// The message ID that was reverted to.
    /// </summary>
    [JsonPropertyName("messageID")]
    public required string MessageId { get; init; }

    /// <summary>
    /// The part ID that was reverted.
    /// </summary>
    [JsonPropertyName("partID")]
    public string? PartId { get; init; }

    /// <summary>
    /// The snapshot hash.
    /// </summary>
    [JsonPropertyName("snapshot")]
    public string? Snapshot { get; init; }

    /// <summary>
    /// The diff hash.
    /// </summary>
    [JsonPropertyName("diff")]
    public string? Diff { get; init; }
}

/// <summary>
/// The status of a session.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SessionStatusIdle), "idle")]
[JsonDerivedType(typeof(SessionStatusRetry), "retry")]
[JsonDerivedType(typeof(SessionStatusBusy), "busy")]
public abstract record SessionStatus
{
    /// <summary>
    /// The type of status (populated from JSON discriminator).
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }
}

/// <summary>
/// Session is idle.
/// </summary>
public record SessionStatusIdle : SessionStatus
{
}

/// <summary>
/// Session is retrying after an error.
/// </summary>
public record SessionStatusRetry : SessionStatus
{
    /// <summary>
    /// The current retry attempt number.
    /// </summary>
    [JsonPropertyName("attempt")]
    public int Attempt { get; init; }

    /// <summary>
    /// The error message.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    /// <summary>
    /// The timestamp for the next retry (Unix timestamp in milliseconds).
    /// </summary>
    [JsonPropertyName("next")]
    public long? Next { get; init; }
}

/// <summary>
/// Session is busy processing.
/// </summary>
public record SessionStatusBusy : SessionStatus
{
}

/// <summary>
/// Request to create a new session.
/// </summary>
public record CreateSessionRequest
{
    /// <summary>
    /// Optional parent session ID to fork from.
    /// </summary>
    [JsonPropertyName("parentID")]
    public string? ParentId { get; init; }

    /// <summary>
    /// Optional title for the session.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }
}

/// <summary>
/// Request to update a session.
/// </summary>
public record UpdateSessionRequest
{
    /// <summary>
    /// New title for the session.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }
}

/// <summary>
/// Request to fork an existing session.
/// </summary>
public record ForkSessionRequest
{
    /// <summary>
    /// The message ID to fork from (creates a new session with messages up to this point).
    /// </summary>
    [JsonPropertyName("messageID")]
    public string? MessageId { get; init; }
}

/// <summary>
/// Request to revert a session.
/// </summary>
public record RevertSessionRequest
{
    /// <summary>
    /// The message ID to revert to.
    /// </summary>
    [JsonPropertyName("messageID")]
    public required string MessageId { get; init; }

    /// <summary>
    /// Optional part ID to revert.
    /// </summary>
    [JsonPropertyName("partID")]
    public string? PartId { get; init; }
}

/// <summary>
/// Request to initialize a session.
/// The API expects flat properties, not nested model object.
/// </summary>
public record InitSessionRequest
{
    /// <summary>
    /// The provider ID to use for the session.
    /// </summary>
    [JsonPropertyName("providerID")]
    public string? ProviderId { get; init; }

    /// <summary>
    /// The model ID to use for the session.
    /// </summary>
    [JsonPropertyName("modelID")]
    public string? ModelId { get; init; }

    /// <summary>
    /// The message ID for the initialization (pattern: ^msg.*).
    /// If not provided, the API will generate one.
    /// </summary>
    [JsonPropertyName("messageID")]
    public string? MessageId { get; init; }

    /// <summary>
    /// Creates an InitSessionRequest from a ModelReference.
    /// </summary>
    public static InitSessionRequest FromModel(ModelReference model, string? messageId = null) => new()
    {
        ProviderId = model.ProviderId,
        ModelId = model.ModelId,
        MessageId = messageId ?? $"msg_{Guid.NewGuid():N}"
    };
}

/// <summary>
/// Request to execute a command in a session.
/// </summary>
public record ExecuteCommandRequest
{
    /// <summary>
    /// The command name to execute.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}

/// <summary>
/// Request to execute a shell command in a session.
/// </summary>
public record ExecuteShellRequest
{
    /// <summary>
    /// The shell command to execute.
    /// </summary>
    [JsonPropertyName("command")]
    public required string Command { get; init; }
}

/// <summary>
/// Request to summarize a session.
/// </summary>
public record SummarizeSessionRequest
{
    /// <summary>
    /// Optional message ID to summarize up to.
    /// </summary>
    [JsonPropertyName("messageID")]
    public string? MessageId { get; init; }
}
