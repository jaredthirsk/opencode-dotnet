# Agent Framework Example

This example demonstrates using the Microsoft.Extensions.AI abstractions with the OpenCode SDK.

## Features Demonstrated

- Creating an IChatClient from OpenCodeClient
- Single-turn conversations
- Multi-turn conversations with history
- Streaming responses with IAsyncEnumerable
- Accessing underlying services via GetService

## Prerequisites

1. .NET 8.0 SDK or later
2. OpenCode server running (`opencode serve`)

## Running the Example

```bash
dotnet run --project examples/AgentFramework
```

## Expected Output

```
LionFire.OpenCode.Serve - Agent Framework Example
==================================================

✓ Connected to OpenCode server
✓ Created IChatClient wrapper

Example 1: Single-turn conversation
------------------------------------
Response: C# is similar to Java in syntax and design...

Example 2: Multi-turn conversation
-----------------------------------
Q: What is .NET?
A: .NET is a cross-platform development framework...

Q: What languages can I use with it?
A: You can use C#, F#, and Visual Basic...

Example 3: Streaming response
------------------------------
Response: 1... 2... 3...

Example 4: Accessing underlying services
-----------------------------------------
Found 1 project(s) via underlying client

All examples completed successfully!

Done!
```

## Key Concepts

### IChatClient Interface

The `IChatClient` interface from Microsoft.Extensions.AI provides:
- `GetResponseAsync`: Send messages and get a complete response
- `GetStreamingResponseAsync`: Stream response updates as they arrive
- `GetService`: Access underlying services

### OpenCodeChatClient

The `OpenCodeChatClient` class implements `IChatClient`:
- Manages session lifecycle automatically
- Converts between ChatMessage and OpenCode message formats
- Supports streaming via event subscription

### ChatMessage Roles

- `ChatRole.System`: System instructions (optional)
- `ChatRole.User`: User messages
- `ChatRole.Assistant`: AI responses
- `ChatRole.Tool`: Tool/function results

## Benefits of IChatClient

1. **Abstraction**: Switch between AI providers without code changes
2. **Compatibility**: Works with Semantic Kernel and other frameworks
3. **Standardization**: Common interface across the .NET ecosystem
4. **Streaming**: Built-in async enumerable support

## Next Steps

- [ASP.NET Core](../AspNetCoreIntegration/) - Use with dependency injection
- [Error Handling](../ErrorHandling/) - Handle errors gracefully
