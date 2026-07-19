namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Reports MudBlazor P1 state-bridge coverage from generated CSS (TS-029 / HR-136).
/// </summary>
public static class NerdMudStateParityTools
{
    public static NerdMudStateParityResult Evaluate(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var css = MudBlazorDesignTokenCssGenerator.Generate(options);
        var entries = NerdMudInventoryRuleTable.Load(options.MudBlazorVersion)
            .Where(entry => entry.RequiredSelectors.Count > 0 || entry.ForbiddenSelectors.Count > 0)
            .ToList();

        var components = entries
            .Select(entry => EvaluateEntry(entry, css))
            .ToList();

        var score = components.Count == 0
            ? 0
            : (int)Math.Round(components.Average(component => component.Score));

        return new NerdMudStateParityResult(score, components);
    }

    private static NerdMudStateParityComponent EvaluateEntry(NerdMudInventoryRuleEntry entry, string css)
    {
        var required = entry.RequiredSelectors.Count(selector => css.Contains(selector, StringComparison.Ordinal));
        var forbidden = entry.ForbiddenSelectors.Count(selector => css.Contains(selector, StringComparison.Ordinal));
        var requiredScore = entry.RequiredSelectors.Count == 0
            ? 100
            : (int)Math.Round(required * 100d / entry.RequiredSelectors.Count);
        var penalty = forbidden * 25;
        var score = Math.Clamp(requiredScore - penalty, 0, 100);
        var detail = forbidden == 0
            ? $"{required}/{entry.RequiredSelectors.Count} required selectors present in generated CSS."
            : $"{required}/{entry.RequiredSelectors.Count} required selectors; {forbidden} forbidden selector(s) still emitted.";

        return new NerdMudStateParityComponent(
            entry.Component,
            ResolveFamily(entry),
            entry.Classification,
            score,
            detail,
            required == entry.RequiredSelectors.Count && forbidden == 0);
    }

    private static string ResolveFamily(NerdMudInventoryRuleEntry entry) =>
        entry.Component switch
        {
            "tabs" or "navmenu" or "menu" or "breadcrumbs" or "stepper" or "pagination" => "navigation",
            "switch" or "checkbox" or "radio" or "togglegroup" => "toggle",
            "slider" or "rating" or "fileupload" or "dropzone" or "input" or "field" or "inputlabel" or "inputcontrol" => "input",
            "button" or "chip" or "iconbutton" or "fab" or "buttongroup" or "fabmenu" => "action",
            "avatar" or "link" or "image" or "typography" or "icons" => "content",
            "divider" => "structure",
            "simpletable" => "data",
            "list" or "table" or "treeview" or "timeline" or "datagrid" => "data",
            "expansionpanel" or "card" or "drawer" or "paper" or "collapse" or "form" => "structure",
            "carousel" => "media",
            "chart" => "chart",
            "toolbar" or "appbar" or "grid" or "layout" => "layout",
            "overlay" => "overlay",
            "skeleton" => "feedback",
            "pickerdate" or "pickertime" or "pickercolor" or "picker" or "select" or "autocomplete" => "picker",
            "popover" or "dialog" or "tooltip" => "overlay",
            "alert" or "badge" or "snackbar" or "progresslinear" or "progresscircular" => "feedback",
            _ => "component"
        };
}

public sealed record NerdMudStateParityResult(
    int Score,
    IReadOnlyList<NerdMudStateParityComponent> Components);

public sealed record NerdMudStateParityComponent(
    string Component,
    string Family,
    string Classification,
    int Score,
    string Detail,
    bool Passes);
