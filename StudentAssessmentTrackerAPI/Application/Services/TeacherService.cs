using AutoMapper;
using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Domain.Entities;
using StudentAssessmentTracker.Domain.Interfaces;

namespace StudentAssessmentTracker.Application.Services
{
    /// <summary>
    /// Contract for teacher business logic operations
    /// </summary>
    public interface ITeacherService
    {
        /// <summary>Retrieves all teachers</summary>
        Task<IEnumerable<TeacherResponseDto>> GetAllTeachersAsync();

        /// <summary>Retrieves a teacher by their identifier</summary>
        Task<TeacherResponseDto?> GetTeacherByIdAsync(int id);

        /// <summary>Registers a new teacher</summary>
        Task<TeacherResponseDto> CreateTeacherAsync(TeacherRegisterDto dto);

        /// <summary>Updates an existing teacher's data</summary>
        Task<bool> UpdateTeacherAsync(int id, TeacherUpdateDto dto);

        /// <summary>Deletes a teacher by their identifier</summary>
        Task<bool> DeleteTeacherAsync(int id);

        /// <summary>Authenticates a teacher and returns a login response</summary>
        Task<TeacherLoginResponseDto?> LoginAsync(TeacherLoginDto dto);
    }

    /// <summary>
    /// Application service implementing teacher business logic
    /// Follows Clean Architecture — depends on domain abstractions only
    /// </summary>
    public class TeacherService : ITeacherService
    {
        private readonly IRepository<Teacher> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<TeacherService> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="TeacherService"/>
        /// </summary>
        /// <param name="repository">Generic repository for Teacher entity</param>
        /// <param name="mapper">AutoMapper instance</param>
        /// <param name="logger">Logger instance</param>
        public TeacherService(IRepository<Teacher> repository, IMapper mapper, ILogger<TeacherService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TeacherResponseDto>> GetAllTeachersAsync()
        {
            _logger.LogInformation("Retrieving all teachers");
            var teachers = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<TeacherResponseDto>>(teachers);
        }

        /// <inheritdoc/>
        public async Task<TeacherResponseDto?> GetTeacherByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving teacher with ID {TeacherId}", id);
            var teacher = await _repository.GetByIdAsync(id);
            return teacher is null ? null : _mapper.Map<TeacherResponseDto>(teacher);
        }

        /// <inheritdoc/>
        public async Task<TeacherResponseDto> CreateTeacherAsync(TeacherRegisterDto dto)
        {
            _logger.LogInformation("Creating teacher with email {Email}", dto.Email);
            var teacher = _mapper.Map<Teacher>(dto);
            teacher.CreatedDate = DateTime.UtcNow;
            await _repository.AddAsync(teacher);
            await _repository.SaveChangesAsync();
            return _mapper.Map<TeacherResponseDto>(teacher);
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateTeacherAsync(int id, TeacherUpdateDto dto)
        {
            _logger.LogInformation("Updating teacher with ID {TeacherId}", id);
            var teacher = await _repository.GetByIdAsync(id);
            if (teacher is null) return false;

            _mapper.Map(dto, teacher);
            await _repository.UpdateAsync(teacher);
            await _repository.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteTeacherAsync(int id)
        {
            _logger.LogInformation("Deleting teacher with ID {TeacherId}", id);
            var teacher = await _repository.GetByIdAsync(id);
            if (teacher is null) return false;

            await _repository.DeleteAsync(id);
            await _repository.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<TeacherLoginResponseDto?> LoginAsync(TeacherLoginDto dto)
        {
            _logger.LogInformation("Login attempt for email {Email}", dto.Email);
            var teachers = await _repository.GetAllAsync();
            var teacher = teachers.FirstOrDefault(t =>
                t.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase) &&
                t.Password == dto.Password);

            if (teacher is null)
            {
                _logger.LogWarning("Failed login attempt for email {Email}", dto.Email);
                return null;
            }

            return new TeacherLoginResponseDto
            {
                Token = $"demo-token-{teacher.Id}-{Guid.NewGuid():N}",
                Teacher = _mapper.Map<TeacherResponseDto>(teacher)
            };
        }
    }
}
