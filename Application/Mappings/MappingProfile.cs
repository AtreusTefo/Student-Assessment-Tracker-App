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
        public MappingProfile()
        {
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
        }
    }
}
