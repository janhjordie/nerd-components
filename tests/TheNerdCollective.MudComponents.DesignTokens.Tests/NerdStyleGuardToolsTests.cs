using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdStyleGuardToolsTests
{
    public NerdStyleGuardToolsTests() => NerdBrandPackTestBootstrap.EnsureRegistered();

    [Theory]
    [InlineData("tnc")]
    [InlineData("dnf")]
    [InlineData("acme")]
    [InlineData("demo")]
    public void Catalog_chrome_placement_passes_for_preset_brands(string brandId)
    {
        var options = NerdBrandPackTestBootstrap.CreateReferenceOptions(brandId);

        var violations = NerdStyleGuardTools.ValidateCatalogChromePlacement(options);

        Assert.Empty(violations);
    }

    [Fact]
    public void Generate_catalog_chrome_maps_labels_to_page_surface_content()
    {
        var options = NerdBrandPackTestBootstrap.CreateReferenceOptions("tnc");
        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".nerd-catalog-chrome [data-nerd-accent=\"tnc-primary-action\"] .mud-typography:not(.mud-tab)", css);
        Assert.Contains("var(--tnc-color-page-surface-content)", css);
    }

    [Fact]
    public void ValidateCatalogChromePlacement_fails_when_label_text_is_unreadable()
    {
        var options = new NerdDesignTokenOptions { Prefix = "test" };
        options.Add("snow", new NerdColorToken
        {
            Value = "#FFFFFF",
            Surface = "#FFFFFF",
            Content = "#FFFFFF",
            ContrastText = "#FFFFFF"
        });
        options.Add("ink", new NerdColorToken
        {
            Value = "#111827",
            Content = "#111827",
            ContrastText = "#FFFFFF"
        });
        options.Alias(NerdDesignSystemUi.PageSurface, "snow");
        options.Alias(NerdDesignSystemUi.PrimaryAction, "snow");

        var violations = NerdStyleGuardTools.ValidateCatalogChromePlacement(options);

        Assert.Contains(violations, violation => violation.Role == "label-text");
    }
}
