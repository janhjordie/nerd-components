using System.Text;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Blazorise adapter — reuses Bootstrap 5 <c>--bs-*</c> bridge (HR-116 / TS-016).
/// </summary>
public static class NerdBlazoriseDesignTokenCssGenerator
{
    public static void AppendBlazoriseBrandPalette(StringBuilder css, NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(css);

        NerdBootstrapDesignTokenCssGenerator.AppendBootstrapBrandPalette(css, options);

        var blazoriseRoot = $".{NerdBlazorisePaletteManifest.BrandRootClass(options.Prefix)}";
        var bootstrapRoot = $".{NerdBootstrapPaletteManifest.BrandRootClass(options.Prefix)}";
        css.AppendLine($"/* Blazorise {NerdBlazorisePaletteManifest.BlazoriseVersion}: apply {blazoriseRoot} with {bootstrapRoot} */");
    }
}
