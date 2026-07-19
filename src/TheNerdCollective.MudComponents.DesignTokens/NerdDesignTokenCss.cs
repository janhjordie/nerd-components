namespace TheNerdCollective.MudComponents.DesignTokens;

public sealed class NerdDesignTokenCss
{
    public NerdDesignTokenCss(string content) => Content = content;

    public string Content { get; private set; }

    public event Action? Changed;

    public void Update(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        Content = MudBlazorDesignTokenCssGenerator.Generate(options);
        Changed?.Invoke();
    }
}
