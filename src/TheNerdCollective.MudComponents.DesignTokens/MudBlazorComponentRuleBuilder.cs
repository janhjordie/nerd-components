using System.Text;

namespace TheNerdCollective.MudComponents.DesignTokens;

internal static class MudBlazorComponentRuleBuilder
{
    private static readonly string[] FilledPatterns =
    [
        "mud-button-filled", "mud-chip-filled", "mud-alert-filled", "mud-fab", "mud-avatar-filled",
        "mud-badge", "mud-progress-linear", "mud-progress-circular", "mud-snackbar",
        "mud-pagination-item-selected", "mud-stepper-step-primary", "mud-theme-primary",
        "mud-theme-secondary", "mud-theme-tertiary", "mud-theme-info", "mud-theme-success",
        "mud-theme-warning", "mud-theme-error"
    ];

    private static readonly string[] OutlinedPatterns =
    [
        "mud-button-outlined", "mud-chip-outlined", "mud-alert-outlined", "mud-avatar-outlined",
        "mud-fab-outlined", "mud-border-primary", "mud-border-secondary", "mud-border-tertiary",
        "mud-border-info", "mud-border-success", "mud-border-warning", "mud-border-error"
    ];

    private static readonly string[] AccentTextPatterns =
    [
        "mud-button-text", "mud-link", "mud-icon", "mud-icon-button", "mud-nav-link",
        "mud-breadcrumb-item", "mud-list-item", "mud-menu-item", "mud-treeview-item-content",
        "mud-timeline-item-content", "mud-fab-color-inherit", "mud-icon-button-color-inherit",
        "mud-button-color-inherit", "mud-button-text-inherit", "mud-button-outlined-inherit"
    ];

    private static readonly string[] ChannelTextPatterns =
    [
        "mud-primary-text", "mud-secondary-text", "mud-tertiary-text", "mud-info-text",
        "mud-success-text", "mud-warning-text", "mud-error-text"
    ];

    private static readonly string[] ContentTextPatterns =
    [
        "mud-alert-text", "mud-typography"
    ];

    private static readonly string[] InputPatterns =
    [
        "mud-input", "mud-input-root", "mud-input-control", "mud-input-slot", "mud-input-text",
        "mud-input-label", "mud-input-adornment", "mud-select", "mud-autocomplete",
        "mud-text-field", "mud-numeric-field", "mud-picker", "mud-checkbox", "mud-radio",
        "mud-slider", "mud-slider-input", "mud-rating", "mud-form"
    ];

    private static readonly string[] DataSurfacePatterns =
    [
        "mud-table", "mud-table-row", "mud-table-cell", "mud-data-grid", "mud-datagrid", "mud-simple-table"
    ];

    private static readonly string[] StructurePatterns =
    [
        "mud-card", "mud-paper", "mud-expansion-panel", "mud-expansion-panels", "mud-expand-panel", "mud-dialog", "mud-drawer",
        "mud-appbar", "mud-toolbar", "mud-tooltip", "mud-popover", "mud-carousel",
        "mud-timeline", "mud-stepper", "mud-treeview", "mud-pagination", "mud-chart",
        "mud-skeleton", "mud-divider", "mud-grid", "mud-container", "mud-list", "mud-button-group",
        "mud-file-upload", "mud-fab-menu", "mud-drop-zone",
        "mud-overlay", "mud-collapse", "mud-image"
    ];

