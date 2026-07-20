using System.Reflection;
using Core.Authentication;
using Infrastructure.Authentication;
using Infrastructure.Database.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        // DbManager artık repository'lerde kullanılmıyor; DbColumn attribute
        // uyumluluğu için tip Infrastructure.Data içinde tutulur.
        services.TryAddScoped<ISqlDataAccess, SqlDataAccess>();
        services.TryAddScoped<IPasswordService, PasswordService>();

        RegisterRepositories(
            services,
            typeof(DependencyInjection).Assembly);

        return services;
    }

    private static void RegisterRepositories(
        IServiceCollection services,
        Assembly assembly)
    {
        var repositoryTypes = assembly
            .DefinedTypes
            .Where(type =>
                type.IsClass
                && !type.IsAbstract
                && !type.ContainsGenericParameters)
            .Where(type =>
                type.Name.EndsWith(
                    "Repository",
                    StringComparison.Ordinal))
            .Select(type => type.AsType());

        foreach (var repositoryType in repositoryTypes)
        {
            services.TryAddScoped(repositoryType);
        }
    }
}
