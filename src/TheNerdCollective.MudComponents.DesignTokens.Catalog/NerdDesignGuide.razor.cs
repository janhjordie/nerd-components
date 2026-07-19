using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MudBlazor;
using TheNerdCollective.MudComponents.ResponsiveTypography;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

public partial class NerdDesignGuide
{
    [Inject]
    private NerdDesignTokenOptions Options { get; set; } = default!;

    [Inject]
    private NerdDesignSystemOptions HubOptions { get; set; } = default!;

    [Inject]
    private IWebHostEnvironment HostEnvironment { get; set; } = default!;

    [Inject]
    private NerdDesignTokenCss TokenCss { get; set; } = default!;

    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = default!;

    [Inject]
    private IEnumerable<INerdBrandPack> BrandPacks { get; set; } = [];

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private string _selectedBrand = string.Empty;
    private string _clientId = "client";
    private NerdDesignParityResult _parity = new(0, []);
    private NerdMudStateParityResult _mudStateParity = new(0, []);
    private NerdMudPaletteFidelityResult _mudPaletteFidelity = new(0, []);
    private IReadOnlyList<NerdComponentFamilyBinding> _familyBindings = [];

    private IEnumerable<INerdBrandPack> InstalledBrandPacks =>
        BrandPacks.OrderBy(pack => pack.Id, StringComparer.OrdinalIgnoreCase);

    private bool IsAvailable =>
        HubOptions.EnableDesignGuidePage &&
        (!HubOptions.RestrictDesignGuideToDevelopment || HostEnvironment.IsDevelopment());

    private bool HasApprovedPairings =>
        ActivePairingPolicy?.GetApprovedPairings().Any() == true;

    private INerdPairingPolicy? ActivePairingPolicy =>
        Options.PairingPolicy is not null && Options.PairingPolicy.IsActive(Options)
            ? Options.PairingPolicy
            : null;

    private string BrandGuideName =>
        ActivePairingPolicy?.BrandGuideName ?? "Brand";

    private string DesignGuideUrl =>
        Navigation.ToAbsoluteUri(HubOptions.DesignGuideRoute).ToString();

    private Color ParityColor =>
        _parity.Score >= 80 ? Color.Success : _parity.Score >= 50 ? Color.Warning : Color.Error;

    private Color MudStateParityColor =>
        _mudStateParity.Score >= 80 ? Color.Success : _mudStateParity.Score >= 50 ? Color.Warning : Color.Error;

    private Color MudPaletteFidelityColor =>
        _mudPaletteFidelity.Score >= 80 ? Color.Success : _mudPaletteFidelity.Score >= 50 ? Color.Warning : Color.Error;

    protected override void OnInitialized()
    {
        if (!IsAvailable)
        {
            return;
        }

        _selectedBrand = Options.Prefix;
        _clientId = Options.ActiveBrandPackId ?? Options.Prefix;
        RefreshParity();
    }

    private string Ui(string semanticAlias) =>
        NerdDesignSystemUi.TokenClass(Options.Prefix, semanticAlias);

    private static string FormatRatio(double ratio) =>
        NerdDesignTokenCatalogRendering.FormatRatio(ratio);

    private void RefreshParity()
    {
        var includeExtended = string.Equals(Options.Prefix, "dnf", StringComparison.OrdinalIgnoreCase);
        _parity = NerdDesignParityTools.Evaluate(Options, includeExtended);
        _mudStateParity = NerdMudStateParityTools.Evaluate(Options);
        _mudPaletteFidelity = NerdMudPaletteParityTools.Evaluate(Options);
        _familyBindings = NerdComponentFamilyTools.ResolveBindings(Options);
    }

    private Task SwitchBrandAsync(string brand)
    {
        _selectedBrand = brand;
        _clientId = brand;
        NerdBrandPackRegistry.Instance.Configure(brand, Options);
        TokenCss.Update(Options);
        HubOptions.ActiveTokenPackId = brand;
        ApplyBrandTypography(brand);
        RefreshParity();
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
}
