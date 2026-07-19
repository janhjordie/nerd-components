using System.Text.Json;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public sealed class NerdTypographyPack
{
    public string ClientId { get; init; } = "default";
    public string? BrandId { get; init; }
    public string? DisplayName { get; init; }
    public string? BrandIdentityVersion { get; init; }
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

    public void ApplyTo(NerdResponsiveTypographyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ApplyTo(options.Typography);
    }

    public void ApplyTo(ResponsiveTypographyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        foreach (var role in Roles)
        {
            typeof(ResponsiveTypographyOptions).GetProperty(role.Key)?.SetValue(options, role.Value);
        }

        options.LineHeight = LineHeight;
        options.LetterSpacing = LetterSpacing;
        options.FontWeight = FontWeight;
    }

    public NerdResponsiveTypographyOptions ToOptions()
    {
        var options = new NerdResponsiveTypographyOptions();
        ApplyTo(options);
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
