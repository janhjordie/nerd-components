using Xunit;
using TheNerdCollective.MudComponents.PlayBook;

namespace TheNerdCollective.MudComponents.PlayBook.Tests;

public class MudBlazorPlayBookCatalogTests
{
    [Fact]
    public void Catalog_contains_at_least_fifty_components()
    {
        Assert.True(MudBlazorPlayBookCatalog.All.Count >= 50);
    }

    [Fact]
    public void Catalog_entries_have_unique_ids()
    {
        var ids = MudBlazorPlayBookCatalog.All.Select(entry => entry.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void Catalog_entries_have_api_urls()
    {
        foreach (var entry in MudBlazorPlayBookCatalog.All)
        {
            Assert.StartsWith("https://mudblazor.com/components/", entry.ApiUrl("https://mudblazor.com/components"));
            Assert.False(string.IsNullOrWhiteSpace(entry.ApiSlug));
        }
    }

    [Theory]
    [InlineData("button", MudBlazorPlayBookCatalog.CategoryButtons)]
    [InlineData("text-field", MudBlazorPlayBookCatalog.CategoryInputs)]
    [InlineData("table", MudBlazorPlayBookCatalog.CategoryDataDisplay)]
    public void FindById_returns_expected_category(string id, string expectedCategory)
    {
        var entry = MudBlazorPlayBookCatalog.FindById(id);
        Assert.NotNull(entry);
        Assert.Equal(expectedCategory, entry!.Category);
    }

    [Fact]
    public void GetByCategory_returns_only_matching_entries()
    {
        var buttons = MudBlazorPlayBookCatalog.GetByCategory(MudBlazorPlayBookCatalog.CategoryButtons).ToList();
        Assert.NotEmpty(buttons);
        Assert.All(buttons, entry => Assert.Equal(MudBlazorPlayBookCatalog.CategoryButtons, entry.Category));
    }

    [Fact]
    public void Typography_presets_create_themes()
    {
        var marketing = NerdPlayBookTypography.CreateTheme(NerdPlayBookTypography.MarketingPreset);
        var dense = NerdPlayBookTypography.CreateTheme(NerdPlayBookTypography.DensePreset);
        Assert.NotNull(marketing);
        Assert.NotNull(dense);
    }

    [Fact]
    public void Playground_registry_covers_all_catalog_components()
    {
        foreach (var entry in MudBlazorPlayBookCatalog.All)
        {
            Assert.True(NerdPlayBookPlaygroundRegistry.SupportsPlayground(entry.Id), $"Missing playground props for {entry.Id}");
        }
    }

    [Fact]
    public void Playground_state_reads_typed_values()
    {
        var state = NerdPlayBookPlaygroundRegistry.CreateState("button");
        state.Set("disabled", "true");
        state.Set("variant", "Outlined");

        Assert.True(state.GetBool("disabled"));
        Assert.Equal(MudBlazor.Variant.Outlined, state.GetEnum("variant", MudBlazor.Variant.Filled));
    }
}
