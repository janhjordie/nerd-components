using TheNerdCollective.Brand.Tnc;
using TheNerdCollective.MudComponents.DesignTokens;
using Xunit;

namespace TheNerdCollective.MudComponents.PlayBook.Tests;

public sealed class NerdPlayBookTokenFilterTests
{
    [Fact]
    public void ResolveVisibleClasses_defaults_to_configured_intents()
    {
        var options = new NerdDesignTokenOptions();
        NerdTncDesignTokenPresets.Apply(options);

        var classes = NerdPlayBookTokenFilter.ResolveVisibleClasses(
            options,
            NerdPlayBookTokenFilter.AllIntents,
            ["navy", "coral"]);

        Assert.Contains("tnc-primary-action", classes);
        Assert.Contains("tnc-page-surface", classes);
        Assert.DoesNotContain("tnc-coral", classes);
    }

    [Fact]
    public void ResolveVisibleClasses_all_palette_returns_color_classes()
    {
        var options = new NerdDesignTokenOptions();
        NerdTncDesignTokenPresets.Apply(options);

        var classes = NerdPlayBookTokenFilter.ResolveVisibleClasses(
            options,
            NerdPlayBookTokenFilter.AllPalette,
            ["navy", "coral", "chalk"]);

        Assert.Equal(
            ["tnc-chalk", "tnc-coral", "tnc-navy"],
            classes.OrderBy(name => name, StringComparer.Ordinal).ToArray());
    }
}
