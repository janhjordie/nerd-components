using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace TheNerdCollective.Services.Azure;

/// <summary>
/// Azure Blob Storage service for uploading, downloading, and managing blobs.
/// </summary>
public class AzureBlobService
{
    private readonly BlobContainerClient _blobContainer;
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobService(IOptions<AzureBlobOptions> options)
    {
        var config = options.Value;
        _blobServiceClient = new BlobServiceClient(config.ConnectionString);
        _blobContainer = CreateOrGetBlobContainer(config.ContainerName);
    }

    private BlobContainerClient CreateOrGetBlobContainer(string container)
    {
        container = container.ToLower();
        var containers = _blobServiceClient
            .GetBlobContainers()
            .ToList();

        if (!containers.Any(x => x.Name.Equals(container)))
            _blobServiceClient.CreateBlobContainer(container);

        var blobContainer = _blobServiceClient.GetBlobContainerClient(container);
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
        var blobContainer = CreateOrGetBlobContainer(container);
        var blobClient = blobContainer.GetBlobClient(destinationPath);
        await blobClient.DeleteIfExistsAsync();
        using var stream = new MemoryStream(data);
        await blobClient.UploadAsync(stream, true);
    }

    /// <summary>
    /// Deletes a blob from the specified container.
    /// </summary>
    public async Task DeleteAsync(string container, string destinationPath)
    {
        var blobContainer = CreateOrGetBlobContainer(container);
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
        var blobContainer = CreateOrGetBlobContainer(container);
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
        var blobContainer = CreateOrGetBlobContainer(container);
        var blobs = new List<BlobItem>();
        await foreach (var blob in blobContainer.GetBlobsAsync())
        {
            blobs.Add(blob);
        }

        return blobs;
    }
}
