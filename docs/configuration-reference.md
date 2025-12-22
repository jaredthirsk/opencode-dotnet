# Configuration Reference

Complete reference for all configuration options in the LionFire.OpenCode.Serve SDK.

## OpenCodeClientOptions

The main configuration class for the OpenCode client.

```csharp
var options = new OpenCodeClientOptions
{
    BaseUrl = "http://localhost:9123",
    Directory = null,
    DefaultTimeout = TimeSpan.FromSeconds(30),
    MessageTimeout = TimeSpan.FromMinutes(5),
    EnableRetry = true,
    MaxRetryAttempts = 3,
    RetryDelaySeconds = 2,
    EnableTelemetry = true,
    ValidateOnStart = true
};
```

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `BaseUrl` | `string` | `"http://localhost:9123"` | The base URL of the OpenCode server |
| `Directory` | `string?` | `null` | Default working directory for operations |
| `DefaultTimeout` | `TimeSpan` | 30 seconds | Timeout for quick operations (list, get, delete) |
| `MessageTimeout` | `TimeSpan` | 5 minutes | Timeout for AI message operations |
| `EnableRetry` | `bool` | `true` | Enable automatic retry for transient failures |
| `MaxRetryAttempts` | `int` | `3` | Maximum number of retry attempts |
| `RetryDelaySeconds` | `int` | `2` | Base delay between retries (exponential backoff) |
| `EnableTelemetry` | `bool` | `true` | Enable OpenTelemetry tracing |
| `ValidateOnStart` | `bool` | `true` | Validate options when client is created |

### Detailed Property Descriptions

#### BaseUrl

The URL where the OpenCode server is running.

```csharp
// Default: localhost
options.BaseUrl = "http://localhost:9123";

// Remote server
options.BaseUrl = "http://opencode.internal:9123";

// With HTTPS (if server supports it)
options.BaseUrl = "https://opencode.company.com:9123";
```

**Environment Variable Override:**
```bash
export OPENCODE_BASE_URL="http://localhost:9123"
```

#### Directory

The default working directory for file and session operations.

```csharp
// All operations default to this directory
options.Directory = "/path/to/project";

// Can be overridden per-operation
await client.ListFilesAsync(".", directory: "/different/project");
```

#### DefaultTimeout

Timeout for quick operations that don't involve AI processing.

```csharp
// For fast network, reduce timeout
options.DefaultTimeout = TimeSpan.FromSeconds(10);

// For slow/unreliable network
options.DefaultTimeout = TimeSpan.FromMinutes(1);
```

**Affects operations:**
- `ListSessionsAsync`
- `GetSessionAsync`
- `DeleteSessionAsync`
- `ListFilesAsync`
- `GetConfigAsync`
- Most non-AI operations

#### MessageTimeout

Timeout for AI operations that may take longer.

```csharp
// For quick responses
options.MessageTimeout = TimeSpan.FromMinutes(2);

// For complex tasks (code generation, large file analysis)
options.MessageTimeout = TimeSpan.FromMinutes(10);
```

**Affects operations:**
- `PromptAsync`
- Operations involving AI model responses

#### EnableRetry

Automatic retry with exponential backoff for transient failures.

```csharp
// Enable (default)
options.EnableRetry = true;
options.MaxRetryAttempts = 3;
options.RetryDelaySeconds = 2; // 2s, 4s, 8s...

// Disable for fail-fast behavior
options.EnableRetry = false;
```

**Retry triggers:**
- HTTP 503 Service Unavailable
- HTTP 429 Too Many Requests
- Network timeouts
- Connection refused

#### EnableTelemetry

OpenTelemetry distributed tracing support.

```csharp
// Enable (requires OpenTelemetry setup)
options.EnableTelemetry = true;

// Disable for minimal overhead
options.EnableTelemetry = false;
```

See [Performance Tuning Guide](./performance-tuning.md) for telemetry setup.

## OpenCodeChatClientOptions

Configuration for the Microsoft.Extensions.AI `IChatClient` wrapper.

```csharp
var chatOptions = new OpenCodeChatClientOptions
{
    Directory = "/path/to/project",
    ModelId = "opencode",
    BaseUrl = "http://localhost:9123",
    CreateSessionPerConversation = false,
    Model = new ModelReference
    {
        ProviderId = "anthropic",
        ModelId = "claude-3-5-sonnet-20241022"
    }
};
```

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Directory` | `string?` | `null` | Working directory for sessions |
| `ModelId` | `string?` | `"opencode"` | Model ID to report in metadata |
| `BaseUrl` | `string?` | `null` | Base URL for metadata (falls back to client URL) |
| `CreateSessionPerConversation` | `bool` | `false` | Create new session for each conversation |
| `Model` | `ModelReference?` | `null` | Specific model to use for prompts |

### Model Selection

```csharp
// Use OpenCode's default model
chatOptions.Model = null;

