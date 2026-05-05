using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TheNerdCollective.Cli.Trello.Models;

var exitCode = await TrelloCli.RunAsync(args);
return exitCode;

internal static class TrelloCli
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    public static async Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0 || IsHelpCommand(args[0]))
        {
            PrintHelp();
            return 0;
        }

        try
        {
            var options = TrelloCliOptions.FromEnvironment();
            using var httpClient = new HttpClient();
            var client = new TrelloApiClient(httpClient, options, SerializerOptions);
            var command = args[0].ToLowerInvariant();
            var commandOptions = ParseOptions(args.Skip(1).ToArray());

            return command switch
            {
                "checklists" => await HandleListChecklistsAsync(client, commandOptions),
                "ensure-checklist" => await HandleEnsureChecklistAsync(client, commandOptions),
                "add-item" => await HandleAddItemAsync(client, commandOptions),
                "set-item-state" => await HandleSetItemStateAsync(client, commandOptions),
                "comment" => await HandleCommentAsync(client, commandOptions),
                _ => Fail($"Unknown command '{command}'. Run 'tnc-trello help' for usage.")
            };
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    private static async Task<int> HandleListChecklistsAsync(TrelloApiClient client, Dictionary<string, string> options)
    {
        var cardId = RequireOption(options, "card");
        var checklists = await client.GetCardChecklistsAsync(cardId);

        foreach (var checklist in checklists)
        {
            Console.WriteLine($"Checklist: {checklist.Name} ({checklist.Id})");
            foreach (var item in checklist.CheckItems ?? new List<TrelloCheckItem>())
            {
                Console.WriteLine($"  - [{(item.State == "complete" ? 'x' : ' ')}] {item.Name} ({item.Id})");
            }
        }

        return 0;
    }

    private static async Task<int> HandleEnsureChecklistAsync(TrelloApiClient client, Dictionary<string, string> options)
    {
        var cardId = RequireOption(options, "card");
        var checklistName = RequireOption(options, "name");
        var checklist = await client.EnsureChecklistAsync(cardId, checklistName);

        Console.WriteLine(JsonSerializer.Serialize(checklist, SerializerOptions));
        return 0;
    }

    private static async Task<int> HandleAddItemAsync(TrelloApiClient client, Dictionary<string, string> options)
    {
        var itemName = RequireOption(options, "name");
        var checklistId = await ResolveChecklistIdAsync(client, options);
        var item = await client.AddChecklistItemAsync(checklistId, itemName);

        Console.WriteLine(JsonSerializer.Serialize(item, SerializerOptions));
        return 0;
    }

    private static async Task<int> HandleSetItemStateAsync(TrelloApiClient client, Dictionary<string, string> options)
    {
        var cardId = RequireOption(options, "card");
        var itemId = RequireOption(options, "item");
        var state = RequireOption(options, "state").ToLowerInvariant();
        if (state is not ("complete" or "incomplete"))
        {
            throw new InvalidOperationException("--state must be 'complete' or 'incomplete'.");
        }

        var item = await client.UpdateCardCheckItemStateAsync(cardId, itemId, state);
        Console.WriteLine(JsonSerializer.Serialize(item, SerializerOptions));
        return 0;
    }

    private static async Task<int> HandleCommentAsync(TrelloApiClient client, Dictionary<string, string> options)
    {
        var cardId = RequireOption(options, "card");
        var text = RequireOption(options, "text");
        var action = await client.AddCommentAsync(cardId, text);

        Console.WriteLine(JsonSerializer.Serialize(action, SerializerOptions));
        return 0;
    }

    private static async Task<string> ResolveChecklistIdAsync(TrelloApiClient client, Dictionary<string, string> options)
    {
        if (options.TryGetValue("checklist-id", out var checklistId) && !string.IsNullOrWhiteSpace(checklistId))
        {
            return checklistId;
        }

        var cardId = RequireOption(options, "card");
        var checklistName = RequireOption(options, "checklist");
        var checklist = await client.EnsureChecklistAsync(cardId, checklistName);
        return checklist.Id;
    }

    private static Dictionary<string, string> ParseOptions(string[] args)
    {
        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < args.Length; index++)
        {
            var current = args[index];
            if (!current.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            var key = current[2..];
            if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                options[key] = "true";
                continue;
            }

            options[key] = args[++index];
        }

        return options;
    }

    private static string RequireOption(Dictionary<string, string> options, string name)
    {
        if (options.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        throw new InvalidOperationException($"Missing required option '--{name}'.");
    }

    private static bool IsHelpCommand(string command)
    {
        return string.Equals(command, "help", StringComparison.OrdinalIgnoreCase)
            || string.Equals(command, "--help", StringComparison.OrdinalIgnoreCase)
            || string.Equals(command, "-h", StringComparison.OrdinalIgnoreCase);
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine(message);
        return 1;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("TheNerdCollective Trello CLI MVP");
        Console.WriteLine();
        Console.WriteLine("Environment:");
        Console.WriteLine("  TRELLO_KEY    Required Trello API key");
        Console.WriteLine("  TRELLO_TOKEN  Required Trello API token");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  tnc-trello checklists --card <cardId>");
        Console.WriteLine("  tnc-trello ensure-checklist --card <cardId> --name <checklistName>");
        Console.WriteLine("  tnc-trello add-item --card <cardId> --checklist <checklistName> --name <itemText>");
        Console.WriteLine("  tnc-trello add-item --checklist-id <checklistId> --name <itemText>");
        Console.WriteLine("  tnc-trello set-item-state --card <cardId> --item <checkItemId> --state complete|incomplete");
        Console.WriteLine("  tnc-trello comment --card <cardId> --text <commentText>");
    }
}

