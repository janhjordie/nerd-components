using MudBlazor;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Builds catalog Mud themes from token packs when no host <see cref="INerdMudThemeController"/> is present.</summary>
public static class NerdCatalogThemeResolver
{
    public static MudTheme Create(
        NerdDesignTokenOptions options,
        INerdMudThemeConfigurator? configurator = null) =>
        NerdMudThemeFactory.Create(options, configurator is null ? null : configurator.Configure);

    public static MudTheme CreateForCatalog(
        NerdDesignTokenOptions options,
        INerdMudThemeController? themeController,
        Action<MudTheme>? configure = null,
        INerdMudThemeConfigurator? configurator = null)
    {
        if (themeController is not null)
        {
            return themeController.CurrentTheme;
        }

        return NerdMudThemeFactory.Create(options, theme =>
        {
            configurator?.Configure(theme);
            configure?.Invoke(theme);
        });
    }
}
