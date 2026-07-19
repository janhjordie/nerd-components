namespace TheNerdCollective.MudComponents.DesignTokens;

public static class NerdBrandPackLabels
{
    public static string Format(INerdBrandPack pack)
    {
        ArgumentNullException.ThrowIfNull(pack);
        return string.IsNullOrWhiteSpace(pack.IdentityVersion)
            ? pack.DisplayName
            : $"{pack.DisplayName} ({pack.IdentityVersion})";
    }
}
