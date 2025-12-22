---
greenlit: true
---

# Epic 02-004: OpenTelemetry Observability

**Phase**: 02 - Production Hardening
**Estimated Effort**: 3-4 days

## Overview

Integrate OpenTelemetry for distributed tracing, metrics, and observability in enterprise environments.

## Tasks

- [x] Add OpenTelemetry.Api dependency (using built-in System.Diagnostics.Activity and System.Diagnostics.Metrics)
- [x] Create ActivitySource for tracing (OpenCodeActivitySource.cs already existed)
- [x] Add traces for all HTTP operations (integrated into OpenCodeClient.cs)
- [x] Add metrics (request duration, error rate, streaming chunks) (OpenCodeMetrics.cs)
- [x] Propagate trace context in headers (W3C traceparent/tracestate via PropagateTraceContext)
- [x] Add OpencodeClientOptions.EnableTelemetry setting (already existed, default: true)
- [ ] Test with Jaeger or Zipkin
- [x] Document APM integration (docs/apm-integration.md)

## Acceptance Criteria

- Traces visible in APM tools (Jaeger, Datadog, etc.)
- Metrics exported correctly
- Trace context propagates across calls
- Telemetry overhead <5%
