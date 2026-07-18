namespace TheNerdCollective.MudComponents.Shared;

/// <summary>Optional feature gates for catalog save/workbook flows. Defaults to fully open.</summary>
public interface INerdCatalogEntitlements
{
    bool CanUseWorkbook();

    bool CanSaveClientPacks();

    bool CanSaveAnotherPack(int currentSavedPackCount);
}

/// <summary>Community default — all catalog pro workflows enabled.</summary>
public sealed class NerdOpenCatalogEntitlements : INerdCatalogEntitlements
{
    public bool CanUseWorkbook() => true;

    public bool CanSaveClientPacks() => true;

    public bool CanSaveAnotherPack(int currentSavedPackCount) => true;
}
