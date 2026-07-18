using System.Text.Json;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public sealed class NerdTypographyPack
{
    public string ClientId { get; init; } = "default";
    public Dictionary<string, string> Roles { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public string? LineHeight { get; init; }
    public string? LetterSpacing { get; init; }
    public string? FontWeight { get; init; }

    public static NerdTypographyPack FromOptions(
        NerdResponsiveTypographyOptions options,
        string clientId = "default")
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        var roles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var role in options.Typography.ConfiguredRoles)
        {
            var property = typeof(ResponsiveTypographyOptions).GetProperty(role);
            if (property?.GetValue(options.Typography) is string value)
            {
                roles[role] = value;
            }
        }

        return new NerdTypographyPack
        {
            ClientId = clientId,
            Roles = roles,
            LineHeight = options.Typography.LineHeight,
            LetterSpacing = options.Typography.LetterSpacing,
            FontWeight = options.Typography.FontWeight
        };
    }

    public NerdResponsiveTypographyOptions ToOptions()
    {
        var options = new NerdResponsiveTypographyOptions();
        foreach (var role in Roles)
        {
            typeof(ResponsiveTypographyOptions).GetProperty(role.Key)?.SetValue(options.Typography, role.Value);
        }
        options.Typography.LineHeight = LineHeight;
        options.Typography.LetterSpacing = LetterSpacing;
        options.Typography.FontWeight = FontWeight;
        return options;
    }

    public string ToJson(JsonSerializerOptions? serializerOptions = null) =>
        JsonSerializer.Serialize(this, serializerOptions);

    public static NerdTypographyPack FromJson(string json, JsonSerializerOptions? serializerOptions = null) =>
        JsonSerializer.Deserialize<NerdTypographyPack>(
            json,
            serializerOptions ?? new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        ?? throw new ArgumentException("The typography pack JSON was empty.", nameof(json));
}
