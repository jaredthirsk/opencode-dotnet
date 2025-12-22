---
greenlit: true
---

# Epic 02-003: Polly Resilience Integration

**Phase**: 02 - Production Hardening
**Estimated Effort**: 4-5 days

## Overview

Integrate Polly for advanced resilience patterns: circuit breaker, advanced retry, timeout, bulkhead.

## Tasks

- [x] Add Polly dependency (optional package or built-in)
- [x] Implement circuit breaker policy for repeated failures
- [x] Enhanced retry policy with jitter
- [x] Per-operation timeout policies
- [x] Add OpencodeClientOptions resilience settings
- [x] Test circuit breaker behavior (open, half-open, closed)
- [x] Document resilience configuration
- [x] Add example demonstrating resilience patterns

## Acceptance Criteria

- Circuit breaker prevents cascading failures
- Retry with jitter reduces thundering herd
- Timeout policies configurable per operation
- >85% coverage of resilience code paths
