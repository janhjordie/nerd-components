namespace TheNerdCollective.Blazor.ThemeKit;

public sealed record ThemeTokenEditorRow(params string[] TokenIds);

public sealed record ThemeTokenEditorSection(string Title, params ThemeTokenEditorRow[] Rows);

public sealed record ThemeTokenEditorGroup(string Title, params ThemeTokenEditorSection[] Sections);

/// <summary>
/// Semantic layout for the theme token editor — related tokens (e.g. primary + contrast) share a row.
/// </summary>
public static class ThemeTokenEditorLayout
{
    public static IReadOnlyList<ThemeTokenEditorGroup> V1 { get; } =
    [
        new("Palette (light)",
        [
            new("Brand",
            [
                new("light.primary", "light.primaryContrastText"),
                new("light.secondary", "light.secondaryContrastText"),
                new("light.tertiary", "light.tertiaryContrastText")
            ]),
            new("Page surface",
            [
                new("light.background", "light.surface")
            ]),
            new("App bar",
            [
                new("light.appbarBackground", "light.appbarText")
            ]),
            new("Drawer",
            [
                new("light.drawerBackground", "light.drawerText")
            ]),
            new("Text",
            [
                new("light.textPrimary")
            ])
        ]),
        new("Palette (dark)",
        [
            new("Brand",
            [
                new("dark.primary", "dark.primaryContrastText"),
                new("dark.secondary", "dark.secondaryContrastText"),
                new("dark.tertiary", "dark.tertiaryContrastText")
            ]),
            new("Page surface",
            [
                new("dark.background", "dark.surface")
            ]),
            new("App bar",
            [
                new("dark.appbarBackground", "dark.appbarText")
            ]),
            new("Drawer",
            [
                new("dark.drawerBackground", "dark.drawerText")
            ]),
            new("Text & actions",
            [
                new("dark.textPrimary", "dark.textSecondary"),
                new("dark.actionDefault", "dark.actionDisabled")
            ])
        ]),
        new("Layout",
        [
            new("",
            [
                new("layout.defaultBorderRadius")
            ])
        ]),
        new("Typography",
        [
            new("",
            [
                new("typography.defaultFontFamily")
            ])
        ])
    ];

    public static ThemeTokenDefinition? FindToken(string id)
        => ThemeTokenRegistry.V1.FirstOrDefault(t => t.Id == id);

    public static int GetGridColumns(int tokensInRow) => tokensInRow switch
    {
        1 => 6,
        2 => 6,
        _ => 4
    };
}
