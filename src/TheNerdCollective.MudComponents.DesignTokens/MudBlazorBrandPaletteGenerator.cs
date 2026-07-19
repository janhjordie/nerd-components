using System.Text;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

internal static class MudBlazorBrandPaletteGenerator
{
    /// <summary>
    /// Global Mud palette is owned by <see cref="NerdMudThemeFactory"/> via <see cref="MudBlazor.MudThemeProvider"/> (HR-158).
    /// </summary>
    public static void AppendBrandRootPalette(StringBuilder css, NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _ = css;
    }

    public static void AppendIntentPaletteOverrides(
        StringBuilder css,
        string root,
        string aliasName,
        string variable,
        string textVariable,
        string hoverVariable,
        string contentVariable,
        string borderVariable,
        string importantSuffix)
    {
        css.AppendLine($"{root} {{");
        AppendIntentChannelSlots(css, aliasName, variable, textVariable, hoverVariable, contentVariable, borderVariable, importantSuffix);
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-switch-base, {root}.mud-switch-base {{");
        css.AppendLine($"  color: var({textVariable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine();
    }

    private static void AppendIntentChannelSlots(
        StringBuilder css,
        string aliasName,
        string variable,
        string textVariable,
        string hoverVariable,
        string contentVariable,
        string borderVariable,
        string importantSuffix)
    {
        if (string.Equals(aliasName, NerdDesignSystemUi.SecondaryAction, StringComparison.OrdinalIgnoreCase))
        {
            css.AppendLine($"  --mud-palette-secondary: var({variable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-secondary-text: var({textVariable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-secondary-hover: var({hoverVariable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-action-default: var({variable}){importantSuffix};");
            return;
        }

        if (string.Equals(aliasName, NerdDesignSystemUi.Highlight, StringComparison.OrdinalIgnoreCase))
        {
            css.AppendLine($"  --mud-palette-warning: var({variable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-warning-text: var({textVariable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-warning-hover: var({hoverVariable}){importantSuffix};");
            return;
        }

        if (string.Equals(aliasName, NerdDesignSystemUi.Info, StringComparison.OrdinalIgnoreCase))
        {
            css.AppendLine($"  --mud-palette-info: var({variable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-info-text: var({textVariable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-info-hover: var({hoverVariable}){importantSuffix};");
            return;
        }

        if (string.Equals(aliasName, NerdDesignSystemUi.Success, StringComparison.OrdinalIgnoreCase))
        {
            css.AppendLine($"  --mud-palette-success: var({variable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-success-text: var({textVariable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-success-hover: var({hoverVariable}){importantSuffix};");
            return;
        }

        if (string.Equals(aliasName, NerdDesignSystemUi.Danger, StringComparison.OrdinalIgnoreCase))
        {
            css.AppendLine($"  --mud-palette-error: var({variable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-error-text: var({textVariable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-error-hover: var({hoverVariable}){importantSuffix};");
            return;
        }

        if (string.Equals(aliasName, NerdDesignSystemUi.NavItem, StringComparison.OrdinalIgnoreCase))
        {
            css.AppendLine($"  --mud-palette-drawer-text: var({contentVariable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-drawer-icon: var({contentVariable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-action-default: var({variable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-text-secondary: var({contentVariable}){importantSuffix};");
            return;
        }

        if (string.Equals(aliasName, NerdDesignSystemUi.NavItemActive, StringComparison.OrdinalIgnoreCase))
        {
            css.AppendLine($"  --mud-palette-primary: var({variable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-primary-text: var({textVariable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-action-default: var({variable}){importantSuffix};");
            return;
        }

        if (string.Equals(aliasName, NerdDesignSystemUi.MutedContent, StringComparison.OrdinalIgnoreCase))
        {
            css.AppendLine($"  --mud-palette-text-secondary: var({contentVariable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-text-primary: var({contentVariable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-action-default: var({variable}){importantSuffix};");
            return;
        }

        if (string.Equals(aliasName, NerdDesignSystemUi.OnBrandChrome, StringComparison.OrdinalIgnoreCase))
        {
            css.AppendLine($"  --mud-palette-appbar-text: var({textVariable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-text-primary: var({textVariable}){importantSuffix};");
            return;
        }

        if (string.Equals(aliasName, NerdDesignSystemUi.OnPrimaryAction, StringComparison.OrdinalIgnoreCase))
        {
            css.AppendLine($"  --mud-palette-primary-text: var({textVariable}){importantSuffix};");
            return;
        }

        if (string.Equals(aliasName, NerdDesignSystemUi.FocusRing, StringComparison.OrdinalIgnoreCase))
        {
            css.AppendLine($"  --mud-palette-primary: var({variable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-lines-default: var({borderVariable}){importantSuffix};");
            return;
        }

        if (string.Equals(aliasName, NerdDesignSystemUi.InputBorder, StringComparison.OrdinalIgnoreCase))
        {
            css.AppendLine($"  --mud-palette-lines-default: var({borderVariable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-lines-inputs: var({borderVariable}){importantSuffix};");
            return;
        }

        css.AppendLine($"  --mud-palette-primary: var({variable}){importantSuffix};");
        css.AppendLine($"  --mud-palette-primary-text: var({textVariable}){importantSuffix};");
        css.AppendLine($"  --mud-palette-primary-hover: var({hoverVariable}){importantSuffix};");
        css.AppendLine($"  --mud-palette-action-default: var({variable}){importantSuffix};");
        css.AppendLine($"  --mud-palette-action-default-hover: color-mix(in srgb, var({variable}) 8%, transparent){importantSuffix};");
        css.AppendLine($"  --mud-palette-text-primary: var({contentVariable}){importantSuffix};");
        css.AppendLine($"  --mud-palette-lines-default: var({borderVariable}){importantSuffix};");
        css.AppendLine($"  --mud-palette-lines-inputs: var({borderVariable}){importantSuffix};");
    }

}
