using System.Reflection;
using Application.Common.Execution;
using Core.Common.Execution;
using Core.Common.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        var applicationAssembly =
            typeof(DependencyInjection).Assembly;

        services.TryAddScoped<
            IServiceExecutor,
            ServiceExecutor>();

        services.TryAddScoped(
            typeof(IValidator<>),
            typeof(DynamicValidator<>));

        services.RegisterApplicationServices(
            applicationAssembly);

        return services;
    }

    private static void RegisterApplicationServices(
        this IServiceCollection services,
        Assembly assembly)
    {
        var serviceTypes =
            assembly
                .DefinedTypes
                .Where(type =>
                    type.IsClass
                    && !type.IsAbstract
                    && !type.ContainsGenericParameters)
                .Where(type =>
                    type.Name.EndsWith(
                        "Service",
                        StringComparison.Ordinal))
                .Where(type =>
                    type != typeof(ServiceExecutor))
                .Select(type => type.AsType())
                .ToList();

        foreach (var implementationType in serviceTypes)
        {
            RegisterService(
                services,
                implementationType);
        }
    }

    private static void RegisterService(
        IServiceCollection services,
        Type implementationType)
    {
        var expectedInterfaceName =
            $"I{implementationType.Name}";

        var serviceInterface =
            implementationType
                .GetInterfaces()
                .FirstOrDefault(interfaceType =>
                    interfaceType.Name.Equals(
                        expectedInterfaceName,
                        StringComparison.Ordinal));

        if (serviceInterface is not null)
        {
            services.TryAdd(
                ServiceDescriptor.Scoped(
                    serviceInterface,
                    implementationType));
        }

        services.TryAdd(
            ServiceDescriptor.Scoped(
                implementationType,
                implementationType));
    }
}