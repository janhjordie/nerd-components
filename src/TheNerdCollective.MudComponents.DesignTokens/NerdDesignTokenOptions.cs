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

    public string Prefix { get; set; } = "nerd";
    public string MudBlazorVersion { get; set; } = "9.6";
    public bool UseCssLayer { get; set; }
    public string CssLayerName { get; set; } = "nerd-design-tokens";
    public string? ScopeSelector { get; set; }
    public bool MinifyCss { get; set; }
    public bool UseImportantOverrides { get; set; } = true;
    public bool EnableCatalogPage { get; set; } = true;
    public string CatalogRoute { get; set; } = "/nerd-design-tokens";
    public bool RestrictCatalogToDevelopment { get; set; } = true;
    public bool WarnOnAccessibilityFailuresAtStartup { get; set; } = true;
    public string WcagVersion { get; set; } = NerdDesignTokenTools.DefaultWcagVersion;

    public IReadOnlyDictionary<string, NerdColorToken> Colors => _colors;
    public IReadOnlySet<string> ConfiguredColors => _configuredColors;
    public IReadOnlyDictionary<string, string> Aliases => _aliases;
    public IReadOnlySet<string> ConfiguredAliases => _configuredAliases;
    public IReadOnlyDictionary<string, string> Radii => _radii;
    public IReadOnlyDictionary<string, string> Shadows => _shadows;
    public IReadOnlyDictionary<string, NerdDesignTokenRecipe> Recipes => _recipes;

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
}
