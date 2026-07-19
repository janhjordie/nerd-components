using System.Text.Json;
using System.Text.Json.Nodes;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Validates token-pack JSON against <c>schema/token-pack.schema.json</c>.</summary>
public static class NerdTokenPackSchemaValidator
{
    public static IReadOnlyList<string> Validate(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return ["JSON document was empty."];
        }

        JsonNode? root;
        try
        {
            root = JsonNode.Parse(json);
        }
        catch (JsonException ex)
        {
            return [$"Invalid JSON: {ex.Message}"];
        }

        if (root is not JsonObject document)
        {
            return ["Root element must be a JSON object."];
        }

        var errors = new List<string>();
        RequireString(document, "clientId", errors);
        RequireString(document, "prefix", errors);
        RequireVersion(document, errors);
        RequireColors(document, errors);
        ValidateRecipes(document, errors);
        ValidateApprovedPairings(document, errors);
        ValidateLockedTokens(document, errors);
        ValidateShell(document, errors);
        return errors;
    }

    public static string? ValidateOrNull(string json) =>
        Validate(json).Count == 0 ? null : string.Join(" ", Validate(json));

    private static void RequireString(JsonObject document, string propertyName, List<string> errors)
    {
        if (!document.TryGetPropertyValue(propertyName, out var node) ||
            node is not JsonValue value ||
            string.IsNullOrWhiteSpace(value.GetValue<string>()))
        {
            errors.Add($"Missing or empty required property '{propertyName}'.");
        }
    }

    private static void RequireVersion(JsonObject document, List<string> errors)
    {
        if (!document.TryGetPropertyValue("version", out var node) ||
            node is not JsonValue value ||
            !value.TryGetValue(out int version) ||
            version < 2)
        {
            errors.Add("Property 'version' must be an integer >= 2.");
        }
    }

    private static void RequireColors(JsonObject document, List<string> errors)
    {
        if (!document.TryGetPropertyValue("colors", out var node) || node is not JsonObject colors)
        {
            errors.Add("Missing required object 'colors'.");
            return;
        }

        if (colors.Count == 0)
        {
            errors.Add("Object 'colors' must contain at least one token.");
            return;
        }

        foreach (var (tokenName, tokenNode) in colors)
        {
            if (tokenNode is not JsonObject token)
            {
                errors.Add($"colors.{tokenName} must be an object.");
                continue;
            }

            if (!token.TryGetPropertyValue("value", out var valueNode) ||
                valueNode is not JsonValue value ||
                string.IsNullOrWhiteSpace(value.GetValue<string>()))
            {
                errors.Add($"colors.{tokenName}.value is required.");
            }
        }
    }

    private static void ValidateRecipes(JsonObject document, List<string> errors)
    {
        if (!document.TryGetPropertyValue("recipes", out var node) || node is null)
        {
            return;
        }

        if (node is not JsonObject recipes)
        {
            errors.Add("Property 'recipes' must be an object.");
            return;
        }

        foreach (var (recipeName, recipeNode) in recipes)
        {
            if (recipeNode is not JsonObject recipe)
            {
                errors.Add($"recipes.{recipeName} must be an object.");
                continue;
            }

            RequireRecipeString(recipe, $"recipes.{recipeName}", "surface", errors);
            RequireRecipeString(recipe, $"recipes.{recipeName}", "content", errors);
        }
    }

    private static void ValidateApprovedPairings(JsonObject document, List<string> errors)
    {
        if (!document.TryGetPropertyValue("approvedPairings", out var node) || node is null)
        {
            return;
        }

        if (node is not JsonArray pairings)
        {
            errors.Add("Property 'approvedPairings' must be an array.");
            return;
        }

        for (var index = 0; index < pairings.Count; index++)
        {
            if (pairings[index] is not JsonObject pairing)
            {
                errors.Add($"approvedPairings[{index}] must be an object.");
                continue;
            }

            RequireRecipeString(pairing, $"approvedPairings[{index}]", "content", errors);
            RequireRecipeString(pairing, $"approvedPairings[{index}]", "surface", errors);
        }
    }

    private static void ValidateLockedTokens(JsonObject document, List<string> errors)
    {
        if (!document.TryGetPropertyValue("lockedTokens", out var node) || node is null)
        {
            return;
        }

        if (node is not JsonArray locked)
        {
            errors.Add("Property 'lockedTokens' must be an array.");
        }
    }

    private static void ValidateShell(JsonObject document, List<string> errors)
    {
        if (!document.TryGetPropertyValue("shell", out var node) || node is null)
        {
            return;
        }

        if (node is not JsonObject shell)
        {
            errors.Add("Property 'shell' must be an object.");
            return;
        }

        foreach (var slotName in new[] { "appBar", "drawer", "navMenu", "main" })
        {
            if (!shell.TryGetPropertyValue(slotName, out var slotNode) || slotNode is null)
            {
                continue;
            }

            if (slotNode is not JsonObject slot)
            {
                errors.Add($"shell.{slotName} must be an object.");
                continue;
            }

            var hasAlias = slot.TryGetPropertyValue("alias", out var aliasNode) &&
                           aliasNode is JsonValue aliasValue &&
                           !string.IsNullOrWhiteSpace(aliasValue.GetValue<string>());
            var hasRecipe = slot.TryGetPropertyValue("recipe", out var recipeNode) &&
                            recipeNode is JsonValue recipeValue &&
                            !string.IsNullOrWhiteSpace(recipeValue.GetValue<string>());
            if (hasAlias == hasRecipe)
            {
                errors.Add($"shell.{slotName} must specify exactly one of 'alias' or 'recipe'.");
            }
        }
    }

    private static void RequireRecipeString(
        JsonObject recipe,
        string path,
        string propertyName,
        List<string> errors)
    {
        if (!recipe.TryGetPropertyValue(propertyName, out var node) ||
            node is not JsonValue value ||
            string.IsNullOrWhiteSpace(value.GetValue<string>()))
        {
            errors.Add($"{path}.{propertyName} is required.");
        }
    }

    public static string GetEmbeddedSchemaText()
    {
        var assembly = typeof(NerdTokenPackSchemaValidator).Assembly;
        const string resourceName = "TheNerdCollective.MudComponents.DesignTokens.schema.token-pack.schema.json";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded schema resource '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
