using Microsoft.Extensions.DependencyInjection;

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
        services.AddSingleton<IThemeJsonFilePersistence, NullThemeJsonFilePersistence>();
        services.AddScoped<IMudThemeSessionStore, MudThemeSessionStore>();
        services.AddScoped<IMudThemeStateService, MudThemeStateService>();
        services.AddSingleton<IThemeEditorGate, ThemeEditorGate>();

        return services;
    }
}
