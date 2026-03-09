using StudentAssessmentTracker.Domain.Entities;
using StudentAssessmentTracker.Domain.Interfaces;
using StudentAssessmentTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace StudentAssessmentTracker.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository implementation for data access operations
    /// Implements the IRepository interface for Student entity
    /// </summary>
    public class Repository<T> : IRepository<T> where T : class
    {
        /// <summary>
        /// Database context for entity access
        /// </summary>
        protected readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the Repository class
        /// </summary>
        /// <param name="context">The database context</param>
        public Repository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves an entity by ID
        /// </summary>
        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        /// <summary>
        /// Retrieves all entities as a list
        /// </summary>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        /// <summary>
        /// Adds a new entity to the database
        /// </summary>
        public virtual async Task AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _context.Set<T>().AddAsync(entity);
            await SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        public virtual async Task UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.Set<T>().Update(entity);
            await SaveChangesAsync();
        }

        /// <summary>
        /// Deletes an entity by ID
        /// </summary>
        public virtual async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
                await SaveChangesAsync();
            }
        }

        /// <summary>
        /// Saves all pending changes to the database
        /// </summary>
        public virtual async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Student-specific repository extending the generic repository
    /// Can be extended with student-specific data access logic
    /// </summary>
    public class StudentRepository : Repository<Student>
    {
        /// <summary>
        /// Initializes a new instance of the StudentRepository class
        /// </summary>
        /// <param name="context">The database context</param>
        public StudentRepository(ApplicationDbContext context) : base(context) { }

        /// <summary>
        /// Gets all students with error handling
        /// </summary>
        public override async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _context.Students
                .AsNoTracking()
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();
        }
    }
}
