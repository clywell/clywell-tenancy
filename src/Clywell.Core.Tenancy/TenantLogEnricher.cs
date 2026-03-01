using Serilog.Core;
using Serilog.Events;

namespace Clywell.Core.Tenancy;

/// <summary>
/// Serilog <see cref="ILogEventEnricher"/> that appends <c>TenantId</c> to every log event
/// when a tenant has been resolved for the current request scope.
/// </summary>
/// <remarks>
/// Register via <c>Log.Logger = new LoggerConfiguration().Enrich.With&lt;TenantLogEnricher&gt;(...).CreateLogger()</c>.
/// Requires <see cref="ITenantContext"/> to be resolvable from the DI container (i.e., after <c>AddTenancy()</c>).
/// </remarks>
public sealed class TenantLogEnricher(ITenantContext tenantContext) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (tenantContext.TenantId.HasValue)
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("TenantId", tenantContext.TenantId.Value));
        }

        if (tenantContext.TenantName is not null)
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("TenantName", tenantContext.TenantName));
        }
    }
}
