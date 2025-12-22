# Performance Tuning Guide

This guide covers optimization strategies for the LionFire.OpenCode.Serve SDK.

## Connection Management

### HttpClientFactory Integration

Use `IHttpClientFactory` for proper connection pooling:

```csharp
// Register with HttpClientFactory
services.AddHttpClient<IOpenCodeClient, OpenCodeClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:9123");
    client.Timeout = TimeSpan.FromMinutes(5);
});

// Or use the extension method
services.AddOpenCodeClient(options =>
{
    options.BaseUrl = "http://localhost:9123";
});
```

**Benefits:**
- Connection pooling
- DNS refresh handling
- Automatic socket management

### Connection Lifetime

Configure connection lifetime for long-running applications:

```csharp
services.AddHttpClient<IOpenCodeClient, OpenCodeClient>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));
```

## Timeout Optimization

### Operation-Specific Timeouts

```csharp
var options = new OpenCodeClientOptions
{
    // Quick operations (list, get, delete)
    DefaultTimeout = TimeSpan.FromSeconds(15),

    // AI operations (longer by nature)
    MessageTimeout = TimeSpan.FromMinutes(3),
};
```

### Adjust Based on Use Case

| Scenario | DefaultTimeout | MessageTimeout |
|----------|----------------|----------------|
| Quick responses | 10s | 1-2 min |
| Complex code generation | 30s | 5-10 min |
| Large file analysis | 30s | 10-15 min |
| Interactive UI | 5s | 30s-1 min |

## JSON Serialization

### Source-Generated JSON (AOT-Compatible)

For best performance and Native AOT compatibility:

```csharp
using LionFire.OpenCode.Serve.Internal;

// The SDK provides a source-generated serializer context
var serializerContext = OpenCodeSerializerContext.Default;

// Access optimized options
var options = JsonOptions.SourceGenerated;
```

### Benefits of Source Generation

- **Startup time**: No reflection-based type analysis
- **Memory**: Reduced allocations
- **AOT**: Full Native AOT support
- **Performance**: 2-5x faster serialization

## Async Best Practices

### Proper Async/Await Usage

```csharp
// Good: Let async flow naturally
public async Task<string> GetResponseAsync()
{
    var session = await client.CreateSessionAsync();
    var response = await client.PromptAsync(session.Id, request);
    return response.Parts?.FirstOrDefault(p => p.IsTextPart)?.Text ?? "";
}

// Avoid: Blocking on async
public string GetResponseBlocking()
{
    // DON'T DO THIS - can cause deadlocks
    return client.PromptAsync(sessionId, request).Result;
}
```

### Cancellation Token Support

```csharp
public async Task<string> GetResponseAsync(CancellationToken cancellationToken)
{
    var session = await client.CreateSessionAsync(cancellationToken: cancellationToken);
    try
    {
        var response = await client.PromptAsync(session.Id, request,
            cancellationToken: cancellationToken);
        return response.Parts?.FirstOrDefault(p => p.IsTextPart)?.Text ?? "";
    }
    finally
    {
        // Clean up even if cancelled
        await client.DeleteSessionAsync(session.Id);
    }
}
```

### Parallel Operations

```csharp
// Good: Parallel independent operations
var tasks = new[]
{
    client.ListProjectsAsync(),
    client.GetConfigAsync(),
    client.ListProvidersAsync()
};

var results = await Task.WhenAll(tasks);
```

## Session Management

### Session Reuse

Reuse sessions for multiple prompts in the same context:

```csharp
// Efficient: Reuse session
var session = await client.CreateSessionAsync();
try
{
    // Multiple prompts to same session
    await client.PromptAsync(session.Id, prompt1);
    await client.PromptAsync(session.Id, prompt2);
    await client.PromptAsync(session.Id, prompt3);
}
finally
{
    await client.DeleteSessionAsync(session.Id);
}

// Less efficient: New session per prompt
foreach (var prompt in prompts)
{
    var session = await client.CreateSessionAsync();
    await client.PromptAsync(session.Id, prompt);
    await client.DeleteSessionAsync(session.Id);
}
```

### Session Cleanup

