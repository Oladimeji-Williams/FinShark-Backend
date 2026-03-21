namespace FinShark.Domain.Interfaces;

/// <summary>
/// Generic repository interface for standard CRUD operations
/// Provides a common contract for all repository implementations
/// Designed to work with Entity Framework Core patterns
/// </summary>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Retrieves all entities
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an entity by ID
    /// </summary>
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the database
    /// </summary>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity in the database
    /// </summary>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity from the database
    /// </summary>
    Task DeleteAsync(T entity, bool hardDelete = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of entities
    /// </summary>
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
}
