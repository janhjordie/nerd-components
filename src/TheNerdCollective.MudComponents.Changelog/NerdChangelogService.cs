using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TheNerdCollective.MudComponents.Changelog;

/// <summary>
/// Reads multi-file <c>changelog*.json</c>, aggregates entries, and calculates semver.
/// Agents write JSON; this service is read-only for UI.
/// </summary>
public sealed class NerdChangelogService
{
    private readonly NerdChangelogOptions _options;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<NerdChangelogService> _logger;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private List<NerdChangelogEntry>? _entries;
    private string? _resolvedDataDirectory;

    private static readonly Encoding[] CandidateEncodings =
    [
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
        new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true),
        new UnicodeEncoding(bigEndian: true, byteOrderMark: true, throwOnInvalidBytes: true)
    ];

    public NerdChangelogService(
        NerdChangelogOptions options,
        IHostEnvironment environment,
        ILogger<NerdChangelogService> logger)
    {
        _options = options;
        _environment = environment;
        _logger = logger;
    }

    public string DataDirectory => _resolvedDataDirectory ??= ResolveDataDirectory();

    public int MaxEntriesPerFile => Math.Max(1, _options.MaxEntriesPerFile);

    public async Task<IReadOnlyList<NerdChangelogEntry>> GetEntriesAsync(bool forceReload = false)
    {
        if (_entries is not null && !forceReload)
        {
            return _entries;
        }

        await _initLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_entries is not null && !forceReload)
            {
                return _entries;
            }

            var localEntries = new List<NerdChangelogEntry>();
            var dataDirectory = DataDirectory;
            if (!Directory.Exists(dataDirectory))
            {
                _logger.LogWarning("Changelog data directory not found: {DataDirectory}", dataDirectory);
                _entries = localEntries;
                return _entries;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };

            foreach (var file in EnumerateChangelogFiles(dataDirectory))
            {
                try
                {
                    var json = await ReadJsonFileAsync(file).ConfigureAwait(false);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        continue;
                    }

                    var parsed = JsonSerializer.Deserialize<List<NerdChangelogEntry>>(json, options);
                    if (parsed is not null)
                    {
                        localEntries.AddRange(parsed);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed reading changelog file {File}", Path.GetFileName(file));
                }
            }

            if (localEntries.Count > 0)
            {
                var sortedForVersions = localEntries
                    .OrderBy(e => e.ParsedDateTime)
                    .ToList();
                CalculateVersions(sortedForVersions);

                foreach (var entry in localEntries)
                {
                    entry.ProcessedDescription = ProcessDescription(entry.Description);
                }

                localEntries = localEntries
                    .OrderByDescending(e => e.ParsedDateTime)
                    .ToList();
            }

            _entries = localEntries;
            return _entries;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<string> GetLatestVersionAsync(bool forceReload = false)
    {
        var list = await GetEntriesAsync(forceReload).ConfigureAwait(false);
        return list.Count > 0 ? list[0].Version : "0.0.0";
    }

    /// <summary>Active write target = highest numbered <c>changelog*.json</c> (or <c>changelog.json</c>).</summary>
    public string? GetActiveFilePath()
    {
        var dataDirectory = DataDirectory;
        if (!Directory.Exists(dataDirectory))
        {
            return null;
        }

        var files = EnumerateChangelogFiles(dataDirectory).ToList();
        if (files.Count == 0)
        {
            return Path.Combine(dataDirectory, "changelog.json");
        }

        return files
            .OrderByDescending(GetChangelogSuffix)
            .ThenBy(path => path, StringComparer.OrdinalIgnoreCase)
            .First();
    }

    public async Task<(string Path, int Count)> GetActiveFileStatusAsync()
    {
        var path = GetActiveFilePath()
            ?? Path.Combine(DataDirectory, "changelog.json");

        if (!File.Exists(path))
        {
            return (path, 0);
        }

        var json = await ReadJsonFileAsync(path).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(json))
        {
            return (path, 0);
        }

        var entries = JsonSerializer.Deserialize<List<NerdChangelogEntry>>(json);
        return (path, entries?.Count ?? 0);
    }

    public bool ShouldRotate(int entryCount) => entryCount >= MaxEntriesPerFile;

    public string NextFilePathAfter(string activePath)
    {
        var dir = Path.GetDirectoryName(activePath) ?? DataDirectory;
        var next = GetChangelogSuffix(activePath) + 1;
        return Path.Combine(dir, next == 0 ? "changelog-1.json" : $"changelog-{next}.json");
    }

    private string ResolveDataDirectory()
    {
        if (!string.IsNullOrWhiteSpace(_options.DataDirectory))
        {
            return Path.GetFullPath(_options.DataDirectory);
        }

        var appDirectory = AppContext.BaseDirectory;
        var candidates = new[]
        {
            Path.Combine(appDirectory, "data"),
            Path.Combine(_environment.ContentRootPath, "data"),
            Path.Combine(appDirectory, "..", "data"),
            Path.Combine(appDirectory, "..", "..", "data")
        };

        return Path.GetFullPath(candidates.FirstOrDefault(Directory.Exists) ?? candidates[0]);
    }

    private static IEnumerable<string> EnumerateChangelogFiles(string dataDirectory) =>
        Directory.GetFiles(dataDirectory, "changelog*.json")
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);

    private static int GetChangelogSuffix(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        if (string.Equals(name, "changelog", StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (name.StartsWith("changelog-", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(name["changelog-".Length..], out var n))
        {
            return n;
        }

        return -1;
    }

    private static string ProcessDescription(string description)
    {
        if (string.IsNullOrEmpty(description))
        {
            return string.Empty;
        }

        var processed = description;
        processed = Regex.Replace(processed, @"\. (?=[A-Z])", ".<br><br> ");
        processed = Regex.Replace(processed, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
        processed = Regex.Replace(processed, @"^- (.+)$", "<li>$1</li>", RegexOptions.Multiline);
        processed = Regex.Replace(processed, @"(<li>.*?</li>(\n)?)+", match =>
            "<ul>" + match.Value.Replace("\n", "", StringComparison.Ordinal) + "</ul>");
        processed = processed.Replace("\n", "<br>", StringComparison.Ordinal);
        processed = Regex.Replace(processed, @"(<br>){2,}", "<br><br>");
        processed = Regex.Replace(
            processed,
            @"(https?://[^\s<]+)",
            match => $"<a href=\"{match.Value}\" target=\"_blank\" rel=\"noopener noreferrer\">{match.Value}</a>",
            RegexOptions.IgnoreCase);

        return processed;
    }

    private static async Task<string> ReadJsonFileAsync(string filePath)
    {
        var bytes = await File.ReadAllBytesAsync(filePath).ConfigureAwait(false);
        if (bytes.Length == 0)
        {
            return string.Empty;
        }

        foreach (var encoding in CandidateEncodings)
        {
            try
            {
                var json = encoding.GetString(bytes);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    return json;
                }
            }
            catch (DecoderFallbackException)
            {
                // try next
            }
        }

        throw new InvalidOperationException(
            $"Unable to decode changelog file '{Path.GetFileName(filePath)}' using supported UTF encodings.");
    }

    private static void CalculateVersions(List<NerdChangelogEntry> entries)
    {
        // Scheme (oldest → newest): major → X.0.0; patch → 0.X.0; minor → 0.0.X
        var major = 0;
        var middle = 0;
        var minor = 0;
        var original = new List<NerdChangelogEntry>(entries);

        foreach (var entry in entries)
        {
            switch (entry.ChangeType?.ToLowerInvariant())
            {
                case "major":
                    major++;
                    middle = 0;
                    minor = 0;
                    break;
                case "patch":
                    middle++;
                    minor = 0;
                    break;
                case "minor":
                    minor++;
                    break;
                default:
                    middle++;
                    minor = 0;
                    break;
            }

            entry.Version = $"{major}.{middle}.{minor}";
        }

        entries.Clear();
        entries.AddRange(original);
    }
}
