namespace Clywell.Core.Tenancy;

/// <summary>
/// ASP.NET Core middleware that delegates tenant resolution to <see cref="ITenantResolver"/>
/// and populates <see cref="ITenantContext"/> for the duration of the request.
/// </summary>
/// <remarks>
/// Register via <c>app.UseMiddleware&lt;TenantResolutionMiddleware&gt;()</c> after
/// authentication middleware. The resolution strategy is controlled by the registered
/// <see cref="ITenantResolver"/> — swap it out to support headers, subdomains,
/// database lookups, or any custom source.
/// </remarks>
public sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, ITenantResolver resolver)
    {
        var tenantInfo = await resolver.ResolveAsync(context);

        if (tenantInfo is not null && tenantContext is TenantContext mutableContext)
        {
            mutableContext.SetTenant(tenantInfo);
        }

        await next(context);
    }
}
