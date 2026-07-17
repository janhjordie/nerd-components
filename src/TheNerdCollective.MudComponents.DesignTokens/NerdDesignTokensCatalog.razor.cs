using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

public partial class NerdDesignTokensCatalog
{
    [Inject]
    private NerdDesignTokenOptions Options { get; set; } = default!;

    [Inject]
    private NerdDesignSystemOptions HubOptions { get; set; } = default!;

    [Inject]
    private IWebHostEnvironment HostEnvironment { get; set; } = default!;

    [Inject]
    private NerdDownloadService DownloadService { get; set; } = default!;

    private bool _previewDark;
    private IReadOnlyList<NerdAccessibilityResult> _accessibility = [];
    private IReadOnlyList<NerdAccessibilityWarning> _warnings = [];

    protected override void OnInitialized()
    {
        if (!Options.EnableCatalogPage)
        {
            return;
        }

        if (Options.RestrictCatalogToDevelopment && !HostEnvironment.IsDevelopment())
        {
            return;
        }

        HubOptions.DesignTokensRoute = Options.CatalogRoute;
        _accessibility = NerdDesignTokenTools.CheckAccessibility(Options);
        _warnings = NerdDesignTokenTools.GetAccessibilityWarnings(Options);
    }

    private bool IsAvailable =>
        Options.EnableCatalogPage &&
        (!Options.RestrictCatalogToDevelopment || HostEnvironment.IsDevelopment());

    private string GetClassName(string tokenName) => $"{Options.Prefix}-{tokenName}";

    private NerdAccessibilityResult? GetAccessibility(string tokenName) =>
        _accessibility.FirstOrDefault(result =>
            string.Equals(result.Name, tokenName, StringComparison.OrdinalIgnoreCase));

    private static string ResolveColor(NerdColorToken token, bool dark) =>
        dark ? token.Dark ?? token.Light ?? token.Value : token.Light ?? token.Value;

    private static string FormatRatio(double ratio) => $"{ratio:0.0}:1";

    private Task DownloadCssAsync() =>
        DownloadService.DownloadTextAsync($"{Options.Prefix}-tokens.css", MudBlazorDesignTokenCssGenerator.Generate(Options)).AsTask();

    private Task DownloadJsonAsync() =>
        DownloadService.DownloadTextAsync($"{Options.Prefix}-tokens.json", NerdDesignTokenTools.ExportJson(Options)).AsTask();

    private Task DownloadStitchAsync() =>
        DownloadService.DownloadTextAsync("DESIGN.md", NerdDesignTokenTools.ExportStitchDesignMd(Options)).AsTask();
}
