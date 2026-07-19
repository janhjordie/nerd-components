using System.Text.Json;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// JSON-serializable design-token pack that can be persisted per client.
/// </summary>
public sealed class NerdTokenPack
{
    public string ClientId { get; init; } = "default";
    public string? BrandId { get; init; }
    public string? DisplayName { get; init; }
    public string Prefix { get; init; } = "nerd";
    public int Version { get; init; } = 2;
    public string? BrandIdentityVersion { get; init; }
    public string? PairingGuideName { get; init; }
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
    public Dictionary<string, NerdColorToken> Colors { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> Aliases { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> Radii { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> Shadows { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, NerdDesignTokenRecipe> Recipes { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, NerdOpacityToken> Opacities { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public List<NerdApprovedPairing> ApprovedPairings { get; init; } = [];
    public List<string> LockedTokens { get; init; } = [];

    public static NerdTokenPack FromOptions(NerdDesignTokenOptions options, string clientId = "default")
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        return new NerdTokenPack
        {
            ClientId = clientId,
            BrandId = options.ActiveBrandPackId,
            Prefix = options.Prefix,
            BrandIdentityVersion = options.ActiveBrandIdentityVersion,
            Colors = new(options.Colors, StringComparer.OrdinalIgnoreCase),
            Aliases = new(options.Aliases, StringComparer.OrdinalIgnoreCase),
            Radii = new(options.Radii, StringComparer.OrdinalIgnoreCase),
            Shadows = new(options.Shadows, StringComparer.OrdinalIgnoreCase),
            Recipes = new(options.Recipes, StringComparer.OrdinalIgnoreCase),
            Opacities = new(options.Opacities, StringComparer.OrdinalIgnoreCase)
        };
    }

    public static NerdTokenPack FromPreset(string name, string clientId = "default") =>
        NerdBrandPackRegistry.Instance.CreateTokenPack(name, clientId);

    public NerdTokenPack Merge(NerdTokenPack overrides)
    {
        ArgumentNullException.ThrowIfNull(overrides);
        var merged = ToOptions();
        merged.ActiveBrandIdentityVersion = overrides.BrandIdentityVersion ?? BrandIdentityVersion;
        foreach (var color in overrides.Colors)
        {
            merged.Add(color.Key, color.Value);
        }
        foreach (var alias in overrides.Aliases)
        {
            merged.Alias(alias.Key, alias.Value);
        }
        foreach (var radius in overrides.Radii)
        {
            merged.AddRadius(radius.Key, radius.Value);
        }
        foreach (var shadow in overrides.Shadows)
        {
            merged.AddShadow(shadow.Key, shadow.Value);
        }
        foreach (var recipe in overrides.Recipes)
        {
            merged.AddRecipe(recipe.Key, recipe.Value);
        }
        foreach (var opacity in overrides.Opacities)
        {
            merged.AddOpacity(opacity.Key, opacity.Value);
        }

        return FromOptions(merged, overrides.ClientId);
    }

    public void ApplyTo(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.ReplaceWith(ToOptions());
        options.ActiveBrandPackId = BrandId ?? Prefix;
        if (NerdJsonPairingPolicy.TryCreate(this, out var jsonPolicy))
        {
            options.PairingPolicy = jsonPolicy;
        }
    }

    public bool IsTokenLocked(string tokenName) =>
        !string.IsNullOrWhiteSpace(tokenName) &&
        LockedTokens.Any(name => string.Equals(name, tokenName, StringComparison.OrdinalIgnoreCase));

    public NerdDesignTokenOptions ToOptions()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ClientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(Prefix);
        var options = new NerdDesignTokenOptions
        {
            Prefix = Prefix,
            ActiveBrandPackId = BrandId ?? Prefix,
            ActiveBrandIdentityVersion = BrandIdentityVersion,
            LockedTokens = [..LockedTokens]
        };
        foreach (var pair in Colors)
        {
            options.Add(pair.Key, pair.Value);
        }
        foreach (var pair in Aliases)
        {
            options.Alias(pair.Key, pair.Value);
        }
        foreach (var pair in Radii)
        {
            options.AddRadius(pair.Key, pair.Value);
        }
        foreach (var pair in Shadows)
        {
            options.AddShadow(pair.Key, pair.Value);
        }
        foreach (var pair in Recipes)
        {
            options.AddRecipe(pair.Key, pair.Value);
        }
        foreach (var pair in Opacities)
        {
            options.AddOpacity(pair.Key, pair.Value);
        }
        return options;
    }

    public string ToJson(JsonSerializerOptions? serializerOptions = null) =>
        JsonSerializer.Serialize(this, serializerOptions ?? DefaultJsonOptions);

    public static NerdTokenPack FromJson(string json, JsonSerializerOptions? serializerOptions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var pack = JsonSerializer.Deserialize<NerdTokenPack>(json, serializerOptions ?? DefaultJsonOptions)
            ?? throw new ArgumentException("The token pack JSON was empty.", nameof(json));
        pack.Validate();
        return pack;
    }

    private static readonly JsonSerializerOptions DefaultJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ClientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(Prefix);
        var options = ToOptions();

        foreach (var alias in Aliases)
        {
            if (!Colors.ContainsKey(alias.Value) && !Aliases.ContainsKey(alias.Value))
            {
                throw new ArgumentException(
                    $"Alias '{alias.Key}' references missing token '{alias.Value}'.",
                    nameof(Aliases));
            }
        }

        foreach (var recipe in Recipes)
        {
            if (!Colors.ContainsKey(recipe.Value.Surface) ||
                !Colors.ContainsKey(recipe.Value.Content) ||
                (recipe.Value.Action is not null && !Colors.ContainsKey(recipe.Value.Action)) ||
                (recipe.Value.Border is not null && !Colors.ContainsKey(recipe.Value.Border)))
            {
                throw new ArgumentException(
                    $"Recipe '{recipe.Key}' references a missing color token.",
                    nameof(Recipes));
            }
        }

        _ = NerdDesignTokenTools.CheckAccessibility(options);
    }
}
