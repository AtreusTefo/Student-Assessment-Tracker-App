using AutoMapper;
using System.Text.Json;
using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Domain.Entities;
using StudentAssessmentTracker.Domain.Interfaces;

namespace StudentAssessmentTracker.Application.Services
{
    /// <summary>
    /// Defines the contract for assessment lifecycle operations scoped to a student
    /// and validated against the calling teacher's ownership.
    /// </summary>
    public interface IStudentAssessmentService
    {
        /// <summary>Returns all assessments for <paramref name="studentId"/>, verified to belong to <paramref name="teacherId"/>.</summary>
        Task<IEnumerable<StudentAssessmentDto>> GetByStudentIdAsync(int studentId, int teacherId);

        /// <summary>
        /// Returns the assessment with <paramref name="assessmentId"/> for the given student.
        /// Throws <see cref="KeyNotFoundException"/> when the student or assessment is not found.
        /// </summary>
        Task<StudentAssessmentDto> GetByIdAsync(int studentId, int assessmentId, int teacherId);

        /// <summary>
        /// Creates a new assessment record for <paramref name="studentId"/>.
        /// Throws <see cref="KeyNotFoundException"/> when the student does not belong to the teacher.
        /// </summary>
        Task<StudentAssessmentDto> AddAsync(int studentId, CreateStudentAssessmentDto dto, int teacherId);

        /// <summary>
        /// Updates an existing assessment for <paramref name="studentId"/>.
        /// Throws <see cref="KeyNotFoundException"/> when the student or assessment is not found.
        /// </summary>
        Task<StudentAssessmentDto> UpdateAsync(int studentId, int assessmentId, UpdateStudentAssessmentDto dto, int teacherId);

        /// <summary>
        /// Deletes the assessment with <paramref name="assessmentId"/> from the given student.
        /// Throws <see cref="KeyNotFoundException"/> when the student or assessment is not found.
        /// </summary>
        Task DeleteAsync(int studentId, int assessmentId, int teacherId);
    }

    /// <summary>
    /// Manages the independent assessment lifecycle for a student.
    /// Each assessment has its own name, max score, score, and optional due date —
    /// fixing the fixed-count, hardcoded-max, and update-whole-record problems.
    /// </summary>
    public class StudentAssessmentService : IStudentAssessmentService
    {
        private readonly IStudentAssessmentRepository _assessmentRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IAuditLogService _auditLog;
        private readonly IMapper _mapper;
        private readonly ILogger<StudentAssessmentService> _logger;

