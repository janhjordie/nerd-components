using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MudBlazor;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public partial class NerdResponsiveTypographyCatalog
{
    [Inject]
    private NerdResponsiveTypographyOptions Options { get; set; } = default!;

    [Inject]
    private NerdDesignSystemOptions HubOptions { get; set; } = default!;

    [Inject]
    private IWebHostEnvironment HostEnvironment { get; set; } = default!;

    private MudTheme _previewTheme = new();
    private IReadOnlyList<NerdTypographyRole> _roles = [];
    private IReadOnlyList<NerdTypographyAccessibilityResult> _accessibility = [];
    private IReadOnlyList<NerdTypographyAccessibilityWarning> _warnings = [];
    private bool _showAllRoles;
    private double _viewportWidth = 1280;

    protected override void OnInitialized()
    {
        if (!IsAvailable)
        {
            return;
        }

        _previewTheme = Options.CreatePreviewTheme();
        _roles = FilterRoles(NerdTypographyAccessibilityTools.GetConfiguredRoles(_previewTheme)).ToArray();
        _accessibility = NerdTypographyAccessibilityTools.CheckAccessibility(Options);
        _warnings = NerdTypographyAccessibilityTools.GetAccessibilityWarnings(Options);
    }

    private bool IsAvailable =>
        Options.EnableCatalogPage &&
        (!Options.RestrictCatalogToDevelopment || HostEnvironment.IsDevelopment());

    private IEnumerable<NerdTypographyRole> FilterRoles(IReadOnlyList<NerdTypographyRole> roles) =>
        _showAllRoles
            ? roles
            : roles.Where(role => Options.Typography.ConfiguredRoles.Contains(role.Role));

    private NerdTypographyAccessibilityResult? GetAccessibility(string role) =>
        _accessibility.FirstOrDefault(result =>
            string.Equals(result.Role, role, StringComparison.OrdinalIgnoreCase));

    private string GetComputedSize(NerdTypographyRole role) =>
        NerdClampEvaluator.EvaluateAtViewport(role.FontSize, _viewportWidth)?.ToString("0.#") + "px"
        ?? role.FontSize;
}