    public static void AppendRules(
        StringBuilder css,
        string root,
        string variable,
        string textVariable,
        string contentVariable,
        string hoverVariable,
        string activeVariable,
        string borderVariable,
        string disabledVariable,
        string pageSurfaceVariable,
        bool important,
        bool bridgesOnly = false,
        string? inactiveTabContentVariable = null,
        string? switchThumbVariable = null,
        string? switchCheckedTrackBackground = null,
        string? switchTrackBorderVariable = null,
        string? inputValueVariable = null,
        string? inputBorderMixVariable = null)
    {
        var importantSuffix = important ? " !important" : string.Empty;
        var resolvedInactiveTabContent = inactiveTabContentVariable ?? contentVariable;
        var resolvedSwitchThumb = switchThumbVariable ?? textVariable;
        var resolvedSwitchTrackBorder = switchTrackBorderVariable ?? hoverVariable;
        var resolvedInputValue = inputValueVariable ?? contentVariable;
        var resolvedInputBorderMix = inputBorderMixVariable ?? variable;

        if (!bridgesOnly)
        {
            AppendBulkPatternRules(
                css,
                root,
                variable,
                textVariable,
                contentVariable,
                hoverVariable,
                activeVariable,
                borderVariable,
                disabledVariable,
                importantSuffix);
        }

        AppendBridgeRules(
            css,
            root,
            variable,
            textVariable,
            contentVariable,
            resolvedInactiveTabContent,
            hoverVariable,
            borderVariable,
            pageSurfaceVariable,
            importantSuffix,
            bridgesOnly,
            resolvedSwitchThumb,
            switchCheckedTrackBackground,
            resolvedSwitchTrackBorder,
            resolvedInputValue,
            resolvedInputBorderMix);
    }

    private static void AppendBulkPatternRules(
        StringBuilder css,
        string root,
        string variable,
        string textVariable,
        string contentVariable,
        string hoverVariable,
        string activeVariable,
        string borderVariable,
        string disabledVariable,
        string importantSuffix)
    {
        AppendPatternRules(css, root, FilledPatterns,
            $"background-color: var({variable}){importantSuffix}; color: var({textVariable}){importantSuffix};");

        AppendPatternRules(css, root, OutlinedPatterns,
            $"color: var({variable}){importantSuffix}; border-color: var({borderVariable}){importantSuffix}; background-color: transparent{importantSuffix};");

        css.AppendLine($"{root}[class*=\"mud-button-outlined\"] .mud-button-label,");
        css.AppendLine($"{root} :where([class*=\"mud-button-outlined\"]) .mud-button-label {{");
        css.AppendLine($"  color: inherit{importantSuffix};");
        css.AppendLine("}");

        AppendPatternRules(css, root, AccentTextPatterns,
            $"color: var({variable}){importantSuffix}; background-color: transparent{importantSuffix};");

        css.AppendLine($"{root} .mud-nav-link .mud-nav-link-text,");
        css.AppendLine($"{root} .mud-nav-link .mud-nav-link-icon,");
        css.AppendLine($"{root} .mud-nav-link .mud-icon-root,");
        css.AppendLine($"{root}.mud-nav-link .mud-nav-link-text,");
        css.AppendLine($"{root}.mud-nav-link .mud-nav-link-icon,");
        css.AppendLine($"{root}.mud-nav-link .mud-icon-root {{");
        css.AppendLine($"  color: inherit{importantSuffix};");
        css.AppendLine("}");

        AppendPatternRules(css, root, ContentTextPatterns,
            $"color: var({contentVariable}){importantSuffix}; background-color: transparent{importantSuffix};");

        AppendPatternRules(css, root, ChannelTextPatterns,
            $"color: var({variable}){importantSuffix}; background-color: transparent{importantSuffix};");

        AppendPatternRules(css, root, InputPatterns,
            $"color: var({contentVariable}){importantSuffix}; caret-color: var({variable}){importantSuffix}; border-color: var({borderVariable}){importantSuffix};");

        var surfaceVariable = contentVariable.Replace("-content", "-surface", StringComparison.Ordinal);
        AppendPatternRules(css, root, DataSurfacePatterns,
            $"background-color: var({surfaceVariable}){importantSuffix}; color: var({contentVariable}){importantSuffix}; border-color: var({borderVariable}){importantSuffix};");

        AppendPatternRules(css, root, StructurePatterns,
            $"color: var({contentVariable}){importantSuffix}; border-color: var({borderVariable}){importantSuffix};");

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
        css.AppendLine($"{root} .mud-list-item-clickable:hover, {root}.mud-list-item-clickable:hover {{");
        css.AppendLine($"  background-color: var({hoverVariable}){importantSuffix};");
        css.AppendLine($"  color: var({textVariable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-nav-link:hover, {root}.mud-nav-link:hover,");
        css.AppendLine($"{root} .mud-nav-link.active, {root}.mud-nav-link.active {{");
        css.AppendLine($"  background-color: var({hoverVariable}){importantSuffix};");
        css.AppendLine($"  color: var({textVariable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-nav-link:hover .mud-nav-link-text,");
        css.AppendLine($"{root} .mud-nav-link:hover .mud-nav-link-icon,");
        css.AppendLine($"{root} .mud-nav-link:hover .mud-icon-root,");
        css.AppendLine($"{root} .mud-nav-link.active .mud-nav-link-text,");
        css.AppendLine($"{root} .mud-nav-link.active .mud-nav-link-icon,");
        css.AppendLine($"{root} .mud-nav-link.active .mud-icon-root,");
        css.AppendLine($"{root}.mud-nav-link:hover .mud-nav-link-text,");
        css.AppendLine($"{root}.mud-nav-link:hover .mud-nav-link-icon,");
        css.AppendLine($"{root}.mud-nav-link:hover .mud-icon-root,");
        css.AppendLine($"{root}.mud-nav-link.active .mud-nav-link-text,");
        css.AppendLine($"{root}.mud-nav-link.active .mud-nav-link-icon,");
        css.AppendLine($"{root}.mud-nav-link.active .mud-icon-root {{");
        css.AppendLine($"  color: var({textVariable}){importantSuffix};");
        css.AppendLine($"  background-color: transparent{importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-chip-filled:hover, {root}.mud-chip-filled:hover {{");
        css.AppendLine($"  background-color: var({hoverVariable}){importantSuffix};");
        css.AppendLine($"  color: var({textVariable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-chip-outlined:hover, {root}.mud-chip-outlined:hover {{");
        css.AppendLine($"  background-color: color-mix(in srgb, var({variable}) 12%, transparent){importantSuffix};");
        css.AppendLine($"  color: var({variable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root}.mud-button-outlined:hover, {root} .mud-button-outlined:hover,");
        css.AppendLine($"{root}.mud-button-outlined:hover .mud-button-label, {root} .mud-button-outlined:hover .mud-button-label,");
        css.AppendLine($"{root}.mud-button-text:hover, {root} .mud-button-text:hover {{");
        css.AppendLine($"  background-color: color-mix(in srgb, var({variable}) 12%, transparent){importantSuffix};");
        css.AppendLine($"  color: var({variable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-switch-base:hover,");
        css.AppendLine($"{root} .mud-checkbox .mud-icon-button:hover,");
        css.AppendLine($"{root} .mud-radio .mud-icon-button:hover,");
        css.AppendLine($"{root} .mud-ripple-switch:hover,");
        css.AppendLine($"{root} .mud-ripple-checkbox:hover,");
        css.AppendLine($"{root} .mud-ripple-radio:hover {{");
        css.AppendLine($"  background-color: color-mix(in srgb, var({variable}) 8%, transparent){importantSuffix};");
        css.AppendLine("}");
    }

