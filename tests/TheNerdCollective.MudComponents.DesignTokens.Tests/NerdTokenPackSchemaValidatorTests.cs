using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdTokenPackSchemaValidatorTests
{
    [Fact]
    public void Validate_rejects_empty_document()
    {
        var errors = NerdTokenPackSchemaValidator.Validate(string.Empty);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Validate_rejects_missing_required_fields()
    {
        var errors = NerdTokenPackSchemaValidator.Validate("""{"clientId":"x"}""");
        Assert.Contains(errors, error => error.Contains("prefix", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, error => error.Contains("version", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, error => error.Contains("colors", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_accepts_minimal_v2_pack()
    {
        var errors = NerdTokenPackSchemaValidator.Validate(
            """
            {
              "clientId": "x",
              "prefix": "x",
              "version": 2,
              "colors": {
                "ink": { "value": "#111111" }
              }
            }
            """);
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("tnc")]
    [InlineData("dnf")]
    [InlineData("acme")]
    [InlineData("demo")]
    public void Reference_brand_json_passes_schema_validation(string brandId)
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "TheNerdCollective.MudComponents.DesignTokens", "reference", "brands", $"{brandId}.token-pack.json"));
        var json = File.ReadAllText(path);
        var errors = NerdTokenPackSchemaValidator.Validate(json);
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    [Fact]
    public void Embedded_schema_resource_loads()
    {
        var schema = NerdTokenPackSchemaValidator.GetEmbeddedSchemaText();
        Assert.Contains("\"version\"", schema);
        Assert.Contains("\"approvedPairings\"", schema);
    }
}
