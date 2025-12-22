using LionFire.OpenCode.Serve.Internal;

namespace LionFire.OpenCode.Serve.Extensions;

/// <summary>
/// Extension methods for configuring OpenTelemetry with OpenCode client.
/// </summary>
/// <remarks>
/// <para>
/// This class provides helper methods for integrating OpenCode telemetry with OpenTelemetry.
/// The OpenCode client uses the built-in .NET diagnostics APIs (System.Diagnostics.Activity
/// and System.Diagnostics.Metrics) which are compatible with OpenTelemetry.
/// </para>
/// <para>
/// Example usage with OpenTelemetry:
/// <code>
/// builder.Services.AddOpenTelemetry()
///     .WithTracing(tracing => tracing
///         .AddSource(OpenCodeActivitySource.Name)
///         .AddHttpClientInstrumentation()
///         .AddJaegerExporter())
///     .WithMetrics(metrics => metrics
///         .AddMeter(OpenCodeMetrics.MeterName)
///         .AddPrometheusExporter());
/// </code>
/// </para>
/// </remarks>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Gets the activity source name used by the OpenCode client.
    /// Use this when configuring OpenTelemetry tracing.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.Services.AddOpenTelemetry()
    ///     .WithTracing(tracing => tracing.AddSource(OpenTelemetryExtensions.ActivitySourceName));
    /// </code>
    /// </example>
    public static string ActivitySourceName => OpenCodeActivitySource.Name;

    /// <summary>
    /// Gets the meter name used by the OpenCode client.
    /// Use this when configuring OpenTelemetry metrics.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.Services.AddOpenTelemetry()
    ///     .WithMetrics(metrics => metrics.AddMeter(OpenTelemetryExtensions.MeterName));
    /// </code>
    /// </example>
    public static string MeterName => OpenCodeMetrics.MeterName;

    /// <summary>
    /// Gets the version of the telemetry instrumentation.
    /// </summary>
    public static string InstrumentationVersion => OpenCodeActivitySource.Version;
}

/// <summary>
/// Constants for OpenCode telemetry semantic conventions.
/// </summary>
public static class OpenCodeTelemetrySemantics
{
    /// <summary>
    /// The service name to use for OpenTelemetry resource configuration.
    /// </summary>
    public const string ServiceName = "opencode-client";

    /// <summary>
    /// Semantic attribute names following OpenTelemetry conventions.
    /// </summary>
    public static class Attributes
    {
        /// <summary>OpenCode session identifier.</summary>
        public const string SessionId = "opencode.session.id";

        /// <summary>OpenCode message identifier.</summary>
        public const string MessageId = "opencode.message.id";

        /// <summary>OpenCode tool identifier.</summary>
        public const string ToolId = "opencode.tool.id";

        /// <summary>File path being operated on.</summary>
        public const string FilePath = "opencode.file.path";

        /// <summary>Command being executed.</summary>
        public const string Command = "opencode.command";

        /// <summary>Number of streaming chunks received.</summary>
        public const string StreamingChunks = "opencode.streaming.chunks";

        /// <summary>AI model used for the request.</summary>
        public const string Model = "opencode.model";

        /// <summary>AI provider used for the request.</summary>
        public const string Provider = "opencode.provider";
    }

    /// <summary>
    /// Span names for different operations.
    /// </summary>
    public static class SpanNames
    {
        /// <summary>Session creation operation.</summary>
        public const string CreateSession = "opencode.session.create";

        /// <summary>Session retrieval operation.</summary>
        public const string GetSession = "opencode.session.get";

        /// <summary>Session listing operation.</summary>
        public const string ListSessions = "opencode.session.list";

        /// <summary>Session deletion operation.</summary>
        public const string DeleteSession = "opencode.session.delete";

        /// <summary>Session forking operation.</summary>
        public const string ForkSession = "opencode.session.fork";

        /// <summary>Send message (blocking) operation.</summary>
        public const string SendMessage = "opencode.message.send";

        /// <summary>Send message (streaming) operation.</summary>
        public const string SendMessageStreaming = "opencode.message.stream";

        /// <summary>File listing operation.</summary>
        public const string ListFiles = "opencode.file.list";

        /// <summary>File reading operation.</summary>
        public const string ReadFile = "opencode.file.read";
    }

    /// <summary>
    /// Metric names for different measurements.
    /// </summary>
    public static class MetricNames
    {
        /// <summary>Counter for total HTTP requests.</summary>
        public const string RequestsTotal = "opencode.client.requests";

        /// <summary>Counter for total errors.</summary>
        public const string ErrorsTotal = "opencode.client.errors";

        /// <summary>Histogram for request duration.</summary>
        public const string RequestDuration = "opencode.client.request.duration";

        /// <summary>Counter for streaming chunks.</summary>
        public const string StreamingChunks = "opencode.client.streaming.chunks";

        /// <summary>Histogram for streaming chunk size.</summary>
        public const string StreamingChunkSize = "opencode.client.streaming.chunk.size";

        /// <summary>Gauge for active requests.</summary>
        public const string ActiveRequests = "opencode.client.requests.active";
    }
}
