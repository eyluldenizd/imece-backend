using Core.Authentication;
using Core.Authorization;
using ImeceWebAPI.Authentication.Options;
using Infrastructure.Authentication.Directory;

namespace ImeceWebAPI.Authentication;

/// <summary>
/// Development ortamı için varsayılan kullanıcı senaryoları. Configuration'da
/// profil tanımlanmamışsa bu varsayılanlar uygulanır; böylece proje ekstra
/// ayar gerektirmeden çeşitli kimlik/yetki senaryolarıyla çalışabilir.
/// Authentication profilleri (dış kimlik) ile dizin profilleri (uygulama
/// profili) bilinçli olarak ayrı tutulur.
/// </summary>
public static class DevelopmentAuthenticationDefaults
{
    private const int CompanyA = 1001;
    private const int CompanyB = 2002;
    private const string CompanyAName = "Company A";
    private const string CompanyBName = "Company B";

    // Profil anahtarı → simüle edilen dış kimlik (ExternalId).
    public static readonly IReadOnlyDictionary<string, string> ProfileExternalIds =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ad-only-user"] = "S-1-5-21-ad-only",
            ["registered-user"] = "S-1-5-21-registered",
            ["company-admin"] = "S-1-5-21-company-admin",
            ["global-admin"] = "S-1-5-21-global-admin",
            ["user-without-company"] = "S-1-5-21-no-company",
            ["disabled-user"] = "S-1-5-21-disabled",
            ["company-a-user"] = "S-1-5-21-company-a",
            ["company-b-user"] = "S-1-5-21-company-b",
            ["multi-company-user"] = "S-1-5-21-multi-company"
        };

    public static Dictionary<string, DevelopmentUserProfile> CreateAuthenticationProfiles()
    {
        var profiles = new Dictionary<string, DevelopmentUserProfile>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var (key, externalId) in ProfileExternalIds)
        {
            profiles[key] = new DevelopmentUserProfile
            {
                IdentityProvider = ImeceIdentityProviders.Development,
                ExternalId = externalId,
                Username = key,
                Email = $"{key}@imece.local",
                DisplayName = ToDisplayName(key),
                DomainOrTenant = "IMECE"
            };
        }

        return profiles;
    }

    public static Dictionary<string, DirectoryProfileOptions> CreateDirectoryProfiles()
    {
        // Not: "ad-only-user" bilinçli olarak dizine eklenmez → kimlik
        // sağlayıcıda var ama uygulamaya kayıtsız (AD-only) senaryosu.
        return new Dictionary<string, DirectoryProfileOptions>(
            StringComparer.OrdinalIgnoreCase)
        {
            [ProfileExternalIds["registered-user"]] = new()
            {
                UserId = 5001,
                Username = "registered-user",
                CompanyId = CompanyA,
                CompanyName = CompanyAName,
                Roles = [Roles.User]
            },
            [ProfileExternalIds["company-admin"]] = new()
            {
                UserId = 5002,
                Username = "company-admin",
                CompanyId = CompanyA,
                CompanyName = CompanyAName,
                Roles = [Roles.CompanyAdmin]
            },
            [ProfileExternalIds["global-admin"]] = new()
            {
                UserId = 5003,
                Username = "global-admin",
                Roles = [Roles.GlobalAdmin]
            },
            [ProfileExternalIds["user-without-company"]] = new()
            {
                UserId = 5004,
                Username = "user-without-company",
                CompanyId = null,
                Roles = [Roles.User]
            },
            [ProfileExternalIds["disabled-user"]] = new()
            {
                UserId = 5005,
                Username = "disabled-user",
                IsActive = false,
                CompanyId = CompanyA,
                CompanyName = CompanyAName,
                Roles = [Roles.User]
            },
            [ProfileExternalIds["company-a-user"]] = new()
            {
                UserId = 5006,
                Username = "company-a-user",
                CompanyId = CompanyA,
                CompanyName = CompanyAName,
                Roles = [Roles.User]
            },
            [ProfileExternalIds["company-b-user"]] = new()
            {
                UserId = 5007,
                Username = "company-b-user",
                CompanyId = CompanyB,
                CompanyName = CompanyBName,
                Roles = [Roles.User]
            },
            // Global admin OLMADAN çoklu şirket yetkili kullanıcı: CompanyA'da
            // CompanyAdmin, CompanyB'de User. UserCompanyRoles senaryosu.
            [ProfileExternalIds["multi-company-user"]] = new()
            {
                UserId = 5008,
                Username = "multi-company-user",
                CompanyRoles =
                [
                    new DirectoryCompanyRoleOptions
                    {
                        CompanyId = CompanyA,
                        CompanyName = CompanyAName,
                        Roles = [Roles.CompanyAdmin]
                    },
                    new DirectoryCompanyRoleOptions
                    {
                        CompanyId = CompanyB,
                        CompanyName = CompanyBName,
                        Roles = [Roles.User]
                    }
                ]
            }
        };
    }

    private static string ToDisplayName(string key) =>
        string.Join(
            ' ',
            key.Split('-')
                .Select(part =>
                    part.Length == 0
                        ? part
                        : char.ToUpperInvariant(part[0]) + part[1..]));
}