    private static void AppendBridgeRules(
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
        bool bridgesOnly,
        string switchThumbVariable,
        string? switchCheckedTrackBackground,
        string switchTrackBorderVariable,
        string inputValueVariable,
        string inputBorderMixVariable)
    {
        MudBlazorSwitchBridge.AppendRules(
            css,
            root,
            variable,
            textVariable,
            contentVariable,
            inactiveTabContentVariable,
            hoverVariable,
            borderVariable,
            pageSurfaceVariable,
            importantSuffix,
            switchThumbVariable,
            switchCheckedTrackBackground,
            switchTrackBorderVariable);

        AppendInputStateRules(
            css,
            root,
            variable,
            inputValueVariable,
            inputBorderMixVariable,
            pageSurfaceVariable,
            importantSuffix);

        AppendPickerPortalStateRules(
            css,
            root,
            variable,
            textVariable,
            contentVariable,
            hoverVariable,
            importantSuffix);

        AppendToggleGroupStateRules(
            css,
            root,
            variable,
            textVariable,
            hoverVariable,
            borderVariable,
            importantSuffix);

        var surfaceVariable = contentVariable.Replace("-content", "-surface", StringComparison.Ordinal);
        css.AppendLine($"{root}[class*=\"mud-popover\"], {root} :where([class*=\"mud-popover\"]) {{");
        css.AppendLine($"  background-color: var({surfaceVariable}){importantSuffix};");
        css.AppendLine($"  color: var({contentVariable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root}[class*=\"mud-popover\"] [class*=\"mud-list-item\"],");
        css.AppendLine($"{root}[class*=\"mud-popover\"] [class*=\"mud-menu-item\"],");
        css.AppendLine($"{root}[class*=\"mud-popover\"] .mud-typography {{");
        css.AppendLine($"  color: inherit{importantSuffix};");
        css.AppendLine($"  background-color: transparent{importantSuffix};");
        css.AppendLine("}");

        if (bridgesOnly)
        {
            css.AppendLine($"{root} .mud-nav-link .mud-nav-link-text,");
            css.AppendLine($"{root} .mud-nav-link .mud-nav-link-icon,");
            css.AppendLine($"{root} .mud-nav-link .mud-icon-root,");
            css.AppendLine($"{root}.mud-nav-link .mud-nav-link-text,");
            css.AppendLine($"{root}.mud-nav-link .mud-nav-link-icon,");
            css.AppendLine($"{root}.mud-nav-link .mud-icon-root {{");
            css.AppendLine($"  color: inherit{importantSuffix};");
            css.AppendLine("}");

            // Palette-first: Mud Color.Default reads palette slots; bridges map intent tokens to component paint.
            css.AppendLine($"{root}.mud-button-filled {{");
            css.AppendLine($"  background-color: var({variable}){importantSuffix};");
            css.AppendLine($"  color: var({textVariable}){importantSuffix};");
            css.AppendLine("}");
            css.AppendLine($"{root}.mud-button-filled:hover {{");
            css.AppendLine($"  background-color: var({hoverVariable}){importantSuffix};");
            css.AppendLine($"  color: var({textVariable}){importantSuffix};");
            css.AppendLine("}");

            css.AppendLine($"{root}.mud-button-outlined {{");
            css.AppendLine($"  color: var({variable}){importantSuffix};");
            css.AppendLine($"  border-color: var({borderVariable}){importantSuffix};");
            css.AppendLine($"  background-color: transparent{importantSuffix};");
            css.AppendLine("}");
            css.AppendLine($"{root}.mud-button-outlined .mud-button-label {{");
            css.AppendLine($"  color: inherit{importantSuffix};");
            css.AppendLine("}");
            css.AppendLine($"{root}.mud-button-outlined:hover {{");
            css.AppendLine($"  background-color: color-mix(in srgb, var({variable}) 12%, transparent){importantSuffix};");
            css.AppendLine($"  color: var({variable}){importantSuffix};");
            css.AppendLine("}");

            css.AppendLine($"{root}.mud-chip-outlined {{");
            css.AppendLine($"  color: var({variable}){importantSuffix};");
            css.AppendLine($"  border-color: var({borderVariable}){importantSuffix};");
            css.AppendLine($"  background-color: transparent{importantSuffix};");
            css.AppendLine("}");
            css.AppendLine($"{root}.mud-chip-outlined:hover {{");
            css.AppendLine($"  background-color: color-mix(in srgb, var({variable}) 12%, transparent){importantSuffix};");
            css.AppendLine($"  color: var({variable}){importantSuffix};");
            css.AppendLine("}");

            css.AppendLine($"{root}.mud-chip-filled {{");
            css.AppendLine($"  background-color: var({variable}){importantSuffix};");
            css.AppendLine($"  color: var({textVariable}){importantSuffix};");
            css.AppendLine($"  border-color: var({variable}){importantSuffix};");
            css.AppendLine("}");
            css.AppendLine($"{root}.mud-chip-filled:hover {{");
            css.AppendLine($"  background-color: var({hoverVariable}){importantSuffix};");
            css.AppendLine($"  color: var({textVariable}){importantSuffix};");
            css.AppendLine("}");
        }

        if (!bridgesOnly)
        {
            css.AppendLine($"{root} .mud-checked:not(.mud-switch-base):not(.mud-ripple-radio):not(.mud-ripple-checkbox),");
            css.AppendLine($"{root}.mud-checked:not(.mud-switch-base):not(.mud-ripple-radio):not(.mud-ripple-checkbox),");
            css.AppendLine($"{root} .mud-selected, {root}.mud-selected {{");
            css.AppendLine($"  background-color: var({variable}){importantSuffix};");
            css.AppendLine($"  color: var({textVariable}){importantSuffix};");
            css.AppendLine($"  border-color: var({variable}){importantSuffix};");
            css.AppendLine("}");
        }

        css.AppendLine($"{root} .mud-checkbox .mud-icon-button, {root}.mud-checkbox .mud-icon-button,");
        css.AppendLine($"{root} .mud-radio .mud-icon-button, {root}.mud-radio .mud-icon-button,");
        css.AppendLine($"{root}[data-nerd-token] :where(.mud-switch) .mud-switch-base,");
        css.AppendLine($"{root}.mud-switch .mud-switch-base,");
        css.AppendLine($"{root} :where(.mud-switch) .mud-switch-base {{");
        css.AppendLine($"  color: var({variable}){importantSuffix};");
        css.AppendLine($"  --mud-ripple-color: var({variable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-rating-root, {root}.mud-rating-root,");
        css.AppendLine($"{root} .mud-rating-item, {root}.mud-rating-item {{");
        css.AppendLine($"  color: var({variable}){importantSuffix};");
        css.AppendLine($"  --mud-ripple-color: var({variable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-chip-content, {root}.mud-chip-content {{");
        css.AppendLine($"  background-color: transparent{importantSuffix};");
        css.AppendLine("}");

        css.AppendLine();
    }

