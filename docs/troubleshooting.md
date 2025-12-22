# Troubleshooting Runbook

This guide provides solutions for common issues when using the LionFire.OpenCode.Serve SDK.

## Quick Diagnosis

### Connection Issues

**Symptom:** `OpenCodeConnectionException: Failed to connect to OpenCode server`

```
1. Is OpenCode server running?
   $ ps aux | grep "opencode serve"
   $ curl http://localhost:9123/api/projects

2. Is the port correct?
   $ netstat -an | grep 9123

3. Is firewall blocking?
   $ sudo ufw status
   $ iptables -L -n

4. Is the URL correct in your code?
   Check: options.BaseUrl = "http://localhost:9123"
```

### Authentication Issues

**Symptom:** Empty responses or `401 Unauthorized`

```
1. Check provider authentication
   $ opencode auth status

2. Re-authenticate if needed
   $ opencode auth login anthropic
   $ opencode auth login openai

3. Verify API keys are set
   $ echo $ANTHROPIC_API_KEY
   $ echo $OPENAI_API_KEY
```

### Timeout Issues

**Symptom:** `OpenCodeTimeoutException`

```
1. For quick operations timing out:
   - Increase DefaultTimeout
   - Check network latency

2. For AI operations timing out:
   - Increase MessageTimeout
   - Simplify the prompt
   - Check if model is available
```

## Common Problems and Solutions

### Problem: "Server returned HTML instead of JSON"

**Cause:** Wrong URL or API path

**Solution:**
```csharp
// Wrong - includes API path
options.BaseUrl = "http://localhost:9123/api";

// Correct - just the base URL
options.BaseUrl = "http://localhost:9123";
```

---

### Problem: "Connection refused on localhost:9123"

**Cause:** OpenCode server not running

**Solution:**
```bash
# Start the server
opencode serve

# Or in background
opencode serve &

# Check if running
curl http://localhost:9123/api/projects
```

---

### Problem: "Empty response from server"

**Causes:**
1. Model not available
2. Provider not authenticated
3. Rate limiting

**Solution:**
```bash
# 1. Check provider status
opencode auth status

# 2. Authenticate if needed
opencode auth login anthropic

# 3. Try without specifying model (use default)
```

```csharp
// Remove specific model to use default
var request = new SendMessageRequest
{
    Parts = new List<PartInput> { PartInput.TextInput("Hello") },
    Model = null  // Use server default
};
```

---

### Problem: "Session not found" after restart

**Cause:** Sessions don't persist across server restarts

**Solution:**
```csharp
// Always handle session not found
try
{
    await client.GetSessionAsync(savedSessionId);
}
catch (OpenCodeNotFoundException)
{
    // Create new session
    var session = await client.CreateSessionAsync();
    savedSessionId = session.Id;
}
```

---

### Problem: "Request timeout" on large prompts

**Cause:** Default timeout too short for complex operations

**Solution:**
```csharp
var options = new OpenCodeClientOptions
{
    // Increase for complex operations
    MessageTimeout = TimeSpan.FromMinutes(10)
};
```

---

### Problem: "JSON deserialization error"

**Causes:**
1. SDK version mismatch with server
2. Unexpected response format

**Solution:**
```bash
# 1. Update SDK to latest
dotnet add package LionFire.OpenCode.Serve --version latest

# 2. Update OpenCode server
npm update -g opencode

# 3. Check versions match
opencode --version
dotnet list package | grep OpenCode
```

---

### Problem: Permission requests not being handled

**Cause:** Not subscribing to events

**Solution:**
```csharp
// Use non-blocking prompt with event subscription
await client.PromptAsyncNonBlocking(sessionId, request);

await foreach (var evt in client.SubscribeToEventsAsync())
{
    if (evt.Type == "permission")
    {
        // Handle permission request
        await client.RespondToPermissionAsync(
            sessionId, permissionId, response);
    }

    if (evt.Type == "complete")
        break;
}
```

---

### Problem: High memory usage with many sessions

**Cause:** Sessions not being cleaned up

**Solution:**
```csharp
// Always delete sessions when done
var session = await client.CreateSessionAsync();
try
{
    // Use session
}
finally
{
    await client.DeleteSessionAsync(session.Id);
}

// Or list and clean up orphaned sessions
var sessions = await client.ListSessionsAsync();
foreach (var session in sessions.Where(IsOrphaned))
{
    await client.DeleteSessionAsync(session.Id);
}
```

