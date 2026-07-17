using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using MudBlazor;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public partial class NerdResponsiveTypographyCatalog
{
    [Inject]
    private NerdResponsiveTypographyOptions Options { get; set; } = default!;

    [Inject]
    private IWebHostEnvironment HostEnvironment { get; set; } = default!;

    private MudTheme _previewTheme = new();
    private IReadOnlyList<NerdTypographyRole> _roles = [];
    private IReadOnlyList<NerdTypographyAccessibilityResult> _accessibility = [];
    private IReadOnlyList<NerdTypographyAccessibilityWarning> _warnings = [];

    protected override void OnInitialized()
    {
        if (!IsAvailable)
        {
            return;
        }

        _previewTheme = Options.CreatePreviewTheme();
        _roles = NerdTypographyAccessibilityTools.GetConfiguredRoles(_previewTheme);
        _accessibility = NerdTypographyAccessibilityTools.CheckAccessibility(Options);
        _warnings = NerdTypographyAccessibilityTools.GetAccessibilityWarnings(Options);
    }

    private bool IsAvailable =>
        Options.EnableCatalogPage &&
        (!Options.RestrictCatalogToDevelopment || HostEnvironment.IsDevelopment());

    private NerdTypographyAccessibilityResult? GetAccessibility(string role) =>
        _accessibility.FirstOrDefault(result =>
            string.Equals(result.Role, role, StringComparison.OrdinalIgnoreCase));
}
