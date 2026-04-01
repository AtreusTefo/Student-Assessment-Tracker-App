using StudentAssessmentTracker.Domain.Entities;

namespace StudentAssessmentTracker.Domain.Interfaces
{
    /// <summary>
    /// Specialized teacher repository — adds an email-based lookup so that the login
    /// path issues a single indexed DB query instead of loading the entire table.
    /// </summary>
    public interface ITeacherRepository : IRepository<Teacher>
    {
        /// <summary>
        /// Returns the teacher whose email matches (case-insensitive) or null if not
        /// found.  Uses a server-side filter — never loads the full table.
        /// </summary>
        Task<Teacher?> FindByEmailAsync(string email);

        /// <summary>
        /// Returns <c>true</c> when the teacher with <paramref name="teacherId"/> has at
        /// least one student registered.  Used to guard the FK RESTRICT constraint
        /// before attempting a delete, so the service can return 409 instead of 500.
        /// </summary>
        Task<bool> HasStudentsAsync(int teacherId);

        /// <summary>
        /// Returns <c>true</c> if any teacher already holds the given
        /// <paramref name="idPassportNo"/> (case-insensitive).  Used for duplicate
        /// detection before insert.
        /// </summary>
        Task<bool> ExistsByIdPassportNoAsync(string idPassportNo);

        /// <summary>
        /// Returns <c>true</c> if any teacher (other than <paramref name="excludeTeacherId"/>)
        /// already holds the given <paramref name="email"/> (case-insensitive).  Pass 0 to
        /// skip the exclusion (create path).
        /// </summary>
        Task<bool> ExistsByEmailAsync(string email, int excludeTeacherId = 0);

        /// <summary>
        /// Returns <c>true</c> if any teacher (other than <paramref name="excludeTeacherId"/>)
        /// already holds the given <paramref name="idPassportNo"/> (case-insensitive).
        /// Pass 0 to skip the exclusion (create path).
        /// </summary>
        Task<bool> ExistsByIdPassportNoAsync(string idPassportNo, int excludeTeacherId);
    }
}
