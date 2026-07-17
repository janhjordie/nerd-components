# TheNerdCollective.MudComponents.ResponsiveTypography

CSS-native responsive font sizes for MudBlazor 9.6 themes. The package uses
`clamp()` through `MudTheme.Typography`, so it needs no JavaScript, viewport
service, breakpoint detection, or resize-triggered renders.

```csharp
using MudBlazor;
using TheNerdCollective.MudComponents.ResponsiveTypography;

var theme = new MudTheme();

theme.UseResponsiveTypography(options =>
{
    options.H1 = ResponsiveFontSize.Clamp("2rem", "4vw", "4rem");
    options.H3 = ResponsiveFontSize.Clamp("1.75rem", "3vw", "2.5rem");
});
```

Only configured roles are changed. Existing values for omitted roles remain
unchanged, and normal `MudText` `Typo` semantics continue to apply.
