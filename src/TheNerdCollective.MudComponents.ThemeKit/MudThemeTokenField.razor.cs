using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Utilities;
using TheNerdCollective.Blazor.ThemeKit;

namespace TheNerdCollective.MudComponents.ThemeKit;

public partial class MudThemeTokenField
{
    private static readonly MudColor FallbackColor = new("#000000");

    [Parameter, EditorRequired]
    public ThemeTokenDefinition Token { get; set; } = null!;

    [Inject]
    private IMudThemeStateService ThemeState { get; set; } = null!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    private bool IsModified => ThemeState.IsTokenModified(Token.Id);

    private string? GetValue() => ThemeState.GetToken(Token.Id) ?? string.Empty;

    private void SetValue(string value) => ThemeState.SetToken(Token.Id, value);

    private void ResetToken()
    {
        ThemeState.ResetToken(Token.Id);
        Snackbar.Add($"{Token.Label} nulstillet", Severity.Info, config => config.VisibleStateDuration = 1500);
    }

    private string GetCatalogDisplayValue()
        => ThemeState.GetCatalogToken(Token.Id) ?? "—";

    private MudColor GetColor()
    {
        var value = GetValue();
        return string.IsNullOrWhiteSpace(value) || !MudColor.TryParse(value, out var color)
            ? FallbackColor
            : color;
    }

    private void SetColor(MudColor color)
        => SetValue(color.ToString(MudColorOutputFormats.Hex));

    private string GetRootClass()
        => IsModified ? "mud-theme-token-field mud-theme-token-field--modified pa-2 mb-1" : "mud-theme-token-field mb-1";

    private string GetSwatchStyle()
    {
        var color = GetColor();
        var modifiedRing = IsModified ? "outline: 2px solid var(--mud-palette-warning); outline-offset: 1px;" : string.Empty;
        return $"background-color: {color.ToString(MudColorOutputFormats.Hex)}; " +
               "width: 2.75rem; height: 2.75rem; border-radius: var(--mud-default-borderradius); " +
               "border: 1px solid var(--mud-palette-lines-default); flex-shrink: 0; margin-bottom: 4px; cursor: pointer; " +
               modifiedRing;
    }

    private async Task CopySwatchHexAsync()
    {
        var hex = GetColor().ToString(MudColorOutputFormats.Hex);
        await JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", hex);
        Snackbar.Add($"{Token.Label}: {hex}", Severity.Info, config => config.VisibleStateDuration = 1500);
    }
}
