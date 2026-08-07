using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace TheNerdCollective.MudComponents.DesignTokens.Analyzers;

/// <summary>
/// Scans Razor AdditionalFiles for MudButton/MudChip/MudLink patterns that typically paint
/// light-on-light (WCAG fail), e.g. Variant.Text/Outlined + muted-content / primary-action / Color.Primary.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NerdMudButtonContrastAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "NRDT001";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Mud control may fail WCAG contrast on page surface",
        messageFormat: "{0}",
        category: "Accessibility",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
            "Outlined/Text Mud controls must not use filled/status intents (muted-content, primary-action, page-surface, info, success, highlight) or Mud Color.Primary/Info/Success on chrome. MudLink must not use action intents. Prefer BrandChrome Outlined or Filled + PrimaryAction.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterAdditionalFileAction(AnalyzeAdditionalFile);
    }

    private static void AnalyzeAdditionalFile(AdditionalFileAnalysisContext context)
    {
        var path = context.AdditionalFile.Path;
        if (path is null ||
            !(path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase) ||
              path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var text = context.AdditionalFile.GetText(context.CancellationToken);
        if (text is null)
        {
            return;
        }

        foreach (var hit in NerdRazorContrastHeuristics.FindViolations(text.ToString()))
        {
            var line = Math.Max(0, hit.Line - 1);
            var lineSpan = text.Lines[Math.Min(line, text.Lines.Count - 1)].Span;
            var location = Location.Create(
                path,
                lineSpan,
                text.Lines.GetLinePositionSpan(lineSpan));

            context.ReportDiagnostic(Diagnostic.Create(Rule, location, hit.Message));
        }
    }
}

public readonly struct NerdRazorContrastHit
{
    public NerdRazorContrastHit(int line, string message)
    {
        Line = line;
        Message = message;
    }

    public int Line { get; }
    public string Message { get; }
}

