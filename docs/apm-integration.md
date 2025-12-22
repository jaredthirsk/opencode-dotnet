# APM Integration Guide

This guide explains how to integrate the OpenCode .NET SDK with Application Performance Monitoring (APM) tools using OpenTelemetry.

## Overview

The OpenCode SDK includes built-in telemetry using .NET's standard diagnostics APIs:
- **Traces**: Using `System.Diagnostics.Activity` (compatible with OpenTelemetry)
- **Metrics**: Using `System.Diagnostics.Metrics` (compatible with OpenTelemetry)

This enables integration with any APM tool that supports OpenTelemetry, including:
- Jaeger
- Zipkin
- Datadog
- New Relic
- Azure Monitor
- AWS X-Ray
- Grafana Tempo

## Quick Start

### 1. Install OpenTelemetry Packages

```bash
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Exporter.Jaeger
# Or your preferred exporter
```

### 2. Configure OpenTelemetry

```csharp
using LionFire.OpenCode.Serve.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add OpenCode client
builder.Services.AddOpenCodeClient(options =>
{
    options.BaseUrl = "http://localhost:9123";
    options.EnableTelemetry = true; // Default is true
});

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("my-opencode-app"))
    .WithTracing(tracing => tracing
        .AddSource(OpenTelemetryExtensions.ActivitySourceName)
        .AddHttpClientInstrumentation()
        .AddJaegerExporter(options =>
        {
            options.AgentHost = "localhost";
            options.AgentPort = 6831;
        }))
    .WithMetrics(metrics => metrics
        .AddMeter(OpenTelemetryExtensions.MeterName)
        .AddPrometheusExporter());
```

## Telemetry Details

### Traces (Activities)

The SDK creates traces for all HTTP operations with the following information:

| Tag | Description |
|-----|-------------|
| `opencode.session.id` | Session identifier |
| `opencode.message.id` | Message identifier |
| `opencode.tool.id` | Tool identifier |
| `opencode.file.path` | File path being accessed |
| `http.status_code` | HTTP response status code |
| `error.type` | Exception type name (on errors) |
| `opencode.streaming.chunks` | Number of chunks in streaming operations |

Activity Source Name: `LionFire.OpenCode.Serve`

### Metrics

The SDK exposes the following metrics:

| Metric | Type | Description |
|--------|------|-------------|
| `opencode.client.requests` | Counter | Total HTTP requests made |
| `opencode.client.errors` | Counter | Total errors encountered |
| `opencode.client.request.duration` | Histogram | Request duration in milliseconds |
| `opencode.client.requests.active` | UpDownCounter | Currently active requests |
| `opencode.client.streaming.chunks` | Counter | Streaming chunks received |
| `opencode.client.streaming.chunk.size` | Histogram | Size of streaming chunks in bytes |

Meter Name: `LionFire.OpenCode.Serve`

## Integration Examples

### Jaeger

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource(OpenTelemetryExtensions.ActivitySourceName)
        .AddJaegerExporter(options =>
        {
            options.AgentHost = "localhost";
            options.AgentPort = 6831;
        }));
```

Run Jaeger locally:
```bash
docker run -d --name jaeger \
  -p 5775:5775/udp \
  -p 6831:6831/udp \
  -p 6832:6832/udp \
  -p 5778:5778 \
  -p 16686:16686 \
  -p 14250:14250 \
  -p 14268:14268 \
  -p 14269:14269 \
  jaegertracing/all-in-one:latest
```

Access UI at http://localhost:16686

### Zipkin

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource(OpenTelemetryExtensions.ActivitySourceName)
        .AddZipkinExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
        }));
```

### Prometheus (Metrics)

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddMeter(OpenTelemetryExtensions.MeterName)
        .AddPrometheusExporter());

