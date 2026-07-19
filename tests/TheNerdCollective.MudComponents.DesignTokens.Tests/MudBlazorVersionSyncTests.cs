namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class MudBlazorVersionSyncTests
{
    private static readonly string DesignTokensRoot = FindDesignTokensRoot();

    [Fact]
    public void MudBlazorVersion_matches_committed_manifest()
    {
        var manifestVersion = NerdMudVersionSyncTools.ReadManifestVersion(DesignTokensRoot);
        Assert.Equal(NerdMudVersionSyncTools.ReadPinnedVersion(), manifestVersion);
    }

    [Fact]
    public void DesignTokens_csproj_MudBlazor_matches_MudBlazorVersion()
    {
        var csprojPath = Path.Combine(DesignTokensRoot, "TheNerdCollective.MudComponents.DesignTokens.csproj");
        var errors = NerdMudVersionSyncTools.ValidateCsprojMatchesPinnedVersion(csprojPath);
        Assert.Empty(errors);
    }

    [Fact]
    public void TokenStudio_host_csproj_MudBlazor_matches_MudBlazorVersion_when_present()
    {
        var hostCsproj = NerdMudVersionSyncTools.TryFindTokenStudioHostCsproj();
        if (hostCsproj is null)
        {
            return;
        }

        var errors = NerdMudVersionSyncTools.ValidateCsprojMatchesPinnedVersion(hostCsproj);
        Assert.Empty(errors);
    }

    private static string FindDesignTokensRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src", "TheNerdCollective.MudComponents.DesignTokens");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            var sibling = Path.Combine(directory.FullName, "TheNerdCollective.MudComponents.DesignTokens");
            if (Directory.Exists(sibling))
            {
                return sibling;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate TheNerdCollective.MudComponents.DesignTokens project root.");
    }
}
