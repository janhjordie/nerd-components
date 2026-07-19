using System.Text;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Emits semantic alias nerd color variables on <c>:root</c> so bridges (tabs, inputs) resolve everywhere.
/// </summary>
internal static class NerdMudRootTokenVariables
{
    public static void Append(StringBuilder css, NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        foreach (var alias in options.Aliases.Keys.OrderBy(name => name, StringComparer.OrdinalIgnoreCase))
        {
            if (!TryResolveAliasToken(options, alias, out var token))
            {
                continue;
            }

            var prefix = options.Prefix;
            var light = NerdColorValue.Validate(token.Light ?? token.Value, nameof(token.Value));
            var contrast = NerdColorValue.Validate(
                token.ContrastText ?? NerdColorValue.ContrastText(light),
                nameof(token.ContrastText));
            var content = token.Content ?? NerdColorParser.ContentText(light, contrast);
            var surface = token.Surface ?? light;

            css.AppendLine($"  --{prefix}-color-{alias}: {light};");
            css.AppendLine($"  --{prefix}-color-{alias}-text: {contrast};");
            css.AppendLine($"  --{prefix}-color-{alias}-content: {content};");
            css.AppendLine($"  --{prefix}-color-{alias}-surface: {surface};");
        }
    }

    private static bool TryResolveAliasToken(NerdDesignTokenOptions options, string alias, out NerdColorToken token)
    {
        token = null!;
        if (!options.Aliases.TryGetValue(alias, out var target))
        {
            return false;
        }

        var current = target;
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (options.Aliases.TryGetValue(current, out var next))
        {
            if (!visited.Add(current))
            {
                break;
            }

            current = next;
        }

        return options.Colors.TryGetValue(current, out token!);
    }
}
