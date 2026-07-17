namespace TheNerdCollective.MudComponents.PlayBook;

/// <summary>
/// Describes a MudBlazor component entry in the PlayBook gallery.
/// </summary>
public sealed record MudBlazorPlayBookEntry(
    string Id,
    string DisplayName,
    string Category,
    string ApiSlug,
    string? Description = null)
{
    public string ApiUrl(string docsBaseUrl) =>
        $"{docsBaseUrl.TrimEnd('/')}/{ApiSlug}";
}

/// <summary>
/// Complete MudBlazor 9.6 component catalog for the PlayBook.
/// </summary>
public static class MudBlazorPlayBookCatalog
{
    public const string CategoryButtons = "Buttons";
    public const string CategoryInputs = "Inputs";
    public const string CategoryDataDisplay = "Data display";
    public const string CategoryFeedback = "Feedback";
    public const string CategorySurfaces = "Surfaces";
    public const string CategoryNavigation = "Navigation";
    public const string CategoryLayout = "Layout";

    public static IReadOnlyList<string> Categories { get; } =
    [
        CategoryButtons,
        CategoryInputs,
        CategoryDataDisplay,
        CategoryFeedback,
        CategorySurfaces,
        CategoryNavigation,
        CategoryLayout
    ];

    public static IReadOnlyList<MudBlazorPlayBookEntry> All { get; } =
    [
        // Buttons
        new("button", "MudButton", CategoryButtons, "button", "Filled, outlined, and text buttons."),
        new("icon-button", "MudIconButton", CategoryButtons, "iconbutton", "Icon-only action button."),
        new("fab", "MudFab", CategoryButtons, "fab", "Floating action button."),
        new("toggle-group", "MudToggleGroup", CategoryButtons, "togglegroup", "Exclusive toggle button group."),
        new("button-group", "MudButtonGroup", CategoryButtons, "buttongroup", "Grouped buttons with shared borders."),

        // Inputs
        new("text-field", "MudTextField", CategoryInputs, "textfield", "Single-line text input."),
        new("numeric-field", "MudNumericField", CategoryInputs, "numericfield", "Numeric input with formatting."),
        new("select", "MudSelect", CategoryInputs, "select", "Dropdown selection."),
        new("autocomplete", "MudAutocomplete", CategoryInputs, "autocomplete", "Type-ahead selection."),
        new("checkbox", "MudCheckbox", CategoryInputs, "checkbox", "Boolean checkbox input."),
        new("radio", "MudRadio", CategoryInputs, "radio", "Single radio option."),
        new("switch", "MudSwitch", CategoryInputs, "switch", "Toggle switch."),
        new("slider", "MudSlider", CategoryInputs, "slider", "Range slider input."),
        new("rating", "MudRating", CategoryInputs, "rating", "Star rating input."),
        new("date-picker", "MudDatePicker", CategoryInputs, "datepicker", "Date picker input."),
        new("time-picker", "MudTimePicker", CategoryInputs, "timepicker", "Time picker input."),
        new("date-range-picker", "MudDateRangePicker", CategoryInputs, "daterangepicker", "Date range picker."),
        new("file-upload", "MudFileUpload", CategoryInputs, "fileupload", "File upload control."),

        // Data display
        new("table", "MudTable", CategoryDataDisplay, "table", "Data table with sorting and paging."),
        new("simple-table", "MudSimpleTable", CategoryDataDisplay, "simpletable", "Lightweight HTML table."),
        new("data-grid", "MudDataGrid", CategoryDataDisplay, "datagrid", "Advanced data grid."),
        new("chip", "MudChip", CategoryDataDisplay, "chip", "Compact label or tag."),
        new("badge", "MudBadge", CategoryDataDisplay, "badge", "Notification badge overlay."),
        new("avatar", "MudAvatar", CategoryDataDisplay, "avatar", "User or entity avatar."),
        new("tooltip", "MudTooltip", CategoryDataDisplay, "tooltip", "Hover tooltip."),
        new("list", "MudList", CategoryDataDisplay, "list", "Vertical list of items."),
        new("tree-view", "MudTreeView", CategoryDataDisplay, "treeview", "Hierarchical tree view."),
        new("timeline", "MudTimeline", CategoryDataDisplay, "timeline", "Vertical timeline."),
        new("typography", "MudText", CategoryDataDisplay, "typography", "Typography and headings."),
        new("icon", "MudIcon", CategoryDataDisplay, "icons", "Material Design icons."),
        new("image", "MudImage", CategoryDataDisplay, "image", "Responsive image."),
        new("markdown", "MudMarkdown", CategoryDataDisplay, "markdown", "Rendered markdown content."),

        // Feedback
        new("alert", "MudAlert", CategoryFeedback, "alert", "Contextual alert banner."),
        new("progress-circular", "MudProgressCircular", CategoryFeedback, "progress", "Circular progress indicator."),
        new("progress-linear", "MudProgressLinear", CategoryFeedback, "progress", "Linear progress bar."),
        new("skeleton", "MudSkeleton", CategoryFeedback, "skeleton", "Loading placeholder."),

        // Surfaces
        new("paper", "MudPaper", CategorySurfaces, "paper", "Elevated surface container."),
        new("card", "MudCard", CategorySurfaces, "card", "Card with header, content, and actions."),
        new("expansion-panel", "MudExpansionPanel", CategorySurfaces, "expansionpanels", "Expandable content panel."),
        new("drawer", "MudDrawer", CategorySurfaces, "drawer", "Side navigation drawer."),
        new("popover", "MudPopover", CategorySurfaces, "popover", "Anchored popover content."),

        // Navigation
        new("app-bar", "MudAppBar", CategoryNavigation, "appbar", "Top application bar."),
        new("nav-menu", "MudNavMenu", CategoryNavigation, "navmenu", "Vertical navigation menu."),
        new("tabs", "MudTabs", CategoryNavigation, "tabs", "Tabbed content navigation."),
        new("breadcrumbs", "MudBreadcrumbs", CategoryNavigation, "breadcrumbs", "Hierarchical breadcrumb trail."),
        new("menu", "MudMenu", CategoryNavigation, "menu", "Dropdown menu."),
        new("pagination", "MudPagination", CategoryNavigation, "pagination", "Page navigation control."),
        new("stepper", "MudStepper", CategoryNavigation, "stepper", "Multi-step wizard."),

        // Layout
        new("grid", "MudGrid", CategoryLayout, "grid", "Responsive 12-column grid."),
        new("container", "MudContainer", CategoryLayout, "container", "Centered max-width container."),
        new("divider", "MudDivider", CategoryLayout, "divider", "Horizontal or vertical divider."),
        new("stack", "MudStack", CategoryLayout, "stack", "Flexbox stack layout."),
        new("spacer", "MudSpacer", CategoryLayout, "spacer", "Flexible space filler.")
    ];

    public static IEnumerable<MudBlazorPlayBookEntry> GetByCategory(string category) =>
        All.Where(entry => string.Equals(entry.Category, category, StringComparison.Ordinal));

    public static MudBlazorPlayBookEntry? FindById(string id) =>
        All.FirstOrDefault(entry => string.Equals(entry.Id, id, StringComparison.OrdinalIgnoreCase));
}
