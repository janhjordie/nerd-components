using System.Text.Json.Serialization;

namespace TheNerdCollective.MudComponents.Changelog;

public sealed class NerdChangelogEntry
{
    [JsonPropertyName("changeType")]
    public string ChangeType { get; set; } = "patch";

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonIgnore]
    public string ProcessedDescription { get; set; } = string.Empty;

    [JsonIgnore]
    public string Version { get; set; } = string.Empty;

    [JsonIgnore]
    public DateTime ParsedDateTime
    {
        get
        {
            if (DateTime.TryParse($"{Date} {Time}", out var dt))
            {
                return dt;
            }

            return DateTime.TryParse(Date, out var d) ? d : DateTime.MinValue;
        }
    }
}
