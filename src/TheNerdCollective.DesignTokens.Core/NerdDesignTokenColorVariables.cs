namespace TheNerdCollective.MudComponents.DesignTokens;

public static class NerdDesignTokenColorVariables
{
    public static Dictionary<string, string> Build(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var pair in options.Colors)
        {
            var token = pair.Value;
            var light = token.Light ?? token.Value;
            var dark = token.Dark ?? light;
            var prefix = options.Prefix;
            var root = $"--{prefix}-color-{pair.Key}";

            variables[root] = light;
            variables[$"{root}-text"] = token.ContrastText ?? NerdColorValue.ContrastText(light);
            variables[$"{root}-dark"] = dark;
            variables[$"{root}-dark-text"] =
                token.DarkContrastText ?? token.ContrastText ?? NerdColorValue.ContrastText(dark);
        }

        return variables;
    }
}
