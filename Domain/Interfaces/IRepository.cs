namespace StudentAssessmentTracker.Domain.Interfaces
{
    /// <summary>
    /// Generic repository interface - abstraction for data access operations
    /// Decouples domain logic from infrastructure implementation
    /// </summary>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Retrieves an entity by its ID
        /// </summary>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all entities
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Adds a new entity to the repository
        /// </summary>
        Task AddAsync(T entity);

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Deletes an entity by ID
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Saves all pending changes to the data store
        /// </summary>
        Task SaveChangesAsync();
    }
}
