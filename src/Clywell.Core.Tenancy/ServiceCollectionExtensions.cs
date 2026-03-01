namespace Clywell.Core.Tenancy;

/// <summary>Extension methods for registering Clywell.Core.Tenancy services.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers tenancy services: <see cref="ITenantContext"/> (scoped),
    /// <see cref="ITenantResolver"/> (defaults to <see cref="ClaimsTenantResolver"/>),
    /// <see cref="TenantLogEnricher"/>, and <see cref="UserLogEnricher"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">
    /// Optional delegate to configure <see cref="TenancyOptions"/>.
    /// Use this to supply a custom <see cref="ITenantResolver"/>:
    /// <code>
    /// services.AddTenancy(options =&gt; options.UseResolver&lt;MyHeaderTenantResolver&gt;());
    /// // or with a factory:
    /// services.AddTenancy(options =&gt; options.UseResolver(sp =&gt; new MyResolver(sp.GetRequiredService&lt;IMyService&gt;())));
    /// </code>
    /// When omitted, <see cref="ClaimsTenantResolver"/> is used.
    /// After calling this, add the middleware to your pipeline (after authentication):
    /// <code>app.UseMiddleware&lt;TenantResolutionMiddleware&gt;();</code>
    /// </param>
    public static IServiceCollection AddTenancy(this IServiceCollection services, Action<TenancyOptions>? configure = null)
    {
        services.TryAddScoped<TenantContext>();
        services.TryAddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

        var options = new TenancyOptions();
        configure?.Invoke(options);
        options.Apply(services);

        services.AddHttpContextAccessor();

        services.TryAddScoped<TenantLogEnricher>();
        services.TryAddScoped<UserLogEnricher>();

        return services;
    }
}
