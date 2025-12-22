# Frequently Asked Questions (FAQ)

## General Questions

### What is LionFire.OpenCode.Serve?

LionFire.OpenCode.Serve is a .NET SDK for interacting with the [OpenCode](https://github.com/opencode-ai/opencode) headless server (`opencode serve`). It provides a type-safe, async-first API for building AI-powered .NET applications.

### What .NET versions are supported?

The SDK requires **.NET 8.0 or later**. It's built with modern .NET features including:
- `IAsyncEnumerable` for streaming
- Source-generated JSON serialization
- Native AOT compatibility

### Is this an official Anthropic/OpenAI SDK?

No. This SDK connects to the OpenCode local server, which in turn connects to AI providers (Anthropic, OpenAI, etc.). You configure provider API keys in OpenCode, not in this SDK.

### How is this different from using provider SDKs directly?

| Feature | Provider SDKs | OpenCode SDK |
|---------|---------------|--------------|
| Multiple providers | One SDK per provider | Single unified API |
| Tool execution | Manual implementation | Built-in (file ops, bash, etc.) |
| Session management | Manual | Built-in with history |
| Permissions | N/A | Built-in permission system |
| Project context | Manual | Automatic workspace awareness |

## Installation

### How do I install the SDK?

```bash
# Main SDK
dotnet add package LionFire.OpenCode.Serve

# For Microsoft.Extensions.AI integration
dotnet add package LionFire.OpenCode.Serve.AgentFramework
```

### What are the system requirements?

- .NET 8.0 SDK or later
- OpenCode server (`npm install -g opencode`)
- Network access to localhost:9123 (default)

### Can I use this without OpenCode installed?

No. The SDK requires a running OpenCode server. Install OpenCode first:

```bash
npm install -g opencode
opencode serve
```

## Usage

### How do I create a basic chat interaction?

```csharp
using LionFire.OpenCode.Serve;
using LionFire.OpenCode.Serve.Models;

var client = new OpenCodeClient();
var session = await client.CreateSessionAsync();

var response = await client.PromptAsync(session.Id, new SendMessageRequest
{
    Parts = new List<PartInput> { PartInput.TextInput("Hello!") }
});

// Get text response
var text = response.Parts?.FirstOrDefault(p => p.IsTextPart)?.Text;
Console.WriteLine(text);
```

### How do I use dependency injection?

```csharp
// Registration
services.AddOpenCodeClient(options =>
{
    options.BaseUrl = "http://localhost:9123";
});

// Injection
public class MyService
{
    private readonly IOpenCodeClient _client;

    public MyService(IOpenCodeClient client)
    {
        _client = client;
    }
}
```

### How do I handle streaming responses?

```csharp
// Option 1: Subscribe to events
await client.PromptAsyncNonBlocking(sessionId, request);

await foreach (var evt in client.SubscribeToEventsAsync())
{
    Console.WriteLine($"Event: {evt.Type}");
    if (evt.Type == "complete") break;
}

// Option 2: Use IChatClient streaming
var chatClient = new OpenCodeChatClient(client);

await foreach (var update in chatClient.GetStreamingResponseAsync(messages))
{
    foreach (var content in update.Contents)
    {
        if (content is TextContent text)
        {
            Console.Write(text.Text);
        }
    }
}
```

### How do I specify which AI model to use?

```csharp
// Option 1: In the request
var request = new SendMessageRequest
{
    Parts = new List<PartInput> { PartInput.TextInput("Hello") },
    Model = new ModelReference
    {
        ProviderId = "anthropic",
        ModelId = "claude-3-5-sonnet-20241022"
    }
};

// Option 2: Initialize session with specific model
await client.InitializeSessionAsync(sessionId, new InitSessionRequest
{
    ProviderId = "anthropic",
    ModelId = "claude-3-5-sonnet-20241022"
});
```

### How do I clean up sessions?

```csharp
// Manual cleanup
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

## Error Handling

### What exceptions should I handle?

```csharp
try
{
    await client.PromptAsync(sessionId, request);
}
catch (OpenCodeConnectionException ex)
{
    // Server not running
}
catch (OpenCodeNotFoundException ex)
{
    // Session/message not found
}
catch (OpenCodeTimeoutException ex)
{
    // Operation timed out
}
catch (OpenCodeApiException ex)
{
    // HTTP error from server
}
catch (OpenCodeException ex)
{
    // Any other SDK error
}
```

### Why am I getting empty responses?

Common causes:
1. **Provider not authenticated**: Run `opencode auth login <provider>`
2. **Model not available**: Try without specifying a model
3. **API key issue**: Check provider API key is set

### Why am I getting connection refused?

The OpenCode server isn't running. Start it:

```bash
opencode serve
```

## Configuration

### How do I configure timeouts?

```csharp
var options = new OpenCodeClientOptions
{
    // For quick operations (list, get, delete)
    DefaultTimeout = TimeSpan.FromSeconds(30),

    // For AI operations
    MessageTimeout = TimeSpan.FromMinutes(5)
};
```

### How do I enable retry?

```csharp
var options = new OpenCodeClientOptions
{
    EnableRetry = true,
    MaxRetryAttempts = 3,
    RetryDelaySeconds = 2  // Exponential backoff
};
```

### How do I disable telemetry?

```csharp
var options = new OpenCodeClientOptions
{
    EnableTelemetry = false
};
```

## Microsoft.Extensions.AI

### How do I use IChatClient?

```csharp
using LionFire.OpenCode.Serve.AgentFramework;
using Microsoft.Extensions.AI;

var chatClient = new OpenCodeChatClient(openCodeClient);

var response = await chatClient.GetResponseAsync(new[]
{
    new ChatMessage(ChatRole.User, "Hello!")
});
```

### Can I use this with Semantic Kernel?

Yes! Since the SDK implements `IChatClient`, it works with any framework that uses Microsoft.Extensions.AI abstractions:

```csharp
// Register as IChatClient
services.AddOpenCodeClient();
services.AddOpenCodeChatClient();

// Use with Semantic Kernel or other frameworks
// that accept IChatClient
```

## Performance

### How do I optimize for high throughput?

1. **Use HttpClientFactory** (automatic with DI)
2. **Reuse sessions** for multiple prompts
3. **Enable retry with backoff**
4. **Configure appropriate timeouts**

See [Performance Tuning Guide](./performance-tuning.md) for details.

### Is the SDK thread-safe?

Yes. `OpenCodeClient` is thread-safe and can be used from multiple threads concurrently. It's recommended to register as a singleton in DI.

### Does the SDK support Native AOT?

Yes! The SDK uses source-generated JSON serialization and is fully AOT-compatible.

## Troubleshooting

### Where are the logs?

Enable logging:

```csharp
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Logging.AddFilter("LionFire.OpenCode.Serve", LogLevel.Debug);
```

### How do I report a bug?

1. Check existing [GitHub issues](https://github.com/lionfire/opencode-dotnet/issues)
2. Collect diagnostic info (versions, error messages)
3. Create a minimal reproduction
4. Open a new issue

### Where can I get help?

- [GitHub Issues](https://github.com/lionfire/opencode-dotnet/issues)
- [Troubleshooting Runbook](./troubleshooting.md)
- [Error Handling Guide](./error-handling.md)

## Contributing

### How can I contribute?

1. Fork the repository
2. Create a feature branch
3. Make your changes with tests
4. Submit a pull request

See [CONTRIBUTING.md](../CONTRIBUTING.md) for details.

### What's the release schedule?

Releases follow semantic versioning:
- **Patches**: Bug fixes, released as needed
- **Minor**: New features, roughly monthly
- **Major**: Breaking changes, annually or less

## Related Documentation

- [Getting Started](./getting-started.md)
- [API Reference](./api-reference.md)
- [Configuration Reference](./configuration-reference.md)
- [Advanced Scenarios](./advanced-scenarios.md)