internal sealed class TrelloApiClient
{
    private readonly HttpClient _httpClient;
    private readonly TrelloCliOptions _options;
    private readonly JsonSerializerOptions _serializerOptions;

    public TrelloApiClient(HttpClient httpClient, TrelloCliOptions options, JsonSerializerOptions serializerOptions)
    {
        _httpClient = httpClient;
        _options = options;
        _serializerOptions = serializerOptions;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "TheNerdCollective.Cli.Trello/0.1.0");
    }

    public async Task<List<TrelloChecklist>> GetCardChecklistsAsync(string cardId)
    {
        return await GetAsync<List<TrelloChecklist>>($"cards/{cardId}/checklists");
    }

    public async Task<TrelloChecklist> EnsureChecklistAsync(string cardId, string name)
    {
        var checklists = await GetCardChecklistsAsync(cardId);
        var existing = checklists.FirstOrDefault(checklist => string.Equals(checklist.Name, name, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            return existing;
        }

        return await PostAsync<TrelloChecklist>($"cards/{cardId}/checklists", new Dictionary<string, string>
        {
            ["name"] = name
        });
    }

    public async Task<TrelloCheckItem> AddChecklistItemAsync(string checklistId, string name)
    {
        return await PostAsync<TrelloCheckItem>($"checklists/{checklistId}/checkItems", new Dictionary<string, string>
        {
            ["name"] = name
        });
    }

    public async Task<TrelloCheckItem> UpdateCardCheckItemStateAsync(string cardId, string itemId, string state)
    {
        return await PutAsync<TrelloCheckItem>($"cards/{cardId}/checkItem/{itemId}", new Dictionary<string, string>
        {
            ["state"] = state
        });
    }

    public async Task<TrelloAction> AddCommentAsync(string cardId, string text)
    {
        return await PostAsync<TrelloAction>($"cards/{cardId}/actions/comments", new Dictionary<string, string>
        {
            ["text"] = text
        });
    }

    private async Task<T> GetAsync<T>(string path)
    {
        using var response = await _httpClient.GetAsync(BuildUri(path, null));
        return await ReadResponseAsync<T>(response);
    }

    private async Task<T> PostAsync<T>(string path, Dictionary<string, string>? query)
    {
        using var response = await _httpClient.PostAsync(BuildUri(path, query), content: null);
        return await ReadResponseAsync<T>(response);
    }

    private async Task<T> PutAsync<T>(string path, Dictionary<string, string>? query)
    {
        using var response = await _httpClient.PutAsync(BuildUri(path, query), content: null);
        return await ReadResponseAsync<T>(response);
    }

    private async Task<T> ReadResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Trello API call failed ({(int)response.StatusCode} {response.StatusCode}): {content}");
        }

        var payload = JsonSerializer.Deserialize<T>(content, _serializerOptions);
        return payload ?? throw new InvalidOperationException("Trello API returned an empty response payload.");
    }

    private string BuildUri(string path, Dictionary<string, string>? query)
    {
        var builder = new StringBuilder(path);
        builder.Append(path.Contains('?', StringComparison.Ordinal) ? '&' : '?');
        builder.Append($"key={Uri.EscapeDataString(_options.Key)}&token={Uri.EscapeDataString(_options.Token)}");

        if (query == null)
        {
            return builder.ToString();
        }

        foreach (var pair in query)
        {
            builder.Append('&');
            builder.Append(Uri.EscapeDataString(pair.Key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(pair.Value));
        }

        return builder.ToString();
    }
}

internal sealed class TrelloCliOptions
{
    public required string Key { get; init; }
    public required string Token { get; init; }
    public string BaseUrl { get; init; } = "https://api.trello.com/1/";

    public static TrelloCliOptions FromEnvironment()
    {
        var key = Environment.GetEnvironmentVariable("TRELLO_KEY");
        var token = Environment.GetEnvironmentVariable("TRELLO_TOKEN");
        var baseUrl = Environment.GetEnvironmentVariable("TRELLO_BASE_URL");

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("TRELLO_KEY environment variable is required.");
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("TRELLO_TOKEN environment variable is required.");
        }

        return new TrelloCliOptions
        {
            Key = key,
            Token = token,
            BaseUrl = string.IsNullOrWhiteSpace(baseUrl) ? "https://api.trello.com/1/" : EnsureTrailingSlash(baseUrl)
        };
    }

    private static string EnsureTrailingSlash(string value)
    {
        return value.EndsWith("/", StringComparison.Ordinal) ? value : value + "/";
    }
}