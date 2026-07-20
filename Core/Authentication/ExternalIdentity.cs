namespace Core.Authentication;

public sealed class ExternalIdentity
{
    public required string IdentityProvider { get; init; }

    public required string ExternalId { get; init; }

    public string? DomainOrTenant { get; init; }

    public string? Username { get; init; }

    public string? Email { get; init; }

    public string? DisplayName { get; init; }
}

public interface IExternalIdentityAccessor
{
    ExternalIdentity? GetExternalIdentity();
}
