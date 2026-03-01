using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Clywell.Core.Tenancy;

/// <summary>
/// Serilog <see cref="ILogEventEnricher"/> that appends <c>UserId</c> to every log event
/// for the current authenticated user.
/// </summary>
/// <remarks>
/// Reads the <c>sub</c> claim (standard JWT subject) or falls back to <c>NameIdentifier</c>.
/// Register alongside <see cref="TenantLogEnricher"/> for full per-request enrichment.
/// </remarks>
public sealed class UserLogEnricher(IHttpContextAccessor httpContextAccessor) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var userId =
            user.FindFirst("sub")?.Value ??
            user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (userId is not null)
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("UserId", userId));
        }
    }
}
