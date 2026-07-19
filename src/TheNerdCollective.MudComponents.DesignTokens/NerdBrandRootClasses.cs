namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Combines framework brand-root CSS classes for layout shells.</summary>
public static class NerdBrandRootClasses
{
    public static string Combine(string prefix) =>
        string.Join(
            ' ',
            MudBlazorPaletteManifest.BrandRootClass(prefix),
            NerdIntentCssManifest.BrandRootClass(prefix),
            NerdFluentBlazorPaletteManifest.BrandRootClass(prefix),
            NerdRadzenPaletteManifest.BrandRootClass(prefix),
            NerdBootstrapPaletteManifest.BrandRootClass(prefix),
            NerdBlazorisePaletteManifest.BrandRootClass(prefix));
}
