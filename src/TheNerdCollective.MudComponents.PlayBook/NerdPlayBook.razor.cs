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

    private bool _previewDark;
    private string _typographyPreset = NerdPlayBookTypography.DefaultPreset;
    private string _selectedTokenFilter = "all";
    private string _selectedCategory = "all";
    private string _searchQuery = string.Empty;
    private MudTheme _previewTheme = new();
    private IReadOnlyList<string> _tokenNames = [];

    protected override void OnInitialized()
    {
        _tokenNames = TokenOptions.Colors.Keys.OrderBy(name => name, StringComparer.Ordinal).ToList();
        ApplyTypographyPreset();
    }

    private bool IsAvailable =>
        Options.EnablePlayBookPage &&
        (!Options.RestrictPlayBookToDevelopment || HostEnvironment.IsDevelopment());

    private IReadOnlyList<string> VisibleTokens =>
        _selectedTokenFilter == "all"
            ? _tokenNames.Select(name => $"{TokenOptions.Prefix}-{name}").ToList()
            : [$"{TokenOptions.Prefix}-{_selectedTokenFilter}"];

    private IReadOnlyList<MudBlazorPlayBookEntry> FilteredEntries
    {
        get
        {
            IEnumerable<MudBlazorPlayBookEntry> entries = MudBlazorPlayBookCatalog.All;

            if (!string.Equals(_selectedCategory, "all", StringComparison.OrdinalIgnoreCase))
            {
                entries = entries.Where(entry =>
                    string.Equals(entry.Category, _selectedCategory, StringComparison.Ordinal));
            }

            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                entries = entries.Where(entry =>
                    entry.DisplayName.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    entry.Id.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    (entry.Description?.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            return entries.ToList();
        }
    }

    private void ApplyTypographyPreset()
    {
        _previewTheme = NerdPlayBookTypography.CreateTheme(_typographyPreset, TypographyOptions);
    }

    private Task OnTypographyPresetChanged(string value)
    {
        _typographyPreset = value;
        ApplyTypographyPreset();
        return Task.CompletedTask;
    }
}
