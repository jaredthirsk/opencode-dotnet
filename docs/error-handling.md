# Error Handling Guide

This guide covers exception handling patterns and best practices for the LionFire.OpenCode.Serve SDK.

## Exception Hierarchy

The SDK provides a structured exception hierarchy with helpful troubleshooting hints:

```
OpenCodeException (base exception)
├── OpenCodeApiException (HTTP errors with status code)
│   ├── OpenCodeNotFoundException (404 - resource not found)
│   └── OpenCodeRateLimitException (429 - too many requests)
├── OpenCodeConnectionException (server not reachable)
├── OpenCodeTimeoutException (operation timeout)
└── OpenCodeSerializationException (JSON errors)
```

## Exception Details

### OpenCodeException

Base exception for all SDK-specific errors.

```csharp
public class OpenCodeException : Exception
{
    // Human-readable troubleshooting suggestion
    public string? TroubleshootingHint { get; }
}
```

**Example:**
```csharp
try
{
    await client.CreateSessionAsync();
}
catch (OpenCodeException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    if (ex.TroubleshootingHint != null)
    {
        Console.WriteLine($"Suggestion: {ex.TroubleshootingHint}");
    }
}
```

### OpenCodeApiException

HTTP-level errors from the OpenCode server.

```csharp
public class OpenCodeApiException : OpenCodeException
{
    public HttpStatusCode StatusCode { get; }
}
```

**Common Status Codes:**

| Status Code | Meaning | Common Cause |
|-------------|---------|--------------|
| 400 | Bad Request | Invalid request parameters |
| 401 | Unauthorized | Missing/invalid authentication |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource doesn't exist |
| 429 | Too Many Requests | Rate limiting |
| 500 | Internal Server Error | Server-side issue |
| 503 | Service Unavailable | Server temporarily unavailable |

**Example:**
```csharp
try
{
    var session = await client.GetSessionAsync("invalid_id");
}
catch (OpenCodeApiException ex)
{
    Console.WriteLine($"HTTP {(int)ex.StatusCode}: {ex.Message}");

    switch (ex.StatusCode)
    {
        case HttpStatusCode.NotFound:
            Console.WriteLine("Session was deleted or never existed");
            break;
        case HttpStatusCode.BadRequest:
            Console.WriteLine("Check your request parameters");
            break;
        case HttpStatusCode.TooManyRequests:
            Console.WriteLine("Slow down - rate limited");
            break;
    }
}
```

### OpenCodeNotFoundException

Specialized exception for 404 errors.

```csharp
public class OpenCodeNotFoundException : OpenCodeApiException
{
    public string? ResourceType { get; }  // e.g., "Session", "Message"
    public string? ResourceId { get; }    // The ID that wasn't found
}
```

**Example:**
```csharp
try
{
    await client.GetSessionAsync("ses_nonexistent");
}
catch (OpenCodeNotFoundException ex)
{
    Console.WriteLine($"Session not found: {ex.Message}");
    // Hint: "The session may have been deleted or expired."
}
```

### OpenCodeConnectionException

Network-level errors when the server cannot be reached.

```csharp
public class OpenCodeConnectionException : OpenCodeException
{
    public string? ServerUrl { get; }
}
```

**Common Causes:**
- OpenCode server not running
- Wrong server URL
- Firewall blocking connection
- Network issues

**Example:**
```csharp
try
{
    var client = new OpenCodeClient("http://localhost:9999");
    await client.ListSessionsAsync();
}
catch (OpenCodeConnectionException ex)
{
    Console.WriteLine($"Cannot connect to {ex.ServerUrl}");
    Console.WriteLine($"Hint: {ex.TroubleshootingHint}");
    // Hint: "Ensure OpenCode server is running. Start it with: opencode serve"
}
```

### OpenCodeTimeoutException

Operation took too long to complete.

```csharp
public class OpenCodeTimeoutException : OpenCodeException
{
    public TimeSpan Timeout { get; }
    public string? OperationType { get; }
}
```

**Example:**
```csharp
try
{
    var response = await client.PromptAsync(sessionId, request);
}
catch (OpenCodeTimeoutException ex)
{
    Console.WriteLine($"Operation timed out after {ex.Timeout.TotalSeconds}s");
    Console.WriteLine($"Hint: {ex.TroubleshootingHint}");
    // Hint: "Consider increasing MessageTimeout or simplifying the request."
}
```

### OpenCodeSerializationException

JSON parsing or serialization errors.

```csharp
public class OpenCodeSerializationException : OpenCodeException
{
    public string? JsonContent { get; }  // Truncated for debugging
}
```

**Common Causes:**
- Server returned unexpected format
- API version mismatch
- Server returned HTML instead of JSON

**Example:**
```csharp
try
{
    var sessions = await client.ListSessionsAsync();
}
catch (OpenCodeSerializationException ex)
{
    Console.WriteLine($"Failed to parse response: {ex.Message}");
    Console.WriteLine($"Content preview: {ex.JsonContent}");
    // Hint: "Check if the server version is compatible with this SDK version."
}
```

## Recommended Error Handling Patterns

### Basic Pattern

```csharp
try
{
    var session = await client.CreateSessionAsync();
    // Use session...
}
catch (OpenCodeConnectionException ex)
{
    // Handle connection issues
    logger.LogError(ex, "Cannot connect to OpenCode server");
    throw; // or return error response
}
catch (OpenCodeApiException ex)
{
    // Handle API errors
    logger.LogError(ex, "API error: {StatusCode}", ex.StatusCode);
    throw;
}
catch (OpenCodeException ex)
{
    // Handle all other SDK errors
    logger.LogError(ex, "OpenCode error: {Message}", ex.Message);
    throw;
}
```

