using StudentAssessmentTracker.Domain.Entities;
using StudentAssessmentTracker.Domain.Interfaces;
using StudentAssessmentTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace StudentAssessmentTracker.Infrastructure.Repositories
{
    /// <summary>
    /// Concrete teacher repository — implements ITeacherRepository which adds a
    /// server-side email lookup to eliminate the full-table-scan on login.
    /// </summary>
    public class TeacherRepository : Repository<Teacher>, ITeacherRepository
    {
        /// <summary>Initialises the repository with the application database context.</summary>
        public TeacherRepository(ApplicationDbContext context) : base(context) { }

        /// <summary>Returns the teacher with <paramref name="id"/>, including subject navigation.</summary>
        public override async Task<Teacher?> GetByIdAsync(int id)
        {
            return await _context.Teachers
                .Include(t => t.SubjectNavigation)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>Returns all teachers, each including their subject navigation property.</summary>
        public override async Task<IEnumerable<Teacher>> GetAllAsync()
        {
            return await _context.Teachers
                .Include(t => t.SubjectNavigation)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Teacher?> FindByEmailAsync(string email)
        {
            // Normalize the parameter in C# so only the column side needs a SQL function,
            // keeping the comparison server-side and translatable by EF Core.
            var normalizedEmail = email.ToLowerInvariant();
            return await _context.Teachers
                .Include(t => t.SubjectNavigation)
                .FirstOrDefaultAsync(t => t.Email.ToLower() == normalizedEmail);
        }

        /// <inheritdoc />
        public async Task<bool> HasStudentsAsync(int teacherId)
        {
            // Query the join table — teacher has students if any TeacherStudent rows reference them
            return await _context.TeacherStudents
                .AsNoTracking()
                .AnyAsync(ts => ts.TeacherId == teacherId);
        }

        /// <inheritdoc />
        public async Task<bool> ExistsByIdPassportNoAsync(string idPassportNo)
        {
            var normalized = idPassportNo.ToUpperInvariant();
            return await _context.Teachers
                .AsNoTracking()
                .AnyAsync(t => t.IdPassportNo.ToUpper() == normalized);
        }

        /// <inheritdoc />
        public async Task<bool> ExistsByEmailAsync(string email, int excludeTeacherId = 0)
        {
            var normalizedEmail = email.ToLowerInvariant();
            return await _context.Teachers
                .AsNoTracking()
                .AnyAsync(t => t.Email.ToLower() == normalizedEmail && t.Id != excludeTeacherId);
        }

        /// <inheritdoc />
        public async Task<bool> ExistsByIdPassportNoAsync(string idPassportNo, int excludeTeacherId)
        {
            var normalized = idPassportNo.ToUpperInvariant();
            return await _context.Teachers
                .AsNoTracking()
                .AnyAsync(t => t.IdPassportNo.ToUpper() == normalized && t.Id != excludeTeacherId);
        }
    }
}
