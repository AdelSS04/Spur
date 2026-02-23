using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Spur.AspNetCore.Options;
using System.Diagnostics.CodeAnalysis;

namespace Spur.AspNetCore;

/// <summary>
/// Extension methods for configuring Spur services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Spur services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSpur(
        this IServiceCollection services,
        Action<SpurOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new SpurOptions();
        configureOptions?.Invoke(options);

        services.TryAddSingleton(options);
        services.TryAddSingleton<IProblemDetailsMapper, DefaultProblemDetailsMapper>();

        return services;
    }

    /// <summary>
    /// Adds Spur services with a custom Problem Details mapper.
    /// </summary>
    /// <typeparam name="TMapper">The custom mapper type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2091", Justification = "TMapper constraint ensures public constructor availability")]
    public static IServiceCollection AddSpur<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TMapper>(
        this IServiceCollection services,
        Action<SpurOptions>? configureOptions = null)
        where TMapper : class, IProblemDetailsMapper
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new SpurOptions();
        configureOptions?.Invoke(options);

        services.TryAddSingleton(options);
        services.TryAddSingleton<IProblemDetailsMapper, TMapper>();

        return services;
    }
}
