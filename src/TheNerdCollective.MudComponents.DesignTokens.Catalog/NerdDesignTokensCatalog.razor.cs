using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.JSInterop;
using MudBlazor;
using TheNerdCollective.MudComponents.ResponsiveTypography;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

public partial class NerdDesignTokensCatalog : IDisposable
{
    [Inject]
    private NerdDesignTokenOptions Options { get; set; } = default!;

    [Inject]
    private NerdDesignSystemOptions HubOptions { get; set; } = default!;

    [Inject]
    private IWebHostEnvironment HostEnvironment { get; set; } = default!;

    [Inject]
    private NerdDownloadService DownloadService { get; set; } = default!;

    [Inject]
    private INerdTokenPackStore TokenPackStore { get; set; } = default!;

    [Inject]
    private INerdBrandPackSource BrandPackSource { get; set; } = default!;

    [Inject]
    private INerdTokenCommentStore CommentStore { get; set; } = default!;

    [Inject]
    private NerdDesignTokenCss TokenCss { get; set; } = default!;

    [Inject]
    private INerdMudThemeController? ThemeController { get; set; }

    [Inject]
    private INerdMudThemeConfigurator? ThemeConfigurator { get; set; }

    [Inject]
    private NerdResponsiveTypographyOptions? TypographyOptions { get; set; }

    [Inject]
    private NerdResponsiveTypographyCss? TypographyCss { get; set; }

    [Inject]
    private INerdBrandSwitcher BrandSwitcher { get; set; } = default!;

    [Inject]
    private IEnumerable<INerdBrandPack> BrandPacks { get; set; } = [];

