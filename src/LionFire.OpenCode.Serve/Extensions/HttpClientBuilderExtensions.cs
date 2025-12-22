using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace LionFire.OpenCode.Serve.Extensions;

/// <summary>
/// Extension methods for configuring the OpenCode HttpClient.
/// </summary>
public static class HttpClientBuilderExtensions
{
    /// <summary>
    /// Configures retry behavior using built-in .NET retry mechanisms.
    /// For advanced scenarios, consider using Polly via AddPolicyHandler.
    /// </summary>
    /// <param name="builder">The HttpClient builder.</param>
    /// <param name="configureRetry">Optional delegate to configure retry options.</param>
    /// <returns>The builder for chaining.</returns>
    /// <remarks>
    /// For more advanced resilience patterns (circuit breaker, bulkhead, etc.),
    /// install Microsoft.Extensions.Http.Polly and use AddPolicyHandler.
    /// </remarks>
    /// <example>
    /// Using built-in retry:
    /// <code>
    /// services.AddOpenCodeClient()
    ///     .ConfigureRetry(retry =>
    ///     {
    ///         retry.MaxRetryAttempts = 5;
    ///         retry.RetryDelaySeconds = 3;
    ///     });
    /// </code>
    ///
    /// Using Polly (requires Microsoft.Extensions.Http.Polly):
    /// <code>
    /// services.AddOpenCodeClient()
    ///     .AddPolicyHandler(Policy.Handle&lt;HttpRequestException&gt;()
    ///         .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(Math.Pow(2, i))));
    /// </code>
    /// </example>
    public static IHttpClientBuilder ConfigureRetry(
        this IHttpClientBuilder builder,
        Action<RetryOptions>? configureRetry = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.Configure<RetryOptions>(configureRetry ?? (_ => { }));

        return builder;
    }

    /// <summary>
    /// Sets the default timeout for HTTP operations.
    /// </summary>
    /// <param name="builder">The HttpClient builder.</param>
    /// <param name="timeout">The timeout duration.</param>
    /// <returns>The builder for chaining.</returns>
    public static IHttpClientBuilder WithTimeout(
        this IHttpClientBuilder builder,
        TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureHttpClient(client => client.Timeout = timeout);

        return builder;
    }

    #region Polly Resilience Policies

    /// <summary>
    /// Adds all OpenCode resilience policies (retry with jitter, circuit breaker, and timeout) to the HttpClient.
    /// Policies are applied in order: timeout (innermost) -> retry -> circuit breaker (outermost).
    /// </summary>
    /// <param name="builder">The HttpClient builder.</param>
    /// <returns>The builder for chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method configures a resilient HttpClient with the following policies:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><b>Timeout</b>: Per-request timeout using <see cref="OpenCodeClientOptions.OperationTimeout"/></description></item>
    ///   <item><description><b>Retry</b>: Exponential backoff with jitter to prevent thundering herd</description></item>
    ///   <item><description><b>Circuit Breaker</b>: Opens after repeated failures to prevent cascading failures</description></item>
    /// </list>
    /// <para>
    /// Configure behavior through <see cref="OpenCodeClientOptions"/>:
    /// </para>
    /// <code>
    /// services.AddOpenCodeClient(options =>
    /// {
    ///     options.MaxRetryAttempts = 5;
    ///     options.RetryDelaySeconds = 1;
    ///     options.MaxRetryJitter = TimeSpan.FromMilliseconds(500);
    ///     options.CircuitBreakerThreshold = 10;
    ///     options.CircuitBreakerDuration = TimeSpan.FromMinutes(1);
    ///     options.OperationTimeout = TimeSpan.FromSeconds(60);
    /// })
    /// .AddOpenCodeResiliencePolicies();
    /// </code>
    /// </remarks>
    public static IHttpClientBuilder AddOpenCodeResiliencePolicies(this IHttpClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Policies are added in reverse order of execution.
        // Execution order: CircuitBreaker -> Retry -> Timeout -> HTTP request
        // So we add: Timeout first, Retry second, CircuitBreaker last
        return builder
            .AddOpenCodeTimeoutPolicy()
            .AddOpenCodeRetryPolicy()
            .AddOpenCodeCircuitBreakerPolicy();
    }

