using TheNerdCollective.MudComponents.DesignTokens.Analyzers;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdMudButtonContrastAnalyzerTests
{
    [Fact]
    public void Flags_text_button_with_muted_content_class()
    {
        const string razor = """
            <MudButton Variant="Variant.Text" Class="dnf-muted-content">Design system</MudButton>
            """;

        var hits = NerdRazorContrastHeuristics.FindViolations(razor);

        Assert.Single(hits);
        Assert.Contains("muted-content", hits[0].Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Flags_outlined_primary_action_button()
    {
        const string razor = """
            <MudButton Variant="Variant.Outlined" Class="@Ui(NerdDesignSystemUi.PrimaryAction)">Copy</MudButton>
            """;

        var hits = NerdRazorContrastHeuristics.FindViolations(razor);

        Assert.NotEmpty(hits);
    }

    [Fact]
    public void Allows_filled_primary_action_button()
    {
        const string razor = """
            <MudButton Variant="Variant.Filled" Class="dnf-primary-action">Save</MudButton>
            """;

        var hits = NerdRazorContrastHeuristics.FindViolations(razor);

        Assert.Empty(hits);
    }

    [Fact]
    public void Allows_outlined_brand_chrome_button()
    {
        const string razor = """
            <MudButton Variant="Variant.Outlined" Class="dnf-brand-chrome">PlayBook</MudButton>
            """;

        var hits = NerdRazorContrastHeuristics.FindViolations(razor);

        Assert.Empty(hits);
    }

    [Fact]
    public void Flags_outlined_info_chip_on_page_surface()
    {
        const string razor = """
            <MudChip T="string" Class="@Ui(NerdDesignSystemUi.Info)" Variant="Variant.Outlined">/nerd-playbook</MudChip>
            """;

        var hits = NerdRazorContrastHeuristics.FindViolations(razor);

        Assert.NotEmpty(hits);
        Assert.Contains("Info", hits[0].Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Flags_outlined_flod_token_class()
    {
        const string razor = """
            <MudChip T="string" Class="dnf-flod" Variant="Variant.Outlined">route</MudChip>
            """;

        var hits = NerdRazorContrastHeuristics.FindViolations(razor);

        Assert.NotEmpty(hits);
    }

    [Fact]
    public void Allows_filled_info_chip()
    {
        const string razor = """
            <MudChip T="string" Class="@Ui(NerdDesignSystemUi.Info)" Variant="Variant.Filled">OK</MudChip>
            """;

        var hits = NerdRazorContrastHeuristics.FindViolations(razor);

        Assert.Empty(hits);
    }

    [Fact]
    public void Flags_outlined_button_with_color_primary()
    {
        const string razor = """
            <MudButton Variant="Variant.Outlined" Color="Color.Primary" Href="/fair-departure">CTA</MudButton>
            """;

        var hits = NerdRazorContrastHeuristics.FindViolations(razor);

        Assert.Single(hits);
        Assert.Contains("Color.Primary", hits[0].Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Flags_text_button_with_color_primary()
    {
        const string razor = """
            <MudButton Variant="Variant.Text" Color="Color.Primary" Href="/pricing">Pricing</MudButton>
            """;

        var hits = NerdRazorContrastHeuristics.FindViolations(razor);

        Assert.Single(hits);
    }

    [Fact]
    public void Flags_mudlink_with_primary_action_class()
    {
        const string razor = """
            <MudLink Href="/mission" Class="nc-hero-impact-link dnf-primary-action">Mission →</MudLink>
            """;

        var hits = NerdRazorContrastHeuristics.FindViolations(razor);

        Assert.Single(hits);
        Assert.Contains("primary-action", hits[0].Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Allows_mudlink_without_dangerous_intent()
    {
        const string razor = """
            <MudLink Href="/trust" Typo="Typo.body2">Trust →</MudLink>
            """;

        var hits = NerdRazorContrastHeuristics.FindViolations(razor);

        Assert.Empty(hits);
    }

    [Fact]
    public void Flags_mudalert_with_severity_without_token_class()
    {
        const string razor = """
            <MudAlert Severity="Severity.Info" Variant="Variant.Filled" Dense="true">Invisible on DNF</MudAlert>
            """;

        var hits = NerdRazorContrastHeuristics.FindViolations(razor);

        Assert.Single(hits);
        Assert.Contains("MudAlert", hits[0].Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Flags_mudtabs_with_primary_action_class()
    {
        const string razor = """
            <MudTabs Elevation="1" Rounded="true" Class="@Ui(NerdDesignSystemUi.PrimaryAction)">
                <MudTabPanel Text="Mud components" />
            </MudTabs>
            """;

        var hits = NerdRazorContrastHeuristics.FindViolations(razor);

        Assert.Single(hits);
        Assert.Contains("MudTabs", hits[0].Message, StringComparison.Ordinal);
        Assert.Contains("PrimaryAction", hits[0].Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Flags_mudtabs_with_literal_primary_action_class()
    {
        const string razor = """
            <MudTabs Class="dnf-primary-action" Elevation="0">
                <MudTabPanel Text="All (53)" />
            </MudTabs>
            """;

        var hits = NerdRazorContrastHeuristics.FindViolations(razor);

        Assert.Single(hits);
    }

    [Fact]
    public void Allows_mudtabs_with_brand_chrome_class()
    {
        const string razor = """
            <MudTabs Class="@Ui(NerdDesignSystemUi.BrandChrome)" Elevation="1" Rounded="true">
                <MudTabPanel Text="Mud components" />
            </MudTabs>
            """;

        var hits = NerdRazorContrastHeuristics.FindViolations(razor);

        Assert.Empty(hits);
    }

    [Fact]
    public void Flags_mudtabs_with_color_primary()
    {
        const string razor = """
            <MudTabs Color="Color.Primary" Elevation="1">
                <MudTabPanel Text="Buttons" />
            </MudTabs>
            """;

        var hits = NerdRazorContrastHeuristics.FindViolations(razor);

        Assert.Single(hits);
        Assert.Contains("Color.Primary", hits[0].Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Allows_mudalert_with_token_class()
    {
        const string razor = """
            <MudAlert Class="dnf-info" Variant="Variant.Outlined" Dense="true">Readable</MudAlert>
            """;

        var hits = NerdRazorContrastHeuristics.FindViolations(razor);

        Assert.Empty(hits);
    }
}
