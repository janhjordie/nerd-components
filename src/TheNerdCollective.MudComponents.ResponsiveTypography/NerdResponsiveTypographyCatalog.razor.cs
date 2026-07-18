using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MudBlazor;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public partial class NerdResponsiveTypographyCatalog
{
    private static readonly (string Label, double Width)[] DeviceFrames =
    [
        ("Phone", 375),
        ("Tablet", 768),
        ("Desktop", 1440),
    ];

    private static readonly double[] BreakpointColumns = [320, 375, 768, 1024, 1280, 1440, 1920];

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
    private string _deviceSampleRole = "H1";
    private double _editorMinimum = 16;
    private double _editorPreferred = 2;
    private double _editorMaximum = 48;

    private string EditorClamp =>
        ResponsiveFontSize.Clamp($"{_editorMinimum:0.#}px", $"{_editorPreferred:0.#}vw", $"{_editorMaximum:0.#}px");

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

        var preferred = _roles.FirstOrDefault(role =>
            string.Equals(role.Role, "H1", StringComparison.OrdinalIgnoreCase));
        if (preferred is not null)
        {
            _deviceSampleRole = preferred.Role;
        }
        else if (_roles.Count > 0)
        {
            _deviceSampleRole = _roles[0].Role;
        }
    }

    private bool IsAvailable =>
        Options.EnableCatalogPage &&
        (!Options.RestrictCatalogToDevelopment || HostEnvironment.IsDevelopment());

    private IEnumerable<NerdTypographyRole> VisibleRoles =>
        FilterRoles(NerdTypographyAccessibilityTools.GetConfiguredRoles(_previewTheme));

    private IEnumerable<NerdTypographyRole> FilterRoles(IReadOnlyList<NerdTypographyRole> roles) =>
        _showAllRoles
            ? roles
            : roles.Where(role => Options.Typography.ConfiguredRoles.Contains(role.Role));

    private NerdTypographyRole? GetRole(string roleName) =>
        NerdTypographyAccessibilityTools.GetConfiguredRoles(_previewTheme)
            .FirstOrDefault(role => string.Equals(role.Role, roleName, StringComparison.OrdinalIgnoreCase));

    private NerdTypographyAccessibilityResult? GetAccessibility(string role) =>
        _accessibility.FirstOrDefault(result =>
            string.Equals(result.Role, role, StringComparison.OrdinalIgnoreCase));

    private string GetComputedSize(NerdTypographyRole role, double? viewportWidth = null) =>
        FormatPixels(NerdClampEvaluator.EvaluateAtViewport(role.FontSize, viewportWidth ?? _viewportWidth));

    private string GetComputedSize(string roleName, double viewportWidth)
    {
        var role = GetRole(roleName);
        return role is null ? "-" : GetComputedSize(role, viewportWidth);
    }

    private static string FormatPixels(double? pixels) =>
        pixels is null ? "-" : $"{pixels.Value.ToString("0.#")}px";

    private string DeviceSampleSnippet
    {
        get
        {
            var role = GetRole(_deviceSampleRole);
            if (role is null)
            {
                return string.Empty;
            }

            return "<MudText Typo=\"Typo." + ToTypoLiteral(role.Typo) + "\">Sample</MudText>";
        }
    }

    private static string BuildRazorSnippet(NerdTypographyRole role) =>
        "<MudText Typo=\"Typo." + ToTypoLiteral(role.Typo) + "\">...</MudText>";

    private static string ToTypoLiteral(Typo typo) => typo.ToString();
}
