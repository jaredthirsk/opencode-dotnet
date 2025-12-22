using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace LionFire.OpenCode.Serve;

/// <summary>
/// Configuration options for <see cref="OpenCodeClient"/>.
/// </summary>
public class OpenCodeClientOptions
{
    /// <summary>
    /// Default base URL for the OpenCode server.
    /// </summary>
    public const string DefaultBaseUrl = "http://localhost:9123";

    /// <summary>
    /// Default timeout for quick operations (list, get, delete).
    /// </summary>
    public static readonly TimeSpan DefaultQuickTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Default timeout for message operations (AI responses).
    /// </summary>
    public static readonly TimeSpan DefaultMessageTimeout = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Default maximum number of retry attempts for transient failures.
    /// </summary>
    public const int DefaultMaxRetryAttempts = 3;

    /// <summary>
    /// Default delay between retry attempts in seconds.
    /// </summary>
    public const int DefaultRetryDelaySeconds = 2;

    /// <summary>
    /// Default number of failures before the circuit breaker opens.
    /// </summary>
    public const int DefaultCircuitBreakerThreshold = 5;

    /// <summary>
    /// Default duration the circuit breaker stays open in seconds.
    /// </summary>
    public const int DefaultCircuitBreakerDurationSeconds = 30;

    /// <summary>
    /// Default per-operation timeout in seconds.
    /// </summary>
    public const int DefaultOperationTimeoutSeconds = 30;

    /// <summary>
    /// Default maximum jitter to add to retry delays in milliseconds.
    /// </summary>
    public const int DefaultMaxJitterMilliseconds = 1000;

    /// <summary>
    /// Gets or sets the base URL of the OpenCode server.
    /// Defaults to <c>http://localhost:9123</c>.
    /// </summary>
    public string BaseUrl { get; set; } = DefaultBaseUrl;

    /// <summary>
    /// Gets or sets the optional working directory for the OpenCode server.
    /// When set, this directory is used as the context for file operations.
    /// </summary>
    public string? Directory { get; set; }

    /// <summary>
    /// Gets or sets the default timeout for quick operations (list, get, delete).
    /// Defaults to 30 seconds.
    /// </summary>
    public TimeSpan DefaultTimeout { get; set; } = DefaultQuickTimeout;

    /// <summary>
    /// Gets or sets the timeout for message operations that involve AI responses.
    /// Defaults to 5 minutes.
    /// </summary>
    public TimeSpan MessageTimeout { get; set; } = DefaultMessageTimeout;

    /// <summary>
    /// Gets or sets whether to enable automatic retry for transient failures.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool EnableRetry { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for transient failures.
    /// Defaults to 3.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = DefaultMaxRetryAttempts;

    /// <summary>
    /// Gets or sets the delay between retry attempts in seconds.
    /// Uses exponential backoff starting from this value.
    /// Defaults to 2 seconds.
    /// </summary>
    public int RetryDelaySeconds { get; set; } = DefaultRetryDelaySeconds;

    /// <summary>
    /// Gets or sets whether to enable OpenTelemetry tracing.
    /// Defaults to true (tracing is enabled if there are listeners).
    /// </summary>
    public bool EnableTelemetry { get; set; } = true;

    #region Resilience Options

    /// <summary>
    /// Gets or sets whether to enable the circuit breaker pattern.
    /// When enabled, the circuit will open after repeated failures to prevent cascading failures.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool EnableCircuitBreaker { get; set; } = true;

    /// <summary>
    /// Gets or sets the number of consecutive failures before the circuit breaker opens.
    /// Defaults to 5.
    /// </summary>
    public int CircuitBreakerThreshold { get; set; } = DefaultCircuitBreakerThreshold;

    /// <summary>
    /// Gets or sets the duration the circuit breaker stays open before allowing a test request (half-open state).
    /// Defaults to 30 seconds.
    /// </summary>
    public TimeSpan CircuitBreakerDuration { get; set; } = TimeSpan.FromSeconds(DefaultCircuitBreakerDurationSeconds);

    /// <summary>
    /// Gets or sets whether to enable per-operation timeout (distinct from HttpClient.Timeout).
    /// This timeout is applied per individual HTTP request through a Polly timeout policy.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool EnableOperationTimeout { get; set; } = true;

