# Daily Report — April 1, 2026

**Developer**: Developer.03  
**Sprint**: Sprint 5  
**Project**: Student Assessment Tracker

---

## What I Did Today

Two major work streams were completed:

1. **Many-to-Many Teacher↔Student Architecture** — converted the one-to-many relationship (single `TeacherId` FK on `Students`) into a proper join table (`TeacherStudents`) so a student can be taught by multiple teachers who teach different subjects.
2. **Security, Data Consistency & Architecture Hardening (8 issues)** — resolved every outstanding referential integrity, security, and clean-architecture violation identified in a formal code review.

---

## What Was Completed

### Stream 1 — Many-to-Many Teacher↔Student Relationship

| Layer | Change |
|-------|--------|
| **Domain** | Created `TeacherStudent.cs` join entity with `TeacherId`, `StudentId`, `AssignedAt`. Removed `TeacherId` FK + `Teacher` nav from `Student.cs`. Replaced `ICollection<Student>` with `ICollection<TeacherStudent>` on `Teacher.cs`. |
| **Infrastructure** | Added `DbSet<TeacherStudent> TeacherStudents` to `ApplicationDbContext`. Configured composite PK `(TeacherId, StudentId)`, SQL default for `AssignedAt`, CASCADE on both FKs. Added `AssignToTeacherAsync`, `UnassignFromTeacherAsync`, `IsAssignedToTeacherAsync` to `StudentRepository`. Rewrote all teacher-scoped queries from `s.TeacherId == id` to `s.TeacherAssignments.Any(ta => ta.TeacherId == id)`. |
| **Application** | Updated `StudentDto` and `StudentProfileDto` — replaced `int TeacherId` with `IEnumerable<TeacherSummaryDto> Teachers`. Added `TeacherSummaryDto` to `TeacherDto.cs`. Updated `MappingProfile.cs` with `TeacherStudent → TeacherSummaryDto` mapping and Teachers member on student mappings. Added `AssignStudentToTeacherAsync` / `UnassignStudentFromTeacherAsync` to `IStudentService` and `StudentService`. |
| **Presentation** | Added `POST /api/students/{studentId}/teachers` and `DELETE /api/students/{studentId}/teachers` endpoints to `StudentsController`. |
| **Database** | Migration `20260401132007_ManyToManyTeacherStudent` — drops `Students.TeacherId` column, creates `TeacherStudents` table. |

---

### Stream 2 — Security, Integrity & Architecture Hardening (8 Issues)

#### Issue 34 — CRITICAL SECURITY: TeachersController PUT & DELETE Were Unauthenticated
- Added `[Authorize]` to `Update` and `Delete` actions.
- Added self-scope check: authenticated teacher may only modify/delete their own record; any other ID returns `403 Forbidden`.
- Added `TryGetTeacherId()` helper and `using System.Security.Claims` import.
- **Files**: `Presentation/Controllers/TeachersController.cs`

#### Issue 35 — DATA CONSISTENCY: UpdateTeacherAsync Had No Duplicate Email / ID-Passport Check
- Added `ExistsByEmailAsync(email, excludeTeacherId)` and `ExistsByIdPassportNoAsync(idPassportNo, excludeTeacherId)` overloads to `ITeacherRepository` and `TeacherRepository`.
- `UpdateTeacherAsync` now pre-checks both fields (excluding the record being updated) and throws `InvalidOperationException` → `409 Conflict` instead of an opaque EF `DbUpdateException` → `500`.
- **Files**: `Domain/Interfaces/ITeacherRepository.cs`, `Infrastructure/Repositories/TeacherRepository.cs`, `Application/Services/TeacherService.cs`

#### Issue 36 — DATA CONSISTENCY: SubjectId Not Validated Against the Subjects Table
- Both `CreateTeacherAsync` and `UpdateTeacherAsync` now call `_subjectRepository.GetByIdAsync(dto.SubjectId)` and throw `ArgumentException` → `400` when the ID does not exist in the `Subjects` table.
- `IRepository<Subject>` injected into `TeacherService` via constructor.
- **Files**: `Application/Services/TeacherService.cs`, `Program.cs`

