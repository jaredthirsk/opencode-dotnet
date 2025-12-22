---
greenlit: true
---

# Epic 02-001: Source-Generated JSON and AOT

**Phase**: 02 - Production Hardening
**Estimated Effort**: 3-4 days

## Overview

Implement System.Text.Json source generators for all DTOs to enable Native AOT compilation and improve startup performance.

## Tasks

- [x] Add [JsonSerializable] attributes to all DTOs
- [x] Create JsonSerializerContext class
- [x] Update serialization calls to use generated context
- [x] Test AOT compilation with sample app
- [ ] Benchmark startup time improvement
- [x] Update documentation for AOT scenarios

## Acceptance Criteria

- AOT compilation succeeds
- All JSON operations work with source-generated code
- Startup time improved by >20%
- No reflection warnings in AOT mode
