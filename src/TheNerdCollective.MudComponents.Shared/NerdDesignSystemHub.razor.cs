using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MudBlazor;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.Shared;

public partial class NerdDesignSystemHub
{
    [Inject]
    private NerdDesignSystemOptions Options { get; set; } = default!;

    [Inject]
    private IWebHostEnvironment HostEnvironment { get; set; } = default!;

    [Inject]
    private INerdBrandPackImportSink? ImportSink { get; set; }

    private string? _importStatus;
    private Color _importStatusColor = Color.Default;

    private bool IsAvailable =>
        Options.EnableHubPage &&
        (!Options.RestrictHubToDevelopment || HostEnvironment.IsDevelopment());

    private string Ui(string semanticAlias) => NerdDesignSystemUi.TokenClass(Options, semanticAlias);

    private async Task OnImportAsync(InputFileChangeEventArgs args)
    {
        if (ImportSink is null)
        {
            _importStatus = "Token import is not configured.";
            _importStatusColor = Color.Warning;
            return;
        }

        var file = args.File;
        if (file is null)
        {
            return;
        }

        await using var stream = file.OpenReadStream(maxAllowedSize: 2_000_000);
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        var result = await ImportSink.ImportAsync(json);
        _importStatus = result.Message;
        _importStatusColor = result.Success ? Color.Success : Color.Error;
    }
}
