# Getting Started with LionFire.OpenCode.Serve

This guide walks you through setting up and using the LionFire.OpenCode.Serve SDK to interact with an OpenCode server.

## Prerequisites

Before you begin, ensure you have:

1. **.NET 8.0 SDK or later** installed
2. **OpenCode** installed and running locally (`opencode serve`)

### Installing OpenCode

If you haven't installed OpenCode yet:

```bash
# Install OpenCode (check https://github.com/opencode-ai/opencode for latest instructions)
npm install -g opencode
# Or via other package managers

# Start the server
opencode serve
```

The server runs on `http://localhost:9123` by default.

## Installation

### NuGet Package

Install the main SDK package:

```bash
dotnet add package LionFire.OpenCode.Serve
```

For Microsoft.Extensions.AI integration (IChatClient support):

```bash
dotnet add package LionFire.OpenCode.Serve.AgentFramework
```

## Quick Start (30 Seconds)

Here's the simplest way to get started:

```csharp
using LionFire.OpenCode.Serve;
using LionFire.OpenCode.Serve.Models;

// Create a client (connects to localhost:9123 by default)
await using var client = new OpenCodeClient();

// Create a session for your project
var session = await client.CreateSessionAsync(directory: "/path/to/your/project");

// Send a message and get the AI response
var request = new SendMessageRequest
{
    Parts = new List<PartInput>
    {
        PartInput.TextInput("Hello! What can you help me with?")
    }
};

var response = await client.PromptAsync(session.Id, request);

// Extract and display the response
foreach (var part in response.Parts ?? new List<Part>())
{
    if (part.IsTextPart)
    {
        Console.WriteLine(part.Text);
    }
}

// Clean up (session is deleted)
await client.DeleteSessionAsync(session.Id);
```

## Step-by-Step Tutorial

### Step 1: Create a Console Application

```bash
dotnet new console -n OpenCodeDemo
cd OpenCodeDemo
dotnet add package LionFire.OpenCode.Serve
```

### Step 2: Update Program.cs

```csharp
using LionFire.OpenCode.Serve;
using LionFire.OpenCode.Serve.Models;

Console.WriteLine("OpenCode SDK Demo");
Console.WriteLine("==================");

try
{
    // Create the client
    await using var client = new OpenCodeClient();
    Console.WriteLine("✓ Connected to OpenCode server");

    // Create a session
    var session = await client.CreateSessionAsync();
    Console.WriteLine($"✓ Created session: {session.Id}");

    // Send a message
    var request = new SendMessageRequest
    {
        Parts = new List<PartInput>
        {
            PartInput.TextInput("What is 2 + 2?")
        }
    };

    Console.WriteLine("\n> Asking: What is 2 + 2?");
    var response = await client.PromptAsync(session.Id, request);

    // Display response
    Console.WriteLine("\n< Response:");
    foreach (var part in response.Parts ?? new List<Part>())
    {
        if (part.IsTextPart)
        {
            Console.WriteLine(part.Text);
        }
    }

    // Clean up
    await client.DeleteSessionAsync(session.Id);
    Console.WriteLine("\n✓ Session cleaned up");
}
catch (OpenCodeConnectionException ex)
{
    Console.WriteLine($"Connection error: {ex.Message}");
    Console.WriteLine("Make sure OpenCode server is running: opencode serve");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

### Step 3: Run the Demo

```bash
# Make sure OpenCode server is running
opencode serve

# In another terminal, run your app
dotnet run
```

## Configuration Options

You can customize the client behavior:

```csharp
var options = new OpenCodeClientOptions
{
    // Server URL (default: http://localhost:9123)
    BaseUrl = "http://localhost:9123",

    // Timeout for quick operations like list/get (default: 30s)
    DefaultTimeout = TimeSpan.FromSeconds(30),

    // Timeout for AI operations like prompts (default: 5 min)
    MessageTimeout = TimeSpan.FromMinutes(5),

    // Enable/disable telemetry (default: true)
    EnableTelemetry = true,

    // Auto-retry on transient failures (default: true)
    EnableRetry = true,

    // Number of retry attempts (default: 3)
    MaxRetryAttempts = 3
};

var client = new OpenCodeClient(options);
```

## Using Dependency Injection

For ASP.NET Core or other DI-enabled applications:

```csharp
// In Program.cs or Startup.cs
builder.Services.AddOpenCodeClient(options =>
{
    options.BaseUrl = "http://localhost:9123";
    options.EnableTelemetry = true;
});

// In your service
public class AIService
{
    private readonly IOpenCodeClient _client;

    public AIService(IOpenCodeClient client)
    {
        _client = client;
    }

    public async Task<string> AskAsync(string question)
    {
        var session = await _client.CreateSessionAsync();
        try
        {
            var request = new SendMessageRequest
            {
                Parts = new List<PartInput> { PartInput.TextInput(question) }
            };
            var response = await _client.PromptAsync(session.Id, request);
            return response.Parts?.FirstOrDefault(p => p.IsTextPart)?.Text ?? "";
        }
        finally
        {
            await _client.DeleteSessionAsync(session.Id);
        }
    }
}
```

## Using with Microsoft.Extensions.AI

If you want to use the standard `IChatClient` interface:

```csharp
using LionFire.OpenCode.Serve;
using LionFire.OpenCode.Serve.AgentFramework;
using Microsoft.Extensions.AI;

// Create the OpenCode client
var openCodeClient = new OpenCodeClient();

// Create an IChatClient wrapper
var chatClient = new OpenCodeChatClient(openCodeClient);

// Use standard AI abstractions
var response = await chatClient.GetResponseAsync(new[]
{
    new ChatMessage(ChatRole.User, "Hello!")
});

Console.WriteLine(response.Messages[0].Text);
```

## Next Steps

- [Advanced Scenarios](./advanced-scenarios.md) - Streaming, file operations, PTY
- [Configuration Reference](./configuration-reference.md) - All configuration options
- [Error Handling Guide](./error-handling.md) - Exception handling patterns
- [API Reference](./api-reference.md) - Complete API documentation
