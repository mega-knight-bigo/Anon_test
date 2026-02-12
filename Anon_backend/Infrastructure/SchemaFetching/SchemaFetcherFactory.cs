using FullstackTemplate.Application.Interfaces;

namespace FullstackTemplate.Infrastructure.SchemaFetching;

public class SchemaFetcherFactory : ISchemaFetcherFactory
{
    public ISchemaFetcher GetFetcher(string connectionType)
    {
        return connectionType.ToLowerInvariant() switch
        {
            "database" or "postgresql" or "postgres" => new PostgresSchemaFetcher(),
            "mysql" => new MySqlSchemaFetcher(),
            "s3" => new S3SchemaFetcher(),
            "azure_blob" or "azure" => new AzureBlobSchemaFetcher(),
            "snowflake" => new SnowflakeSchemaFetcher(),
            _ => throw new ArgumentException($"Unsupported connection type: {connectionType}")
        };
    }
}
