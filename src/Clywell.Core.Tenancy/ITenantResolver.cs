namespace Clywell.Core.Tenancy;

/// <summary>
/// Resolves the current tenant from an incoming <see cref="HttpContext"/>.
/// </summary>
/// <remarks>
/// Implement this interface to support custom tenant resolution strategies
/// (e.g., subdomain, header, database lookup, API key), then supply it via
/// <see cref="TenancyOptions.UseResolver{TResolver}"/> when calling <c>AddTenancy()</c>:
/// <code>
/// services.AddTenancy(options =&gt; options.UseResolver&lt;MyHeaderTenantResolver&gt;());
/// </code>
/// The default implementation is <see cref="ClaimsTenantResolver"/>.
/// </remarks>
public interface ITenantResolver
{
    /// <summary>
    /// Attempts to resolve the tenant from the provided HTTP context.
    /// </summary>
    /// <returns>A <see cref="TenantInfo"/> if resolution succeeded; <see langword="null"/> otherwise.</returns>
    public Task<TenantInfo?> ResolveAsync(HttpContext context);
}
