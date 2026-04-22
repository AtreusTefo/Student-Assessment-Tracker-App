# AutoMapper DTO Implementation Summary

**Version:** AutoMapper 12.0  
**Location:** `StudentAssessmentTrackerAPI/Application/`

## Overview

The project uses AutoMapper to map between Domain entities and Data Transfer Objects (DTOs). This separates what the domain model contains from what the API exposes to clients.

## DTO Files

All DTOs are located in `StudentAssessmentTrackerAPI/Application/DTOs/`:

| File | Purpose |
|------|---------|
| `StudentDto.cs` | Student response DTO (list and detail views) |
| `TeacherDto.cs` | Teacher response DTO with `IsActive` derived field |
| `AdminDto.cs` | Admin response DTO |
| `GradeDto.cs` | Grade (school year) response DTO |
| `SubjectDto.cs` | Subject response DTO |
| `ClassGroupDto.cs` | Class group response DTO |
| `StudentAssessmentDto.cs` | Named assessment with score and maxScore |
| `AssessmentSubmissionDto.cs` | Assessment file submission DTO |
| `AuditLogDto.cs` | Audit log entry DTO |

## Mapping Profile

All mappings are defined in a single profile:

```
StudentAssessmentTrackerAPI/Application/Mappings/MappingProfile.cs
```

Key mappings:

```csharp
// Student entity -> StudentDto
CreateMap<Student, StudentDto>();

// Teacher entity -> TeacherResponseDto
// IsActive is derived: true if Password is set
CreateMap<Teacher, TeacherResponseDto>()
    .ForMember(dest => dest.IsActive,
        opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Password)));

// StudentAssessment entity -> StudentAssessmentDto
CreateMap<StudentAssessment, StudentAssessmentDto>();

// AssessmentSubmission entity -> AssessmentSubmissionDto
CreateMap<AssessmentSubmission, AssessmentSubmissionDto>();
```

## Domain Entity Locations

All domain entities are in `StudentAssessmentTrackerAPI/Domain/Entities/`:

| Entity | File |
|--------|------|
| `Student` | `Domain/Entities/Student.cs` |
| `Teacher` | `Domain/Entities/Teacher.cs` |
| `Admin` | `Domain/Entities/Admin.cs` |
| `Grade` | `Domain/Entities/Grade.cs` |
| `Subject` | `Domain/Entities/Subject.cs` |
| `ClassGroup` | `Domain/Entities/ClassGroup.cs` |
| `StudentAssessment` | `Domain/Entities/StudentAssessment.cs` |
| `AssessmentSubmission` | `Domain/Entities/AssessmentSubmission.cs` |
| `AuditLog` | `Domain/Entities/AuditLog.cs` |

## Controller Locations

All controllers are in `StudentAssessmentTrackerAPI/Presentation/Controllers/`. They depend on service interfaces, not concrete implementations:

```csharp
// Example: StudentsController uses IStudentService interface
public StudentsController(IStudentService studentService) { ... }
```

## Why DTOs matter

- **Security:** Domain entities may contain password hashes or internal state. DTOs expose only what clients need.
- **Flexibility:** API shape can change without changing the domain model.
- **Special derivations:** `TeacherResponseDto.IsActive` is computed from whether a password is set — this logic lives in the mapping, not in the entity.
- **Validation:** `CreateStudentDto` and `UpdateStudentDto` go through FluentValidation before reaching the service layer.

## FluentValidation

Validators for DTOs/request models live in `StudentAssessmentTrackerAPI/Application/Validators/`:

- `CreateStudentValidator` — phone must be exactly 8 numeric digits; email required
- `UpdateStudentValidator` — same rules as create
- `TeacherRegisterValidator` — email and password validation
- `StudentAssessmentValidator` — score must be between 0 and maxScore
