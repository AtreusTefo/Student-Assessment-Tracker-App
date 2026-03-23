using AutoMapper;
using StudentAssessmentTracker.Application.DTOs;
using StudentAssessmentTracker.Domain.Entities;

namespace StudentAssessmentTracker.Application.Mappings
{
    /// <summary>
    /// AutoMapper profile for mapping between domain entities and DTOs
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ── Grade Mappings ────────────────────────────────────────────────
            CreateMap<Grade, GradeDto>();

            // ── Subject Mappings ──────────────────────────────────────────────
            CreateMap<Subject, SubjectDto>();

            // ── StudentAssessment Mappings ────────────────────────────────────
            CreateMap<StudentAssessment, StudentAssessmentDto>();
            CreateMap<CreateStudentAssessmentDto, StudentAssessment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.StudentId, opt => opt.Ignore())
                .ForMember(dest => dest.Student, opt => opt.Ignore());
            CreateMap<UpdateStudentAssessmentDto, StudentAssessment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.StudentId, opt => opt.Ignore())
                .ForMember(dest => dest.Student, opt => opt.Ignore());

            // ── Student Mappings ──────────────────────────────────────────────

            // Student → StudentDto with calculated fields and resolved grade name
            CreateMap<Student, StudentDto>()
                .ForMember(dest => dest.GradeName,
                    opt => opt.MapFrom(src => src.GradeNavigation != null ? src.GradeNavigation.Name : string.Empty))
                .ForMember(dest => dest.TotalScore,
                    opt => opt.MapFrom(src => src.GetTotalScore()))
                .ForMember(dest => dest.MaxPossible,
                    opt => opt.MapFrom(src => src.GetMaxPossible()))
                .ForMember(dest => dest.AverageScore,
                    opt => opt.MapFrom(src => src.GetAverageScore()))
                .ForMember(dest => dest.Percentage,
                    opt => opt.MapFrom(src => src.GetPercentage()))
                .ForMember(dest => dest.PerformanceLevel,
                    opt => opt.MapFrom(src => src.GetPerformanceLevel()));

            // CreateStudentDto → Student (StudentUniqueId generated in service)
            CreateMap<CreateStudentDto, Student>()
                .ForMember(dest => dest.StudentUniqueId, opt => opt.Ignore())
                .ForMember(dest => dest.GradeNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.Teacher, opt => opt.Ignore())
                .ForMember(dest => dest.Assessments, opt => opt.Ignore());

            // UpdateStudentDto → Student (StudentUniqueId, TeacherId, Assessments must never be overwritten via update)
            CreateMap<UpdateStudentDto, Student>()
                .ForMember(dest => dest.StudentUniqueId, opt => opt.Ignore())
                .ForMember(dest => dest.TeacherId, opt => opt.Ignore())
                .ForMember(dest => dest.GradeNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.Teacher, opt => opt.Ignore())
                .ForMember(dest => dest.Assessments, opt => opt.Ignore());

            // ── Teacher Mappings ──────────────────────────────────────────────

            CreateMap<Teacher, TeacherResponseDto>()
                .ForMember(dest => dest.TeacherId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.SubjectName,
                    opt => opt.MapFrom(src => src.SubjectNavigation != null ? src.SubjectNavigation.Name : string.Empty));

            CreateMap<TeacherRegisterDto, Teacher>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.SubjectNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.Students, opt => opt.Ignore());

            CreateMap<TeacherUpdateDto, Teacher>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.SubjectNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.Students, opt => opt.Ignore());

            // ── Student Auth Mappings ─────────────────────────────────────────

            // Student → StudentProfileDto (safe public view, no password)
            CreateMap<Student, StudentProfileDto>()
                .ForMember(dest => dest.GradeName,
                    opt => opt.MapFrom(src => src.GradeNavigation != null ? src.GradeNavigation.Name : string.Empty))
                .ForMember(dest => dest.TotalScore,
                    opt => opt.MapFrom(src => src.GetTotalScore()))
                .ForMember(dest => dest.MaxPossible,
                    opt => opt.MapFrom(src => src.GetMaxPossible()))
                .ForMember(dest => dest.AverageScore,
                    opt => opt.MapFrom(src => src.GetAverageScore()))
                .ForMember(dest => dest.Percentage,
                    opt => opt.MapFrom(src => src.GetPercentage()))
                .ForMember(dest => dest.PerformanceLevel,
                    opt => opt.MapFrom(src => src.GetPerformanceLevel()));
        }
    }
}
