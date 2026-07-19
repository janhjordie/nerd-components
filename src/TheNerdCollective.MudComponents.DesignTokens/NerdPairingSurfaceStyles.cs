namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Shared class, CSS variables and inline styles for token pairing preview surfaces (content on surface).
/// MudBlazor chrome inside these surfaces is normalized by generated CSS.
/// </summary>
public static class NerdPairingSurfaceStyles
{
    public const string ClassName = "nerd-pairing-surface";
    public const string SwatchClassName = "nerd-pairing-surface--swatch";
    public const string StudioClassName = "nerd-pairing-surface--studio";

    public const string SurfaceColorVariable = "--nerd-pairing-surface-color";
    public const string ContentColorVariable = "--nerd-pairing-content-color";

    public static string Create(string surfaceColor, string contentColor) =>
        $"{SurfaceColorVariable}:{surfaceColor};{ContentColorVariable}:{contentColor};" +
        $"background-color:var({SurfaceColorVariable});color:var({ContentColorVariable});";

    public static string ForVariant(
        NerdPairingSurfaceVariant variant,
        string surfaceColor,
        string contentColor) =>
        variant switch
        {
            NerdPairingSurfaceVariant.Swatch => Create(surfaceColor, contentColor),
            NerdPairingSurfaceVariant.Studio => Create(surfaceColor, contentColor),
            _ => Create(surfaceColor, contentColor)
        };

    public static string ClassFor(NerdPairingSurfaceVariant variant, string? additionalClass = null)
    {
        var classes = new List<string> { ClassName };
        switch (variant)
        {
            case NerdPairingSurfaceVariant.Swatch:
                classes.Add(SwatchClassName);
                break;
            case NerdPairingSurfaceVariant.Studio:
                classes.Add(StudioClassName);
                break;
        }

        if (!string.IsNullOrWhiteSpace(additionalClass))
        {
            classes.Add(additionalClass);
        }

        return string.Join(' ', classes);
    }
}
