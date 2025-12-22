# Enterprise Deployment Guide

This guide covers deploying applications using LionFire.OpenCode.Serve in enterprise environments.

## Architecture Overview

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────┐
│  Your .NET App  │────▶│  OpenCode Server │────▶│  AI Provider │
│  (SDK Client)   │     │  (localhost:9123) │     │ (Anthropic,  │
└─────────────────┘     └──────────────────┘     │  OpenAI, etc)│
                                                  └─────────────┘
```

## Deployment Models

### 1. Sidecar Pattern (Recommended)

Deploy OpenCode server as a sidecar container alongside your application:

```yaml
# docker-compose.yml
services:
  app:
    image: your-app:latest
    environment:
      - OPENCODE_BASE_URL=http://opencode:9123
    depends_on:
      - opencode

  opencode:
    image: opencode:latest
    command: serve --host 0.0.0.0
    ports:
      - "9123:9123"
    volumes:
      - ./workspace:/workspace
```

### 2. Shared Service Pattern

Central OpenCode server for multiple applications:

```
┌─────────────┐
│   App 1     │──┐
└─────────────┘  │     ┌──────────────────┐
                 ├────▶│  Central OpenCode │
┌─────────────┐  │     │      Server       │
│   App 2     │──┤     └──────────────────┘
└─────────────┘  │
                 │
┌─────────────┐  │
│   App 3     │──┘
└─────────────┘
```

### 3. Per-User Pattern

Each user gets their own OpenCode instance:

```csharp
public class UserOpenCodeFactory
{
    private readonly ConcurrentDictionary<string, IOpenCodeClient> _clients = new();

    public IOpenCodeClient GetClientForUser(string userId, string userWorkspace)
    {
        return _clients.GetOrAdd(userId, _ =>
        {
            // Each user has isolated workspace
            return new OpenCodeClient(new OpenCodeClientOptions
            {
                BaseUrl = $"http://localhost:{GetPortForUser(userId)}",
                Directory = userWorkspace
            });
        });
    }
}
```

## Security Considerations

### Network Security

**Internal Network Only:**
```csharp
// OpenCode should NOT be exposed to internet
services.AddOpenCodeClient(options =>
{
    // Only internal addresses
    options.BaseUrl = "http://opencode.internal:9123";
});
```

**Use Service Mesh:**
```yaml
# Kubernetes with Istio
apiVersion: networking.istio.io/v1beta1
kind: ServiceEntry
metadata:
  name: opencode
spec:
  hosts:
    - opencode.internal
  ports:
    - number: 9123
      name: http
      protocol: HTTP
  resolution: DNS
```

### API Key Management

```csharp
// Configure provider API keys securely
services.AddOpenCodeClient(options =>
{
    options.BaseUrl = configuration["OpenCode:BaseUrl"];
});

// API keys should be configured in OpenCode server environment
// NOT passed through the SDK
```

### Workspace Isolation

```csharp
// Ensure proper workspace isolation
var userWorkspace = Path.Combine(
    configuration["WorkspaceRoot"],
    SanitizePath(userId)
);

// Validate path is within allowed root
if (!userWorkspace.StartsWith(configuration["WorkspaceRoot"]))
{
    throw new SecurityException("Path traversal detected");
}

var session = await client.CreateSessionAsync(directory: userWorkspace);
```

## High Availability

### Health Checks

```csharp
// Register health check
services.AddHealthChecks()
    .AddCheck<OpenCodeHealthCheck>("opencode", tags: new[] { "ready" });

public class OpenCodeHealthCheck : IHealthCheck
{
    private readonly IOpenCodeClient _client;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.ListProjectsAsync(cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (OpenCodeConnectionException)
        {
            return HealthCheckResult.Unhealthy("Cannot connect to OpenCode server");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}
```

### Circuit Breaker Pattern

```csharp
// Using Polly
services.AddHttpClient<IOpenCodeClient, OpenCodeClient>()
    .AddPolicyHandler(Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30)));
