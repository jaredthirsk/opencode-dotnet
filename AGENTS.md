# AGENTS.md

## Build/Test Commands

```bash
# Build entire solution
dotnet build

# Run all tests
dotnet test

# Run single test (use full test name)
dotnet test --filter "FullyQualifiedName~TestMethodName"

# Run tests in specific project
dotnet test src/LionFire.OpenCode.Serve.Tests/
dotnet test src/LionFire.OpenCode.Serve.AgentFramework.Tests/

# Build with warnings as errors (project default)
dotnet build -c Release --verbosity normal
```

## Code Style Guidelines

### Project Configuration
- **Target Framework**: .NET 8.0
- **Language Version**: C# 12
- **Nullable Reference Types**: Enabled
- **Implicit Usings**: Enabled
- **Warnings as Errors**: Enabled (TreatWarningsAsErrors=true)

### Naming Conventions
- **Classes**: PascalCase (e.g., `OpenCodeClient`)
- **Interfaces**: PascalCase with `I` prefix (e.g., `IOpenCodeClient`)
- **Methods**: PascalCase (e.g., `SendMessageAsync`)
- **Properties**: PascalCase (e.g., `BaseUrl`)
- **Fields**: camelCase with `_` prefix for private fields
- **Constants**: PascalCase
- **Namespaces**: PascalCase (e.g., `LionFire.OpenCode.Serve.Models`)

### Import Organization
- System namespaces first, alphabetically
- Microsoft namespaces second, alphabetically
- Third-party namespaces third, alphabetically
- Project namespaces last, alphabetically
- Use `global using` directives for common imports in test projects

### Error Handling
- Use custom exception hierarchy inheriting from `OpenCodeException`
- Include `TroubleshootingHint` property for user guidance
- Use `try-catch` blocks with specific exception types
- Log errors using Microsoft.Extensions.Logging

### JSON Serialization
- Use camelCase property naming (JsonNamingPolicy.CamelCase)
- Enable source generation for AOT compatibility
- Use `JsonIgnoreCondition.WhenWritingNull`
- Include enum converters with camelCase naming

### Testing
- Use xUnit framework with `[Fact]` and `[Theory]`
- Use FluentAssertions for assertions (`.Should().`)
- Use NSubstitute for mocking
- Use RichardSzalay.MockHttp for HTTP mocking
- Test projects include global using directives for test frameworks

### Async Patterns
- Use `async`/`await` for all async operations
- Return `Task` or `Task<T>` from async methods
- Use `IAsyncEnumerable<T>` for streaming responses
- Configure `ConfigureAwait(false)` in library code

### Documentation
- Include XML documentation for public APIs
- Use `<summary>`, `<param>`, `<returns>` tags
- Add `<remarks>` for detailed usage examples
- Suppress CS1591 warnings for internal code

### Dependencies
- Prefer Microsoft.Extensions.* packages
- Use HttpClientFactory for HTTP client management
- Enable OpenTelemetry tracing support
- Support dependency injection patterns