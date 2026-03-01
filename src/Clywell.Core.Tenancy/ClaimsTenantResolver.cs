namespace Clywell.Core.Tenancy;

/// <summary>
/// Default <see cref="ITenantResolver"/> that resolves the tenant from the authenticated
/// user's JWT claims (<c>tid</c> / <c>tenantid</c>).
/// </summary>
/// <remarks>
/// Replace this with a custom <see cref="ITenantResolver"/> implementation if your
/// tenant identity lives in a request header, subdomain, query string, or any
/// other source.
/// </remarks>
public sealed class ClaimsTenantResolver : ITenantResolver
{
    public Task<TenantInfo?> ResolveAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return Task.FromResult<TenantInfo?>(null);
        }

        var rawTenantId =
            context.User.FindFirst(TenantClaimTypes.TenantId)?.Value ??
            context.User.FindFirst(TenantClaimTypes.TenantIdAlternate)?.Value;

        if (rawTenantId is null || !Guid.TryParse(rawTenantId, out var tenantId))
        {
            return Task.FromResult<TenantInfo?>(null);
        }

        var tenantName = context.User.FindFirst(TenantClaimTypes.TenantName)?.Value;
        return Task.FromResult<TenantInfo?>(new TenantInfo(tenantId, tenantName));
    }
}
