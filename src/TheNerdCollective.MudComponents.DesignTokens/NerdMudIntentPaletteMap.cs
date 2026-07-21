using System.Text;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Maps semantic intents to Mud palette slots (HR-165). Shared by CSS overrides and PseudoCss themes.
/// </summary>
public static class NerdMudIntentPaletteMap
{
    public static IReadOnlyList<string> GetIntentAliases(NerdDesignTokenOptions options) =>
        options.Aliases.Keys
            .Where(alias => IsPaletteFirstIntentAlias(alias) || IsSurfaceIntentAlias(alias))
            .OrderBy(alias => alias, StringComparer.OrdinalIgnoreCase)
            .ToList();

    public static string GetPseudoCssScope(NerdDesignTokenOptions options, string alias) =>
        $":root .{options.Prefix}-{alias}";

    public static NerdMudBrandPaletteMap ResolveIntentPaletteMap(
        NerdDesignTokenOptions options,
        string alias,
        NerdMudPaletteMode mode)
    {
        ArgumentNullException.ThrowIfNull(options);
        var brand = NerdMudBrandPaletteMap.Resolve(options, mode);
        if (!options.Aliases.ContainsKey(alias))
        {
            return brand;
        }

        var bundle = NerdMudBrandPaletteMap.ResolveAliasBundle(options, alias, mode);
        var map = ApplyIntentOverrides(brand, alias, bundle);
        return ApplyBrandChromeInputPaletteOverrides(options, alias, map, mode);
    }

    private static NerdMudBrandPaletteMap ApplyBrandChromeInputPaletteOverrides(
        NerdDesignTokenOptions options,
        string alias,
        NerdMudBrandPaletteMap map,
        NerdMudPaletteMode mode)
    {
        if (!string.Equals(alias, NerdDesignSystemUi.BrandChrome, StringComparison.OrdinalIgnoreCase) ||
            !options.Aliases.ContainsKey(NerdDesignSystemUi.OnBrandChrome))
        {
            return map;
        }

        var onChrome = NerdMudBrandPaletteMap.ResolveAliasBundle(options, NerdDesignSystemUi.OnBrandChrome, mode);
        return map with
        {
            TextPrimary = onChrome.Color,
            TextSecondary = onChrome.Color,
            LinesInputs = onChrome.Border
        };
    }

    public static void AppendIntentPaletteOverrides(
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
            css.AppendLine($"  --mud-palette-drawer-text: var({variable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-drawer-icon: var({variable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-action-default: var({variable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-text-secondary: var({variable}){importantSuffix};");
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
            css.AppendLine($"  --mud-palette-text-secondary: var({variable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-text-primary: var({variable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-action-default: var({variable}){importantSuffix};");
            return;
        }

        if (string.Equals(aliasName, NerdDesignSystemUi.OnBrandChrome, StringComparison.OrdinalIgnoreCase))
        {
            css.AppendLine($"  --mud-palette-appbar-text: var({variable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-text-primary: var({variable}){importantSuffix};");
            return;
        }

