using TheNerdCollective.Brand.Tnc;
using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdMudPaletteParityToolsTests
{
    [Fact]
    public void Evaluate_tnc_scores_high_for_palette_first_pack()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var result = NerdMudPaletteParityTools.Evaluate(options);

        Assert.True(result.Score >= 80, $"Expected score >= 80, got {result.Score}");
        Assert.Contains(result.Checks, check => check.Name == "Brand root palette" && check.Score >= 80);
        Assert.Contains(result.Checks, check => check.Name == "Primary intent" && check.Score == 100);
    }

    [Fact]
    public void Evaluate_detects_bulk_button_patterns_as_low_primary_intent_score()
    {
        var options = new NerdDesignTokenOptions
        {
            Prefix = "tnc",
            UsePaletteFirstAdapter = false
        };
        NerdTncDesignTokenPresets.Apply(options);

        var result = NerdMudPaletteParityTools.Evaluate(options);
        var primary = result.Checks.Single(check => check.Name == "Primary intent");

        Assert.True(primary.Score < 100);
    }

    [Fact]
    public void Evaluate_muted_content_uses_palette_override_not_bulk_buttons()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains("--mud-palette-text-secondary: var(--tnc-color-muted-content-content)", css);
        Assert.DoesNotContain(".tnc-muted-content[class*=\"mud-button-filled\"]", css);
    }
}