Always clean up sessions to avoid resource leaks:

```csharp
// Using try/finally
var session = await client.CreateSessionAsync();
try
{
    // Use session
}
finally
{
    await client.DeleteSessionAsync(session.Id);
}
```

## Memory Optimization

### Streaming Large Responses

For large responses, prefer streaming to reduce memory pressure:

```csharp
// Send non-blocking, then stream events
await client.PromptAsyncNonBlocking(sessionId, request);

await foreach (var evt in client.SubscribeToEventsAsync())
{
    // Process events incrementally
    ProcessEvent(evt);

    if (evt.Type == "complete")
        break;
}
```

### Dispose Resources

```csharp
// Client implements IAsyncDisposable
await using var client = new OpenCodeClient();

// Ensure cleanup even on exceptions
try
{
    await using var client = new OpenCodeClient();
    // Use client
}
catch (Exception)
{
    // Client is properly disposed
}
```

## OpenTelemetry Integration

### Setup Tracing

```csharp
services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("LionFire.OpenCode.Serve")
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter());
```

### Available Spans

The SDK emits traces for:
- HTTP requests to OpenCode server
- Session lifecycle (create, delete)
- Message operations (prompt)
- File operations

### Trace Attributes

| Attribute | Description |
|-----------|-------------|
| `opencode.session.id` | Session ID |
| `opencode.operation` | Operation name |
| `opencode.duration.ms` | Operation duration |
| `http.status_code` | HTTP response code |

### Disable for Performance

If telemetry overhead is a concern:

```csharp
services.AddOpenCodeClient(options =>
{
    options.EnableTelemetry = false;
});
```

## Retry Configuration

### Tune Retry Behavior

```csharp
var options = new OpenCodeClientOptions
{
    EnableRetry = true,
    MaxRetryAttempts = 3,        // Max attempts
    RetryDelaySeconds = 2         // Base delay (exponential: 2s, 4s, 8s)
};
```

### Disable Retry for Latency-Sensitive Operations

```csharp
// For real-time UI, fail fast
var options = new OpenCodeClientOptions
{
    EnableRetry = false
};
```

## Caching Strategies

### Cache Static Data

```csharp
public class CachedOpenCodeService
{
    private readonly IOpenCodeClient _client;
    private readonly IMemoryCache _cache;

    public async Task<List<Provider>> GetProvidersAsync()
    {
        return await _cache.GetOrCreateAsync("providers", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            return await _client.ListProvidersAsync();
        });
    }

    public async Task<Dictionary<string, object>> GetConfigAsync()
    {
        return await _cache.GetOrCreateAsync("config", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _client.GetConfigAsync();
        });
    }
}
```

## Load Balancing Multiple Servers

For high availability, configure multiple OpenCode servers:

```csharp
public class LoadBalancedOpenCodeClient
{
    private readonly IOpenCodeClient[] _clients;
    private int _currentIndex;

    public LoadBalancedOpenCodeClient(string[] serverUrls)
    {
        _clients = serverUrls.Select(url =>
            new OpenCodeClient(new OpenCodeClientOptions { BaseUrl = url }))
            .ToArray();
    }

    public IOpenCodeClient GetClient()
    {
        // Round-robin selection
        var index = Interlocked.Increment(ref _currentIndex) % _clients.Length;
        return _clients[index];
    }
}
```

## Benchmarking

The SDK includes a comprehensive benchmark suite to measure performance characteristics.

### Running Benchmarks

```bash
# Navigate to benchmark project
cd benchmarks/LionFire.OpenCode.Serve.Benchmarks

# Run all benchmarks
dotnet run -c Release

# Run specific benchmark category
dotnet run -c Release -- --filter *JsonSerialization*
dotnet run -c Release -- --filter *MessageList*
dotnet run -c Release -- --filter *StreamingEvent*
```

### Performance Targets

The SDK is designed to meet these performance targets:

| Operation | Target | Notes |
|-----------|--------|-------|
| Message serialization | < 1ms | Single message with parts |
| Message deserialization | < 1ms | Single message with parts |
| Thread serialization (100 msgs) | < 10ms | Full conversation |
| SSE event parsing | < 5ms overhead | Per streaming chunk |
| Session roundtrip | < 0.5ms | Serialize + deserialize |

