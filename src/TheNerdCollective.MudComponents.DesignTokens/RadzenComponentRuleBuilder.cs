using System.Text;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Maps Radzen component selectors to nerd Radzen theme variables (HR-115).</summary>
internal static class RadzenComponentRuleBuilder
{
    public static void AppendRules(StringBuilder css, NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(css);

        var root = $".{NerdRadzenPaletteManifest.BrandRootClass(options.Prefix)}";
        var important = options.UseImportantOverrides ? " !important" : string.Empty;

        css.AppendLine($"{root} .rz-button.rz-primary {{");
        css.AppendLine($"  background-color: var(--rz-primary){important};");
        css.AppendLine($"  border-color: var(--rz-primary){important};");
        css.AppendLine($"  color: #fff{important};");
        css.AppendLine("}");
        css.AppendLine($"{root} .rz-button.rz-secondary {{");
        css.AppendLine($"  background-color: var(--rz-secondary){important};");
        css.AppendLine($"  border-color: var(--rz-secondary){important};");
        css.AppendLine($"  color: #fff{important};");
        css.AppendLine("}");
        css.AppendLine($"{root} .rz-button.rz-danger {{");
        css.AppendLine($"  background-color: var(--rz-danger){important};");
        css.AppendLine($"  border-color: var(--rz-danger){important};");
        css.AppendLine($"  color: #fff{important};");
        css.AppendLine("}");
        css.AppendLine($"{root} .rz-badge.rz-primary {{ background-color: var(--rz-primary){important}; color: #fff{important}; }}");
        css.AppendLine($"{root} .rz-textbox, {root} .rz-textarea, {root} .rz-dropdown {{");
        css.AppendLine($"  background-color: var(--rz-body-background-color){important};");
        css.AppendLine($"  color: var(--rz-text-color){important};");
        css.AppendLine("}");
        css.AppendLine($"{root} .rz-card {{");
        css.AppendLine($"  background-color: var(--rz-body-background-color){important};");
        css.AppendLine($"  color: var(--rz-text-color){important};");
        css.AppendLine("}");
        css.AppendLine();
    }
}
