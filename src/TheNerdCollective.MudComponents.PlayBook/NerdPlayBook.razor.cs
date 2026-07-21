using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MudBlazor;
using TheNerdCollective.MudComponents.DesignTokens;
using TheNerdCollective.MudComponents.ResponsiveTypography;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.PlayBook;

public partial class NerdPlayBook : IDisposable
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
    private INerdBrandSwitcher BrandSwitcher { get; set; } = default!;

    private int _activeSectionTabIndex;
    private int _activeCategoryTabIndex;
    private bool _previewDark;
    private string _typographyPreset = NerdPlayBookTypography.DefaultPreset;
    private string _selectedTokenFilter = NerdPlayBookTokenFilter.AllIntents;
    private string _searchQuery = string.Empty;
    private double _typographyViewport = 1280;
    private MudTheme _previewTheme = new();
    private IReadOnlyList<string> _tokenNames = [];
    private string? _inspectedTokenClass;

    protected override void OnInitialized()
    {
        BrandSwitcher.BrandChanged += OnGlobalBrandChanged;
        RefreshTokenNames();
        ApplyTypographyPreset();
    }

    private bool IsAvailable =>
        Options.EnablePlayBookPage &&
        (!Options.RestrictPlayBookToDevelopment || HostEnvironment.IsDevelopment());

    private IReadOnlyList<string> VisibleTokens =>
        NerdPlayBookTokenFilter.ResolveVisibleClasses(TokenOptions, _selectedTokenFilter, _tokenNames);

    private string PlaygroundTokenClass =>
        VisibleTokens.FirstOrDefault() ?? string.Empty;

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
        var typography = NerdPlayBookTypography.ResolveTypography(_typographyPreset, TypographyOptions);

        if (ThemeController is not null)
        {
            ThemeController.RefreshTheme(theme => theme.UseResponsiveTypography(typography.CopyTo));
            _previewTheme = ThemeController.CurrentTheme;
        }
        else
        {
            _previewTheme = NerdPlayBookTypography.CreateBrandTheme(_typographyPreset, TokenOptions, TypographyOptions);
        }

        TypographyCss.Update(typography);
    }

    private void RefreshTokenNames()
    {
        _tokenNames = TokenOptions.Colors.Keys.OrderBy(name => name, StringComparer.Ordinal).ToList();

        if (!NerdPlayBookTokenFilter.IsValid(TokenOptions, _selectedTokenFilter, _tokenNames))
        {
            _selectedTokenFilter = NerdPlayBookTokenFilter.AllIntents;
        }
    }

    private void OnGlobalBrandChanged(string _)
    {
        RefreshTokenNames();
        ApplyTypographyPreset();
        InvokeAsync(StateHasChanged);
    }

    private Task OnTypographyPresetChanged(string value)
    {
        _typographyPreset = value;
        ApplyTypographyPreset();
        return Task.CompletedTask;
    }

    private void InspectToken(string tokenClass) => _inspectedTokenClass = tokenClass;

    public void Dispose() => BrandSwitcher.BrandChanged -= OnGlobalBrandChanged;
}
