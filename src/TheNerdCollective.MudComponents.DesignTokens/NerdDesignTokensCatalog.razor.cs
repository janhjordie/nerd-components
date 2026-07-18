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
    private string? _studioToken;
    private string _studioColor = "#365C3A";
    private int _activeTabIndex;
    private string _previewRadioValue = "radio";
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

        _accessibility = NerdDesignTokenTools.CheckAccessibility(Options);
        _warnings = NerdDesignTokenTools.GetAccessibilityWarnings(Options);
        var firstToken = Options.Colors.Keys.OrderBy(name => name, StringComparer.Ordinal).FirstOrDefault();
        if (firstToken is not null)
        {
            SelectStudioToken(firstToken);
        }
    }

    private bool IsAvailable =>
        Options.EnableCatalogPage &&
        (!Options.RestrictCatalogToDevelopment || HostEnvironment.IsDevelopment());

    private string GetClassName(string tokenName) => $"{Options.Prefix}-{tokenName}";

    private void SelectStudioToken(string tokenName)
    {
        _studioToken = tokenName;
        _studioColor = Options.Colors[tokenName].Value;
    }

    private void OpenTokenTab(string tokenName)
    {
        var tokenIndex = Options.Colors
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select((pair, index) => new { pair.Key, index })
            .FirstOrDefault(item => string.Equals(item.Key, tokenName, StringComparison.OrdinalIgnoreCase));

        if (tokenIndex is not null)
        {
            // The first tab is the swatch overview; token tabs follow it.
            _activeTabIndex = tokenIndex.index + 1;
        }
    }

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

    private static string FormatRatio(double ratio) => $"{ratio:0.0}:1";

    private static RenderFragment ColorValue(string? value) => builder =>
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            builder.AddContent(0, "–");
            return;
        }

        builder.OpenElement(1, "span");
        builder.AddAttribute(
            2,
            "style",
            $"display:inline-flex;align-items:center;gap:.5rem;background:transparent;");
        builder.OpenElement(3, "span");
        builder.AddAttribute(4, "class", "nerd-token-color-tile");
        builder.AddAttribute(
            5,
            "style",
            $"display:inline-block;width:1.1rem;height:1.1rem;border-radius:3px;border:1px solid rgba(0,0,0,.2);background-color:{value};");
        builder.AddAttribute(6, "title", value);
        builder.CloseElement();
        builder.OpenElement(7, "code");
        builder.AddContent(8, value);
        builder.CloseElement();
        builder.CloseElement();
    };

    private Task DownloadCssAsync() =>
        DownloadService.DownloadTextAsync($"{Options.Prefix}-tokens.css", MudBlazorDesignTokenCssGenerator.Generate(Options)).AsTask();

    private Task DownloadJsonAsync() =>
        DownloadService.DownloadTextAsync($"{Options.Prefix}-tokens.json", NerdDesignTokenTools.ExportJson(Options)).AsTask();

    private Task DownloadStitchAsync() =>
        DownloadService.DownloadTextAsync("DESIGN.md", NerdDesignTokenTools.ExportStitchDesignMd(Options)).AsTask();
}
