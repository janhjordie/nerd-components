// Licensed under the Apache License, Version 2.0.
// See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace TheNerdCollective.MudComponents;

/// <summary>
/// A Blazor rich-text editor component wrapping Quill with MudBlazor styling.
/// </summary>
public partial class MudQuillEditor : IAsyncDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = null!;

    private string _elementId = $"mud-quill-{Guid.NewGuid():N}";
    private DotNetObjectReference<MudQuillEditor>? _objRef;
    private bool _initialized;
    private bool _isHtmlMode;
    private object? _previousToolbar;
    private string _sourceValue = string.Empty;
    private string _lastKnownValue = string.Empty;

    private string EditorCssClass => _isHtmlMode
        ? "mud-quill-editor mud-quill-editor-hidden"
        : "mud-quill-editor";

    /// <summary>
    /// Gets or sets the HTML content of the editor.
    /// </summary>
    [Parameter] public string? Value { get; set; }

    /// <summary>
    /// Fires when the editor content changes.
    /// </summary>
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets whether the editor is read-only.
    /// </summary>
    [Parameter] public bool ReadOnly { get; set; }

    /// <summary>
    /// Gets or sets the Quill theme ("snow" or "bubble").
    /// </summary>
    [Parameter] public string Theme { get; set; } = "snow";

    /// <summary>
    /// Gets or sets the minimum height of the editor (CSS value).
    /// </summary>
    [Parameter] public string? MinHeight { get; set; }

    /// <summary>
    /// Gets or sets the maximum height of the editor (CSS value). Default is 150px.
    /// </summary>
    [Parameter] public string MaxHeight { get; set; } = "150px";

    /// <summary>
    /// Gets or sets the toolbar modules. Default includes bold, italic, underline, lists, link, and image.
    /// </summary>
    [Parameter] public object? Toolbar { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text shown when editor is empty.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets whether users can switch between rich text and raw HTML editing.
    /// </summary>
    [Parameter] public bool EnableHtmlToggle { get; set; }

    // === INLINE FORMATS ===

    /// <summary>
    /// Gets or sets whether background color format is enabled.
    /// </summary>
    [Parameter] public bool? EnableBackground { get; set; }

    /// <summary>
    /// Gets or sets whether bold format is enabled.
    /// </summary>
    [Parameter] public bool? EnableBold { get; set; }

    /// <summary>
    /// Gets or sets whether color format is enabled.
    /// </summary>
    [Parameter] public bool? EnableColor { get; set; }

    /// <summary>
    /// Gets or sets whether font format is enabled.
    /// </summary>
    [Parameter] public bool? EnableFont { get; set; }

    /// <summary>
    /// Gets or sets whether inline code format is enabled.
    /// </summary>
    [Parameter] public bool? EnableCode { get; set; }

    /// <summary>
    /// Gets or sets whether italic format is enabled.
    /// </summary>
    [Parameter] public bool? EnableItalic { get; set; }

    /// <summary>
    /// Gets or sets whether link format is enabled.
    /// </summary>
    [Parameter] public bool? EnableLink { get; set; }

    /// <summary>
    /// Gets or sets whether size format is enabled.
    /// </summary>
    [Parameter] public bool? EnableSize { get; set; }

    /// <summary>
    /// Gets or sets whether strikethrough format is enabled.
    /// </summary>
    [Parameter] public bool? EnableStrike { get; set; }

    /// <summary>
    /// Gets or sets whether superscript/subscript format is enabled.
    /// </summary>
    [Parameter] public bool? EnableScript { get; set; }

    /// <summary>
    /// Gets or sets whether underline format is enabled.
    /// </summary>
    [Parameter] public bool? EnableUnderline { get; set; }

    // === BLOCK FORMATS ===

    /// <summary>
    /// Gets or sets whether blockquote format is enabled.
    /// </summary>
    [Parameter] public bool? EnableBlockquote { get; set; }

    /// <summary>
    /// Gets or sets whether header format is enabled.
    /// </summary>
    [Parameter] public bool? EnableHeader { get; set; }

    /// <summary>
    /// Gets or sets whether indent format is enabled.
    /// </summary>
    [Parameter] public bool? EnableIndent { get; set; }

    /// <summary>
    /// Gets or sets whether list format is enabled.
    /// </summary>
    [Parameter] public bool? EnableList { get; set; }

    /// <summary>
    /// Gets or sets whether text alignment format is enabled.
    /// </summary>
    [Parameter] public bool? EnableAlign { get; set; }

    /// <summary>
    /// Gets or sets whether text direction format is enabled.
    /// </summary>
    [Parameter] public bool? EnableDirection { get; set; }

    /// <summary>
    /// Gets or sets whether code block format is enabled.
    /// </summary>
    [Parameter] public bool? EnableCodeBlock { get; set; }

    // === EMBED FORMATS ===

    /// <summary>
    /// Gets or sets whether image embed format is enabled.
    /// </summary>
    [Parameter] public bool? EnableImage { get; set; }

    // === FORMAT GROUPS ===

    /// <summary>
    /// Gets or sets whether all inline formats are enabled. When set, overrides individual inline format settings.
    /// Includes: background, bold, color, font, code, italic, link, size, strike, script, underline.
    /// </summary>
    [Parameter] public bool? EnableAllInlineFormats { get; set; }

    /// <summary>
    /// Gets or sets whether all block formats are enabled. When set, overrides individual block format settings.
    /// Includes: blockquote, header, indent, list, align, direction, code-block.
    /// </summary>
    [Parameter] public bool? EnableAllBlockFormats { get; set; }

    /// <summary>
    /// Gets or sets whether all embed formats are enabled. When set, overrides individual embed format settings.
    /// Includes: image.
    /// </summary>
    [Parameter] public bool? EnableAllEmbedFormats { get; set; }

    /// <summary>
    /// Gets or sets whether all formats are enabled. When set, overrides all other format settings.
    /// </summary>
    [Parameter] public bool? EnableAllFormats { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        var currentValue = Value ?? string.Empty;
        _sourceValue = currentValue;

        if (_initialized)
        {
            // Update read-only state
            await JS.InvokeVoidAsync("mudQuillEditor.setReadOnly", _elementId, ReadOnly);

            // Update placeholder
            await JS.InvokeVoidAsync("mudQuillEditor.setPlaceholder", _elementId, Placeholder ?? string.Empty);

            if (!_isHtmlMode && currentValue != _lastKnownValue)
            {
                await JS.InvokeVoidAsync("mudQuillEditor.setHtml", _elementId, currentValue);
                _lastKnownValue = currentValue;
            }
        }

        await base.OnParametersSetAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_initialized && firstRender)
        {
            if (_objRef == null)
                _objRef = DotNetObjectReference.Create(this);

            const int maxAttempts = 6;
            int attempt = 0;

            while (attempt < maxAttempts && !_initialized)
            {
                attempt++;

                try
                {
                    await JS.InvokeVoidAsync("mudQuillEditor.initialize", _elementId, _objRef, new { readOnly = ReadOnly, theme = Theme, value = Value, minHeight = MinHeight, maxHeight = MaxHeight, toolbar = Toolbar, placeholder = Placeholder, formats = BuildFormatsArray() });
                    _initialized = true;
                    _previousToolbar = Toolbar;
                    _lastKnownValue = Value ?? string.Empty;
                    break;
                }
                catch
                {
                    if (attempt < maxAttempts)
                    {
                        await Task.Delay(500);
                    }
                }
            }
        }
        else if (_initialized && !firstRender)
        {
            // Reinitialize if toolbar changes (toolbar can't be updated live)
            if (!ToolbarEqual(_previousToolbar, Toolbar))
            {
                _previousToolbar = Toolbar;
                await JS.InvokeVoidAsync("mudQuillEditor.dispose", _elementId);
                _initialized = false;

                if (_objRef == null)
                    _objRef = DotNetObjectReference.Create(this);

                try
                {
                    await JS.InvokeVoidAsync("mudQuillEditor.initialize", _elementId, _objRef, new { readOnly = ReadOnly, theme = Theme, value = Value, minHeight = MinHeight, maxHeight = MaxHeight, toolbar = Toolbar, placeholder = Placeholder, formats = BuildFormatsArray() });
                    _initialized = true;
                    _lastKnownValue = Value ?? string.Empty;
                }
                catch
                {
                    // Ignore errors
                }
            }
        }
    }

    private bool ToolbarEqual(object? a, object? b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a == null || b == null) return a == b;
        return System.Text.Json.JsonSerializer.Serialize(a) == System.Text.Json.JsonSerializer.Serialize(b);
    }

    /// <summary>
    /// Builds the formats configuration array based on enabled format parameters.
    /// </summary>
    private string[]? BuildFormatsArray()
    {
        // If all formats are enabled, return null to use default (all formats)
        if (EnableAllFormats == true)
            return null;

        var formats = new List<string>();

        // Add inline formats
        if (EnableAllInlineFormats == true)
        {
            formats.AddRange(new[] { "background", "bold", "color", "font", "code", "italic", "link", "size", "strike", "script", "underline" });
        }
        else
        {
            if (EnableBackground == true) formats.Add("background");
            if (EnableBold == true) formats.Add("bold");
            if (EnableColor == true) formats.Add("color");
            if (EnableFont == true) formats.Add("font");
            if (EnableCode == true) formats.Add("code");
            if (EnableItalic == true) formats.Add("italic");
            if (EnableLink == true) formats.Add("link");
            if (EnableSize == true) formats.Add("size");
            if (EnableStrike == true) formats.Add("strike");
            if (EnableScript == true) formats.Add("script");
            if (EnableUnderline == true) formats.Add("underline");
        }

        // Add block formats
        if (EnableAllBlockFormats == true)
        {
            formats.AddRange(new[] { "blockquote", "header", "indent", "list", "align", "direction", "code-block" });
        }
        else
        {
            if (EnableBlockquote == true) formats.Add("blockquote");
            if (EnableHeader == true) formats.Add("header");
            if (EnableIndent == true) formats.Add("indent");
            if (EnableList == true) formats.Add("list");
            if (EnableAlign == true) formats.Add("align");
            if (EnableDirection == true) formats.Add("direction");
            if (EnableCodeBlock == true) formats.Add("code-block");
        }

        // Add embed formats
        if (EnableAllEmbedFormats == true)
        {
            formats.Add("image");
        }
        else
        {
            if (EnableImage == true) formats.Add("image");
        }

        // Return null if no formats specified (use default), otherwise return the array
        return formats.Count > 0 ? formats.ToArray() : null;
    }

    /// <summary>
    /// Sets the editor HTML content programmatically.
    /// </summary>
    public async Task SetHtmlAsync(string? html)
    {
        if (!_initialized) return;
        await JS.InvokeVoidAsync("mudQuillEditor.setHtml", _elementId, html ?? string.Empty);
        Value = html;
        _sourceValue = html ?? string.Empty;
        _lastKnownValue = _sourceValue;
        await ValueChanged.InvokeAsync(Value);
    }

    /// <summary>
    /// Gets the editor HTML content programmatically.
    /// </summary>
    public async Task<string?> GetHtmlAsync()
    {
        if (!_initialized) return Value;
        return await JS.InvokeAsync<string>("mudQuillEditor.getHtml", _elementId);
    }

    /// <summary>
    /// Invoked by JavaScript when the editor content changes.
    /// </summary>
    [JSInvokable]
    public async Task NotifyValueChanged(string html)
    {
        Value = html;
        _sourceValue = html ?? string.Empty;
        _lastKnownValue = _sourceValue;
        await ValueChanged.InvokeAsync(Value);
    }

    private async Task OnHtmlModeChangedAsync(bool isHtmlMode)
    {
        if (_isHtmlMode == isHtmlMode)
            return;

        if (isHtmlMode)
        {
            _sourceValue = await GetHtmlAsync() ?? Value ?? string.Empty;
            _lastKnownValue = _sourceValue;
            _isHtmlMode = true;
            return;
        }

        _isHtmlMode = false;
        await SetHtmlAsync(_sourceValue);
    }

    private async Task OnSourceValueChangedAsync(string? value)
    {
        _sourceValue = value ?? string.Empty;
        Value = _sourceValue;
        _lastKnownValue = _sourceValue;
        await ValueChanged.InvokeAsync(Value);
    }

    /// <summary>
    /// Disposes the editor and cleans up resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_initialized)
        {
            try
            {
                await JS.InvokeVoidAsync("mudQuillEditor.dispose", _elementId);
            }
            catch (JSDisconnectedException)
            {
                // Circuit was disconnected; JS interop is no longer available
                // This is expected during page unload/reload
            }
            catch
            {
                // Ignore other errors during disposal
            }
            finally
            {
                _objRef?.Dispose();
            }
        }
    }
}