var app = builder.Build();
app.UseOpenTelemetryPrometheusScrapingEndpoint();
```

### Azure Monitor

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource(OpenTelemetryExtensions.ActivitySourceName)
        .AddAzureMonitorTraceExporter(options =>
        {
            options.ConnectionString = "YOUR_CONNECTION_STRING";
        }))
    .WithMetrics(metrics => metrics
        .AddMeter(OpenTelemetryExtensions.MeterName)
        .AddAzureMonitorMetricExporter(options =>
        {
            options.ConnectionString = "YOUR_CONNECTION_STRING";
        }));
```

### Datadog

```csharp
// Install Datadog.Trace.OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource(OpenTelemetryExtensions.ActivitySourceName)
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317");
        }));
```

## Distributed Tracing

The SDK automatically propagates W3C Trace Context headers (`traceparent`, `tracestate`) on outgoing HTTP requests. This enables distributed tracing across services.

### Trace Context Propagation

When the SDK makes HTTP requests to the OpenCode server, it includes:
- `traceparent`: Contains trace ID, parent span ID, and trace flags
- `tracestate`: Contains vendor-specific trace information

This allows traces to be correlated across:
- Your application
- The OpenCode SDK
- The OpenCode server
- Any downstream services

## Disabling Telemetry

If you need to disable telemetry for performance or privacy reasons:

```csharp
builder.Services.AddOpenCodeClient(options =>
{
    options.EnableTelemetry = false;
});
```

When disabled:
- No Activity spans are created
- No metrics are recorded
- No trace context headers are propagated
- Overhead is reduced to near-zero

## Performance Considerations

The telemetry implementation is designed for minimal overhead:
- Activities are only created when listeners are registered
- Metrics use efficient histogram implementations
- Trace context propagation adds minimal header overhead (~100 bytes)
- Estimated overhead: <5% on typical workloads

## Custom Instrumentation

You can extend the built-in telemetry with custom spans:

```csharp
using System.Diagnostics;
using LionFire.OpenCode.Serve.Internal;

// Start a custom child span
using var activity = OpenCodeActivitySource.Source.StartActivity("custom.operation");
activity?.SetTag("custom.attribute", "value");

// Your code here

activity?.SetStatus(ActivityStatusCode.Ok);
```

## Troubleshooting

### Traces Not Appearing

1. Verify telemetry is enabled: `options.EnableTelemetry = true`
2. Ensure the activity source is added: `.AddSource(OpenTelemetryExtensions.ActivitySourceName)`
3. Check exporter configuration and connectivity
4. Verify the exporter endpoint is accessible

### Metrics Not Appearing

1. Verify the meter is added: `.AddMeter(OpenTelemetryExtensions.MeterName)`
2. For Prometheus, ensure the scraping endpoint is configured
3. Check metric exporter configuration

### High Cardinality Warnings

The SDK uses bounded cardinality for all metric dimensions. If you see warnings:
- Session IDs are included but typically have reasonable cardinality
- Consider filtering in your metrics pipeline if needed

## Reference

### Activity Source

```csharp
// Name for OpenTelemetry configuration
OpenTelemetryExtensions.ActivitySourceName // "LionFire.OpenCode.Serve"

// Direct access to activity source
OpenCodeActivitySource.Source
OpenCodeActivitySource.Name
OpenCodeActivitySource.Version
```

### Metrics

```csharp
// Name for OpenTelemetry configuration
OpenTelemetryExtensions.MeterName // "LionFire.OpenCode.Serve"

// Direct access to metrics
OpenCodeMetrics.Instance
OpenCodeMetrics.MeterName
OpenCodeMetrics.MeterVersion
```

### Semantic Conventions

```csharp
// Attribute names
OpenCodeTelemetrySemantics.Attributes.SessionId
OpenCodeTelemetrySemantics.Attributes.MessageId
// etc.

// Span names
OpenCodeTelemetrySemantics.SpanNames.CreateSession
OpenCodeTelemetrySemantics.SpanNames.SendMessage
// etc.

// Metric names
OpenCodeTelemetrySemantics.MetricNames.RequestsTotal
OpenCodeTelemetrySemantics.MetricNames.RequestDuration
// etc.
```
