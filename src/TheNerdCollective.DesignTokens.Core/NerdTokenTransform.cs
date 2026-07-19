namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Reference-based color transform definition (HR-103).</summary>
public sealed record NerdTokenTransform(string Source, string Operation, double Amount = 0.12)
{
    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Source);
        ArgumentException.ThrowIfNullOrWhiteSpace(Operation);
        if (Amount < 0 || Amount > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(Amount), "Amount must be between 0 and 1.");
        }

        if (!NerdTokenTransformTools.SupportedOperations.Contains(Operation))
        {
            throw new ArgumentException($"Unsupported transform operation '{Operation}'.", nameof(Operation));
        }
    }
}
