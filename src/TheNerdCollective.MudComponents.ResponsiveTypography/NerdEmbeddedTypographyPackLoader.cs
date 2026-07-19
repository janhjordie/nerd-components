using System.Reflection;
using System.Text.Json;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

/// <summary>Loads <see cref="NerdTypographyPack"/> JSON embedded in the ResponsiveTypography assembly.</summary>
public static class NerdEmbeddedTypographyPackLoader
{
    private static readonly Assembly Assembly = typeof(NerdEmbeddedTypographyPackLoader).Assembly;

    public static NerdTypographyPack LoadBrand(string brandId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(brandId);
        var resourceName =
            $"TheNerdCollective.MudComponents.ResponsiveTypography.reference.brands.{brandId}.typography-pack.json";
        return LoadResource(resourceName);
    }

    public static NerdTypographyPack LoadResource(string resourceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);
        using var stream = Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded typography pack '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);
        return NerdTypographyPack.FromJson(reader.ReadToEnd(), SerializerOptions);
    }

    public static string Serialize(NerdTypographyPack pack) =>
        pack.ToJson(SerializerOptions);

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}