### With Retry Logic

```csharp
async Task<Session> CreateSessionWithRetryAsync(int maxAttempts = 3)
{
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            return await client.CreateSessionAsync();
        }
        catch (OpenCodeConnectionException) when (attempt < maxAttempts)
        {
            // Connection issue - wait and retry
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
        }
        catch (OpenCodeTimeoutException) when (attempt < maxAttempts)
        {
            // Timeout - maybe server is busy, retry
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
        catch (OpenCodeApiException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests && attempt < maxAttempts)
        {
            // Rate limited - back off significantly
            await Task.Delay(TimeSpan.FromSeconds(30));
        }
    }

    throw new InvalidOperationException("Failed after max retry attempts");
}
```

### Graceful Degradation

```csharp
async Task<string?> TryGetAIResponseAsync(string question)
{
    try
    {
        var session = await client.CreateSessionAsync();
        try
        {
            var response = await client.PromptAsync(session.Id, new SendMessageRequest
            {
                Parts = new List<PartInput> { PartInput.TextInput(question) }
            });
            return response.Parts?.FirstOrDefault(p => p.IsTextPart)?.Text;
        }
        finally
        {
            await client.DeleteSessionAsync(session.Id);
        }
    }
    catch (OpenCodeConnectionException)
    {
        // Fallback when OpenCode is unavailable
        logger.LogWarning("OpenCode unavailable, using fallback");
        return GetFallbackResponse(question);
    }
    catch (OpenCodeTimeoutException)
    {
        // Timeout - return null to indicate no response
        logger.LogWarning("OpenCode timed out");
        return null;
    }
}
```

### Error Logging with Context

```csharp
try
{
    await client.PromptAsync(sessionId, request);
}
catch (OpenCodeException ex)
{
    logger.LogError(ex,
        "OpenCode operation failed. " +
        "Type: {ExceptionType}, " +
        "SessionId: {SessionId}, " +
        "Hint: {Hint}",
        ex.GetType().Name,
        sessionId,
        ex.TroubleshootingHint);
    throw;
}
```

## ASP.NET Core Error Handling

### Exception Filter

```csharp
public class OpenCodeExceptionFilter : IExceptionFilter
{
    private readonly ILogger<OpenCodeExceptionFilter> _logger;

    public OpenCodeExceptionFilter(ILogger<OpenCodeExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is OpenCodeException ex)
        {
            _logger.LogError(ex, "OpenCode error: {Message}", ex.Message);

            var statusCode = ex switch
            {
                OpenCodeNotFoundException => StatusCodes.Status404NotFound,
                OpenCodeConnectionException => StatusCodes.Status503ServiceUnavailable,
                OpenCodeTimeoutException => StatusCodes.Status504GatewayTimeout,
                OpenCodeApiException apiEx => (int)apiEx.StatusCode,
                _ => StatusCodes.Status500InternalServerError
            };

            context.Result = new ObjectResult(new
            {
                error = ex.Message,
                hint = ex.TroubleshootingHint,
                type = ex.GetType().Name
            })
            {
                StatusCode = statusCode
            };

            context.ExceptionHandled = true;
        }
    }
}
```

### Registration

```csharp
services.AddControllers(options =>
{
    options.Filters.Add<OpenCodeExceptionFilter>();
});
```

## Health Check Integration

```csharp
public class OpenCodeHealthCheck : IHealthCheck
{
    private readonly IOpenCodeClient _client;

    public OpenCodeHealthCheck(IOpenCodeClient client)
    {
        _client = client;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Quick check - list projects
            await _client.ListProjectsAsync(cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy("OpenCode server is responding");
        }
        catch (OpenCodeConnectionException)
        {
            return HealthCheckResult.Unhealthy("Cannot connect to OpenCode server");
        }
        catch (OpenCodeTimeoutException)
        {
            return HealthCheckResult.Degraded("OpenCode server is slow to respond");
        }
        catch (OpenCodeException ex)
        {
            return HealthCheckResult.Unhealthy($"OpenCode error: {ex.Message}");
        }
    }
}

// Registration
services.AddHealthChecks()
    .AddCheck<OpenCodeHealthCheck>("opencode");
```

## Troubleshooting Common Issues

### "Connection Refused"

```
OpenCodeConnectionException: Failed to connect to OpenCode server at http://localhost:9123
Hint: Ensure OpenCode server is running. Start it with: opencode serve
```

**Solutions:**
1. Start the OpenCode server: `opencode serve`
2. Check if the port is correct
3. Verify firewall settings

### "Server returned HTML instead of JSON"

```
OpenCodeApiException: Server returned HTML instead of JSON. This may indicate an invalid endpoint or server error.
```

**Solutions:**
1. Verify the BaseUrl is correct (no trailing path)
2. Check if the API endpoint exists
3. Verify OpenCode server version compatibility

### "Timeout waiting for response"

```
OpenCodeTimeoutException: Operation timed out after 300 seconds
Hint: Consider increasing MessageTimeout or simplifying the request.
```

**Solutions:**
1. Increase `MessageTimeout` in options
2. Simplify the prompt
3. Check if the model/provider is working

### "Empty response from server"

```
OpenCodeApiException: Server returned empty response.
Hint: This may indicate the specified model is not available or requires provider authentication.
```

**Solutions:**
1. Use the default model without specifying provider/model
2. Authenticate with the provider: `opencode auth login <provider>`
3. Check provider status

## Related Documentation

- [Getting Started](./getting-started.md)
- [Configuration Reference](./configuration-reference.md)
- [Performance Tuning Guide](./performance-tuning.md)
