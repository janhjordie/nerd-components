using System.Text.Json;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// JSON-serializable design-token pack that can be persisted per client.
/// </summary>
public sealed class NerdTokenPack
{
    public string ClientId { get; init; } = "default";
    public string Prefix { get; init; } = "nerd";
    public int Version { get; init; } = 1;
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
    public Dictionary<string, NerdColorToken> Colors { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> Aliases { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> Radii { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> Shadows { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, NerdDesignTokenRecipe> Recipes { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public static NerdTokenPack FromOptions(NerdDesignTokenOptions options, string clientId = "default")
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        return new NerdTokenPack
        {
            ClientId = clientId,
            Prefix = options.Prefix,
            Colors = new(options.Colors, StringComparer.OrdinalIgnoreCase),
            Aliases = new(options.Aliases, StringComparer.OrdinalIgnoreCase),
            Radii = new(options.Radii, StringComparer.OrdinalIgnoreCase),
            Shadows = new(options.Shadows, StringComparer.OrdinalIgnoreCase),
            Recipes = new(options.Recipes, StringComparer.OrdinalIgnoreCase)
        };
    }

    public static NerdTokenPack FromPreset(string name, string clientId = "default")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var options = new NerdDesignTokenOptions { Prefix = name.Equals("dnf", StringComparison.OrdinalIgnoreCase) ? "dnf" : "nerd" };
        if (name.Equals("dnf", StringComparison.OrdinalIgnoreCase))
        {
            NerdDnfDesignTokenPresets.Apply(options);
        }
        else
        {
            throw new ArgumentException($"Unknown token preset '{name}'.", nameof(name));
        }

        return FromOptions(options, clientId);
    }

    public NerdDesignTokenOptions ToOptions()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ClientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(Prefix);
        var options = new NerdDesignTokenOptions { Prefix = Prefix };
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
        return options;
    }

    public string ToJson(JsonSerializerOptions? serializerOptions = null) =>
        JsonSerializer.Serialize(this, serializerOptions);

    public static NerdTokenPack FromJson(string json, JsonSerializerOptions? serializerOptions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        serializerOptions ??= new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var pack = JsonSerializer.Deserialize<NerdTokenPack>(json, serializerOptions)
            ?? throw new ArgumentException("The token pack JSON was empty.", nameof(json));
        pack.Validate();
        return pack;
    }

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
