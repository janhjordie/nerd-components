namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public sealed class NerdResponsiveTypographyCss
{
    public NerdResponsiveTypographyCss(string content) => Content = content;

    public string Content { get; private set; }

    public event Action? Changed;

    public void Update(ResponsiveTypographyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        Content = MudBlazorResponsiveTypographyCssGenerator.Generate(options);
        Changed?.Invoke();
    }
}
