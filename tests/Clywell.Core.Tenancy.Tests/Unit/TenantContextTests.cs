namespace Clywell.Core.Tenancy.Tests.Unit;

public class TenantContextTests
{
    [Fact]
    public void IsResolved_WhenTenantNotSet_ReturnsFalse()
    {
        var context = new TenantContext();

        Assert.False(context.IsResolved);
        Assert.Null(context.TenantId);
        Assert.Null(context.TenantName);
    }

    [Fact]
    public void SetTenant_WithIdOnly_SetsIdAndResolved()
    {
        var context = new TenantContext();
        var tenantId = Guid.NewGuid();

        context.SetTenant(new TenantInfo(tenantId));

        Assert.True(context.IsResolved);
        Assert.Equal(tenantId, context.TenantId);
        Assert.Null(context.TenantName);
    }

    [Fact]
    public void SetTenant_WithIdAndName_SetsBothProperties()
    {
        var context = new TenantContext();
        var tenantId = Guid.NewGuid();
        const string tenantName = "Acme Corp";

        context.SetTenant(new TenantInfo(tenantId, tenantName));

        Assert.Equal(tenantId, context.TenantId);
        Assert.Equal(tenantName, context.TenantName);
    }

    [Fact]
    public void SetTenant_CalledTwice_OverwritesPreviousValues()
    {
        var context = new TenantContext();
        var firstTenantId = Guid.NewGuid();
        var secondTenantId = Guid.NewGuid();

        context.SetTenant(new TenantInfo(firstTenantId, "First"));
        context.SetTenant(new TenantInfo(secondTenantId, "Second"));

        Assert.Equal(secondTenantId, context.TenantId);
        Assert.Equal("Second", context.TenantName);
    }

    [Fact]
    public void SetTenant_StoresFullTenantInfo()
    {
        var context = new TenantContext();
        var tenantInfo = new TenantInfo(Guid.NewGuid(), "Acme Corp");

        context.SetTenant(tenantInfo);

        Assert.Same(tenantInfo, context.TenantInfo);
    }

    [Fact]
    public void ITenantContext_ExposesSetValues()
    {
        var context = new TenantContext();
        var tenantId = Guid.NewGuid();
        context.SetTenant(new TenantInfo(tenantId, "My Tenant"));

        ITenantContext iface = context;

        Assert.Equal(tenantId, iface.TenantId);
        Assert.Equal("My Tenant", iface.TenantName);
        Assert.True(iface.IsResolved);
        Assert.NotNull(iface.TenantInfo);
    }
}
