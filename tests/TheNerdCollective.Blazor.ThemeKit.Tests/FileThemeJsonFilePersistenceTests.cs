using MudBlazor;
using TheNerdCollective.Blazor.ThemeKit;
using Xunit;

namespace TheNerdCollective.Blazor.ThemeKit.Tests;

public class FileThemeJsonFilePersistenceTests
{
    [Fact]
    public async Task SaveAsync_updates_theme_file_and_index()
    {
        var themesDir = CreateThemesDirectory();
        try
        {
            var catalog = new TestCatalog();
            catalog.Reload(themesDir);
            var persistence = new FileThemeJsonFilePersistence(themesDir, catalog);
            var document = new MudThemeJsonDocument
            {
                Id = "playbook-sandbox",
                Version = "0.2.0",
                UpdatedAt = "2026-07-18",
                Tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["light.primary"] = "#123456"
                }
            };

            var result = await persistence.SaveAsync(document);

            Assert.True(result.Success);
            Assert.Equal("0.2.0", result.Version);
            var saved = await File.ReadAllTextAsync(Path.Combine(themesDir, "playbook-sandbox.theme.json"));
            Assert.Contains("#123456", saved);
        }
        finally
        {
            Directory.Delete(themesDir, recursive: true);
        }
    }

    private static string CreateThemesDirectory()
    {
        var themesDir = Path.Combine(Path.GetTempPath(), $"theme-kit-persist-{Guid.NewGuid():N}");
        Directory.CreateDirectory(themesDir);

        File.WriteAllText(Path.Combine(themesDir, "themes.index.json"), """
            {
              "schemaVersion": "1.0",
              "defaultThemeId": "playbook-sandbox",
              "themes": [
                {
                  "id": "playbook-sandbox",
                  "displayName": "Playbook sandbox",
                  "version": "0.1.0",
                  "file": "playbook-sandbox.theme.json",
                  "source": "playbook-local"
                }
              ]
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
        public string DefaultThemeId { get; private set; } = "playbook-sandbox";
        public IReadOnlyList<MudThemeDescriptor> All { get; private set; } = [];
        public MudTheme GetTheme(string themeId) => new();
        public MudThemeIndexEntry? TryGetIndexEntry(string themeId) => null;
        public void Reload() { }
        public void Reload(string themesDirectory)
        {
            var index = MudThemeJsonSerializer.LoadIndex(themesDirectory);
            DefaultThemeId = index.DefaultThemeId;
            All = index.Themes.Select(entry => new MudThemeDescriptor(entry.Id, entry.DisplayName, entry.Version, entry.PreviewPrimaryHex, entry.UpdatedAt, entry.Source)).ToList();
        }
    }
}
