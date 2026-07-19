namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Enriches token packs with pairing metadata from a brand policy or recipes.</summary>
public static class NerdTokenPackEnricher
{
    public static NerdTokenPack WithPairingPolicy(NerdTokenPack pack, INerdPairingPolicy? policy)
    {
        ArgumentNullException.ThrowIfNull(pack);
        if (policy is null)
        {
            return pack;
        }

        return new NerdTokenPack
        {
            ClientId = pack.ClientId,
            BrandId = pack.BrandId,
            DisplayName = pack.DisplayName,
            Prefix = pack.Prefix,
            Version = pack.Version,
            BrandIdentityVersion = pack.BrandIdentityVersion,
            PairingGuideName = policy.BrandGuideName,
            UpdatedAt = pack.UpdatedAt,
            Colors = new(pack.Colors, StringComparer.OrdinalIgnoreCase),
            Aliases = new(pack.Aliases, StringComparer.OrdinalIgnoreCase),
            Radii = new(pack.Radii, StringComparer.OrdinalIgnoreCase),
            Shadows = new(pack.Shadows, StringComparer.OrdinalIgnoreCase),
            Recipes = new(pack.Recipes, StringComparer.OrdinalIgnoreCase),
            Opacities = new(pack.Opacities, StringComparer.OrdinalIgnoreCase),
            ApprovedPairings = policy.GetApprovedPairings()
                .Select(pair => new NerdApprovedPairing(pair.Content, pair.Surface))
                .ToList(),
            LockedTokens = [..pack.LockedTokens]
        };
    }

    public static NerdTokenPack FromBrandPack(INerdBrandPack brandPack, string clientId = "reference")
    {
        ArgumentNullException.ThrowIfNull(brandPack);
        var pack = NerdBrandPackRegistry.Instance.CreateTokenPack(brandPack.Id, clientId);
        pack = new NerdTokenPack
        {
            ClientId = pack.ClientId,
            BrandId = brandPack.Id,
            DisplayName = brandPack.DisplayName,
            Prefix = pack.Prefix,
            Version = pack.Version,
            BrandIdentityVersion = brandPack.IdentityVersion,
            UpdatedAt = pack.UpdatedAt,
            Colors = new(pack.Colors, StringComparer.OrdinalIgnoreCase),
            Aliases = new(pack.Aliases, StringComparer.OrdinalIgnoreCase),
            Radii = new(pack.Radii, StringComparer.OrdinalIgnoreCase),
            Shadows = new(pack.Shadows, StringComparer.OrdinalIgnoreCase),
            Recipes = new(pack.Recipes, StringComparer.OrdinalIgnoreCase),
            Opacities = new(pack.Opacities, StringComparer.OrdinalIgnoreCase),
            LockedTokens = [..pack.LockedTokens]
        };
        return WithPairingPolicy(pack, brandPack.PairingPolicy);
    }