#### Issue 37 — REFERENTIAL INTEGRITY: Last Teacher Unassign Leaves Orphaned Student
- Added `CountTeacherAssignmentsAsync(studentId)` to `IStudentRepository` and `StudentRepository`.
- `UnassignStudentFromTeacherAsync` now guards: if the student has only one teacher, the operation throws `InvalidOperationException` → `400` with an explanatory message.
- **Files**: `Domain/Interfaces/IStudentRepository.cs`, `Infrastructure/Repositories/StudentRepository.cs`, `Application/Services/StudentService.cs`

#### Issue 38 — DATA CONSISTENCY: StudentUniqueId Collision Not Handled
- Replaced `new Random()` (not thread-safe) with `Random.Shared`.
- Added a `do { generate } while (DB already has this ID)` retry loop so a collision retries silently rather than letting EF throw a `DbUpdateException`.
- **File**: `Application/Services/StudentService.cs`

#### Issue 39 — INFRASTRUCTURE: DeleteTeacherAsync Called SaveChangesAsync Twice
- `Repository<T>.DeleteAsync(id)` already calls `SaveChangesAsync()` internally. The service was calling it a second time, resulting in a redundant, empty roundtrip to the database.
- Removed the second `await _repository.SaveChangesAsync()` call.
- **File**: `Application/Services/TeacherService.cs`

#### Issue 40 — ARCHITECTURE: StudentAssessmentService Directly Injected ApplicationDbContext
- Created new `IStudentAssessmentRepository` interface (`Domain/Interfaces/`) with `GetByStudentIdAsync` and `GetByIdForStudentAsync`.
- Created `StudentAssessmentRepository` implementation (`Infrastructure/Repositories/`) backed by the same EF Core DbContext.
- `StudentAssessmentService` now depends on `IStudentAssessmentRepository` + `IStudentRepository` only — `ApplicationDbContext` removed entirely from the Application layer.
- Registered `IStudentAssessmentRepository → StudentAssessmentRepository` in `Program.cs`.
- **Files**: `Domain/Interfaces/IStudentAssessmentRepository.cs` *(new)*, `Infrastructure/Repositories/StudentAssessmentRepository.cs` *(new)*, `Application/Services/StudentAssessmentService.cs`, `Program.cs`

#### Issue 41 — DATA CONSISTENCY: TeacherUpdateDto Allowed Blind Password Overwrite
- Removed `public string Password { get; set; }` from `TeacherUpdateDto` — the field no longer exists on the DTO, so callers cannot supply a value at all.
- `MappingProfile` retains `.ForMember(dest => dest.Password, opt => opt.Ignore())` as a defence-in-depth guard.
- **File**: `Application/DTOs/TeacherDto.cs`

---

## Challenges Faced and How They Were Resolved

### Challenge 1: Many-to-Many MappingProfile Conflict
**Problem**: A single `multi_replace_string_in_file` call with 4 replacements failed with "Edit at index 1 conflicts with another replacement" because two of the replacement targets were adjacent lines in the same text block.  
**Resolution**: Split into separate sequential replacement calls, verifying the file state between each one.

### Challenge 2: IStudentService Interface Missing New Methods
**Problem**: After adding `AssignTeacher`/`UnassignTeacher` controller endpoints, the build failed with `CS1061: 'IStudentService' does not contain a definition for 'AssignStudentToTeacherAsync'` — the concrete methods had been added to `StudentService` but the interface declaration was missed.  
**Resolution**: Added the two method signatures (with XML docs) to the `IStudentService` interface block in `StudentService.cs`. Build succeeded immediately.

### Challenge 3: TeacherService Required a New Dependency
**Problem**: Adding SubjectId validation to `TeacherService` required `IRepository<Subject>`, which was not previously injected.  
**Resolution**: Added `IRepository<Subject> _subjectRepository` to the constructor. The existing `Program.cs` registration `builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>))` already covered the generic registration, so no additional DI wiring was needed.