    [Inject]
    private INerdCatalogEntitlements Entitlements { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IJSRuntime Js { get; set; } = default!;

    private bool ShowResponsiveTypography => TypographyCss is not null;

    private bool _previewDark;
    private MudTheme _catalogTheme = new();
    private bool _dualPreview = true;
    private string _clientId = "client";
    private string? _selectedPackId;
    private string? _saveStatus;
    private string _tokenSearch = string.Empty;
    private bool _showFavoritesOnly;
    private int _activeTabIndex;
    private IReadOnlyList<string> _packIds = [];
    private IReadOnlyList<NerdAccessibilityResult> _accessibility = [];
    private IReadOnlyList<NerdAccessibilityWarning> _warnings = [];
    private string _diffBaseline = string.Empty;
    private IReadOnlyList<NerdTokenPackDiffEntry> _packDiff = [];
    private Dictionary<string, string> _comments = new(StringComparer.OrdinalIgnoreCase);
    private string? _selectedTreeTarget;
    private string? _selectedAliasName;
    private NerdTokenTreeNavigation? _selectedNavigation;
    private string _activeThemeSet = string.Empty;

    private IReadOnlyDictionary<string, NerdThemeSet> ThemeSets =>
        Options.ThemeSets.Count > 0
            ? Options.ThemeSets
            : NerdThemeSetTools.CreateFromOptions(Options);

    private IEnumerable<INerdBrandPack> InstalledBrandPacks =>
        BrandPacks.OrderBy(pack => pack.Id, StringComparer.OrdinalIgnoreCase);

    private readonly HashSet<string> _favoriteTokens = new(StringComparer.OrdinalIgnoreCase);

    private sealed record NerdCatalogGridRow(string Name, int Quantity);

    private readonly List<NerdCatalogGridRow> _previewGridRows =
    [
        new("Alpha", 1),
        new("Beta", 2)
    ];

    private readonly List<string> _previewDropItems = ["Alpha"];

    private readonly List<string> _previewAutocompleteOptions = ["Alpha", "Beta", "Gamma"];

    private readonly List<ChartSeries<double>> _previewChartSeries =
    [
        new() { Name = "Series", Data = new ChartData<double>([2, 4, 3]) }
    ];

    private Task<IEnumerable<string>> SearchPreviewAutocompleteAsync(string value, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IEnumerable<string> results = string.IsNullOrWhiteSpace(value)
            ? _previewAutocompleteOptions
            : _previewAutocompleteOptions.Where(option =>
                option.Contains(value, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(results);
    }

    protected override async Task OnInitializedAsync()
    {
        if (!Options.EnableCatalogPage)
        {
            return;
        }

        if (Options.RestrictCatalogToDevelopment && !HostEnvironment.IsDevelopment())
        {
            return;
        }

        _diffBaseline = Options.Prefix;

        BrandSwitcher.BrandChanged += OnGlobalBrandChanged;
        RefreshCatalogTheme();
        RefreshCatalogState();
        RefreshPackDiff();
        _packIds = await TokenPackStore.ListAsync();
        if (_packIds.Count > 0)
        {
            _selectedPackId ??= _packIds[0];
        }

        if (EnableCollaborativeComments)
        {
            _comments = new Dictionary<string, string>(
                await CommentStore.LoadAsync(_clientId),
                StringComparer.OrdinalIgnoreCase);
        }
    }

    private bool EnableCollaborativeComments =>
        HostEnvironment.IsDevelopment();

    private bool IsAvailable =>
        Options.EnableCatalogPage &&
        (!Options.RestrictCatalogToDevelopment || HostEnvironment.IsDevelopment());

    private string GetClassName(string tokenName) => $"{Options.Prefix}-{tokenName}";

    private string GetOpacityClassName(string opacityName) => $"{Options.Prefix}-opacity-{opacityName}";

    private string Ui(string semanticAlias) => NerdDesignSystemUi.TokenClass(Options.Prefix, semanticAlias);

    private string? GetComment(string tokenName) =>
        _comments.TryGetValue(tokenName, out var comment) ? comment : null;

    private void SetComment(string tokenName, string? comment)
    {
        if (!EnableCollaborativeComments)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(comment))
        {
            _comments.Remove(tokenName);
        }
        else
        {
            _comments[tokenName] = comment.Trim();
        }
    }

    private async Task PersistCommentsAsync()
    {
        if (!EnableCollaborativeComments)
        {
            return;
        }

        await CommentStore.SaveAsync(_clientId, _comments);
    }

    private IEnumerable<KeyValuePair<string, NerdColorToken>> FilteredColorTokens =>
        Options.Colors
            .Where(pair => string.IsNullOrWhiteSpace(_tokenSearch) ||
                           pair.Key.Contains(_tokenSearch, StringComparison.OrdinalIgnoreCase))
            .Where(pair => !_showFavoritesOnly || _favoriteTokens.Contains(pair.Key))
            .OrderBy(pair => pair.Key, StringComparer.Ordinal);

    private void RefreshCatalogState()
    {
        _accessibility = NerdDesignTokenTools.CheckAccessibility(Options);
        _warnings = NerdDesignTokenTools.GetAccessibilityWarnings(Options);
        HubOptions.DesignTokenCount = Options.Colors.Count;
        HubOptions.DesignTokenRecipeCount = Options.Recipes.Count;
        HubOptions.ActiveBrandIdentityVersion = Options.ActiveBrandIdentityVersion;
        RefreshPackDiff();
    }

    private string BuildRazorSnippet(string tokenName) =>
        $"<MudButton Class=\"{GetClassName(tokenName)}\" Variant=\"Variant.Filled\">Action</MudButton>";

    private string BuildCssSnippet(string tokenName) =>
        $".{GetClassName(tokenName)} {{ /* generated by NerdDesignTokenStyles */ }}";

    private void OpenTokenTab(string tokenName)
    {
        var tokenIndex = Options.Colors
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select((pair, index) => new { pair.Key, index })
            .FirstOrDefault(item => string.Equals(item.Key, tokenName, StringComparison.OrdinalIgnoreCase));

        if (tokenIndex is not null)
        {
            _activeTabIndex = tokenIndex.index + 1;
            _selectedTreeTarget = tokenName;
        }
    }

    private async Task OnTreeNavigateAsync(NerdTokenTreeNavigation navigation)
    {
        _selectedTreeTarget = navigation.TargetId;
        _selectedNavigation = navigation;
        switch (navigation.Kind)
        {
            case NerdTokenTreeTargetKind.Color:
            case NerdTokenTreeTargetKind.ThemeSetColor:
                _selectedAliasName = null;
                _activeTabIndex = 0;
                _tokenSearch = string.Empty;
                OpenTokenTab(navigation.TargetId);
                break;
            case NerdTokenTreeTargetKind.Alias:
                _selectedAliasName = navigation.TargetId;
                if (Options.Aliases.TryGetValue(navigation.TargetId, out var target))
                {
                    OpenTokenTab(target);
                }
                break;
            case NerdTokenTreeTargetKind.Spacing:
            case NerdTokenTreeTargetKind.Radius:
            case NerdTokenTreeTargetKind.Shadow:
            case NerdTokenTreeTargetKind.Opacity:
                _activeTabIndex = 0;
                if (!string.IsNullOrWhiteSpace(navigation.AnchorId))
                {
                    await Js.InvokeVoidAsync("nerdShared.scrollToElement", navigation.AnchorId);
                }
                break;
            case NerdTokenTreeTargetKind.Recipe:
                if (NerdRecipePlayBookLinks.TryGetLayoutKitAnchor(navigation.TargetId) is not null)
                {
                    NavigationManager.NavigateTo(
                        NerdRecipePlayBookLinks.BuildPlayBookUrl(HubOptions.PlayBookRoute, navigation.TargetId));
                }
                else
                {
                    NavigationManager.NavigateTo($"{Options.RecipesCatalogRoute}#{navigation.TargetId}");
                }
                break;
            case NerdTokenTreeTargetKind.RecipeRole:
                OpenTokenTab(navigation.TargetId);
                break;
            case NerdTokenTreeTargetKind.Breakpoint:
            case NerdTokenTreeTargetKind.MotionDuration:
            case NerdTokenTreeTargetKind.MotionEasing:
            case NerdTokenTreeTargetKind.ZIndex:
                _activeTabIndex = 0;
                if (!string.IsNullOrWhiteSpace(navigation.AnchorId))
                {
                    await Js.InvokeVoidAsync("nerdShared.scrollToElement", navigation.AnchorId);
                }
                break;
            case NerdTokenTreeTargetKind.ThemeSet:
                await OnThemeSetChanged(navigation.TargetId);
                break;
        }

        StateHasChanged();
    }

    private Task OnThemeSetChanged(string themeSetId)
    {
        _activeThemeSet = themeSetId ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_activeThemeSet))
        {
            NerdBrandPackRegistry.Instance.Configure(BrandSwitcher.ActiveBrandId, Options);
        }
        else
        {
            NerdThemeSetTools.SyncColorTokensFromThemeSets(Options);
        }

        _previewDark = string.Equals(_activeThemeSet, "dark", StringComparison.OrdinalIgnoreCase);
        _dualPreview = string.IsNullOrWhiteSpace(_activeThemeSet);
        TokenCss.Update(Options);
        RefreshCatalogState();
        StateHasChanged();
        return Task.CompletedTask;
    }

