using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GymHero.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace GymHero.Infrastructure.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _storageAccountUrl;

    public BlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"]
            ?? configuration.GetConnectionString("AzureStorage");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Azure Storage connection string not found. " +
                "Add 'AzureStorage:ConnectionString' or 'ConnectionStrings:AzureStorage' to configuration.");
        }

        _blobServiceClient = new BlobServiceClient(connectionString);

        // Extract storage account URL from connection string
        var accountNameStart = connectionString.IndexOf("AccountName=") + "AccountName=".Length;
        var accountNameEnd = connectionString.IndexOf(";", accountNameStart);
        var accountName = connectionString.Substring(accountNameStart, accountNameEnd - accountNameStart);
        _storageAccountUrl = $"https://{accountName}.blob.core.windows.net";
    }

    public async Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string containerName,
        CancellationToken cancellationToken = default)
    {
        // Ensure container exists
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(
            PublicAccessType.Blob, // Allow public read access to blobs
            cancellationToken: cancellationToken);

        // Get blob client
        var blobClient = containerClient.GetBlobClient(fileName);

        // Upload with content type
        var blobHttpHeaders = new BlobHttpHeaders
        {
            ContentType = contentType
        };

        await blobClient.UploadAsync(
            stream,
            new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders
            },
            cancellationToken);

        // Return the public URL
        return blobClient.Uri.ToString();
    }

    public async Task DeleteAsync(
        string blobUrl,
        string containerName,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        // Extract blob name from URL if it's a full URL
        var blobName = ExtractBlobNameFromUrl(blobUrl, containerName);

        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        string blobUrl,
        string containerName,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        // Extract blob name from URL if it's a full URL
        var blobName = ExtractBlobNameFromUrl(blobUrl, containerName);

        var blobClient = containerClient.GetBlobClient(blobName);
        return await blobClient.ExistsAsync(cancellationToken);
    }

    private string ExtractBlobNameFromUrl(string blobUrlOrName, string containerName)
    {
        // If it's already just a blob name (no http), return as-is
        if (!blobUrlOrName.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return blobUrlOrName;
        }

        // Extract blob name from full URL
        // URL format: https://{account}.blob.core.windows.net/{container}/{blobname}
        var uri = new Uri(blobUrlOrName);
        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Skip container name and get the blob name
        if (segments.Length > 1 && segments[0] == containerName)
        {
            return string.Join("/", segments.Skip(1));
        }

        // Fallback: return the last segment
        return segments.LastOrDefault() ?? blobUrlOrName;
    }
}
