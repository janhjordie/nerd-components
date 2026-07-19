using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public class FileNerdTokenCommentStoreTests
{
    [Fact]
    public async Task Comment_store_round_trips_token_annotations()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"nerd-comments-{Guid.NewGuid():N}");
        try
        {
            var store = new FileNerdTokenCommentStore(directory);
            var comments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["skov"] = "Primary brand surface",
                ["himmel"] = "CTA only"
            };

            await store.SaveAsync("acme", comments);
            var loaded = await store.LoadAsync("acme");

            Assert.Equal("Primary brand surface", loaded["skov"]);
            Assert.Equal("CTA only", loaded["himmel"]);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
