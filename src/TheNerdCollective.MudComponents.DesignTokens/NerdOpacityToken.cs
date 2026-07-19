namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Semi-transparent overlay token referencing a base color token.
/// </summary>
public sealed record NerdOpacityToken(string BaseToken, double Opacity)
{
    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(BaseToken);
        if (Opacity is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(Opacity), Opacity, "Opacity must be between 0 and 1.");
        }
    }
}
