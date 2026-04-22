using Microsoft.EntityFrameworkCore;
using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Domain.Entities;
using StudentAssessmentTracker.Infrastructure.Data;

namespace StudentAssessmentTracker.Application.Services
{
    /// <summary>Contract for class group management operations.</summary>
    public interface IClassGroupService
    {
        /// <summary>Returns all class groups owned by the given teacher.</summary>
        Task<IEnumerable<ClassGroupDto>> GetAllByTeacherAsync(int teacherId);

        /// <summary>Returns a single class group by ID, scoped to the teacher.</summary>
        Task<ClassGroupDto> GetByIdAsync(int classGroupId, int teacherId);

        /// <summary>Creates a new class group under the authenticated teacher.</summary>
        Task<ClassGroupDto> CreateAsync(CreateClassGroupDto dto, int teacherId);

        /// <summary>Updates the name of an existing class group.</summary>
        Task<ClassGroupDto> UpdateAsync(int classGroupId, UpdateClassGroupDto dto, int teacherId);

        /// <summary>Deletes a class group and all its enrollments.</summary>
        Task DeleteAsync(int classGroupId, int teacherId);

        /// <summary>Enrolls a student in a class group. Idempotent.</summary>
        Task EnrollStudentAsync(int classGroupId, int studentId, int teacherId);

        /// <summary>Removes a student from a class group.</summary>
        Task UnenrollStudentAsync(int classGroupId, int studentId, int teacherId);
    }

    /// <summary>Manages class group lifecycle and student enrollment operations.</summary>
    public class ClassGroupService : IClassGroupService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAuditLogService _auditLog;
        private readonly ILogger<ClassGroupService> _logger;

