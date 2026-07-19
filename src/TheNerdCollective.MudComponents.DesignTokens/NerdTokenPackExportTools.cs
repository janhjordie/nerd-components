using System.IO.Compression;
using System.Text;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Multi-format token pack export (HR-110).</summary>
public static class NerdTokenPackExportTools
{
    public static byte[] BuildZip(
        NerdDesignTokenOptions options,
        NerdTokenPackExportRequest request,
        IReadOnlyDictionary<string, string>? typographyRoles = null,
        string clientId = "export")
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(request);

        using var memory = new MemoryStream();
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
        {
            if (request.IncludeCss)
            {
                WriteEntry(archive, $"{options.Prefix}-design-tokens.css", MudBlazorDesignTokenCssGenerator.Generate(options));
            }

            if (request.IncludePackJson)
            {
                WriteEntry(archive, $"{options.Prefix}-token-pack.json", NerdTokenPack.FromOptions(options, clientId).ToJson());
            }

            if (request.IncludeDtcg)
            {
                WriteEntry(archive, $"{options.Prefix}-design-tokens.dtcg.json", NerdDtcgTokenTools.Export(options, request));
            }

            if (request.IncludeTokensStudio)
            {
                WriteEntry(archive, $"{options.Prefix}-tokens-studio.json", NerdDesignTokenTools.ExportTokensStudioJson(options));
            }

            if (request.IncludeStitch)
            {
                WriteEntry(
                    archive,
                    "DESIGN.md",
                    NerdDesignTokenTools.ExportStitchDesignMd(options, typographyRoles));
            }
        }

        return memory.ToArray();
    }

    private static void WriteEntry(ZipArchive archive, string name, string content)
    {
        var entry = archive.CreateEntry(name, CompressionLevel.Optimal);
        using var stream = entry.Open();
        var bytes = Encoding.UTF8.GetBytes(content);
        stream.Write(bytes, 0, bytes.Length);
    }
}

public sealed class NerdTokenPackExportRequest
{
    public bool IncludeCss { get; set; } = true;
    public bool IncludePackJson { get; set; } = true;
    public bool IncludeDtcg { get; set; } = true;
    public bool IncludeTokensStudio { get; set; }
    public bool IncludeStitch { get; set; }

    public bool IncludeColors { get; set; } = true;
    public bool IncludeSpacing { get; set; } = true;
    public bool IncludeRadii { get; set; } = true;
    public bool IncludeShadows { get; set; } = true;
    public bool IncludeMotion { get; set; } = true;
    public bool IncludeBreakpoints { get; set; } = true;
    public bool IncludeZIndex { get; set; } = true;
}
