# Multi-Layered Architecture Implementation Summary

**Last Updated:** April 22, 2026

## Overview

The Student Assessment Tracker implements Clean Architecture with a decoupled multi-layered design across a .NET 8 REST API backend and an Angular 21 standalone frontend.

---

## Backend Architecture (4 Layers)

### 1. Domain Layer

**Location:** `StudentAssessmentTrackerAPI/Domain/`

Contains pure business logic with no framework dependencies.

**Entities:** Student, Teacher, Admin, Grade, Subject, ClassGroup, StudentAssessment, AssessmentSubmission, AuditLog, TeacherStudent, ClassGroupStudent, ClassGroupSubject

**Business methods on Student:**
- `GetTotalScore()` — sums all assessment scores
- `GetMaxPossible()` — sums all assessment max scores
- `GetAverageScore()` — average score-as-percentage across assessments
- `GetPercentage()` — percentage based on actual max possible
- `GetPerformanceLevel()` — Needs Support / Satisfactory / Good / Excellent

### 2. Infrastructure Layer

**Location:** `StudentAssessmentTrackerAPI/Infrastructure/`

Handles all data access via Entity Framework Core 8.

**Database:** SQL Server LocalDB (`StudentAssessmentTrackerDev`)  
**Migrations:** 19 EF Core migrations — applied automatically on startup  
**Generic base:** `Repository<T>` — CRUD operations for any entity  
**Specialized repos:** `StudentRepository`, `TeacherRepository`, and others per entity

### 3. Application Layer

**Location:** `StudentAssessmentTrackerAPI/Application/`

Orchestrates domain logic; defines DTOs, validators, and AutoMapper mappings.

| Sublayer | Contents |
|---|---|
| `DTOs/` | StudentDto, TeacherDto, AdminDto, GradeDto, SubjectDto, ClassGroupDto, StudentAssessmentDto, AssessmentSubmissionDto, AuditLogDto |
| `Services/` | StudentService, TeacherService, AdminService, StudentAssessmentService, AssessmentSubmissionService, ClassGroupService, ReportService, and more |
| `Validators/` | CreateStudentValidator, UpdateStudentValidator, TeacherRegisterValidator, StudentAssessmentValidator |
| `Mappings/` | MappingProfile.cs — AutoMapper 12.0 config for all entity-to-DTO mappings |

### 4. Presentation Layer

**Location:** `StudentAssessmentTrackerAPI/Presentation/Controllers/`

9 REST API controllers secured by role-based JWT auth (Admin / Teacher / Student).

| Controller | Route | Roles |
|---|---|---|
| AdminsController | `/api/admins` | Admin |
| TeachersController | `/api/teachers` | Teacher / Public |
| StudentsController | `/api/students` | Admin |
| StudentAssessmentsController | `/api/studentassessments` | Admin, Teacher |
| AssessmentSubmissionsController | `/api/assessmentsubmissions` | Admin, Teacher, Student |
| ReportsController | `/api/reports` | Admin, Teacher |
| GradesController | `/api/grades` | Admin |
| SubjectsController | `/api/subjects` | Admin |
| ClassGroupsController | `/api/classgroups` | Admin |

---

## Frontend Architecture (Angular 21)

**Location:** `StudentApp/src/`

- Standalone, zoneless Angular 21 application
- Template-driven forms (NgForm)
- 11 standalone components: login-form, signup-form, student-list, student-detail, student-form, student-login, student-activate, student-dashboard, teacher-dashboard, admin-login, admin-dashboard
- 10 HTTP API service files in `core/services/http/`
- Proxy: `/api` requests forwarded to `http://localhost:5000`

---

## Data Flow

```
Angular Component
    |
    v
HTTP API Service (core/services/http/)
    |  HTTP JSON
    v
Presentation Layer (REST Controller)
    |  IService interface
    v
Application Layer (Service, Validators, DTOs, AutoMapper)
    |  IRepository interface
    v
Infrastructure Layer (Repository, ApplicationDbContext)
    |  EF Core
    v
SQL Server LocalDB
```

---

## Dependency Injection (Program.cs)

```csharp
// Infrastructure
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlServer(connectionString));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Application
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateStudentValidator>();
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Presentation
builder.Services.AddControllers();
```

---

## Key Principle

Each layer only depends on layers below it — never upward. Controllers know nothing about repositories; repositories know nothing about controllers.