### Measure Operation Latency

```csharp
public async Task<(T Result, TimeSpan Duration)> MeasureAsync<T>(Func<Task<T>> operation)
{
    var sw = Stopwatch.StartNew();
    var result = await operation();
    sw.Stop();
    return (result, sw.Elapsed);
}

// Usage
var (session, duration) = await MeasureAsync(() => client.CreateSessionAsync());
Console.WriteLine($"Session created in {duration.TotalMilliseconds}ms");
```

### Performance Metrics to Monitor

| Metric | Target | Alert Threshold |
|--------|--------|-----------------|
| Session creation | < 500ms | > 2s |
| Prompt response (simple) | < 5s | > 30s |
| Prompt response (complex) | < 30s | > 2min |
| List operations | < 200ms | > 1s |
| JSON serialization | < 1ms | > 5ms |
| SSE event parsing | < 5ms | > 20ms |

## Object Pooling

The SDK includes built-in object pooling to reduce garbage collection pressure.

### StringBuilder Pool

For efficient string building during SSE parsing:

```csharp
using LionFire.OpenCode.Serve.Internal;

// Get from pool
var sb = StringBuilderPool.Get();
try
{
    sb.Append("data: ");
    sb.Append(jsonContent);
    return sb.ToString();
}
finally
{
    // Return to pool
    StringBuilderPool.Return(sb);
}

// Or use the helper method
var result = StringBuilderPool.GetString(sb =>
{
    sb.Append("prefix_");
    sb.Append(content);
});
```

### Array Pool Usage

For efficient buffer management:

```csharp
using LionFire.OpenCode.Serve.Internal;

// Rent a byte buffer
var buffer = BufferPoolHelper.RentBytes(4096);
try
{
    // Use buffer for reading/writing
    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
}
finally
{
    // Return to pool
    BufferPoolHelper.ReturnBytes(buffer);
}

// Or use RentedArray for automatic cleanup
using var rentedBuffer = new RentedArray<byte>(4096);
await stream.ReadAsync(rentedBuffer.Memory);
```

### SSE Parsing Helpers

Optimized helpers for Server-Sent Events parsing:

```csharp
using LionFire.OpenCode.Serve.Internal;

// Check if a line is a data line
ReadOnlySpan<char> line = "data: {...}";
if (SseParsingHelper.IsDataLine(line))
{
    var json = SseParsingHelper.ExtractJsonFromDataLine(line);
    // Parse json...
}

// Find complete events in a buffer
ReadOnlySpan<byte> buffer = ...;
if (SseParsingHelper.TryFindNextEvent(buffer, out int eventEnd))
{
    var eventData = buffer.Slice(0, eventEnd);
    // Process event...
}
```

## Span-Based Optimizations

The SDK uses `Span<T>` and `ReadOnlySpan<T>` where possible to reduce allocations.

### String Operations

```csharp
// Efficient substring check without allocation
ReadOnlySpan<char> line = someLine;
if (line.StartsWith("data: "))
{
    var content = line.Slice(6);
    // content is a span - no heap allocation
}
```

### JSON Parsing with Spans

```csharp
// Parse directly from a span
ReadOnlySpan<char> jsonSpan = GetJsonContent();
var message = JsonSerializer.Deserialize<Message>(jsonSpan, JsonOptions.SourceGenerated);
```

## Production Checklist

- [ ] Use `IHttpClientFactory` for connection pooling
- [ ] Configure appropriate timeouts for your use case
- [ ] Enable retry with sensible limits
- [ ] Implement proper session cleanup
- [ ] Set up OpenTelemetry for monitoring
- [ ] Add health checks for OpenCode server
- [ ] Configure logging at appropriate levels
- [ ] Cache static data where possible
- [ ] Use cancellation tokens for user-initiated operations
- [ ] Test under load to validate settings

## Related Documentation

- [Configuration Reference](./configuration-reference.md)
- [Error Handling Guide](./error-handling.md)
- [Enterprise Deployment](./enterprise-deployment.md)
