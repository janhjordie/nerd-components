namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Theme set helpers for pack export and catalog preview (HR-104).</summary>
public static class NerdThemeSetTools
{
    public static IReadOnlyDictionary<string, NerdThemeSet> CreateFromOptions(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var light = new NerdThemeSet { Id = "light", DisplayName = "Light" };
        var dark = new NerdThemeSet { Id = "dark", DisplayName = "Dark" };

        foreach (var (name, token) in options.Colors)
        {
            var lightValue = token.Light ?? token.Value;
            if (!string.Equals(lightValue, token.Value, StringComparison.OrdinalIgnoreCase) ||
                !string.IsNullOrWhiteSpace(token.ContrastText))
            {
                light.Colors[name] = new NerdThemeSetColorToken
                {
                    Value = lightValue,
                    ContrastText = token.ContrastText,
                    Surface = token.Surface,
                    Content = token.Content
                };
            }

            if (!string.IsNullOrWhiteSpace(token.Dark))
            {
                dark.Colors[name] = new NerdThemeSetColorToken
                {
                    Value = token.Dark,
                    ContrastText = token.DarkContrastText ?? token.ContrastText,
                    Surface = token.Surface,
                    Content = token.Content
                };
            }
        }

        var sets = new Dictionary<string, NerdThemeSet>(StringComparer.OrdinalIgnoreCase);
        if (light.Colors.Count > 0)
        {
            sets[light.Id] = light;
        }

        if (dark.Colors.Count > 0)
        {
            sets[dark.Id] = dark;
        }

        return sets;
    }

    public static bool TryGetThemeSet(
        IReadOnlyDictionary<string, NerdThemeSet>? themeSets,
        string themeSetId,
        out NerdThemeSet themeSet)
    {
        if (themeSets is not null && themeSets.TryGetValue(themeSetId, out var found))
        {
            themeSet = found;
            return true;
        }

        themeSet = default!;
        return false;
    }

    public static NerdColorToken ResolveColor(
        string tokenName,
        NerdColorToken baseToken,
        string? themeSetId,
        IReadOnlyDictionary<string, NerdThemeSet>? themeSets)
    {
        ArgumentNullException.ThrowIfNull(baseToken);
        if (string.IsNullOrWhiteSpace(themeSetId) ||
            themeSets is null ||
            !themeSets.TryGetValue(themeSetId, out var set) ||
            !set.Colors.TryGetValue(tokenName, out var overrideToken))
        {
            return baseToken;
        }

        return Merge(baseToken, overrideToken, themeSetId);
    }

    /// <summary>
    /// Merges explicit pack theme sets onto color tokens before CSS generation (HR-104).
    /// </summary>
    public static void SyncColorTokensFromThemeSets(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.ThemeSets.Count == 0)
        {
            return;
        }

        foreach (var (name, baseToken) in options.Colors.ToList())
        {
            var merged = baseToken;
            if (options.ThemeSets.TryGetValue("light", out var lightSet) &&
                lightSet.Colors.TryGetValue(name, out var lightOverride))
            {
                merged = Merge(merged, lightOverride, "light");
            }

            if (options.ThemeSets.TryGetValue("dark", out var darkSet) &&
                darkSet.Colors.TryGetValue(name, out var darkOverride))
            {
                merged = Merge(merged, darkOverride, "dark");
            }

            if (!ReferenceEquals(merged, baseToken))
            {
                options.Add(name, merged);
            }
        }
    }

    public static void EnsureThemeSets(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.ThemeSets.Count == 0)
        {
            foreach (var (id, set) in CreateFromOptions(options))
            {
                options.SetThemeSet(id, set);
            }
        }
    }

    private static NerdColorToken Merge(NerdColorToken baseToken, NerdThemeSetColorToken overrideToken, string themeSetId)
    {
        if (string.Equals(themeSetId, "dark", StringComparison.OrdinalIgnoreCase))
        {
            return new NerdColorToken
            {
                Value = overrideToken.Value ?? overrideToken.Dark ?? baseToken.Dark ?? baseToken.Value,
                Light = baseToken.Light,
                Dark = overrideToken.Dark ?? overrideToken.Value ?? baseToken.Dark,
                ContrastText = baseToken.ContrastText,
                DarkContrastText = overrideToken.DarkContrastText ?? overrideToken.ContrastText ?? baseToken.DarkContrastText,
                Surface = overrideToken.Surface ?? baseToken.Surface,
                Content = overrideToken.Content ?? baseToken.Content,
                Interactive = baseToken.Interactive,
                Hover = baseToken.Hover,
                Active = baseToken.Active,
                Border = baseToken.Border,
                Disabled = baseToken.Disabled,
                Description = baseToken.Description,
                Roles = baseToken.Roles
            };
        }

        return new NerdColorToken
        {
            Value = overrideToken.Value ?? overrideToken.Light ?? baseToken.Value,
            Light = overrideToken.Light ?? overrideToken.Value ?? baseToken.Light,
            Dark = baseToken.Dark,
            ContrastText = overrideToken.ContrastText ?? baseToken.ContrastText,
            DarkContrastText = baseToken.DarkContrastText,
            Surface = overrideToken.Surface ?? baseToken.Surface,
            Content = overrideToken.Content ?? baseToken.Content,
            Interactive = baseToken.Interactive,
            Hover = baseToken.Hover,
            Active = baseToken.Active,
            Border = baseToken.Border,
            Disabled = baseToken.Disabled,
            Description = baseToken.Description,
            Roles = baseToken.Roles
        };
    }
}
