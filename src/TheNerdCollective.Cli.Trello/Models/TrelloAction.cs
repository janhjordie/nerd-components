using System.Text.Json.Serialization;

namespace TheNerdCollective.Cli.Trello.Models;

internal sealed class TrelloAction
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public DateTimeOffset Date { get; set; }
}