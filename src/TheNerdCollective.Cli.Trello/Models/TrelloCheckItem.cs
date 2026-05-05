using System.Text.Json.Serialization;

namespace TheNerdCollective.Cli.Trello.Models;

internal sealed class TrelloCheckItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;
}