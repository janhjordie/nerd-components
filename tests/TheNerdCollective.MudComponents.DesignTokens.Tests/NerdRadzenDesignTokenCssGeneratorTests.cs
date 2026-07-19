using TheNerdCollective.Brand.Tnc;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdRadzenDesignTokenCssGeneratorTests
{
    [Fact]
    public void Generate_maps_radzen_tokens_from_nerd_intents()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".tnc-radzen-brand {", css);
        Assert.Contains("--rz-primary: var(--nerd-intent-primary-action-surface)", css);
        Assert.Contains("--rz-secondary: var(--nerd-intent-secondary-action-surface)", css);
        Assert.Contains("--rz-body-background-color: var(--nerd-intent-page-surface-surface)", css);
        Assert.Contains("--rz-text-secondary-color: var(--nerd-intent-muted-content-content)", css);
        Assert.Contains(".tnc-radzen-brand .rz-button.rz-primary", css);
    }

    [Fact]
    public void Radzen_palette_map_uses_convention_intents()
    {
        var map = NerdRadzenPaletteMap.CreateConventionBindings();

        Assert.Contains("primary-action", map.Primary);
        Assert.Contains("secondary-action", map.Secondary);
        Assert.Contains("page-surface", map.BodyBackground);
    }
}
