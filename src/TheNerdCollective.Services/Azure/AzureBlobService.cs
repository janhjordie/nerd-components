using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using TheNerdCollective.Helpers;

namespace TheNerdCollective.Services.Azure;

/// <summary>
/// Azure Blob Storage service for uploading, downloading, and managing blobs.
/// </summary>
public class AzureBlobService
{
    private readonly BlobContainerClient _blobContainer;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ConcurrentDictionary<string, BlobContainerClient> _containerClients = new(StringComparer.OrdinalIgnoreCase);

    public AzureBlobService(IOptions<AzureBlobOptions> options)
    {
        var config = options.Value;
        _blobServiceClient = new BlobServiceClient(config.ConnectionString);
        _blobContainer = GetOrCreateContainerClient(config.ContainerName);
    }

    private BlobContainerClient GetOrCreateContainerClient(string container)
    {
        container = container.ToLowerInvariant();
        return _containerClients.GetOrAdd(
            container,
            name => _blobServiceClient.GetBlobContainerClient(name));
    }

    private async Task<BlobContainerClient> EnsureContainerExistsAsync(string container)
    {
        var blobContainer = GetOrCreateContainerClient(container);
        await blobContainer.CreateIfNotExistsAsync();
        return blobContainer;
    }

    /// <summary>
    /// Uploads data to the default blob container.
    /// </summary>
    public async Task UploadAsync(byte[] data, string destinationPath)
    {
        var blobClient = _blobContainer.GetBlobClient(destinationPath);
        await blobClient.DeleteIfExistsAsync();
        using var stream = new MemoryStream(data);
        await blobClient.UploadAsync(stream, true);
    }

    /// <summary>
    /// Uploads data to a specific blob container.
    /// </summary>
    public async Task UploadAsync(byte[] data, string container, string destinationPath)
    {
        await UploadAsync(data, container, destinationPath, cacheControl: null);
    }

    /// <summary>
    /// Uploads data to a specific blob container with optional HTTP cache headers.
    /// </summary>
    public async Task UploadAsync(byte[] data, string container, string destinationPath, string? cacheControl)
    {
        await UploadAsync(data, container, destinationPath, cacheControl, contentType: null);
    }

    /// <summary>
    /// Uploads data to a specific blob container with optional HTTP cache and content-type headers.
    /// </summary>
    public async Task UploadAsync(
        byte[] data,
        string container,
        string destinationPath,
        string? cacheControl,
        string? contentType)
    {
        var blobContainer = await EnsureContainerExistsAsync(container);
        var blobClient = blobContainer.GetBlobClient(destinationPath);
        await blobClient.DeleteIfExistsAsync();
        using var stream = new MemoryStream(data);

        if (string.IsNullOrWhiteSpace(cacheControl) && string.IsNullOrWhiteSpace(contentType))
        {
            await blobClient.UploadAsync(stream, overwrite: true);
            return;
        }

        var httpHeaders = new BlobHttpHeaders();
        if (!string.IsNullOrWhiteSpace(cacheControl))
        {
            httpHeaders.CacheControl = cacheControl;
        }

        if (!string.IsNullOrWhiteSpace(contentType))
        {
            httpHeaders.ContentType = contentType;
        }

        await blobClient.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = httpHeaders
        });
    }

    /// <summary>
    /// Returns whether a blob exists in the specified container.
    /// </summary>
    public virtual async Task<bool> ExistsAsync(string container, string destinationPath, CancellationToken cancellationToken = default)
    {
        var blobContainer = GetOrCreateContainerClient(container);
        var blobClient = blobContainer.GetBlobClient(destinationPath);
        var response = await blobClient.ExistsAsync(cancellationToken);
        return response.Value;
    }

    /// <summary>
    /// Deletes a blob from the specified container.
    /// </summary>
    public async Task DeleteAsync(string container, string destinationPath)
    {
        var blobContainer = await EnsureContainerExistsAsync(container);
        var blobClient = blobContainer.GetBlobClient(destinationPath);
        await blobClient.DeleteIfExistsAsync();
    }

    /// <summary>
    /// Downloads data from the default blob container.
    /// </summary>
    public async Task<byte[]> DownloadAsync(string sourcePath)
    {
        var blobClient = _blobContainer.GetBlobClient(sourcePath);

        using var stream = new MemoryStream();
        var response = await blobClient.DownloadAsync();
        await response.Value.Content.CopyToAsync(stream);

        return stream.ToArray();
    }

    /// <summary>
    /// Downloads data from a specific blob container.
    /// </summary>
    public async Task<byte[]> DownloadAsync(string container, string sourcePath)
    {
        var blobContainer = await EnsureContainerExistsAsync(container);
        var blobClient = blobContainer.GetBlobClient(sourcePath);

        using var stream = new MemoryStream();
        var response = await blobClient.DownloadAsync();
        await response.Value.Content.CopyToAsync(stream);

        return stream.ToArray();
    }

    /// <summary>
    /// Lists all blobs in the default container.
    /// </summary>
    public async Task<List<BlobItem>> FilesAsync()
    {
        var blobs = new List<BlobItem>();
        await foreach (var blob in _blobContainer.GetBlobsAsync())
        {
            blobs.Add(blob);
        }

        return blobs;
    }

    /// <summary>
    /// Lists all blobs in a specific container.
    /// </summary>
    public async Task<List<BlobItem>> FilesAsync(string container)
    {
        var blobContainer = await EnsureContainerExistsAsync(container);
        var blobs = new List<BlobItem>();
        await foreach (var blob in blobContainer.GetBlobsAsync())
        {
            blobs.Add(blob);
        }

        return blobs;
    }

    /// <summary>
    /// Compresses data and uploads to the default container.
    /// </summary>
    public async Task CompressAndUploadAsync(byte[] data, string destinationPath)
    {
        var compressed = ZipHelpers.Compress(data);
        if (compressed != null)
            await UploadAsync(compressed, destinationPath);
    }

    /// <summary>
    /// Compresses data and uploads to a specific container.
    /// </summary>
    public async Task CompressAndUploadAsync(byte[] data, string container, string destinationPath)
    {
        var compressed = ZipHelpers.Compress(data);
        if (compressed != null)
            await UploadAsync(compressed, container.ToLower(), destinationPath);
    }

    /// <summary>
    /// Downloads and decompresses data from the default container.
    /// </summary>
    public async Task<byte[]> DownloadAndDecompressAsync(string sourcePath)
    {
        var compressed = await DownloadAsync(sourcePath);
        return ZipHelpers.Decompress(compressed);
    }

    /// <summary>
    /// Downloads and decompresses data from a specific container.
    /// </summary>
    public async Task<byte[]> DownloadAndDecompressAsync(string container, string sourcePath)
    {
        var compressed = await DownloadAsync(container, sourcePath);
        return ZipHelpers.Decompress(compressed);
    }
}
