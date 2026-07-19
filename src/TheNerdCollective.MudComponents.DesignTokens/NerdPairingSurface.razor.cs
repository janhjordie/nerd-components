using Microsoft.AspNetCore.Components;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Renders a pairing preview surface (content color on surface color) with MudBlazor-safe chrome.
/// </summary>
public partial class NerdPairingSurface
{
    [Parameter]
    public string SurfaceColor { get; set; } = string.Empty;

    [Parameter]
    public string ContentColor { get; set; } = string.Empty;

    [Parameter]
    public NerdTokenPairingValidation? Validation { get; set; }

    [Parameter]
    public NerdPairingSurfaceVariant Variant { get; set; } = NerdPairingSurfaceVariant.Default;

    [Parameter]
    public string? DataTheme { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string ResolvedSurfaceColor =>
        Validation?.SurfaceColor ?? SurfaceColor;

    private string ResolvedContentColor =>
        Validation?.ContentColor ?? ContentColor;

    private string RootClass =>
        NerdPairingSurfaceStyles.ClassFor(Variant, Class);

    private string Style =>
        NerdPairingSurfaceStyles.ForVariant(Variant, ResolvedSurfaceColor, ResolvedContentColor);
}
