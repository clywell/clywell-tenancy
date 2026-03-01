namespace Clywell.Core.Tenancy;

/// <summary>
/// Configuration options for <see cref="ServiceCollectionExtensions.AddTenancy"/>.
/// </summary>
/// <remarks>
/// Use <see cref="UseResolver{TResolver}()"/> or <see cref="UseResolver(Func{IServiceProvider,ITenantResolver})"/>
/// to supply a custom tenant resolution strategy. When no resolver is configured,
/// <see cref="ClaimsTenantResolver"/> is registered as the default.
/// </remarks>
public sealed class TenancyOptions
{
    private Action<IServiceCollection>? _resolverRegistration;

    /// <summary>
    /// Registers <typeparamref name="TResolver"/> as the <see cref="ITenantResolver"/> for this application.
    /// The type is resolved from the DI container, so its own dependencies will be injected automatically.
    /// </summary>
    public TenancyOptions UseResolver<TResolver>() where TResolver : class, ITenantResolver
    {
        _resolverRegistration = services => services.AddScoped<ITenantResolver, TResolver>();
        return this;
    }

    /// <summary>
    /// Registers a factory delegate as the <see cref="ITenantResolver"/> for this application.
    /// Use this overload when you need to compose the resolver manually from the service provider.
    /// </summary>
    public TenancyOptions UseResolver(Func<IServiceProvider, ITenantResolver> factory)
    {
        _resolverRegistration = services => services.AddScoped<ITenantResolver>(factory);
        return this;
    }

    /// <summary>
    /// Applies the configured resolver, or falls back to <see cref="ClaimsTenantResolver"/> when none was specified.
    /// Called internally by <see cref="ServiceCollectionExtensions.AddTenancy"/>.
    /// </summary>
    internal void Apply(IServiceCollection services)
    {
        if (_resolverRegistration is not null)
        {
            _resolverRegistration(services);
        }
        else
        {
            services.TryAddScoped<ITenantResolver, ClaimsTenantResolver>();
        }
    }
}