---

### Problem: SSL/TLS errors with HTTPS

**Cause:** Certificate issues

**Solution:**
```csharp
// For development only - don't use in production
services.AddHttpClient<IOpenCodeClient, OpenCodeClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
```

For production, ensure proper certificates are installed.

---

### Problem: Rate limiting (429 errors)

**Cause:** Too many requests to AI provider

**Solution:**
```csharp
// 1. Enable retry with backoff
var options = new OpenCodeClientOptions
{
    EnableRetry = true,
    MaxRetryAttempts = 5,
    RetryDelaySeconds = 5  // Longer delay
};

// 2. Add request throttling
services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        context => RateLimitPartition.GetSlidingWindowLimiter(
            "opencode",
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

## Diagnostic Commands

### Check OpenCode Server Status

```bash
# Is server running?
curl http://localhost:9123/api/projects

# Check configuration
curl http://localhost:9123/api/config

# List providers
curl http://localhost:9123/api/providers

# Check provider auth
opencode auth status
```

### Check .NET SDK Configuration

```csharp
// Log current configuration
Console.WriteLine($"BaseUrl: {options.BaseUrl}");
Console.WriteLine($"DefaultTimeout: {options.DefaultTimeout}");
Console.WriteLine($"MessageTimeout: {options.MessageTimeout}");
Console.WriteLine($"EnableRetry: {options.EnableRetry}");
Console.WriteLine($"EnableTelemetry: {options.EnableTelemetry}");
```

### Enable Debug Logging

```csharp
// In Program.cs
builder.Logging
    .AddConsole()
    .SetMinimumLevel(LogLevel.Debug)
    .AddFilter("LionFire.OpenCode.Serve", LogLevel.Trace);
```

### Network Diagnostics

```bash
# Check if port is in use
netstat -an | grep 9123

# Test connectivity
curl -v http://localhost:9123/api/projects

# Check DNS resolution (if using hostname)
nslookup opencode.internal

# Trace network path
traceroute localhost
```

## Log Analysis

### Common Log Patterns

**Successful request:**
```
[DBG] GET /api/sessions
[DBG] Response: 200 OK
```

**Connection failure:**
```
[ERR] HttpRequestException: Connection refused
[ERR] Failed to connect to OpenCode server at http://localhost:9123
```

**Timeout:**
```
[WRN] Request timeout after 00:05:00
[ERR] OpenCodeTimeoutException: Operation timed out
```

**Authentication error:**
```
[WRN] POST /api/session/{id}/prompt returned 200 with empty body
[ERR] Empty response - provider may require authentication
```

### Log Aggregation Query Examples

**Serilog with Seq:**
```sql
-- Find all errors
Level = "Error" and SourceContext like "LionFire.OpenCode.Serve%"

-- Find timeout issues
@Message like "%timeout%"

-- Track request durations
SourceContext = "LionFire.OpenCode.Serve.OpenCodeClient"
| summarize avg(Duration) by Operation
```

## Recovery Procedures

### Restart OpenCode Server

```bash
# Find and kill existing process
pkill -f "opencode serve"

# Start fresh
opencode serve

# Verify
curl http://localhost:9123/api/projects
```

### Clear OpenCode Cache

```bash
# Clear OpenCode cache and data
rm -rf ~/.opencode/cache

# Restart server
opencode serve
```

### Reset Session State

```csharp
// Clear all sessions
var sessions = await client.ListSessionsAsync();
foreach (var session in sessions)
{
    await client.DeleteSessionAsync(session.Id);
}
```

### Re-authenticate Providers

```bash
# Clear and re-authenticate
opencode auth logout anthropic
opencode auth login anthropic

opencode auth logout openai
opencode auth login openai

# Verify
opencode auth status
```

## Getting Help

### Collect Diagnostic Information

Before reporting an issue, collect:

```bash
# 1. SDK version
dotnet list package | grep OpenCode

# 2. OpenCode version
opencode --version

# 3. .NET version
dotnet --version

# 4. OS information
uname -a  # Linux/Mac
systeminfo | findstr /B /C:"OS"  # Windows