        if (string.Equals(aliasName, NerdDesignSystemUi.OnPrimaryAction, StringComparison.OrdinalIgnoreCase))
        {
            css.AppendLine($"  --mud-palette-primary-text: var({variable}){importantSuffix};");
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

        if (string.Equals(aliasName, NerdDesignSystemUi.PageSurface, StringComparison.OrdinalIgnoreCase))
        {
            css.AppendLine($"  --mud-palette-surface: var({variable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-background: var({variable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-text-primary: var({contentVariable}){importantSuffix};");
            return;
        }

        if (string.Equals(aliasName, NerdDesignSystemUi.BrandChrome, StringComparison.OrdinalIgnoreCase))
        {
            css.AppendLine($"  --mud-palette-appbar-background: var({variable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-appbar-text: var({textVariable}){importantSuffix};");
            var onBrandChromeVariable = variable.Replace(
                $"-color-{aliasName}",
                $"-color-{NerdDesignSystemUi.OnBrandChrome}",
                StringComparison.OrdinalIgnoreCase);
            css.AppendLine($"  --mud-palette-text-primary: var({onBrandChromeVariable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-text-secondary: var({onBrandChromeVariable}){importantSuffix};");
            css.AppendLine(
                $"  --mud-palette-lines-inputs: var({onBrandChromeVariable}-border, var({onBrandChromeVariable})){importantSuffix};");
            return;
        }

        if (string.Equals(aliasName, NerdDesignSystemUi.NavSurface, StringComparison.OrdinalIgnoreCase))
        {
            css.AppendLine($"  --mud-palette-drawer-background: var({variable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-drawer-text: var({contentVariable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-drawer-icon: var({contentVariable}){importantSuffix};");
            return;
        }

        if (string.Equals(aliasName, NerdDesignSystemUi.InputSurface, StringComparison.OrdinalIgnoreCase))
        {
            css.AppendLine($"  --mud-palette-surface: var({variable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-lines-inputs: var({borderVariable}){importantSuffix};");
            css.AppendLine($"  --mud-palette-text-primary: var({contentVariable}){importantSuffix};");
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

    private static NerdMudBrandPaletteMap ApplyIntentOverrides(
        NerdMudBrandPaletteMap brand,
        string alias,
        NerdMudAliasColorBundle bundle)
    {
        if (string.Equals(alias, NerdDesignSystemUi.SecondaryAction, StringComparison.OrdinalIgnoreCase))
        {
            return brand with
            {
                Secondary = bundle.Color,
                SecondaryText = bundle.Text,
                SecondaryHover = bundle.Hover,
                ActionDefault = bundle.Color
            };
        }

        if (string.Equals(alias, NerdDesignSystemUi.Highlight, StringComparison.OrdinalIgnoreCase))
        {
            return brand with
            {
                Warning = bundle.Color,
                WarningText = bundle.Text
            };
        }

        if (string.Equals(alias, NerdDesignSystemUi.Info, StringComparison.OrdinalIgnoreCase))
        {
            return brand with { Info = bundle.Color, InfoText = bundle.Text };
        }

        if (string.Equals(alias, NerdDesignSystemUi.Success, StringComparison.OrdinalIgnoreCase))
        {
            return brand with { Success = bundle.Color, SuccessText = bundle.Text };
        }

        if (string.Equals(alias, NerdDesignSystemUi.Danger, StringComparison.OrdinalIgnoreCase))
        {
            return brand with { Error = bundle.Color, ErrorText = bundle.Text };
        }

        if (string.Equals(alias, NerdDesignSystemUi.NavItem, StringComparison.OrdinalIgnoreCase))
        {
            return brand with
            {
                DrawerText = bundle.Color,
                DrawerIcon = bundle.Color,
                ActionDefault = bundle.Color,
                TextSecondary = bundle.Color
            };
        }

        if (string.Equals(alias, NerdDesignSystemUi.MutedContent, StringComparison.OrdinalIgnoreCase))
        {
            return brand with
            {
                TextSecondary = bundle.Color,
                TextPrimary = bundle.Color,
                ActionDefault = bundle.Color
            };
        }

        if (string.Equals(alias, NerdDesignSystemUi.OnBrandChrome, StringComparison.OrdinalIgnoreCase))
        {
            return brand with
            {
                AppbarText = bundle.Color,
                TextPrimary = bundle.Color
            };
        }

        if (string.Equals(alias, NerdDesignSystemUi.OnPrimaryAction, StringComparison.OrdinalIgnoreCase))
        {
            return brand with { PrimaryText = bundle.Color };
        }

        if (string.Equals(alias, NerdDesignSystemUi.PageSurface, StringComparison.OrdinalIgnoreCase))
        {
            return brand with
            {
                Surface = bundle.Surface,
                Background = bundle.Surface,
                TextPrimary = bundle.Content
            };
        }

        if (string.Equals(alias, NerdDesignSystemUi.BrandChrome, StringComparison.OrdinalIgnoreCase))
        {
            return brand with { AppbarBackground = bundle.Surface, AppbarText = bundle.Text };
        }

        if (string.Equals(alias, NerdDesignSystemUi.NavSurface, StringComparison.OrdinalIgnoreCase))
        {
            return brand with
            {
                DrawerBackground = bundle.Surface,
                DrawerText = bundle.Content,
                DrawerIcon = bundle.Content
            };
        }

        if (string.Equals(alias, NerdDesignSystemUi.InputSurface, StringComparison.OrdinalIgnoreCase))
        {
            return brand with
            {
                Surface = bundle.Surface,
                LinesInputs = bundle.Border,
                TextPrimary = bundle.Content
            };
        }

        return brand with
        {
            Primary = bundle.Color,
            PrimaryText = bundle.Text,
            PrimaryHover = bundle.Hover,
            ActionDefault = bundle.Color,
            TextPrimary = bundle.Content,
            LinesDefault = bundle.Border,
            LinesInputs = bundle.Border
        };
    }

    private static bool IsPaletteFirstIntentAlias(string aliasName) =>
        string.Equals(aliasName, NerdDesignSystemUi.PrimaryAction, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.SecondaryAction, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.MutedContent, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.NavItem, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.NavItemActive, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.Highlight, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.Info, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.Success, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.Danger, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.OnBrandChrome, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.OnPrimaryAction, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.FocusRing, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.InputBorder, StringComparison.OrdinalIgnoreCase);

    private static bool IsSurfaceIntentAlias(string aliasName) =>
        string.Equals(aliasName, NerdDesignSystemUi.PageSurface, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.BrandChrome, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.NavSurface, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.InputSurface, StringComparison.OrdinalIgnoreCase);
}

public sealed record NerdMudAliasColorBundle(
    string Color,
    string Text,
    string Content,
    string Border,
    string Hover,
    string Surface);