    /// <summary>
    /// Copies pairing foreground/surface roles from a reference options graph (typically C# presets) into pack colors.
    /// </summary>
    public static NerdTokenPack EnrichPairingColors(NerdTokenPack pack, NerdDesignTokenOptions referenceOptions)
    {
        ArgumentNullException.ThrowIfNull(pack);
        ArgumentNullException.ThrowIfNull(referenceOptions);

        var policy = referenceOptions.PairingPolicy;
        if (policy is null || !policy.IsActive(referenceOptions))
        {
            return pack;
        }

        var colors = new Dictionary<string, NerdColorToken>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, token) in pack.Colors)
        {
            var baseColor = token.Light ?? token.Value;
            var content = token.Content;
            var surface = token.Surface;
            var foreground = policy.ResolveForegroundColor(name, referenceOptions);
            if (!string.Equals(foreground, baseColor, StringComparison.OrdinalIgnoreCase))
            {
                content = foreground;
            }

            var surfaceColor = policy.ResolveSurfaceColor(name, referenceOptions);
            if (!string.Equals(surfaceColor, baseColor, StringComparison.OrdinalIgnoreCase))
            {
                surface = surfaceColor;
            }

            colors[name] = new NerdColorToken
            {
                Value = token.Value,
                Light = token.Light,
                Dark = token.Dark,
                ContrastText = token.ContrastText,
                DarkContrastText = token.DarkContrastText,
                Surface = surface,
                Content = content,
                Interactive = token.Interactive,
                Hover = token.Hover,
                Active = token.Active,
                Border = token.Border,
                Disabled = token.Disabled,
                Description = token.Description,
                Roles = token.Roles
            };
        }

        return new NerdTokenPack
        {
            ClientId = pack.ClientId,
            BrandId = pack.BrandId,
            DisplayName = pack.DisplayName,
            Prefix = pack.Prefix,
            Version = pack.Version,
            BrandIdentityVersion = pack.BrandIdentityVersion,
            PairingGuideName = pack.PairingGuideName,
            UpdatedAt = pack.UpdatedAt,
            Colors = colors,
            Aliases = new(pack.Aliases, StringComparer.OrdinalIgnoreCase),
            Radii = new(pack.Radii, StringComparer.OrdinalIgnoreCase),
            Shadows = new(pack.Shadows, StringComparer.OrdinalIgnoreCase),
            Recipes = new(pack.Recipes, StringComparer.OrdinalIgnoreCase),
            Opacities = new(pack.Opacities, StringComparer.OrdinalIgnoreCase),
            ApprovedPairings = [..pack.ApprovedPairings],
            LockedTokens = [..pack.LockedTokens]
        };
    }

    public static NerdTokenPack EnrichRecipeMetadata(NerdTokenPack pack, string brandId)
    {
        ArgumentNullException.ThrowIfNull(pack);
        if (!RecipeMetadataByBrand.TryGetValue(brandId, out var metadata) || metadata.Count == 0)
        {
            return pack;
        }

        var recipes = new Dictionary<string, NerdDesignTokenRecipe>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, recipe) in pack.Recipes)
        {
            if (!metadata.TryGetValue(name, out var meta))
            {
                recipes[name] = recipe;
                continue;
            }

            recipes[name] = recipe with
            {
                Label = string.IsNullOrWhiteSpace(recipe.Label) ? meta.Label : recipe.Label,
                Usage = string.IsNullOrWhiteSpace(recipe.Usage) ? meta.Usage : recipe.Usage
            };
        }

        return new NerdTokenPack
        {
            ClientId = pack.ClientId,
            BrandId = pack.BrandId,
            DisplayName = pack.DisplayName,
            Prefix = pack.Prefix,
            Version = pack.Version,
            BrandIdentityVersion = pack.BrandIdentityVersion,
            PairingGuideName = pack.PairingGuideName,
            UpdatedAt = pack.UpdatedAt,
            Colors = new(pack.Colors, StringComparer.OrdinalIgnoreCase),
            Aliases = new(pack.Aliases, StringComparer.OrdinalIgnoreCase),
            Radii = new(pack.Radii, StringComparer.OrdinalIgnoreCase),
            Shadows = new(pack.Shadows, StringComparer.OrdinalIgnoreCase),
            Recipes = recipes,
            Opacities = new(pack.Opacities, StringComparer.OrdinalIgnoreCase),
            ApprovedPairings = [..pack.ApprovedPairings],
            LockedTokens = [..pack.LockedTokens]
        };
    }

    private static readonly Dictionary<string, Dictionary<string, (string Label, string Usage)>> RecipeMetadataByBrand =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["tnc"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["hero"] = ("Hero banner", "Full-width hero with chalk headline on navy"),
                ["header"] = ("Site header", "Light header bar with ink links"),
                ["tagline"] = ("Tagline strip", "Navy strip with coral accent copy"),
                ["cta"] = ("Primary CTA", "Coral button on chalk body copy")
            },
            ["dnf"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["kridt-himmel"] = ("Kridt card", "Default card with skov text and himmel action"),
                ["hero"] = ("Hero surface", "Light hero with skov headline"),
                ["cta-strip"] = ("CTA strip", "Skov strip with kridt text and sol action"),
                ["link-card"] = ("Link card", "Kridt card with skov content"),
                ["footer"] = ("Footer", "Dark jord footer with kridt text")
            },
            ["acme"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["hero"] = ("Marketing hero", "Cloud surface with ink headline")
            },
            ["demo"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["cta-strip"] = ("CTA strip", "Slate strip with paper text and sky action")
            }
        };
}
