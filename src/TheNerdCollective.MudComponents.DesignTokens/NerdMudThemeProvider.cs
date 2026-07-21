using System.Text;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Single <see cref="MudThemeProvider"/> emitting brand :root + intent/recipe PseudoCss scopes (HR-170).
/// </summary>
public partial class NerdMudThemeProvider : MudThemeProvider
{
    private bool _isDark;

    [Parameter]
    public NerdDesignTokenOptions? DesignTokenOptions { get; set; }

    /// <summary>
    /// Inactive brand pack ids to emit PseudoCss preview scopes for (e.g. tnc + dnf side-by-side).
    /// </summary>
    [Parameter]
    public IReadOnlyList<string>? PreviewBrandPackIds { get; set; }

    private bool IsDark => _isDark;

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        if (parameters.TryGetValue<bool>(nameof(IsDarkMode), out var isDark))
        {
            _isDark = isDark;
        }

        await base.SetParametersAsync(parameters);
    }

    protected string BuildNerdScopedTheme()
    {
        var themeStringBuilder = new StringBuilder();
        themeStringBuilder.AppendLine("<style class=\"mud-theme-provider\">");
        themeStringBuilder.AppendLine(":root {");
        GenerateTheme(themeStringBuilder);
        if (DesignTokenOptions is not null)
        {
            NerdMudRootTokenVariables.Append(themeStringBuilder, DesignTokenOptions);
        }
        themeStringBuilder.AppendLine("}");

        if (DesignTokenOptions?.UseIntentPseudoCssThemes == true)
        {
            AppendIntentScopes(themeStringBuilder);
            AppendRecipeScopes(themeStringBuilder);

            if (PreviewBrandPackIds is { Count: > 0 })
            {
                NerdMudPreviewThemeEmitter.AppendPreviewScopes(
                    themeStringBuilder,
                    PreviewBrandPackIds,
                    DesignTokenOptions.Prefix,
                    Theme,
                    IsDark);
            }
        }

        themeStringBuilder.AppendLine("</style>");
        return themeStringBuilder.ToString();
    }

    private void AppendIntentScopes(StringBuilder themeStringBuilder)
    {
        NerdMudPreviewThemeEmitter.AppendIntentScopes(
            themeStringBuilder,
            DesignTokenOptions!,
            Theme,
            IsDark);
    }

    private void AppendRecipeScopes(StringBuilder themeStringBuilder)
    {
        NerdMudPreviewThemeEmitter.AppendRecipeScopes(
            themeStringBuilder,
            DesignTokenOptions!,
            Theme,
            IsDark);
    }
}
