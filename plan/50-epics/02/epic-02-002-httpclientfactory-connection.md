---
greenlit: true
---

# Epic 02-002: HttpClientFactory and Connection Management

**Phase**: 02 - Production Hardening
**Estimated Effort**: 2-3 days

## Overview

Integrate with IHttpClientFactory for proper HttpClient lifecycle management and connection pooling.

## Tasks

- [x] Add Microsoft.Extensions.Http dependency
- [x] Create AddOpencodeClient() DI extension
- [x] Support IHttpClientFactory in client construction
- [x] Configure HttpClient with default settings (timeout, headers)
- [x] Document DI registration patterns
- [x] Test connection pooling behavior

## Acceptance Criteria

- [x] AddOpencodeClient() extension works with IServiceCollection
- [x] HttpClient managed by factory
- [x] Connection pooling verified
- [x] ASP.NET Core example works

## Implementation Notes

### Completed Implementation

**Files Modified/Created:**

1. **`src/LionFire.OpenCode.Serve/LionFire.OpenCode.Serve.csproj`**
   - Added `Microsoft.Extensions.Http` package reference (v8.0.0)

2. **`src/LionFire.OpenCode.Serve/Extensions/ServiceCollectionExtensions.cs`**
   - `AddOpenCodeClient()` extension method with IHttpClientFactory integration
   - Returns `IHttpClientBuilder` for Polly policy chaining
   - Configures HttpClient with BaseUrl, Timeout, and Accept headers
   - Options validation via `IValidateOptions<OpenCodeClientOptions>`
   - Overload for using existing HttpClient instance

3. **`src/LionFire.OpenCode.Serve/OpenCodeClientOptions.cs`**
   - Comprehensive options: BaseUrl, Timeouts, Retry settings, Telemetry
   - Built-in validation

4. **`examples/AspNetCoreIntegration/`**
   - Complete ASP.NET Core example with DI registration
   - Health checks implementation
   - Chat endpoints using both IOpenCodeClient and IChatClient
   - Comprehensive README documenting DI patterns and connection pooling

### Key Features

- **IHttpClientFactory Integration**: Proper HttpClient lifecycle with connection pooling
- **Polly Support**: IHttpClientBuilder return allows chaining resilience policies
- **Options Pattern**: Full configuration support with validation
- **Multiple Registration Methods**: With options, with HttpClient instance
- **Thread Safety**: Client is thread-safe for concurrent operations
