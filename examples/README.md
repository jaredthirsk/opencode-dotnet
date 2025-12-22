# LionFire.OpenCode.Serve Examples

This directory contains example projects demonstrating various features of the LionFire.OpenCode.Serve SDK.

## Examples

| Example | Description |
|---------|-------------|
| [BasicSession](./BasicSession/) | Create sessions, send messages, get responses |
| [StreamingResponses](./StreamingResponses/) | Handle real-time streaming with SSE events |
| [AgentFramework](./AgentFramework/) | Use Microsoft.Extensions.AI IChatClient |
| [AspNetCoreIntegration](./AspNetCoreIntegration/) | Dependency injection in ASP.NET Core |
| [ErrorHandling](./ErrorHandling/) | Exception handling patterns |
| [AotExample](./AotExample/) | Native AOT compilation with source-generated JSON |

## Prerequisites

All examples require:

1. **.NET 8.0 SDK** or later
2. **OpenCode server** running locally:
   ```bash
   npm install -g opencode
   opencode serve
   ```

## Running Examples

From the repository root:

```bash
# Run a specific example
dotnet run --project examples/BasicSession

# Or navigate to the example directory
cd examples/BasicSession
dotnet run
```

## Quick Reference

### Basic Usage

```csharp
using LionFire.OpenCode.Serve;

var client = new OpenCodeClient();
var session = await client.CreateSessionAsync();
var response = await client.PromptAsync(session.Id, new SendMessageRequest
{
    Parts = new List<PartInput> { PartInput.TextInput("Hello!") }
});
```

### With IChatClient

```csharp
using LionFire.OpenCode.Serve.AgentFramework;
using Microsoft.Extensions.AI;

var chatClient = new OpenCodeChatClient(client);
var response = await chatClient.GetResponseAsync(new[]
{
    new ChatMessage(ChatRole.User, "Hello!")
});
```

### With Dependency Injection

```csharp
services.AddOpenCodeClient(options =>
{
    options.BaseUrl = "http://localhost:9123";
});
```

## Documentation

- [Getting Started](../docs/getting-started.md)
- [API Reference](../docs/api-reference.md)
- [Configuration Reference](../docs/configuration-reference.md)
- [Advanced Scenarios](../docs/advanced-scenarios.md)
