using Bunit;
using Microsoft.Extensions.DependencyInjection;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.Shared.Tests;

public class NerdClipboardButtonTests : MudComponentTestContext
{
    [Fact]
    public void Clipboard_button_renders_label_and_invokes_js_copy()
    {
        Services.AddScoped<NerdClipboardService>();

        var cut = RenderComponent<NerdClipboardButton>(parameters => parameters
            .Add(p => p.Text, ".demo-forest")
            .Add(p => p.Label, "Copy class"));

        Assert.Contains("Copy class", cut.Markup);

        cut.Find("button").Click();

        JSInterop.VerifyInvoke("nerdShared.copyText");
    }
}
