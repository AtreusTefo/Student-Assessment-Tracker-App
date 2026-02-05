using AutoMapper;
using StudentAssessmentTracker.Models;

namespace StudentAssessmentTracker.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Map Student to Student (for basic operations)
        CreateMap<Student, Student>().ReverseMap();

        // Map Student to StudentListDto (filters to Id, FirstName, LastName for list views)
        CreateMap<Student, StudentListDto>()
            .ForMember(dest => dest.StudentId, opt => opt.MapFrom(src => src.StudentId))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));

        // Map Student to StudentDetailDto (includes assessments and calculations for detail views)
        CreateMap<Student, StudentDetailDto>()
            .ForMember(dest => dest.StudentId, opt => opt.MapFrom(src => src.StudentId))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.Grade, opt => opt.MapFrom(src => src.Grade))
            .ForMember(dest => dest.EnrollmentDate, opt => opt.MapFrom(src => src.EnrollmentDate))
            .ForMember(dest => dest.Assessment1, opt => opt.MapFrom(src => src.Assessment1))
            .ForMember(dest => dest.Assessment2, opt => opt.MapFrom(src => src.Assessment2))
            .ForMember(dest => dest.Assessment3, opt => opt.MapFrom(src => src.Assessment3))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total))
            .ForMember(dest => dest.Average, opt => opt.MapFrom(src => src.Average))
            .ForMember(dest => dest.Percentage, opt => opt.MapFrom(src => src.Percentage))
            .ForMember(dest => dest.PerformanceLevel, opt => opt.MapFrom(src => src.PerformanceLevel));
    }
}


