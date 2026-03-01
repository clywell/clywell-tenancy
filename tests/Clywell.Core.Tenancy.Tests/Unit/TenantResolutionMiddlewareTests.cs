namespace Clywell.Core.Tenancy.Tests.Unit;

public class TenantResolutionMiddlewareTests
{
    private static Mock<ITenantResolver> ResolverReturning(TenantInfo? info)
    {
        var mock = new Mock<ITenantResolver>();
        mock.Setup(r => r.ResolveAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(info);
        return mock;
    }

    [Fact]
    public async Task InvokeAsync_ResolverReturnsTenantInfo_SetsTenantContext()
    {
        var tenantId = Guid.NewGuid();
        var resolver = ResolverReturning(new TenantInfo(tenantId, "Acme"));
        var tenantContext = new TenantContext();
        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(new DefaultHttpContext(), tenantContext, resolver.Object);

        Assert.True(tenantContext.IsResolved);
        Assert.Equal(tenantId, tenantContext.TenantId);
        Assert.Equal("Acme", tenantContext.TenantName);
    }

    [Fact]
    public async Task InvokeAsync_ResolverReturnsNull_ContextNotPopulated()
    {
        var resolver = ResolverReturning(null);
        var tenantContext = new TenantContext();
        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(new DefaultHttpContext(), tenantContext, resolver.Object);

        Assert.False(tenantContext.IsResolved);
    }

    [Fact]
    public async Task InvokeAsync_AlwaysCallsNext_EvenWhenResolverReturnsNull()
    {
        var resolver = ResolverReturning(null);
        var tenantContext = new TenantContext();
        var nextCalled = false;
        var middleware = new TenantResolutionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(new DefaultHttpContext(), tenantContext, resolver.Object);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_AlwaysCallsNext_EvenWhenResolverSucceeds()
    {
        var resolver = ResolverReturning(new TenantInfo(Guid.NewGuid()));
        var tenantContext = new TenantContext();
        var nextCalled = false;
        var middleware = new TenantResolutionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(new DefaultHttpContext(), tenantContext, resolver.Object);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_CallsResolverOnce()
    {
        var resolver = ResolverReturning(null);
        var tenantContext = new TenantContext();
        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(new DefaultHttpContext(), tenantContext, resolver.Object);

        resolver.Verify(r => r.ResolveAsync(It.IsAny<HttpContext>()), Times.Once);
    }
}

