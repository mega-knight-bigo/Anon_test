using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using FullstackTemplate.Application.Interfaces;
using FullstackTemplate.Domain.Entities;

namespace FullstackTemplate.Infrastructure.SchemaFetching;

public class S3SchemaFetcher : ISchemaFetcher
{
    public async Task<IEnumerable<ConnectionMetadata>> FetchSchemaAsync(Connection connection)
    {
        var config = connection.Config.RootElement;
        var bucket = config.GetProperty("bucket").GetString()!;
        var region = config.TryGetProperty("region", out var r) ? r.GetString() : "us-east-1";
        var accessKeyId = config.TryGetProperty("accessKeyId", out var ak) ? ak.GetString() : null;
        var secretAccessKey = config.TryGetProperty("secretAccessKey", out var sk) ? sk.GetString() : null;

        var s3Config = new AmazonS3Config { RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region!) };

        using var client = !string.IsNullOrEmpty(accessKeyId) && !string.IsNullOrEmpty(secretAccessKey)
            ? new AmazonS3Client(accessKeyId, secretAccessKey, s3Config)
            : new AmazonS3Client(s3Config);

        var request = new ListObjectsV2Request { BucketName = bucket, MaxKeys = 100 };
        var response = await client.ListObjectsV2Async(request);

        var results = new List<ConnectionMetadata>();
        var folders = new HashSet<string>();

        foreach (var obj in response.S3Objects)
        {
            var parts = obj.Key.Split('/');
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
                        ObjectPath = folder + "/",
                        FetchedAt = DateTime.UtcNow
                    });
                }
            }

            if (!obj.Key.EndsWith('/'))
            {
                results.Add(new ConnectionMetadata
                {
                    ConnectionId = connection.Id,
                    ObjectType = "file",
                    ObjectName = parts.Last(),
                    ObjectPath = obj.Key,
                    FetchedAt = DateTime.UtcNow
                });
            }
        }

        return results;
    }
}