// Use specific provider/model
chatOptions.Model = new ModelReference
{
    ProviderId = "anthropic",
    ModelId = "claude-3-5-sonnet-20241022"
};

// Use OpenAI
chatOptions.Model = new ModelReference
{
    ProviderId = "openai",
    ModelId = "gpt-4o"
};
```

## Dependency Injection Configuration

### Basic Registration

```csharp
services.AddOpenCodeClient(options =>
{
    options.BaseUrl = "http://localhost:9123";
});
```

### With Configuration Binding

```csharp
// appsettings.json
{
  "OpenCode": {
    "BaseUrl": "http://localhost:9123",
    "DefaultTimeout": "00:00:30",
    "MessageTimeout": "00:05:00",
    "EnableRetry": true,
    "MaxRetryAttempts": 3
  }
}

// Program.cs
services.Configure<OpenCodeClientOptions>(
    configuration.GetSection("OpenCode"));
services.AddOpenCodeClient();
```

### With Validation

```csharp
services.AddOptions<OpenCodeClientOptions>()
    .Configure(options => options.BaseUrl = "http://localhost:9123")
    .ValidateDataAnnotations()
    .ValidateOnStart();

services.AddOpenCodeClient();
```

### Full Chat Client Registration

```csharp
// Register both client and chat client
services.AddOpenCodeClient(options =>
{
    options.BaseUrl = "http://localhost:9123";
    options.EnableTelemetry = true;
});

services.AddOpenCodeChatClient(chatOptions =>
{
    chatOptions.Directory = "/my/project";
    chatOptions.Model = new ModelReference
    {
        ProviderId = "anthropic",
        ModelId = "claude-3-5-sonnet-20241022"
    };
});
```

### Keyed Services

```csharp
// Register multiple chat clients
services.AddKeyedOpenCodeChatClient("project1", options =>
{
    options.Directory = "/project1";
});

services.AddKeyedOpenCodeChatClient("project2", options =>
{
    options.Directory = "/project2";
});
```

## Environment Variables

The SDK supports configuration via environment variables:

| Variable | Affects | Example |
|----------|---------|---------|
| `OPENCODE_BASE_URL` | `OpenCodeClientOptions.BaseUrl` | `http://localhost:9123` |
| `OPENCODE_TIMEOUT` | `OpenCodeClientOptions.DefaultTimeout` | `30` (seconds) |
| `OPENCODE_ENABLE_TELEMETRY` | `OpenCodeClientOptions.EnableTelemetry` | `true` |

```bash
export OPENCODE_BASE_URL="http://opencode.internal:9123"
export OPENCODE_TIMEOUT="60"
export OPENCODE_ENABLE_TELEMETRY="true"
```

## JSON Serialization Options

The SDK uses optimized JSON serialization:

```csharp
// Default options (reflection-based)
JsonOptions.Default

// Source-generated options (AOT-compatible)
JsonOptions.SourceGenerated

// Access the serializer context
JsonOptions.Context
```

### Custom Serialization

```csharp
using LionFire.OpenCode.Serve.Internal;

// Get the default options
var jsonOptions = JsonOptions.Default;

// Features enabled by default:
// - CamelCase property naming
// - Case-insensitive deserialization
// - Null values ignored when writing
// - String enums with camelCase
```

## Logging Configuration

The SDK integrates with Microsoft.Extensions.Logging:

```csharp
// With Serilog
services.AddLogging(builder =>
{
    builder.AddSerilog();
    builder.SetMinimumLevel(LogLevel.Debug);
});

// Enable detailed HTTP logging
builder.AddFilter("LionFire.OpenCode.Serve", LogLevel.Debug);
```

### Log Categories

| Category | Description |
|----------|-------------|
| `LionFire.OpenCode.Serve` | Main SDK operations |
| `LionFire.OpenCode.Serve.OpenCodeClient` | Client operations |
| `LionFire.OpenCode.Serve.AgentFramework` | Chat client operations |

## Related Documentation

- [Getting Started](./getting-started.md)
- [Advanced Scenarios](./advanced-scenarios.md)
- [Performance Tuning Guide](./performance-tuning.md)
- [Enterprise Deployment](./enterprise-deployment.md)
