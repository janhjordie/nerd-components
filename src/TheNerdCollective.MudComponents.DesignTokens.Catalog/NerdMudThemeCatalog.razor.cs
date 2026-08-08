using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MudBlazor;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

public partial class NerdMudThemeCatalog : IDisposable
{
    [Inject]
    private NerdDesignTokenOptions Options { get; set; } = default!;

    [Inject]
    private NerdDesignSystemOptions HubOptions { get; set; } = default!;

    [Inject]
    private IWebHostEnvironment HostEnvironment { get; set; } = default!;

    [Inject]
    private INerdMudThemeController? ThemeController { get; set; }

    [Inject]
    private INerdMudThemeConfigurator? ThemeConfigurator { get; set; }

    [Inject]
    private INerdBrandSwitcher BrandSwitcher { get; set; } = default!;

    private bool _previewDark;
    private MudTheme _catalogTheme = new();
    private string _search = string.Empty;
    private string? _categoryFilter;
    private IReadOnlyList<NerdMudThemeMappingEntry> _mappings = [];
    private IReadOnlyList<BindingRow> _bindingRows = [];
    private IReadOnlyList<string> _categories = [];

    private bool IsAvailable =>
        Options.EnableCatalogPage &&
        (!Options.RestrictCatalogToDevelopment || HostEnvironment.IsDevelopment());

    private IEnumerable<NerdMudThemeMappingEntry> FilteredMappings
    {
        get
        {
            IEnumerable<NerdMudThemeMappingEntry> query = _mappings;
            if (!string.IsNullOrWhiteSpace(_categoryFilter))
            {
                query = query.Where(entry =>
                    string.Equals(entry.Category, _categoryFilter, StringComparison.Ordinal));
            }

            if (!string.IsNullOrWhiteSpace(_search))
            {
                query = query.Where(entry => MatchesSearch(entry, _search));
            }

            return query;
        }
    }

    protected override void OnInitialized()
    {
        if (!IsAvailable)
        {
            return;
        }

        BrandSwitcher.BrandChanged += OnGlobalBrandChanged;
        Refresh();
    }

    private void Refresh()
    {
        _catalogTheme = NerdCatalogThemeResolver.CreateForCatalog(
            Options,
            ThemeController,
            configurator: ThemeConfigurator);
        _mappings = NerdMudThemeMappingTools.BuildAll(Options);
        _categories = _mappings
            .Select(entry => entry.Category)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(category => category, StringComparer.Ordinal)
            .ToList();

        var bindings = NerdMudThemeMappingTools.ResolveBindings(Options);
        var light = NerdMudBrandPaletteMap.Resolve(Options, NerdMudPaletteMode.Light);
        var dark = NerdMudBrandPaletteMap.Resolve(Options, NerdMudPaletteMode.Dark);
        _bindingRows = NerdMudThemeMappingTools.EnumerateBindingSlots(bindings)
            .Select(slot =>
            {
                var colorToken = NerdMudThemeMappingTools.ResolveColorTokenName(Options, slot.Alias);
                var lightValue = ResolveBindingPreview(light, slot.MudSlot);
                var darkValue = ResolveBindingPreview(dark, slot.MudSlot);
                return new BindingRow(slot.MudSlot, slot.Alias, colorToken, lightValue, darkValue);
            })
            .ToList();
    }

    private void OnGlobalBrandChanged(string _)
    {
        Refresh();
        InvokeAsync(StateHasChanged);
    }

    private string Ui(string semanticAlias) =>
        NerdDesignSystemUi.TokenClass(Options.Prefix, semanticAlias);

    private static string SwatchStyle(string value) =>
        $"display:inline-block;width:18px;height:18px;border-radius:4px;border:1px solid var(--mud-palette-lines-default);background:{value};flex-shrink:0;";

    private static bool LooksLikeColor(string value) =>
        !string.IsNullOrWhiteSpace(value) &&
        (value.StartsWith('#') ||
         value.StartsWith("rgb", StringComparison.OrdinalIgnoreCase) ||
         value.StartsWith("hsl", StringComparison.OrdinalIgnoreCase) ||
         value.Contains("color-mix", StringComparison.OrdinalIgnoreCase));

    private static string Truncate(string value) =>
        value.Length <= 64 ? value : value[..61] + "…";

    private static string Sanitize(string value) =>
        value.Replace('.', '-').Replace('[', '-').Replace(']', '-').ToLowerInvariant();

    private static bool MatchesSearch(NerdMudThemeMappingEntry entry, string search) =>
        Contains(entry.MudThemeProperty, search) ||
        Contains(entry.CssVariable, search) ||
        Contains(entry.BindingAlias, search) ||
        Contains(entry.ColorToken, search) ||
        Contains(entry.ValueKind, search) ||
        Contains(entry.Notes, search) ||
        Contains(entry.LightValue, search) ||
        Contains(entry.DarkValue, search);

    private static bool Contains(string? haystack, string needle) =>
        !string.IsNullOrWhiteSpace(haystack) &&
        haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);

    private static string ResolveBindingPreview(NerdMudBrandPaletteMap map, string mudSlot) =>
        mudSlot switch
        {
            "Primary" => map.Primary,
            "Secondary" => map.Secondary,
            "Tertiary" => map.Tertiary,
            "Info" => map.Info,
            "Success" => map.Success,
            "Warning" => map.Warning,
            "Error" => map.Error,
            "Dark" => map.Dark,
            "Surface" => map.Surface,
            "Background" => map.Background,
            "TextPrimary" => map.TextPrimary,
            "TextSecondary" => map.TextSecondary,
            "TextDisabled" => map.TextDisabled,
            "ActionDefault" => map.ActionDefault,
            "ActionDisabled" => map.ActionDisabled,
            "AppbarBackground" => map.AppbarBackground,
            "AppbarText" => map.AppbarText,
            "DrawerBackground" => map.DrawerBackground,
            "DrawerText" => map.DrawerText,
            "DrawerIcon" => map.DrawerIcon,
            "LinesDefault" => map.LinesDefault,
            "LinesInputs" => map.LinesInputs,
            _ => "—"
        };

    public void Dispose() => BrandSwitcher.BrandChanged -= OnGlobalBrandChanged;

    private sealed record BindingRow(
        string MudSlot,
        string? Alias,
        string? ColorToken,
        string LightValue,
        string DarkValue);
}
