using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdCoreAssemblyGuardTests
{
    [Fact]
    public void Core_assembly_does_not_reference_MudBlazor()
    {
        var coreAssembly = typeof(NerdCoreCssGenerator).Assembly;
        var references = coreAssembly.GetReferencedAssemblies();

        Assert.DoesNotContain(
            references,
            reference => string.Equals(reference.Name, "MudBlazor", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Mud_harvest_adapter_delegates_to_inventory_validator()
    {
        var errors = NerdMudHarvestAdapter.ValidateHarvestCoverage("9.7.0");

        Assert.Empty(errors);
    }
}
