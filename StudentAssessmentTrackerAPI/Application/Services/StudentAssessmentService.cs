using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Domain.Entities;
using StudentAssessmentTracker.Infrastructure.Data;

namespace StudentAssessmentTracker.Application.Services
{
    /// <summary>
    /// Service interface for managing student assessments
    /// </summary>
    public interface IStudentAssessmentService
    {
        Task<IEnumerable<StudentAssessmentDto>> GetByStudentIdAsync(int studentId);
        Task<StudentAssessmentDto> GetByIdAsync(int studentId, int assessmentId);
        Task<StudentAssessmentDto> AddAsync(int studentId, CreateStudentAssessmentDto dto);
        Task<StudentAssessmentDto> UpdateAsync(int studentId, int assessmentId, UpdateStudentAssessmentDto dto);
        Task DeleteAsync(int studentId, int assessmentId);
    }

    /// <summary>
    /// Manages the independent assessment lifecycle for a student.
    /// Each assessment has its own name, max score, score, and optional due date —
    /// fixing the fixed-count, hardcoded-max, and update-whole-record problems.
    /// </summary>
    public class StudentAssessmentService : IStudentAssessmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<StudentAssessmentService> _logger;

        public StudentAssessmentService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<StudentAssessmentService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<StudentAssessmentDto>> GetByStudentIdAsync(int studentId)
        {
            await EnsureStudentExistsAsync(studentId);

            var assessments = await _context.StudentAssessments
                .AsNoTracking()
                .Where(a => a.StudentId == studentId)
                .OrderBy(a => a.DueDate)
                .ThenBy(a => a.Name)
                .ToListAsync();

            return _mapper.Map<IEnumerable<StudentAssessmentDto>>(assessments);
        }

        public async Task<StudentAssessmentDto> GetByIdAsync(int studentId, int assessmentId)
        {
            var assessment = await FindAssessmentAsync(studentId, assessmentId);
            return _mapper.Map<StudentAssessmentDto>(assessment);
        }

        public async Task<StudentAssessmentDto> AddAsync(int studentId, CreateStudentAssessmentDto dto)
        {
            await EnsureStudentExistsAsync(studentId);

            var assessment = _mapper.Map<StudentAssessment>(dto);
            assessment.StudentId = studentId;
            assessment.CreatedAt = DateTime.UtcNow;
            assessment.UpdatedAt = DateTime.UtcNow;

            _context.StudentAssessments.Add(assessment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Assessment '{Name}' added to student {StudentId}", dto.Name, studentId);
            return _mapper.Map<StudentAssessmentDto>(assessment);
        }

        public async Task<StudentAssessmentDto> UpdateAsync(int studentId, int assessmentId, UpdateStudentAssessmentDto dto)
        {
            var assessment = await FindAssessmentAsync(studentId, assessmentId);

            _mapper.Map(dto, assessment);
            assessment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Assessment {AssessmentId} for student {StudentId} updated", assessmentId, studentId);
            return _mapper.Map<StudentAssessmentDto>(assessment);
        }

        public async Task DeleteAsync(int studentId, int assessmentId)
        {
            var assessment = await FindAssessmentAsync(studentId, assessmentId);

            _context.StudentAssessments.Remove(assessment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Assessment {AssessmentId} for student {StudentId} deleted", assessmentId, studentId);
        }

        private async Task EnsureStudentExistsAsync(int studentId)
        {
            var exists = await _context.Students.AnyAsync(s => s.Id == studentId);
            if (!exists)
                throw new KeyNotFoundException($"Student with ID {studentId} not found");
        }

        private async Task<StudentAssessment> FindAssessmentAsync(int studentId, int assessmentId)
        {
            var assessment = await _context.StudentAssessments
                .FirstOrDefaultAsync(a => a.Id == assessmentId && a.StudentId == studentId);

            if (assessment == null)
                throw new KeyNotFoundException(
                    $"Assessment {assessmentId} for student {studentId} not found");

            return assessment;
        }
    }
}
