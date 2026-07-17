namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public static class ResponsiveFontSize
{
    public static string Clamp(string minimum, string preferred, string maximum)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(minimum);
        ArgumentException.ThrowIfNullOrWhiteSpace(preferred);
        ArgumentException.ThrowIfNullOrWhiteSpace(maximum);

        return $"clamp({minimum}, {preferred}, {maximum})";
    }
}
