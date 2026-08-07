using Microsoft.Extensions.DependencyInjection;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.Changelog;

public static class NerdChangelogServiceCollectionExtensions
{
    public static IServiceCollection AddNerdChangelog(
        this IServiceCollection services,
        Action<NerdChangelogOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new NerdChangelogOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<NerdChangelogService>();
        services.AddNerdDesignSystem(hub => hub.ChangelogRoute = options.ChangelogRoute);

        return services;
    }
}
