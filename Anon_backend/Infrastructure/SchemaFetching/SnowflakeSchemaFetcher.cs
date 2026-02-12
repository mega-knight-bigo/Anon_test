using System.Text.Json;
using FullstackTemplate.Application.Interfaces;
using FullstackTemplate.Domain.Entities;

namespace FullstackTemplate.Infrastructure.SchemaFetching;

public class SnowflakeSchemaFetcher : ISchemaFetcher
{
    public Task<IEnumerable<ConnectionMetadata>> FetchSchemaAsync(Connection connection)
    {
        // Snowflake SDK not installed — return simulated data
        var results = new List<ConnectionMetadata>
        {
            new()
            {
                ConnectionId = connection.Id,
                ObjectType = "table",
                ObjectName = "SAMPLE_TABLE",
                ObjectPath = "PUBLIC.SAMPLE_TABLE",
                Columns = JsonDocument.Parse(JsonSerializer.Serialize(new[]
                {
                    new { name = "ID", type = "NUMBER", nullable = false },
                    new { name = "NAME", type = "VARCHAR", nullable = true },
                    new { name = "CREATED_AT", type = "TIMESTAMP_NTZ", nullable = false }
                })),
                FetchedAt = DateTime.UtcNow
            }
        };
        return Task.FromResult<IEnumerable<ConnectionMetadata>>(results);
    }
}
