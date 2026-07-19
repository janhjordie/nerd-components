using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TheNerdCollective.MudComponents.PlayBook;

public partial class NerdPlayBookPreview
{
    [Parameter]
    public NerdPlayBookPlaygroundState? Playground { get; set; }

    [Parameter]
    public string? TokenClass { get; set; }

    private bool IsPlayground => Playground is not null;

    private string? Tc => string.IsNullOrWhiteSpace(TokenClass) ? null : TokenClass;

    private string PText(string key, string fallback) =>
        IsPlayground ? Playground!.GetString(key, fallback) : fallback;

    private bool PBool(string key, bool fallback = false) =>
        IsPlayground ? Playground!.GetBool(key, fallback) : fallback;

    private Variant PVariant(Variant fallback = Variant.Filled) =>
        IsPlayground ? Playground!.GetEnum("variant", fallback) : fallback;

    private Size PSize(Size fallback = Size.Small) =>
        IsPlayground ? Playground!.GetEnum("size", fallback) : fallback;

    private Severity PSeverity(Severity fallback = Severity.Info) =>
        IsPlayground ? Playground!.GetEnum("severity", fallback) : fallback;

    private Typo PTypo(Typo fallback = Typo.h6) =>
        IsPlayground ? Playground!.GetEnum("typo", fallback) : fallback;

    private int PInt(string key, int fallback) =>
        IsPlayground ? Playground!.GetInt(key, fallback) : fallback;

    private bool PDense(bool fallback = true) =>
        IsPlayground ? Playground!.GetBool("dense", fallback) : fallback;
}
