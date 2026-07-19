using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MudBlazor;
using TheNerdCollective.MudComponents.ResponsiveTypography;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

public partial class NerdDesignTokenRecipesCatalog
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
    private NerdDesignTokenCss TokenCss { get; set; } = default!;

    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = default!;

    [Inject]
    private IEnumerable<INerdBrandPack> BrandPacks { get; set; } = [];

    private bool _previewDark;
    private bool _contrastDark;
    private string? _studioToken;
    private string _studioColor = "#365C3A";
    private string _studioSurface = "kridt";
    private string _studioContent = "skov";
    private string _studioAction = "himmel";
    private string _recipeName = "custom";
    private string _recipeSurface = "kridt";
    private string _recipeContent = "skov";
    private string _recipeAction = "himmel";
    private string? _saveStatus;
    private bool _showContrastMatrix;
    private string _selectedBrand = string.Empty;
    private IReadOnlyList<NerdAccessibilityWarning> _warnings = [];
    private IReadOnlyList<NerdContrastPairResult> _contrastMatrix = [];
    private Dictionary<(string Foreground, string Background), NerdContrastPairResult> _contrastMatrixByKey = [];
    private NerdBrandHealthResult? _brandHealth;
    private NerdManualComplianceResult? _manualCompliance;
    private readonly Stack<NerdTokenPack> _undoStack = new();
    private readonly Stack<NerdTokenPack> _redoStack = new();

    private IEnumerable<INerdBrandPack> InstalledBrandPacks =>
        BrandPacks.OrderBy(pack => pack.Id, StringComparer.OrdinalIgnoreCase);

    private INerdPairingPolicy? ActivePairingPolicy =>
        Options.PairingPolicy is not null && Options.PairingPolicy.IsActive(Options)
            ? Options.PairingPolicy
            : null;

    private bool HasBrandPairingGuide =>
        ActivePairingPolicy?.GetApprovedPairings().Count > 0;

    private string BrandGuideName =>
        ActivePairingPolicy?.BrandGuideName ?? "brand";

    private bool CanUndo => _undoStack.Count > 0;
    private bool CanRedo => _redoStack.Count > 0;

    private bool IsAvailable =>
        Options.EnableCatalogPage &&
        (!Options.RestrictCatalogToDevelopment || HostEnvironment.IsDevelopment());

    private string StudioPairingLabel => $"{_studioContent} on {_studioSurface}";

    private NerdDesignTokenRecipe StudioPreviewRecipe =>
        new(_studioSurface, _studioContent, _studioAction);

    private string StudioPreviewRecipeName =>
        string.IsNullOrWhiteSpace(_studioToken) ? "studio" : $"{_studioToken}-preview";

    private NerdTokenPairingValidation StudioPairingValidation =>
        NerdTokenPairingTools.ValidatePairing(_studioContent, _studioSurface, Options, ResolveStudioPairingColor);

    private string? StudioActionClassName =>
        string.IsNullOrWhiteSpace(_studioAction) ? null : GetClassName(_studioAction);

    private string ComposerClassName => $"{Options.Prefix}-recipe-{_recipeName}";

    private IEnumerable<string> TokenNames =>
        Options.Colors.Keys.OrderBy(name => name, StringComparer.Ordinal);

    private IEnumerable<string> StudioSurfaceOptions =>
        NerdRecipeStudioTools.HasRecipes(Options)
            ? NerdRecipeStudioTools.GetStudioSurfaceOptions(Options)
            : TokenNames;

    private IEnumerable<string> StudioContentOptions =>
        NerdRecipeStudioTools.HasRecipes(Options)
            ? NerdRecipeStudioTools.GetStudioContentOptions(_studioSurface, Options)
            : ActivePairingPolicy is not null
                ? ActivePairingPolicy.GetApprovedContentTokens(_studioSurface, Options)
                : TokenNames;

    private IEnumerable<string> StudioActionOptions =>
        NerdRecipeStudioTools.HasRecipes(Options)
            ? NerdRecipeStudioTools.GetStudioActionOptions(_studioSurface, _studioContent, Options)
            : TokenNames;

    protected override void OnInitialized()
    {
        if (!IsAvailable)
        {
            return;
        }

        _selectedBrand = Options.Prefix;
        RefreshCatalogState();
    }

    private string GetClassName(string tokenName) => $"{Options.Prefix}-{tokenName}";

    private string GetRecipeClassName(string recipeName) => $"{Options.Prefix}-recipe-{recipeName}";

    private string Ui(string semanticAlias) => NerdDesignSystemUi.TokenClass(Options.Prefix, semanticAlias);

    private static string FormatRatio(double ratio) => NerdDesignTokenCatalogRendering.FormatRatio(ratio);

    private static RenderFragment ColorValue(string? value) =>
        NerdDesignTokenCatalogRendering.ColorValue(value);

    private void RefreshCatalogState()
    {
        _warnings = NerdDesignTokenTools.GetAccessibilityWarnings(Options);
        RebuildContrastMatrix();
        _brandHealth = NerdBrandHealthTools.Evaluate(Options);
        _manualCompliance = NerdManualComplianceTools.Evaluate(Options);
        HubOptions.DesignTokenRecipeCount = Options.Recipes.Count;
        HubOptions.DesignTokenCount = Options.Colors.Count;
        HubOptions.ActiveBrandIdentityVersion = Options.ActiveBrandIdentityVersion;

        var firstToken = Options.Colors.Keys.OrderBy(name => name, StringComparer.Ordinal).FirstOrDefault();
        if (firstToken is not null && string.IsNullOrWhiteSpace(_studioToken))
        {
            SelectStudioToken(firstToken);
        }
    }

    private void SelectStudioToken(string tokenName)
    {
        _studioToken = tokenName;
        _studioColor = Options.Colors[tokenName].Value;

        if (NerdRecipeStudioTools.HasRecipes(Options))
        {
            var recipe = NerdRecipeStudioTools.FindRecipeForToken(tokenName, Options)
                ?? NerdRecipeStudioTools.GetDefaultRecipe(Options);
            ApplyStudioRecipe(recipe.Key, recipe.Value);
            return;
        }

        ApplyStudioPairingFromPolicy(tokenName);
    }

    private void ApplyStudioRecipe(string recipeName, NerdDesignTokenRecipe recipe)
    {
        _studioSurface = recipe.Surface;
        _studioContent = recipe.Content;
        _studioAction = recipe.Action
            ?? NerdTokenPairingTools.SuggestActionToken(Options, recipe.Surface, recipe.Content);
        _recipeName = recipeName;
        _recipeSurface = recipe.Surface;
        _recipeContent = recipe.Content;
        _recipeAction = _studioAction;
    }

    private void ApplyStudioPairingFromPolicy(string? editedToken = null)
    {
        if (ActivePairingPolicy is not null)
        {
            var pairings = ActivePairingPolicy.GetApprovedPairings();
            var pairing = editedToken is null
                ? pairings[0]
                : pairings.FirstOrDefault(pair =>
                      string.Equals(pair.Content, editedToken, StringComparison.OrdinalIgnoreCase) ||
                      string.Equals(pair.Surface, editedToken, StringComparison.OrdinalIgnoreCase));
            if (pairing != default)
            {
                _studioSurface = pairing.Surface;
                _studioContent = pairing.Content;
                _studioAction = ActivePairingPolicy.SuggestActionToken(Options, _studioSurface, _studioContent);
                return;
            }
        }

        _studioSurface = TokenNames.First();
        _studioContent = NerdTokenPairingTools.SuggestContentToken(_studioSurface, Options);
        _studioAction = NerdTokenPairingTools.SuggestActionToken(Options, _studioSurface, _studioContent);
    }

    private string ResolveStudioTokenColor(string tokenName) =>
        string.Equals(tokenName, _studioToken, StringComparison.OrdinalIgnoreCase)
            ? _studioColor
            : NerdTokenPairingTools.ResolveTokenColor(tokenName, Options);

    private string ResolveStudioPairingColor(string tokenName)
    {
        if (string.Equals(tokenName, _studioToken, StringComparison.OrdinalIgnoreCase))
        {
            return _studioColor;
        }

        if (string.Equals(tokenName, _studioContent, StringComparison.OrdinalIgnoreCase))
        {
            return NerdTokenPairingTools.ResolvePairingForegroundColor(tokenName, Options);
        }

        if (string.Equals(tokenName, _studioSurface, StringComparison.OrdinalIgnoreCase))
        {
            return NerdTokenPairingTools.ResolvePairingSurfaceColor(tokenName, Options);
        }

        return NerdTokenPairingTools.ResolveTokenColor(tokenName, Options);
    }

    private Task OnStudioSurfaceChanged(string surfaceToken)
    {
        if (NerdRecipeStudioTools.HasRecipes(Options))
        {
            var recipe = NerdRecipeStudioTools.FindRecipeForSurface(surfaceToken, Options);
            if (recipe.HasValue)
            {
                ApplyStudioRecipe(recipe.Value.Key, recipe.Value.Value);
                return Task.CompletedTask;
            }
        }

        _studioSurface = surfaceToken;
        ApplyStudioPairingFromPolicy();
        return Task.CompletedTask;
    }

    private Task OnStudioContentChanged(string contentToken)
    {
        if (NerdRecipeStudioTools.HasRecipes(Options))
        {
            var recipe = NerdRecipeStudioTools.FindRecipeForPairing(_studioSurface, contentToken, Options);
            if (recipe.HasValue)
            {
                ApplyStudioRecipe(recipe.Value.Key, recipe.Value.Value);
                return Task.CompletedTask;
            }
        }

        _studioContent = contentToken;
        _studioAction = NerdTokenPairingTools.SuggestActionToken(Options, _studioSurface, _studioContent);
        return Task.CompletedTask;
    }

    private Task OnStudioActionChanged(string actionToken)
    {
        if (!StudioActionOptions.Any(name =>
                string.Equals(name, actionToken, StringComparison.OrdinalIgnoreCase)))
        {
            _studioAction = StudioActionOptions.First();
            return Task.CompletedTask;
        }

        _studioAction = actionToken;
        return Task.CompletedTask;
    }

    private Task OnStudioTokenChanged(string? tokenName)
    {
        if (!string.IsNullOrWhiteSpace(tokenName))
        {
            SelectStudioToken(tokenName);
        }

        return Task.CompletedTask;
    }

    private void PushUndoSnapshot()
    {
        _undoStack.Push(NerdTokenPack.FromOptions(Options));
        _redoStack.Clear();
    }

    private Task ApplyStudioColorAsync()
    {
        if (string.IsNullOrWhiteSpace(_studioToken))
        {
            return Task.CompletedTask;
        }

        if (Options.IsTokenLocked(_studioToken))
        {
            _saveStatus = $"Token '{_studioToken}' is locked by the brand guide.";
            return Task.CompletedTask;
        }

        PushUndoSnapshot();
        var existing = Options.Colors[_studioToken];
        var validated = NerdColorValue.Validate(_studioColor, nameof(_studioColor));
        var updated = new NerdColorToken
        {
            Value = validated,
            Light = validated,
            Dark = existing.Dark,
            ContrastText = NerdColorValue.ContrastText(validated),
            DarkContrastText = existing.DarkContrastText,
            Hover = existing.Hover,
            Active = existing.Active,
            Border = existing.Border,
            Surface = existing.Surface,
            Content = existing.Content,
            Interactive = existing.Interactive,
            Disabled = existing.Disabled
        };
        Options.Add(_studioToken, updated);
        TokenCss.Update(Options);
        RefreshCatalogState();
        SelectStudioToken(_studioToken);
        _saveStatus = $"Applied {_studioToken}.";
        return Task.CompletedTask;
    }

    private Task UndoStudioEditAsync()
    {
        if (_undoStack.Count == 0)
        {
            return Task.CompletedTask;
        }

        _redoStack.Push(NerdTokenPack.FromOptions(Options));
        _undoStack.Pop().ApplyTo(Options);
        TokenCss.Update(Options);
        RefreshCatalogState();
        if (_studioToken is not null)
        {
            SelectStudioToken(_studioToken);
        }

        _saveStatus = "Undone studio edit.";
        return Task.CompletedTask;
    }

    private Task RedoStudioEditAsync()
    {
        if (_redoStack.Count == 0)
        {
            return Task.CompletedTask;
        }

        _undoStack.Push(NerdTokenPack.FromOptions(Options));
        _redoStack.Pop().ApplyTo(Options);
        TokenCss.Update(Options);
        RefreshCatalogState();
        if (_studioToken is not null)
        {
            SelectStudioToken(_studioToken);
        }

        _saveStatus = "Redone studio edit.";
        return Task.CompletedTask;
    }

    private void RefreshContrastMatrix() => RebuildContrastMatrix();

    private void RebuildContrastMatrix()
    {
        _contrastMatrix = NerdDesignTokenTools.BuildContrastMatrix(Options, _contrastDark);
        _contrastMatrixByKey = _contrastMatrix.ToDictionary(
            result => (result.ForegroundToken, result.BackgroundToken));
    }

    private NerdContrastPairResult? GetContrastPair(string foregroundToken, string backgroundToken) =>
        _contrastMatrixByKey.TryGetValue((foregroundToken, backgroundToken), out var result)
            ? result
            : null;

    private NerdTokenPairingValidation ValidateApprovedPairing(string content, string surface) =>
        NerdTokenPairingTools.ValidatePairing(content, surface, Options);

    private Task SwitchBrandAsync(string brand)
    {
        _selectedBrand = brand;
        NerdBrandPackRegistry.Instance.Configure(brand, Options);
        TokenCss.Update(Options);
        HubOptions.ActiveTokenPackId = brand;
        ApplyBrandTypography(brand);
        _undoStack.Clear();
        _redoStack.Clear();
        _studioToken = null;
        RefreshCatalogState();
        _saveStatus = $"Switched to {brand} brand.";
        return Task.CompletedTask;
    }

    private void ApplyBrandTypography(string brand)
    {
        var typographyOptions = ServiceProvider.GetService<NerdResponsiveTypographyOptions>();
        if (typographyOptions is null)
        {
            return;
        }

        NerdBrandTypographySwitcher.TrySwitchBrand(
            brand,
            typographyOptions,
            HubOptions,
            ServiceProvider.GetService<MudTheme>());
    }

    private Task DownloadCssAsync() =>
        DownloadService.DownloadTextAsync(
            $"{Options.Prefix}-tokens.css",
            MudBlazorDesignTokenCssGenerator.Generate(Options)).AsTask();
}
