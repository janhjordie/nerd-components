using System.Text.Json;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public static class NerdTypographyTools
{
    public static string ExportTokensStudioJson(NerdResponsiveTypographyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var fontSizeTokens = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        foreach (var role in options.Typography.ConfiguredRoles)
        {
            var property = typeof(ResponsiveTypographyOptions).GetProperty(role);
            if (property?.GetValue(options.Typography) is string value)
            {
                fontSizeTokens[role] = new Dictionary<string, string>
                {
                    ["value"] = value,
                    ["type"] = "fontSizes"
                };
            }
        }

        var payload = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["$schema"] = "https://tokens.studio/schemas/1.0.0",
            ["fontSizes"] = fontSizeTokens
        };

        if (!string.IsNullOrWhiteSpace(options.Typography.LineHeight))
        {
            payload["lineHeights"] = new Dictionary<string, object>
            {
                ["default"] = new Dictionary<string, string>
                {
                    ["value"] = options.Typography.LineHeight!,
                    ["type"] = "lineHeights"
                }
            };
        }

        if (!string.IsNullOrWhiteSpace(options.Typography.LetterSpacing))
        {
            payload["letterSpacing"] = new Dictionary<string, object>
            {
                ["default"] = new Dictionary<string, string>
                {
                    ["value"] = options.Typography.LetterSpacing!,
                    ["type"] = "letterSpacing"
                }
            };
        }

        if (!string.IsNullOrWhiteSpace(options.Typography.FontWeight))
        {
            payload["fontWeights"] = new Dictionary<string, object>
            {
                ["default"] = new Dictionary<string, string>
                {
                    ["value"] = options.Typography.FontWeight!,
                    ["type"] = "fontWeights"
                }
            };
        }

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
    }

    public static void ImportTokensStudioJson(NerdResponsiveTypographyOptions options, string json)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (root.TryGetProperty("fontSizes", out var fontSizes) &&
            fontSizes.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in fontSizes.EnumerateObject())
            {
                var value = ReadTokenValue(property.Value);
                if (value is not null)
                {
                    typeof(ResponsiveTypographyOptions).GetProperty(property.Name)?.SetValue(options.Typography, value);
                }
            }
        }

        options.Typography.LineHeight = ReadGroupDefault(root, "lineHeights") ?? options.Typography.LineHeight;
        options.Typography.LetterSpacing = ReadGroupDefault(root, "letterSpacing") ?? options.Typography.LetterSpacing;
        options.Typography.FontWeight = ReadGroupDefault(root, "fontWeights") ?? options.Typography.FontWeight;
    }

    private static string? ReadGroupDefault(JsonElement root, string groupName)
    {
        if (!root.TryGetProperty(groupName, out var group) || group.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (group.TryGetProperty("default", out var defaultToken))
        {
            return ReadTokenValue(defaultToken);
        }

        foreach (var property in group.EnumerateObject())
        {
            var value = ReadTokenValue(property.Value);
            if (value is not null)
            {
                return value;
            }
        }

        return null;
    }

    private static string? ReadTokenValue(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Object when element.TryGetProperty("value", out var value) => value.GetString(),
            _ => null
        };
}
