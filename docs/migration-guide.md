# Migration Guide

This guide helps you migrate between versions of the LionFire.OpenCode.Serve SDK.

## Version History

| Version | Release Date | .NET Target | Key Changes |
|---------|--------------|-------------|-------------|
| 1.0.0 | TBD | .NET 8+ | Initial release |

## Migrating to 1.0.0 (Initial Release)

If you're coming from a pre-release or development version, here are the key things to know:

### Namespace Changes

The SDK uses the `LionFire.OpenCode.Serve` namespace:

```csharp
using LionFire.OpenCode.Serve;
using LionFire.OpenCode.Serve.Models;
using LionFire.OpenCode.Serve.Exceptions;
using LionFire.OpenCode.Serve.Extensions;
using LionFire.OpenCode.Serve.AgentFramework;
```

### Client Interface

The main client interface is `IOpenCodeClient`:

```csharp
// Create directly
var client = new OpenCodeClient();

// Or with options
var client = new OpenCodeClient(new OpenCodeClientOptions
{
    BaseUrl = "http://localhost:9123"
});

// Or via DI
services.AddOpenCodeClient(options => options.BaseUrl = "http://localhost:9123");
```

### Message Model

Messages use the `MessageWithParts` model:

```csharp
// Sending a message
var request = new SendMessageRequest
{
    Parts = new List<PartInput>
    {
        PartInput.TextInput("Hello!")
    }
};

var response = await client.PromptAsync(sessionId, request);

// Reading response
foreach (var part in response.Parts ?? new List<Part>())
{
    if (part.IsTextPart)
    {
        Console.WriteLine(part.Text);
    }
}
```

### Exception Handling

Use the SDK-specific exceptions:

```csharp
try
{
    await client.CreateSessionAsync();
}
catch (OpenCodeConnectionException ex)
{
    // Server not reachable
    Console.WriteLine(ex.TroubleshootingHint);
}
catch (OpenCodeNotFoundException ex)
{
    // Resource not found
}
catch (OpenCodeApiException ex)
{
    // HTTP error
    Console.WriteLine($"Status: {ex.StatusCode}");
}
catch (OpenCodeException ex)
{
    // Base exception
}
```

## Future Version Migration

This section will be updated as new versions are released.

### Breaking Change Policy

- **Major versions (X.0.0)**: May contain breaking changes
- **Minor versions (0.X.0)**: New features, backward compatible
- **Patch versions (0.0.X)**: Bug fixes only

### Deprecation Notices

When APIs are deprecated, they will be marked with:

```csharp
[Obsolete("Use NewMethod instead. Will be removed in v2.0.0")]
public Task<T> OldMethodAsync() { }
```

Deprecated APIs will be:
1. Marked with `[Obsolete]` in version N
2. Emit compiler warnings
3. Removed in version N+1 (major) or N+2 (minor)

## API Compatibility

### Stable APIs (1.0+)

These APIs are considered stable and will follow semantic versioning:

- `IOpenCodeClient` interface
- `OpenCodeClientOptions` configuration
- `OpenCodeException` hierarchy
- Model classes in `LionFire.OpenCode.Serve.Models`

### Experimental APIs

APIs marked with `[Experimental]` may change without notice:

```csharp
[Experimental("OPENCODE001")]
public Task<T> ExperimentalFeatureAsync() { }
```

To use experimental APIs:
```csharp
#pragma warning disable OPENCODE001
var result = await client.ExperimentalFeatureAsync();
#pragma warning restore OPENCODE001
```

## Package Dependencies

### Current Dependencies

| Package | Minimum Version | Purpose |
|---------|-----------------|---------|
| Microsoft.Extensions.Http | 8.0.0 | HttpClientFactory |
| Microsoft.Extensions.Logging.Abstractions | 8.0.0 | Logging |
| Microsoft.Extensions.Options | 8.0.0 | Configuration |
| System.Text.Json | 8.0.0 | JSON serialization |

### Agent Framework Dependencies

| Package | Minimum Version | Purpose |
|---------|-----------------|---------|
| Microsoft.Extensions.AI.Abstractions | 9.0.0 | IChatClient |

## Upgrade Checklist

When upgrading to a new version:

- [ ] Read release notes for breaking changes
- [ ] Update NuGet packages
- [ ] Fix any compiler warnings for deprecated APIs
- [ ] Run tests to verify functionality
- [ ] Update configuration if needed
- [ ] Review new features that might simplify your code

## Getting Help

If you encounter issues during migration:

1. Check [release notes](https://github.com/lionfire/opencode-dotnet/releases)
2. Search [GitHub issues](https://github.com/lionfire/opencode-dotnet/issues)
3. Open a new issue with:
   - Previous version
   - New version
   - Error messages
   - Code that's failing

## Related Documentation

- [Getting Started](./getting-started.md)
- [Configuration Reference](./configuration-reference.md)
- [API Reference](./api-reference.md)