    /// <summary>
    /// Gets or sets the per-operation timeout.
    /// This is applied to each individual HTTP request, independent of HttpClient.Timeout.
    /// Defaults to 30 seconds.
    /// </summary>
    public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromSeconds(DefaultOperationTimeoutSeconds);

    /// <summary>
    /// Gets or sets the maximum random jitter to add to retry delays to prevent thundering herd.
    /// Set to <see cref="TimeSpan.Zero"/> to disable jitter.
    /// Defaults to 1000 milliseconds.
    /// </summary>
    public TimeSpan MaxRetryJitter { get; set; } = TimeSpan.FromMilliseconds(DefaultMaxJitterMilliseconds);

    #endregion

    /// <summary>
    /// Gets or sets whether to validate options on startup.
    /// Defaults to true.
    /// </summary>
    public bool ValidateOnStart { get; set; } = true;

    /// <summary>
    /// Validates that the <see cref="BaseUrl"/> is a valid URI.
    /// </summary>
    /// <returns><c>true</c> if the base URL is valid; otherwise, <c>false</c>.</returns>
    public bool ValidateBaseUrl()
    {
        return Uri.TryCreate(BaseUrl, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}

/// <summary>
/// Validates <see cref="OpenCodeClientOptions"/> on startup.
/// </summary>
public class OpenCodeClientOptionsValidator : IValidateOptions<OpenCodeClientOptions>
{
    /// <summary>
    /// Maximum reasonable timeout value (24 hours) to catch configuration errors.
    /// </summary>
    public static readonly TimeSpan MaxReasonableTimeout = TimeSpan.FromHours(24);

    /// <summary>
    /// Maximum reasonable retry attempts to prevent infinite retry loops.
    /// </summary>
    public const int MaxReasonableRetryAttempts = 10;

    /// <summary>
    /// Maximum reasonable retry delay in seconds.
    /// </summary>
    public const int MaxReasonableRetryDelaySeconds = 300; // 5 minutes

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, OpenCodeClientOptions options)
    {
        var failures = new List<string>();

        // BaseUrl validation
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            failures.Add("BaseUrl cannot be null or empty. " +
                "Ensure you have set the BaseUrl property or the 'OpenCode:BaseUrl' configuration.");
        }
        else if (!options.ValidateBaseUrl())
        {
            failures.Add($"BaseUrl '{options.BaseUrl}' is not a valid HTTP or HTTPS URI. " +
                "Expected format: http://localhost:9123 or https://your-server.com:9123");
        }
        else if (Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri))
        {
            // Additional URL checks
            if (uri.PathAndQuery != "/" && !string.IsNullOrEmpty(uri.PathAndQuery))
            {
                failures.Add($"BaseUrl '{options.BaseUrl}' should not contain a path. " +
                    "Use just the scheme, host, and port (e.g., http://localhost:9123).");
            }

            // Check for common port mistakes
            if (uri.Port == 80 && uri.Scheme == Uri.UriSchemeHttp)
            {
                // Port 80 is fine for HTTP, but warn about common misconfigurations
            }

            if (uri.Port == 443 && uri.Scheme == Uri.UriSchemeHttp)
            {
                failures.Add($"BaseUrl '{options.BaseUrl}' uses HTTP scheme with port 443. " +
                    "Did you mean to use HTTPS? Change to https:// or use a different port.");
            }

            if (uri.Port == 9123 && uri.Scheme == Uri.UriSchemeHttps)
            {
                // This is fine - just the default port with HTTPS
            }
        }

        // Timeout validations
        if (options.DefaultTimeout <= TimeSpan.Zero)
        {
            failures.Add("DefaultTimeout must be greater than zero.");
        }
        else if (options.DefaultTimeout > MaxReasonableTimeout)
        {
            failures.Add($"DefaultTimeout of {options.DefaultTimeout.TotalHours:F1} hours exceeds maximum reasonable value of 24 hours. " +
                "This may indicate a configuration error.");
        }

