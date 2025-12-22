# Advanced Scenarios Guide

This guide covers advanced usage patterns for the LionFire.OpenCode.Serve SDK.

## Table of Contents

- [Configuration Validation](#configuration-validation)
- [Message Pagination](#message-pagination)
- [Session Filtering](#session-filtering)
- [Timeout Overrides](#timeout-overrides)
- [Streaming Progress Callbacks](#streaming-progress-callbacks)
- [Streaming Responses](#streaming-responses)
- [Event Subscriptions](#event-subscriptions)
- [Session Management](#session-management)
- [File Operations](#file-operations)
- [Permission Handling](#permission-handling)
- [PTY (Pseudo-Terminal) Operations](#pty-operations)
- [MCP Server Integration](#mcp-server-integration)
- [Provider Management](#provider-management)
- [Agent Framework Integration](#agent-framework-integration)

## Configuration Validation

The SDK includes comprehensive configuration validation that catches common errors at startup.

### Built-in Validation

```csharp
// These configurations will fail validation with clear error messages

// Invalid: URL with path component
var options = new OpenCodeClientOptions
{
    BaseUrl = "http://localhost:9123/api/v1"  // Error: should not contain a path
};

// Invalid: HTTP scheme with HTTPS port
var options = new OpenCodeClientOptions
{
    BaseUrl = "http://localhost:443"  // Error: did you mean HTTPS?
};

// Invalid: Excessive timeout (configuration error)
var options = new OpenCodeClientOptions
{
    DefaultTimeout = TimeSpan.FromHours(48)  // Error: exceeds 24 hour maximum
};

// Invalid: Directory is a URL
var options = new OpenCodeClientOptions
{
    Directory = "http://example.com/path"  // Error: should be file system path
};
```

### Validation Categories

| Category | What's Validated | Max/Min Values |
|----------|-----------------|----------------|
| BaseUrl | Valid HTTP/HTTPS URI, no path component | - |
| Timeouts | Positive, less than 24 hours | 0 < timeout <= 24h |
| Retry | Non-negative attempts, reasonable delay | 0-10 attempts, 0-300s delay |
| Circuit Breaker | Threshold and duration bounds | 1-100 threshold, 0-30min duration |
| Jitter | Non-negative, reasonable range | 0-30s |
| Directory | Not a URL, no null characters | - |

## Message Pagination

The SDK supports two pagination strategies for message history.

### Offset-Based Pagination

Traditional page-based navigation using skip/limit:

```csharp
// Get page 3 with 20 messages per page
var page3 = await client.ListMessagesAsync(sessionId, new MessageListOptions
{
    Skip = 40,  // Skip first 2 pages (20 * 2)
    Limit = 20
});

// Helper method for page navigation
async Task<List<MessageWithParts>> GetPageAsync(string sessionId, int pageNumber, int pageSize)
{
    return await client.ListMessagesAsync(sessionId, new MessageListOptions
    {
        Skip = (pageNumber - 1) * pageSize,
        Limit = pageSize
    });
}
```

### Cursor-Based Pagination

More stable pagination for real-time scenarios:

```csharp
// Get messages before a specific message (older messages)
var olderMessages = await client.ListMessagesAsync(sessionId, new MessageListOptions
{
    Before = "msg_abc123",  // Cursor: get messages before this one
    Limit = 20
});

// Get messages after a specific message (newer messages)
var newerMessages = await client.ListMessagesAsync(sessionId, new MessageListOptions
{
    After = "msg_xyz789",  // Cursor: get messages after this one
    Limit = 20
});

// Load more messages as user scrolls
string? lastMessageId = null;
while (true)
{
    var messages = await client.ListMessagesAsync(sessionId, new MessageListOptions
    {
        Before = lastMessageId,
        Limit = 20
    });

    if (messages.Count == 0) break;

    lastMessageId = messages.Last().Message?.Id;
    // Process messages...
}
```

### Filtering by Role

```csharp
// Get only user messages
var userMessages = await client.ListMessagesAsync(sessionId, new MessageListOptions
{
    Role = "user",
    Limit = 50
});

// Get only assistant messages
var assistantMessages = await client.ListMessagesAsync(sessionId, new MessageListOptions
{
    Role = "assistant",
    Limit = 50
});
```

## Session Filtering

Filter and search sessions with flexible options.

### Date Range Filtering

```csharp
// Sessions from the last 7 days
var recentSessions = await client.ListSessionsAsync(new SessionListOptions
{
    CreatedAfter = DateTimeOffset.UtcNow.AddDays(-7)
});

// Sessions from a specific date range
var sessions = await client.ListSessionsAsync(new SessionListOptions
{
    CreatedAfter = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
    CreatedBefore = new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero)
});

// Sessions updated in the last hour
var activeSessions = await client.ListSessionsAsync(new SessionListOptions
{
    UpdatedAfter = DateTimeOffset.UtcNow.AddHours(-1)
});
```

### Title Search

```csharp
// Search sessions by title
var matchingSessions = await client.ListSessionsAsync(new SessionListOptions
{
    TitleSearch = "bug fix",  // Case-insensitive substring match
    Limit = 10
});
```

### Status Filtering

```csharp
// Get busy sessions only
var busySessions = await client.ListSessionsAsync(new SessionListOptions
{
    Status = "busy"
});

// Get idle sessions
var idleSessions = await client.ListSessionsAsync(new SessionListOptions
{
    Status = "idle"
});
```

### Relationship Filtering

```csharp
// Get only root sessions (no parent)
var rootSessions = await client.ListSessionsAsync(new SessionListOptions
{
    HasParent = false
});

// Get only forked sessions
var forkedSessions = await client.ListSessionsAsync(new SessionListOptions
{
    HasParent = true
});

// Get children of a specific session
var children = await client.ListSessionsAsync(new SessionListOptions
{
    ParentId = "ses_parent123"
});

// Get only shared sessions
var sharedSessions = await client.ListSessionsAsync(new SessionListOptions
{
    IsShared = true
});
```

### Sorting

```csharp
// Most recently updated first (default)
var sessions = await client.ListSessionsAsync(new SessionListOptions
{
    SortOrder = SessionSortOrder.UpdatedDescending
});

// Oldest first
var oldestFirst = await client.ListSessionsAsync(new SessionListOptions
{
    SortOrder = SessionSortOrder.CreatedAscending
});

// Alphabetical by title
var alphabetical = await client.ListSessionsAsync(new SessionListOptions
{
    SortOrder = SessionSortOrder.TitleAscending
});
```

### Combined Filters

```csharp
// Complex query: recent busy sessions with "refactor" in title
var sessions = await client.ListSessionsAsync(new SessionListOptions
{
    TitleSearch = "refactor",
    Status = "busy",
    CreatedAfter = DateTimeOffset.UtcNow.AddDays(-30),
    SortOrder = SessionSortOrder.UpdatedDescending,
    Limit = 10
});
```

## Timeout Overrides

Override global timeouts for specific operations.

### Per-Operation Timeout

```csharp
// Use custom timeout for a complex prompt
try
{
    var response = await client.PromptAsync(
        sessionId,
        new SendMessageRequest
        {
            Parts = { PartInput.TextInput("Analyze this entire codebase and create a comprehensive report...") }
        },
        timeout: TimeSpan.FromMinutes(15)  // Override for this specific call
    );
}
catch (TimeoutException ex)
{
    Console.WriteLine($"Operation timed out: {ex.Message}");
}
```

### Different Timeouts for Different Operations

```csharp
// Quick operations: use short timeout
var sessions = await client.ListSessionsAsync();  // Uses default 30s timeout

// Complex AI operations: use longer timeout
var response = await client.PromptAsync(
    sessionId,
    complexRequest,
    timeout: TimeSpan.FromMinutes(10)
);
```

## Streaming Progress Callbacks

Monitor streaming operations with progress callbacks for UI scenarios.

### Basic Progress Reporting

```csharp
// Create progress handler
var progress = new Progress<StreamingProgress>(p =>
{
    switch (p.Status)
    {
        case StreamingStatus.Starting:
            Console.WriteLine("Connecting...");
            break;
        case StreamingStatus.Connected:
            Console.WriteLine("Connected, waiting for data...");
            break;
        case StreamingStatus.Receiving:
            Console.WriteLine($"Received {p.ChunkCount} chunks, {p.BytesReceived} bytes");
            break;
        case StreamingStatus.Completed:
            Console.WriteLine($"Completed: {p.ChunkCount} chunks in {p.Elapsed.TotalSeconds:F1}s");
            break;
        case StreamingStatus.Error:
            Console.WriteLine($"Error: {p.Error?.Message}");
            break;
        case StreamingStatus.Cancelled:
            Console.WriteLine("Cancelled by user");
            break;
    }
});

// Subscribe with progress
await foreach (var evt in client.SubscribeToEventsAsync(progress))
{
    // Process events
    Console.WriteLine($"Event: {evt.Type}");
}
```

### UI Integration with Progress

```csharp
// Blazor/WPF example: Update UI with streaming status
public class StreamingViewModel : INotifyPropertyChanged
{
    private StreamingStatus _status;
    private int _chunkCount;
    private long _bytesReceived;
    private TimeSpan _elapsed;

    public async Task StartStreamingAsync(string sessionId)
    {
        var progress = new Progress<StreamingProgress>(UpdateProgress);

        await foreach (var evt in client.SubscribeToEventsAsync(progress))
        {
            ProcessEvent(evt);
        }
    }

    private void UpdateProgress(StreamingProgress p)
    {
        Status = p.Status;
        ChunkCount = p.ChunkCount;
        BytesReceived = p.BytesReceived;
        Elapsed = p.Elapsed;
        OnPropertyChanged(nameof(StatusText));
    }

    public string StatusText => Status switch
    {
        StreamingStatus.Starting => "Connecting...",
        StreamingStatus.Connected => "Connected",
        StreamingStatus.Receiving => $"Receiving: {ChunkCount} chunks, {BytesReceived:N0} bytes",
        StreamingStatus.Completed => $"Complete ({Elapsed.TotalSeconds:F1}s)",
        StreamingStatus.Error => "Error occurred",
        StreamingStatus.Cancelled => "Cancelled",
        _ => "Unknown"
    };
}
```

### Progress with Timeout

```csharp
// Stream with both progress and timeout
var cts = new CancellationTokenSource();
var progress = new Progress<StreamingProgress>(p =>
{
    if (p.Status == StreamingStatus.Error)
    {
        // Log error for diagnostics
        Logger.LogError(p.Error, "Streaming failed after {ChunkCount} chunks", p.ChunkCount);
    }
});

try
{
    await foreach (var evt in client.SubscribeToEventsAsync(
        progress,
        timeout: TimeSpan.FromMinutes(30),
        cancellationToken: cts.Token))
    {
        // Process events
    }
}
catch (OperationCanceledException)
{
    // Handle timeout or cancellation
}
```

## Streaming Responses

### Real-time Event Subscription

For real-time updates during long-running operations, subscribe to server events:

```csharp
using LionFire.OpenCode.Serve;

var client = new OpenCodeClient();

// Subscribe to events for a specific session
await foreach (var evt in client.SubscribeToEventsAsync(directory: "/path/to/project"))
{
    Console.WriteLine($"Event type: {evt.Type}");

    // Handle different event types
    switch (evt)
    {
        case var e when e.Type == "message":
            Console.WriteLine("New message received");
            break;
        case var e when e.Type == "session":
            Console.WriteLine("Session state changed");
            break;
    }
}
```

### Non-Blocking Prompts

Send a message without waiting for the response:

```csharp
// Send prompt without blocking
await client.PromptAsyncNonBlocking(sessionId, new SendMessageRequest
{
    Parts = new List<PartInput>
    {
        PartInput.TextInput("Write a comprehensive test suite for this project")
    }
});

// Subscribe to events to get updates
await foreach (var evt in client.SubscribeToEventsAsync())
{
    // Handle streaming updates here
    if (evt.Type == "complete")
        break;
}
```

### IChatClient Streaming

Using Microsoft.Extensions.AI abstractions:

```csharp
using LionFire.OpenCode.Serve.AgentFramework;
using Microsoft.Extensions.AI;

var chatClient = new OpenCodeChatClient(client);

await foreach (var update in chatClient.GetStreamingResponseAsync(new[]
{
    new ChatMessage(ChatRole.User, "Explain this codebase")
}))
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

## Event Subscriptions

### Global Events

Subscribe to global server events:

```csharp
await foreach (var globalEvent in client.SubscribeToGlobalEventsAsync())
{
    Console.WriteLine($"Global event: {globalEvent.Type}");
    // Handle session creation, deletion, etc.
}
```

### Session-Specific Events

For directory-scoped events:

```csharp
await foreach (var evt in client.SubscribeToEventsAsync(directory: "/my/project"))
{
    // Events specific to this directory context
}
```

## Session Management

### Forking Sessions

Create a branch from a specific point in the conversation:

```csharp
// Fork from a specific message
var forkedSession = await client.ForkSessionAsync(originalSessionId, new ForkSessionRequest
{
    MessageId = "msg_abc123" // Fork point
});

Console.WriteLine($"Forked session: {forkedSession.Id}");
```

### Session Hierarchies

Get child sessions (forks):

```csharp
var children = await client.GetSessionChildrenAsync(parentSessionId);
foreach (var child in children)
{
    Console.WriteLine($"Child session: {child.Id} - {child.Title}");
}
```

### Reverting Sessions

Revert to a previous state:

```csharp
// Revert to a specific message
await client.RevertSessionAsync(sessionId, new RevertSessionRequest
{
    MessageId = "msg_xyz789"
});

// Undo the revert
await client.UnrevertSessionAsync(sessionId);
```

### Sharing Sessions

Share a session publicly:

```csharp
var sharedSession = await client.ShareSessionAsync(sessionId);
Console.WriteLine($"Share URL: {sharedSession.Share?.Url}");

// Later, unshare
await client.UnshareSessionAsync(sessionId);
```

### Session Summarization

Trigger session summarization:

```csharp
await client.SummarizeSessionAsync(sessionId, new SummarizeSessionRequest
{
    MessageId = "msg_latest" // Summarize up to this point (optional)
});
```

## File Operations

### Listing Files

```csharp
var files = await client.ListFilesAsync(path: "src", directory: "/my/project");
foreach (var file in files)
{
    Console.WriteLine($"{file.Name} ({file.Type})");
}
```

### Reading File Content

```csharp
var content = await client.ReadFileAsync("src/Program.cs", directory: "/my/project");
Console.WriteLine(content.Content);
```

### File Status (Git)

```csharp
var status = await client.GetFileStatusAsync(directory: "/my/project");
// status contains git status information
```

### Session Diffs

Get file changes in a session:

```csharp
var diffs = await client.GetSessionDiffAsync(sessionId);
foreach (var diff in diffs)
{
    Console.WriteLine($"File: {diff.Path}");
    Console.WriteLine($"Additions: {diff.Additions}, Deletions: {diff.Deletions}");
}
```

## Permission Handling

OpenCode requests permission for certain operations:

```csharp
// Subscribe to events to catch permission requests
await foreach (var evt in client.SubscribeToEventsAsync())
{
    if (evt.Type == "permission")
    {
        // Parse permission details from event
        var permissionId = "perm_abc"; // From event data

        // Respond to permission request
        await client.RespondToPermissionAsync(sessionId, permissionId, new PermissionResponse
        {
            Allow = true,
            Scope = PermissionScope.Once // Or Always, Never
        });
    }
}
```

### Permission Scopes

- `PermissionScope.Once` - Allow this single operation
- `PermissionScope.Always` - Allow all similar operations
- `PermissionScope.Never` - Reject and don't ask again

## PTY Operations

Create and manage pseudo-terminal sessions:

### Create a PTY

```csharp
var pty = await client.CreatePtyAsync(new CreatePtyRequest
{
    Command = "bash",
    Cols = 80,
    Rows = 24
});

Console.WriteLine($"PTY created: {pty.Id}");
```

### List PTY Sessions

```csharp
var ptys = await client.ListPtysAsync();
foreach (var pty in ptys)
{
    Console.WriteLine($"PTY: {pty.Id}");
}
```

### Get PTY Details

```csharp
var pty = await client.GetPtyAsync(ptyId);
Console.WriteLine($"PTY state: {pty.Status}");
```

### Update PTY Size

```csharp
var updated = await client.UpdatePtyAsync(ptyId, new UpdatePtyRequest
{
    Cols = 120,
    Rows = 40
});
```

### Delete PTY

```csharp
await client.DeletePtyAsync(ptyId);
```

## MCP Server Integration

Manage Model Context Protocol servers:

### Check MCP Status

```csharp
var mcpStatus = await client.GetMcpStatusAsync();
foreach (var server in mcpStatus)
{
    Console.WriteLine($"MCP Server: {server.Name} - {server.Status}");
}
```

### Add MCP Server

```csharp
await client.AddMcpServerAsync(new AddMcpServerRequest
{
    Name = "my-mcp-server",
    // Additional configuration
});
```

### Connect/Disconnect MCP Servers

```csharp
await client.ConnectMcpAsync("my-mcp-server");
// Later...
await client.DisconnectMcpAsync("my-mcp-server");
```

### MCP Authentication

```csharp
await client.StartMcpAuthAsync("my-mcp-server");
await client.AuthenticateMcpAsync("my-mcp-server");
```

## Provider Management

### List Available Providers

```csharp
var providers = await client.ListProvidersAsync();
foreach (var provider in providers)
{
    Console.WriteLine($"Provider: {provider.Id} - {provider.Name}");
}
```

### Check Provider Authentication

```csharp
var authStatus = await client.GetProviderAuthAsync();
foreach (var auth in authStatus)
{
    Console.WriteLine($"Provider {auth.ProviderId}: {(auth.Authenticated ? "Authenticated" : "Not authenticated")}");
}
```

## Agent Framework Integration

### Using IChatClient

```csharp
using LionFire.OpenCode.Serve.AgentFramework;
using Microsoft.Extensions.AI;

// Direct instantiation
var chatClient = new OpenCodeChatClient(client, new OpenCodeChatClientOptions
{
    ModelId = "opencode",
    Directory = "/my/project",
    Model = new ModelReference
    {
        ProviderId = "anthropic",
        ModelId = "claude-3-5-sonnet-20241022"
    }
});

// Send messages
var response = await chatClient.GetResponseAsync(new[]
{
    new ChatMessage(ChatRole.System, "You are a helpful coding assistant."),
    new ChatMessage(ChatRole.User, "Review this code for bugs.")
});
```

### With Dependency Injection

```csharp
// Registration
services.AddOpenCodeClient(options => options.BaseUrl = "http://localhost:9123");
services.AddOpenCodeChatClient(chatOptions =>
{
    chatOptions.ModelId = "opencode";
    chatOptions.Directory = "/my/project";
});

// Usage
public class CodeReviewService
{
    private readonly IChatClient _chatClient;

    public CodeReviewService(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<string> ReviewCodeAsync(string code)
    {
        var response = await _chatClient.GetResponseAsync(new[]
        {
            new ChatMessage(ChatRole.User, $"Review this code:\n```\n{code}\n```")
        });
        return response.Messages[0].Text ?? "";
    }
}
```

### Keyed Chat Clients

Register multiple chat clients with different configurations:

```csharp
services.AddKeyedOpenCodeChatClient("reviewer", options =>
{
    options.Directory = "/project1";
});

services.AddKeyedOpenCodeChatClient("generator", options =>
{
    options.Directory = "/project2";
});

// Usage
public class MultiProjectService
{
    private readonly IChatClient _reviewer;
    private readonly IChatClient _generator;

    public MultiProjectService(
        [FromKeyedServices("reviewer")] IChatClient reviewer,
        [FromKeyedServices("generator")] IChatClient generator)
    {
        _reviewer = reviewer;
        _generator = generator;
    }
}
```

## Find Operations

### Search Text in Files

```csharp
var results = await client.FindTextAsync("TODO", directory: "/my/project");
foreach (var match in results)
{
    Console.WriteLine(match);
}
```

### Search for Files

```csharp
var files = await client.FindFilesAsync("*.cs", directory: "/my/project");
foreach (var file in files)
{
    Console.WriteLine(file);
}
```

### Search Symbols

```csharp
var symbols = await client.FindSymbolsAsync("MyClass", directory: "/my/project");
foreach (var symbol in symbols)
{
    Console.WriteLine($"{symbol.Name} at {symbol.Location}");
}
```

## Configuration Management

### Get Configuration

```csharp
var config = await client.GetConfigAsync();
foreach (var (key, value) in config)
{
    Console.WriteLine($"{key}: {value}");
}
```

### Update Configuration

```csharp
await client.UpdateConfigAsync(new Dictionary<string, object>
{
    ["theme"] = "dark",
    ["autoSave"] = true
});
```

## Advanced Error Handling

See [Error Handling Guide](./error-handling.md) for comprehensive error handling patterns.

## Performance Considerations

See [Performance Tuning Guide](./performance-tuning.md) for optimization strategies.

## Related Documentation

- [Getting Started](./getting-started.md)
- [Configuration Reference](./configuration-reference.md)
- [API Reference](./api-reference.md)
- [Enterprise Deployment](./enterprise-deployment.md)
