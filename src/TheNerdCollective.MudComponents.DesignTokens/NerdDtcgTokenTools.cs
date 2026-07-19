using System.Text.Json;
using System.Text.Json.Nodes;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>W3C Design Tokens Format import/export (HR-106 / TS-013).</summary>
public static class NerdDtcgTokenTools
{
    private const string DtcgSchema = "https://design-tokens.github.io/community-group/format/";

    public static string Export(NerdDesignTokenOptions options) =>
        Export(options, new NerdTokenPackExportRequest());

    public static string Export(NerdDesignTokenOptions options, NerdTokenPackExportRequest request)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(request);

        var root = new JsonObject
        {
            ["$schema"] = DtcgSchema,
            ["$description"] = $"Nerd design tokens ({options.Prefix})"
        };

        if (request.IncludeColors && options.Colors.Count > 0)
        {
            var colorGroup = new JsonObject();
            foreach (var pair in options.Colors.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                colorGroup[pair.Key] = CreateColorToken(pair.Value);
            }

            root["color"] = colorGroup;
        }

        if (request.IncludeSpacing && options.Spacing.Count > 0)
        {
            root["spacing"] = CreateGroup(options.Spacing, "dimension");
        }

        if (request.IncludeRadii && options.Radii.Count > 0)
        {
            root["radius"] = CreateGroup(options.Radii, "dimension");
        }

        if (request.IncludeShadows && options.Shadows.Count > 0)
        {
            root["shadow"] = CreateGroup(options.Shadows, "shadow");
        }

        if (request.IncludeBreakpoints && options.Breakpoints.Count > 0)
        {
            root["breakpoint"] = CreateGroup(options.Breakpoints, "dimension");
        }

        if (request.IncludeMotion)
        {
            if (options.MotionDurations.Count > 0)
            {
                root["duration"] = CreateGroup(options.MotionDurations, "duration");
            }

            if (options.MotionEasings.Count > 0)
            {
                root["easing"] = CreateGroup(options.MotionEasings, "cubicBezier");
            }
        }

        if (request.IncludeZIndex && options.ZIndex.Count > 0)
        {
            root["zIndex"] = CreateGroup(options.ZIndex, "number");
        }

