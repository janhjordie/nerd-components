namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public static class ResponsiveFontSize
{
    public static string Clamp(string minimum, string preferred, string maximum)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(minimum);
        ArgumentException.ThrowIfNullOrWhiteSpace(preferred);
        ArgumentException.ThrowIfNullOrWhiteSpace(maximum);
        Validate(minimum, nameof(minimum));
        Validate(preferred, nameof(preferred));
        Validate(maximum, nameof(maximum));

        return $"clamp({minimum.Trim()}, {preferred.Trim()}, {maximum.Trim()})";
    }

    private static void Validate(string value, string parameterName)
    {
        if (value.Contains(',', StringComparison.Ordinal))
        {
            throw new ArgumentException("A clamp argument cannot contain a comma.", parameterName);
        }
    }
}
