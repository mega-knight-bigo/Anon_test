using System.Text.Json;
using FullstackTemplate.Application.Interfaces;

namespace FullstackTemplate.Infrastructure.ConnectionTesting;

public class SnowflakeConnectionTester : IConnectionTester
{
    public Task<TestConnectionResult> TestConnectionAsync(JsonDocument config)
    {
        // Snowflake requires the official Snowflake.Data SDK which has complex setup
        // For now, validate the configuration has required fields
        try
        {
            var root = config.RootElement;
            
            // Validate required fields exist
            var account = root.GetProperty("account").GetString();
            var warehouse = root.TryGetProperty("warehouse", out var wh) ? wh.GetString() : "COMPUTE_WH";
            var database = root.GetProperty("database").GetString();
            var schema = root.TryGetProperty("schema", out var sc) ? sc.GetString() : "PUBLIC";
            var user = root.GetProperty("user").GetString();
            var password = root.GetProperty("password").GetString();
            
            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(database) || 
                string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
            {
                return Task.FromResult(new TestConnectionResult(
                    false,
                    "Invalid Snowflake configuration",
                    "Missing required fields: account, database, user, or password"
                ));
            }

            // Note: To fully test Snowflake connections, install Snowflake.Data NuGet package
            // and uncomment the connection test code below:
            /*
            var connStr = $"account={account};user={user};password={password};warehouse={warehouse};db={database};schema={schema}";
            using var conn = new SnowflakeDbConnection();
            conn.ConnectionString = connStr;
            await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1";
            await cmd.ExecuteScalarAsync();
            */

            return Task.FromResult(new TestConnectionResult(
                true,
                "Snowflake configuration is valid",
                $"Configuration validated for account: {account}, database: {database}. Note: Full connection test requires Snowflake SDK."
            ));
        }
        catch (KeyNotFoundException ex)
        {
            return Task.FromResult(new TestConnectionResult(
                false,
                "Invalid Snowflake configuration",
                $"Missing required field: {ex.Message}"
            ));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new TestConnectionResult(
                false,
                "Configuration validation failed",
                ex.Message
            ));
        }
    }
}
