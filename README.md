# Clywell.Core.Tenancy

[![NuGet](https://img.shields.io/nuget/v/Clywell.Core.Tenancy.svg)](https://www.nuget.org/packages/Clywell.Core.Tenancy)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Clywell.Core.Tenancy.svg)](https://www.nuget.org/packages/Clywell.Core.Tenancy)
[![Build Status](https://github.com/clywell/clywell-tenancy/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/clywell/clywell-tenancy/actions/workflows/ci-cd.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

Multi-tenancy plumbing for .NET — tenant context resolution, ASP.NET Core middleware, and Serilog log enrichers. Zero EF Core dependency; pure infrastructure.

## Features

- **`ITenantContext`** — scoped service carrying `TenantId`, `TenantName`, `IsResolved`, and the full `TenantInfo` object for the current request
- **`ITenantResolver`** — pluggable abstraction for tenant resolution; implement it to resolve from headers, subdomains, databases, API keys, or any custom source
- **`TenantInfo`** — extensible record carrying the result of a successful resolution; subclass it to carry additional tenant metadata
- **`ClaimsTenantResolver`** — built-in resolver that reads `tid` / `tenantid` JWT claims; used by default when no custom resolver is configured
- **`TenantResolutionMiddleware`** — opt-in middleware that delegates to `ITenantResolver` and populates `ITenantContext` per request
- **`TenancyOptions`** — configuration object accepted by `AddTenancy()` for declaring a custom resolver at registration time
- **`TenantLogEnricher`** — Serilog enricher that appends `TenantId` and `TenantName` to every log event
- **`UserLogEnricher`** — Serilog enricher that appends `UserId` from the authenticated user's claims
- **`AddTenancy()`** — single DI registration call; no-arg form uses the default claims resolver

## Installation

```bash
dotnet add package Clywell.Core.Tenancy
```

## Quick Start

### 1. Register services

```csharp
// Program.cs — default: resolves tenant from JWT claims
builder.Services.AddTenancy();

// or supply a custom resolver:
builder.Services.AddTenancy(options =>
    options.UseResolver<MyHeaderTenantResolver>());
```

### 2. Add middleware (after authentication)

```csharp
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantResolutionMiddleware>();
```

### 3. Consume the tenant context

```csharp
public class MyService(ITenantContext tenantContext)
{
    public Guid GetCurrentTenant() => tenantContext.TenantId
        ?? throw new InvalidOperationException("No tenant resolved.");
}
```

### 4. Enrich logs with tenant info

```csharp
var tenantEnricher = app.Services.GetRequiredService<TenantLogEnricher>();
var userEnricher   = app.Services.GetRequiredService<UserLogEnricher>();

Log.Logger = new LoggerConfiguration()
    .Enrich.With(tenantEnricher)
    .Enrich.With(userEnricher)
    .WriteTo.Console()
    .CreateLogger();
```

Every log event will include `TenantId`, `TenantName`, and `UserId` properties.

## Custom Tenant Resolver

Implement `ITenantResolver` to resolve the tenant from any source:

```csharp
public class HeaderTenantResolver : ITenantResolver
{
    public Task<TenantInfo?> ResolveAsync(HttpContext context)
    {
        var raw = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        if (raw is null || !Guid.TryParse(raw, out var id))
            return Task.FromResult<TenantInfo?>(null);

        return Task.FromResult<TenantInfo?>(new TenantInfo(id));
    }
}
```

Register it via `AddTenancy()`:

```csharp
// By type — HeaderTenantResolver is resolved from DI, so its own dependencies are injected
services.AddTenancy(options => options.UseResolver<HeaderTenantResolver>());

// Or with a factory when you need manual composition
services.AddTenancy(options => options.UseResolver(sp =>
    new HeaderTenantResolver(sp.GetRequiredService<IMyService>())));
```

## Extending TenantInfo

`TenantInfo` is an open record — subclass it to carry additional metadata that your resolver provides and your application needs:

```csharp
public record MyTenantInfo(Guid TenantId, string Region, string Plan, string? TenantName = null)
    : TenantInfo(TenantId, TenantName);
```

Return it from your resolver:

```csharp
return new MyTenantInfo(tenantId, region: "eu-west", plan: "enterprise");
```

Access the extra fields from `ITenantContext.TenantInfo`:

```csharp
if (tenantContext.TenantInfo is MyTenantInfo info)
{
    var region = info.Region;
    var plan   = info.Plan;
}
```

`TenantId` and `TenantName` remain available directly on `ITenantContext` as a convenience — no cast needed for standard properties.

## JWT Claim Resolution (default)

When no custom resolver is configured, `ClaimsTenantResolver` reads from the following claims:

| Claim | Description |
|---|---|
| `tid` | Primary tenant identifier (Azure AD / Entra ID) |
| `tenantid` | Fallback tenant identifier |
| `tenant_name` | Optional human-readable tenant name |
| `sub` | Subject claim for user identity (`UserLogEnricher`) |
| `NameIdentifier` | Fallback for user identity (`UserLogEnricher`) |

## Notes

- **No EF Core dependency** — EF global query filters belong in `Clywell.Core.Data.EntityFramework`; this package is pure plumbing.
- **Opt-in middleware** — `AddTenancy()` does not add middleware automatically; you must call `app.UseMiddleware<TenantResolutionMiddleware>()`.
- **Scoped by design** — `ITenantContext` is `Scoped` and populated once per HTTP request.
- **`TryAdd` semantics** — calling `AddTenancy()` multiple times is safe; subsequent calls are no-ops for already-registered services.

## License

MIT — see [LICENSE](LICENSE) for details.
