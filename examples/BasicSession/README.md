# Basic Session Example

This example demonstrates the fundamental operations of the LionFire.OpenCode.Serve SDK:

- Creating a client
- Creating a session
- Sending a message
- Receiving and displaying the response
- Cleaning up the session

## Prerequisites

1. Install [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or later
2. Install and run [OpenCode](https://github.com/opencode-ai/opencode):
   ```bash
   npm install -g opencode
   opencode serve
   ```

## Running the Example

```bash
# From the examples/BasicSession directory
dotnet run

# Or from the repository root
dotnet run --project examples/BasicSession
```

## Expected Output

```
LionFire.OpenCode.Serve - Basic Session Example
================================================

✓ Connected to OpenCode server
✓ Created session: ses_abc123

> Sending message: What is the capital of France?

< Response:
The capital of France is Paris.

Message ID: msg_xyz789
Tokens used: Input=15, Output=8
Cost: $0.000120

✓ Session cleaned up

Done!
```

## Key Concepts

1. **OpenCodeClient**: The main entry point for all SDK operations
2. **Session**: A conversation context with history
3. **SendMessageRequest**: Contains the message parts to send
4. **MessageWithParts**: The response containing message metadata and content parts
5. **Cleanup**: Always delete sessions when done to free resources

## Next Steps

- [Streaming Example](../StreamingResponses/) - Handle real-time streaming responses
- [DI Example](../AspNetCoreIntegration/) - Use dependency injection in ASP.NET Core
