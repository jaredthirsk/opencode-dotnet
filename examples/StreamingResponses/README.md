# Streaming Responses Example

This example demonstrates how to handle streaming responses from the OpenCode server using Server-Sent Events (SSE).

## Features Demonstrated

- Non-blocking prompt submission
- SSE event subscription
- Handling different event types
- Real-time progress indication
- Fetching final response

## Prerequisites

1. .NET 8.0 SDK or later
2. OpenCode server running (`opencode serve`)

## Running the Example

```bash
dotnet run --project examples/StreamingResponses
```

## Expected Output

```
LionFire.OpenCode.Serve - Streaming Responses Example
======================================================

✓ Connected to OpenCode server
✓ Created session: ses_abc123

Sending prompt (non-blocking)...

Streaming response:
> .......
  [Message event received]
  [Complete]

Fetching final response...

Final response:
1... 2... 3... 4... 5...

✓ Session cleaned up

Done!
```

## How It Works

1. **PromptAsyncNonBlocking**: Sends the prompt and returns immediately without waiting
2. **SubscribeToEventsAsync**: Opens an SSE connection to receive real-time updates
3. **Event Types**:
   - `chunk`: Partial content update
   - `message`: Message state changed
   - `complete`: Response fully generated
   - `error`: An error occurred

## Use Cases

- Progress indicators for long-running tasks
- Real-time chat interfaces
- Streaming code generation
- Large document processing

## Next Steps

- [Agent Framework](../AgentFramework/) - Use Microsoft.Extensions.AI streaming
- [ASP.NET Core](../AspNetCoreIntegration/) - Stream to web clients
