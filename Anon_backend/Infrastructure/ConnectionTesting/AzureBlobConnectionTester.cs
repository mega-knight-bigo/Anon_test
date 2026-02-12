using System.Text.Json;
using Azure.Storage.Blobs;
using FullstackTemplate.Application.Interfaces;

namespace FullstackTemplate.Infrastructure.ConnectionTesting;

public class AzureBlobConnectionTester : IConnectionTester
{
    public async Task<TestConnectionResult> TestConnectionAsync(JsonDocument config)
    {
        try
        {
            var root = config.RootElement;
            
            string connectionString;
            
            // Check if connectionString is provided directly
            if (root.TryGetProperty("connectionString", out var connStr) && !string.IsNullOrEmpty(connStr.GetString()))
            {
                connectionString = connStr.GetString()!;
            }
            else
            {
                // Build connection string from individual properties
                var accountName = root.GetProperty("accountName").GetString()!;
                var accountKey = root.TryGetProperty("accountKey", out var ak) ? ak.GetString() : null;
                connectionString = $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};EndpointSuffix=core.windows.net";
            }

            var containerName = root.TryGetProperty("containerName", out var cn) ? cn.GetString() : null;

            var blobServiceClient = new BlobServiceClient(connectionString);

            if (!string.IsNullOrEmpty(containerName))
            {
                // Test specific container access
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                var exists = await containerClient.ExistsAsync();
                
                if (!exists.Value)
                {
                    return new TestConnectionResult(
                        false,
                        "Container not found",
                        $"Container '{containerName}' does not exist or is not accessible"
                    );
                }

                return new TestConnectionResult(
                    true,
                    "Successfully connected to Azure Blob Storage",
                    $"Container '{containerName}' is accessible"
                );
            }
            else
            {
                // Test account access by listing containers (max 1)
                await foreach (var container in blobServiceClient.GetBlobContainersAsync())
                {
                    break; // Just need to verify we can list
                }

                return new TestConnectionResult(
                    true,
                    "Successfully connected to Azure Blob Storage",
                    "Storage account is accessible"
                );
            }
        }
        catch (Azure.RequestFailedException ex)
        {
            return new TestConnectionResult(
                false,
                "Failed to connect to Azure Blob Storage",
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
