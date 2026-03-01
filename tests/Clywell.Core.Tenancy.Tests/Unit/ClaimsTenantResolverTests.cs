
namespace Clywell.Core.Tenancy.Tests.Unit;

public class ClaimsTenantResolverTests
{
    private static DefaultHttpContext CreateAuthenticatedContext(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "Bearer");
        return new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
    }

    private static DefaultHttpContext CreateUnauthenticatedContext() => new();

    [Fact]
    public async Task ResolveAsync_AuthenticatedWithTidClaim_ReturnsTenantInfo()
    {
        var tenantId = Guid.NewGuid();
        var context = CreateAuthenticatedContext(
            new Claim(TenantClaimTypes.TenantId, tenantId.ToString()));

        var result = await new ClaimsTenantResolver().ResolveAsync(context);

        Assert.NotNull(result);
        Assert.Equal(tenantId, result.TenantId);
    }

    [Fact]
    public async Task ResolveAsync_AuthenticatedWithFallbackClaim_ReturnsTenantInfo()
    {
        var tenantId = Guid.NewGuid();
        var context = CreateAuthenticatedContext(
            new Claim(TenantClaimTypes.TenantIdAlternate, tenantId.ToString()));

        var result = await new ClaimsTenantResolver().ResolveAsync(context);

        Assert.NotNull(result);
        Assert.Equal(tenantId, result.TenantId);
    }

    [Fact]
    public async Task ResolveAsync_AuthenticatedWithTenantNameClaim_SetsTenantName()
    {
        var tenantId = Guid.NewGuid();
        var context = CreateAuthenticatedContext(
            new Claim(TenantClaimTypes.TenantId, tenantId.ToString()),
            new Claim(TenantClaimTypes.TenantName, "Acme Corp"));

        var result = await new ClaimsTenantResolver().ResolveAsync(context);

        Assert.Equal("Acme Corp", result?.TenantName);
    }

    [Fact]
    public async Task ResolveAsync_AuthenticatedWithNoTenantNameClaim_TenantNameIsNull()
    {
        var tenantId = Guid.NewGuid();
        var context = CreateAuthenticatedContext(
            new Claim(TenantClaimTypes.TenantId, tenantId.ToString()));

        var result = await new ClaimsTenantResolver().ResolveAsync(context);

        Assert.NotNull(result);
        Assert.Null(result.TenantName);
    }

    [Fact]
    public async Task ResolveAsync_AuthenticatedWithInvalidTenantId_ReturnsNull()
    {
        var context = CreateAuthenticatedContext(
            new Claim(TenantClaimTypes.TenantId, "not-a-guid"));

        var result = await new ClaimsTenantResolver().ResolveAsync(context);

        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveAsync_AuthenticatedWithNoTenantClaims_ReturnsNull()
    {
        var context = CreateAuthenticatedContext(
            new Claim(ClaimTypes.Name, "alice"));

        var result = await new ClaimsTenantResolver().ResolveAsync(context);

        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveAsync_Unauthenticated_ReturnsNull()
    {
        var result = await new ClaimsTenantResolver().ResolveAsync(CreateUnauthenticatedContext());

        Assert.Null(result);
    }
}
