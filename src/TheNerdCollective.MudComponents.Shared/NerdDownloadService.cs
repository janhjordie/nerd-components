using Microsoft.JSInterop;

namespace TheNerdCollective.MudComponents.Shared;

/// <summary>
/// Downloads generated text as a file through <c>nerd-shared.js</c>.
/// </summary>
public sealed class NerdDownloadService(IJSRuntime jsRuntime)
{
    /// <summary>Triggers a browser download for the provided file name and text content.</summary>
    public ValueTask DownloadTextAsync(string fileName, string content) =>
        jsRuntime.InvokeVoidAsync("nerdShared.downloadText", fileName, content);
}
