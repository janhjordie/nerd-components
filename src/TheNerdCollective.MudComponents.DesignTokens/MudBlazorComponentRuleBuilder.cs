using System.Text;

namespace TheNerdCollective.MudComponents.DesignTokens;

internal static class MudBlazorComponentRuleBuilder
{
    private static readonly string[] FilledPatterns =
    [
        "mud-button-filled", "mud-chip", "mud-alert-filled", "mud-fab", "mud-avatar-filled",
        "mud-badge", "mud-progress-linear", "mud-progress-circular", "mud-snackbar",
        "mud-pagination-item-selected", "mud-stepper-step-primary", "mud-primary",
        "mud-secondary", "mud-tertiary", "mud-info", "mud-success", "mud-warning", "mud-error",
        "mud-theme-primary", "mud-theme-secondary", "mud-theme-tertiary", "mud-theme-info",
        "mud-theme-success", "mud-theme-warning", "mud-theme-error"
    ];

    private static readonly string[] OutlinedPatterns =
    [
        "mud-button-outlined", "mud-chip-outlined", "mud-alert-outlined", "mud-avatar-outlined",
        "mud-fab-outlined", "mud-border-primary", "mud-border-secondary", "mud-border-tertiary",
        "mud-border-info", "mud-border-success", "mud-border-warning", "mud-border-error"
    ];

    private static readonly string[] TextPatterns =
    [
        "mud-button-text", "mud-alert-text", "mud-link", "mud-typography", "mud-icon-button",
        "mud-nav-link", "mud-tab", "mud-breadcrumb-item", "mud-list-item", "mud-menu-item",
        "mud-treeview-item-content", "mud-timeline-item-content", "mud-primary-text",
        "mud-secondary-text", "mud-tertiary-text", "mud-info-text", "mud-success-text",
        "mud-warning-text", "mud-error-text", "mud-button-root", "mud-fab-color-inherit",
        "mud-icon-button-color-inherit", "mud-button-color-inherit", "mud-button-text-inherit",
        "mud-button-outlined-inherit"
    ];

    private static readonly string[] InputPatterns =
    [
        "mud-input", "mud-input-root", "mud-input-control", "mud-input-slot", "mud-input-text",
        "mud-input-label", "mud-input-adornment", "mud-select", "mud-autocomplete",
        "mud-text-field", "mud-numeric-field", "mud-picker", "mud-checkbox", "mud-radio",
        "mud-switch", "mud-slider", "mud-slider-input", "mud-rating", "mud-form"
    ];

    private static readonly string[] StructurePatterns =
    [
        "mud-table", "mud-table-row", "mud-table-cell", "mud-data-grid", "mud-datagrid",
        "mud-card", "mud-paper", "mud-expansion-panel", "mud-dialog", "mud-drawer",
        "mud-appbar", "mud-toolbar", "mud-tooltip", "mud-popover", "mud-carousel",
        "mud-timeline", "mud-stepper", "mud-treeview", "mud-pagination", "mud-chart",
        "mud-skeleton", "mud-divider", "mud-grid", "mud-container", "mud-list"
    ];

    public static void AppendRules(
        StringBuilder css,
        string root,
        string variable,
        string textVariable,
        string hoverVariable,
        string activeVariable,
        string borderVariable,
        string disabledVariable,
        bool important)
    {
        var importantSuffix = important ? " !important" : string.Empty;

        AppendPatternRules(css, root, FilledPatterns,
            $"background-color: var({variable}){importantSuffix}; color: var({textVariable}){importantSuffix};");

        AppendPatternRules(css, root, OutlinedPatterns,
            $"color: var({variable}){importantSuffix}; border-color: var({borderVariable}){importantSuffix};");

        AppendPatternRules(css, root, TextPatterns,
            $"color: var({variable}){importantSuffix};");

        AppendPatternRules(css, root, InputPatterns,
            $"color: var({textVariable}){importantSuffix}; caret-color: var({variable}){importantSuffix}; border-color: var({borderVariable}){importantSuffix};");

        AppendPatternRules(css, root, StructurePatterns,
            $"color: var({textVariable}){importantSuffix}; border-color: var({borderVariable}){importantSuffix};");

        css.AppendLine($"{root} .mud-checked, {root}.mud-checked,");
        css.AppendLine($"{root} .mud-selected, {root}.mud-selected,");
        css.AppendLine($"{root} .mud-radio.mud-checked .mud-radio-content, {root}.mud-radio.mud-checked .mud-radio-content,");
        css.AppendLine($"{root} .mud-switch.mud-checked .mud-switch-thumb, {root}.mud-switch.mud-checked .mud-switch-thumb {{");
        css.AppendLine($"  background-color: var({variable}){importantSuffix};");
        css.AppendLine($"  color: var({textVariable}){importantSuffix};");
        css.AppendLine($"  border-color: var({variable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-switch.mud-checked + .mud-switch-track, {root}.mud-switch.mud-checked + .mud-switch-track {{");
        css.AppendLine($"  background-color: var({hoverVariable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-rating-root, {root}.mud-rating-root,");
        css.AppendLine($"{root} .mud-rating-item, {root}.mud-rating-item {{");
        css.AppendLine($"  color: var({variable}){importantSuffix};");
        css.AppendLine($"  --mud-ripple-color: var({variable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root}:focus-visible, {root}:focus-within, {root} .mud-button:focus, {root}.mud-button:focus,");
        css.AppendLine($"{root} .mud-button:active, {root}.mud-button:active,");
        css.AppendLine($"{root} .mud-chip:active, {root}.mud-chip:active,");
        css.AppendLine($"{root} .mud-expanded, {root}.mud-expanded, {root}[aria-pressed=\"true\"] {{");
        css.AppendLine($"  outline-color: var({activeVariable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-disabled, {root}.mud-disabled, {root}[disabled], {root}[aria-disabled=\"true\"],");
        css.AppendLine($"{root} .mud-button:disabled, {root}.mud-button:disabled,");
        css.AppendLine($"{root} .mud-chip.mud-disabled, {root}.mud-chip.mud-disabled {{");
        css.AppendLine($"  color: var({disabledVariable}){importantSuffix};");
        css.AppendLine($"  background-color: transparent{importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-button-filled:hover, {root}.mud-button-filled:hover,");
        css.AppendLine($"{root} .mud-button-outlined:hover, {root}.mud-button-outlined:hover,");
        css.AppendLine($"{root} .mud-button-text:hover, {root}.mud-button-text:hover,");
        css.AppendLine($"{root} .mud-chip:hover, {root}.mud-chip:hover,");
        css.AppendLine($"{root} .mud-list-item-clickable:hover, {root}.mud-list-item-clickable:hover,");
        css.AppendLine($"{root} .mud-nav-link:hover, {root}.mud-nav-link:hover,");
        css.AppendLine($"{root} .mud-tab:hover, {root}.mud-tab:hover {{");
        css.AppendLine($"  background-color: var({hoverVariable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine();
    }

    private static void AppendPatternRules(
        StringBuilder css,
        string root,
        IReadOnlyList<string> patterns,
        string declarations)
    {
        if (patterns.Count == 0)
        {
            return;
        }

        for (var i = 0; i < patterns.Count; i++)
        {
            var pattern = patterns[i];
            css.Append($"{root}[class*=\"{pattern}\"], {root} [class*=\"{pattern}\"]");
            css.AppendLine(i == patterns.Count - 1 ? " {" : ",");
        }

        css.AppendLine($"  {declarations}");
        css.AppendLine("}");
    }
}
