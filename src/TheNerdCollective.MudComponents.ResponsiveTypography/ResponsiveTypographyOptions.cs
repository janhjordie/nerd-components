namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public sealed class ResponsiveTypographyOptions
{
    private readonly HashSet<string> _configuredRoles = new(StringComparer.OrdinalIgnoreCase);

    public string? LineHeight { get; set; }
    public string? LetterSpacing { get; set; }
    public string? FontWeight { get; set; }

    public IReadOnlySet<string> ConfiguredRoles => _configuredRoles;

    public string? Default { get => _default; set => SetRole(nameof(Default), ref _default, value); }
    public string? H1 { get => _h1; set => SetRole(nameof(H1), ref _h1, value); }
    public string? H2 { get => _h2; set => SetRole(nameof(H2), ref _h2, value); }
    public string? H3 { get => _h3; set => SetRole(nameof(H3), ref _h3, value); }
    public string? H4 { get => _h4; set => SetRole(nameof(H4), ref _h4, value); }
    public string? H5 { get => _h5; set => SetRole(nameof(H5), ref _h5, value); }
    public string? H6 { get => _h6; set => SetRole(nameof(H6), ref _h6, value); }
    public string? Subtitle1 { get => _subtitle1; set => SetRole(nameof(Subtitle1), ref _subtitle1, value); }
    public string? Subtitle2 { get => _subtitle2; set => SetRole(nameof(Subtitle2), ref _subtitle2, value); }
    public string? Body1 { get => _body1; set => SetRole(nameof(Body1), ref _body1, value); }
    public string? Body2 { get => _body2; set => SetRole(nameof(Body2), ref _body2, value); }
    public string? Button { get => _button; set => SetRole(nameof(Button), ref _button, value); }
    public string? Caption { get => _caption; set => SetRole(nameof(Caption), ref _caption, value); }
    public string? Overline { get => _overline; set => SetRole(nameof(Overline), ref _overline, value); }

    private string? _default;
    private string? _h1;
    private string? _h2;
    private string? _h3;
    private string? _h4;
    private string? _h5;
    private string? _h6;
    private string? _subtitle1;
    private string? _subtitle2;
    private string? _body1;
    private string? _body2;
    private string? _button;
    private string? _caption;
    private string? _overline;

    private void SetRole(string role, ref string? field, string? value)
    {
        field = value;
        if (value is not null)
        {
            _configuredRoles.Add(role);
        }
        else
        {
            _configuredRoles.Remove(role);
        }
    }
}
