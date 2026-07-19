namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Resolves alias → token → hex chains for catalog/tree UI (HR-109).</summary>
public static class NerdAliasChainTools
{
    public static IReadOnlyList<NerdAliasChainStep> Build(NerdDesignTokenOptions options, string name)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var chain = new List<NerdAliasChainStep>();
        var current = name;
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (visited.Add(current))
        {
            if (options.Aliases.TryGetValue(current, out var target))
            {
                chain.Add(new NerdAliasChainStep(current, NerdAliasChainStepKind.Alias, target));
                current = target;
                continue;
            }

            if (options.Colors.TryGetValue(current, out var token))
            {
                chain.Add(new NerdAliasChainStep(current, NerdAliasChainStepKind.Color, token.Value ?? token.Light ?? "#000000"));
                break;
            }

            if (chain.Count == 0)
            {
                chain.Add(new NerdAliasChainStep(current, NerdAliasChainStepKind.Missing, null));
            }

            break;
        }

        return chain;
    }

    public static string Format(IReadOnlyList<NerdAliasChainStep> chain) =>
        chain.Count == 0
            ? string.Empty
            : string.Join(" → ", chain.Select(step => step.Kind == NerdAliasChainStepKind.Alias
                ? step.Name
                : $"{step.Name} ({step.Value})"));

    public static bool TryBuildForAlias(NerdDesignTokenOptions options, string aliasName, out IReadOnlyList<NerdAliasChainStep> chain)
    {
        chain = [];
        if (!options.IsAlias(aliasName))
        {
            return false;
        }

        chain = Build(options, aliasName);
        return chain.Count > 0;
    }
}

public enum NerdAliasChainStepKind
{
    Alias,
    Color,
    Missing
}

public sealed record NerdAliasChainStep(string Name, NerdAliasChainStepKind Kind, string? Value);
