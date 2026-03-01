namespace Clywell.Core.Tenancy;

/// <summary>
/// Carries the result of a successful tenant resolution.
/// Produced by <see cref="ITenantResolver"/> and consumed by <see cref="TenantResolutionMiddleware"/>
/// to populate <see cref="ITenantContext"/>.
/// </summary>
public record TenantInfo(Guid TenantId, string? TenantName = null);
