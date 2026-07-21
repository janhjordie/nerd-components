using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MudBlazor;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

public partial class NerdDesignGuide : IDisposable
{
    [Inject]
    private NerdDesignTokenOptions Options { get; set; } = default!;

    [Inject]
    private NerdDesignSystemOptions HubOptions { get; set; } = default!;

    [Inject]
    private IWebHostEnvironment HostEnvironment { get; set; } = default!;

    [Inject]
    private INerdBrandSwitcher BrandSwitcher { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private string _clientId = "client";
    private NerdDesignParityResult _parity = new(0, []);
    private NerdMudStateParityResult _mudStateParity = new(0, []);
    private NerdMudPaletteFidelityResult _mudPaletteFidelity = new(0, []);
    private IReadOnlyList<NerdComponentFamilyBinding> _familyBindings = [];

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

        _clientId = Options.ActiveBrandPackId ?? Options.Prefix;
        BrandSwitcher.BrandChanged += OnGlobalBrandChanged;
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

    private void OnGlobalBrandChanged(string brand)
    {
        _clientId = brand;
        RefreshParity();
        InvokeAsync(StateHasChanged);
    }

    public void Dispose() => BrandSwitcher.BrandChanged -= OnGlobalBrandChanged;
}
