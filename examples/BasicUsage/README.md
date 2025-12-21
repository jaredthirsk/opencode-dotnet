# Basic Usage Example

> **Status: Working** - This example runs successfully against a live `opencode serve` instance.

This example demonstrates the core functionality of the LionFire.OpenCode.Serve SDK.

## Sample Output

```
LionFire.OpenCode.Serve SDK - Basic Usage Example
==================================================

1. Checking server health...
   Server is responding

2. Listing available providers and models...
   Found 72 providers
   Providers with FREE models: zai-coding-plan, ollama-cloud, nvidia, ...
   Run with --list-models --free to see all free models

3. Creating session...
   Created session: ses_4c14e80e6ffeIutDoEPagoxBUu
   Session directory: /src/opencode-dotnet
   Using default model (specify -p and -m to use a specific model)
   Session cleaned up.

4. Listing sessions...
   Found 4 sessions
   - ses_4c151ebd5ffeMHidga5he0Speo (created: 12/21/2025 02:12)
   ... and more

All examples completed successfully!
```

## Prerequisites

1. Install OpenCode CLI: https://github.com/opencode-ai/opencode
2. Start the OpenCode server:
   ```bash
   opencode serve --port 9123
   ```

## Running the Example

```bash
dotnet run
```

## What This Example Shows

1. **Health Check** - Verify the server is running
2. **Configuration** - Get server configuration and version
3. **Tools** - List available tools
4. **Session Management** - Create sessions with automatic cleanup
5. **Message Sending** - Send messages and receive responses
6. **Message History** - Retrieve conversation history
7. **Streaming** - Stream responses using `IAsyncEnumerable<T>`

## Code Highlights

### Creating a Client

```csharp
// Default: localhost:9123
await using var client = new OpenCodeClient();

// Custom URL
await using var client = new OpenCodeClient("http://localhost:8080");

// With options
await using var client = new OpenCodeClient(new OpenCodeClientOptions
{
    BaseUrl = "http://localhost:9123",
    MessageTimeout = TimeSpan.FromMinutes(10)
});
```

### Session Scope Pattern

```csharp
// Session is automatically deleted when scope is disposed
await using (var scope = await client.CreateSessionScopeAsync())
{
    var response = await client.SendMessageAsync(scope.SessionId, "Hello!");
}
// Session deleted here
```

### Streaming Responses

```csharp
await foreach (var update in client.SendMessageStreamingAsync(sessionId, "Hello"))
{
    if (update.Delta is not null)
    {
        Console.Write(update.Delta);
    }
}
```

### Error Handling

```csharp
try
{
    var session = await client.GetSessionAsync("invalid-id");
}
catch (OpenCodeNotFoundException ex)
{
    Console.WriteLine($"Session not found: {ex.ResourceId}");
}
catch (OpenCodeConnectionException ex)
{
    Console.WriteLine($"Server not reachable: {ex.BaseUrl}");
}
```
