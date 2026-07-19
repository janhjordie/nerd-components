namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Maps recipe names to PlayBook layout kit anchors (HR-105).</summary>
public static class NerdRecipePlayBookLinks
{
    private static readonly IReadOnlyDictionary<string, string> LayoutKitAnchors =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["hero"] = "layout-kit-hero",
            ["hero-photo"] = "layout-kit-hero-photo",
            ["hero-organic"] = "layout-kit-hero-organic",
            ["hero-light"] = "layout-kit-hero-light",
            ["sidebar"] = "layout-kit-sidebar",
            ["feature-panel"] = "layout-kit-feature-panel",
            ["cta-strip"] = "layout-kit-cta-strip",
            ["footer-minimal"] = "layout-kit-footer-minimal",
            ["partner-row"] = "layout-kit-partner-row",
            ["formular"] = "layout-kit-formular"
        };

    public static string? TryGetLayoutKitAnchor(string recipeName) =>
        LayoutKitAnchors.TryGetValue(recipeName, out var anchor) ? anchor : null;

    public static string BuildPlayBookUrl(string playBookRoute, string recipeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playBookRoute);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipeName);
        var anchor = TryGetLayoutKitAnchor(recipeName);
        return anchor is null ? playBookRoute : $"{playBookRoute}#{anchor}";
    }
}
