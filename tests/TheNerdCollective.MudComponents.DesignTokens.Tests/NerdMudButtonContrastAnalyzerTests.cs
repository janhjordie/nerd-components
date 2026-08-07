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
}
