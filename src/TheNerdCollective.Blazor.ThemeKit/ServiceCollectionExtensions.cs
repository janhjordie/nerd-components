using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TheNerdCollective.Blazor.ThemeKit;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMudThemeKit(
        this IServiceCollection services,
        Action<MudThemeKitOptions>? configure = null)
    {
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.AddSingleton<MudThemePreferencesService>();
        services.TryAddSingleton<IThemeJsonFilePersistence, NullThemeJsonFilePersistence>();
        services.AddScoped<IMudThemeSessionStore, MudThemeSessionStore>();
        services.AddScoped<IMudThemeStateService, MudThemeStateService>();
        services.AddSingleton<IThemeEditorGate, ThemeEditorGate>();

        return services;
    }
}
