namespace TheNerdCollective.MudComponents.Shared;

/// <summary>
/// Semantic design-token aliases for design-system catalog chrome (hub, catalogs, PlayBook, demo).
/// Host apps register these via NerdDesignTokenOptions.Alias in token presets.
/// </summary>
public static class NerdDesignSystemUi
{
    public const string MutedContent = "muted-content";
    public const string PrimaryAction = "primary-action";
    public const string PageSurface = "page-surface";
    public const string BrandChrome = "brand-chrome";
    public const string OnBrandChrome = "on-brand-chrome";
    public const string Info = "info";
    public const string Success = "success";
    public const string Danger = "danger";
    public const string Highlight = "highlight";
    public const string SecondaryAction = "secondary-action";
    public const string OnPrimaryAction = "on-primary-action";
    public const string NavSurface = "nav-surface";
    public const string NavItem = "nav-item";
    public const string NavItemActive = "nav-item-active";
    public const string InputSurface = "input-surface";
    public const string InputBorder = "input-border";
    public const string FocusRing = "focus-ring";

    /// <summary>Shell recipe name for app drawer / side navigation.</summary>
    public const string SidebarRecipe = "sidebar";

    /// <summary>Marker class for catalog control chrome on page-surface.</summary>
    public const string CatalogChromeClass = "nerd-catalog-chrome";

    /// <summary><c>data-nerd-style-guard</c> value for catalog chrome placement checks.</summary>
    public const string CatalogChromePlacement = "catalog-chrome";

    /// <summary><c>data-nerd-style-guard</c> value for catalog toolbars (switches, sliders).</summary>
    public const string CatalogToolbarPlacement = "catalog-toolbar";

    public static string TokenClass(string prefix, string semanticAlias) =>
        $"{prefix}-{semanticAlias}";

    public static string TokenClass(NerdDesignSystemOptions options, string semanticAlias) =>
        TokenClass(options.TokenPrefix, semanticAlias);

    public static string RecipeClass(string prefix, string recipeName) =>
        $"{prefix}-recipe-{recipeName}";

    public static string RecipeClass(NerdDesignSystemOptions options, string recipeName) =>
        RecipeClass(options.TokenPrefix, recipeName);

    /// <summary>
    /// Surface background for catalog chrome without applying full token MudBlazor rules to descendants.
    /// </summary>
    public static string PageSurfaceStyle(string prefix) =>
        SurfaceAliasStyle(prefix, PageSurface);

    public static string PageSurfaceStyle(NerdDesignSystemOptions options) =>
        PageSurfaceStyle(options.TokenPrefix);

    public static string NavSurfaceStyle(string prefix) =>
        SurfaceAliasStyle(prefix, NavSurface);

    public static string NavSurfaceStyle(NerdDesignSystemOptions options) =>
        NavSurfaceStyle(options.TokenPrefix);

    private static string SurfaceAliasStyle(string prefix, string aliasName) =>
        $"background-color: var(--{prefix}-color-{aliasName}-surface); color: var(--{prefix}-color-{aliasName}-content);";
}
