using System.Text.RegularExpressions;

namespace TheNerdCollective.MudComponents.DesignTokens;

internal static partial class NerdTokenNameValidator
{
    [GeneratedRegex("^[a-z][a-z0-9]*(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex NamePattern();

    public static void Validate(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (!NamePattern().IsMatch(name))
        {
            throw new ArgumentException(
                "Token names must be lowercase CSS identifiers using letters, numbers, and hyphens.",
                nameof(name));
        }
    }
}