```

### Failover Configuration

```csharp
public class FailoverOpenCodeClient
{
    private readonly IOpenCodeClient _primary;
    private readonly IOpenCodeClient _secondary;
    private readonly ILogger _logger;

    public async Task<T> ExecuteWithFailoverAsync<T>(
        Func<IOpenCodeClient, Task<T>> operation)
    {
        try
        {
            return await operation(_primary);
        }
        catch (OpenCodeConnectionException)
        {
            _logger.LogWarning("Primary OpenCode failed, trying secondary");
            return await operation(_secondary);
        }
    }
}
```

## Monitoring and Observability

### OpenTelemetry Setup

```csharp
services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService("your-app")
            .AddAttributes(new Dictionary<string, object>
            {
                ["environment"] = configuration["Environment"]
            }))
        .AddSource("LionFire.OpenCode.Serve")
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(configuration["Otlp:Endpoint"]);
        }))
    .WithMetrics(metrics => metrics
        .AddMeter("LionFire.OpenCode.Serve")
        .AddHttpClientInstrumentation()
        .AddOtlpExporter());
```

### Custom Metrics

```csharp
public class OpenCodeMetrics
{
    private readonly Counter<long> _requestCount;
    private readonly Histogram<double> _requestDuration;
    private readonly UpDownCounter<int> _activeSessions;

    public OpenCodeMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("LionFire.OpenCode.Serve");

        _requestCount = meter.CreateCounter<long>(
            "opencode.requests.total",
            description: "Total OpenCode requests");

        _requestDuration = meter.CreateHistogram<double>(
            "opencode.request.duration.ms",
            description: "Request duration in milliseconds");

        _activeSessions = meter.CreateUpDownCounter<int>(
            "opencode.sessions.active",
            description: "Active session count");
    }

    public void RecordRequest(string operation, double durationMs)
    {
        _requestCount.Add(1, new KeyValuePair<string, object>("operation", operation));
        _requestDuration.Record(durationMs, new KeyValuePair<string, object>("operation", operation));
    }
}
```

### Logging Configuration

```csharp
// Structured logging with Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("LionFire.OpenCode.Serve", LogEventLevel.Debug)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .WriteTo.Console(new JsonFormatter())
    .WriteTo.Seq(configuration["Seq:Url"])
    .CreateLogger();

builder.Host.UseSerilog();
```

## Scaling

### Horizontal Scaling

```yaml
# Kubernetes deployment
apiVersion: apps/v1
kind: Deployment
metadata:
  name: your-app
spec:
  replicas: 3
  template:
    spec:
      containers:
        - name: app
          env:
            - name: OPENCODE_BASE_URL
              value: "http://opencode:9123"
        - name: opencode
          image: opencode:latest
          command: ["opencode", "serve", "--host", "0.0.0.0"]
          resources:
            requests:
              memory: "512Mi"
              cpu: "250m"
            limits:
              memory: "1Gi"
              cpu: "1000m"
```

### Resource Limits

```yaml
# Per-container limits
resources:
  requests:
    memory: "512Mi"
    cpu: "250m"
  limits:
    memory: "2Gi"
    cpu: "2000m"
```

## Configuration Management

### Environment-Based Configuration

```csharp
// appsettings.Production.json
{
  "OpenCode": {
    "BaseUrl": "http://opencode.internal:9123",
    "DefaultTimeout": "00:00:30",
    "MessageTimeout": "00:10:00",
    "EnableTelemetry": true,
    "EnableRetry": true,
    "MaxRetryAttempts": 5
  }
}

// Program.cs
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

services.Configure<OpenCodeClientOptions>(
    builder.Configuration.GetSection("OpenCode"));
```

### Secret Management

```csharp
// Azure Key Vault
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());

// AWS Secrets Manager
builder.Configuration.AddSecretsManager();

