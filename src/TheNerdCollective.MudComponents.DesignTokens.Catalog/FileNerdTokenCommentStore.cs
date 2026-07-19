using System.Text.Json;

namespace TheNerdCollective.MudComponents.DesignTokens;

public sealed class FileNerdTokenCommentStore : INerdTokenCommentStore
{
    private readonly string _directory;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public FileNerdTokenCommentStore(string directory = "App_Data/token-comments")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        _directory = directory;
    }

    public async Task<IReadOnlyDictionary<string, string>> LoadAsync(
        string clientId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        var path = GetPath(clientId);
        if (!File.Exists(path))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        await using var stream = File.OpenRead(path);
        var comments = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream, _jsonOptions, cancellationToken);
        return comments ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public async Task SaveAsync(
        string clientId,
        IReadOnlyDictionary<string, string> comments,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentNullException.ThrowIfNull(comments);
        Directory.CreateDirectory(_directory);
        var path = GetPath(clientId);
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, comments, _jsonOptions, cancellationToken);
    }

    private string GetPath(string clientId) =>
        Path.Combine(_directory, $"{Sanitize(clientId)}.json");

    private static string Sanitize(string clientId) =>
        string.Concat(clientId.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
}
