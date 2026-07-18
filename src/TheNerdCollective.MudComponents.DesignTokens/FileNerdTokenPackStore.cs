using System.Text.Json;

namespace TheNerdCollective.MudComponents.DesignTokens;

public sealed class FileNerdTokenPackStore : INerdTokenPackStore
{
    private readonly string _directory;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public FileNerdTokenPackStore(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        _directory = directory;
    }

    public async Task SaveAsync(NerdTokenPack pack, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pack);
        var path = GetPath(pack.ClientId);
        Directory.CreateDirectory(_directory);
        await File.WriteAllTextAsync(path, pack.ToJson(_jsonOptions), cancellationToken);
    }

    public async Task<NerdTokenPack?> LoadAsync(
        string clientId,
        CancellationToken cancellationToken = default)
    {
        var path = GetPath(clientId);
        if (!File.Exists(path))
        {
            return null;
        }

        return NerdTokenPack.FromJson(
            await File.ReadAllTextAsync(path, cancellationToken),
            _jsonOptions);
    }

    public Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_directory))
        {
            return Task.FromResult<IReadOnlyList<string>>([]);
        }

        IReadOnlyList<string> clients = Directory.EnumerateFiles(_directory, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray()!;
        return Task.FromResult(clients);
    }

    private string GetPath(string clientId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        if (clientId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            clientId.Contains("..", StringComparison.Ordinal))
        {
            throw new ArgumentException("ClientId is not a safe file name.", nameof(clientId));
        }

        return Path.Combine(_directory, $"{clientId}.json");
    }
}
