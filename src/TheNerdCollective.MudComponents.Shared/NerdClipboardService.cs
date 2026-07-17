using Microsoft.JSInterop;

namespace TheNerdCollective.MudComponents.Shared;

public sealed class NerdClipboardService(IJSRuntime jsRuntime)
{
    public ValueTask CopyAsync(string text) =>
        jsRuntime.InvokeVoidAsync("nerdShared.copyText", text);
}
