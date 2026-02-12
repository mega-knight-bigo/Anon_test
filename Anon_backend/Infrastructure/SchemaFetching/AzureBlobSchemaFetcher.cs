using Azure.Storage.Blobs;
using FullstackTemplate.Application.Interfaces;
using FullstackTemplate.Domain.Entities;

namespace FullstackTemplate.Infrastructure.SchemaFetching;

public class AzureBlobSchemaFetcher : ISchemaFetcher
{
    public async Task<IEnumerable<ConnectionMetadata>> FetchSchemaAsync(Connection connection)
    {
        var config = connection.Config.RootElement;
        var accountName = config.GetProperty("accountName").GetString()!;
        var containerName = config.TryGetProperty("containerName", out var cn) ? cn.GetString() : null;
        var accountKey = config.TryGetProperty("accountKey", out var ak) ? ak.GetString() : null;

        var connectionString = $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};EndpointSuffix=core.windows.net";
        var blobServiceClient = new BlobServiceClient(connectionString);

        var results = new List<ConnectionMetadata>();

        if (!string.IsNullOrEmpty(containerName))
        {
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var folders = new HashSet<string>();

            await foreach (var blob in containerClient.GetBlobsAsync())
            {
                var parts = blob.Name.Split('/');
                if (parts.Length > 1)
                {
                    var folder = parts[0];
                    if (folders.Add(folder))
                    {
                        results.Add(new ConnectionMetadata
                        {
                            ConnectionId = connection.Id,
                            ObjectType = "folder",
                            ObjectName = folder,
                            ObjectPath = $"{containerName}/{folder}/",
                            FetchedAt = DateTime.UtcNow
                        });
                    }
                }

                results.Add(new ConnectionMetadata
                {
                    ConnectionId = connection.Id,
                    ObjectType = "file",
                    ObjectName = parts.Last(),
                    ObjectPath = $"{containerName}/{blob.Name}",
                    FetchedAt = DateTime.UtcNow
                });

                if (results.Count >= 100) break;
            }
        }
        else
        {
            await foreach (var container in blobServiceClient.GetBlobContainersAsync())
            {
                results.Add(new ConnectionMetadata
                {
                    ConnectionId = connection.Id,
                    ObjectType = "bucket",
                    ObjectName = container.Name,
                    ObjectPath = container.Name,
                    FetchedAt = DateTime.UtcNow
                });
            }
        }

        return results;
    }
}
