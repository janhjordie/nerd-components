using TheNerdCollective.Brand.Dnf;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

/// <summary>
/// CI gate: DNF brand pack must pass WCAG 2.1 AA token contrast and catalog chrome placement.
/// </summary>
public sealed class NerdDnfWcagComplianceTests
{
    public NerdDnfWcagComplianceTests() => NerdBrandPackTestBootstrap.EnsureRegistered();

    [Fact]
    public void Dnf_preset_tokens_meet_wcag_aa()
    {
        var options = CreateDnfOptions();

        NerdDesignTokenTools.AssertAccessibilityCompliance(options);
    }

    [Fact]
    public void Dnf_preset_catalog_chrome_placement_meets_ui_contrast()
    {
        var options = CreateDnfOptions();

        NerdStyleGuardTools.AssertPlacementCompliance(options);
    }

    [Fact]
    public void Dnf_preset_reports_no_accessibility_warnings()
    {
        var options = CreateDnfOptions();
        var warnings = NerdDesignTokenTools.GetAccessibilityWarnings(options);

        Assert.Empty(warnings);
    }

    private static NerdDesignTokenOptions CreateDnfOptions()
    {
        var options = new NerdDesignTokenOptions { WcagVersion = "2.1" };
        NerdDnfDesignTokenPresets.Apply(options);
        return options;
    }
}
