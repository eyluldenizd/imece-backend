namespace Core.Authentication;

public static class ImeceClaimTypes
{
    public const string IdentityProvider = "imece:idp";
    public const string ExternalId = "imece:external_id";
    public const string DomainOrTenant = "imece:domain";
    public const string Username = "imece:username";
    public const string Email = "imece:email";
    public const string DisplayName = "imece:display_name";
}

public static class ImeceIdentityProviders
{
    public const string Development = "development";
    public const string Local = "local";
    public const string Test = "test";
    public const string Windows = "windows";
    public const string EntraId = "entra_id";
}
