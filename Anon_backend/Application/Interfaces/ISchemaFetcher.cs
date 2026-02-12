using FullstackTemplate.Domain.Entities;

namespace FullstackTemplate.Application.Interfaces;

public interface ISchemaFetcher
{
    Task<IEnumerable<ConnectionMetadata>> FetchSchemaAsync(Connection connection);
}

public interface ISchemaFetcherFactory
{
    ISchemaFetcher GetFetcher(string connectionType);
}