        /// <summary>Initialises the service.</summary>
        public ClassGroupService(
            ApplicationDbContext db,
            IAuditLogService auditLog,
            ILogger<ClassGroupService> logger)
        {
            _db = db;
            _auditLog = auditLog;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ClassGroupDto>> GetAllByTeacherAsync(int teacherId)
        {
            var groups = await _db.ClassGroups
                .AsNoTracking()
                .Where(cg => cg.TeacherId == teacherId)
                .Include(cg => cg.Subject)
                .Include(cg => cg.Grade)
                .Include(cg => cg.Enrollments)
                    .ThenInclude(e => e.Student)
                .OrderBy(cg => cg.Name)
                .ToListAsync();

            return groups.Select(MapToDto);
        }

        /// <inheritdoc />
        public async Task<ClassGroupDto> GetByIdAsync(int classGroupId, int teacherId)
        {
            var group = await LoadGroupAsync(classGroupId, teacherId);
            return MapToDto(group);
        }

        /// <inheritdoc />
        public async Task<ClassGroupDto> CreateAsync(CreateClassGroupDto dto, int teacherId)
        {
            // Validate FK lookups
            if (!await _db.Subjects.AnyAsync(s => s.Id == dto.SubjectId))
                throw new ArgumentException($"Subject {dto.SubjectId} does not exist.");
            if (!await _db.Grades.AnyAsync(g => g.Id == dto.GradeId))
                throw new ArgumentException($"Grade {dto.GradeId} does not exist.");

            // Issue #3: Class group subject must match the owning teacher's registered subject.
            // A Maths teacher cannot create an English class group.
            var teacher = await _db.Teachers.FindAsync(teacherId)
                ?? throw new KeyNotFoundException($"Teacher {teacherId} not found.");
            if (teacher.SubjectId != dto.SubjectId)
                throw new ArgumentException(
                    $"Class group subject (ID {dto.SubjectId}) must match the teacher's registered subject (ID {teacher.SubjectId}).");

            var group = new ClassGroup
            {
                Name = dto.Name!.Trim(),
                SubjectId = dto.SubjectId,
                GradeId = dto.GradeId,
                TeacherId = teacherId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.ClassGroups.Add(group);

            // Issue #6: use an explicit transaction so the mutation and the audit log entry
            // are committed atomically. Both SaveChangesAsync calls share the same DbContext
            // (scoped DI) and therefore the same underlying connection and transaction.
            await using var tx = await _db.Database.BeginTransactionAsync();
            await _db.SaveChangesAsync();
            _logger.LogInformation("Class group '{Name}' created by teacher {TeacherId}", group.Name, teacherId);

            // Issue #7: audit all class group mutations
            await _auditLog.LogAsync("ClassGroup", group.Id, "Create",
                oldValues: null,
                newValues: System.Text.Json.JsonSerializer.Serialize(new { group.Name, group.SubjectId, group.GradeId }),
                changedBy: teacherId.ToString(), changedByRole: "Teacher");

            await tx.CommitAsync();

            return await GetByIdAsync(group.Id, teacherId);
        }

        /// <inheritdoc />
        public async Task<ClassGroupDto> UpdateAsync(int classGroupId, UpdateClassGroupDto dto, int teacherId)
        {
            var group = await _db.ClassGroups
                .FirstOrDefaultAsync(cg => cg.Id == classGroupId && cg.TeacherId == teacherId)
                ?? throw new KeyNotFoundException($"Class group {classGroupId} not found.");

            var oldName = group.Name;
            group.Name = dto.Name!.Trim();
            group.UpdatedAt = DateTime.UtcNow;

            await using var tx = await _db.Database.BeginTransactionAsync();
            await _db.SaveChangesAsync();

            // Issue #7: audit update
            await _auditLog.LogAsync("ClassGroup", classGroupId, "Update",
                oldValues: System.Text.Json.JsonSerializer.Serialize(new { Name = oldName }),
                newValues: System.Text.Json.JsonSerializer.Serialize(new { group.Name }),
                changedBy: teacherId.ToString(), changedByRole: "Teacher");

            await tx.CommitAsync();

            return await GetByIdAsync(classGroupId, teacherId);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(int classGroupId, int teacherId)
        {
            var group = await _db.ClassGroups
                .FirstOrDefaultAsync(cg => cg.Id == classGroupId && cg.TeacherId == teacherId)
                ?? throw new KeyNotFoundException($"Class group {classGroupId} not found.");

            _db.ClassGroups.Remove(group);

            await using var tx = await _db.Database.BeginTransactionAsync();
            await _db.SaveChangesAsync();
            _logger.LogInformation("Class group {Id} deleted by teacher {TeacherId}", classGroupId, teacherId);

            // Issue #7: audit delete
            await _auditLog.LogAsync("ClassGroup", classGroupId, "Delete",
                oldValues: System.Text.Json.JsonSerializer.Serialize(new { group.Name }),
                newValues: null,
                changedBy: teacherId.ToString(), changedByRole: "Teacher");

            await tx.CommitAsync();
        }

        /// <inheritdoc />
        public async Task EnrollStudentAsync(int classGroupId, int studentId, int teacherId)
        {
            var group = await LoadGroupAsync(classGroupId, teacherId); // validates ownership

            // Verify the student belongs to this teacher
            var assigned = await _db.TeacherStudents
                .AnyAsync(ts => ts.TeacherId == teacherId && ts.StudentId == studentId);
            if (!assigned)
                throw new KeyNotFoundException($"Student {studentId} is not assigned to teacher {teacherId}.");

            // Issue #1: grade consistency check — student's grade must match the class group's grade.
            // Cannot enroll a Grade 9 student into a Grade 10 class group.
            var student = await _db.Students.FindAsync(studentId)
                ?? throw new KeyNotFoundException($"Student {studentId} not found.");
            if (student.GradeId != group.GradeId)
                throw new InvalidOperationException(
                    $"Student grade (ID {student.GradeId}) does not match class group grade (ID {group.GradeId}). " +
                    "A student can only be enrolled in class groups that match their grade level.");

            var exists = await _db.ClassGroupStudents
                .AnyAsync(cgs => cgs.ClassGroupId == classGroupId && cgs.StudentId == studentId);
            if (exists) return; // idempotent

            // Issue #7: prevent a student from being enrolled in two class groups that teach
            // the same subject simultaneously. This mirrors the TeacherStudent unique index
            // UX_TeacherStudents_StudentId_SubjectId and prevents the student receiving
            // duplicate broadcast assessments from two parallel groups.
            // Use a correlated subquery to avoid fragile nullable navigation in LINQ-to-SQL.
            var subjectConflict = await _db.ClassGroupStudents
                .AnyAsync(cgs => cgs.StudentId == studentId
                              && cgs.ClassGroupId != classGroupId
                              && _db.ClassGroups.Any(cg =>
                                     cg.Id == cgs.ClassGroupId
                                     && cg.SubjectId == group.SubjectId));
            if (subjectConflict)
                throw new InvalidOperationException(
                    $"Student {studentId} is already enrolled in another class group for subject {group.SubjectId}. " +
                    "Unenroll from the existing group before enrolling in a parallel one.");

            _db.ClassGroupStudents.Add(new ClassGroupStudent
            {
                ClassGroupId = classGroupId,
                StudentId = studentId,
                EnrolledAt = DateTime.UtcNow
            });

            await using var enrollTx = await _db.Database.BeginTransactionAsync();
            await _db.SaveChangesAsync();
            _logger.LogInformation("Student {StudentId} enrolled in class group {ClassGroupId}", studentId, classGroupId);

            // Issue #7: audit enrollment
            await _auditLog.LogAsync("ClassGroupStudent", classGroupId, "Create",
                oldValues: null,
                newValues: System.Text.Json.JsonSerializer.Serialize(new { classGroupId, studentId }),
                changedBy: teacherId.ToString(), changedByRole: "Teacher");

            await enrollTx.CommitAsync();
        }

        /// <inheritdoc />
        public async Task UnenrollStudentAsync(int classGroupId, int studentId, int teacherId)
        {
            await LoadGroupAsync(classGroupId, teacherId); // validates ownership

            var enrollment = await _db.ClassGroupStudents
                .FirstOrDefaultAsync(cgs => cgs.ClassGroupId == classGroupId && cgs.StudentId == studentId)
                ?? throw new KeyNotFoundException(
                    $"Student {studentId} is not enrolled in class group {classGroupId}.");

            _db.ClassGroupStudents.Remove(enrollment);

            await using var unenrollTx = await _db.Database.BeginTransactionAsync();
            await _db.SaveChangesAsync();

            // Issue #7: audit unenrollment
            await _auditLog.LogAsync("ClassGroupStudent", classGroupId, "Delete",
                oldValues: System.Text.Json.JsonSerializer.Serialize(new { classGroupId, studentId }),
                newValues: null,
                changedBy: teacherId.ToString(), changedByRole: "Teacher");

            await unenrollTx.CommitAsync();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private async Task<ClassGroup> LoadGroupAsync(int classGroupId, int teacherId)
        {
            return await _db.ClassGroups
                .Include(cg => cg.Subject)
                .Include(cg => cg.Grade)
                .Include(cg => cg.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(cg => cg.Id == classGroupId && cg.TeacherId == teacherId)
                ?? throw new KeyNotFoundException($"Class group {classGroupId} not found.");
        }

        private static ClassGroupDto MapToDto(ClassGroup cg) => new()
        {
            Id = cg.Id,
            Name = cg.Name,
            SubjectId = cg.SubjectId,
            SubjectName = cg.Subject?.Name ?? string.Empty,
            GradeId = cg.GradeId,
            GradeName = cg.Grade?.Name ?? string.Empty,
            TeacherId = cg.TeacherId,
            CreatedAt = cg.CreatedAt,
            StudentCount = cg.Enrollments.Count,
            Students = cg.Enrollments.Select(e => new ClassGroupMemberDto
            {
                StudentId = e.StudentId,
                StudentUniqueId = e.Student?.StudentUniqueId ?? string.Empty,
                FullName = e.Student is { } s ? $"{s.FirstName} {s.LastName}" : string.Empty,
                EnrolledAt = e.EnrolledAt
            }).ToList()
        };
    }
}
