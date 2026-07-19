using Microsoft.AspNetCore.Components;

namespace TheNerdCollective.MudComponents.DesignTokens;

internal static class NerdDesignTokenCatalogRendering
{
    public static string FormatRatio(double ratio) => $"{ratio:0.0}:1";

    public static RenderFragment ColorValue(string? value) => builder =>
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            builder.AddContent(0, "–");
            return;
        }

        builder.OpenElement(1, "span");
        builder.AddAttribute(
            2,
            "style",
            "display:inline-flex;align-items:center;gap:.5rem;background:transparent;");
        builder.OpenElement(3, "span");
        builder.AddAttribute(4, "class", "nerd-token-color-tile");
        builder.AddAttribute(
            5,
            "style",
            "display:inline-block;width:1.1rem;height:1.1rem;border-radius:3px;border:1px solid rgba(0,0,0,.35);box-shadow:inset 0 0 0 1px rgba(255,255,255,.4);background-color:" +
            value + ";");
        builder.AddAttribute(6, "title", value);
        builder.CloseElement();
        builder.OpenElement(7, "code");
        builder.AddContent(8, value);
        builder.CloseElement();
        builder.CloseElement();
    };
}
