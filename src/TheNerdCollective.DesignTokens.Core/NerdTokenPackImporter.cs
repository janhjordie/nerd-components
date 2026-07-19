namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Result of importing a token pack JSON document.</summary>
public sealed record NerdTokenPackImportResult(
    bool Success,
    string Message,
    NerdTokenPack? Pack = null);

/// <summary>Validates and parses token pack JSON for catalog import flows.</summary>
public static class NerdTokenPackImporter
{
    public static NerdTokenPackImportResult TryImport(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new NerdTokenPackImportResult(false, "JSON file was empty.");
        }

        var schemaErrors = NerdTokenPackSchemaValidator.Validate(json);
        if (schemaErrors.Count > 0)
        {
            return new NerdTokenPackImportResult(false, string.Join(" ", schemaErrors));
        }

        try
        {
            var pack = NerdTokenPack.FromJson(json);
            var summary =
                $"{pack.DisplayName ?? pack.BrandId ?? pack.Prefix}: {pack.Colors.Count} colors, " +
                $"{pack.Recipes.Count} recipes, {pack.ApprovedPairings.Count} approved pairings.";
            return new NerdTokenPackImportResult(true, summary, pack);
        }
        catch (Exception ex)
        {
            return new NerdTokenPackImportResult(false, ex.Message);
        }
    }
}
