namespace TheNerdCollective.Services.Azure;

/// <summary>
/// Configuration options for Azure Blob Storage service.
/// </summary>
public class AzureBlobOptions
{
    /// <summary>
    /// Gets or sets the Azure Storage connection string.
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the default blob container name.
    /// </summary>
    public required string ContainerName { get; set; }
}
