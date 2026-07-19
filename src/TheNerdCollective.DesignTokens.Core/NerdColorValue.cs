using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

public static class NerdColorValue
{
    public static string Validate(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Contains(';') || value.Contains('{') || value.Contains('}') ||
            value.Contains('<') || value.Contains('>'))
        {
            throw new ArgumentException("Color values must be valid CSS values without declarations or markup.", parameterName);
        }

        return value.Trim();
    }

    public static string ContrastText(string value) => NerdColorParser.ContrastText(value);
}
