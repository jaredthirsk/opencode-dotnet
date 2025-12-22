# Error Handling and Resilience Example

This example demonstrates proper exception handling patterns and Polly resilience integration when using the LionFire.OpenCode.Serve SDK.

## Features Demonstrated

- Exception hierarchy understanding
- Connection error handling
- Not found error handling
- **Polly resilience policies** (retry, circuit breaker, timeout)
- Retry patterns with exponential backoff and jitter
- Graceful degradation
- Comprehensive error handling

## Prerequisites

1. .NET 8.0 SDK or later
2. OpenCode server (optional - example handles missing server)

## Running the Example

```bash
dotnet run --project examples/ErrorHandling
```

## Polly Resilience Integration

The SDK includes built-in Polly integration for advanced resilience patterns. Configure via `OpenCodeClientOptions` and enable with extension methods.

### Quick Start

```csharp
services.AddOpenCodeClient(options =>
{
    // Retry with jitter
    options.EnableRetry = true;
    options.MaxRetryAttempts = 3;
    options.RetryDelaySeconds = 2;
    options.MaxRetryJitter = TimeSpan.FromMilliseconds(1000);

    // Circuit breaker
    options.EnableCircuitBreaker = true;
    options.CircuitBreakerThreshold = 5;
    options.CircuitBreakerDuration = TimeSpan.FromSeconds(30);

    // Per-operation timeout
    options.EnableOperationTimeout = true;
    options.OperationTimeout = TimeSpan.FromSeconds(30);
})
.AddOpenCodeResiliencePolicies(); // Enable all Polly policies
```

### Available Extension Methods

| Method | Description |
|--------|-------------|
| `AddOpenCodeResiliencePolicies()` | Adds all policies (retry + circuit breaker + timeout) |
| `AddOpenCodeRetryPolicy()` | Retry with exponential backoff and jitter |
| `AddOpenCodeCircuitBreakerPolicy()` | Circuit breaker for repeated failures |
| `AddOpenCodeTimeoutPolicy()` | Per-operation timeout |

### Resilience Options Reference

#### Retry Options

| Option | Default | Description |
|--------|---------|-------------|
| `EnableRetry` | `true` | Enable/disable retry policy |
| `MaxRetryAttempts` | `3` | Maximum retry attempts |
| `RetryDelaySeconds` | `2` | Base delay between retries (uses exponential backoff) |
| `MaxRetryJitter` | `1000ms` | Maximum random jitter added to delays |

#### Circuit Breaker Options

| Option | Default | Description |
|--------|---------|-------------|
| `EnableCircuitBreaker` | `true` | Enable/disable circuit breaker |
| `CircuitBreakerThreshold` | `5` | Consecutive failures before circuit opens |
| `CircuitBreakerDuration` | `30s` | How long circuit stays open before testing |

#### Timeout Options

| Option | Default | Description |
|--------|---------|-------------|
| `EnableOperationTimeout` | `true` | Enable/disable per-operation timeout |
| `OperationTimeout` | `30s` | Timeout for each individual HTTP request |

## Circuit Breaker Pattern

The circuit breaker prevents cascading failures by "breaking" after repeated failures:

```
CLOSED (normal) -> OPEN (failing fast) -> HALF-OPEN (testing) -> CLOSED
```

**States:**
- **Closed**: Normal operation, requests are allowed
- **Open**: Circuit is broken, requests fail immediately with `BrokenCircuitException`
- **Half-Open**: Testing if service recovered with a single request

```csharp
try
{
    await client.ListSessionsAsync();
}
catch (BrokenCircuitException)
{
    // Circuit is open - service is down
    // Use cached data or show degraded experience
    return GetCachedData();
}
```

## Retry with Jitter

Jitter (random delay) prevents the "thundering herd" problem where many clients retry simultaneously:

```
Retry 1: 2s + random(0-1000ms)
Retry 2: 4s + random(0-1000ms)
Retry 3: 8s + random(0-1000ms)
```

The Polly policy automatically handles:
- HTTP 5xx server errors
- HTTP 408 Request Timeout
- HTTP 429 Too Many Requests
- Network failures
- Polly timeout rejections

## Exception Hierarchy

