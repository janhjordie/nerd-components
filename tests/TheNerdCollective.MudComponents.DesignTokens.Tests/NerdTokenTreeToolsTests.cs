using TheNerdCollective.Brand.Tnc;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdTokenTreeToolsTests
{
  [Fact]
  public void Build_includes_spacing_and_theme_set_groups_for_tnc()
  {
    var options = new NerdDesignTokenOptions();
    NerdTncDesignTokenPresets.Apply(options);

    var tree = NerdTokenTreeTools.Build(options);

    Assert.Contains(tree, node => node.Id == "spacing");
    Assert.Contains(tree, node => node.Id == "colors");
    Assert.Contains(tree, node => node.Id == "theme-sets");
    Assert.True(tree.First(node => node.Id == "spacing").Children.Count > 0);
  }

  [Fact]
  public void Filter_matches_spacing_token_name()
  {
    var options = new NerdDesignTokenOptions();
    NerdSpacingScaleTools.ApplyDefaultScale(options);

    var filtered = NerdTokenTreeTools.Filter(NerdTokenTreeTools.Build(options), "4");

    Assert.Single(filtered);
    Assert.Equal("spacing", filtered[0].Id);
    Assert.Contains(filtered[0].Children, child => child.TargetId == "4");
  }

  [Fact]
  public void Build_recipes_include_semantic_roles_and_playbook_link()
  {
    var options = new NerdDesignTokenOptions();
    NerdTncDesignTokenPresets.Apply(options);

    var recipes = NerdTokenTreeTools.Build(options).First(node => node.Id == "recipes");
    var hero = recipes.Children.First(child => child.TargetId == "hero");

    Assert.Equal("layout-kit-hero", hero.AnchorId);
    Assert.Contains(hero.Children, child => child.Label == "surface");
    Assert.Contains(hero.Children, child => child.Label == "content");
  }

  [Fact]
  public void Build_includes_foundation_groups_for_tnc()
  {
    var options = new NerdDesignTokenOptions();
    NerdTncDesignTokenPresets.Apply(options);

    var tree = NerdTokenTreeTools.Build(options);

    Assert.Contains(tree, node => node.Id == "breakpoints");
    Assert.Contains(tree, node => node.Id == "motion-durations");
    Assert.Contains(tree, node => node.Id == "z-index");
  }
}
