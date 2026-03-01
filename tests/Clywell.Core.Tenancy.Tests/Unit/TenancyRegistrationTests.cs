namespace Clywell.Core.Tenancy.Tests.Unit;

public class TenancyRegistrationTests
{
    [Fact]
    public void AddTenancy_WithoutConfigure_RegistersClaimsTenantResolverAsDefault()
    {
        var services = new ServiceCollection();
        services.AddTenancy();

        var sp = services.BuildServiceProvider();
        var resolver = sp.GetRequiredService<ITenantResolver>();

        Assert.IsType<ClaimsTenantResolver>(resolver);
    }

    [Fact]
    public void AddTenancy_WithUseResolverType_RegistersCustomResolver()
    {
        var services = new ServiceCollection();
        services.AddTenancy(options => options.UseResolver<StubTenantResolver>());

        var sp = services.BuildServiceProvider();
        var resolver = sp.GetRequiredService<ITenantResolver>();

        Assert.IsType<StubTenantResolver>(resolver);
    }

    [Fact]
    public void AddTenancy_WithUseResolverFactory_RegistersFactoryResolver()
    {
        var marker = new StubTenantResolver();
        var services = new ServiceCollection();
        services.AddTenancy(options => options.UseResolver(_ => marker));

        var sp = services.BuildServiceProvider();
        var resolver = sp.GetRequiredService<ITenantResolver>();

        Assert.Same(marker, resolver);
    }

    [Fact]
    public void AddTenancy_CustomResolver_DoesNotRegisterClaimsResolver()
    {
        var services = new ServiceCollection();
        services.AddTenancy(options => options.UseResolver<StubTenantResolver>());

        // Only one ITenantResolver registration; it must be the custom type
        var sp = services.BuildServiceProvider();
        var resolvers = sp.GetServices<ITenantResolver>().ToList();

        Assert.Single(resolvers);
        Assert.IsType<StubTenantResolver>(resolvers[0]);
    }

    [Fact]
    public void AddTenancy_CallsTenancy_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();
        var result = services.AddTenancy();

        Assert.Same(services, result);
    }

    [Fact]
    public void AddTenancy_RegistersITenantContext()
    {
        var services = new ServiceCollection();
        services.AddTenancy();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITenantContext));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddTenancy_RegistersTenantLogEnricher()
    {
        var services = new ServiceCollection();
        services.AddTenancy();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(TenantLogEnricher));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddTenancy_RegistersUserLogEnricher()
    {
        var services = new ServiceCollection();
        services.AddTenancy();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(UserLogEnricher));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    private sealed class StubTenantResolver : ITenantResolver
    {
        public Task<TenantInfo?> ResolveAsync(HttpContext context) =>
            Task.FromResult<TenantInfo?>(null);
    }
}
