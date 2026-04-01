using StudentAssessmentTracker.Domain.Entities;

namespace StudentAssessmentTracker.Domain.Interfaces
{
    /// <summary>
    /// Repository contract for student assessment persistence.
    /// Extends the generic CRUD baseline with student-scoped queries.
    /// </summary>
    public interface IStudentAssessmentRepository : IRepository<StudentAssessment>
    {
        /// <summary>
        /// Returns all assessments for <paramref name="studentId"/>, ordered by due date then name.
        /// </summary>
        Task<IEnumerable<StudentAssessment>> GetByStudentIdAsync(int studentId);

        /// <summary>
        /// Returns the assessment with <paramref name="assessmentId"/> that belongs to
        /// <paramref name="studentId"/>, or <c>null</c> when not found.
        /// </summary>
        Task<StudentAssessment?> GetByIdForStudentAsync(int studentId, int assessmentId);
    }
}
