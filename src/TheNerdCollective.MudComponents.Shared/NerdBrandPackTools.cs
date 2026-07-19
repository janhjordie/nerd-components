using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace TheNerdCollective.MudComponents.Shared;

public static class NerdBrandPackTools
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static NerdBrandPack Create(string clientId, string designTokensJson, string typographyJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(designTokensJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(typographyJson);

        using var tokensDocument = JsonDocument.Parse(designTokensJson);
        using var typographyDocument = JsonDocument.Parse(typographyJson);

        return new NerdBrandPack
        {
            ClientId = clientId,
            DesignTokens = tokensDocument.RootElement.Clone(),
            Typography = typographyDocument.RootElement.Clone()
        };
    }

    public static string ToJson(NerdBrandPack pack) =>
        JsonSerializer.Serialize(pack, SerializerOptions);

    public static NerdBrandPack FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var pack = JsonSerializer.Deserialize<NerdBrandPack>(json, SerializerOptions)
                   ?? throw new ArgumentException("Brand pack JSON was empty.", nameof(json));
        ArgumentException.ThrowIfNullOrWhiteSpace(pack.ClientId);
        return pack;
    }

    public static (string DesignTokensJson, string TypographyJson) Unbundle(NerdBrandPack pack)
    {
        ArgumentNullException.ThrowIfNull(pack);
        if (pack.DesignTokens is null || pack.Typography is null)
        {
            throw new ArgumentException("Brand pack must include designTokens and typography sections.", nameof(pack));
        }

        return (
            pack.DesignTokens.Value.GetRawText(),
            pack.Typography.Value.GetRawText());
    }

    public static byte[] ToZip(NerdBrandPack pack)
    {
        ArgumentNullException.ThrowIfNull(pack);
        var (tokensJson, typographyJson) = Unbundle(pack);

        using var memory = new MemoryStream();
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteZipEntry(archive, "brand-pack.json", ToJson(pack));
            WriteZipEntry(archive, "design-tokens.json", tokensJson);
            WriteZipEntry(archive, "typography.json", typographyJson);
        }

        return memory.ToArray();
    }

    public static NerdBrandPack FromZip(Stream zipStream)
    {
        ArgumentNullException.ThrowIfNull(zipStream);
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);

        var bundleEntry = archive.GetEntry("brand-pack.json")
                          ?? throw new ArgumentException("ZIP archive is missing brand-pack.json.");
        using var bundleStream = bundleEntry.Open();
        using var reader = new StreamReader(bundleStream, Encoding.UTF8);
        return FromJson(reader.ReadToEnd());
    }

    private static void WriteZipEntry(ZipArchive archive, string name, string content)
    {
        var entry = archive.CreateEntry(name, CompressionLevel.Optimal);
        using var entryStream = entry.Open();
        using var writer = new StreamWriter(entryStream, Encoding.UTF8);
        writer.Write(content);
    }
}
