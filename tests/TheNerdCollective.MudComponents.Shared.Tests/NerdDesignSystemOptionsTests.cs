using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.Shared.Tests;

public sealed class NerdDesignSystemOptionsTests
{
    [Fact]
    public void FormatActiveTokenPackLabel_includes_identity_version_when_present()
    {
        var options = new NerdDesignSystemOptions
        {
            ActiveTokenPackId = "dnf",
            ActiveBrandIdentityVersion = "2025.1"
        };

        Assert.Equal("dnf (2025.1)", options.FormatActiveTokenPackLabel());
    }

    [Fact]
    public void FormatActiveTokenPackLabel_returns_pack_id_when_identity_version_missing()
    {
        var options = new NerdDesignSystemOptions
        {
            ActiveTokenPackId = "tnc"
        };

        Assert.Equal("tnc", options.FormatActiveTokenPackLabel());
    }
}
