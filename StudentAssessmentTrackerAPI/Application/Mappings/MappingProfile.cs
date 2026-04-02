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
        /// <summary>Registers all entity-to-DTO and DTO-to-entity mappings.</summary>
        public MappingProfile()
        {
            // ── Grade Mappings ────────────────────────────────────────────────
            CreateMap<Grade, GradeDto>();

            // ── Subject Mappings ──────────────────────────────────────────────
            CreateMap<Subject, SubjectDto>();

            // ── StudentAssessment Mappings ────────────────────────────────────
            CreateMap<StudentAssessment, StudentAssessmentDto>()
                .ForMember(dest => dest.SubmissionCount,
                    opt => opt.MapFrom(src => src.Submissions.Count));
            CreateMap<CreateStudentAssessmentDto, StudentAssessment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.StudentId, opt => opt.Ignore())
                .ForMember(dest => dest.Student, opt => opt.Ignore())
                .ForMember(dest => dest.Submissions, opt => opt.Ignore())
                .ForMember(dest => dest.IsAssigned, opt => opt.MapFrom(src => src.IsAssigned ?? false));
            CreateMap<UpdateStudentAssessmentDto, StudentAssessment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.StudentId, opt => opt.Ignore())
                .ForMember(dest => dest.Student, opt => opt.Ignore())
                .ForMember(dest => dest.Submissions, opt => opt.Ignore())
                .ForMember(dest => dest.IsAssigned, opt => opt.MapFrom(src => src.IsAssigned ?? false));

            // ── AssessmentSubmission Mappings ────────────────────────────────
            CreateMap<AssessmentSubmission, AssessmentSubmissionDto>();

            // ── Student Mappings ──────────────────────────────────────────────

            // TeacherStudent → TeacherSummaryDto (used when embedding teacher list inside StudentDto)
            CreateMap<TeacherStudent, TeacherSummaryDto>()
                .ForMember(dest => dest.TeacherId,
                    opt => opt.MapFrom(src => src.TeacherId))
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src => src.Teacher != null ? src.Teacher.GetFullName() : string.Empty))
                .ForMember(dest => dest.SubjectName,
                    opt => opt.MapFrom(src => src.Teacher != null && src.Teacher.SubjectNavigation != null
                        ? src.Teacher.SubjectNavigation.Name
                        : string.Empty));

            // Student → StudentDto with calculated fields, resolved grade name, and teacher list
            CreateMap<Student, StudentDto>()
                .ForMember(dest => dest.GradeName,
                    opt => opt.MapFrom(src => src.GradeNavigation != null ? src.GradeNavigation.Name : string.Empty))
                .ForMember(dest => dest.Teachers,
                    opt => opt.MapFrom(src => src.TeacherAssignments))
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

            // CreateStudentDto → Student (StudentUniqueId generated in service; teacher assigned via join table)
            CreateMap<CreateStudentDto, Student>()
                .ForMember(dest => dest.StudentUniqueId, opt => opt.Ignore())
                .ForMember(dest => dest.GradeNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.TeacherAssignments, opt => opt.Ignore())
                .ForMember(dest => dest.Assessments, opt => opt.Ignore());

            // UpdateStudentDto → Student (StudentUniqueId, teacher assignments, Assessments must never be overwritten via update)
            CreateMap<UpdateStudentDto, Student>()
                .ForMember(dest => dest.StudentUniqueId, opt => opt.Ignore())
                .ForMember(dest => dest.GradeNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.TeacherAssignments, opt => opt.Ignore())
                .ForMember(dest => dest.Assessments, opt => opt.Ignore());

            // ── Teacher Mappings ──────────────────────────────────────────────

            CreateMap<Teacher, TeacherResponseDto>()
                .ForMember(dest => dest.TeacherId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.SubjectName,
                    opt => opt.MapFrom(src => src.SubjectNavigation != null ? src.SubjectNavigation.Name : string.Empty));

            CreateMap<TeacherRegisterDto, Teacher>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                // EnrollmentDate is server-controlled — always set to UtcNow on first registration.
                .ForMember(dest => dest.EnrollmentDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.SubjectNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.StudentAssignments, opt => opt.Ignore());

            CreateMap<TeacherUpdateDto, Teacher>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                // EnrollmentDate must never be changed after registration.
                .ForMember(dest => dest.EnrollmentDate, opt => opt.Ignore())
                .ForMember(dest => dest.SubjectNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.StudentAssignments, opt => opt.Ignore());

            // ── Student Auth Mappings ─────────────────────────────────────────

            // Student → StudentProfileDto (safe public view, no password)
            CreateMap<Student, StudentProfileDto>()
                .ForMember(dest => dest.GradeName,
                    opt => opt.MapFrom(src => src.GradeNavigation != null ? src.GradeNavigation.Name : string.Empty))
                .ForMember(dest => dest.Teachers,
                    opt => opt.MapFrom(src => src.TeacherAssignments))
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
