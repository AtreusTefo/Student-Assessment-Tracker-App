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
    /// Concrete student repository — implements IStudentRepository which extends
    /// IRepository&lt;Student&gt; with teacher-scoped queries that enforce data isolation.
    /// All teacher-facing operations must use GetAllByTeacherAsync / GetByIdForTeacherAsync
    /// instead of the unfiltered base methods.
    /// </summary>
    public class StudentRepository : Repository<Student>, IStudentRepository
    {
        /// <summary>Initialises the repository with the application database context.</summary>
        public StudentRepository(ApplicationDbContext context) : base(context) { }

        // Unscoped overrides — kept for student self-service paths (activation / login)
        /// <summary>Returns all students regardless of teacher ownership — used only by internal self-service paths.</summary>
        public override async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _context.Students
                .AsNoTracking()
                .Include(s => s.Assessments)
                .Include(s => s.GradeNavigation)
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();
        }

        /// <summary>Returns the student with <paramref name="id"/>, including assessments and grade navigation.</summary>
        public override async Task<Student?> GetByIdAsync(int id)
        {
            return await _context.Students
                .Include(s => s.Assessments)
                .Include(s => s.GradeNavigation)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Student>> GetAllByTeacherAsync(int teacherId)
        {
            return await _context.Students
                .AsNoTracking()
                .Where(s => s.TeacherAssignments.Any(ta => ta.TeacherId == teacherId))
                .Include(s => s.Assessments)
                .Include(s => s.GradeNavigation)
                .Include(s => s.TeacherAssignments)
                    .ThenInclude(ta => ta.Teacher)
                    .ThenInclude(t => t.SubjectNavigation)
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Student?> GetByIdForTeacherAsync(int id, int teacherId)
        {
            return await _context.Students
                .Where(s => s.Id == id && s.TeacherAssignments.Any(ta => ta.TeacherId == teacherId))
                .Include(s => s.Assessments)
                .Include(s => s.GradeNavigation)
                .Include(s => s.TeacherAssignments)
                    .ThenInclude(ta => ta.Teacher)
                    .ThenInclude(t => t.SubjectNavigation)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task<bool> ExistsForTeacherAsync(int id, int teacherId)
        {
            return await _context.Students
                .AsNoTracking()
                .AnyAsync(s => s.Id == id && s.TeacherAssignments.Any(ta => ta.TeacherId == teacherId));
        }

        /// <inheritdoc />
        public async Task<Student?> FindByUniqueIdAsync(string uniqueId)
        {
            // Normalize the lookup value in C# so the column comparison is a simple
            // indexed equality check — avoids calling a SQL function on the column.
            var normalized = uniqueId.ToUpperInvariant();
            return await _context.Students
                .Include(s => s.Assessments)
                .Include(s => s.GradeNavigation)
                .FirstOrDefaultAsync(s => s.StudentUniqueId == normalized);
        }

        /// <inheritdoc />
        public async Task<bool> ExistsByEmailAsync(string email, int excludeStudentId = 0)
        {
            var normalizedEmail = email.ToLowerInvariant();
            return await _context.Students
                .AsNoTracking()
                .AnyAsync(s => s.Email != null && s.Email.ToLower() == normalizedEmail && s.Id != excludeStudentId);
        }

        /// <inheritdoc />
        public async Task<bool> ExistsByIdPassportNoAsync(string idPassportNo, int excludeStudentId = 0)
        {
            var normalized = idPassportNo.ToUpperInvariant();
            return await _context.Students
                .AsNoTracking()
                .AnyAsync(s => s.IdPassportNo != null && s.IdPassportNo.ToUpper() == normalized && s.Id != excludeStudentId);
        }

        /// <inheritdoc />
        public async Task AssignToTeacherAsync(int studentId, int teacherId)
        {
            // Idempotent — skip if the assignment already exists
            var exists = await _context.TeacherStudents
                .AsNoTracking()
                .AnyAsync(ts => ts.TeacherId == teacherId && ts.StudentId == studentId);
            if (!exists)
            {
                _context.TeacherStudents.Add(new TeacherStudent
                {
                    TeacherId = teacherId,
                    StudentId = studentId,
                    AssignedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
        }

        /// <inheritdoc />
        public async Task UnassignFromTeacherAsync(int studentId, int teacherId)
        {
            var assignment = await _context.TeacherStudents
                .FirstOrDefaultAsync(ts => ts.TeacherId == teacherId && ts.StudentId == studentId);
            if (assignment is not null)
            {
                _context.TeacherStudents.Remove(assignment);
                await _context.SaveChangesAsync();
            }
        }

        /// <inheritdoc />
        public async Task<bool> IsAssignedToTeacherAsync(int studentId, int teacherId)
        {
            return await _context.TeacherStudents
                .AsNoTracking()
                .AnyAsync(ts => ts.TeacherId == teacherId && ts.StudentId == studentId);
        }

        /// <inheritdoc />
        public async Task<int> CountTeacherAssignmentsAsync(int studentId)
        {
            return await _context.TeacherStudents
                .AsNoTracking()
                .CountAsync(ts => ts.StudentId == studentId);
        }
    }
}
