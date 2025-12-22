# API Reference

Complete API reference for the LionFire.OpenCode.Serve SDK.

## Namespaces

| Namespace | Description |
|-----------|-------------|
| `LionFire.OpenCode.Serve` | Core client and configuration |
| `LionFire.OpenCode.Serve.Models` | Request/response models |
| `LionFire.OpenCode.Serve.Exceptions` | Exception types |
| `LionFire.OpenCode.Serve.Extensions` | Extension methods |
| `LionFire.OpenCode.Serve.AgentFramework` | Microsoft.Extensions.AI integration |

## IOpenCodeClient Interface

The main client interface for interacting with the OpenCode server.

### Session Operations

| Method | Description |
|--------|-------------|
| `CreateSessionAsync(CreateSessionRequest?, string?, CancellationToken)` | Create a new session |
| `GetSessionAsync(string, string?, CancellationToken)` | Get session by ID |
| `ListSessionsAsync(string?, CancellationToken)` | List all sessions |
| `DeleteSessionAsync(string, string?, CancellationToken)` | Delete a session |
| `UpdateSessionAsync(string, UpdateSessionRequest, string?, CancellationToken)` | Update session properties |
| `GetSessionStatusAsync(string?, CancellationToken)` | Get current session status |
| `AbortSessionAsync(string, string?, CancellationToken)` | Abort running session |
| `ForkSessionAsync(string, ForkSessionRequest?, string?, CancellationToken)` | Fork session at message point |
| `ShareSessionAsync(string, string?, CancellationToken)` | Share session publicly |
| `UnshareSessionAsync(string, string?, CancellationToken)` | Remove session sharing |
| `InitializeSessionAsync(string, InitSessionRequest, string?, CancellationToken)` | Initialize session with model |
| `RevertSessionAsync(string, RevertSessionRequest, string?, CancellationToken)` | Revert to previous state |
| `UnrevertSessionAsync(string, string?, CancellationToken)` | Undo session revert |
| `GetSessionChildrenAsync(string, string?, CancellationToken)` | Get forked sessions |
| `ExecuteSessionCommandAsync(string, ExecuteCommandRequest, string?, CancellationToken)` | Execute session command |
| `ExecuteSessionShellAsync(string, ExecuteShellRequest, string?, CancellationToken)` | Execute shell command |
| `SummarizeSessionAsync(string, SummarizeSessionRequest?, string?, CancellationToken)` | Summarize session |
| `GetSessionDiffAsync(string, string?, string?, CancellationToken)` | Get session file diffs |
| `GetSessionTodosAsync(string, string?, CancellationToken)` | Get session todos |

### Message Operations

| Method | Description |
|--------|-------------|
| `PromptAsync(string, SendMessageRequest, string?, CancellationToken)` | Send prompt and get response |
| `PromptAsyncNonBlocking(string, SendMessageRequest, string?, CancellationToken)` | Send prompt asynchronously |
| `ListMessagesAsync(string, int?, string?, CancellationToken)` | List messages in session |
| `GetMessageAsync(string, string, string?, CancellationToken)` | Get specific message |

### Permission Operations

| Method | Description |
|--------|-------------|
| `RespondToPermissionAsync(string, string, PermissionResponse, string?, CancellationToken)` | Respond to permission request |

### File Operations

| Method | Description |
|--------|-------------|
| `ListFilesAsync(string, string?, CancellationToken)` | List files in directory |
| `ReadFileAsync(string, string?, CancellationToken)` | Read file content |
| `GetFileStatusAsync(string?, CancellationToken)` | Get file git status |

### Find Operations

| Method | Description |
|--------|-------------|
| `FindTextAsync(string, string?, CancellationToken)` | Search text in files |
| `FindFilesAsync(string, string?, string?, CancellationToken)` | Search for files |
| `FindSymbolsAsync(string, string?, CancellationToken)` | Search for symbols |

### PTY Operations

| Method | Description |
|--------|-------------|
| `CreatePtyAsync(CreatePtyRequest, string?, CancellationToken)` | Create PTY session |
| `ListPtysAsync(string?, CancellationToken)` | List PTY sessions |
| `GetPtyAsync(string, string?, CancellationToken)` | Get PTY by ID |
| `UpdatePtyAsync(string, UpdatePtyRequest, string?, CancellationToken)` | Update PTY settings |
| `DeletePtyAsync(string, string?, CancellationToken)` | Delete PTY session |
| `ConnectToPtyAsync(string, string?, CancellationToken)` | Connect to PTY (WebSocket) |

