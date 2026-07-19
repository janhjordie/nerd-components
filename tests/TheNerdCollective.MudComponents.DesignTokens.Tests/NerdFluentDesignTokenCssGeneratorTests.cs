using TheNerdCollective.Brand.Tnc;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdFluentDesignTokenCssGeneratorTests
{
    [Fact]
    public void Generate_maps_fluent_tokens_from_nerd_intents()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".tnc-fluent-brand {", css);
        Assert.Contains("--colorBrandBackground: var(--nerd-intent-primary-action-surface)", css);
        Assert.Contains("--colorBrandForeground1: var(--nerd-intent-on-primary-action-content)", css);
        Assert.Contains("--colorNeutralBackground1: var(--nerd-intent-page-surface-surface)", css);
        Assert.Contains("--colorStrokeFocus1: var(--nerd-intent-focus-ring-interactive)", css);
    }

    [Fact]
    public void Fluent_palette_map_uses_convention_intents()
    {
        var map = NerdFluentBlazorPaletteMap.CreateConventionBindings();

        Assert.Contains("primary-action", map.BrandBackground);
        Assert.Contains("page-surface", map.NeutralBackground);
        Assert.Contains("focus-ring", map.FocusStroke);
    }
}
