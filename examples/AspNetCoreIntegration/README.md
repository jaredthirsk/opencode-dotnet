# ASP.NET Core Integration Example

This example demonstrates integrating the LionFire.OpenCode.Serve SDK with ASP.NET Core using dependency injection, IHttpClientFactory, and connection pooling.

## Features Demonstrated

- **IHttpClientFactory Integration**: Proper HttpClient lifecycle management with connection pooling
- **Dependency Injection**: Full DI support with `AddOpenCodeClient()` extension method
- **Configuration Binding**: Options pattern integration with appsettings.json
- **Health Checks**: Monitor OpenCode server availability
- **IOpenCodeClient Usage**: Direct SDK usage in endpoints
- **IChatClient Usage**: Microsoft.Extensions.AI abstraction support
- **Session Management**: Create and clean up sessions per request

## Prerequisites

1. .NET 8.0 SDK or later
2. OpenCode server running (`opencode serve`)

## Running the Example

```bash
cd examples/AspNetCoreIntegration
dotnet run
```

The server will start on `http://localhost:5000`.

## DI Registration Patterns

### Basic Registration

The simplest way to register the OpenCode client:

```csharp
// Program.cs
using LionFire.OpenCode.Serve.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register with default options (localhost:9123)
builder.Services.AddOpenCodeClient();
```

### With Custom Options

Configure the client with custom settings:

```csharp
builder.Services.AddOpenCodeClient(options =>
{
    options.BaseUrl = "http://localhost:9123";
    options.DefaultTimeout = TimeSpan.FromSeconds(60);
    options.MessageTimeout = TimeSpan.FromMinutes(10);
    options.EnableTelemetry = true;
    options.EnableRetry = true;
    options.MaxRetryAttempts = 3;
});
```

### With Configuration Binding

Bind options from appsettings.json:

```csharp
// Program.cs
builder.Services.AddOpenCodeClient(options =>
{
    options.BaseUrl = builder.Configuration.GetValue<string>("OpenCode:BaseUrl")
        ?? "http://localhost:9123";
    options.EnableTelemetry = builder.Configuration.GetValue<bool>("OpenCode:EnableTelemetry", true);
});
```

```json
// appsettings.json
{
  "OpenCode": {
    "BaseUrl": "http://localhost:9123",
    "EnableTelemetry": true
  }
}
```

### With Polly Resilience Policies

The `AddOpenCodeClient()` method returns an `IHttpClientBuilder`, allowing you to chain Polly policies:

```csharp
using Microsoft.Extensions.Http.Resilience;

builder.Services.AddOpenCodeClient(options =>
{
    options.BaseUrl = "http://localhost:9123";
})
.AddStandardResilienceHandler(); // Add standard resilience (retry, circuit breaker, timeout)
```

Or with custom policies:

```csharp
using Polly;
using Polly.Extensions.Http;

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

builder.Services.AddOpenCodeClient(options =>
{
    options.BaseUrl = "http://localhost:9123";
})
.AddPolicyHandler(retryPolicy);
```

### With Custom HttpClient (Advanced)

For full control over HttpClient lifecycle:

```csharp
var httpClient = new HttpClient
{
    BaseAddress = new Uri("http://localhost:9123"),
    Timeout = TimeSpan.FromMinutes(5)
};

builder.Services.AddOpenCodeClient(httpClient, options =>
{
    options.EnableTelemetry = true;
});
```

## Connection Pooling and HttpClientFactory

The SDK uses `IHttpClientFactory` for proper connection management:

### Benefits

1. **Connection Pooling**: HttpClientFactory manages a pool of HttpMessageHandler instances, reusing them efficiently
2. **DNS Refresh**: Automatically refreshes DNS lookups to handle server IP changes
3. **Resource Management**: Prevents socket exhaustion by properly managing connections
4. **Handler Lifetime**: Default handler lifetime of 2 minutes ensures fresh connections

