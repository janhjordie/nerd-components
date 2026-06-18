using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Utilities;
using TheNerdCollective.Blazor.ThemeKit;

namespace TheNerdCollective.MudComponents.ThemeKit;

public partial class MudThemeTokenPreview : IDisposable
{
    [Parameter]
    public bool Compact { get; set; }

    [Inject]
    private IMudThemeStateService ThemeState { get; set; } = null!;

    private IReadOnlyList<ThemeTokenChange> _changes = [];

    protected override void OnInitialized()
    {
        ThemeState.Changed += HandleThemeChanged;
        RefreshChanges();
    }

    private void HandleThemeChanged()
    {
        RefreshChanges();
        InvokeAsync(StateHasChanged);
    }

    private void RefreshChanges()
        => _changes = ThemeState.GetModifiedTokens();

    private static string Truncate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "—";
        }

        return value.Length <= 28 ? value : $"{value[..25]}…";
    }

    private Palette GetActivePalette()
        => ThemeState.IsDarkMode ? ThemeState.CurrentTheme.PaletteDark : ThemeState.CurrentTheme.PaletteLight;

    private string GetSurfacePreviewStyle()
    {
        var palette = GetActivePalette();
        return $"background-color: {ToHex(palette.Background)}; padding: 0.75rem; border-radius: var(--mud-default-borderradius);";
    }

    private string GetCardOnSurfaceStyle()
    {
        var palette = GetActivePalette();
        return $"background-color: {ToHex(palette.Surface)}; color: {ToHex(palette.TextPrimary)};";
    }

    private IReadOnlyList<ColorPairPreview> GetColorPairs()
    {
        var theme = ThemeState.CurrentTheme;
        return
        [
            CreatePair("Light · Primary", theme.PaletteLight.Primary, theme.PaletteLight.PrimaryContrastText),
            CreatePair("Light · Secondary", theme.PaletteLight.Secondary, theme.PaletteLight.SecondaryContrastText),
            CreatePair("Light · Tertiary", theme.PaletteLight.Tertiary, theme.PaletteLight.TertiaryContrastText),
            CreatePair("Dark · Primary", theme.PaletteDark.Primary, theme.PaletteDark.PrimaryContrastText),
            CreatePair("Dark · Secondary", theme.PaletteDark.Secondary, theme.PaletteDark.SecondaryContrastText),
            CreatePair("Dark · Tertiary", theme.PaletteDark.Tertiary, theme.PaletteDark.TertiaryContrastText),
        ];
    }

    private static ColorPairPreview CreatePair(string label, MudColor background, MudColor foreground)
        => new(label, ToHex(background), ToHex(foreground));

    private static string ToHex(MudColor color) => color.ToString(MudColorOutputFormats.Hex);

    public void Dispose() => ThemeState.Changed -= HandleThemeChanged;

    private sealed record ColorPairPreview(string Label, string BackgroundHex, string ForegroundHex)
    {
        public string Style =>
            $"background-color: {BackgroundHex}; color: {ForegroundHex}; padding: 0.5rem 0.75rem; " +
            "border-radius: var(--mud-default-borderradius); font-weight: 500;";
    }
}
