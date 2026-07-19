using System.Text;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Maps nerd intent variables to Bootstrap 5 theme CSS variables (HR-153 / shared Blazorise line).
/// </summary>
public static class NerdBootstrapDesignTokenCssGenerator
{
    public static void AppendBootstrapBrandPalette(StringBuilder css, NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(css);

        var map = NerdBootstrapPaletteMap.CreateConventionBindings();
        var important = options.UseImportantOverrides ? " !important" : string.Empty;
        var nerdRoot = $".{NerdIntentCssManifest.BrandRootClass(options.Prefix)}";
        var bootstrapRoot = $".{NerdBootstrapPaletteManifest.BrandRootClass(options.Prefix)}";

        css.AppendLine($"{nerdRoot}, {bootstrapRoot} {{");
        css.AppendLine($"  --bs-primary: {map.Primary}{important};");
        css.AppendLine($"  --bs-secondary: {map.Secondary}{important};");
        css.AppendLine($"  --bs-success: {map.Success}{important};");
        css.AppendLine($"  --bs-info: {map.Info}{important};");
        css.AppendLine($"  --bs-warning: {map.Warning}{important};");
        css.AppendLine($"  --bs-danger: {map.Danger}{important};");
        css.AppendLine($"  --bs-body-bg: {map.BodyBackground}{important};");
        css.AppendLine($"  --bs-body-color: {map.BodyColor}{important};");
        css.AppendLine($"  --bs-border-color: {map.BorderColor}{important};");
        css.AppendLine($"  --bs-link-color: {map.LinkColor}{important};");
        css.AppendLine("}");
        css.AppendLine();
    }
}
