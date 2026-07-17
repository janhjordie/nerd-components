namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public static class ResponsiveTypographyOptionsExtensions
{
    public static void CopyTo(this ResponsiveTypographyOptions source, ResponsiveTypographyOptions target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        target.Default = source.Default;
        target.H1 = source.H1;
        target.H2 = source.H2;
        target.H3 = source.H3;
        target.H4 = source.H4;
        target.H5 = source.H5;
        target.H6 = source.H6;
        target.Subtitle1 = source.Subtitle1;
        target.Subtitle2 = source.Subtitle2;
        target.Body1 = source.Body1;
        target.Body2 = source.Body2;
        target.Button = source.Button;
        target.Caption = source.Caption;
        target.Overline = source.Overline;
        target.LineHeight = source.LineHeight;
        target.LetterSpacing = source.LetterSpacing;
        target.FontWeight = source.FontWeight;
    }
}