        return root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    public static NerdDtcgImportResult TryImport(NerdDesignTokenOptions options, string json)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(json))
        {
            return NerdDtcgImportResult.Fail("DTCG JSON was empty.");
        }

        try
        {
            var count = Import(options, json);
            return NerdDtcgImportResult.Ok(count);
        }
        catch (JsonException ex)
        {
            return NerdDtcgImportResult.Fail($"Invalid JSON: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            return NerdDtcgImportResult.Fail(ex.Message);
        }
    }

    public static int Import(NerdDesignTokenOptions options, string json)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var root = JsonNode.Parse(json) as JsonObject
            ?? throw new ArgumentException("DTCG document root must be an object.", nameof(json));

        var counter = new ImportCounter();
        ImportObject(options, root, group: null, prefix: null, counter);
        return counter.Count;
    }

    private static void ImportObject(
        NerdDesignTokenOptions options,
        JsonObject node,
        string? group,
        string? prefix,
        ImportCounter counter)
    {
        foreach (var pair in node)
        {
            if (pair.Key.StartsWith("$", StringComparison.Ordinal))
            {
                continue;
            }

            if (pair.Value is not JsonObject tokenNode)
            {
                continue;
            }

            var name = string.IsNullOrWhiteSpace(prefix) ? pair.Key : $"{prefix}-{pair.Key}";
            if (TryReadTokenValue(tokenNode, out var value, out var type))
            {
                ImportToken(options, name, value, type, group ?? InferGroup(type), counter);
                continue;
            }

            if (IsTypedRootGroup(pair.Key))
            {
                ImportObject(options, tokenNode, pair.Key, prefix: null, counter);
                continue;
            }

            ImportObject(options, tokenNode, group ?? pair.Key, name, counter);
        }
    }

    private static bool IsTypedRootGroup(string key) =>
        key.Trim().ToLowerInvariant() switch
        {
            "color" or "spacing" or "radius" or "shadow" or "breakpoint"
                or "duration" or "easing" or "zindex" or "z-index" => true,
            _ => false
        };

    private static string? InferGroup(string type) =>
        type.Trim().ToLowerInvariant() switch
        {
            "color" => "color",
            "dimension" => "spacing",
            "duration" => "duration",
            "cubicbezier" => "easing",
            "shadow" => "shadow",
            "number" => "zIndex",
            _ => null
        };

    private static void ImportToken(
        NerdDesignTokenOptions options,
        string name,
        string value,
        string type,
        string? group,
        ImportCounter counter)
    {
        switch (group?.Trim().ToLowerInvariant())
        {
            case "color":
                options.Add(name, new NerdColorToken { Value = value });
                counter.Increment();
                return;
            case "spacing":
                options.AddSpacing(name, value);
                counter.Increment();
                return;
            case "radius":
                options.AddRadius(name, value);
                counter.Increment();
                return;
            case "breakpoint":
                options.AddBreakpoint(name, value);
                counter.Increment();
                return;
            case "shadow":
                options.AddShadow(name, value);
                counter.Increment();
                return;
            case "duration":
                options.AddMotionDuration(name, value);
                counter.Increment();
                return;
            case "easing":
                options.AddMotionEasing(name, value);
                counter.Increment();
                return;
            case "zindex":
                options.AddZIndex(name, value);
                counter.Increment();
                return;
        }

        switch (type.Trim().ToLowerInvariant())
        {
            case "color":
                options.Add(name, new NerdColorToken { Value = value });
                counter.Increment();
                break;
            case "dimension":
                options.AddSpacing(name, value);
                counter.Increment();
                break;
            case "shadow":
                options.AddShadow(name, value);
                counter.Increment();
                break;
            case "duration":
                options.AddMotionDuration(name, value);
                counter.Increment();
                break;
            case "cubicbezier":
                options.AddMotionEasing(name, value);
                counter.Increment();
                break;
            case "number":
                options.AddZIndex(name, value);
                counter.Increment();
                break;
            default:
                if (value.StartsWith('#') || value.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
                {
                    options.Add(name, new NerdColorToken { Value = value });
                    counter.Increment();
                }

                break;
        }
    }

    private static JsonObject CreateGroup(IReadOnlyDictionary<string, string> values, string type)
    {
        var group = new JsonObject();
        foreach (var pair in values.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            group[pair.Key] = CreateToken(pair.Value, type);
        }

        return group;
    }

    private static JsonObject CreateColorToken(NerdColorToken token)
    {
        var node = CreateToken(token.Value ?? token.Light ?? "#000000", "color");
        if (!string.IsNullOrWhiteSpace(token.Description))
        {
            node["$description"] = token.Description;
        }

        return node;
    }

    private static JsonObject CreateToken(string value, string type) =>
        new()
        {
            ["$value"] = value,
            ["$type"] = type
        };

    private static bool TryReadTokenValue(JsonNode? node, out string value, out string type)
    {
        value = string.Empty;
        type = string.Empty;
        if (node is not JsonObject token)
        {
            return false;
        }

        if (token["$value"] is JsonValue rawValue)
        {
            value = rawValue.ToString();
            type = token["$type"]?.GetValue<string>() ?? string.Empty;
            return !string.IsNullOrWhiteSpace(value);
        }

        return false;
    }

    private sealed class ImportCounter
    {
        public int Count { get; private set; }

        public void Increment() => Count++;
    }
}

public sealed class NerdDtcgImportResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int ImportedTokenCount { get; init; }

    public static NerdDtcgImportResult Ok(int count) =>
        new() { Success = true, ImportedTokenCount = count, Message = $"Imported {count} DTCG token(s)." };

    public static NerdDtcgImportResult Fail(string message) =>
        new() { Success = false, Message = message };
}
