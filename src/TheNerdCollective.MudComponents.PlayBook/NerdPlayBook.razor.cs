using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MudBlazor;
using TheNerdCollective.MudComponents.DesignTokens;
using TheNerdCollective.MudComponents.ResponsiveTypography;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.PlayBook;

public partial class NerdPlayBook
{
    [Inject]
    private NerdPlayBookOptions Options { get; set; } = default!;

    [Inject]
    private NerdDesignTokenOptions TokenOptions { get; set; } = default!;

    [Inject]
    private NerdResponsiveTypographyOptions TypographyOptions { get; set; } = default!;

    [Inject]
    private NerdDesignSystemOptions HubOptions { get; set; } = default!;

    [Inject]
    private IWebHostEnvironment HostEnvironment { get; set; } = default!;

    [Inject]
    private NerdDesignTokenCss TokenCss { get; set; } = default!;

    [Inject]
    private NerdResponsiveTypographyCss TypographyCss { get; set; } = default!;

    [Inject]
    private INerdMudThemeController? ThemeController { get; set; }

    [Inject]
    private IEnumerable<INerdBrandPack> BrandPacks { get; set; } = [];

    private int _activeSectionTabIndex;
    private int _activeCategoryTabIndex;
    private bool _previewDark;
    private string _selectedBrand = string.Empty;
    private string _typographyPreset = NerdPlayBookTypography.DefaultPreset;
    private string _selectedTokenFilter = "all";
    private string _searchQuery = string.Empty;
    private double _typographyViewport = 1280;
    private MudTheme _previewTheme = new();
    private IReadOnlyList<string> _tokenNames = [];
    private string? _inspectedTokenClass;

    private IEnumerable<INerdBrandPack> InstalledBrandPacks =>
        BrandPacks.OrderBy(pack => pack.Id, StringComparer.OrdinalIgnoreCase);

    private bool ShowBrandSwitcher => InstalledBrandPacks.Any();

    protected override void OnInitialized()
    {
        _selectedBrand = TokenOptions.ActiveBrandPackId ?? TokenOptions.Prefix;
        RefreshTokenNames();
        ApplyTypographyPreset();
    }

    private bool IsAvailable =>
        Options.EnablePlayBookPage &&
        (!Options.RestrictPlayBookToDevelopment || HostEnvironment.IsDevelopment());

    private IReadOnlyList<string> VisibleTokens =>
        _selectedTokenFilter == "all"
            ? _tokenNames.Select(name => $"{TokenOptions.Prefix}-{name}").ToList()
            : [$"{TokenOptions.Prefix}-{_selectedTokenFilter}"];

    private string PlaygroundTokenClass =>
        _selectedTokenFilter == "all"
            ? VisibleTokens.FirstOrDefault() ?? string.Empty
            : $"{TokenOptions.Prefix}-{_selectedTokenFilter}";

    private string Ui(string semanticAlias) => NerdDesignSystemUi.TokenClass(TokenOptions.Prefix, semanticAlias);

    private string ThemeMode => _previewDark ? "dark" : "light";

    private string? ActiveCategory =>
        _activeCategoryTabIndex <= 0
            ? null
            : MudBlazorPlayBookCatalog.Categories[_activeCategoryTabIndex - 1];

    private bool IsSearching => !string.IsNullOrWhiteSpace(_searchQuery);

    private int CountInCategory(string? category) =>
        category is null
            ? MudBlazorPlayBookCatalog.All.Count
            : MudBlazorPlayBookCatalog.GetByCategory(category).Count();

    private IReadOnlyList<MudBlazorPlayBookEntry> FilteredEntries => GetFilteredEntries(ActiveCategory);

    private IReadOnlyList<MudBlazorPlayBookEntry> GetFilteredEntries(string? category)
    {
        IEnumerable<MudBlazorPlayBookEntry> entries = MudBlazorPlayBookCatalog.All;

        if (!IsSearching && category is not null)
        {
            entries = entries.Where(entry =>
                string.Equals(entry.Category, category, StringComparison.Ordinal));
        }

        if (IsSearching)
        {
            entries = entries.Where(entry =>
                entry.DisplayName.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ||
                entry.Id.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ||
                entry.Category.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ||
                (entry.Description?.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return entries.ToList();
    }

    private void ApplyTypographyPreset()
    {
        _previewTheme = NerdPlayBookTypography.CreateTheme(_typographyPreset, TypographyOptions);
        TypographyCss.Update(TypographyOptions.Typography);
    }

    private void RefreshTokenNames()
    {
        _tokenNames = TokenOptions.Colors.Keys.OrderBy(name => name, StringComparer.Ordinal).ToList();

        if (_selectedTokenFilter != "all" &&
            !_tokenNames.Contains(_selectedTokenFilter, StringComparer.OrdinalIgnoreCase))
        {
            _selectedTokenFilter = "all";
        }
    }

    private Task SwitchBrandAsync(string brand)
    {
        _selectedBrand = brand;
        if (ThemeController is not null)
        {
            ThemeController.ApplyBrandPack(brand);
        }
        else
        {
            NerdBrandPackRegistry.Instance.Configure(brand, TokenOptions);
            TokenCss.Update(TokenOptions);
            HubOptions.ActiveTokenPackId = brand;
            HubOptions.ActiveBrandIdentityVersion = TokenOptions.ActiveBrandIdentityVersion;
            NerdBrandTypographySwitcher.TrySwitchBrand(brand, TypographyOptions, HubOptions, _previewTheme);
        }

        RefreshTokenNames();
        ApplyTypographyPreset();
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task OnTypographyPresetChanged(string value)
    {
        _typographyPreset = value;
        ApplyTypographyPreset();
        return Task.CompletedTask;
    }

    private void InspectToken(string tokenClass) => _inspectedTokenClass = tokenClass;
}
