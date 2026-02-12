using System.Text.Json;
using Npgsql;
using FullstackTemplate.Application.Interfaces;

namespace FullstackTemplate.Infrastructure.ConnectionTesting;

public class PostgresConnectionTester : IConnectionTester
{
    public async Task<TestConnectionResult> TestConnectionAsync(JsonDocument config)
    {
        try
        {
            var root = config.RootElement;
            var host = root.GetProperty("host").GetString() ?? "localhost";
            var port = GetPortValue(root, "port", 5432);
            var database = root.GetProperty("database").GetString() ?? "";
            var username = root.TryGetProperty("username", out var u) ? u.GetString() 
                         : root.TryGetProperty("user", out var usr) ? usr.GetString() : null;
            var password = root.TryGetProperty("password", out var pw) ? pw.GetString() : null;

            var connStr = $"Host={host};Port={port};Database={database};Timeout=10;CommandTimeout=10";
            if (!string.IsNullOrEmpty(username)) connStr += $";Username={username}";
            if (!string.IsNullOrEmpty(password)) connStr += $";Password={password}";

            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();

            // Run a simple query to verify the connection is working
            await using var cmd = new NpgsqlCommand("SELECT 1", conn);
            await cmd.ExecuteScalarAsync();

            return new TestConnectionResult(
                true,
                "Successfully connected to PostgreSQL database",
                $"Connected to {database} on {host}:{port}"
            );
        }
        catch (NpgsqlException ex)
        {
            return new TestConnectionResult(
                false,
                "Failed to connect to PostgreSQL database",
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
