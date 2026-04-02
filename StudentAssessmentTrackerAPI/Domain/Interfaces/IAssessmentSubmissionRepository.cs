using StudentAssessmentTracker.Domain.Entities;

namespace StudentAssessmentTracker.Domain.Interfaces
{
    /// <summary>
    /// Repository contract for <see cref="AssessmentSubmission"/> persistence.
    /// </summary>
    public interface IAssessmentSubmissionRepository
    {
        /// <summary>Returns all submissions for a given assessment and student.</summary>
        Task<IEnumerable<AssessmentSubmission>> GetByAssessmentAndStudentAsync(int assessmentId, int studentId);

        /// <summary>Returns a single submission by its primary key, or null if not found.</summary>
        Task<AssessmentSubmission?> GetByIdAsync(int submissionId);

        /// <summary>Persists a new submission row.</summary>
        Task AddAsync(AssessmentSubmission submission);

        /// <summary>Removes a submission row permanently.</summary>
        Task DeleteAsync(AssessmentSubmission submission);
    }
}
