namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Shell slot binding — either a semantic alias (<c>brand-chrome</c>) or a named recipe (<c>sidebar</c>).
/// </summary>
public sealed class NerdTokenPackShellSlot
{
    public string? Alias { get; init; }

    public string? Recipe { get; init; }

    public void Validate(string slotName)
    {
        var hasAlias = !string.IsNullOrWhiteSpace(Alias);
        var hasRecipe = !string.IsNullOrWhiteSpace(Recipe);
        if (hasAlias == hasRecipe)
        {
            throw new ArgumentException(
                $"Shell slot '{slotName}' must specify exactly one of 'alias' or 'recipe'.",
                nameof(slotName));
        }
    }
}