    /// <summary>
    /// Adds a retry policy with exponential backoff and jitter to the HttpClient.
    /// </summary>
    /// <param name="builder">The HttpClient builder.</param>
    /// <returns>The builder for chaining.</returns>
    /// <remarks>
    /// <para>
    /// The retry policy handles transient HTTP errors (5xx, 408, network failures) and
    /// rate limiting (429) responses. It uses exponential backoff with decorrelated jitter
    /// to prevent thundering herd problems.
    /// </para>
    /// <para>
    /// Configure through <see cref="OpenCodeClientOptions"/>:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><see cref="OpenCodeClientOptions.EnableRetry"/>: Enable/disable retry</description></item>
    ///   <item><description><see cref="OpenCodeClientOptions.MaxRetryAttempts"/>: Maximum retry attempts</description></item>
    ///   <item><description><see cref="OpenCodeClientOptions.RetryDelaySeconds"/>: Base delay between retries</description></item>
    ///   <item><description><see cref="OpenCodeClientOptions.MaxRetryJitter"/>: Maximum jitter added to delays</description></item>
    /// </list>
    /// </remarks>
    public static IHttpClientBuilder AddOpenCodeRetryPolicy(this IHttpClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddPolicyHandler((serviceProvider, _) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<OpenCodeClientOptions>>().Value;
            var logger = serviceProvider.GetService<ILogger<OpenCodeClient>>();

            if (!options.EnableRetry || options.MaxRetryAttempts <= 0)
            {
                return Policy.NoOpAsync<HttpResponseMessage>();
            }

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(
                    retryCount: options.MaxRetryAttempts,
                    sleepDurationProvider: (retryAttempt, _, _) =>
                    {
                        // Exponential backoff with decorrelated jitter
                        var baseDelay = TimeSpan.FromSeconds(options.RetryDelaySeconds * Math.Pow(2, retryAttempt - 1));
                        var jitter = options.MaxRetryJitter > TimeSpan.Zero
                            ? TimeSpan.FromMilliseconds(Random.Shared.NextDouble() * options.MaxRetryJitter.TotalMilliseconds)
                            : TimeSpan.Zero;
                        return baseDelay + jitter;
                    },
                    onRetryAsync: (outcome, timespan, retryAttempt, _) =>
                    {
                        logger?.LogWarning(
                            outcome.Exception,
                            "Retry {RetryAttempt}/{MaxRetries} for OpenCode request after {Delay}ms. Status: {StatusCode}",
                            retryAttempt,
                            options.MaxRetryAttempts,
                            timespan.TotalMilliseconds,
                            outcome.Result?.StatusCode);
                        return Task.CompletedTask;
                    });
        });
    }

    /// <summary>
    /// Adds a circuit breaker policy to the HttpClient.
    /// </summary>
    /// <param name="builder">The HttpClient builder.</param>
    /// <returns>The builder for chaining.</returns>
    /// <remarks>
    /// <para>
    /// The circuit breaker prevents cascading failures by "opening" after a threshold of
    /// consecutive failures. While open, requests fail immediately without being attempted.
    /// After the break duration, the circuit enters "half-open" state and allows a test request.
    /// </para>
    /// <para>
    /// Circuit states:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><b>Closed</b>: Normal operation, requests are allowed</description></item>
    ///   <item><description><b>Open</b>: Circuit is broken, requests fail immediately</description></item>
    ///   <item><description><b>Half-Open</b>: Testing if the service has recovered</description></item>
    /// </list>
    /// <para>
    /// Configure through <see cref="OpenCodeClientOptions"/>:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><see cref="OpenCodeClientOptions.EnableCircuitBreaker"/>: Enable/disable circuit breaker</description></item>
    ///   <item><description><see cref="OpenCodeClientOptions.CircuitBreakerThreshold"/>: Failures before circuit opens</description></item>
    ///   <item><description><see cref="OpenCodeClientOptions.CircuitBreakerDuration"/>: How long circuit stays open</description></item>
    /// </list>
    /// </remarks>
    public static IHttpClientBuilder AddOpenCodeCircuitBreakerPolicy(this IHttpClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddPolicyHandler((serviceProvider, _) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<OpenCodeClientOptions>>().Value;
            var logger = serviceProvider.GetService<ILogger<OpenCodeClient>>();

            if (!options.EnableCircuitBreaker)
            {
                return Policy.NoOpAsync<HttpResponseMessage>();
            }

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
                .Or<TimeoutRejectedException>()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: options.CircuitBreakerThreshold,
                    durationOfBreak: options.CircuitBreakerDuration,
                    onBreak: (outcome, breakDuration) =>
                    {
                        logger?.LogWarning(
                            outcome.Exception,
                            "Circuit breaker opened for {BreakDuration}s. Status: {StatusCode}",
                            breakDuration.TotalSeconds,
                            outcome.Result?.StatusCode);
                    },
                    onReset: () =>
                    {
                        logger?.LogInformation("Circuit breaker reset. Normal operation resumed.");
                    },
                    onHalfOpen: () =>
                    {
                        logger?.LogInformation("Circuit breaker half-open. Testing with next request.");
                    });
        });
    }

    /// <summary>
    /// Adds a per-operation timeout policy to the HttpClient.
    /// </summary>
    /// <param name="builder">The HttpClient builder.</param>
    /// <returns>The builder for chaining.</returns>
    /// <remarks>
    /// <para>
    /// This timeout is applied to each individual HTTP request, independent of the
    /// <see cref="HttpClient.Timeout"/> property. It is placed inside the retry policy,
    /// so each retry attempt gets its own timeout.
    /// </para>
    /// <para>
    /// Configure through <see cref="OpenCodeClientOptions"/>:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><see cref="OpenCodeClientOptions.EnableOperationTimeout"/>: Enable/disable timeout</description></item>
    ///   <item><description><see cref="OpenCodeClientOptions.OperationTimeout"/>: Timeout duration per request</description></item>
    /// </list>
    /// </remarks>
    public static IHttpClientBuilder AddOpenCodeTimeoutPolicy(this IHttpClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddPolicyHandler((serviceProvider, _) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<OpenCodeClientOptions>>().Value;
            var logger = serviceProvider.GetService<ILogger<OpenCodeClient>>();

            if (!options.EnableOperationTimeout)
            {
                return Policy.NoOpAsync<HttpResponseMessage>();
            }

            return Policy.TimeoutAsync<HttpResponseMessage>(
                timeout: options.OperationTimeout,
                timeoutStrategy: TimeoutStrategy.Optimistic,
                onTimeoutAsync: (_, timeout, _, _) =>
                {
                    logger?.LogWarning(
                        "Request timed out after {Timeout}s",
                        timeout.TotalSeconds);
                    return Task.CompletedTask;
                });
        });
    }

    #endregion
}

/// <summary>
/// Options for configuring retry behavior.
/// </summary>
public class RetryOptions
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// Defaults to 3.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the initial delay between retries in seconds.
    /// Defaults to 2 seconds.
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 2;

    /// <summary>
    /// Gets or sets whether to use exponential backoff.
    /// Defaults to true.
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum jitter to add to retry delays in milliseconds.
    /// Set to 0 to disable jitter.
    /// Defaults to 1000ms.
    /// </summary>
    public int MaxJitterMilliseconds { get; set; } = 1000;
}
