using Microsoft.AspNetCore.Components;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public partial class NerdResponsiveTypographyStyles : IDisposable
{
    [Inject]
    private NerdResponsiveTypographyCss TypographyCss { get; set; } = default!;

    protected override void OnInitialized() => TypographyCss.Changed += OnTypographyCssChanged;

    private void OnTypographyCssChanged() => InvokeAsync(StateHasChanged);

    public void Dispose() => TypographyCss.Changed -= OnTypographyCssChanged;
}
