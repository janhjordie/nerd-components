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

    public static string TokenClass(string prefix, string semanticAlias) =>
        $"{prefix}-{semanticAlias}";

    public static string TokenClass(NerdDesignSystemOptions options, string semanticAlias) =>
        TokenClass(options.TokenPrefix, semanticAlias);

    /// <summary>
    /// Surface background for catalog chrome without applying full token MudBlazor rules to descendants.
    /// </summary>
    public static string PageSurfaceStyle(string prefix) =>
        $"background-color: var(--{prefix}-color-page-surface-surface); color: var(--{prefix}-color-page-surface-content);";

    public static string PageSurfaceStyle(NerdDesignSystemOptions options) =>
        PageSurfaceStyle(options.TokenPrefix);
}
