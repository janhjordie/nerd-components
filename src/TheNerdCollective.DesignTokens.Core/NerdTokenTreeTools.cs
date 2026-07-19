namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Builds hierarchical token trees for catalog/workbook navigation (HR-099).</summary>
public static class NerdTokenTreeTools
{
    public static IReadOnlyList<NerdTokenTreeNode> Build(
        NerdDesignTokenOptions options,
        IReadOnlyDictionary<string, NerdThemeSet>? themeSets = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        var nodes = new List<NerdTokenTreeNode>();

        if (options.Colors.Count > 0)
        {
            nodes.Add(CreateGroup(
                "colors",
                "Colors",
                options.Colors
                    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair => new NerdTokenTreeNode
                    {
                        Id = $"color:{pair.Key}",
                        Label = pair.Key,
                        Kind = NerdTokenTreeTargetKind.Color,
                        TargetId = pair.Key,
                        Detail = pair.Value.Light ?? pair.Value.Value,
                        AnchorId = $"token-color-{pair.Key}"
                    })
                    .ToList()));
        }

        if (options.Aliases.Count > 0)
        {
            nodes.Add(CreateGroup(
                "aliases",
                "Semantic aliases",
                options.Aliases
                    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair =>
                    {
                        var chain = NerdAliasChainTools.Build(options, pair.Key);
                        return new NerdTokenTreeNode
                        {
                            Id = $"alias:{pair.Key}",
                            Label = pair.Key,
                            Kind = NerdTokenTreeTargetKind.Alias,
                            TargetId = pair.Key,
                            Detail = NerdAliasChainTools.Format(chain),
                            AnchorId = $"token-alias-{pair.Key}"
                        };
                    })
                    .ToList()));
        }

        if (options.Spacing.Count > 0)
        {
            nodes.Add(CreateGroup(
                "spacing",
                "Spacing",
                options.Spacing
                    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair => new NerdTokenTreeNode
                    {
                        Id = $"spacing:{pair.Key}",
                        Label = pair.Key,
                        Kind = NerdTokenTreeTargetKind.Spacing,
                        TargetId = pair.Key,
                        Detail = pair.Value,
                        AnchorId = $"token-spacing-{pair.Key}"
                    })
                    .ToList()));
        }

        if (options.Radii.Count > 0)
        {
            nodes.Add(CreateGroup(
                "radii",
                "Radii",
                options.Radii
                    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair => new NerdTokenTreeNode
                    {
                        Id = $"radius:{pair.Key}",
                        Label = pair.Key,
                        Kind = NerdTokenTreeTargetKind.Radius,
                        TargetId = pair.Key,
                        Detail = pair.Value,
                        AnchorId = $"token-radius-{pair.Key}"
                    })
                    .ToList()));
        }

        if (options.Shadows.Count > 0)
        {
            nodes.Add(CreateGroup(
                "shadows",
                "Shadows",
                options.Shadows
                    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair => new NerdTokenTreeNode
                    {
                        Id = $"shadow:{pair.Key}",
                        Label = pair.Key,
                        Kind = NerdTokenTreeTargetKind.Shadow,
                        TargetId = pair.Key,
                        Detail = pair.Value,
                        AnchorId = $"token-shadow-{pair.Key}"
                    })
                    .ToList()));
        }

        if (options.Opacities.Count > 0)
        {
            nodes.Add(CreateGroup(
                "opacities",
                "Opacities",
                options.Opacities
                    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair => new NerdTokenTreeNode
                    {
                        Id = $"opacity:{pair.Key}",
                        Label = pair.Key,
                        Kind = NerdTokenTreeTargetKind.Opacity,
                        TargetId = pair.Key,
                        Detail = pair.Value.Opacity.ToString("0.##"),
                        AnchorId = $"token-opacity-{pair.Key}"
                    })
                    .ToList()));
        }

        if (options.Recipes.Count > 0)
        {
            nodes.Add(CreateGroup(
                "recipes",
                "Recipes",
                options.Recipes
                    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair => CreateRecipeNode(pair.Key, pair.Value))
                    .ToList()));
        }

        if (options.Breakpoints.Count > 0)
        {
            nodes.Add(CreateFoundationGroup(
                "breakpoints",
                "Breakpoints",
                options.Breakpoints,
                NerdTokenTreeTargetKind.Breakpoint,
                "breakpoint"));
        }

        if (options.MotionDurations.Count > 0)
        {
            nodes.Add(CreateFoundationGroup(
                "motion-durations",
                "Motion durations",
                options.MotionDurations,
                NerdTokenTreeTargetKind.MotionDuration,
                "duration"));
        }

        if (options.MotionEasings.Count > 0)
        {
            nodes.Add(CreateFoundationGroup(
                "motion-easings",
                "Motion easings",
                options.MotionEasings,
                NerdTokenTreeTargetKind.MotionEasing,
                "ease"));
        }

        if (options.ZIndex.Count > 0)
        {
            nodes.Add(CreateFoundationGroup(
                "z-index",
                "Z-index",
                options.ZIndex,
                NerdTokenTreeTargetKind.ZIndex,
                "z"));
        }

        var sets = themeSets is { Count: > 0 }
            ? themeSets
            : NerdThemeSetTools.CreateFromOptions(options);
        if (sets.Count > 0)
        {
            nodes.Add(new NerdTokenTreeNode
            {
                Id = "theme-sets",
                Label = "Theme sets",
                Kind = NerdTokenTreeTargetKind.Group,
                Children = sets
                    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair => CreateThemeSetNode(pair.Key, pair.Value))
                    .ToList()
            });
        }

        return nodes;
    }

    public static IReadOnlyList<NerdTokenTreeNode> Filter(
        IReadOnlyList<NerdTokenTreeNode> nodes,
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return nodes;
        }

        var query = search.Trim();
        return nodes
            .Select(node => FilterNode(node, query))
            .Where(node => node is not null)
            .Select(node => node!)
            .ToList();
    }

    public static NerdTokenTreeNavigation CreateNavigation(NerdTokenTreeNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (node.Kind == NerdTokenTreeTargetKind.Group || string.IsNullOrWhiteSpace(node.TargetId))
        {
            throw new ArgumentException($"Node '{node.Id}' is not navigable.", nameof(node));
        }

        return new NerdTokenTreeNavigation(node.Kind, node.TargetId, node.AnchorId);
    }

    private static NerdTokenTreeNode CreateGroup(string id, string label, IReadOnlyList<NerdTokenTreeNode> children) =>
        new()
        {
            Id = id,
            Label = label,
            Kind = NerdTokenTreeTargetKind.Group,
            Children = children
        };

    private static NerdTokenTreeNode CreateFoundationGroup(
        string id,
        string label,
        IReadOnlyDictionary<string, string> tokens,
        NerdTokenTreeTargetKind kind,
        string anchorPrefix) =>
        CreateGroup(
            id,
            label,
            tokens
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => new NerdTokenTreeNode
                {
                    Id = $"{id}:{pair.Key}",
                    Label = pair.Key,
                    Kind = kind,
                    TargetId = pair.Key,
                    Detail = pair.Value,
                    AnchorId = $"token-{anchorPrefix}-{pair.Key}"
                })
                .ToList());

    private static NerdTokenTreeNode CreateRecipeNode(string name, NerdDesignTokenRecipe recipe)
    {
        var children = new List<NerdTokenTreeNode>
        {
            CreateRecipeRoleNode(name, "surface", recipe.Surface),
            CreateRecipeRoleNode(name, "content", recipe.Content)
        };

        if (!string.IsNullOrWhiteSpace(recipe.Action))
        {
            children.Add(CreateRecipeRoleNode(name, "action", recipe.Action));
        }

        if (!string.IsNullOrWhiteSpace(recipe.Border))
        {
            children.Add(CreateRecipeRoleNode(name, "border", recipe.Border));
        }

        var layoutKitAnchor = NerdRecipePlayBookLinks.TryGetLayoutKitAnchor(name);
        return new NerdTokenTreeNode
        {
            Id = $"recipe:{name}",
            Label = name,
            Kind = NerdTokenTreeTargetKind.Recipe,
            TargetId = name,
            Detail = layoutKitAnchor is null
                ? $"{recipe.Surface} / {recipe.Content}"
                : $"PlayBook · {recipe.Surface} / {recipe.Content}",
            AnchorId = layoutKitAnchor ?? $"token-recipe-{name}",
            Children = children
        };
    }

    private static NerdTokenTreeNode CreateRecipeRoleNode(string recipeName, string role, string tokenName) =>
        new()
        {
            Id = $"recipe:{recipeName}:{role}",
            Label = role,
            Kind = NerdTokenTreeTargetKind.RecipeRole,
            TargetId = tokenName,
            Detail = $"→ {tokenName}",
            AnchorId = $"token-color-{tokenName}"
        };

    private static NerdTokenTreeNode CreateThemeSetNode(string id, NerdThemeSet set)
    {
        var children = set.Colors
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => new NerdTokenTreeNode
            {
                Id = $"theme:{id}:{pair.Key}",
                Label = pair.Key,
                Kind = NerdTokenTreeTargetKind.ThemeSetColor,
                TargetId = pair.Key,
                Detail = pair.Value.Value ?? pair.Value.Dark ?? pair.Value.Light,
                AnchorId = $"token-color-{pair.Key}"
            })
            .ToList();

        return new NerdTokenTreeNode
        {
            Id = $"theme-set:{id}",
            Label = set.DisplayName ?? id,
            Kind = NerdTokenTreeTargetKind.ThemeSet,
            TargetId = id,
            Detail = $"{children.Count} overrides",
            Children = children
        };
    }

    private static NerdTokenTreeNode? FilterNode(NerdTokenTreeNode node, string query)
    {
        if (node.Children.Count == 0)
        {
            return Matches(node, query) ? node : null;
        }

        var children = node.Children
            .Select(child => FilterNode(child, query))
            .Where(child => child is not null)
            .Select(child => child!)
            .ToList();

        if (children.Count == 0 && !Matches(node, query))
        {
            return null;
        }

        return new NerdTokenTreeNode
        {
            Id = node.Id,
            Label = node.Label,
            Kind = node.Kind,
            TargetId = node.TargetId,
            Detail = node.Detail,
            AnchorId = node.AnchorId,
            Children = children.Count > 0 ? children : node.Children.Where(child => Matches(child, query)).ToList()
        };
    }

    private static bool Matches(NerdTokenTreeNode node, string query) =>
        node.Label.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        (node.Detail?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
        (node.TargetId?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false);
}