    private void RefreshPackDiff()
    {
        if (!NerdBrandPackRegistry.Instance.TryGet(_diffBaseline, out _))
        {
            _packDiff = [];
            return;
        }

        _packDiff = NerdTokenPackDiff.CompareToPreset(_diffBaseline, Options, _clientId);
    }

    private Task OnDiffBaselineChanged(string baseline)
    {
        _diffBaseline = baseline;
        RefreshPackDiff();
        return Task.CompletedTask;
    }

    private void OnGlobalBrandChanged(string brand)
    {
        _diffBaseline = brand;
        RefreshCatalogTheme();
        RefreshCatalogState();
        RefreshPackDiff();
        _saveStatus = $"Switched to {brand} brand.";
        InvokeAsync(StateHasChanged);
    }

    private void RefreshCatalogTheme()
    {
        _catalogTheme = NerdCatalogThemeResolver.CreateForCatalog(
            Options,
            ThemeController,
            ThemeController is null && TypographyOptions is not null
                ? theme => theme.UseResponsiveTypography(TypographyOptions.Typography)
                : null,
            ThemeConfigurator);
    }

    private static string DiffSymbol(NerdTokenPackDiffKind kind) => kind switch
    {
        NerdTokenPackDiffKind.Added => "+",
        NerdTokenPackDiffKind.Removed => "-",
        NerdTokenPackDiffKind.Modified => "~",
        _ => "?"
    };

    private IEnumerable<string> GetAliasUsages(string aliasName, string targetToken) =>
        NerdAliasUsageTools.GetUsages(Options, aliasName, targetToken);

    private void ToggleFavorite(string tokenName)
    {
        if (!_favoriteTokens.Add(tokenName))
        {
            _favoriteTokens.Remove(tokenName);
        }

        StateHasChanged();
    }

    private bool IsFavorite(string tokenName) => _favoriteTokens.Contains(tokenName);

    private Task RefreshAfterDtcgImportAsync()
    {
        NerdFoundationTaxonomyTools.ApplyDefaults(Options);
        NerdTokenTransformTools.ApplyTransforms(Options, Options.Transforms, overwrite: true);
        TokenCss.Update(Options);
        RefreshCatalogState();
        return Task.CompletedTask;
    }

    private Task DownloadTokensStudioAsync() =>
        DownloadService.DownloadTextAsync(
            $"{Options.Prefix}-tokens-studio.json",
            NerdDesignTokenTools.ExportTokensStudioJson(Options)).AsTask();

