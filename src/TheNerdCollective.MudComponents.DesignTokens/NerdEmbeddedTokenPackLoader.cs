using System.Reflection;
using System.Text.Json;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Loads <see cref="NerdTokenPack"/> JSON embedded in the DesignTokens assembly.</summary>
public static class NerdEmbeddedTokenPackLoader
{
    private static readonly Assembly Assembly = typeof(NerdEmbeddedTokenPackLoader).Assembly;

    public static NerdTokenPack LoadBrand(string brandId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(brandId);
        var resourceName = $"TheNerdCollective.MudComponents.DesignTokens.reference.brands.{brandId}.token-pack.json";
        return LoadResource(resourceName);
    }

    public static NerdTokenPack LoadResource(string resourceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);
        using var stream = Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded token pack '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);
        return NerdTokenPack.FromJson(reader.ReadToEnd(), SerializerOptions);
    }

    public static string Serialize(NerdTokenPack pack) =>
        pack.ToJson(SerializerOptions);

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}
