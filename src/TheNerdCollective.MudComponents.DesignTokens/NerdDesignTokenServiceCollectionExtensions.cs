using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

public static class NerdDesignTokenServiceCollectionExtensions
{
    public static IServiceCollection AddNerdTokenPackStore(
        this IServiceCollection services,
        string directory = "App_Data/token-packs")
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<INerdTokenPackStore>(
            _ => new FileNerdTokenPackStore(directory));
        return services;
    }

    public static IServiceCollection AddNerdDesignTokens(
        this IServiceCollection services,
        Action<NerdDesignTokenOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new NerdDesignTokenOptions();
        configure(options);

        if (string.IsNullOrWhiteSpace(options.DefaultBrandPackId) &&
            !string.IsNullOrWhiteSpace(options.ActiveBrandPackId))
        {
            options.DefaultBrandPackId = options.ActiveBrandPackId;
        }

        services.AddNerdDesignSystem(hub =>
        {
            hub.DesignTokensRoute = options.CatalogRoute;
            hub.DesignTokenRecipesRoute = options.RecipesCatalogRoute;
            hub.DesignTokenCount = options.Colors.Count;
            hub.DesignTokenRecipeCount = options.Recipes.Count;
            hub.TokenPrefix = options.Prefix;
            hub.ActiveTokenPackId = options.ActiveBrandPackId;
            hub.ActiveBrandIdentityVersion = options.ActiveBrandIdentityVersion;
        });
        services.AddSingleton(options);
        services.TryAddSingleton<INerdTokenPackStore>(
            _ => new FileNerdTokenPackStore("App_Data/token-packs"));
        services.AddSingleton(sp => new NerdDesignTokenCss(
            MudBlazorDesignTokenCssGenerator.Generate(sp.GetRequiredService<NerdDesignTokenOptions>())));
        services.TryAddSingleton<INerdMudThemeConfigurator, NullNerdMudThemeConfigurator>();
        services.TryAddSingleton<INerdMudThemeController, NerdMudThemeController>();
        services.TryAddSingleton<INerdBrandPackImportSink, NerdBrandPackImportSink>();

        if (options.Colors.Count > 0 && options.WarnOnAccessibilityFailuresAtStartup)
        {
            services.AddHostedService<NerdDesignTokenAccessibilityStartupValidator>();
        }

        return services;
    }
}
