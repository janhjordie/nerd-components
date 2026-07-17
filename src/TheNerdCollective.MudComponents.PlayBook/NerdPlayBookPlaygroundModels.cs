namespace TheNerdCollective.MudComponents.PlayBook;

public enum NerdPlayBookPropType
{
    Boolean,
    Text,
    Select
}

/// <summary>
/// Describes a single editable property in the component playground.
/// </summary>
public sealed record NerdPlayBookPropDefinition(
    string Key,
    string Label,
    NerdPlayBookPropType Type,
    string DefaultValue,
    IReadOnlyList<string>? Options = null);

/// <summary>
/// Mutable playground values for a component preview.
/// </summary>
public sealed class NerdPlayBookPlaygroundState
{
    private readonly Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);

    public NerdPlayBookPlaygroundState(IEnumerable<NerdPlayBookPropDefinition> definitions)
    {
        foreach (var definition in definitions)
        {
            _values[definition.Key] = definition.DefaultValue;
        }
    }

    public IReadOnlyDictionary<string, string> Values => _values;

    public string GetString(string key, string defaultValue = "") =>
        _values.TryGetValue(key, out var value) ? value : defaultValue;

    public bool GetBool(string key, bool defaultValue = false) =>
        _values.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed)
            ? parsed
            : defaultValue;

    public TEnum GetEnum<TEnum>(string key, TEnum defaultValue)
        where TEnum : struct, Enum
    {
        if (!_values.TryGetValue(key, out var value))
        {
            return defaultValue;
        }

        return Enum.TryParse<TEnum>(value, true, out var parsed) ? parsed : defaultValue;
    }

    public int GetInt(string key, int defaultValue = 0) =>
        _values.TryGetValue(key, out var value) && int.TryParse(value, out var parsed)
            ? parsed
            : defaultValue;

    public void Set(string key, string value) => _values[key] = value;

    public void Reset(IEnumerable<NerdPlayBookPropDefinition> definitions)
    {
        _values.Clear();
        foreach (var definition in definitions)
        {
            _values[definition.Key] = definition.DefaultValue;
        }
    }
}
