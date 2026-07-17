using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;

namespace TheNerdCollective.MudComponents.DesignTokens;

public partial class NerdDesignTokensCatalog
{
    [Inject]
    private NerdDesignTokenOptions Options { get; set; } = default!;

    [Inject]
    private IWebHostEnvironment HostEnvironment { get; set; } = default!;

    private bool _previewDark;
    private IReadOnlyList<NerdAccessibilityResult> _accessibility = [];

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
}
