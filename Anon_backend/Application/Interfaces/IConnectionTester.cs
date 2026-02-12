using System.Text.Json;

namespace FullstackTemplate.Application.Interfaces;

public record TestConnectionResult(
    bool Success,
    string Message,
    string? Details = null
);

public interface IConnectionTester
{
    Task<TestConnectionResult> TestConnectionAsync(JsonDocument config);
}

public interface IConnectionTesterFactory
{
    IConnectionTester GetTester(string connectionType);
}
