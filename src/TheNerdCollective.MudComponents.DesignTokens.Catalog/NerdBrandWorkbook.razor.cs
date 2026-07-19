using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MudBlazor;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

public partial class NerdBrandWorkbook
{
    [Inject]
    private NerdDesignTokenOptions Options { get; set; } = default!;

    [Inject]
    private NerdDesignSystemOptions HubOptions { get; set; } = default!;

    [Inject]
    private NerdDesignTokenCss TokenCss { get; set; } = default!;

    [Inject]
    private NerdDownloadService DownloadService { get; set; } = default!;

    [Inject]
    private IWebHostEnvironment HostEnvironment { get; set; } = default!;

    [Inject]
    private INerdCatalogEntitlements Entitlements { get; set; } = default!;

    private const int ExportStepIndex = 5;

    private int _activeStep;
    private string? _status;
    private Severity _statusSeverity = Severity.Success;

    private string _newTokenName = string.Empty;
    private string _newTokenValue = "#336699";
    private string _newAliasName = string.Empty;
    private string _newAliasTarget = string.Empty;
    private string _newRecipeName = "custom";
    private string _newRecipeSurface = string.Empty;
    private string _newRecipeContent = string.Empty;
    private string? _newRecipeAction;
    private string? _newRecipeLabel;
    private string? _newRecipeUsage;
    private string _newPairingContent = string.Empty;
    private string _newPairingSurface = string.Empty;
    private string _pairingGuideName = "brand";
    private readonly List<NerdApprovedPairing> _approvedPairings = [];

    private bool IsAvailable =>
        Options.EnableCatalogPage &&
        (!Options.RestrictCatalogToDevelopment || HostEnvironment.IsDevelopment());

    private IEnumerable<string> TokenNames =>
        Options.Colors.Keys.OrderBy(name => name, StringComparer.Ordinal);

