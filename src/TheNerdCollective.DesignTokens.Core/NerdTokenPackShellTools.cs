using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Resolves pack shell bindings and framework defaults to CSS classes and styles.</summary>
public static class NerdTokenPackShellTools
{
    public static NerdTokenPackShell DefaultShell { get; } = new()
    {
        AppBar = new NerdTokenPackShellSlot { Alias = NerdDesignSystemUi.BrandChrome },
        Drawer = new NerdTokenPackShellSlot { Alias = NerdDesignSystemUi.NavSurface },
        NavMenu = new NerdTokenPackShellSlot { Recipe = NerdDesignSystemUi.SidebarRecipe },
        Main = new NerdTokenPackShellSlot { Alias = NerdDesignSystemUi.PageSurface }
    };

    public static NerdMudBlazorFrameworkDefaults DefaultMudBlazorDefaults { get; } = new()
    {
        Button = new NerdMudBlazorButtonDefaults
        {
            Filled = NerdDesignSystemUi.PrimaryAction,
            Outlined = NerdDesignSystemUi.SecondaryAction,
            Text = NerdDesignSystemUi.MutedContent
        },
        TextField = new NerdMudBlazorComponentIntent { Intent = NerdDesignSystemUi.InputSurface },
        DatePicker = new NerdMudBlazorComponentIntent { Popover = NerdDesignSystemUi.PageSurface },
        NavLink = new NerdMudBlazorNavLinkDefaults
        {
            Default = NerdDesignSystemUi.NavItem,
            Active = NerdDesignSystemUi.NavItemActive
        }
    };

    public static NerdTokenPackShell ResolveShell(NerdDesignTokenOptions options) =>
        options.Shell ?? DefaultShell;

    public static NerdMudBlazorFrameworkDefaults ResolveMudBlazorDefaults(NerdDesignTokenOptions options) =>
        options.FrameworkDefaults?.MudBlazor ?? DefaultMudBlazorDefaults;

    public static string ResolveClass(NerdDesignTokenOptions options, NerdTokenPackShellSlot? slot)
    {
        if (slot is null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(slot.Alias))
        {
            return NerdDesignSystemUi.TokenClass(options.Prefix, slot.Alias);
        }

        if (!string.IsNullOrWhiteSpace(slot.Recipe))
        {
            return NerdDesignSystemUi.RecipeClass(options.Prefix, slot.Recipe);
        }

        return string.Empty;
    }

    public static string? ResolveSurfaceStyle(NerdDesignTokenOptions options, NerdTokenPackShellSlot? slot)
    {
        if (slot is null || string.IsNullOrWhiteSpace(slot.Alias))
        {
            return null;
        }

        return slot.Alias switch
        {
            NerdDesignSystemUi.PageSurface => NerdDesignSystemUi.PageSurfaceStyle(options.Prefix),
            NerdDesignSystemUi.NavSurface => NerdDesignSystemUi.NavSurfaceStyle(options.Prefix),
            _ => null
        };
    }

    public static string ResolveIntentClass(NerdDesignTokenOptions options, string? intentAlias) =>
        string.IsNullOrWhiteSpace(intentAlias)
            ? string.Empty
            : NerdDesignSystemUi.TokenClass(options.Prefix, intentAlias);

    public static void ValidateShellReferences(NerdDesignTokenOptions options, NerdTokenPackShell shell)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(shell);
        shell.Validate();
        ValidateSlot(options, shell.AppBar, nameof(shell.AppBar));
        ValidateSlot(options, shell.Drawer, nameof(shell.Drawer));
        ValidateSlot(options, shell.NavMenu, nameof(shell.NavMenu));
        ValidateSlot(options, shell.Main, nameof(shell.Main));
    }

    private static void ValidateSlot(NerdDesignTokenOptions options, NerdTokenPackShellSlot? slot, string slotName)
    {
        if (slot is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(slot.Alias) && !options.Aliases.ContainsKey(slot.Alias))
        {
            throw new ArgumentException(
                $"Shell slot '{slotName}' references missing alias '{slot.Alias}'.",
                nameof(slot));
        }

        if (!string.IsNullOrWhiteSpace(slot.Recipe) && !options.Recipes.ContainsKey(slot.Recipe))
        {
            throw new ArgumentException(
                $"Shell slot '{slotName}' references missing recipe '{slot.Recipe}'.",
                nameof(slot));
        }
    }
}
