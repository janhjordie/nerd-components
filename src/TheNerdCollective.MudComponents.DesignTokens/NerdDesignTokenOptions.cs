namespace TheNerdCollective.MudComponents.DesignTokens;

public sealed class NerdDesignTokenOptions
{
    private readonly Dictionary<string, NerdColorToken> _colors = new(StringComparer.OrdinalIgnoreCase);

    public string Prefix { get; set; } = "nerd";

    public IReadOnlyDictionary<string, NerdColorToken> Colors => _colors;

    public NerdDesignTokenOptions Add(string name, NerdColorToken token)
    {
        NerdTokenNameValidator.Validate(name);
        ArgumentNullException.ThrowIfNull(token);
        _colors[name] = token;
        return this;
    }
}
