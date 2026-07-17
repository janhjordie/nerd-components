namespace TheNerdCollective.MudComponents.PlayBook;

/// <summary>
/// Playground property definitions per MudBlazor component.
/// </summary>
public static class NerdPlayBookPlaygroundRegistry
{
    private static readonly IReadOnlyList<NerdPlayBookPropDefinition> ButtonProps =
    [
        new("variant", "Variant", NerdPlayBookPropType.Select, "Filled", ["Filled", "Outlined", "Text"]),
        new("size", "Size", NerdPlayBookPropType.Select, "Small", ["Small", "Medium", "Large"]),
        new("disabled", "Disabled", NerdPlayBookPropType.Boolean, "false"),
        new("text", "Text", NerdPlayBookPropType.Text, "Button")
    ];

    private static readonly IReadOnlyList<NerdPlayBookPropDefinition> InputProps =
    [
        new("label", "Label", NerdPlayBookPropType.Text, "Name"),
        new("placeholder", "Placeholder", NerdPlayBookPropType.Text, "Enter value"),
        new("variant", "Variant", NerdPlayBookPropType.Select, "Outlined", ["Outlined", "Filled", "Text"]),
        new("disabled", "Disabled", NerdPlayBookPropType.Boolean, "false"),
        new("readOnly", "Read only", NerdPlayBookPropType.Boolean, "false")
    ];

    private static readonly IReadOnlyList<NerdPlayBookPropDefinition> ToggleProps =
    [
        new("label", "Label", NerdPlayBookPropType.Text, "Enabled"),
        new("disabled", "Disabled", NerdPlayBookPropType.Boolean, "false"),
        new("checked", "Checked", NerdPlayBookPropType.Boolean, "true")
    ];

    private static readonly IReadOnlyList<NerdPlayBookPropDefinition> AlertProps =
    [
        new("severity", "Severity", NerdPlayBookPropType.Select, "Info", ["Normal", "Info", "Success", "Warning", "Error"]),
        new("variant", "Variant", NerdPlayBookPropType.Select, "Filled", ["Filled", "Outlined", "Text"]),
        new("dense", "Dense", NerdPlayBookPropType.Boolean, "true"),
        new("text", "Text", NerdPlayBookPropType.Text, "Alert preview")
    ];

    private static readonly IReadOnlyList<NerdPlayBookPropDefinition> ChipProps =
    [
        new("variant", "Variant", NerdPlayBookPropType.Select, "Filled", ["Filled", "Outlined", "Text"]),
        new("disabled", "Disabled", NerdPlayBookPropType.Boolean, "false"),
        new("text", "Text", NerdPlayBookPropType.Text, "Chip")
    ];

    private static readonly IReadOnlyList<NerdPlayBookPropDefinition> ProgressLinearProps =
    [
        new("value", "Value", NerdPlayBookPropType.Text, "60"),
        new("indeterminate", "Indeterminate", NerdPlayBookPropType.Boolean, "false")
    ];

    private static readonly IReadOnlyList<NerdPlayBookPropDefinition> SliderProps =
    [
        new("min", "Min", NerdPlayBookPropType.Text, "0"),
        new("max", "Max", NerdPlayBookPropType.Text, "100"),
        new("value", "Value", NerdPlayBookPropType.Text, "40"),
        new("disabled", "Disabled", NerdPlayBookPropType.Boolean, "false")
    ];

    private static readonly IReadOnlyList<NerdPlayBookPropDefinition> RatingProps =
    [
        new("value", "Value", NerdPlayBookPropType.Text, "3"),
        new("max", "Max", NerdPlayBookPropType.Text, "5"),
        new("readOnly", "Read only", NerdPlayBookPropType.Boolean, "false")
    ];

    private static readonly IReadOnlyList<NerdPlayBookPropDefinition> TypographyProps =
    [
        new("typo", "Typo", NerdPlayBookPropType.Select, "H6", ["H4", "H5", "H6", "Body1", "Body2", "Caption"]),
        new("text", "Text", NerdPlayBookPropType.Text, "Typography preview")
    ];

    private static readonly IReadOnlyList<NerdPlayBookPropDefinition> DenseProps =
    [
        new("dense", "Dense", NerdPlayBookPropType.Boolean, "true")
    ];

    private static readonly IReadOnlyList<NerdPlayBookPropDefinition> DisabledProps =
    [
        new("disabled", "Disabled", NerdPlayBookPropType.Boolean, "false")
    ];

    private static readonly IReadOnlyList<NerdPlayBookPropDefinition> TextProps =
    [
        new("text", "Text", NerdPlayBookPropType.Text, "Preview")
    ];

    private static readonly Dictionary<string, IReadOnlyList<NerdPlayBookPropDefinition>> Map =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["button"] = ButtonProps,
            ["icon-button"] = ButtonProps,
            ["fab"] = ButtonProps,
            ["toggle-group"] = DisabledProps,
            ["button-group"] = DisabledProps,
            ["text-field"] = InputProps,
            ["numeric-field"] = InputProps,
            ["select"] = InputProps,
            ["autocomplete"] = InputProps,
            ["checkbox"] = ToggleProps,
            ["radio"] = ToggleProps,
            ["switch"] = ToggleProps,
            ["slider"] = SliderProps,
            ["rating"] = RatingProps,
            ["date-picker"] = InputProps,
            ["time-picker"] = InputProps,
            ["date-range-picker"] = InputProps,
            ["file-upload"] = DisabledProps,
            ["table"] = DenseProps,
            ["simple-table"] = DenseProps,
            ["data-grid"] = DenseProps,
            ["chip"] = ChipProps,
            ["badge"] = TextProps,
            ["avatar"] = TextProps,
            ["tooltip"] = TextProps,
            ["list"] = DenseProps,
            ["tree-view"] = DenseProps,
            ["timeline"] = DenseProps,
            ["typography"] = TypographyProps,
            ["icon"] = DisabledProps,
            ["image"] = DisabledProps,
            ["markdown"] = TextProps,
            ["alert"] = AlertProps,
            ["progress-circular"] = ProgressLinearProps,
            ["progress-linear"] = ProgressLinearProps,
            ["skeleton"] = DisabledProps,
            ["paper"] = TextProps,
            ["card"] = TextProps,
            ["expansion-panel"] = TextProps,
            ["drawer"] = TextProps,
            ["popover"] = TextProps,
            ["app-bar"] = TextProps,
            ["nav-menu"] = DenseProps,
            ["tabs"] = DenseProps,
            ["breadcrumbs"] = DisabledProps,
            ["menu"] = DisabledProps,
            ["pagination"] = DisabledProps,
            ["stepper"] = DisabledProps,
            ["grid"] = DisabledProps,
            ["container"] = DisabledProps,
            ["divider"] = DisabledProps,
            ["stack"] = DisabledProps,
            ["spacer"] = DisabledProps
        };

    public static IReadOnlyList<NerdPlayBookPropDefinition> GetProps(string componentId) =>
        Map.TryGetValue(componentId, out var props) ? props : [];

    public static bool SupportsPlayground(string componentId) => GetProps(componentId).Count > 0;

    public static NerdPlayBookPlaygroundState CreateState(string componentId) =>
        new(GetProps(componentId));
}
