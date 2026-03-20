using StudentAssessmentTracker.Domain.Entities;
using StudentAssessmentTracker.Domain.Interfaces;
using StudentAssessmentTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace StudentAssessmentTracker.Infrastructure.Repositories
{
    /// <summary>
    /// Teacher-specific repository that includes the SubjectNavigation property
    /// so AutoMapper can resolve SubjectName without a separate lookup.
    /// </summary>
    public class TeacherRepository : Repository<Teacher>
    {
        public TeacherRepository(ApplicationDbContext context) : base(context) { }

        /// <inheritdoc/>
        public override async Task<Teacher?> GetByIdAsync(int id)
        {
            return await _context.Teachers
                .Include(t => t.SubjectNavigation)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<Teacher>> GetAllAsync()
        {
            return await _context.Teachers
                .Include(t => t.SubjectNavigation)
                .ToListAsync();
        }
    }
}
