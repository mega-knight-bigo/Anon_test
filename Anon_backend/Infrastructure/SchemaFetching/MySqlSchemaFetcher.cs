using System.Text.Json;
using MySqlConnector;
using FullstackTemplate.Application.Interfaces;
using FullstackTemplate.Domain.Entities;

namespace FullstackTemplate.Infrastructure.SchemaFetching;

public class MySqlSchemaFetcher : ISchemaFetcher
{
    public async Task<IEnumerable<ConnectionMetadata>> FetchSchemaAsync(Connection connection)
    {
        var config = connection.Config.RootElement;
        var host = config.GetProperty("host").GetString() ?? "localhost";
        var port = GetPortValue(config, "port", 3306);
        var database = config.GetProperty("database").GetString() ?? "";
        var username = config.TryGetProperty("username", out var u) ? u.GetString() 
                     : config.TryGetProperty("user", out var usr) ? usr.GetString() : null;
        var password = config.TryGetProperty("password", out var pw) ? pw.GetString() : null;

        var connStr = $"Server={host};Port={port};Database={database}";
        if (!string.IsNullOrEmpty(username)) connStr += $";User={username}";
        if (!string.IsNullOrEmpty(password)) connStr += $";Password={password}";

        var results = new List<ConnectionMetadata>();

        await using var conn = new MySqlConnection(connStr);
        await conn.OpenAsync();

        const string tablesSql = @"
            SELECT table_name 
            FROM information_schema.tables 
            WHERE table_schema = @db
            ORDER BY table_name";

        await using var tablesCmd = new MySqlCommand(tablesSql, conn);
        tablesCmd.Parameters.AddWithValue("@db", database);
        await using var tablesReader = await tablesCmd.ExecuteReaderAsync();

        var tableNames = new List<string>();
        while (await tablesReader.ReadAsync())
            tableNames.Add(tablesReader.GetString(0));
        await tablesReader.CloseAsync();

        foreach (var tableName in tableNames)
        {
            var columnsSql = @"
                SELECT column_name, data_type, is_nullable
                FROM information_schema.columns
                WHERE table_schema = @db AND table_name = @table
                ORDER BY ordinal_position";

            await using var colsCmd = new MySqlCommand(columnsSql, conn);
            colsCmd.Parameters.AddWithValue("@db", database);
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
                ObjectPath = $"{database}.{tableName}",
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
