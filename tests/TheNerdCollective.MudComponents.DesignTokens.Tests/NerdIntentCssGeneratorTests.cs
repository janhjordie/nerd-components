using TheNerdCollective.Brand.Tnc;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdIntentCssGeneratorTests
{
    [Fact]
    public void Generate_emits_nerd_intent_variables_on_brand_root()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".tnc-nerd-brand {", css);
        Assert.Contains(
            $"{NerdIntentCssManifest.IntentVariable(NerdDesignSystemUi.PrimaryAction, "surface")}: var(--tnc-color-primary-action-surface)",
            css);
        Assert.Contains(
            $"{NerdIntentCssManifest.IntentVariable(NerdDesignSystemUi.PrimaryAction, "content")}: var(--tnc-color-primary-action-content)",
            css);
    }

    [Fact]
    public void Generate_skips_nerd_intent_bridge_when_disabled()
    {
        var options = new NerdDesignTokenOptions
        {
            Prefix = "tnc",
            UseImportantOverrides = false,
            EmitFrameworkNeutralIntents = false
        };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.DoesNotContain(".tnc-nerd-brand {", css);
        Assert.DoesNotContain("--nerd-intent-", css);
    }
}
