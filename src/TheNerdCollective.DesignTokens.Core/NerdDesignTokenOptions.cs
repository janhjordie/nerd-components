namespace TheNerdCollective.MudComponents.DesignTokens;

using System.Text.RegularExpressions;

public sealed class NerdDesignTokenOptions
{
    private static readonly Regex SpacingNamePattern = new(
        "^[a-z0-9]+(?:-[a-z0-9]+)*$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private readonly Dictionary<string, NerdColorToken> _colors = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _configuredColors = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _aliases = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _configuredAliases = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _radii = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _shadows = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _spacing = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _breakpoints = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _motionDurations = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _motionEasings = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _zIndex = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, NerdTokenTransform> _transforms = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, NerdDesignTokenRecipe> _recipes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, NerdOpacityToken> _opacities = new(StringComparer.OrdinalIgnoreCase);
    private NerdTokenPackShell? _shell;
    private NerdFrameworkDefaults? _frameworkDefaults;
    private readonly Dictionary<string, NerdThemeSet> _themeSets = new(StringComparer.OrdinalIgnoreCase);

    public string Prefix { get; set; } = "nerd";
    public string MudBlazorVersion { get; set; } = "9.7.0";
    public bool UseCssLayer { get; set; }
    public string CssLayerName { get; set; } = "nerd-design-tokens";
    public string? ScopeSelector { get; set; }
    public bool EnablePortalTokenScope { get; set; } = true;
    public string PortalScopeSelector { get; set; } = ".mud-popover-open";
    public bool MinifyCss { get; set; }
    public bool UseImportantOverrides { get; set; } = true;

    /// <summary>
    /// When true, emit full Mud palette once at brand root and intent-scoped overrides
    /// instead of flattening all palette channels onto every token class.
    /// </summary>
    public bool UsePaletteFirstAdapter { get; set; } = true;

    /// <summary>
    /// When true with palette-first mode, intent palette overrides are emitted via
    /// <see cref="NerdMudThemeProvider"/> PseudoCss scopes instead of token CSS.
    /// </summary>
    public bool UseIntentPseudoCssThemes { get; set; }

    /// <summary>
    /// When true with palette-first mode, emit <c>--nerd-intent-*</c> CSS variables.
    /// </summary>
    public bool EmitFrameworkNeutralIntents { get; set; } = true;

    /// <summary>
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
    public IReadOnlyDictionary<string, string> Spacing => _spacing;
    public IReadOnlyDictionary<string, string> Breakpoints => _breakpoints;
    public IReadOnlyDictionary<string, string> MotionDurations => _motionDurations;
    public IReadOnlyDictionary<string, string> MotionEasings => _motionEasings;
    public IReadOnlyDictionary<string, string> ZIndex => _zIndex;
    public IReadOnlyDictionary<string, NerdTokenTransform> Transforms => _transforms;
    public IReadOnlyDictionary<string, NerdDesignTokenRecipe> Recipes => _recipes;
    public IReadOnlyDictionary<string, NerdOpacityToken> Opacities => _opacities;
    public IReadOnlyDictionary<string, NerdThemeSet> ThemeSets => _themeSets;

    /// <summary>Application shell bindings (app bar, drawer, nav menu, main).</summary>
    public NerdTokenPackShell? Shell
    {
        get => _shell;
        set
        {
            value?.Validate();
            _shell = value;
        }
    }

    /// <summary>Per-UI-framework default intent mappings.</summary>
    public NerdFrameworkDefaults? FrameworkDefaults
    {
        get => _frameworkDefaults;
        set => _frameworkDefaults = value;
    }

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

    public NerdDesignTokenOptions AddSpacing(string name, string value)
    {
        ValidateFoundationName(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        _spacing[name] = value.Trim();
        return this;
    }

    public NerdDesignTokenOptions AddBreakpoint(string name, string value) =>
        AddFoundationToken(_breakpoints, name, value);

    public NerdDesignTokenOptions AddMotionDuration(string name, string value) =>
        AddFoundationToken(_motionDurations, name, value);

    public NerdDesignTokenOptions AddMotionEasing(string name, string value) =>
        AddFoundationToken(_motionEasings, name, value);

    public NerdDesignTokenOptions AddZIndex(string name, string value) =>
        AddFoundationToken(_zIndex, name, value);

    public NerdDesignTokenOptions AddTransform(string name, NerdTokenTransform transform)
    {
        NerdTokenNameValidator.Validate(name);
        ArgumentNullException.ThrowIfNull(transform);
        transform.Validate();
        _transforms[name] = transform;
        return this;
    }

    public NerdDesignTokenOptions RemoveTransform(string name)
    {
        _transforms.Remove(name);
        RemoveColor(name);
        return this;
    }

    private NerdDesignTokenOptions AddFoundationToken(Dictionary<string, string> target, string name, string value)
    {
        ValidateFoundationName(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        target[name] = value.Trim();
        return this;
    }

    private static void ValidateFoundationName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (!SpacingNamePattern.IsMatch(name))
        {
            throw new ArgumentException(
                "Foundation token names must be lowercase identifiers using letters, numbers, and hyphens.",
                nameof(name));
        }
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

    public NerdDesignTokenOptions SetThemeSet(string id, NerdThemeSet themeSet)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(themeSet);
        _themeSets[id] = themeSet;
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
        UsePaletteFirstAdapter = source.UsePaletteFirstAdapter;
        UseIntentPseudoCssThemes = source.UseIntentPseudoCssThemes;
        EmitFrameworkNeutralIntents = source.EmitFrameworkNeutralIntents;
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
        _spacing.Clear();
        _breakpoints.Clear();
        _motionDurations.Clear();
        _motionEasings.Clear();
        _zIndex.Clear();
        _transforms.Clear();
        _recipes.Clear();
        _opacities.Clear();
        _themeSets.Clear();
        Prefix = source.Prefix;
        ActiveBrandPackId = source.ActiveBrandPackId;
        ActiveBrandIdentityVersion = source.ActiveBrandIdentityVersion;
        PairingPolicy = source.PairingPolicy;
        LockedTokens = [..source.LockedTokens];
        Shell = source.Shell;
        FrameworkDefaults = source.FrameworkDefaults;

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

        foreach (var pair in source.Spacing)
        {
            AddSpacing(pair.Key, pair.Value);
        }

        foreach (var pair in source.Breakpoints)
        {
            AddBreakpoint(pair.Key, pair.Value);
        }

        foreach (var pair in source.MotionDurations)
        {
            AddMotionDuration(pair.Key, pair.Value);
        }

        foreach (var pair in source.MotionEasings)
        {
            AddMotionEasing(pair.Key, pair.Value);
        }

        foreach (var pair in source.ZIndex)
        {
            AddZIndex(pair.Key, pair.Value);
        }

        foreach (var pair in source.Transforms)
        {
            AddTransform(pair.Key, pair.Value);
        }

        foreach (var pair in source.Recipes)
        {
            AddRecipe(pair.Key, pair.Value);
        }

        foreach (var pair in source.Opacities)
        {
            AddOpacity(pair.Key, pair.Value);
        }

        foreach (var pair in source.ThemeSets)
        {
            SetThemeSet(pair.Key, pair.Value);
        }
    }
}
