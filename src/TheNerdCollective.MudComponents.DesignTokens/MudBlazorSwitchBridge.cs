using System.Text;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Inventory-backed MudSwitch bridges (HR-161). Mud SCSS hardcodes thumb #fafafa — override via token vars.
/// </summary>
internal static class MudBlazorSwitchBridge
{
    public static void AppendRules(
        StringBuilder css,
        string root,
        string variable,
        string textVariable,
        string contentVariable,
        string inactiveTabContentVariable,
        string hoverVariable,
        string borderVariable,
        string pageSurfaceVariable,
        string importantSuffix,
        string switchThumbVariable,
        string? switchCheckedTrackBackground,
        string switchTrackBorderVariable)
    {
        var trackMixBase = string.IsNullOrWhiteSpace(pageSurfaceVariable)
            ? "transparent"
            : $"var({pageSurfaceVariable})";
        var checkedTrackBackground = switchCheckedTrackBackground ?? $"var({variable})";

        css.AppendLine($"{root}[data-nerd-token] :where(.mud-switch) + .mud-typography,");
        css.AppendLine($"{root} .mud-switch + .mud-typography,");
        css.AppendLine($"{root} .mud-switch .mud-typography:not(.mud-tab),");
        css.AppendLine($"{root} :where(.mud-switch) + .mud-typography,");
        css.AppendLine($"{root} :where(.mud-switch) .mud-typography:not(.mud-tab) {{");
        css.AppendLine($"  color: var({contentVariable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root}[data-nerd-token] :where(.mud-switch) .mud-switch-track,");
        css.AppendLine($"{root}.mud-switch .mud-switch-track,");
        css.AppendLine($"{root} :where(.mud-switch) .mud-switch-track {{");
        css.AppendLine($"  border: 1px solid var({switchTrackBorderVariable}){importantSuffix};");
        css.AppendLine($"  background-color: color-mix(in srgb, var({variable}) 30%, {trackMixBase}){importantSuffix};");
        css.AppendLine($"  opacity: 1{importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root}[data-nerd-token] :where(.mud-switch) .mud-switch-base.mud-checked + .mud-switch-track,");
        css.AppendLine($"{root}.mud-switch .mud-switch-base.mud-checked + .mud-switch-track,");
        css.AppendLine($"{root} :where(.mud-switch) .mud-switch-base.mud-checked + .mud-switch-track {{");
        css.AppendLine($"  background-color: {checkedTrackBackground}{importantSuffix};");
        css.AppendLine($"  border-color: var({switchTrackBorderVariable}){importantSuffix};");
        css.AppendLine($"  opacity: 1{importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root}[data-nerd-token] :where(.mud-switch) .mud-switch-base.mud-button-root,");
        css.AppendLine($"{root}.mud-switch .mud-switch-base.mud-button-root,");
        css.AppendLine($"{root} :where(.mud-switch) .mud-switch-base.mud-button-root {{");
        css.AppendLine($"  background-color: transparent{importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root}[data-nerd-token] :where(.mud-switch) .mud-switch-thumb-small,");
        css.AppendLine($"{root}[data-nerd-token] :where(.mud-switch) .mud-switch-thumb-medium,");
        css.AppendLine($"{root}[data-nerd-token] :where(.mud-switch) .mud-switch-thumb-large,");
        css.AppendLine($"{root}[data-nerd-token] :where(.mud-switch) .mud-switch-thumb,");
        css.AppendLine($"{root}[data-nerd-token] :where(.mud-switch) .mud-switch-button .mud-switch-thumb-small,");
        css.AppendLine($"{root}[data-nerd-token] :where(.mud-switch) .mud-switch-button .mud-switch-thumb-medium,");
        css.AppendLine($"{root}[data-nerd-token] :where(.mud-switch) .mud-switch-button .mud-switch-thumb-large,");
        css.AppendLine($"{root}.mud-switch .mud-switch-thumb-small, {root}.mud-switch .mud-switch-thumb-medium,");
        css.AppendLine($"{root}.mud-switch .mud-switch-thumb-large, {root}.mud-switch .mud-switch-thumb,");
        css.AppendLine($"{root}.mud-switch .mud-switch-button .mud-switch-thumb-small, {root}.mud-switch .mud-switch-button .mud-switch-thumb-medium,");
        css.AppendLine($"{root}.mud-switch .mud-switch-button .mud-switch-thumb-large,");
        css.AppendLine($"{root} :where(.mud-switch) .mud-switch-thumb-small, {root} :where(.mud-switch) .mud-switch-thumb-medium,");
        css.AppendLine($"{root} :where(.mud-switch) .mud-switch-thumb-large, {root} :where(.mud-switch) .mud-switch-thumb,");
        css.AppendLine($"{root} :where(.mud-switch) .mud-switch-button .mud-switch-thumb-small, {root} :where(.mud-switch) .mud-switch-button .mud-switch-thumb-medium,");
        css.AppendLine($"{root} :where(.mud-switch) .mud-switch-button .mud-switch-thumb-large {{");
        css.AppendLine($"  background-color: var({switchThumbVariable}){importantSuffix};");
        css.AppendLine($"  color: var({switchThumbVariable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-tabs .mud-tab:not(.mud-tab-active),");
        css.AppendLine($"{root}.mud-tabs .mud-tab:not(.mud-tab-active),");
        css.AppendLine($"{root} .mud-tab:not(.mud-tab-active) {{");
        css.AppendLine($"  color: var({inactiveTabContentVariable}){importantSuffix};");
        css.AppendLine($"  background-color: transparent{importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-tabs .mud-tab.mud-tab-active,");
        css.AppendLine($"{root}.mud-tabs .mud-tab.mud-tab-active,");
        css.AppendLine($"{root} .mud-tab.mud-tab-active,");
        css.AppendLine($"{root} .mud-tab.mud-tab-active .mud-tab-icon-text {{");
        css.AppendLine($"  color: var({variable}){importantSuffix};");
        css.AppendLine($"  background-color: transparent{importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-tabs .mud-tab-slider,");
        css.AppendLine($"{root} .mud-tab-slider {{");
        css.AppendLine($"  background-color: var({variable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-tab:hover:not(.mud-tab-active),");
        css.AppendLine($"{root}.mud-tab:hover:not(.mud-tab-active) {{");
        css.AppendLine($"  background-color: color-mix(in srgb, var({variable}) 8%, transparent){importantSuffix};");
        css.AppendLine($"  color: var({inactiveTabContentVariable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-switch-base, {root}.mud-switch-base {{");
        css.AppendLine($"  color: var({textVariable}){importantSuffix};");
        css.AppendLine("}");
    }
}
