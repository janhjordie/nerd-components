namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public static class ResponsiveFontSize
{
    /// <summary>
    /// Creates a CSS <c>clamp()</c> expression for a responsive font size.
    /// </summary>
    /// <param name="minimum">The smallest CSS font-size value.</param>
    /// <param name="preferred">The fluid preferred CSS font-size value.</param>
    /// <param name="maximum">The largest CSS font-size value.</param>
    /// <returns>A CSS <c>clamp(minimum, preferred, maximum)</c> expression.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when an argument is empty, whitespace, or contains a comma.
    /// </exception>
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
