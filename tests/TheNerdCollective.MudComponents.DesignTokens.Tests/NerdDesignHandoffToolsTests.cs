using System.IO.Compression;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdDesignHandoffToolsTests
{
    [Fact]
    public void CreateZip_includes_expected_handoff_files()
    {
        var options = NerdEmbeddedBrandPack.FromBrandJson("tnc").TokenPack.ToOptions();
        var bytes = NerdDesignHandoffTools.CreateZip(new NerdDesignHandoffRequest
        {
            Options = options,
            ClientId = "tnc",
            DesignGuideUrl = "http://localhost/nerd-design-guide"
        });

        using var memory = new MemoryStream(bytes);
        using var archive = new ZipArchive(memory, ZipArchiveMode.Read);
        var names = archive.Entries.Select(entry => entry.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("token-pack.json", names);
        Assert.Contains("tokens.css", names);
        Assert.Contains("tokens.json", names);
        Assert.Contains("DESIGN.md", names);
        Assert.Contains("WCAG-REPORT.md", names);
        Assert.Contains("README-HANDOFF.md", names);
    }
}
