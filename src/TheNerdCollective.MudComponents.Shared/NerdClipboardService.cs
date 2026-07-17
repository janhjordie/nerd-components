using Microsoft.JSInterop;

namespace TheNerdCollective.MudComponents.Shared;

/// <summary>
/// Copies text to the browser clipboard through <c>nerd-shared.js</c>.
/// </summary>
public sealed class NerdClipboardService(IJSRuntime jsRuntime)
{
    /// <summary>Copies the provided text to the clipboard.</summary>
    public ValueTask CopyAsync(string text) =>
        jsRuntime.InvokeVoidAsync("nerdShared.copyText", text);
}
