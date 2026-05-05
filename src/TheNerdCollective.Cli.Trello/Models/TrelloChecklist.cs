using System.Text.Json.Serialization;

namespace TheNerdCollective.Cli.Trello.Models;

internal sealed class TrelloChecklist
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("checkItems")]
    public List<TrelloCheckItem>? CheckItems { get; set; }
}