        if (options.MessageTimeout <= TimeSpan.Zero)
        {
            failures.Add("MessageTimeout must be greater than zero.");
        }
        else if (options.MessageTimeout > MaxReasonableTimeout)
        {
            failures.Add($"MessageTimeout of {options.MessageTimeout.TotalHours:F1} hours exceeds maximum reasonable value of 24 hours. " +
                "This may indicate a configuration error.");
        }

        if (options.OperationTimeout <= TimeSpan.Zero)
        {
            failures.Add("OperationTimeout must be greater than zero.");
        }
        else if (options.OperationTimeout > MaxReasonableTimeout)
        {
            failures.Add($"OperationTimeout of {options.OperationTimeout.TotalHours:F1} hours exceeds maximum reasonable value of 24 hours. " +
                "This may indicate a configuration error.");
        }

        // Check timeout relationships
        if (options.DefaultTimeout > TimeSpan.Zero && options.OperationTimeout > TimeSpan.Zero &&
            options.OperationTimeout > options.DefaultTimeout)
        {
            // This is a warning case - operation timeout exceeding default timeout is unusual
            // but not necessarily an error
        }

        // Retry validations
        if (options.MaxRetryAttempts < 0)
        {
            failures.Add("MaxRetryAttempts cannot be negative. Use 0 to disable retries.");
        }
        else if (options.MaxRetryAttempts > MaxReasonableRetryAttempts)
        {
            failures.Add($"MaxRetryAttempts of {options.MaxRetryAttempts} exceeds maximum reasonable value of {MaxReasonableRetryAttempts}. " +
                "High retry counts may cause excessive delays.");
        }

        if (options.RetryDelaySeconds < 0)
        {
            failures.Add("RetryDelaySeconds cannot be negative. Use 0 for no delay between retries (not recommended).");
        }
        else if (options.RetryDelaySeconds > MaxReasonableRetryDelaySeconds)
        {
            failures.Add($"RetryDelaySeconds of {options.RetryDelaySeconds} exceeds maximum reasonable value of {MaxReasonableRetryDelaySeconds}. " +
                "This may cause very long delays between retry attempts.");
        }

        // Circuit breaker validations
        if (options.CircuitBreakerThreshold < 1)
        {
            failures.Add("CircuitBreakerThreshold must be at least 1. " +
                "This is the number of consecutive failures before the circuit opens.");
        }
        else if (options.CircuitBreakerThreshold > 100)
        {
            failures.Add($"CircuitBreakerThreshold of {options.CircuitBreakerThreshold} exceeds maximum reasonable value of 100. " +
                "High thresholds may defeat the purpose of the circuit breaker.");
        }

        if (options.CircuitBreakerDuration <= TimeSpan.Zero)
        {
            failures.Add("CircuitBreakerDuration must be greater than zero. " +
                "This is how long the circuit stays open before allowing a test request.");
        }
        else if (options.CircuitBreakerDuration > TimeSpan.FromMinutes(30))
        {
            failures.Add($"CircuitBreakerDuration of {options.CircuitBreakerDuration.TotalMinutes:F1} minutes exceeds maximum reasonable value of 30 minutes. " +
                "Very long durations may cause extended outages.");
        }

        // Jitter validation
        if (options.MaxRetryJitter < TimeSpan.Zero)
        {
            failures.Add("MaxRetryJitter cannot be negative. Use TimeSpan.Zero to disable jitter.");
        }
        else if (options.MaxRetryJitter > TimeSpan.FromSeconds(30))
        {
            failures.Add($"MaxRetryJitter of {options.MaxRetryJitter.TotalSeconds:F1} seconds exceeds maximum reasonable value of 30 seconds. " +
                "Excessive jitter may cause unpredictable retry behavior.");
        }

        // Directory validation
        if (!string.IsNullOrEmpty(options.Directory))
        {
            // Check for potentially problematic directory values
            if (options.Directory.Contains('\0'))
            {
                failures.Add("Directory contains null characters which are invalid in file paths.");
            }

            // Check for obviously wrong values (URLs passed as directory)
            if (options.Directory.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                options.Directory.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                failures.Add($"Directory '{options.Directory}' appears to be a URL. " +
                    "This should be a local file system path.");
            }
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
