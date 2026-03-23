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
        /// <summary>Retrieves a student by their ID</summary>
        Task<StudentDto> GetStudentByIdAsync(int id);
        /// <summary>Retrieves all students</summary>
        Task<IEnumerable<StudentDto>> GetAllStudentsAsync();
        /// <summary>Creates a new student</summary>
        Task<StudentDto> CreateStudentAsync(CreateStudentDto dto);
        /// <summary>Updates an existing student</summary>
        Task<StudentDto> UpdateStudentAsync(int id, UpdateStudentDto dto);
        /// <summary>Deletes a student</summary>
        Task DeleteStudentAsync(int id);
        /// <summary>Activates a student account — links StudentUniqueId + Email to a new password</summary>
        Task<StudentLoginResponseDto?> ActivateStudentAsync(StudentActivateDto dto);
        /// <summary>Authenticates a student and returns a login response</summary>
        Task<StudentLoginResponseDto?> LoginStudentAsync(StudentLoginDto dto);
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

        /// <summary>
        /// Initializes a new instance of the StudentService class
        /// </summary>
        /// <param name="repository">Repository for student data access</param>
        /// <param name="mapper">AutoMapper instance for DTO mapping</param>
        /// <param name="logger">Logger for service operations</param>
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
            student.StudentUniqueId = GenerateStudentUniqueId();
            student.CreatedAt = DateTime.UtcNow;
            student.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(student);

            _logger.LogInformation("Student created successfully with ID: {StudentId}, UniqueId: {UniqueId}", student.Id, student.StudentUniqueId);
            return _mapper.Map<StudentDto>(student);
        }

        /// <summary>
        /// Generates a unique student identifier in the format STU-XXXXXXXX
        /// </summary>
        private static string GenerateStudentUniqueId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var suffix = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return $"STU-{suffix}";
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

        /// <summary>
        /// Activates a student account. The student provides their teacher-assigned StudentUniqueId
        /// and the email on file to prove identity, then sets a password for future logins.
        /// </summary>
        public async Task<StudentLoginResponseDto?> ActivateStudentAsync(StudentActivateDto dto)
        {
            _logger.LogInformation("Activation attempt for StudentUniqueId {UniqueId}", dto.StudentUniqueId);

            var students = await _repository.GetAllAsync();
            var student = students.FirstOrDefault(s =>
                s.StudentUniqueId != null &&
                s.StudentUniqueId.Equals(dto.StudentUniqueId, StringComparison.OrdinalIgnoreCase) &&
                s.Email != null &&
                s.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase));

            if (student is null)
            {
                _logger.LogWarning("Activation failed — no match for UniqueId {UniqueId}", dto.StudentUniqueId);
                return null;
            }

            if (!string.IsNullOrEmpty(student.Password))
            {
                _logger.LogWarning("Activation rejected — account already activated for {UniqueId}", dto.StudentUniqueId);
                throw new InvalidOperationException("This account has already been activated. Please use the login page.");
            }

            student.Password = dto.Password;
            student.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(student);

            _logger.LogInformation("Student account activated for {UniqueId}", dto.StudentUniqueId);

            // Re-fetch with navigation properties so the profile has GradeName and Assessments
            var fullStudent = await _repository.GetByIdAsync(student.Id) ?? student;

            return new StudentLoginResponseDto
            {
                Token = $"demo-student-token-{student.Id}-{Guid.NewGuid():N}",
                Student = _mapper.Map<StudentProfileDto>(fullStudent)
            };
        }

        /// <summary>
        /// Authenticates a student using their StudentUniqueId and password.
        /// </summary>
        public async Task<StudentLoginResponseDto?> LoginStudentAsync(StudentLoginDto dto)
        {
            _logger.LogInformation("Student login attempt for {UniqueId}", dto.StudentUniqueId);

            var students = await _repository.GetAllAsync();
            var student = students.FirstOrDefault(s =>
                s.StudentUniqueId != null &&
                s.StudentUniqueId.Equals(dto.StudentUniqueId, StringComparison.OrdinalIgnoreCase));

            if (student is null)
            {
                _logger.LogWarning("Student login failed — UniqueId {UniqueId} not found", dto.StudentUniqueId);
                return null;
            }

            if (string.IsNullOrEmpty(student.Password))
            {
                _logger.LogWarning("Student login failed — account not activated for {UniqueId}", dto.StudentUniqueId);
                throw new InvalidOperationException("Account not activated. Please use the sign-up page to activate your account first.");
            }

            if (student.Password != dto.Password)
            {
                _logger.LogWarning("Student login failed — wrong password for {UniqueId}", dto.StudentUniqueId);
                return null;
            }

            // Re-fetch with navigation properties
            var fullStudent = await _repository.GetByIdAsync(student.Id) ?? student;

            return new StudentLoginResponseDto
            {
                Token = $"demo-student-token-{student.Id}-{Guid.NewGuid():N}",
                Student = _mapper.Map<StudentProfileDto>(fullStudent)
            };
        }
    }
}
