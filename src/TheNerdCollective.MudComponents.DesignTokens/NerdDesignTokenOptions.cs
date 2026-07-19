namespace TheNerdCollective.MudComponents.DesignTokens;

public sealed class NerdDesignTokenOptions
{
    private readonly Dictionary<string, NerdColorToken> _colors = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _configuredColors = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _aliases = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _configuredAliases = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _radii = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _shadows = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, NerdDesignTokenRecipe> _recipes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, NerdOpacityToken> _opacities = new(StringComparer.OrdinalIgnoreCase);

    public string Prefix { get; set; } = "nerd";
    public string MudBlazorVersion { get; set; } = "9.6";
    public bool UseCssLayer { get; set; }
    public string CssLayerName { get; set; } = "nerd-design-tokens";
    public string? ScopeSelector { get; set; }
    public bool EnablePortalTokenScope { get; set; } = true;
    public string PortalScopeSelector { get; set; } = ".mud-popover-open";
    public bool MinifyCss { get; set; }
    public bool UseImportantOverrides { get; set; } = true;
    public bool EnableCatalogPage { get; set; } = true;
    public string CatalogRoute { get; set; } = "/nerd-design-tokens";
    public string RecipesCatalogRoute { get; set; } = "/nerd-design-token-recipes";
    public bool RestrictCatalogToDevelopment { get; set; } = true;
    public bool WarnOnAccessibilityFailuresAtStartup { get; set; } = true;
    public string WcagVersion { get; set; } = NerdDesignTokenTools.DefaultWcagVersion;

    /// <summary>Brand pack restored on each new Blazor circuit (set during startup configure).</summary>
    public string? DefaultBrandPackId { get; set; }

    /// <summary>Active brand pack id after <see cref="NerdBrandPackRegistry.Configure"/>.</summary>
    public string? ActiveBrandPackId { get; set; }

    /// <summary>Identity guide revision for the active brand pack.</summary>
    public string? ActiveBrandIdentityVersion { get; set; }

    /// <summary>Brand-specific pairing rules, if the installed brand pack provides them.</summary>
    public INerdPairingPolicy? PairingPolicy { get; set; }

    /// <summary>Token names that must not be edited in the live studio.</summary>
    public List<string> LockedTokens { get; set; } = [];

