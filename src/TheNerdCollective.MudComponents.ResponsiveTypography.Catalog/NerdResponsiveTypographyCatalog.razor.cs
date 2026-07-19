using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MudBlazor;
using TheNerdCollective.MudComponents.DesignTokens;
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
    private NerdDesignTokenOptions? TokenOptions { get; set; }

    [Inject]
    private IWebHostEnvironment HostEnvironment { get; set; } = default!;

    [Inject]
    private INerdTypographyPackStore TypographyPackStore { get; set; } = default!;

    [Inject]
    private NerdDownloadService DownloadService { get; set; } = default!;

    [Inject]
    private NerdResponsiveTypographyCss TypographyCss { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private INerdCatalogEntitlements Entitlements { get; set; } = default!;

    private int _activeTabIndex;
    private MudTheme _previewTheme = new();
    private IReadOnlyList<NerdTypographyRole> _roles = [];
    private IReadOnlyList<NerdTypographyAccessibilityResult> _accessibility = [];
    private IReadOnlyList<NerdTypographyAccessibilityWarning> _warnings = [];
    private bool _showAllRoles;
    private double _viewportWidth = 1280;
    private string _deviceSampleRole = "H1";
    private string _deviceSampleText = "The quick brown fox";
    private double _editorMinimum = 16;
    private double _editorPreferred = 2;
    private double _editorMaximum = 48;
    private string _editorRole = "H1";
    private double _scaleBaseRem = 1;
    private double _scaleRatio = 1.25;
    private string _clientId = "client";
    private string? _selectedPackId;
    private string? _saveStatus;
    private IReadOnlyList<string> _packIds = [];
    private IReadOnlyDictionary<string, string> _scalePreview =
        NerdModularScaleGenerator.Generate();

    private string EditorClamp =>
        ResponsiveFontSize.Clamp(
            FormatCssLength(_editorMinimum, "px"),
            FormatCssLength(_editorPreferred, "vw"),
            FormatCssLength(_editorMaximum, "px"));

    protected override async Task OnInitializedAsync()
    {
        if (!IsAvailable)
        {
            return;
        }

        RefreshPreviewTheme();
        _packIds = await TypographyPackStore.ListAsync();
        if (_packIds.Count > 0)
        {
            _selectedPackId ??= _packIds[0];
        }

        SyncEditorFromRole();
        Navigation.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e) => RefreshPreviewTheme();

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
        pixels is null ? "-" : FormatCssLength(pixels.Value, "px");

    private static string FormatCssLength(double value, string unit) =>
        $"{value.ToString("0.#", CultureInfo.InvariantCulture)}{unit}";

    private static string FormatCssPercent(double value) =>
        $"{value.ToString("0.#", CultureInfo.InvariantCulture)}%";

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

    private static string BuildOptionsSnippet(NerdTypographyRole role) =>
        "options.Typography." + role.Role + " = ResponsiveFontSize.Clamp(\"2rem\", \"4vw\", \"4rem\");";

    private double GetCurveWidth(double pixels)
    {
        var maximum = VisibleRoles
            .Select(role => NerdClampEvaluator.EvaluateAtViewport(role.FontSize, _viewportWidth) ?? 0)
            .DefaultIfEmpty(1)
            .Max();

        return Math.Clamp(pixels / maximum * 100d, 4d, 100d);
    }

    private static string ToTypoLiteral(Typo typo) => typo.ToString();

    private void RefreshPreviewTheme()
    {
        _previewTheme = Options.CreatePreviewTheme();
        TypographyCss.Update(Options.Typography);
        _roles = FilterRoles(NerdTypographyAccessibilityTools.GetConfiguredRoles(_previewTheme)).ToArray();
        _accessibility = NerdTypographyAccessibilityTools.CheckAccessibility(Options);
        _warnings = NerdTypographyAccessibilityTools.GetAccessibilityWarnings(Options);
        HubOptions.TypographyRoleCount = Options.Typography.ConfiguredRoles.Count;

        var preferred = _roles.FirstOrDefault(role =>
            string.Equals(role.Role, "H1", StringComparison.OrdinalIgnoreCase));
        if (preferred is not null && string.Equals(_deviceSampleRole, "H1", StringComparison.OrdinalIgnoreCase))
        {
            _deviceSampleRole = preferred.Role;
        }
        else if (_roles.Count > 0 && !_roles.Any(role => role.Role == _deviceSampleRole))
        {
            _deviceSampleRole = _roles[0].Role;
        }
    }

    private void SyncEditorFromRole()
    {
        var role = GetRole(_editorRole);
        if (role is null ||
            !NerdClampEvaluator.TryParseForEditor(role.FontSize, out var minimum, out var preferred, out var maximum))
        {
            return;
        }

        _editorMinimum = minimum;
        _editorPreferred = preferred;
        _editorMaximum = maximum;
    }

    private Task OnEditorRoleChanged(string role)
    {
        _editorRole = role;
        SyncEditorFromRole();
        return Task.CompletedTask;
    }

    private void ApplyEditorToRole()
    {
        var property = typeof(ResponsiveTypographyOptions).GetProperty(_editorRole);
        if (property is null)
        {
            return;
        }

        property.SetValue(Options.Typography, EditorClamp);
        RefreshPreviewTheme();
        _saveStatus = $"Applied clamp to {_editorRole}.";
    }

    private void RefreshModularScalePreview() =>
        _scalePreview = NerdModularScaleGenerator.Generate(_scaleBaseRem, _scaleRatio);

    private void ApplyModularScaleToPreview()
    {
        foreach (var pair in _scalePreview)
        {
            typeof(ResponsiveTypographyOptions).GetProperty(pair.Key)?.SetValue(Options.Typography, pair.Value);
        }

        RefreshPreviewTheme();
        SyncEditorFromRole();
        _saveStatus = "Applied modular scale to preview.";
    }

    private async Task SaveClientPackAsync()
    {
        if (!Entitlements.CanSaveAnotherPack(_packIds.Count))
        {
            _saveStatus = "Saving client packs is not enabled for the current licence.";
            return;
        }

        await TypographyPackStore.SaveAsync(
            NerdTypographyPack.FromOptions(Options, _clientId));
        _saveStatus = $"Saved {_clientId}.";
        _packIds = await TypographyPackStore.ListAsync();
        _selectedPackId = _clientId;
        HubOptions.ActiveTypographyPackId = _clientId;
    }

    private async Task LoadClientPackAsync()
    {
        if (string.IsNullOrWhiteSpace(_selectedPackId))
        {
            return;
        }

        var pack = await TypographyPackStore.LoadAsync(_selectedPackId);
        if (pack is null)
        {
            _saveStatus = $"Pack '{_selectedPackId}' not found.";
            return;
        }

        pack.ApplyTo(Options);
        HubOptions.ActiveTypographyPackId = pack.ClientId;
        RefreshPreviewTheme();
        SyncEditorFromRole();
        _saveStatus = $"Loaded {pack.ClientId}.";
    }

    private string Ui(string semanticAlias) =>
        NerdDesignSystemUi.TokenClass(TokenOptions?.Prefix ?? HubOptions.TokenPrefix, semanticAlias);

    private Task DownloadTokensStudioAsync() =>
        DownloadService.DownloadTextAsync(
            "typography-tokens-studio.json",
            NerdTypographyTools.ExportTokensStudioJson(Options)).AsTask();

    private async Task ImportTokensStudioAsync(InputFileChangeEventArgs args)
    {
        var file = args.File;
        if (file is null)
        {
            return;
        }

        await using var stream = file.OpenReadStream(maxAllowedSize: 2_000_000);
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        NerdTypographyTools.ImportTokensStudioJson(Options, json);
        RefreshPreviewTheme();
        SyncEditorFromRole();
        _saveStatus = "Imported Tokens Studio typography.";
    }
}