    private static void AppendInputStateRules(
        StringBuilder css,
        string root,
        string variable,
        string inputValueVariable,
        string inputBorderMixVariable,
        string pageSurfaceVariable,
        string importantSuffix)
    {
        var pageMixBase = string.IsNullOrWhiteSpace(pageSurfaceVariable)
            ? "transparent"
            : $"var({pageSurfaceVariable})";

        css.AppendLine($"{root}[data-nerd-token] :where([class*=\"mud-input-label\"]),");
        css.AppendLine($"{root}.mud-text-field :where([class*=\"mud-input-label\"]),");
        css.AppendLine($"{root}.mud-numeric-field :where([class*=\"mud-input-label\"]),");
        css.AppendLine($"{root}.mud-select :where([class*=\"mud-input-label\"]),");
        css.AppendLine($"{root}.mud-autocomplete :where([class*=\"mud-input-label\"]),");
        css.AppendLine($"{root}.mud-picker :where([class*=\"mud-input-label\"]),");
        css.AppendLine($"{root} :where([class*=\"mud-input-label\"]) {{");
        css.AppendLine($"  color: var({variable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root}[data-nerd-token] :where([class*=\"mud-input-slot\"]),");
        css.AppendLine($"{root}[data-nerd-token] :where([class*=\"mud-input-text\"]),");
        css.AppendLine($"{root}.mud-text-field :where([class*=\"mud-input-slot\"]),");
        css.AppendLine($"{root}.mud-numeric-field :where([class*=\"mud-input-slot\"]),");
        css.AppendLine($"{root}.mud-select :where([class*=\"mud-input-slot\"]),");
        css.AppendLine($"{root}.mud-autocomplete :where([class*=\"mud-input-slot\"]),");
        css.AppendLine($"{root}.mud-picker :where([class*=\"mud-input-slot\"]),");
        css.AppendLine($"{root} :where([class*=\"mud-input-slot\"]),");
        css.AppendLine($"{root} :where([class*=\"mud-input-text\"]) {{");
        css.AppendLine($"  color: var({inputValueVariable}){importantSuffix};");
        css.AppendLine($"  caret-color: var({variable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root}[data-nerd-token] :where([class*=\"mud-input-adornment\"]),");
        css.AppendLine($"{root}.mud-numeric-field :where([class*=\"mud-input-adornment\"]),");
        css.AppendLine($"{root}.mud-text-field :where([class*=\"mud-input-adornment\"]),");
        css.AppendLine($"{root} :where([class*=\"mud-input-adornment\"]) {{");
        css.AppendLine($"  color: var({variable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root}[data-nerd-token] :where(.mud-input-outlined) .mud-input-outlined-border,");
        css.AppendLine($"{root}.mud-text-field :where(.mud-input-outlined) .mud-input-outlined-border,");
        css.AppendLine($"{root}.mud-numeric-field :where(.mud-input-outlined) .mud-input-outlined-border,");
        css.AppendLine($"{root}.mud-select :where(.mud-input-outlined) .mud-input-outlined-border,");
        css.AppendLine($"{root}.mud-autocomplete :where(.mud-input-outlined) .mud-input-outlined-border,");
        css.AppendLine($"{root}.mud-picker :where(.mud-input-outlined) .mud-input-outlined-border,");
        css.AppendLine($"{root} :where(.mud-input-outlined) .mud-input-outlined-border {{");
        css.AppendLine($"  border-color: color-mix(in srgb, var({inputBorderMixVariable}) 65%, {pageMixBase}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root}[data-nerd-token] :where(.mud-input-outlined.mud-input-focused) .mud-input-outlined-border,");
        css.AppendLine($"{root}.mud-text-field :where(.mud-input-outlined.mud-input-focused) .mud-input-outlined-border,");
        css.AppendLine($"{root}.mud-numeric-field :where(.mud-input-outlined.mud-input-focused) .mud-input-outlined-border,");
        css.AppendLine($"{root}.mud-select :where(.mud-input-outlined.mud-input-focused) .mud-input-outlined-border,");
        css.AppendLine($"{root}.mud-autocomplete :where(.mud-input-outlined.mud-input-focused) .mud-input-outlined-border,");
        css.AppendLine($"{root}.mud-picker :where(.mud-input-outlined.mud-input-focused) .mud-input-outlined-border,");
        css.AppendLine($"{root} :where(.mud-input-outlined.mud-input-focused) .mud-input-outlined-border {{");
        css.AppendLine($"  border-color: var({inputBorderMixVariable}){importantSuffix};");
        css.AppendLine("}");
    }

