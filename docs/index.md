# LionFire.OpenCode.Serve Documentation

Welcome to the LionFire.OpenCode.Serve SDK documentation.

## Overview

LionFire.OpenCode.Serve is a .NET SDK for interacting with the [OpenCode](https://github.com/opencode-ai/opencode) headless server. It provides a type-safe, async-first API for building AI-powered .NET applications.

## Quick Links

| Guide | Description |
|-------|-------------|
| [Getting Started](./getting-started.md) | Install the SDK and make your first API call |
| [API Reference](./api-reference.md) | Complete API documentation |
| [Configuration Reference](./configuration-reference.md) | All configuration options |
| [Advanced Scenarios](./advanced-scenarios.md) | Streaming, file operations, PTY, MCP |

## Core Features

- **Full OpenCode API Coverage**: All 60+ API methods
- **Microsoft.Extensions.AI**: `IChatClient` integration
- **Dependency Injection**: First-class DI support
- **Streaming**: `IAsyncEnumerable` for real-time responses
- **AOT Compatible**: Source-generated JSON serialization
- **Observable**: OpenTelemetry tracing support

## Installation

```bash
# Core SDK
dotnet add package LionFire.OpenCode.Serve

# Microsoft.Extensions.AI integration
dotnet add package LionFire.OpenCode.Serve.AgentFramework
```

## Quick Start

```csharp
using LionFire.OpenCode.Serve;
using LionFire.OpenCode.Serve.Models;

var client = new OpenCodeClient();
var session = await client.CreateSessionAsync();

var response = await client.PromptAsync(session.Id, new SendMessageRequest
{
    Parts = new List<PartInput> { PartInput.TextInput("Hello!") }
});

Console.WriteLine(response.Parts?.FirstOrDefault(p => p.IsTextPart)?.Text);
```

## Documentation Index

### Getting Started
- [Getting Started Tutorial](./getting-started.md) - Step-by-step setup guide

### Core Guides
- [Configuration Reference](./configuration-reference.md) - All configuration options
- [Error Handling Guide](./error-handling.md) - Exception handling patterns
- [Advanced Scenarios](./advanced-scenarios.md) - Streaming, DI, Agent Framework

### Production
- [Performance Tuning](./performance-tuning.md) - Optimization strategies
- [Enterprise Deployment](./enterprise-deployment.md) - Production deployment guide
- [Troubleshooting Runbook](./troubleshooting.md) - Common issues and solutions

### Reference
- [API Reference](./api-reference.md) - Complete API documentation
- [Migration Guide](./migration-guide.md) - Version upgrade guide
- [FAQ](./faq.md) - Frequently asked questions

## Prerequisites

- .NET 8.0 or later
- OpenCode server running locally (`opencode serve`)

## Support

- [GitHub Issues](https://github.com/lionfire/opencode-dotnet/issues) - Bug reports and feature requests
- [Contributing Guide](../CONTRIBUTING.md) - How to contribute

## License

MIT License - see [LICENSE](../LICENSE) for details.