### Challenge 4: Eliminating DbContext from the Application Layer
**Problem**: `StudentAssessmentService` directly referenced `ApplicationDbContext`, violating Clean Architecture's rule that the Application layer must not depend on Infrastructure concerns.  
**Resolution**: Created `IStudentAssessmentRepository` in the Domain layer and `StudentAssessmentRepository` in Infrastructure. Rewired the service to use the new interface and `IStudentRepository.IsAssignedToTeacherAsync` for the ownership guard — removing the last direct DbContext dependency from the Application layer.

---

## Files Changed

### New Files
| File | Purpose |
|------|---------|
| `Domain/Entities/TeacherStudent.cs` | Join entity for many-to-many |
| `Domain/Interfaces/IStudentAssessmentRepository.cs` | Repository contract for assessments |
| `Infrastructure/Repositories/StudentAssessmentRepository.cs` | Concrete assessment repo |
| `Infrastructure/Data/Migrations/20260401132007_ManyToManyTeacherStudent.cs` | EF migration |

### Modified Files
| File | What Changed |
|------|-------------|
| `Domain/Entities/Student.cs` | Removed `TeacherId` FK; added `TeacherAssignments` |
| `Domain/Entities/Teacher.cs` | Replaced `Students` collection with `StudentAssignments` |
| `Domain/Interfaces/IStudentRepository.cs` | Added 3 assignment methods + `CountTeacherAssignmentsAsync` |
| `Domain/Interfaces/ITeacherRepository.cs` | Added `ExistsByEmailAsync` + `excludeTeacherId` overload |
| `Infrastructure/Data/ApplicationDbContext.cs` | Added `TeacherStudents` DbSet + configuration |
| `Infrastructure/Repositories/StudentRepository.cs` | All teacher-scoped queries rewired; 4 new methods |
| `Infrastructure/Repositories/TeacherRepository.cs` | `HasStudentsAsync` + 2 new duplicate-check methods |
| `Application/DTOs/StudentDto.cs` | `TeacherId` → `IEnumerable<TeacherSummaryDto> Teachers` |
| `Application/DTOs/TeacherDto.cs` | Added `TeacherSummaryDto`; removed `Password` from `TeacherUpdateDto` |
| `Application/Mappings/MappingProfile.cs` | New `TeacherStudent → TeacherSummaryDto` mapping; updated ignores |
| `Application/Services/StudentAssessmentService.cs` | DbContext removed; IStudentAssessmentRepository + IStudentRepository injected |
| `Application/Services/StudentService.cs` | `CreateStudentAsync` uses join table; 2 new assign/unassign methods; `Random.Shared` + retry loop |
| `Application/Services/TeacherService.cs` | SubjectId validation; duplicate checks in Update; removed double SaveChanges; `IRepository<Subject>` injected |
| `Presentation/Controllers/StudentsController.cs` | Assign/Unassign teacher endpoints |
| `Presentation/Controllers/TeachersController.cs` | `[Authorize]` + self-scope on PUT/DELETE; `TryGetTeacherId` helper |
| `Program.cs` | Registered `IStudentAssessmentRepository` |

---

## Key Technical Decisions

- **Cascade on both TeacherStudent FKs**: deleting a teacher removes their assignments; deleting a student removes their assignments. Prevents orphaned join rows without requiring application-layer cleanup.
- **Orphan guard on unassign**: A student with zero teachers becomes completely invisible (no teacher can query or delete them). The service now enforces a minimum of one teacher before permitting an unassign.
- **`Random.Shared` instead of `new Random()`**: `Random.Shared` is a thread-safe static instance available since .NET 6; `new Random()` per call is not concurrency-safe and can produce duplicate sequences when called in quick succession.
- **Repository abstraction over direct DbContext**: Services in the Application layer must not depend on EF Core or Infrastructure types. The new `IStudentAssessmentRepository` restores Clean Architecture compliance.

---

**Build status**: ✅ Succeeded — zero errors  
**Migration applied**: ✅ `20260401132007_ManyToManyTeacherStudent`  
**Total issues resolved this session**: 10 (2 architecture + 3 security + 5 data-integrity)