# 5. Server status
curl http://localhost:9123/api/projects 2>&1

# 6. Provider status
opencode auth status
```

### Report Format

```markdown
## Environment
- SDK Version: x.x.x
- OpenCode Version: x.x.x
- .NET Version: x.x
- OS: Linux/Windows/macOS

## Problem
[Describe the issue]

## Steps to Reproduce
1. ...
2. ...
3. ...

## Expected Behavior
[What should happen]

## Actual Behavior
[What actually happens]

## Error Messages
```
[Paste full error message and stack trace]
```

## Code Sample
```csharp
// Minimal reproduction code
```

## Logs
```
[Relevant log entries]
```
```

## Performance Issues

### Problem: High memory usage during streaming

**Cause:** Not processing events incrementally

**Solution:**
```csharp
// Bad: Collecting all events in memory
var allEvents = new List<Event>();
await foreach (var evt in client.SubscribeToEventsAsync())
{
    allEvents.Add(evt);  // Memory grows unbounded
}

// Good: Process events as they arrive
await foreach (var evt in client.SubscribeToEventsAsync())
{
    ProcessEvent(evt);  // Process and discard
    if (IsComplete(evt)) break;
}
```

---

### Problem: Slow JSON serialization

**Cause:** Using reflection-based serialization

**Solution:**
```csharp
// Use source-generated JSON options for better performance
using LionFire.OpenCode.Serve.Internal;

var options = JsonOptions.SourceGenerated;  // Pre-compiled
// NOT: new JsonSerializerOptions { ... }   // Reflection-based
```

---

### Problem: High allocation rate / GC pressure

**Cause:** Creating new objects in hot paths

**Solution:**
```csharp
using LionFire.OpenCode.Serve.Internal;

// Use StringBuilder pooling
var sb = StringBuilderPool.Get();
try
{
    sb.Append("data: ");
    sb.Append(json);
    return sb.ToString();
}
finally
{
    StringBuilderPool.Return(sb);
}

// Use array pooling for buffers
using var buffer = new RentedArray<byte>(4096);
await stream.ReadAsync(buffer.Memory);
```

---

### Problem: Slow SSE event parsing

**Cause:** Inefficient string operations

**Solution:**
```csharp
using LionFire.OpenCode.Serve.Internal;

// Use span-based parsing
ReadOnlySpan<char> line = eventLine;
if (SseParsingHelper.IsDataLine(line))
{
    var json = SseParsingHelper.ExtractJsonFromDataLine(line);
    // No intermediate string allocations
}
```

---

### Problem: Thread pool exhaustion

**Cause:** Blocking on async code

**Solution:**
```csharp
// Bad: Blocking
var result = client.CreateSessionAsync().Result;

// Good: Async all the way
var result = await client.CreateSessionAsync();

// If you must block (console app Main):
await client.CreateSessionAsync().AsTask();
```

---

### Problem: Connection pool exhaustion

**Cause:** Not using IHttpClientFactory

**Solution:**
```csharp
// Use dependency injection with HttpClientFactory
services.AddOpenCodeClient(options =>
{
    options.BaseUrl = "http://localhost:9123";
});

// Or configure HttpClient properly
services.AddHttpClient<IOpenCodeClient, OpenCodeClient>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));
```

---

### Performance Diagnostics

**Check allocation rate:**
```csharp
// Enable allocation tracking
GC.TryStartNoGCRegion(1024 * 1024);  // 1MB
try
{
    // Your code here
}
finally
{
    GC.EndNoGCRegion();
}

// Or use dotnet-counters
// dotnet counters monitor -p <PID> --counters System.Runtime
```

**Profile with BenchmarkDotNet:**
```bash
cd benchmarks/LionFire.OpenCode.Serve.Benchmarks
dotnet run -c Release -- --filter *YourBenchmark*
```

**Check memory with dotnet-gcdump:**
```bash
dotnet gcdump collect -p <PID>
dotnet gcdump report <dumpfile.gcdump>
```

### Resources

- [GitHub Issues](https://github.com/lionfire/opencode-dotnet/issues)
- [API Reference](./api-reference.md)
- [Error Handling Guide](./error-handling.md)
- [Configuration Reference](./configuration-reference.md)
- [Performance Tuning Guide](./performance-tuning.md)
