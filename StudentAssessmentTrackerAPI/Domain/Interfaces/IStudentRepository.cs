using StudentAssessmentTracker.Domain.Entities;

namespace StudentAssessmentTracker.Domain.Interfaces
{
    /// <summary>
    /// Specialized student repository — adds teacher-scoped queries on top of the
    /// generic CRUD baseline so that data access is always filtered to the owning
    /// teacher and never leaks cross-teacher data.
    /// </summary>
    public interface IStudentRepository : IRepository<Student>
    {
        /// <summary>Returns only the students that belong to the given teacher.</summary>
        Task<IEnumerable<Student>> GetAllByTeacherAsync(int teacherId);

        /// <summary>
        /// Returns the student with <paramref name="id"/> only when it belongs to
        /// <paramref name="teacherId"/>; returns null otherwise (caller should 404).
        /// </summary>
        Task<Student?> GetByIdForTeacherAsync(int id, int teacherId);

        /// <summary>Fast existence check scoped to the teacher — avoids full loads.</summary>
        Task<bool> ExistsForTeacherAsync(int id, int teacherId);

        /// <summary>Looks up a student by their system-generated unique ID (e.g. STU-XXXX).</summary>
        Task<Student?> FindByUniqueIdAsync(string uniqueId);

        /// <summary>
        /// Returns <c>true</c> if any student (other than <paramref name="excludeStudentId"/>)
        /// already has the given email.  Pass 0 to skip the exclusion (create path).
        /// </summary>
        Task<bool> ExistsByEmailAsync(string email, int excludeStudentId = 0);

        /// <summary>
        /// Returns <c>true</c> if any student (other than <paramref name="excludeStudentId"/>)
        /// already has the given ID/Passport number.  Pass 0 to skip the exclusion
        /// (create path).
        /// </summary>
        Task<bool> ExistsByIdPassportNoAsync(string idPassportNo, int excludeStudentId = 0);

        /// <summary>
        /// Creates a <see cref="TeacherStudent"/> assignment linking <paramref name="studentId"/>
        /// to <paramref name="teacherId"/>.  Silently no-ops when the assignment already exists.
        /// </summary>
        Task AssignToTeacherAsync(int studentId, int teacherId);

        /// <summary>
        /// Removes the <see cref="TeacherStudent"/> assignment between <paramref name="studentId"/>
        /// and <paramref name="teacherId"/>.  Silently no-ops when the assignment does not exist.
        /// </summary>
        Task UnassignFromTeacherAsync(int studentId, int teacherId);

        /// <summary>
        /// Returns <c>true</c> when a <see cref="TeacherStudent"/> row exists for the given
        /// pair — i.e., the teacher is currently assigned to the student.
        /// </summary>
        Task<bool> IsAssignedToTeacherAsync(int studentId, int teacherId);

        /// <summary>
        /// Returns the number of teachers currently assigned to <paramref name="studentId"/>.
        /// Used to prevent unassigning the last teacher, which would leave an orphaned student.
        /// </summary>
        Task<int> CountTeacherAssignmentsAsync(int studentId);
    }
}
