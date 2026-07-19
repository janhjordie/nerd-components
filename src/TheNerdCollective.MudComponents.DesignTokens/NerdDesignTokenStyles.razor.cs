using Microsoft.AspNetCore.Components;

namespace TheNerdCollective.MudComponents.DesignTokens;

public partial class NerdDesignTokenStyles : IDisposable
{
    [Inject]
    private NerdDesignTokenCss TokenCss { get; set; } = default!;

    protected override void OnInitialized() => TokenCss.Changed += OnTokenCssChanged;

    private void OnTokenCssChanged() => InvokeAsync(StateHasChanged);

    public void Dispose() => TokenCss.Changed -= OnTokenCssChanged;
}
