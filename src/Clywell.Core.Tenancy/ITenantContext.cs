namespace Clywell.Core.Tenancy;

/// <summary>
/// Provides read access to the current tenant context for the request scope.
/// Populated by <see cref="TenantResolutionMiddleware"/> from the incoming JWT.
/// </summary>
public interface ITenantContext
{
    /// <summary>Unique identifier of the resolved tenant. <see langword="null"/> when no tenant has been resolved.</summary>
    public Guid? TenantId { get; }

    /// <summary>Display name of the resolved tenant. May be <see langword="null"/> when not included in the JWT claims.</summary>
    public string? TenantName { get; }

    /// <summary><see langword="true"/> when a tenant has been successfully resolved for this request.</summary>
    public bool IsResolved { get; }

    /// <summary>
    /// The full <see cref="TenantInfo"/> object produced by <see cref="ITenantResolver"/>.
    /// Cast to your own subtype to access additional properties populated by a custom resolver.
    /// </summary>
    public TenantInfo? TenantInfo { get; }
}
