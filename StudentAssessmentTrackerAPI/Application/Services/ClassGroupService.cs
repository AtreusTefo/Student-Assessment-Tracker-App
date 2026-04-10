using AutoMapper;
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
        private readonly ILogger<ClassGroupService> _logger;

        /// <summary>Initialises the service.</summary>
        public ClassGroupService(ApplicationDbContext db, ILogger<ClassGroupService> logger)
        {
            _db = db;
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

            var group = new ClassGroup
            {
                Name = dto.Name!.Trim(),
                SubjectId = dto.SubjectId,
                GradeId = dto.GradeId,
                TeacherId = teacherId,
                CreatedAt = DateTime.UtcNow
            };

            _db.ClassGroups.Add(group);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Class group '{Name}' created by teacher {TeacherId}", group.Name, teacherId);

            return await GetByIdAsync(group.Id, teacherId);
        }

        /// <inheritdoc />
        public async Task<ClassGroupDto> UpdateAsync(int classGroupId, UpdateClassGroupDto dto, int teacherId)
        {
            var group = await _db.ClassGroups
                .FirstOrDefaultAsync(cg => cg.Id == classGroupId && cg.TeacherId == teacherId)
                ?? throw new KeyNotFoundException($"Class group {classGroupId} not found.");

            group.Name = dto.Name!.Trim();
            await _db.SaveChangesAsync();
            return await GetByIdAsync(classGroupId, teacherId);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(int classGroupId, int teacherId)
        {
            var group = await _db.ClassGroups
                .FirstOrDefaultAsync(cg => cg.Id == classGroupId && cg.TeacherId == teacherId)
                ?? throw new KeyNotFoundException($"Class group {classGroupId} not found.");

            _db.ClassGroups.Remove(group);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Class group {Id} deleted by teacher {TeacherId}", classGroupId, teacherId);
        }

        /// <inheritdoc />
        public async Task EnrollStudentAsync(int classGroupId, int studentId, int teacherId)
        {
            await LoadGroupAsync(classGroupId, teacherId); // validates ownership

            // Verify the student belongs to this teacher
            var assigned = await _db.TeacherStudents
                .AnyAsync(ts => ts.TeacherId == teacherId && ts.StudentId == studentId);
            if (!assigned)
                throw new KeyNotFoundException($"Student {studentId} is not assigned to teacher {teacherId}.");

            var exists = await _db.ClassGroupStudents
                .AnyAsync(cgs => cgs.ClassGroupId == classGroupId && cgs.StudentId == studentId);
            if (exists) return; // idempotent

            _db.ClassGroupStudents.Add(new ClassGroupStudent
            {
                ClassGroupId = classGroupId,
                StudentId = studentId,
                EnrolledAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
            _logger.LogInformation("Student {StudentId} enrolled in class group {ClassGroupId}", studentId, classGroupId);
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
            await _db.SaveChangesAsync();
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