### MCP Operations

| Method | Description |
|--------|-------------|
| `GetMcpStatusAsync(string?, CancellationToken)` | Get MCP server status |
| `AddMcpServerAsync(AddMcpServerRequest, string?, CancellationToken)` | Add MCP server |
| `ConnectMcpAsync(string, string?, CancellationToken)` | Connect to MCP server |
| `DisconnectMcpAsync(string, string?, CancellationToken)` | Disconnect from MCP server |
| `StartMcpAuthAsync(string, string?, CancellationToken)` | Start MCP authentication |
| `AuthenticateMcpAsync(string, string?, CancellationToken)` | Complete MCP authentication |
| `RemoveMcpAuthAsync(string, string?, CancellationToken)` | Remove MCP authentication |
| `HandleMcpAuthCallbackAsync(string, string?, CancellationToken)` | Handle auth callback |

### Project Operations

| Method | Description |
|--------|-------------|
| `ListProjectsAsync(string?, CancellationToken)` | List all projects |
| `GetCurrentProjectAsync(string?, CancellationToken)` | Get current project |

### Provider Operations

| Method | Description |
|--------|-------------|
| `ListProvidersAsync(string?, CancellationToken)` | List available providers |
| `GetProviderAuthAsync(string?, CancellationToken)` | Get provider auth status |

### Configuration Operations

| Method | Description |
|--------|-------------|
| `GetConfigAsync(string?, CancellationToken)` | Get server configuration |
| `UpdateConfigAsync(Dictionary<string, object>, string?, CancellationToken)` | Update configuration |
| `GetConfigProvidersAsync(string?, CancellationToken)` | Get configured providers |

### Other Operations

| Method | Description |
|--------|-------------|
| `ListAgentsAsync(string?, CancellationToken)` | List available agents |
| `ListCommandsAsync(string?, CancellationToken)` | List available commands |
| `GetLspStatusAsync(string?, CancellationToken)` | Get LSP status |
| `GetFormatterStatusAsync(string?, CancellationToken)` | Get formatter status |
| `GetPathAsync(string?, CancellationToken)` | Get path information |
| `GetVcsInfoAsync(string?, CancellationToken)` | Get VCS information |
| `SubscribeToEventsAsync(string?, CancellationToken)` | Subscribe to SSE events |
| `SubscribeToGlobalEventsAsync(CancellationToken)` | Subscribe to global events |
| `DisposeInstanceAsync(string?, CancellationToken)` | Dispose server instance |

## OpenCodeClientOptions

Configuration options for the OpenCode client.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `BaseUrl` | `string` | `http://localhost:9123` | Server URL |
| `Directory` | `string?` | `null` | Default working directory |
| `DefaultTimeout` | `TimeSpan` | 30 seconds | Quick operation timeout |
| `MessageTimeout` | `TimeSpan` | 5 minutes | AI operation timeout |
| `EnableRetry` | `bool` | `true` | Enable automatic retry |
| `MaxRetryAttempts` | `int` | `3` | Maximum retry attempts |
| `RetryDelaySeconds` | `int` | `2` | Base retry delay |
| `EnableTelemetry` | `bool` | `true` | Enable OpenTelemetry |
| `ValidateOnStart` | `bool` | `true` | Validate on construction |

## Models

### Session

```csharp
public record Session
{
    public required string Id { get; init; }
    public required string ProjectId { get; init; }
    public required string Directory { get; init; }
    public string? ParentId { get; init; }
    public SessionSummary? Summary { get; init; }
    public SessionShare? Share { get; init; }
    public required string Title { get; init; }
    public required string Version { get; init; }
    public required SessionTime Time { get; init; }
    public SessionRevert? Revert { get; init; }
}
```

### Message

```csharp
public record Message
{
    public string Id { get; init; }
    public string SessionId { get; init; }
    public string Role { get; init; }  // "user" or "assistant"
    public MessageTime? Time { get; init; }
    public MessageError? Error { get; init; }
    public string? ParentId { get; init; }
    public string? ModelId { get; init; }
    public string? ProviderId { get; init; }
    public TokenUsage? Tokens { get; init; }
    public double? Cost { get; init; }
}
```

### MessageWithParts