    private static void AppendToggleGroupStateRules(
        StringBuilder css,
        string root,
        string variable,
        string textVariable,
        string hoverVariable,
        string borderVariable,
        string importantSuffix)
    {
        // Use attribute selectors on {root} so a direct token class (e.g. .tnc-navy on the item)
        // wins over ancestor intent scopes (e.g. .tnc-primary-action). Descendant branch uses :where()
        // to keep alias-on-ancestor rules from overriding nested token classes.
        css.AppendLine($"{root}[class*=\"mud-toggle-group\"], {root} :where([class*=\"mud-toggle-group\"]) {{");
        css.AppendLine($"  border-color: var({borderVariable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root}[class*=\"mud-toggle-item\"][class*=\"mud-button-outlined\"],");
        css.AppendLine($"{root} :where([class*=\"mud-toggle-item\"][class*=\"mud-button-outlined\"]) {{");
        css.AppendLine($"  color: var({variable}){importantSuffix};");
        css.AppendLine($"  border-color: var({borderVariable}){importantSuffix};");
        css.AppendLine($"  background-color: transparent{importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root}[class*=\"mud-toggle-item\"][class*=\"mud-button-outlined\"]:hover,");
        css.AppendLine($"{root} :where([class*=\"mud-toggle-item\"][class*=\"mud-button-outlined\"]):hover {{");
        css.AppendLine($"  background-color: color-mix(in srgb, var({variable}) 12%, transparent){importantSuffix};");
        css.AppendLine($"  color: var({variable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root}[class*=\"mud-toggle-item\"][class*=\"mud-button-filled\"],");
        css.AppendLine($"{root}[class*=\"mud-toggle-item\"][class*=\"mud-toggle-item-selected\"],");
        css.AppendLine($"{root} :where([class*=\"mud-toggle-item\"][class*=\"mud-button-filled\"]),");
        css.AppendLine($"{root} :where([class*=\"mud-toggle-item\"][class*=\"mud-toggle-item-selected\"]) {{");
        css.AppendLine($"  background-color: var({variable}){importantSuffix};");
        css.AppendLine($"  color: var({textVariable}){importantSuffix};");
        css.AppendLine($"  border-color: var({variable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root}[class*=\"mud-toggle-item\"][class*=\"mud-button-filled\"]:hover,");
        css.AppendLine($"{root} :where([class*=\"mud-toggle-item\"][class*=\"mud-button-filled\"]):hover {{");
        css.AppendLine($"  background-color: var({hoverVariable}){importantSuffix};");
        css.AppendLine($"  color: var({textVariable}){importantSuffix};");
        css.AppendLine("}");
    }

