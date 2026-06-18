using MudBlazor;
using TheNerdCollective.Blazor.ThemeKit;
using Xunit;

namespace TheNerdCollective.Blazor.ThemeKit.Tests;

public class ThemeCatalogFileOperationsTests
{
    [Theory]
    [InlineData("My Brand Theme", "my-brand-theme")]
    [InlineData("Playbook 2", "playbook-2")]
    [InlineData("2026 Theme", "theme-2026-theme")]
    public void ThemeIdHelper_suggests_slug_from_display_name(string displayName, string expectedId)
    {
        Assert.Equal(expectedId, ThemeIdHelper.SuggestFromDisplayName(displayName));
    }

    [Theory]
    [InlineData("billetsalg-default", true)]
    [InlineData("playbook-sandbox", true)]
    [InlineData("My Theme", false)]
    [InlineData("theme_", false)]
    public void ThemeIdHelper_validates_theme_ids(string themeId, bool expectedValid)
    {
        Assert.Equal(expectedValid, ThemeIdHelper.IsValidThemeId(themeId));
    }

    [Fact]
    public async Task CreateAsync_adds_theme_file_and_index_entry()
    {
        var themesDir = CreateThemesDirectory();
        try
        {
            var catalog = new TestCatalog();
            catalog.Reload(themesDir);
            var result = await ThemeCatalogFileOperations.CreateAsync(
                themesDir,
                new ThemeCreateRequest("brand-alpha", "Brand Alpha", "playbook-sandbox"),
                catalog,
                () => catalog.Reload(themesDir));

            Assert.True(result.Success);
            Assert.Equal("brand-alpha", result.ThemeId);
            Assert.True(File.Exists(Path.Combine(themesDir, "brand-alpha.theme.json")));
            Assert.Contains(catalog.All, theme => theme.Id == "brand-alpha");
        }
        finally
        {
            Directory.Delete(themesDir, recursive: true);
        }
    }

    [Fact]
    public async Task DeleteAsync_removes_playbook_theme_and_reassigns_default()
    {
        var themesDir = CreateThemesDirectory();
        try
        {
            var catalog = new TestCatalog();
            catalog.Reload(themesDir);
            await ThemeCatalogFileOperations.CreateAsync(
                themesDir,
                new ThemeCreateRequest("brand-beta", "Brand Beta", "playbook-sandbox"),
                catalog,
                () => catalog.Reload(themesDir));

            var result = await ThemeCatalogFileOperations.DeleteAsync(
                themesDir,
                "brand-beta",
                () => catalog.Reload(themesDir));

            Assert.True(result.Success);
            Assert.Equal("brand-beta", result.DeletedThemeId);
            Assert.Equal("billetsalg-default", result.FallbackThemeId);
            Assert.False(File.Exists(Path.Combine(themesDir, "brand-beta.theme.json")));
            Assert.DoesNotContain(catalog.All, theme => theme.Id == "brand-beta");
        }
        finally
        {
            Directory.Delete(themesDir, recursive: true);
        }
    }

    [Fact]
    public async Task DeleteAsync_blocks_shared_ui_theme()
    {
        var themesDir = CreateThemesDirectory();
        try
        {
            var catalog = new TestCatalog();
            catalog.Reload(themesDir);
            var result = await ThemeCatalogFileOperations.DeleteAsync(
                themesDir,
                "billetsalg-default",
                () => catalog.Reload(themesDir));

            Assert.False(result.Success);
            Assert.Contains("beskyttet", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(themesDir, recursive: true);
        }
    }

    private static string CreateThemesDirectory()
    {
        var themesDir = Path.Combine(Path.GetTempPath(), $"theme-kit-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(themesDir);

        File.WriteAllText(Path.Combine(themesDir, "themes.index.json"), """
            {
              "schemaVersion": "1.0",
              "defaultThemeId": "billetsalg-default",
              "updatedAt": "2026-06-16",
              "themes": [
                {
                  "id": "billetsalg-default",
                  "displayName": "BilletSalg Default",
                  "version": "1.0.0",
                  "updatedAt": "2026-06-16",
                  "previewPrimaryHex": "#0B7285",
                  "file": "billetsalg-default.theme.json",
                  "source": "shared-ui"
                },
                {
                  "id": "playbook-sandbox",
                  "displayName": "Playbook Sandbox",
                  "version": "0.1.0",
                  "updatedAt": "2026-06-16",
                  "previewPrimaryHex": "#594AE2",
                  "file": "playbook-sandbox.theme.json",
                  "source": "playbook-local"
                }
              ]
            }
            """);

        File.WriteAllText(Path.Combine(themesDir, "billetsalg-default.theme.json"), """
            {
              "schemaVersion": "1.0",
              "id": "billetsalg-default",
              "version": "1.0.0",
              "tokens": { "light.primary": "#0B7285" }
            }
            """);

        File.WriteAllText(Path.Combine(themesDir, "playbook-sandbox.theme.json"), """
            {
              "schemaVersion": "1.0",
              "id": "playbook-sandbox",
              "version": "0.1.0",
              "tokens": { "light.primary": "#594AE2" }
            }
            """);

        return themesDir;
    }

    private sealed class TestCatalog : IReloadableMudThemeCatalog
    {
        private readonly Dictionary<string, MudThemeIndexEntry> _indexEntries = new(StringComparer.OrdinalIgnoreCase);
        private string _themesDirectory = string.Empty;

        public TestCatalog()
        {
            All = [];
            DefaultThemeId = "billetsalg-default";
        }

        public string DefaultThemeId { get; private set; }

        public IReadOnlyList<MudThemeDescriptor> All { get; private set; }

        public MudTheme GetTheme(string themeId)
        {
            var theme = new MudTheme
            {
                PaletteLight = new PaletteLight
                {
                    Primary = themeId.Equals("playbook-sandbox", StringComparison.OrdinalIgnoreCase)
                        ? "#594AE2"
                        : "#0B7285",
                },
            };
            theme.Typography.Default.FontFamily = ["Roboto"];
            return theme;
        }

        public MudThemeIndexEntry? TryGetIndexEntry(string themeId)
            => _indexEntries.TryGetValue(themeId, out var entry) ? entry : null;

        public void Reload()
        {
            if (!string.IsNullOrWhiteSpace(_themesDirectory))
            {
                Reload(_themesDirectory);
            }
        }

        public void Reload(string themesDirectory)
        {
            _themesDirectory = themesDirectory;
            var index = MudThemeJsonSerializer.LoadIndex(themesDirectory);
            DefaultThemeId = index.DefaultThemeId;
            _indexEntries.Clear();

            var descriptors = new List<MudThemeDescriptor>();
            foreach (var entry in index.Themes)
            {
                _indexEntries[entry.Id] = entry;
                descriptors.Add(new MudThemeDescriptor(
                    entry.Id,
                    entry.DisplayName,
                    entry.Version,
                    entry.PreviewPrimaryHex,
                    entry.UpdatedAt,
                    entry.Source));
            }

            All = descriptors;
        }
    }
}
