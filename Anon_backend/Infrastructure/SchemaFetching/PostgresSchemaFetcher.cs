using System.Text.Json;
using Npgsql;
using FullstackTemplate.Application.Interfaces;
using FullstackTemplate.Domain.Entities;

namespace FullstackTemplate.Infrastructure.SchemaFetching;

public class PostgresSchemaFetcher : ISchemaFetcher
{
    public async Task<IEnumerable<ConnectionMetadata>> FetchSchemaAsync(Connection connection)
    {
        var config = connection.Config.RootElement;
        var host = config.GetProperty("host").GetString() ?? "localhost";
        var port = GetPortValue(config, "port", 5432);
        var database = config.GetProperty("database").GetString() ?? "";
        var username = config.TryGetProperty("username", out var u) ? u.GetString() 
                     : config.TryGetProperty("user", out var usr) ? usr.GetString() : null;
        var password = config.TryGetProperty("password", out var pw) ? pw.GetString() : null;

        var connStr = $"Host={host};Port={port};Database={database}";
        if (!string.IsNullOrEmpty(username)) connStr += $";Username={username}";
        if (!string.IsNullOrEmpty(password)) connStr += $";Password={password}";

        var results = new List<ConnectionMetadata>();

        await using var conn = new NpgsqlConnection(connStr);
        await conn.OpenAsync();

        // Get tables
        const string tablesSql = @"
            SELECT table_schema, table_name 
            FROM information_schema.tables 
            WHERE table_schema NOT IN ('pg_catalog', 'information_schema')
            ORDER BY table_schema, table_name";

        await using var tablesCmd = new NpgsqlCommand(tablesSql, conn);
        await using var tablesReader = await tablesCmd.ExecuteReaderAsync();

        var tables = new List<(string schema, string name)>();
        while (await tablesReader.ReadAsync())
        {
            tables.Add((tablesReader.GetString(0), tablesReader.GetString(1)));
        }
        await tablesReader.CloseAsync();

        // Get columns for each table
        foreach (var (schema, tableName) in tables)
        {
            var columnsSql = @"
                SELECT column_name, data_type, is_nullable
                FROM information_schema.columns
                WHERE table_schema = @schema AND table_name = @table
                ORDER BY ordinal_position";

            await using var colsCmd = new NpgsqlCommand(columnsSql, conn);
            colsCmd.Parameters.AddWithValue("@schema", schema);
            colsCmd.Parameters.AddWithValue("@table", tableName);

            var columns = new List<object>();
            await using var colsReader = await colsCmd.ExecuteReaderAsync();
            while (await colsReader.ReadAsync())
            {
                columns.Add(new
                {
                    name = colsReader.GetString(0),
                    type = colsReader.GetString(1),
                    nullable = colsReader.GetString(2) == "YES"
                });
            }

            results.Add(new ConnectionMetadata
            {
                ConnectionId = connection.Id,
                ObjectType = "table",
                ObjectName = tableName,
                ObjectPath = $"{schema}.{tableName}",
                Columns = JsonDocument.Parse(JsonSerializer.Serialize(columns)),
                FetchedAt = DateTime.UtcNow
            });
        }

        return results;
    }

    private static int GetPortValue(JsonElement root, string propertyName, int defaultValue)
    {
        if (!root.TryGetProperty(propertyName, out var prop))
            return defaultValue;

        return prop.ValueKind switch
        {
            JsonValueKind.Number => prop.GetInt32(),
            JsonValueKind.String when int.TryParse(prop.GetString(), out var port) => port,
            _ => defaultValue
        };
    }
}
