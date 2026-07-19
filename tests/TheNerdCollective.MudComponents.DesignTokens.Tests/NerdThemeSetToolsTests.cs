using TheNerdCollective.Brand.Tnc;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdThemeSetToolsTests
{
  [Fact]
  public void CreateFromOptions_builds_dark_set_when_tokens_define_dark()
  {
    var options = new NerdDesignTokenOptions();
    options.Add("ink", new NerdColorToken
    {
      Value = "#111827",
      Dark = "#F9FAFB",
      DarkContrastText = "#111827"
    });

    var sets = NerdThemeSetTools.CreateFromOptions(options);

    Assert.True(sets.ContainsKey("dark"));
    Assert.True(sets["dark"].Colors.ContainsKey("ink"));
    Assert.Equal("#F9FAFB", sets["dark"].Colors["ink"].Value);
  }

  [Fact]
  public void Pack_roundtrip_includes_theme_sets()
  {
    var options = new NerdDesignTokenOptions();
    NerdTncDesignTokenPresets.Apply(options);

    var pack = NerdTokenPack.FromOptions(options, "tnc");

    Assert.NotEmpty(pack.ThemeSets);
  }

  [Fact]
  public void SyncColorTokensFromThemeSets_applies_dark_overrides_to_color_tokens()
  {
    var options = new NerdDesignTokenOptions { Prefix = "demo" };
    options.Add("ink", new NerdColorToken { Value = "#111827", Light = "#111827" });
    options.SetThemeSet("dark", new NerdThemeSet
    {
      Id = "dark",
      Colors = new(StringComparer.OrdinalIgnoreCase)
      {
        ["ink"] = new NerdThemeSetColorToken { Value = "#F9FAFB", ContrastText = "#111827" }
      }
    });

    NerdThemeSetTools.SyncColorTokensFromThemeSets(options);

    Assert.Equal("#F9FAFB", options.Colors["ink"].Dark);
    Assert.Equal("#111827", options.Colors["ink"].DarkContrastText);
  }
}
