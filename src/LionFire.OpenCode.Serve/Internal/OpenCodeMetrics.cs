using System.Diagnostics.Metrics;

namespace LionFire.OpenCode.Serve.Internal;

/// <summary>
/// Provides metrics instrumentation for OpenCode client operations using System.Diagnostics.Metrics.
/// </summary>
/// <remarks>
/// <para>
/// This class exposes metrics that can be consumed by OpenTelemetry, Prometheus, or any other
/// metrics collection system that supports .NET's Meter API.
/// </para>
/// <para>
/// To enable metrics collection with OpenTelemetry, add the meter name to your MeterProvider:
/// <code>
/// builder.Services.AddOpenTelemetry()
///     .WithMetrics(metrics => metrics.AddMeter(OpenCodeMetrics.MeterName));
/// </code>
/// </para>
/// </remarks>
public sealed class OpenCodeMetrics : IDisposable
{
    /// <summary>
    /// The name of the meter for OpenCode metrics.
    /// </summary>
    public const string MeterName = "LionFire.OpenCode.Serve";

    /// <summary>
    /// The version of the meter.
    /// </summary>
    public const string MeterVersion = "0.1.0";

    private readonly Meter _meter;
    private readonly Counter<long> _requestCounter;
    private readonly Counter<long> _errorCounter;
    private readonly Histogram<double> _requestDuration;
    private readonly Counter<long> _streamingChunkCounter;
    private readonly Histogram<long> _streamingChunkSize;
    private readonly UpDownCounter<int> _activeRequests;

    /// <summary>
    /// Gets the singleton instance of <see cref="OpenCodeMetrics"/>.
    /// </summary>
    public static OpenCodeMetrics Instance { get; } = new();

    /// <summary>
    /// Initializes a new instance of <see cref="OpenCodeMetrics"/>.
    /// </summary>
    public OpenCodeMetrics()
    {
        _meter = new Meter(MeterName, MeterVersion);

        _requestCounter = _meter.CreateCounter<long>(
            "opencode.client.requests",
            unit: "{request}",
            description: "Total number of HTTP requests made to the OpenCode server");

        _errorCounter = _meter.CreateCounter<long>(
            "opencode.client.errors",
            unit: "{error}",
            description: "Total number of errors encountered during HTTP requests");

        _requestDuration = _meter.CreateHistogram<double>(
            "opencode.client.request.duration",
            unit: "ms",
            description: "Duration of HTTP requests in milliseconds");

        _streamingChunkCounter = _meter.CreateCounter<long>(
            "opencode.client.streaming.chunks",
            unit: "{chunk}",
            description: "Total number of streaming chunks received");

        _streamingChunkSize = _meter.CreateHistogram<long>(
            "opencode.client.streaming.chunk.size",
            unit: "By",
            description: "Size of streaming chunks in bytes");

        _activeRequests = _meter.CreateUpDownCounter<int>(
            "opencode.client.requests.active",
            unit: "{request}",
            description: "Number of currently active HTTP requests");
    }

    /// <summary>
    /// Records a request being initiated.
    /// </summary>
    /// <param name="operationType">The type of operation (e.g., "session.create", "message.send").</param>
    /// <param name="httpMethod">The HTTP method used.</param>
    public void RecordRequestStarted(string operationType, string httpMethod)
    {
        _requestCounter.Add(1,
            new KeyValuePair<string, object?>("operation.type", operationType),
            new KeyValuePair<string, object?>("http.method", httpMethod));
        _activeRequests.Add(1,
            new KeyValuePair<string, object?>("operation.type", operationType));
    }

    /// <summary>
    /// Records a request completion (successful or failed).
    /// </summary>
    /// <param name="operationType">The type of operation.</param>
    /// <param name="httpMethod">The HTTP method used.</param>
    /// <param name="statusCode">The HTTP status code returned.</param>
    /// <param name="durationMs">The duration of the request in milliseconds.</param>
    /// <param name="success">Whether the request was successful.</param>
    public void RecordRequestCompleted(
        string operationType,
        string httpMethod,
        int statusCode,
        double durationMs,
        bool success)
    {
        _activeRequests.Add(-1,
            new KeyValuePair<string, object?>("operation.type", operationType));

        var tags = new[]
        {
            new KeyValuePair<string, object?>("operation.type", operationType),
            new KeyValuePair<string, object?>("http.method", httpMethod),
            new KeyValuePair<string, object?>("http.status_code", statusCode),
            new KeyValuePair<string, object?>("success", success)
        };

        _requestDuration.Record(durationMs, tags);
    }

    /// <summary>
    /// Records an error during a request.
    /// </summary>
    /// <param name="operationType">The type of operation.</param>
    /// <param name="errorType">The type of error (e.g., exception type name).</param>
    /// <param name="httpMethod">The HTTP method used.</param>
    public void RecordError(string operationType, string errorType, string httpMethod)
    {
        _errorCounter.Add(1,
            new KeyValuePair<string, object?>("operation.type", operationType),
            new KeyValuePair<string, object?>("error.type", errorType),
            new KeyValuePair<string, object?>("http.method", httpMethod));
    }

    /// <summary>
    /// Records a streaming chunk being received.
    /// </summary>
    /// <param name="operationType">The type of streaming operation.</param>
    /// <param name="sessionId">The session ID receiving the stream.</param>
    /// <param name="chunkSizeBytes">The size of the chunk in bytes.</param>
    public void RecordStreamingChunk(string operationType, string? sessionId, long chunkSizeBytes)
    {
        var tags = new[]
        {
            new KeyValuePair<string, object?>("operation.type", operationType),
            new KeyValuePair<string, object?>("session.id", sessionId ?? "unknown")
        };

        _streamingChunkCounter.Add(1, tags);
        _streamingChunkSize.Record(chunkSizeBytes, tags);
    }

    /// <summary>
    /// Releases the meter and all associated instruments.
    /// </summary>
    public void Dispose()
    {
        _meter.Dispose();
    }
}

/// <summary>
/// Tag names for metrics attributes following OpenTelemetry semantic conventions.
/// </summary>
public static class MetricTags
{
    /// <summary>HTTP request method (GET, POST, etc.).</summary>
    public const string HttpMethod = "http.method";

    /// <summary>HTTP response status code.</summary>
    public const string HttpStatusCode = "http.status_code";

    /// <summary>Type of operation being performed.</summary>
    public const string OperationType = "operation.type";

    /// <summary>Whether the operation was successful.</summary>
    public const string Success = "success";

    /// <summary>Type of error encountered.</summary>
    public const string ErrorType = "error.type";

    /// <summary>Session identifier.</summary>
    public const string SessionId = "session.id";
}
