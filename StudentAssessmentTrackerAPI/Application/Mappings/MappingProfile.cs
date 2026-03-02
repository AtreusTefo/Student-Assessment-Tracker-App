using AutoMapper;
using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Domain.Entities;

namespace StudentAssessmentTracker.Application.Mappings
{
    /// <summary>
    /// AutoMapper profile for mapping between domain entities and DTOs
    /// Centralizes all mapping configurations
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// Initializes AutoMapper configurations for student and teacher mappings
        /// </summary>
        public MappingProfile()
        {
            // ── Student Mappings ──────────────────────────────────────────────

            // Student to StudentDto mapping with calculations
            CreateMap<Student, StudentDto>()
                .ForMember(dest => dest.TotalScore, opt => opt.MapFrom(src => src.GetTotalScore()))
                .ForMember(dest => dest.AverageScore, opt => opt.MapFrom(src => src.GetAverageScore()))
                .ForMember(dest => dest.Percentage, opt => opt.MapFrom(src => src.GetPercentage()))
                .ForMember(dest => dest.PerformanceLevel, opt => opt.MapFrom(src => src.GetPerformanceLevel()));

            // CreateStudentDto to Student mapping
            CreateMap<CreateStudentDto, Student>();

            // UpdateStudentDto to Student mapping
            CreateMap<UpdateStudentDto, Student>();

            // ── Teacher Mappings ──────────────────────────────────────────────

            // Teacher to TeacherResponseDto — map Id → TeacherId
            CreateMap<Teacher, TeacherResponseDto>()
                .ForMember(dest => dest.TeacherId, opt => opt.MapFrom(src => src.Id));

            // TeacherRegisterDto to Teacher entity
            CreateMap<TeacherRegisterDto, Teacher>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow));

            // TeacherUpdateDto to existing Teacher entity (in-place)
            CreateMap<TeacherUpdateDto, Teacher>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
