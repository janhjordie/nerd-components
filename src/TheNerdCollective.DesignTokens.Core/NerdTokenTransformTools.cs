using System.Globalization;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Applies lighten/darken/alpha transforms to source color tokens (HR-103).</summary>
public static class NerdTokenTransformTools
{
    public static readonly IReadOnlySet<string> SupportedOperations =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "lighten", "darken", "alpha" };

    public static string Apply(string sourceColor, string operation, double amount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceColor);
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);

        return operation.Trim().ToLowerInvariant() switch
        {
            "lighten" => NerdColorDerivatives.Lighten(sourceColor, amount),
            "darken" => NerdColorDerivatives.Darken(sourceColor, amount),
            "alpha" => $"color-mix(in srgb, {sourceColor} {FormatPercent(amount)}, transparent)",
            _ => throw new ArgumentException($"Unsupported transform operation '{operation}'.", nameof(operation))
        };
    }

    public static void ApplyTransforms(
        NerdDesignTokenOptions options,
        IReadOnlyDictionary<string, NerdTokenTransform> transforms,
        bool overwrite = false)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(transforms);

        foreach (var (name, transform) in transforms.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            transform.Validate();
            if (!overwrite && options.Colors.ContainsKey(name))
            {
                continue;
            }

            var sourceColor = ResolveSourceColor(options, transform.Source);
            var value = Apply(sourceColor, transform.Operation, transform.Amount);
            options.Add(name, new NerdColorToken { Value = value });
        }
    }

    private static string ResolveSourceColor(NerdDesignTokenOptions options, string source)
    {
        if (options.Colors.TryGetValue(source, out var token))
        {
            return token.Value ?? token.Light ?? throw new InvalidOperationException($"Color token '{source}' has no value.");
        }

        if (options.Aliases.TryGetValue(source, out var aliasTarget) &&
            options.Colors.TryGetValue(aliasTarget, out var aliasToken))
        {
            return aliasToken.Value ?? aliasToken.Light ?? throw new InvalidOperationException($"Alias target '{aliasTarget}' has no value.");
        }

        throw new InvalidOperationException($"Transform source '{source}' was not found in colors or aliases.");
    }

    private static string FormatPercent(double amount) =>
        $"{Math.Round(amount * 100, 2).ToString(CultureInfo.InvariantCulture)}%";
}