// HashiCorp Vault
builder.Configuration.AddVault(options =>
{
    options.Address = configuration["Vault:Address"];
    options.Token = configuration["Vault:Token"];
});
```

## Disaster Recovery

### Backup Strategy

```bash
# Backup OpenCode data
tar -czf opencode-backup-$(date +%Y%m%d).tar.gz \
    ~/.opencode \
    /var/lib/opencode
```

### Session Recovery

```csharp
public class ResilientSessionManager
{
    private readonly IOpenCodeClient _client;
    private readonly IDistributedCache _cache;

    public async Task<string> GetOrCreateSessionAsync(string contextId)
    {
        // Try to recover existing session
        var cachedSessionId = await _cache.GetStringAsync($"session:{contextId}");

        if (cachedSessionId != null)
        {
            try
            {
                // Verify session still exists
                await _client.GetSessionAsync(cachedSessionId);
                return cachedSessionId;
            }
            catch (OpenCodeNotFoundException)
            {
                // Session was deleted, create new one
            }
        }

        // Create new session
        var session = await _client.CreateSessionAsync();
        await _cache.SetStringAsync(
            $"session:{contextId}",
            session.Id,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            });

        return session.Id;
    }
}
```

## Compliance Considerations

### Data Residency

```csharp
// Ensure data stays in region
services.AddOpenCodeClient(options =>
{
    // Use region-specific endpoints
    options.BaseUrl = configuration["OpenCode:RegionalEndpoint"];
    options.Directory = configuration["OpenCode:RegionalWorkspace"];
});
```

### Audit Logging

```csharp
public class AuditingOpenCodeClient : IOpenCodeClient
{
    private readonly IOpenCodeClient _inner;
    private readonly IAuditLogger _auditLogger;
    private readonly ICurrentUserService _userService;

    public async Task<Session> CreateSessionAsync(
        CreateSessionRequest? request = null,
        string? directory = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _inner.CreateSessionAsync(request, directory, cancellationToken);

        await _auditLogger.LogAsync(new AuditEvent
        {
            Action = "SessionCreated",
            UserId = _userService.UserId,
            ResourceId = result.Id,
            Timestamp = DateTime.UtcNow,
            Details = new { directory }
        });

        return result;
    }

    // Implement other methods with similar auditing...
}
```

### PII Handling

```csharp
public class SanitizingOpenCodeClient
{
    private readonly IOpenCodeClient _client;
    private readonly IPiiDetector _piiDetector;

    public async Task<MessageWithParts> PromptAsync(
        string sessionId,
        SendMessageRequest request,
        string? directory = null,
        CancellationToken cancellationToken = default)
    {
        // Detect and optionally redact PII before sending
        foreach (var part in request.Parts)
        {
            if (part.Text != null && _piiDetector.ContainsPii(part.Text))
            {
                // Log warning, redact, or reject based on policy
                _logger.LogWarning("PII detected in prompt");
            }
        }

        return await _client.PromptAsync(sessionId, request, directory, cancellationToken);
    }
}
```

## Production Checklist

### Pre-Deployment

- [ ] Security review completed
- [ ] Network isolation configured
- [ ] API keys securely managed
- [ ] Workspace isolation verified
- [ ] Health checks implemented
- [ ] Circuit breakers configured
- [ ] Telemetry and logging set up
- [ ] Resource limits defined
- [ ] Backup strategy in place
- [ ] Compliance requirements addressed

### Monitoring

- [ ] Dashboards created
- [ ] Alerts configured
- [ ] Log aggregation working
- [ ] Distributed tracing enabled
- [ ] Performance baselines established

### Operations

- [ ] Runbook documented
- [ ] On-call procedures defined
- [ ] Escalation paths clear
- [ ] Recovery procedures tested
- [ ] Capacity planning done

## Related Documentation

- [Configuration Reference](./configuration-reference.md)
- [Performance Tuning Guide](./performance-tuning.md)
- [Error Handling Guide](./error-handling.md)
- [Troubleshooting Runbook](./troubleshooting.md)
