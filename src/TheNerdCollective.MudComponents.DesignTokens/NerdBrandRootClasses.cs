namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Combines MudBlazor brand-root CSS classes for layout shells.</summary>
public static class NerdBrandRootClasses
{
    public static string Combine(string prefix) =>
        string.Join(
            ' ',
            MudBlazorPaletteManifest.BrandRootClass(prefix),
            NerdIntentCssManifest.BrandRootClass(prefix));
}
