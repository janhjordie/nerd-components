using Microsoft.JSInterop;

namespace TheNerdCollective.MudComponents.Shared;

public sealed class NerdDownloadService(IJSRuntime jsRuntime)
{
    public ValueTask DownloadTextAsync(string fileName, string content) =>
        jsRuntime.InvokeVoidAsync("nerdShared.downloadText", fileName, content);
}
