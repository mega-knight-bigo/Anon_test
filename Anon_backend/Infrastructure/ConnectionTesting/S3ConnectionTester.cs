using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using FullstackTemplate.Application.Interfaces;

namespace FullstackTemplate.Infrastructure.ConnectionTesting;

public class S3ConnectionTester : IConnectionTester
{
    public async Task<TestConnectionResult> TestConnectionAsync(JsonDocument config)
    {
        try
        {
            var root = config.RootElement;
            var bucket = root.GetProperty("bucket").GetString()!;
            var region = root.TryGetProperty("region", out var r) ? r.GetString() : "us-east-1";
            var accessKeyId = root.TryGetProperty("accessKeyId", out var ak) ? ak.GetString() : null;
            var secretAccessKey = root.TryGetProperty("secretAccessKey", out var sk) ? sk.GetString() : null;

            var s3Config = new AmazonS3Config 
            { 
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region!),
                Timeout = TimeSpan.FromSeconds(10)
            };

            using var client = !string.IsNullOrEmpty(accessKeyId) && !string.IsNullOrEmpty(secretAccessKey)
                ? new AmazonS3Client(accessKeyId, secretAccessKey, s3Config)
                : new AmazonS3Client(s3Config);

            // Try to get bucket location to verify access
            var request = new ListObjectsV2Request
            {
                BucketName = bucket,
                MaxKeys = 1
            };
            await client.ListObjectsV2Async(request);

            return new TestConnectionResult(
                true,
                "Successfully connected to S3 bucket",
                $"Bucket '{bucket}' is accessible in region {region}"
            );
        }
        catch (AmazonS3Exception ex)
        {
            return new TestConnectionResult(
                false,
                "Failed to connect to S3 bucket",
                ex.Message
            );
        }
        catch (Exception ex)
        {
            return new TestConnectionResult(
                false,
                "Connection test failed",
                ex.Message
            );
        }
    }
}
