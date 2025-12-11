# LionFire.OpenCode.Serve.AgentFramework

Microsoft.Extensions.AI integration for LionFire.OpenCode.Serve SDK.

## Installation

```bash
dotnet add package LionFire.OpenCode.Serve.AgentFramework
```

## Quick Start

```csharp
using LionFire.OpenCode.Serve;
using LionFire.OpenCode.Serve.AgentFramework;
using Microsoft.Extensions.AI;

// Create OpenCode client and wrap as IChatClient
var openCodeClient = new OpenCodeClient();
IChatClient chatClient = openCodeClient.AsChatClient();

// Use standard Microsoft.Extensions.AI interface
var response = await chatClient.GetResponseAsync([
    new ChatMessage(ChatRole.User, "Hello!")
]);

Console.WriteLine(response.Messages[0].Text);
```

## Streaming Responses

```csharp
await foreach (var update in chatClient.GetStreamingResponseAsync([
    new ChatMessage(ChatRole.User, "Explain async/await")
]))
{
    Console.Write(update.Text);
}
```

## Dependency Injection

```csharp
// Register OpenCode client
services.AddOpenCodeClient(options =>
{
    options.BaseUrl = "http://localhost:9123";
});

// Register as IChatClient
services.AddOpenCodeChatClient(chatOptions =>
{
    chatOptions.ModelId = "opencode";
    chatOptions.Directory = "/my/project";
});

// Use in your services
public class ChatService(IChatClient chatClient)
{
    public async Task<string> Chat(string message)
    {
        var response = await chatClient.GetResponseAsync([
            new ChatMessage(ChatRole.User, message)
        ]);
        return response.Messages[0].Text ?? "";
    }
}
```

## Keyed Services

```csharp
// Register multiple chat clients
services.AddKeyedOpenCodeChatClient("coding", options =>
{
    options.Directory = "/code/project";
});

services.AddKeyedOpenCodeChatClient("docs", options =>
{
    options.Directory = "/docs/project";
});

// Resolve by key
var codingClient = serviceProvider.GetKeyedService<IChatClient>("coding");
```

## Session Management

```csharp
// Create chat client with specific session
var chatClient = openCodeClient.AsChatClient("existing-session-id");

// Or use a session scope
await using var scope = await openCodeClient.CreateSessionScopeAsync();
var chatClient = scope.AsChatClient(openCodeClient);
```

## Message Conversion

```csharp
using LionFire.OpenCode.Serve.AgentFramework;
using LionFire.OpenCode.Serve.Models;

// Convert OpenCode messages to ChatMessages
ChatMessage chat = MessageConverter.ToChatMessage(openCodeMessage);

// Convert ChatMessages to OpenCode message parts
IReadOnlyList<MessagePart> parts = MessageConverter.ToMessageParts(chatMessage);
```

## Accessing Underlying Client

```csharp
// Get the OpenCode client from IChatClient
var openCodeClient = chatClient.GetService(typeof(IOpenCodeClient)) as IOpenCodeClient;

// Or get the specific chat client implementation
var openCodeChatClient = chatClient.GetService(typeof(OpenCodeChatClient)) as OpenCodeChatClient;
```

## Requirements

- .NET 8.0 or later
- LionFire.OpenCode.Serve
- Microsoft.Extensions.AI.Abstractions

## License

MIT
