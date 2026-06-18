namespace TheNerdCollective.Blazor.ThemeKit;

public interface IThemeEditorGate
{
    bool IsEditorEnabled { get; }

    bool IsSwitcherEnabled { get; }
}
