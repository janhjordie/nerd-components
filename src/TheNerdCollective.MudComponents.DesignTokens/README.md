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
        Light = "#4D7A50",
        Dark = "#203B25",
        Hover = "#2D4D30",
        Surface = "#F0F7F0",
        Content = "#19301D",
        Interactive = "#2D4D30"
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

The generator emits the MudBlazor palette variables used by all components
that consume the theme variables, so the token works generically on
`MudGrid`, `MudPaper`, `MudCard`, and other components. It also emits explicit
selectors for filled, outlined, and text buttons, chips, alerts, badges,
progress bars, icon buttons, links, typography, hover, focus, active, and
disabled states. This covers both inherited theme behavior and component
variants where MudBlazor uses a more specific selector.

Token names must be lowercase CSS identifiers, such as `sand`,
`forest-dark`, or `sea-2`. Each application can define a different set of
tokens and a different prefix.

`ContrastText` is optional for hex colors and is calculated automatically when
omitted. `Light` and `Dark` provide mode-specific values; dark values are
activated below an ancestor with `data-theme="dark"`. `Surface`, `Content`,
and `Interactive` are semantic roles that can be consumed by application CSS
without changing the token's component selectors.

The generated selectors are intentionally versioned against MudBlazor 9.6.
The package emits both MudBlazor palette variables and explicit selectors for
component variants and states, so updates to MudBlazor can be checked with the
CSS snapshot tests before changing the package dependency.
