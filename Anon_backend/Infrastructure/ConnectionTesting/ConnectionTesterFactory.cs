using FullstackTemplate.Application.Interfaces;

namespace FullstackTemplate.Infrastructure.ConnectionTesting;

public class ConnectionTesterFactory : IConnectionTesterFactory
{
    public IConnectionTester GetTester(string connectionType)
    {
        return connectionType.ToLowerInvariant() switch
        {
            "database" or "postgresql" or "postgres" => new PostgresConnectionTester(),
            "mysql" => new MySqlConnectionTester(),
            "s3" => new S3ConnectionTester(),
            "azure_blob" or "azure" => new AzureBlobConnectionTester(),
            "snowflake" => new SnowflakeConnectionTester(),
            _ => throw new ArgumentException($"Unsupported connection type: {connectionType}")
        };
    }
}