### How It Works

```csharp
// Internally, the SDK registers like this:
services.AddHttpClient<IOpenCodeClient, OpenCodeClient>("OpenCodeClient", (sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<OpenCodeClientOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = options.DefaultTimeout;
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});
```

### Configuring Handler Lifetime

```csharp
builder.Services.AddOpenCodeClient(options =>
{
    options.BaseUrl = "http://localhost:9123";
})
.SetHandlerLifetime(TimeSpan.FromMinutes(5)); // Customize handler lifetime
```

### Primary Handler Configuration

```csharp
builder.Services.AddOpenCodeClient(options =>
{
    options.BaseUrl = "http://localhost:9123";
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(2),
    MaxConnectionsPerServer = 100,
    EnableMultipleHttp2Connections = true
});
```

## API Endpoints

### POST /api/chat
Chat using IOpenCodeClient directly.

```bash
curl -X POST http://localhost:5000/api/chat \
  -H "Content-Type: application/json" \
  -d '{"message": "Hello!"}'
```

Response:
```json
{
  "response": "Hello! How can I help you today?",
  "messageId": "msg_abc123"
}
```

### POST /api/chat/ai
Chat using IChatClient (Microsoft.Extensions.AI abstraction).

```bash
curl -X POST http://localhost:5000/api/chat/ai \
  -H "Content-Type: application/json" \
  -d '{"message": "What is 2+2?"}'
```

### GET /api/sessions
List all sessions.

```bash
curl http://localhost:5000/api/sessions
```

### GET /health
Health check endpoint.

```bash
curl http://localhost:5000/health
```

## Configuration

Edit `appsettings.json` to change the OpenCode server URL:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "LionFire.OpenCode.Serve": "Debug"
    }
  },
  "OpenCode": {
    "BaseUrl": "http://localhost:9123"
  }
}
```

## Options Reference

| Option | Default | Description |
|--------|---------|-------------|
| `BaseUrl` | `http://localhost:9123` | Base URL of the OpenCode server |
| `Directory` | `null` | Optional working directory context |
| `DefaultTimeout` | 30 seconds | Timeout for quick operations |
| `MessageTimeout` | 5 minutes | Timeout for AI message operations |
| `EnableRetry` | `true` | Enable automatic retry for transient failures |
| `MaxRetryAttempts` | 3 | Maximum retry attempts |
| `RetryDelaySeconds` | 2 | Base delay between retries (exponential backoff) |
| `EnableTelemetry` | `true` | Enable OpenTelemetry tracing |
| `ValidateOnStart` | `true` | Validate options on startup |

## Key Concepts

1. **AddOpenCodeClient()**: Registers IOpenCodeClient with IHttpClientFactory for proper lifecycle management
2. **AddOpenCodeChatClient()**: Registers IChatClient implementation for Microsoft.Extensions.AI compatibility
3. **IHttpClientFactory**: Ensures connection pooling and prevents socket exhaustion
4. **Health Checks**: Monitor OpenCode server availability with ASP.NET Core health checks
5. **Session Management**: Create and clean up sessions per request to avoid resource leaks

## Testing Connection Pooling

To verify connection pooling is working:

```csharp
// Add logging to see connection reuse
builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Debug);
```

With debug logging enabled, you'll see:
- Handler creation and reuse
- Connection pool statistics
- DNS refresh events

## Production Recommendations

1. **Configure Timeouts**: Set appropriate timeouts for your use case
2. **Enable Retry Policies**: Use Polly for resilience against transient failures
3. **Monitor Health**: Use the health check endpoint in your monitoring system
4. **Logging**: Enable structured logging for troubleshooting
5. **OpenTelemetry**: Enable telemetry for distributed tracing

## Next Steps

- Add authentication/authorization
- Implement rate limiting
- Configure OpenTelemetry tracing with Jaeger/Zipkin
- Deploy to production with health probes
