using FullstackTemplate.Domain.Entities;

namespace FullstackTemplate.Domain.Interfaces;

public interface IConnectionMetadataRepository
{
    Task<IEnumerable<ConnectionMetadata>> GetByConnectionIdAsync(Guid connectionId);
    Task<IEnumerable<ConnectionMetadata>> SaveAsync(Guid connectionId, IEnumerable<ConnectionMetadata> metadata);
    Task<bool> DeleteByConnectionIdAsync(Guid connectionId);
}
