using System.IO.Compression;
using System.Text;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdTokenPackExportToolsTests
{
    [Fact]
    public void BuildZip_includes_requested_formats()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc" };
        options.Add("navy", new NerdColorToken { Value = "#001B3A" });
        options.AddSpacing("4", "4px");

        var request = new NerdTokenPackExportRequest
        {
            IncludeCss = true,
            IncludePackJson = true,
            IncludeDtcg = true,
            IncludeTokensStudio = true
        };

        var zipBytes = NerdTokenPackExportTools.BuildZip(options, request, clientId: "acme");
        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);

        Assert.NotNull(archive.GetEntry("tnc-design-tokens.css"));
        Assert.NotNull(archive.GetEntry("tnc-token-pack.json"));
        Assert.NotNull(archive.GetEntry("tnc-design-tokens.dtcg.json"));
        Assert.NotNull(archive.GetEntry("tnc-tokens-studio.json"));
    }

    [Fact]
    public void BuildZip_respects_group_filters_for_dtcg()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc" };
        options.Add("navy", new NerdColorToken { Value = "#001B3A" });
        options.AddSpacing("4", "4px");

        var request = new NerdTokenPackExportRequest
        {
            IncludeCss = false,
            IncludePackJson = false,
            IncludeDtcg = true,
            IncludeSpacing = false
        };

        var zipBytes = NerdTokenPackExportTools.BuildZip(options, request);
        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        using var stream = archive.GetEntry("tnc-design-tokens.dtcg.json")!.Open();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var json = reader.ReadToEnd();

        Assert.Contains("\"color\"", json);
        Assert.DoesNotContain("\"spacing\"", json);
    }
}