    private Task DownloadDtcgAsync() =>
        DownloadService.DownloadTextAsync(
            $"{Options.Prefix}-design-tokens.dtcg.json",
            NerdDtcgTokenTools.Export(Options)).AsTask();

    private NerdAccessibilityResult? GetAccessibility(string tokenName) =>
        _accessibility.FirstOrDefault(result =>
            string.Equals(result.Name, tokenName, StringComparison.OrdinalIgnoreCase));

    private static string ResolveColor(NerdColorToken token, bool dark) =>
        dark ? token.Dark ?? token.Light ?? token.Value : token.Light ?? token.Value;

    private static string ResolveContrastText(NerdColorToken token, NerdAccessibilityResult? accessibility, bool dark)
    {
        if (dark)
        {
            return token.DarkContrastText
                   ?? token.ContrastText
                   ?? accessibility?.Dark.RecommendedForeground
                   ?? NerdColorValue.ContrastText(token.Dark ?? token.Light ?? token.Value);
        }

        return token.ContrastText
               ?? accessibility?.Light.RecommendedForeground
               ?? NerdColorValue.ContrastText(token.Light ?? token.Value);
    }

    private static string FormatRatio(double ratio) => NerdDesignTokenCatalogRendering.FormatRatio(ratio);

    private static RenderFragment ColorValue(string? value) => NerdDesignTokenCatalogRendering.ColorValue(value);

    private Task DownloadCssAsync() =>
        DownloadService.DownloadTextAsync($"{Options.Prefix}-tokens.css", MudBlazorDesignTokenCssGenerator.Generate(Options)).AsTask();

    private Task DownloadJsonAsync() =>
        DownloadService.DownloadTextAsync($"{Options.Prefix}-tokens.json", NerdDesignTokenTools.ExportJson(Options)).AsTask();

    private Task DownloadStitchAsync() =>
        DownloadService.DownloadTextAsync(
            "DESIGN.md",
            NerdDesignTokenTools.ExportStitchDesignMd(Options, GetTypographyRolesForExport())).AsTask();

    private IReadOnlyDictionary<string, string>? GetTypographyRolesForExport()
    {
        if (!BrandPackSource.IsConfigured)
        {
            return null;
        }

        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(BrandPackSource.ExportTypographyJson());
            if (!document.RootElement.TryGetProperty("roles", out var rolesElement) ||
                rolesElement.ValueKind != System.Text.Json.JsonValueKind.Object)
            {
                return null;
            }

            var roles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in rolesElement.EnumerateObject())
            {
                roles[property.Name] = property.Value.GetString() ?? string.Empty;
            }

            return roles;
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    private async Task ImportTokenPackAsync(InputFileChangeEventArgs args)
    {
        var file = args.File;
        if (file is null)
        {
            return;
        }

        await using var stream = file.OpenReadStream(maxAllowedSize: 2_000_000);
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        var result = NerdTokenPackImporter.TryImport(json);
        if (!result.Success || result.Pack is null)
        {
            _saveStatus = result.Message;
            return;
        }

        result.Pack.ApplyTo(Options);
        TokenCss.Update(Options);
        HubOptions.ActiveTokenPackId = result.Pack.ClientId;
        _clientId = result.Pack.ClientId;
        RefreshCatalogState();
        _saveStatus = $"Imported {result.Pack.ClientId}.";
    }

    private async Task SaveClientPackAsync()
    {
        if (!Entitlements.CanSaveAnotherPack(_packIds.Count))
        {
            _saveStatus = "Saving client packs is not enabled for the current licence.";
            return;
        }

        await TokenPackStore.SaveAsync(NerdTokenPack.FromOptions(Options, _clientId));
        _saveStatus = $"Saved {_clientId}.";
        _packIds = await TokenPackStore.ListAsync();
        _selectedPackId = _clientId;
        HubOptions.ActiveTokenPackId = _clientId;
    }

    private async Task LoadClientPackAsync()
    {
        if (string.IsNullOrWhiteSpace(_selectedPackId))
        {
            return;
        }

        var pack = await TokenPackStore.LoadAsync(_selectedPackId);
        if (pack is null)
        {
            _saveStatus = $"Pack '{_selectedPackId}' not found.";
            return;
        }

        pack.ApplyTo(Options);
        TokenCss.Update(Options);
        HubOptions.ActiveTokenPackId = pack.ClientId;
        RefreshCatalogState();
        _saveStatus = $"Loaded {pack.ClientId}.";
    }

    public void Dispose() => BrandSwitcher.BrandChanged -= OnGlobalBrandChanged;
}