```
OpenCodeException (base)
+-- OpenCodeApiException (HTTP errors)
|   +-- OpenCodeNotFoundException (404)
|   +-- OpenCodeRateLimitException (429)
+-- OpenCodeConnectionException (network errors)
+-- OpenCodeTimeoutException (operation timeout)
+-- OpenCodeSerializationException (JSON errors)

Polly Exceptions:
+-- BrokenCircuitException (circuit breaker open)
+-- TimeoutRejectedException (Polly timeout)
```

## Best Practices

### 1. Use DI with Polly Policies

```csharp
services.AddOpenCodeClient(options =>
{
    options.CircuitBreakerThreshold = 10;
    options.MaxRetryAttempts = 5;
})
.AddOpenCodeResiliencePolicies();
```

### 2. Handle Circuit Breaker Exceptions

```csharp
try
{
    await client.CreateSessionAsync();
}
catch (BrokenCircuitException)
{
    // Fail fast - don't retry
    return ShowOfflineMode();
}
catch (OpenCodeConnectionException)
{
    // May want to retry manually
    return QueueForLater();
}
```

### 3. Configure Timeouts Appropriately

```csharp
// Short timeout for quick operations
options.OperationTimeout = TimeSpan.FromSeconds(10);

// Longer timeout for AI responses (override per-operation)
options.MessageTimeout = TimeSpan.FromMinutes(5);
```

### 4. Tune for Your Environment

```csharp
// Production: More conservative settings
options.CircuitBreakerThreshold = 10;
options.CircuitBreakerDuration = TimeSpan.FromMinutes(1);
options.MaxRetryAttempts = 5;

// Development: Faster feedback
options.CircuitBreakerThreshold = 3;
options.CircuitBreakerDuration = TimeSpan.FromSeconds(10);
options.MaxRetryAttempts = 2;
```

## Expected Output

```
LionFire.OpenCode.Serve - Error Handling & Resilience Example
==============================================================

Example 1: Connection Error (wrong URL)
----------------------------------------
  Exception Type: OpenCodeConnectionException
  Message: Failed to connect to OpenCode server at http://localhost:9999
  Server URL: http://localhost:9999
  Troubleshooting Hint: Ensure OpenCode server is running. Start it with: opencode serve

Example 2: Not Found Error (invalid session ID)
------------------------------------------------
  (Skipped - OpenCode server not running)

Example 3: Polly Resilience Policies with DI
---------------------------------------------
  Setting up DI with Polly resilience policies...
  Configuration:
    - Retry: 3 attempts with exponential backoff + jitter
    - Circuit Breaker: Opens after 5 failures, stays open 30s
    - Timeout: 10s per operation

  Making request with resilience policies...
  Connection failed - but retries were attempted automatically

Example 4: Manual Retry Pattern (without Polly)
------------------------------------------------
  Attempt 1...
  Failed, waiting 2.5s before retry (with jitter)...
  Attempt 2...
  Failed, waiting 4.8s before retry (with jitter)...
  Attempt 3...
  (Failed after retries - OpenCode server not running)

Example 5: Graceful Degradation
--------------------------------
  Response: [Fallback] OpenCode unavailable - using cached response: 4

Example 6: Circuit Breaker Pattern
-----------------------------------
  Circuit Breaker Pattern prevents cascading failures:

  States:
    CLOSED   -> Normal operation, requests allowed
    OPEN     -> After threshold failures, requests fail fast
    HALF-OPEN -> Testing if service recovered

  Simulating failures to trigger circuit breaker...
    Request 1: Connection failed (circuit still closed)
    Request 2: Connection failed (circuit still closed)
    Request 3: Connection failed (circuit still closed)
    Request 4: CIRCUIT OPEN - Failing fast (no network call)
    Request 5: CIRCUIT OPEN - Failing fast (no network call)

  After circuit opens, requests fail immediately without network calls.
  This prevents overwhelming a struggling service.

Example 7: Comprehensive Error Handler
---------------------------------------
  [Connection Error] Failed to connect to OpenCode server
  Hint: Ensure OpenCode server is running. Start it with: opencode serve

Done!
```

## Next Steps

- [Performance Tuning](../../docs/performance-tuning.md) - Configure timeouts and retries
- [Enterprise Deployment](../../docs/enterprise-deployment.md) - Production error handling
- [API Reference](../../docs/api-reference.md) - Full API documentation
