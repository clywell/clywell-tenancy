namespace Clywell.Core.Tenancy.Tests.Unit;

public class UserLogEnricherTests
{
    private static LogEvent CreateLogEvent() =>
        new(DateTimeOffset.UtcNow, LogEventLevel.Information, null,
            new MessageTemplate("Test", []), []);

    private static IHttpContextAccessor CreateAccessor(HttpContext? context)
    {
        var mock = new Mock<IHttpContextAccessor>();
        mock.Setup(a => a.HttpContext).Returns(context);
        return mock.Object;
    }

    [Fact]
    public void Enrich_AuthenticatedWithSubClaim_AddsUserId()
    {
        const string userId = "user-123";
        var identity = new ClaimsIdentity([new Claim("sub", userId)], "Bearer");
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        var accessor = CreateAccessor(httpContext);

        var enricher = new UserLogEnricher(accessor);
        var logEvent = CreateLogEvent();
        var propertyFactory = new Mock<Serilog.Core.ILogEventPropertyFactory>();
        propertyFactory
            .Setup(f => f.CreateProperty("UserId", userId, false))
            .Returns(new LogEventProperty("UserId", new ScalarValue(userId)));

        enricher.Enrich(logEvent, propertyFactory.Object);

        Assert.True(logEvent.Properties.ContainsKey("UserId"));
    }

    [Fact]
    public void Enrich_AuthenticatedWithNameIdentifierFallback_AddsUserId()
    {
        const string userId = "user-456";
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId)], "Bearer");
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        var accessor = CreateAccessor(httpContext);

        var enricher = new UserLogEnricher(accessor);
        var logEvent = CreateLogEvent();
        var propertyFactory = new Mock<Serilog.Core.ILogEventPropertyFactory>();
        propertyFactory
            .Setup(f => f.CreateProperty("UserId", userId, false))
            .Returns(new LogEventProperty("UserId", new ScalarValue(userId)));

        enricher.Enrich(logEvent, propertyFactory.Object);

        Assert.True(logEvent.Properties.ContainsKey("UserId"));
    }

    [Fact]
    public void Enrich_Unauthenticated_DoesNotAddUserId()
    {
        var httpContext = new DefaultHttpContext();
        var accessor = CreateAccessor(httpContext);

        var enricher = new UserLogEnricher(accessor);
        var logEvent = CreateLogEvent();
        var propertyFactory = new Mock<Serilog.Core.ILogEventPropertyFactory>();

        enricher.Enrich(logEvent, propertyFactory.Object);

        Assert.Empty(logEvent.Properties);
        propertyFactory.Verify(f => f.CreateProperty(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public void Enrich_NullHttpContext_DoesNotThrow()
    {
        var accessor = CreateAccessor(null);

        var enricher = new UserLogEnricher(accessor);
        var logEvent = CreateLogEvent();
        var propertyFactory = new Mock<Serilog.Core.ILogEventPropertyFactory>();

        var exception = Record.Exception(() => enricher.Enrich(logEvent, propertyFactory.Object));
        Assert.Null(exception);
    }
}
