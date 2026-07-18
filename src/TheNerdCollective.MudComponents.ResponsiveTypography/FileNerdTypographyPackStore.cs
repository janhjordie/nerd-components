using System.Text.Json;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public sealed class FileNerdTypographyPackStore : INerdTypographyPackStore
{
    private readonly string _directory;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public FileNerdTypographyPackStore(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        _directory = directory;
    }

    public async Task SaveAsync(NerdTypographyPack pack, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pack);
        ArgumentException.ThrowIfNullOrWhiteSpace(pack.ClientId);
        Directory.CreateDirectory(_directory);
        var path = GetPath(pack.ClientId);
        await File.WriteAllTextAsync(path, pack.ToJson(_jsonOptions), cancellationToken);
    }

    public async Task<NerdTypographyPack?> LoadAsync(
        string clientId,
        CancellationToken cancellationToken = default)
    {
        var path = GetPath(clientId);
        if (!File.Exists(path))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);
        return NerdTypographyPack.FromJson(json, _jsonOptions);
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
