# TheNerdCollective.MudComponents.DesignTokens

Customer-specific CSS design tokens for MudBlazor 9.6. Define meaningful
colors such as `sand`, `forest`, `sun`, and `sea`, then use the generated
classes directly on any MudBlazor component. No wrappers, JavaScript, or
MudBlazor fork is required.

## Setup

```csharp
builder.Services.AddNerdDesignTokens(options =>
{
    options.Prefix = "dnf";
    options.Add("sand", new NerdColorToken
    {
        Value = "#E8D8AD",
        ContrastText = "#2D2D2D",
        Hover = "#D8C58E"
    });
    options.Add("forest", new NerdColorToken
    {
        Value = "#365C3A",
        ContrastText = "#FFFFFF",
        Hover = "#2D4D30"
    });
});
```

Add the style component once, after `MudThemeProvider`:

```razor
<MudThemeProvider />
<NerdDesignTokenStyles />
```

Use the customer vocabulary in markup:

```razor
<MudGrid Class="dnf-forest">
    <MudText Class="dnf-sand">Nature first</MudText>
    <MudButton Class="dnf-forest">Read more</MudButton>
</MudGrid>
```

The generator emits CSS variables and selectors for MudBlazor filled,
outlined, and text buttons, chips, alerts, badges, progress bars, icon
buttons, links, typography, hover, focus, active, and disabled states.
Generic container classes also set inherited foreground and background colors,
so the same token can be applied to `MudGrid`, `MudPaper`, `MudCard`, and
other components.

Token names must be lowercase CSS identifiers, such as `sand`,
`forest-dark`, or `sea-2`. Each application can define a different set of
tokens and a different prefix.