    private NerdDesignTokenRecipe? PreviewRecipe =>
        Options.Recipes.OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => pair.Value).FirstOrDefault();

    private string PreviewRecipeName =>
        Options.Recipes.OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => pair.Key).FirstOrDefault() ?? "preview";

    private NerdTokenPairingValidation? PreviewValidation =>
        PreviewRecipe is null
            ? null
            : NerdTokenPairingTools.ValidatePairing(PreviewRecipe.Content, PreviewRecipe.Surface, Options);

    private string Ui(string semanticAlias) =>
        NerdDesignSystemUi.TokenClass(Options.Prefix, semanticAlias);

    protected override void OnInitialized()
    {
        if (!IsAvailable)
        {
            return;
        }

        LoadPairingsFromOptions();
        EnsureEditorDefaults();
    }

    private void EnsureEditorDefaults()
    {
        _newAliasTarget = TokenNames.FirstOrDefault() ?? string.Empty;
        _newRecipeSurface = TokenNames.FirstOrDefault() ?? string.Empty;
        _newRecipeContent = TokenNames.Skip(1).FirstOrDefault() ?? _newRecipeSurface;
        _newPairingSurface = _newRecipeSurface;
        _newPairingContent = _newRecipeContent;
    }

    private void LoadPairingsFromOptions()
    {
        _approvedPairings.Clear();
        if (Options.PairingPolicy is null)
        {
            return;
        }

        _pairingGuideName = Options.PairingPolicy.BrandGuideName;
        foreach (var (content, surface) in Options.PairingPolicy.GetApprovedPairings())
        {
            _approvedPairings.Add(new NerdApprovedPairing(content, surface));
        }
    }

    private Task ApplyImportedPackAsync(NerdTokenPack pack)
    {
        pack.ApplyTo(Options);
        RefreshAfterEdit();
        _status = $"Imported {pack.DisplayName ?? pack.ClientId}.";
        _statusSeverity = Severity.Success;
        LoadPairingsFromOptions();
        EnsureEditorDefaults();
        _activeStep = 1;
        return Task.CompletedTask;
    }

    private void RefreshAfterEdit()
    {
        SyncPairingPolicy();
        TokenCss.Update(Options);
        HubOptions.ActiveTokenPackId = Options.ActiveBrandPackId ?? Options.Prefix;
        HubOptions.DesignTokenCount = Options.Colors.Count;
        HubOptions.DesignTokenRecipeCount = Options.Recipes.Count;
    }

    private void SyncPairingPolicy()
    {
        if (_approvedPairings.Count == 0 || string.IsNullOrWhiteSpace(_pairingGuideName))
        {
            return;
        }

        Options.PairingPolicy = new NerdJsonPairingPolicy(_pairingGuideName, _approvedPairings);
    }

    private Task AddColorAsync()
    {
        if (string.IsNullOrWhiteSpace(_newTokenName))
        {
            return Task.CompletedTask;
        }

        try
        {
            var validated = NerdColorValue.Validate(_newTokenValue, nameof(_newTokenValue));
            Options.Add(_newTokenName, new NerdColorToken
            {
                Value = validated,
                ContrastText = NerdColorValue.ContrastText(validated)
            });
            RefreshAfterEdit();
            _status = $"Added color '{_newTokenName}'.";
            _statusSeverity = Severity.Success;
            _newTokenName = string.Empty;
            EnsureEditorDefaults();
        }
        catch (Exception ex)
        {
            _status = ex.Message;
            _statusSeverity = Severity.Error;
        }

        return Task.CompletedTask;
    }

    private Task UpdateColorAsync(string tokenName, string value)
    {
        if (Options.IsTokenLocked(tokenName) || !Options.Colors.ContainsKey(tokenName))
        {
            return Task.CompletedTask;
        }

        try
        {
            var existing = Options.Colors[tokenName];
            var validated = NerdColorValue.Validate(value, nameof(value));
            Options.Add(tokenName, new NerdColorToken
            {
                Value = validated,
                Light = validated,
                Dark = existing.Dark,
                ContrastText = NerdColorValue.ContrastText(validated),
                DarkContrastText = existing.DarkContrastText,
                Surface = existing.Surface,
                Content = existing.Content,
                Interactive = existing.Interactive,
                Hover = existing.Hover,
                Active = existing.Active,
                Border = existing.Border,
                Disabled = existing.Disabled,
                Description = existing.Description,
                Roles = existing.Roles
            });
            RefreshAfterEdit();
            _status = $"Updated '{tokenName}'.";
            _statusSeverity = Severity.Success;
        }
        catch (Exception ex)
        {
            _status = ex.Message;
            _statusSeverity = Severity.Error;
        }

        return Task.CompletedTask;
    }

    private Task RemoveColorAsync(string tokenName)
    {
        if (!Options.RemoveColor(tokenName))
        {
            _status = $"Token '{tokenName}' is locked or missing.";
            _statusSeverity = Severity.Warning;
            return Task.CompletedTask;
        }

        RefreshAfterEdit();
        _status = $"Removed '{tokenName}'.";
        _statusSeverity = Severity.Success;
        EnsureEditorDefaults();
        return Task.CompletedTask;
    }

    private Task AddAliasAsync()
    {
        if (string.IsNullOrWhiteSpace(_newAliasName) || string.IsNullOrWhiteSpace(_newAliasTarget))
        {
            return Task.CompletedTask;
        }

        try
        {
            Options.Alias(_newAliasName, _newAliasTarget);
            RefreshAfterEdit();
            _status = $"Added alias '{_newAliasName}'.";
            _statusSeverity = Severity.Success;
            _newAliasName = string.Empty;
        }
        catch (Exception ex)
        {
            _status = ex.Message;
            _statusSeverity = Severity.Error;
        }

        return Task.CompletedTask;
    }

    private Task UpdateAliasAsync(string aliasName, string target)
    {
        try
        {
            Options.Alias(aliasName, target);
            RefreshAfterEdit();
            _status = $"Updated alias '{aliasName}'.";
            _statusSeverity = Severity.Success;
        }
        catch (Exception ex)
        {
            _status = ex.Message;
            _statusSeverity = Severity.Error;
        }

        return Task.CompletedTask;
    }

    private Task RemoveAliasAsync(string aliasName)
    {
        Options.RemoveAlias(aliasName);
        RefreshAfterEdit();
        _status = $"Removed alias '{aliasName}'.";
        _statusSeverity = Severity.Success;
        return Task.CompletedTask;
    }

    private Task AddRecipeAsync()
    {
        if (string.IsNullOrWhiteSpace(_newRecipeName) ||
            string.IsNullOrWhiteSpace(_newRecipeSurface) ||
            string.IsNullOrWhiteSpace(_newRecipeContent))
        {
            return Task.CompletedTask;
        }

        try
        {
            Options.AddRecipe(
                _newRecipeName,
                new NerdDesignTokenRecipe(
                    _newRecipeSurface,
                    _newRecipeContent,
                    string.IsNullOrWhiteSpace(_newRecipeAction) ? null : _newRecipeAction,
                    Label: string.IsNullOrWhiteSpace(_newRecipeLabel) ? null : _newRecipeLabel,
                    Usage: string.IsNullOrWhiteSpace(_newRecipeUsage) ? null : _newRecipeUsage));
            RefreshAfterEdit();
            _status = $"Added recipe '{_newRecipeName}'.";
            _statusSeverity = Severity.Success;
            _newRecipeName = "custom";
            _newRecipeLabel = null;
            _newRecipeUsage = null;
        }
        catch (Exception ex)
        {
            _status = ex.Message;
            _statusSeverity = Severity.Error;
        }

        return Task.CompletedTask;
    }

    private Task UpdateRecipeAsync(
        string recipeName,
        string surface,
        string content,
        string? action,
        string? label,
        string? usage)
    {
        try
        {
            Options.AddRecipe(
                recipeName,
                new NerdDesignTokenRecipe(
                    surface,
                    content,
                    string.IsNullOrWhiteSpace(action) ? null : action,
                    Label: string.IsNullOrWhiteSpace(label) ? null : label,
                    Usage: string.IsNullOrWhiteSpace(usage) ? null : usage));
            RefreshAfterEdit();
            _status = $"Updated recipe '{recipeName}'.";
            _statusSeverity = Severity.Success;
        }
        catch (Exception ex)
        {
            _status = ex.Message;
            _statusSeverity = Severity.Error;
        }

        return Task.CompletedTask;
    }

    private Task RemoveRecipeAsync(string recipeName)
    {
        Options.RemoveRecipe(recipeName);
        RefreshAfterEdit();
        _status = $"Removed recipe '{recipeName}'.";
        _statusSeverity = Severity.Success;
        return Task.CompletedTask;
    }

    private Task AddPairingAsync()
    {
        if (string.IsNullOrWhiteSpace(_newPairingContent) || string.IsNullOrWhiteSpace(_newPairingSurface))
        {
            return Task.CompletedTask;
        }

        if (_approvedPairings.Any(pair =>
                string.Equals(pair.Content, _newPairingContent, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(pair.Surface, _newPairingSurface, StringComparison.OrdinalIgnoreCase)))
        {
            _status = "Pairing already exists.";
            _statusSeverity = Severity.Warning;
            return Task.CompletedTask;
        }

        _approvedPairings.Add(new NerdApprovedPairing(_newPairingContent, _newPairingSurface));
        RefreshAfterEdit();
        _status = $"Added pairing {_newPairingContent} on {_newPairingSurface}.";
        _statusSeverity = Severity.Success;
        return Task.CompletedTask;
    }

    private Task RemovePairingAsync(NerdApprovedPairing pairing)
    {
        _approvedPairings.RemoveAll(pair =>
            string.Equals(pair.Content, pairing.Content, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(pair.Surface, pairing.Surface, StringComparison.OrdinalIgnoreCase));
        RefreshAfterEdit();
        _status = $"Removed pairing {pairing.Content} on {pairing.Surface}.";
        _statusSeverity = Severity.Success;
        return Task.CompletedTask;
    }

    private Task OnPairingGuideNameChanged(string value)
    {
        _pairingGuideName = value;
        RefreshAfterEdit();
        return Task.CompletedTask;
    }

    private Task DownloadPackJsonAsync()
    {
        var pack = NerdTokenPackEnricher.WithPairingPolicy(
            NerdTokenPack.FromOptions(Options, Options.ActiveBrandPackId ?? Options.Prefix),
            Options.PairingPolicy);
        pack = new NerdTokenPack
        {
            ClientId = pack.ClientId,
            BrandId = pack.BrandId,
            DisplayName = pack.DisplayName,
            Prefix = pack.Prefix,
            Version = pack.Version,
            BrandIdentityVersion = pack.BrandIdentityVersion,
            PairingGuideName = _pairingGuideName,
            Colors = new(pack.Colors, StringComparer.OrdinalIgnoreCase),
            Aliases = new(pack.Aliases, StringComparer.OrdinalIgnoreCase),
            Radii = new(pack.Radii, StringComparer.OrdinalIgnoreCase),
            Shadows = new(pack.Shadows, StringComparer.OrdinalIgnoreCase),
            Recipes = new(pack.Recipes, StringComparer.OrdinalIgnoreCase),
            Opacities = new(pack.Opacities, StringComparer.OrdinalIgnoreCase),
            ApprovedPairings = [.._approvedPairings],
            LockedTokens = [..pack.LockedTokens]
        };
        return DownloadService.DownloadTextAsync($"{Options.Prefix}-token-pack.json", pack.ToJson()).AsTask();
    }
}
