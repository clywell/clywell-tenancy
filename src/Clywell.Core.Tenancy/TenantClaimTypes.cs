namespace Clywell.Core.Tenancy;

/// <summary>JWT claim name constants used when resolving the tenant from an incoming request.</summary>
public static class TenantClaimTypes
{
    /// <summary>Standard Azure AD / Entra ID tenant identifier claim.</summary>
    public const string TenantId = "tid";

    /// <summary>Fallback claim name for custom tenant identifiers.</summary>
    public const string TenantIdAlternate = "tenantid";

    /// <summary>Claim carrying the human-readable tenant name.</summary>
    public const string TenantName = "tenant_name";
}
