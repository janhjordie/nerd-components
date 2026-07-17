using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TheNerdCollective.Blazor.ThemeKit;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.PlayBook;

public static class NerdPlayBookServiceCollectionExtensions
{
    public static IServiceCollection AddNerdPlayBook(
        this IServiceCollection services,
        Action<NerdPlayBookOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new NerdPlayBookOptions();
        configure?.Invoke(options);

        services.AddNerdDesignSystem(hub => hub.PlayBookRoute = options.PlayBookRoute);
        services.AddSingleton(options);

        if (options.EnableThemeKit)
        {
            RegisterThemeKit(services, options);
        }

        return services;
    }

    private static void RegisterThemeKit(IServiceCollection services, NerdPlayBookOptions options)
    {
        services.AddSingleton<NerdPlayBookDefaultThemeCatalog>();
        services.AddSingleton<IReloadableMudThemeCatalog>(sp =>
        {
            var fallback = sp.GetRequiredService<NerdPlayBookDefaultThemeCatalog>();
            var themesDirectory = NerdPlayBookThemePathResolver.ResolveThemesDirectory(options);
            return new JsonFileMudThemeCatalog(themesDirectory, fallback);
        });
        services.AddSingleton<IMudThemeCatalog>(sp => sp.GetRequiredService<IReloadableMudThemeCatalog>());

        services.AddMudThemeKit(kit =>
        {
            kit.PlaybookMode = options.ThemeKitPlaybookMode;
            kit.DefaultThemeId = options.DefaultThemeId;
        });

        services.AddSingleton<IThemeJsonFilePersistence>(sp =>
        {
            var catalog = sp.GetRequiredService<IReloadableMudThemeCatalog>();
            var themesDirectory = NerdPlayBookThemePathResolver.ResolveThemesDirectory(options);
            return new FileThemeJsonFilePersistence(themesDirectory, catalog);
        });
    }
}
