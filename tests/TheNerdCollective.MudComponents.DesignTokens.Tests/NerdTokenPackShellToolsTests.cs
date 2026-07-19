using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdTokenPackShellToolsTests
{
    [Fact]
    public void Default_shell_resolves_tnc_classes()
    {
        var options = NerdEmbeddedBrandPack.FromBrandJson("tnc").TokenPack.ToOptions();
        var shell = NerdTokenPackShellTools.ResolveShell(options);

        Assert.Equal("tnc-brand-chrome", NerdTokenPackShellTools.ResolveClass(options, shell.AppBar));
        Assert.Equal("tnc-nav-surface", NerdTokenPackShellTools.ResolveClass(options, shell.Drawer));
        Assert.Equal("tnc-recipe-sidebar", NerdTokenPackShellTools.ResolveClass(options, shell.NavMenu));
        Assert.Equal("tnc-page-surface", NerdTokenPackShellTools.ResolveClass(options, shell.Main));
    }

    [Fact]
    public void Embedded_tnc_pack_includes_shell_and_framework_defaults()
    {
        var pack = NerdEmbeddedBrandPack.FromBrandJson("tnc").TokenPack;

        Assert.NotNull(pack.Shell);
        Assert.NotNull(pack.FrameworkDefaults?.MudBlazor?.Button?.Filled);
        Assert.Equal("primary-action", pack.FrameworkDefaults.MudBlazor.Button.Filled);
    }

    [Fact]
    public void Token_pack_roundtrip_preserves_shell_bindings()
    {
        var options = NerdEmbeddedBrandPack.FromBrandJson("tnc").TokenPack.ToOptions();

        var pack = NerdTokenPack.FromOptions(options, "tnc");
        var restored = pack.ToOptions();

        Assert.NotNull(restored.Shell?.NavMenu?.Recipe);
        Assert.Equal(NerdDesignSystemUi.SidebarRecipe, restored.Shell.NavMenu.Recipe);
        Assert.Equal(
            NerdDesignSystemUi.PrimaryAction,
            restored.FrameworkDefaults?.MudBlazor?.Button?.Filled);
    }
}