    private static void AppendPickerPortalStateRules(
        StringBuilder css,
        string root,
        string variable,
        string textVariable,
        string contentVariable,
        string hoverVariable,
        string importantSuffix)
    {
        css.AppendLine($"{root} .mud-picker-calendar .mud-day.mud-selected,");
        css.AppendLine($"{root}.mud-picker-calendar .mud-day.mud-selected,");
        css.AppendLine($"{root} .mud-picker-calendar .mud-day.mud-selected.mud-button-root,");
        css.AppendLine($"{root}.mud-picker-calendar .mud-day.mud-selected.mud-button-root {{");
        css.AppendLine($"  background-color: var({variable}){importantSuffix};");
        css.AppendLine($"  color: var({textVariable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-picker-calendar .mud-day.mud-selected .mud-typography,");
        css.AppendLine($"{root}.mud-picker-calendar .mud-day.mud-selected .mud-typography {{");
        css.AppendLine($"  color: inherit{importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-picker-calendar .mud-day.mud-current:not(.mud-selected),");
        css.AppendLine($"{root}.mud-picker-calendar .mud-day.mud-current:not(.mud-selected) {{");
        css.AppendLine($"  color: var({variable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-picker-calendar .mud-day:not(.mud-selected):not(.mud-disabled),");
        css.AppendLine($"{root}.mud-picker-calendar .mud-day:not(.mud-selected):not(.mud-disabled) {{");
        css.AppendLine($"  color: var({variable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-picker-calendar .mud-day:not(.mud-disabled):hover,");
        css.AppendLine($"{root}.mud-picker-calendar .mud-day:not(.mud-disabled):hover {{");
        css.AppendLine($"  background-color: color-mix(in srgb, var({variable}) 12%, transparent){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-picker-calendar-header-switch .mud-icon-button,");
        css.AppendLine($"{root} .mud-picker-calendar-header-day .mud-day-label {{");
        css.AppendLine($"  color: var({contentVariable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-list-item.mud-selected,");
        css.AppendLine($"{root}.mud-list-item.mud-selected,");
        css.AppendLine($"{root} .mud-list-item.mud-selected-item,");
        css.AppendLine($"{root}.mud-list-item.mud-selected-item,");
        css.AppendLine($"{root} [role=\"option\"][aria-selected=\"true\"],");
        css.AppendLine($"{root}[role=\"option\"][aria-selected=\"true\"],");
        css.AppendLine($"{root} .mud-selected-item,");
        css.AppendLine($"{root}.mud-selected-item {{");
        css.AppendLine($"  background-color: var({variable}){importantSuffix};");
        css.AppendLine($"  color: var({textVariable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-list-item.mud-selected .mud-typography,");
        css.AppendLine($"{root} .mud-list-item.mud-selected-item .mud-typography,");
        css.AppendLine($"{root} [role=\"option\"][aria-selected=\"true\"] .mud-typography,");
        css.AppendLine($"{root}.mud-selected-item .mud-typography {{");
        css.AppendLine($"  color: inherit{importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-timepicker-button:not(.mud-timepicker-toolbar-text),");
        css.AppendLine($"{root}.mud-timepicker-button:not(.mud-timepicker-toolbar-text) {{");
        css.AppendLine($"  background-color: var({variable}){importantSuffix};");
        css.AppendLine($"  color: var({textVariable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine($"{root}[class*=\"mud-picker-color\"], {root} :where([class*=\"mud-picker-color\"]) {{");
        css.AppendLine($"  color: var({contentVariable}){importantSuffix};");
        css.AppendLine("}");
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
            // Keep nested token classes overridable: a direct `.dnf-himmel`
            // class must win over an ancestor `.dnf-kridt` scope.
            css.Append($"{root}[class*=\"{pattern}\"], {root} :where([class*=\"{pattern}\"])");
            css.AppendLine(i == patterns.Count - 1 ? " {" : ",");
        }

        css.AppendLine($"  {declarations}");
        css.AppendLine("}");
    }
}