/// <summary>
/// Shared heuristics so unit tests can assert without spinning a full analyzer host.
/// </summary>
public static class NerdRazorContrastHeuristics
{
    private static readonly Regex ControlBlock = new(
        @"<(?<tag>MudButton|MudIconButton|MudChip|MudFab|MudLink|MudAlert)\b(?<attrs>[\s\S]*?)(?:/>|>)",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex VariantAttr = new(
        @"Variant\s*=\s*""(?<v>[^""]+)""",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex ClassAttr = new(
        @"Class\s*=\s*""(?<c>[^""]+)""",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex ClassExpr = new(
        @"Class\s*=\s*""?@[^""\n>]*(?<c>MutedContent|PrimaryAction|PageSurface|OnPrimaryAction|Info|Success|Highlight|muted-content|primary-action|page-surface|on-primary-action|info|success|highlight)[^""\n>]*""?",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex ColorAttr = new(
        @"Color\s*=\s*""(?:@)?Color\.(?<c>\w+)""",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex SeverityAttr = new(
        @"Severity\s*=\s*""(?:@)?Severity\.(\w+)""",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>
    /// Intents that paint poorly as Outlined/Text on page-surface (light fills, ContrastText, status accents).
    /// Status aliases (info/success/highlight) are for Filled alerts/chips — not chrome labels.
    /// </summary>
    private static readonly string[] DangerousIntents =
    [
        "muted-content",
        "MutedContent",
        "primary-action",
        "PrimaryAction",
        "page-surface",
        "PageSurface",
        "kridt-lys",
        "on-primary-action",
        "OnPrimaryAction",
        "info",
        "Info",
        "success",
        "Success",
        "highlight",
        "Highlight",
        "flod" // DNF Info accent — light cyan fails WCAG as outlined chrome
    ];

    /// <summary>
    /// Mud theme colors that paint poorly as Outlined/Text chrome on page-surface (DNF lime/cyan on cream).
    /// </summary>
    private static readonly string[] DangerousMudColors =
    [
        "Primary",
        "Info",
        "Success",
        "Warning",
        "Secondary",
        "Tertiary"
    ];

    public static IReadOnlyList<NerdRazorContrastHit> FindViolations(string razorSource)
    {
        if (string.IsNullOrWhiteSpace(razorSource))
        {
            return Array.Empty<NerdRazorContrastHit>();
        }

        var hits = new List<NerdRazorContrastHit>();
        foreach (Match match in ControlBlock.Matches(razorSource))
        {
            var attrs = match.Groups["attrs"].Value;
            var tag = match.Groups["tag"].Value;
            var line = LineNumber(razorSource, match.Index);

            if (string.Equals(tag, "MudLink", StringComparison.Ordinal))
            {
                TryAddIntentViolation(hits, tag, attrs, variantValue: null, line);
                continue;
            }

            if (string.Equals(tag, "MudAlert", StringComparison.Ordinal))
            {
                TryAddMudAlertViolation(hits, attrs, line);
                continue;
            }

            var variant = VariantAttr.Match(attrs);
            var variantValue = variant.Success ? variant.Groups["v"].Value : string.Empty;
            var isTextOrOutlined =
                variantValue.Contains("Text", StringComparison.OrdinalIgnoreCase) ||
                variantValue.Contains("Outlined", StringComparison.OrdinalIgnoreCase);

            // Default MudButton without Variant is Text in MudBlazor — treat missing as risky when intent is dangerous.
            if (!variant.Success)
            {
                isTextOrOutlined = true;
            }

            if (!isTextOrOutlined)
            {
                continue;
            }

            TryAddIntentViolation(hits, tag, attrs, variantValue, line);
            TryAddColorViolation(hits, tag, attrs, variantValue, line);
        }

        return hits;
    }

    private static void TryAddIntentViolation(
        List<NerdRazorContrastHit> hits,
        string tag,
        string attrs,
        string? variantValue,
        int line)
    {
        var classMatch = ClassAttr.Match(attrs);
        var classValue = classMatch.Success ? classMatch.Groups["c"].Value : string.Empty;
        if (string.IsNullOrEmpty(classValue) && ClassExpr.IsMatch(attrs))
        {
            classValue = ClassExpr.Match(attrs).Value;
        }

        if (string.IsNullOrEmpty(classValue))
        {
            return;
        }

        foreach (var intent in DangerousIntents)
        {
            if (classValue.IndexOf(intent, StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            var variantLabel = string.Equals(tag, "MudLink", StringComparison.Ordinal)
                ? "(link)"
                : string.IsNullOrEmpty(variantValue) ? "(default/Text)" : variantValue;

            hits.Add(new NerdRazorContrastHit(
                line,
                $"{tag} with Variant {variantLabel} uses Class intent '{intent}', which often paints light-on-light (WCAG 2.1). Use BrandChrome (Outlined) or PrimaryAction with Variant.Filled."));
            break;
        }
    }

    private static void TryAddMudAlertViolation(
        List<NerdRazorContrastHit> hits,
        string attrs,
        int line)
    {
        if (!SeverityAttr.IsMatch(attrs) || HasDesignTokenClass(attrs))
        {
            return;
        }

        var severity = SeverityAttr.Match(attrs).Groups[1].Value;
        hits.Add(new NerdRazorContrastHit(
            line,
            $"MudAlert uses Severity.{severity} without a design-token Class. Mud theme severity colors often fail WCAG on page-surface (e.g. forest-on-forest). Use Class with Info/Success/Highlight/Danger token + Variant.Outlined or Filled."));
    }

    private static bool HasDesignTokenClass(string attrs)
    {
        if (ClassExpr.IsMatch(attrs))
        {
            return true;
        }

        var classMatch = ClassAttr.Match(attrs);
        if (!classMatch.Success)
        {
            return false;
        }

        var classValue = classMatch.Groups["c"].Value;
        return classValue.Contains("dnf-", StringComparison.OrdinalIgnoreCase)
               || classValue.Contains("tnc-", StringComparison.OrdinalIgnoreCase)
               || classValue.Contains("acme-", StringComparison.OrdinalIgnoreCase)
               || classValue.Contains("dryk-", StringComparison.OrdinalIgnoreCase)
               || classValue.Contains("recipe-", StringComparison.OrdinalIgnoreCase)
               || classValue.Contains("brand-chrome", StringComparison.OrdinalIgnoreCase)
               || classValue.Contains("primary-action", StringComparison.OrdinalIgnoreCase)
               || classValue.Contains("muted-content", StringComparison.OrdinalIgnoreCase);
    }

    private static void TryAddColorViolation(
        List<NerdRazorContrastHit> hits,
        string tag,
        string attrs,
        string variantValue,
        int line)
    {
        var colorMatch = ColorAttr.Match(attrs);
        if (!colorMatch.Success)
        {
            return;
        }

        var colorValue = colorMatch.Groups["c"].Value;
        if (DangerousMudColors.All(c => !string.Equals(c, colorValue, StringComparison.Ordinal)))
        {
            return;
        }

        hits.Add(new NerdRazorContrastHit(
            line,
            $"{tag} with Variant {(string.IsNullOrEmpty(variantValue) ? "(default/Text)" : variantValue)} uses Color.{colorValue}, which often paints theme accent on page-surface (WCAG 2.1). Use Class=\"dnf-brand-chrome\" for Outlined chrome or Variant.Filled for primary CTAs."));
    }

    private static int LineNumber(string text, int index)
    {
        var line = 1;
        for (var i = 0; i < index && i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                line++;
            }
        }

        return line;
    }
}
