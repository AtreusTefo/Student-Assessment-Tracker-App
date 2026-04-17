using StudentAssessmentTracker.Domain.Entities;
using StudentAssessmentTracker.Domain.Interfaces;
using StudentAssessmentTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace StudentAssessmentTracker.Infrastructure.Repositories
{
    /// <summary>
    /// Concrete assessment repository — student-scoped queries on top of the generic CRUD baseline.
    /// </summary>
    public class StudentAssessmentRepository : Repository<StudentAssessment>, IStudentAssessmentRepository
    {
        /// <summary>Initialises the repository with the application database context.</summary>
        public StudentAssessmentRepository(ApplicationDbContext context) : base(context) { }

        /// <inheritdoc />
        public async Task<IEnumerable<StudentAssessment>> GetByStudentIdAsync(int studentId)
        {
            return await _context.StudentAssessments
                .AsNoTracking()
                .Include(a => a.Submissions)
                .Where(a => a.StudentId == studentId)
                .OrderBy(a => a.DueDate)
                .ThenBy(a => a.Name)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<StudentAssessment?> GetByIdForStudentAsync(int studentId, int assessmentId)
        {
            return await _context.StudentAssessments
                .Include(a => a.Submissions)
                .FirstOrDefaultAsync(a => a.Id == assessmentId && a.StudentId == studentId);
        }

        /// <inheritdoc />
        public async Task<bool> ExistsByNameForStudentAsync(int studentId, string name)
        {
            var normalizedName = name.Trim();
            return await _context.StudentAssessments
                .AsNoTracking()
                .AnyAsync(a => a.StudentId == studentId
                            && a.Name.ToLower() == normalizedName.ToLower());
        }
    }
}