```csharp
public record MessageWithParts
{
    public Message? Message { get; init; }
    public List<Part>? Parts { get; init; }
}
```

### Part

```csharp
public record Part
{
    public string Id { get; init; }
    public string Type { get; init; }
    public string? Text { get; init; }
    public string? CallId { get; init; }
    public string? Status { get; init; }

    public bool IsTextPart { get; }
    public bool IsToolCall { get; }
    public bool IsToolCompleted { get; }
}
```

### SendMessageRequest

```csharp
public record SendMessageRequest
{
    public List<PartInput> Parts { get; init; }
    public string? Agent { get; init; }
    public ModelReference? Model { get; init; }
}
```

### PartInput

```csharp
public record PartInput
{
    public string Type { get; init; }
    public string? Text { get; init; }

    public static PartInput TextInput(string text);
}
```

### ModelReference

```csharp
public record ModelReference
{
    public string ProviderId { get; init; }
    public string ModelId { get; init; }
}
```

## Exceptions

### Exception Hierarchy

```
OpenCodeException
├── OpenCodeApiException
│   ├── OpenCodeNotFoundException
│   └── OpenCodeRateLimitException
├── OpenCodeConnectionException
├── OpenCodeTimeoutException
└── OpenCodeSerializationException
```

### OpenCodeException

Base exception with troubleshooting hints.

```csharp
public class OpenCodeException : Exception
{
    public string? TroubleshootingHint { get; }
}
```

### OpenCodeApiException

HTTP-level errors.

```csharp
public class OpenCodeApiException : OpenCodeException
{
    public HttpStatusCode StatusCode { get; }
}
```

### OpenCodeConnectionException

Network connectivity errors.

```csharp
public class OpenCodeConnectionException : OpenCodeException
{
    public string? ServerUrl { get; }
}
```

## Agent Framework

### OpenCodeChatClient

`IChatClient` implementation for Microsoft.Extensions.AI.

```csharp
public class OpenCodeChatClient : IChatClient
{
    public OpenCodeChatClient(IOpenCodeClient client, OpenCodeChatClientOptions? options = null);

    public string? SessionId { get; set; }
    public ChatClientMetadata Metadata { get; }

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default);

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default);

    public object? GetService(Type serviceType, object? serviceKey = null);
}
```

### OpenCodeChatClientOptions

```csharp
public class OpenCodeChatClientOptions
{
    public string? Directory { get; set; }
    public string? ModelId { get; set; } = "opencode";
    public string? BaseUrl { get; set; }
    public bool CreateSessionPerConversation { get; set; }
    public ModelReference? Model { get; set; }
}
```

### MessageConverter

Utility for converting between message formats.

```csharp
public static class MessageConverter
{
    public static ChatMessage ToChatMessage(MessageWithParts messageWithParts);
    public static IReadOnlyList<PartInput> ToPartInputs(ChatMessage message);
    public static ChatResponse ToChatResponse(MessageWithParts messageWithParts);
    public static ChatRole ToChatRole(string role);
    public static string ToMessageRole(ChatRole role);
    public static AIContent? ToAIContent(Part part);
    public static PartInput? ToPartInput(AIContent content);
    public static IReadOnlyList<ChatMessage> ToChatMessages(IEnumerable<MessageWithParts> messages);
}
```

## Extension Methods

### ServiceCollectionExtensions

```csharp
public static class ServiceCollectionExtensions
{
    // Register IOpenCodeClient
    public static IServiceCollection AddOpenCodeClient(
        this IServiceCollection services,
        Action<OpenCodeClientOptions>? configureOptions = null);

    // Register IChatClient
    public static IServiceCollection AddOpenCodeChatClient(
        this IServiceCollection services,
        Action<OpenCodeChatClientOptions>? configureOptions = null);

    // Register keyed IChatClient
    public static IServiceCollection AddKeyedOpenCodeChatClient(
        this IServiceCollection services,
        string key,
        Action<OpenCodeChatClientOptions>? configureOptions = null);
}
```

## See Also

- [Getting Started](./getting-started.md)
- [Configuration Reference](./configuration-reference.md)
- [Advanced Scenarios](./advanced-scenarios.md)
- Source code: [`/src/LionFire.OpenCode.Serve/IOpenCodeClient.cs`](../src/LionFire.OpenCode.Serve/IOpenCodeClient.cs)
