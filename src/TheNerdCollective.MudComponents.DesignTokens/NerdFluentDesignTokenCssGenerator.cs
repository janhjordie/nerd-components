using System.Text;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Maps nerd intent variables to Fluent UI Blazor design tokens (HR-117 / TS-017 spike).
/// </summary>
public static class NerdFluentDesignTokenCssGenerator
{
    public static void AppendFluentBrandPalette(StringBuilder css, NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(css);

        var map = NerdFluentBlazorPaletteMap.CreateConventionBindings();
        var important = options.UseImportantOverrides ? " !important" : string.Empty;
        var nerdRoot = $".{NerdIntentCssManifest.BrandRootClass(options.Prefix)}";
        var fluentRoot = $".{NerdFluentBlazorPaletteManifest.BrandRootClass(options.Prefix)}";

        css.AppendLine($"{nerdRoot}, {fluentRoot} {{");
        css.AppendLine($"  --colorBrandBackground: {map.BrandBackground}{important};");
        css.AppendLine($"  --colorBrandForeground1: {map.BrandForeground}{important};");
        css.AppendLine($"  --colorNeutralBackground1: {map.NeutralBackground}{important};");
        css.AppendLine($"  --colorNeutralForeground1: {map.NeutralForeground}{important};");
        css.AppendLine($"  --colorNeutralForeground2: {map.NeutralForegroundSecondary}{important};");
        css.AppendLine($"  --colorNeutralStroke1: {map.NeutralStroke}{important};");
        css.AppendLine($"  --colorStrokeFocus1: {map.FocusStroke}{important};");
        css.AppendLine("}");
        css.AppendLine();
    }
}
