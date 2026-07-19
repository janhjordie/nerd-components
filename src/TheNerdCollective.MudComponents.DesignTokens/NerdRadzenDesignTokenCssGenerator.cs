using System.Text;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Maps nerd intent variables to Radzen Blazor theme CSS variables (HR-115 / TS-015 spike).
/// </summary>
public static class NerdRadzenDesignTokenCssGenerator
{
    public static void AppendRadzenBrandPalette(StringBuilder css, NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(css);

        var map = NerdRadzenPaletteMap.CreateConventionBindings();
        var important = options.UseImportantOverrides ? " !important" : string.Empty;
        var nerdRoot = $".{NerdIntentCssManifest.BrandRootClass(options.Prefix)}";
        var radzenRoot = $".{NerdRadzenPaletteManifest.BrandRootClass(options.Prefix)}";

        css.AppendLine($"{nerdRoot}, {radzenRoot} {{");
        css.AppendLine($"  --rz-primary: {map.Primary}{important};");
        css.AppendLine($"  --rz-secondary: {map.Secondary}{important};");
        css.AppendLine($"  --rz-success: {map.Success}{important};");
        css.AppendLine($"  --rz-info: {map.Info}{important};");
        css.AppendLine($"  --rz-warning: {map.Warning}{important};");
        css.AppendLine($"  --rz-danger: {map.Danger}{important};");
        css.AppendLine($"  --rz-body-background-color: {map.BodyBackground}{important};");
        css.AppendLine($"  --rz-text-color: {map.TextColor}{important};");
        css.AppendLine($"  --rz-text-secondary-color: {map.TextSecondaryColor}{important};");
        css.AppendLine("}");
        css.AppendLine();

        RadzenComponentRuleBuilder.AppendRules(css, options);
    }
}
