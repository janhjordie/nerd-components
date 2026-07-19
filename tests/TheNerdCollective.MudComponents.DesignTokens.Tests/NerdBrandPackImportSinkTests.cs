using TheNerdCollective.MudComponents.DesignTokens;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdBrandPackImportSinkTests
{
    public NerdBrandPackImportSinkTests() => NerdBrandPackTestBootstrap.EnsureRegistered();

    [Fact]
    public async Task ImportAsync_applies_valid_pack()
    {
        var options = new NerdDesignTokenOptions();
        NerdBrandPackRegistry.Instance.Configure("tnc", options);
        var hub = new NerdDesignSystemOptions { TokenPrefix = options.Prefix };
        var css = new NerdDesignTokenCss(MudBlazorDesignTokenCssGenerator.Generate(options));
        var sink = new NerdBrandPackImportSink(options, css, hub);

        var pack = NerdTokenPack.FromOptions(options, "tnc");
        pack = NerdTokenPackEnricher.WithPairingPolicy(pack, options.PairingPolicy);
        var result = await sink.ImportAsync(pack.ToJson());

        Assert.True(result.Success);
        Assert.Equal("tnc", hub.ActiveTokenPackId);
        Assert.NotEmpty(options.Colors);
    }

    [Fact]
    public async Task ImportAsync_returns_schema_errors()
    {
        var options = new NerdDesignTokenOptions { Prefix = "test" };
        var hub = new NerdDesignSystemOptions();
        var css = new NerdDesignTokenCss(string.Empty);
        var sink = new NerdBrandPackImportSink(options, css, hub);

        var result = await sink.ImportAsync("""{"clientId":"x"}""");

        Assert.False(result.Success);
        Assert.Contains("prefix", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}