        /// <summary>Initialises the service with the assessment repository, student repository, mapper, and logger.</summary>
        public StudentAssessmentService(
            IStudentAssessmentRepository assessmentRepository,
            IStudentRepository studentRepository,
            IAuditLogService auditLog,
            IMapper mapper,
            ILogger<StudentAssessmentService> logger)
        {
            _assessmentRepository = assessmentRepository;
            _studentRepository = studentRepository;
            _auditLog = auditLog;
            _mapper = mapper;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<StudentAssessmentDto>> GetByStudentIdAsync(int studentId, int teacherId)
        {
            await EnsureStudentBelongsToTeacherAsync(studentId, teacherId);
            var assessments = await _assessmentRepository.GetByStudentIdAsync(studentId);
            return _mapper.Map<IEnumerable<StudentAssessmentDto>>(assessments);
        }

        /// <inheritdoc />
        public async Task<StudentAssessmentDto> GetByIdAsync(int studentId, int assessmentId, int teacherId)
        {
            await EnsureStudentBelongsToTeacherAsync(studentId, teacherId);
            var assessment = await FindAssessmentAsync(studentId, assessmentId);
            return _mapper.Map<StudentAssessmentDto>(assessment);
        }

        /// <inheritdoc />
        public async Task<StudentAssessmentDto> AddAsync(int studentId, CreateStudentAssessmentDto dto, int teacherId)
        {
            await EnsureStudentBelongsToTeacherAsync(studentId, teacherId);

            // Issue #5: prevent duplicate assessment names per student before hitting the
            // DB unique index. Two assessments named "Test 1" for the same student produce
            // ambiguous reports and confuse students reviewing their results.
            var duplicate = await _assessmentRepository.ExistsByNameForStudentAsync(studentId, dto.Name!);
            if (duplicate)
                throw new InvalidOperationException(
                    $"An assessment named '{dto.Name}' already exists for student {studentId}.");

            var assessment = _mapper.Map<StudentAssessment>(dto);
            assessment.StudentId = studentId;
            assessment.CreatedAt = DateTime.UtcNow;
            assessment.UpdatedAt = DateTime.UtcNow;
            await _assessmentRepository.AddAsync(assessment);
            _logger.LogInformation("Assessment '{Name}' added to student {StudentId}", dto.Name, studentId);

            await _auditLog.LogAsync("StudentAssessment", assessment.Id, "Create",
                oldValues: null,
                newValues: JsonSerializer.Serialize(new { assessment.Name, assessment.MaxScore, assessment.Score, assessment.StudentId }),
                changedBy: teacherId.ToString(), changedByRole: "Teacher");

            return _mapper.Map<StudentAssessmentDto>(assessment);
        }

        /// <inheritdoc />
        public async Task<StudentAssessmentDto> UpdateAsync(int studentId, int assessmentId, UpdateStudentAssessmentDto dto, int teacherId)
        {
            await EnsureStudentBelongsToTeacherAsync(studentId, teacherId);
            var assessment = await FindAssessmentAsync(studentId, assessmentId);

            // Prevent renaming to a name already used by another assessment on the same student.
            // Without this, the DB unique index UX_StudentAssessments_StudentId_Name fires and
            // surfaces as an unhandled DbUpdateException → HTTP 500 instead of a clean 409.
            if (!string.Equals(dto.Name, assessment.Name, StringComparison.OrdinalIgnoreCase) &&
                await _assessmentRepository.ExistsByNameForStudentAsync(studentId, dto.Name!))
                throw new InvalidOperationException(
                    $"An assessment named '{dto.Name}' already exists for student {studentId}.");

            _mapper.Map(dto, assessment);
            assessment.UpdatedAt = DateTime.UtcNow;
            await _assessmentRepository.UpdateAsync(assessment);
            _logger.LogInformation("Assessment {AssessmentId} for student {StudentId} updated", assessmentId, studentId);

            await _auditLog.LogAsync("StudentAssessment", assessmentId, "Update",
                oldValues: JsonSerializer.Serialize(new { assessment.Name, assessment.MaxScore, assessment.Score }),
                newValues: JsonSerializer.Serialize(new { dto.Name, dto.MaxScore, dto.Score }),
                changedBy: teacherId.ToString(), changedByRole: "Teacher");

            return _mapper.Map<StudentAssessmentDto>(assessment);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(int studentId, int assessmentId, int teacherId)
        {
            await EnsureStudentBelongsToTeacherAsync(studentId, teacherId);
            var assessment = await FindAssessmentAsync(studentId, assessmentId);
            await _assessmentRepository.DeleteAsync(assessment.Id);
            _logger.LogInformation("Assessment {AssessmentId} for student {StudentId} deleted", assessmentId, studentId);

            await _auditLog.LogAsync("StudentAssessment", assessmentId, "Delete",
                oldValues: JsonSerializer.Serialize(new { assessment.Name, assessment.StudentId }),
                newValues: null,
                changedBy: teacherId.ToString(), changedByRole: "Teacher");
        }

        // Verifies the student exists AND is assigned to the calling teacher — prevents cross-teacher data access
        private async Task EnsureStudentBelongsToTeacherAsync(int studentId, int teacherId)
        {
            var owned = await _studentRepository.IsAssignedToTeacherAsync(studentId, teacherId);
            if (!owned)
                throw new KeyNotFoundException($"Student {studentId} not found or not assigned to you.");
        }

        private async Task<StudentAssessment> FindAssessmentAsync(int studentId, int assessmentId)
        {
            var assessment = await _assessmentRepository.GetByIdForStudentAsync(studentId, assessmentId);
            if (assessment == null)
                throw new KeyNotFoundException($"Assessment {assessmentId} for student {studentId} not found");
            return assessment;
        }
    }
}
