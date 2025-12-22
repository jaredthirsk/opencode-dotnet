namespace LionFire.OpenCode.Serve.Models;

/// <summary>
/// Represents progress information during a streaming operation.
/// </summary>
/// <remarks>
/// This class is designed for use with <see cref="IProgress{T}"/> to provide
/// real-time updates during streaming operations. It is particularly useful
/// for updating UI elements or logging progress in long-running operations.
/// </remarks>
public sealed class StreamingProgress
{
    /// <summary>
    /// Gets or sets the current status of the streaming operation.
    /// </summary>
    public StreamingStatus Status { get; set; } = StreamingStatus.Starting;

    /// <summary>
    /// Gets or sets the total number of chunks received so far.
    /// </summary>
    public int ChunkCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of bytes received so far.
    /// </summary>
    public long BytesReceived { get; set; }

    /// <summary>
    /// Gets or sets the total number of characters in text content received so far.
    /// </summary>
    public int CharacterCount { get; set; }

    /// <summary>
    /// Gets or sets the elapsed time since the streaming operation started.
    /// </summary>
    public TimeSpan Elapsed { get; set; }

    /// <summary>
    /// Gets or sets the most recent chunk of text content (if available).
    /// </summary>
    /// <remarks>
    /// This is set for each progress update and contains only the latest chunk,
    /// not the cumulative content.
    /// </remarks>
    public string? LatestChunk { get; set; }

    /// <summary>
    /// Gets or sets an optional message providing additional context about the progress.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the event type from the server-sent event (if applicable).
    /// </summary>
    public string? EventType { get; set; }

    /// <summary>
    /// Gets or sets any error that occurred during streaming.
    /// </summary>
    /// <remarks>
    /// This is only set when <see cref="Status"/> is <see cref="StreamingStatus.Error"/>.
    /// </remarks>
    public Exception? Error { get; set; }

    /// <summary>
    /// Creates a new progress instance indicating the operation is starting.
    /// </summary>
    public static StreamingProgress Starting() => new()
    {
        Status = StreamingStatus.Starting,
        Message = "Initiating streaming connection..."
    };

    /// <summary>
    /// Creates a new progress instance indicating the connection is established.
    /// </summary>
    public static StreamingProgress Connected() => new()
    {
        Status = StreamingStatus.Connected,
        Message = "Connection established, waiting for data..."
    };

    /// <summary>
    /// Creates a new progress instance for receiving a chunk.
    /// </summary>
    /// <param name="chunkCount">The current chunk count.</param>
    /// <param name="bytesReceived">Total bytes received.</param>
    /// <param name="characterCount">Total characters received.</param>
    /// <param name="elapsed">Time elapsed since start.</param>
    /// <param name="latestChunk">The content of the latest chunk.</param>
    /// <param name="eventType">The event type from the server.</param>
    public static StreamingProgress ChunkReceived(
        int chunkCount,
        long bytesReceived,
        int characterCount,
        TimeSpan elapsed,
        string? latestChunk = null,
        string? eventType = null) => new()
    {
        Status = StreamingStatus.Receiving,
        ChunkCount = chunkCount,
        BytesReceived = bytesReceived,
        CharacterCount = characterCount,
        Elapsed = elapsed,
        LatestChunk = latestChunk,
        EventType = eventType
    };

    /// <summary>
    /// Creates a new progress instance indicating the stream has completed.
    /// </summary>
    /// <param name="chunkCount">The final chunk count.</param>
    /// <param name="bytesReceived">Total bytes received.</param>
    /// <param name="characterCount">Total characters received.</param>
    /// <param name="elapsed">Total time elapsed.</param>
    public static StreamingProgress Completed(
        int chunkCount,
        long bytesReceived,
        int characterCount,
        TimeSpan elapsed) => new()
    {
        Status = StreamingStatus.Completed,
        ChunkCount = chunkCount,
        BytesReceived = bytesReceived,
        CharacterCount = characterCount,
        Elapsed = elapsed,
        Message = "Streaming completed successfully"
    };

    /// <summary>
    /// Creates a new progress instance indicating an error occurred.
    /// </summary>
    /// <param name="error">The exception that occurred.</param>
    /// <param name="chunkCount">The chunk count at the time of error.</param>
    /// <param name="elapsed">Time elapsed before the error.</param>
    public static StreamingProgress Failed(
        Exception error,
        int chunkCount,
        TimeSpan elapsed) => new()
    {
        Status = StreamingStatus.Error,
        ChunkCount = chunkCount,
        Elapsed = elapsed,
        Error = error,
        Message = $"Error: {error.Message}"
    };

    /// <summary>
    /// Creates a new progress instance indicating the operation was cancelled.
    /// </summary>
    /// <param name="chunkCount">The chunk count at cancellation.</param>
    /// <param name="elapsed">Time elapsed before cancellation.</param>
    public static StreamingProgress Cancelled(
        int chunkCount,
        TimeSpan elapsed) => new()
    {
        Status = StreamingStatus.Cancelled,
        ChunkCount = chunkCount,
        Elapsed = elapsed,
        Message = "Operation cancelled"
    };
}

/// <summary>
/// Represents the status of a streaming operation.
/// </summary>
public enum StreamingStatus
{
    /// <summary>
    /// The streaming operation is starting.
    /// </summary>
    Starting,

    /// <summary>
    /// The connection to the server has been established.
    /// </summary>
    Connected,

    /// <summary>
    /// The streaming operation is actively receiving data.
    /// </summary>
    Receiving,

    /// <summary>
    /// The streaming operation has completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// An error occurred during the streaming operation.
    /// </summary>
    Error,

    /// <summary>
    /// The streaming operation was cancelled.
    /// </summary>
    Cancelled
}
