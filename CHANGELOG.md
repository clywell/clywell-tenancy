# Changelog

All notable changes to `Clywell.Core.Tenancy` will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-03-01

### Added

#### Tenant Context
- `ITenantContext` — scoped interface exposing `TenantId`, `TenantName`, `IsResolved`, and `TenantInfo` for the current request
- `TenantContext` — internal mutable implementation of `ITenantContext`; written once per request by `TenantResolutionMiddleware`
- `TenantInfo` — open record (`record TenantInfo(Guid TenantId, string? TenantName)`) carrying the result of a successful resolution; subclass to attach additional tenant metadata
- `TenantClaimTypes` — constants for JWT claim names: `tid`, `tenantid`, `tenant_name`

#### Tenant Resolution
- `ITenantResolver` — pluggable abstraction for tenant resolution; implement to resolve from JWT claims, HTTP headers, subdomains, databases, API keys, or any custom source; returns `TenantInfo?` (null = unresolved)
- `ClaimsTenantResolver` — default `ITenantResolver` implementation; reads `tid` (primary) and `tenantid` (fallback) claims and optionally `tenant_name`; used automatically when no custom resolver is configured
- `TenantResolutionMiddleware` — ASP.NET Core middleware that delegates to the registered `ITenantResolver` and populates `ITenantContext` for the request scope; register via `app.UseMiddleware<TenantResolutionMiddleware>()` after authentication

#### Configuration
- `TenancyOptions` — configuration object accepted by `AddTenancy()`; exposes `UseResolver<TResolver>()` (type-based, DI-resolved) and `UseResolver(factory)` (delegate-based) for declaring a custom resolver at registration time — order-independent, no "register before" convention required
- `ServiceCollectionExtensions.AddTenancy(Action<TenancyOptions>? configure = null)` — single DI registration call; registers `ITenantContext`, `ITenantResolver`, `TenantLogEnricher`, `UserLogEnricher`, and `IHttpContextAccessor`; uses `TryAdd` semantics throughout so multiple calls are safe

#### Log Enrichment
- `TenantLogEnricher` — Serilog `ILogEventEnricher` that appends `TenantId` and `TenantName` (when resolved) to every log event in scope
- `UserLogEnricher` — Serilog `ILogEventEnricher` that appends `UserId` from the `sub` claim, falling back to `NameIdentifier`

[Unreleased]: https://github.com/clywell/clywell-tenancy/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/clywell/clywell-tenancy/releases/tag/v1.0.0
