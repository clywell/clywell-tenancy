namespace Clywell.Core.Tenancy;

/// <summary>
/// Mutable, scoped implementation of <see cref="ITenantContext"/>.
/// Set once per request by <see cref="TenantResolutionMiddleware"/>; read-only thereafter via the interface.
/// </summary>
internal sealed class TenantContext : ITenantContext
{
    public TenantInfo? TenantInfo { get; private set; }
    public Guid? TenantId => TenantInfo?.TenantId;
    public string? TenantName => TenantInfo?.TenantName;
    public bool IsResolved => TenantInfo is not null;

    /// <summary>Populates the context with resolved tenant data. Called exclusively by <see cref="TenantResolutionMiddleware"/>.</summary>
    internal void SetTenant(TenantInfo tenantInfo) => TenantInfo = tenantInfo;
}
