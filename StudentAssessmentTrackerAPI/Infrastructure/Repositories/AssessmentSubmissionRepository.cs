using Microsoft.EntityFrameworkCore;
using StudentAssessmentTracker.Domain.Entities;
using StudentAssessmentTracker.Domain.Interfaces;
using StudentAssessmentTracker.Infrastructure.Data;

namespace StudentAssessmentTracker.Infrastructure.Repositories
{
    /// <summary>
    /// EF Core repository for <see cref="AssessmentSubmission"/> persistence.
    /// </summary>
    public class AssessmentSubmissionRepository : IAssessmentSubmissionRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>Initialises the repository with the application database context.</summary>
        public AssessmentSubmissionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AssessmentSubmission>> GetByAssessmentAndStudentAsync(int assessmentId, int studentId)
        {
            return await _context.AssessmentSubmissions
                .AsNoTracking()
                .Where(s => s.StudentAssessmentId == assessmentId && s.StudentId == studentId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<AssessmentSubmission?> GetByIdAsync(int submissionId)
        {
            return await _context.AssessmentSubmissions.FindAsync(submissionId);
        }

        /// <inheritdoc />
        public async Task AddAsync(AssessmentSubmission submission)
        {
            await _context.AssessmentSubmissions.AddAsync(submission);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task DeleteAsync(AssessmentSubmission submission)
        {
            _context.AssessmentSubmissions.Remove(submission);
            await _context.SaveChangesAsync();
        }
    }
}