    public bool IsTokenLocked(string tokenName) =>
        !string.IsNullOrWhiteSpace(tokenName) &&
        LockedTokens.Any(name => string.Equals(name, tokenName, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyDictionary<string, NerdColorToken> Colors => _colors;
    public IReadOnlySet<string> ConfiguredColors => _configuredColors;
    public IReadOnlyDictionary<string, string> Aliases => _aliases;
    public IReadOnlySet<string> ConfiguredAliases => _configuredAliases;
    public IReadOnlyDictionary<string, string> Radii => _radii;
    public IReadOnlyDictionary<string, string> Shadows => _shadows;
    public IReadOnlyDictionary<string, NerdDesignTokenRecipe> Recipes => _recipes;
    public IReadOnlyDictionary<string, NerdOpacityToken> Opacities => _opacities;

    public bool IsAlias(string name) => _aliases.ContainsKey(name);

    public NerdDesignTokenOptions Add(string name, NerdColorToken token)
    {
        NerdTokenNameValidator.Validate(name);
        ArgumentNullException.ThrowIfNull(token);
        _colors[name] = token;
        _configuredColors.Add(name);
        return this;
    }

    public NerdDesignTokenOptions Alias(string name, string target)
    {
        NerdTokenNameValidator.Validate(name);
        NerdTokenNameValidator.Validate(target);
        _aliases[name] = target;
        _configuredAliases.Add(name);
        return this;
    }

    public NerdDesignTokenOptions AddRadius(string name, string value)
    {
        NerdTokenNameValidator.Validate(name);
        _radii[name] = NerdColorValue.Validate(value, nameof(value));
        return this;
    }

    public NerdDesignTokenOptions AddShadow(string name, string value)
    {
        NerdTokenNameValidator.Validate(name);
        _shadows[name] = NerdColorValue.Validate(value, nameof(value));
        return this;
    }

    public NerdDesignTokenOptions AddRecipe(string name, NerdDesignTokenRecipe recipe)
    {
        NerdTokenNameValidator.Validate(name);
        ArgumentNullException.ThrowIfNull(recipe);
        NerdTokenNameValidator.Validate(recipe.Surface);
        NerdTokenNameValidator.Validate(recipe.Content);
        if (recipe.Action is not null)
        {
            NerdTokenNameValidator.Validate(recipe.Action);
        }
        if (recipe.Border is not null)
        {
            NerdTokenNameValidator.Validate(recipe.Border);
        }

        _recipes[name] = recipe;
        return this;
    }

    public NerdDesignTokenOptions AddOpacity(string name, NerdOpacityToken token)
    {
        NerdTokenNameValidator.Validate(name);
        ArgumentNullException.ThrowIfNull(token);
        token.Validate();
        _opacities[name] = token;
        return this;
    }

    public bool RemoveColor(string name)
    {
        if (IsTokenLocked(name))
        {
            return false;
        }

        _colors.Remove(name);
        _configuredColors.Remove(name);
        return true;
    }

    public bool RemoveAlias(string name)
    {
        _aliases.Remove(name);
        _configuredAliases.Remove(name);
        return true;
    }

    public bool RemoveRecipe(string name) => _recipes.Remove(name);

    public void CopyHostSettingsFrom(NerdDesignTokenOptions source)
    {
        ArgumentNullException.ThrowIfNull(source);
        MudBlazorVersion = source.MudBlazorVersion;
        UseCssLayer = source.UseCssLayer;
        CssLayerName = source.CssLayerName;
        ScopeSelector = source.ScopeSelector;
        EnablePortalTokenScope = source.EnablePortalTokenScope;
        PortalScopeSelector = source.PortalScopeSelector;
        MinifyCss = source.MinifyCss;
        UseImportantOverrides = source.UseImportantOverrides;
        EnableCatalogPage = source.EnableCatalogPage;
        CatalogRoute = source.CatalogRoute;
        RecipesCatalogRoute = source.RecipesCatalogRoute;
        RestrictCatalogToDevelopment = source.RestrictCatalogToDevelopment;
        WarnOnAccessibilityFailuresAtStartup = source.WarnOnAccessibilityFailuresAtStartup;
        WcagVersion = source.WcagVersion;
    }

    public void ReplaceWith(NerdDesignTokenOptions source)
    {
        ArgumentNullException.ThrowIfNull(source);
        _colors.Clear();
        _configuredColors.Clear();
        _aliases.Clear();
        _configuredAliases.Clear();
        _radii.Clear();
        _shadows.Clear();
        _recipes.Clear();
        _opacities.Clear();
        Prefix = source.Prefix;
        ActiveBrandPackId = source.ActiveBrandPackId;
        ActiveBrandIdentityVersion = source.ActiveBrandIdentityVersion;
        PairingPolicy = source.PairingPolicy;
        LockedTokens = [..source.LockedTokens];

        foreach (var pair in source.Colors)
        {
            Add(pair.Key, pair.Value);
        }

        foreach (var pair in source.Aliases)
        {
            Alias(pair.Key, pair.Value);
        }

        foreach (var pair in source.Radii)
        {
            AddRadius(pair.Key, pair.Value);
        }

        foreach (var pair in source.Shadows)
        {
            AddShadow(pair.Key, pair.Value);
        }

        foreach (var pair in source.Recipes)
        {
            AddRecipe(pair.Key, pair.Value);
        }

        foreach (var pair in source.Opacities)
        {
            AddOpacity(pair.Key, pair.Value);
        }
    }
}
