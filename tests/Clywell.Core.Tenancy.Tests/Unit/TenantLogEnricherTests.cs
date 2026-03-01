namespace Clywell.Core.Tenancy.Tests.Unit;

public class TenantLogEnricherTests
{
    private static LogEvent CreateLogEvent() =>
        new(DateTimeOffset.UtcNow, LogEventLevel.Information, null,
            new MessageTemplate("Test", []), []);

    [Fact]
    public void Enrich_WhenTenantResolved_AddsTenantIdProperty()
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(new TenantInfo(tenantId, "Acme"));

        var enricher = new TenantLogEnricher(tenantContext);
        var logEvent = CreateLogEvent();
        var propertyFactory = new Mock<Serilog.Core.ILogEventPropertyFactory>();
        propertyFactory
            .Setup(f => f.CreateProperty("TenantId", tenantId, false))
            .Returns(new LogEventProperty("TenantId", new ScalarValue(tenantId)));
        propertyFactory
            .Setup(f => f.CreateProperty("TenantName", "Acme", false))
            .Returns(new LogEventProperty("TenantName", new ScalarValue("Acme")));

        enricher.Enrich(logEvent, propertyFactory.Object);

        Assert.True(logEvent.Properties.ContainsKey("TenantId"));
    }

    [Fact]
    public void Enrich_WhenTenantResolved_AddsTenantNameProperty()
    {
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(new TenantInfo(Guid.NewGuid(), "Acme"));

        var enricher = new TenantLogEnricher(tenantContext);
        var logEvent = CreateLogEvent();
        var propertyFactory = new Mock<Serilog.Core.ILogEventPropertyFactory>();
        propertyFactory
            .Setup(f => f.CreateProperty("TenantId", It.IsAny<object>(), false))
            .Returns(new LogEventProperty("TenantId", new ScalarValue("x")));
        propertyFactory
            .Setup(f => f.CreateProperty("TenantName", "Acme", false))
            .Returns(new LogEventProperty("TenantName", new ScalarValue("Acme")));

        enricher.Enrich(logEvent, propertyFactory.Object);

        Assert.True(logEvent.Properties.ContainsKey("TenantName"));
    }

    [Fact]
    public void Enrich_WhenTenantNotResolved_DoesNotAddProperties()
    {
        var tenantContext = new TenantContext();
        var enricher = new TenantLogEnricher(tenantContext);
        var logEvent = CreateLogEvent();
        var propertyFactory = new Mock<Serilog.Core.ILogEventPropertyFactory>();

        enricher.Enrich(logEvent, propertyFactory.Object);

        Assert.Empty(logEvent.Properties);
        propertyFactory.Verify(f => f.CreateProperty(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public void Enrich_WithNullTenantName_OnlyAddsTenantId()
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(new TenantInfo(tenantId));

        var enricher = new TenantLogEnricher(tenantContext);
        var logEvent = CreateLogEvent();
        var propertyFactory = new Mock<Serilog.Core.ILogEventPropertyFactory>();
        propertyFactory
            .Setup(f => f.CreateProperty("TenantId", tenantId, false))
            .Returns(new LogEventProperty("TenantId", new ScalarValue(tenantId)));

        enricher.Enrich(logEvent, propertyFactory.Object);

        Assert.True(logEvent.Properties.ContainsKey("TenantId"));
        Assert.False(logEvent.Properties.ContainsKey("TenantName"));
    }
}
