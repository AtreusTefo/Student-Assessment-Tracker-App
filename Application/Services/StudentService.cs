using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Domain.Entities;
using StudentAssessmentTracker.Domain.Interfaces;
using AutoMapper;

namespace StudentAssessmentTracker.Application.Services
{
    /// <summary>
    /// Service interface for student business logic operations
    /// Abstracts application-level operations from controllers
    /// </summary>
    public interface IStudentService
    {
        Task<StudentDto> GetStudentByIdAsync(int id);
        Task<IEnumerable<StudentDto>> GetAllStudentsAsync();
        Task<StudentDto> CreateStudentAsync(CreateStudentDto dto);
        Task<StudentDto> UpdateStudentAsync(int id, UpdateStudentDto dto);
        Task DeleteStudentAsync(int id);
    }

    /// <summary>
    /// Student service implementation
    /// Handles business logic, validation orchestration, and data transformation
    /// </summary>
    public class StudentService : IStudentService
    {
        private readonly IRepository<Student> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<StudentService> _logger;

        public StudentService(
            IRepository<Student> repository,
            IMapper mapper,
            ILogger<StudentService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a student by ID and maps to DTO
        /// </summary>
        public async Task<StudentDto> GetStudentByIdAsync(int id)
        {
            _logger.LogInformation("Fetching student with ID: {StudentId}", id);

            var student = await _repository.GetByIdAsync(id);
            if (student == null)
            {
                _logger.LogWarning("Student with ID {StudentId} not found", id);
                throw new KeyNotFoundException($"Student with ID {id} not found");
            }

            return _mapper.Map<StudentDto>(student);
        }

        /// <summary>
        /// Retrieves all students and maps to DTOs
        /// </summary>
        public async Task<IEnumerable<StudentDto>> GetAllStudentsAsync()
        {
            _logger.LogInformation("Fetching all students");

            var students = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<StudentDto>>(students);
        }

        /// <summary>
        /// Creates a new student from DTO
        /// </summary>
        public async Task<StudentDto> CreateStudentAsync(CreateStudentDto dto)
        {
            _logger.LogInformation("Creating new student: {FirstName} {LastName}", dto.FirstName, dto.LastName);

            var student = _mapper.Map<Student>(dto);
            student.CreatedAt = DateTime.UtcNow;
            student.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(student);

            _logger.LogInformation("Student created successfully with ID: {StudentId}", student.Id);
            return _mapper.Map<StudentDto>(student);
        }

        /// <summary>
        /// Updates an existing student
        /// </summary>
        public async Task<StudentDto> UpdateStudentAsync(int id, UpdateStudentDto dto)
        {
            _logger.LogInformation("Updating student with ID: {StudentId}", id);

            var student = await _repository.GetByIdAsync(id);
            if (student == null)
            {
                _logger.LogWarning("Student with ID {StudentId} not found for update", id);
                throw new KeyNotFoundException($"Student with ID {id} not found");
            }

            // Map DTO to existing student entity
            _mapper.Map(dto, student);
            student.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(student);

            _logger.LogInformation("Student with ID {StudentId} updated successfully", id);
            return _mapper.Map<StudentDto>(student);
        }

        /// <summary>
        /// Deletes a student by ID
        /// </summary>
        public async Task DeleteStudentAsync(int id)
        {
            _logger.LogInformation("Deleting student with ID: {StudentId}", id);

            var student = await _repository.GetByIdAsync(id);
            if (student == null)
            {
                _logger.LogWarning("Student with ID {StudentId} not found for deletion", id);
                throw new KeyNotFoundException($"Student with ID {id} not found");
            }

            await _repository.DeleteAsync(id);

            _logger.LogInformation("Student with ID {StudentId} deleted successfully", id);
        }
    }
}
