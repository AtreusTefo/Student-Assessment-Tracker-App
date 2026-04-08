# Daily Report — April 8, 2026

**Developer**: Atreus Tefo Ramokate  
**Sprint**: Sprint 5 (Post-Sprint Hardening)  
**Project**: Student Assessment Tracker

---

## What I Did Today

Conducted two consecutive deep-analysis passes on the full-stack codebase (ASP.NET Core 8 / Angular 18) targeting **referential integrity, data consistency, and security** between the Teacher ↔ Student relationship. Each pass produced a prioritised issue list that was then fully implemented.

**Pass 1 — Schema & Architecture (10 issues)** covered the frontend Angular models, backend domain schema, repository patterns, service-layer security, and an EF Core migration.

**Pass 2 — Logic Bugs & Security (7 issues)** covered controller exception handling gaps, an N+1 query regression, AutoMapper data-source correctness, server-side input validation, API auth documentation, and a TOCTOU race condition.

A separate cosmetic fix was made to suppress third-party TypeScript deprecation warnings from `node_modules/rxjs` appearing in the VS Code Problems panel.

---

## What Was Completed

### Pass 1 — 10 Issues Fixed

---

#### Issue 1 + 2 — `SubjectId` denormalisation on `TeacherStudent` + teacher subject-change guard
**Files**: `Domain/Entities/TeacherStudent.cs`, `Infrastructure/Data/ApplicationDbContext.cs`, `Infrastructure/Repositories/StudentRepository.cs`

Added `SubjectId` (FK → `Subjects`) and a `Subject` navigation property to the `TeacherStudent` join entity so that every assignment row records the subject at the time of assignment. Configured a unique index `UX_TeacherStudents_StudentId_SubjectId` on `(StudentId, SubjectId)` — enforcing at the database level that a student can only have one teacher per subject. `AssignToTeacherAsync` now detects subject conflicts pre-insert and throws `InvalidOperationException` (409 at the controller).

`TeacherService.UpdateTeacherAsync` was extended with a guard that blocks a teacher's subject being changed while they still have students assigned, preventing orphaned subject references on existing join rows.

---

#### Issue 3 — Orphan-student guard moved to repository layer
**Files**: `Infrastructure/Repositories/StudentRepository.cs`, `Application/Services/StudentService.cs`

The minimum-assignment guard (a student must always have at least one teacher) was moved from `StudentService` into `StudentRepository.UnassignFromTeacherAsync`. This ensures the invariant is enforced at the data-access boundary regardless of which code path calls the repository, eliminating any future risk of a service bypass.

---

#### Issue 5 — Stale `teacherId` removed from frontend student models
**File**: `StudentApp/src/app/core/models/student.model.ts`

Removed the legacy `teacherId: number` single-FK field from the `Student`, `StudentDetailDto`, and `CreateStudentDto` TypeScript interfaces — a leftover from the one-teacher-per-student era that no longer maps to any backend field. Added the `TeacherSummaryDto` interface (`teacherId`, `fullName`, `subjectName`) and replaced `teacherId` with `teachers: TeacherSummaryDto[]` on `Student` and `StudentDetailDto`.

---

#### Issue 6 — `assignTeacher` and `unassignTeacher` missing from `StudentApiService`
**File**: `StudentApp/src/app/core/services/http/student-api.service.ts`

Added the two missing HTTP methods:
- `assignTeacher(studentId)` → `POST /api/students/{id}/teachers`
- `unassignTeacher(studentId)` → `DELETE /api/students/{id}/teachers`

---

#### Issue 7 — Legacy service files deleted
**Files**: `StudentApp/src/app/services/student.service.ts`, `StudentApp/src/app/services/teacher.service.ts`

Two unreferenced legacy service files from the pre-migration schema (defined `assessment1`, `assessment2`, `assessment3`, `grade: string`, `subject: string`) were confirmed to have zero imports across the codebase and deleted.

---

#### Issue 8 — Broken access control on `StudentsController`
**File**: `StudentAssessmentTrackerAPI/Presentation/Controllers/StudentsController.cs`

All seven teacher-facing actions (`GetAllStudents`, `GetStudent`, `CreateStudent`, `UpdateStudent`, `DeleteStudent`, `AssignTeacher`, `UnassignTeacher`) were changed from `[Authorize]` to `[Authorize(Roles = "Teacher")]`. A student JWT could previously call these endpoints; now only Teacher-role tokens are accepted. The public `[HttpPost("activate")]` and `[HttpPost("login")]` endpoints remain unauthenticated.

---

#### Issue 9 — Nested subscription anti-pattern in `updateStudent`
**File**: `StudentApp/src/app/features/students/services/student-business.service.ts`

Replaced the `tap(() => { this.loadStudents().subscribe(); })` nested subscription with a `switchMap(() => this.loadStudents())` chain. The nested subscribe created a fire-and-forget inner Observable with no error propagation and no cancellation. The `switchMap` approach propagates errors correctly and allows the outer subscriber to react to list-refresh completion.

---

#### Issue 10 — `TeacherRepository.GetAllAsync` not loading student assignments
**File**: `StudentAssessmentTrackerAPI/Infrastructure/Repositories/TeacherRepository.cs`

Added `.Include(t => t.StudentAssignments).ThenInclude(sa => sa.Student)` to `GetAllAsync` so that callers receive teachers with their full assignment data populated, consistent with what `GetByIdAsync` already loaded.

---

#### Migration — `AddSubjectIdToTeacherStudents`
**File**: `Infrastructure/Data/Migrations/20260408134157_AddSubjectIdToTeacherStudents.cs`

Created and applied an EF Core migration that:
1. Adds `SubjectId int NULL` to `TeacherStudents`
2. Backfills every existing row with the owning teacher's `SubjectId` via a raw SQL `UPDATE … JOIN`
3. Converts the column to `NOT NULL`
4. Creates `IX_TeacherStudents_SubjectId` (general index)
5. Creates unique index `UX_TeacherStudents_StudentId_SubjectId`
6. Adds `FK_TeacherStudents_Subjects_SubjectId` with `ON DELETE NO ACTION`

---

### Pass 2 — 7 Issues Fixed

---

#### Bug 1 — `UnassignTeacher` returned HTTP 500 instead of 409 for orphan violations
**File**: `Presentation/Controllers/StudentsController.cs`

`StudentRepository.UnassignFromTeacherAsync` throws `InvalidOperationException` when unassigning would leave a student with zero teachers, but the `UnassignTeacher` action only caught `KeyNotFoundException`. The uncaught exception fell to the generic 500 handler. Added `catch (InvalidOperationException ex) { return Conflict(...); }` so the client receives a 409 with the business-rule message.

---

#### Bug 2 — `TeachersController.Create` returned HTTP 500 for an invalid `SubjectId`
**File**: `Presentation/Controllers/TeachersController.cs`

`TeacherService.CreateTeacherAsync` throws `ArgumentException` when the supplied `SubjectId` does not exist in the `Subjects` table. The `Create` action caught only `InvalidOperationException`, so any bad `SubjectId` returned a silent 500. Added `catch (ArgumentException ex) { return BadRequest(...); }` — matching the pattern already present on the `Update` action.

---

#### Bug 4 — `TeacherRepository.GetAllAsync` over-fetched all student assignments (N+1 regression)
**File**: `Infrastructure/Repositories/TeacherRepository.cs`

The `GetAllAsync` override added in Pass 1 had inadvertently re-introduced the `StudentAssignments`/`Student` includes that had been identified as an N+1 problem. `TeacherResponseDto` has no `Students` field, so every student row for every teacher was loaded in a cartesian join and immediately discarded. Removed the two redundant `.Include`/`.ThenInclude` lines, restoring the correct single-include form.

---

#### Issue 5 — `TeacherSummaryDto.SubjectName` read from teacher's live subject instead of the join row
**Files**: `Application/Mappings/MappingProfile.cs`, `Infrastructure/Repositories/StudentRepository.cs`

The AutoMapper rule for `TeacherStudent → TeacherSummaryDto` was reading `src.Teacher.SubjectNavigation.Name` — the teacher's current subject — rather than `src.Subject.Name` from the join row's own denormalized `SubjectId`. The `Subject` navigation on `TeacherStudent` was never loaded because no query included `.ThenInclude(ta => ta.Subject)`. Fixed in two coordinated steps:
1. Added `.Include(s => s.TeacherAssignments).ThenInclude(ta => ta.Subject)` as a second include branch in all four student-loading queries in `StudentRepository`.
2. Changed the mapping to `opt => opt.MapFrom(src => src.Subject != null ? src.Subject.Name : string.Empty)`.

---

#### Issue 6 — No server-side `ConfirmPassword` check on student account activation
**Files**: `Application/DTOs/StudentDto.cs`, `Application/Validators/StudentValidator.cs`

Direct API calls could activate a student account to any password without a confirmation field — only the Angular frontend enforced that constraint. Added `ConfirmPassword` to `StudentActivateDto` and a `.Equal(x => x.Password).WithMessage("Passwords do not match.")` rule to `StudentActivateValidator`, enforcing the check at the API boundary for all clients.

---

#### Issue 7 — `GET /api/teachers` documented as Public but requires Teacher JWT
**File**: `README.md`

The API reference table in `README.md` listed `GET /api/teachers` and `GET /api/teachers/{id}` as `Public`. The controller has had `[Authorize(Roles = "Teacher")]` since Sprint 2. Updated both rows from `Public` to `Teacher JWT` so documentation matches the enforced behaviour.

---

#### Issue 8 — TOCTOU FK violation in `AssignStudentToTeacherAsync`
**File**: `Application/Services/StudentService.cs`

Between the existence check (`GetByIdAsync`) and the INSERT, a concurrent deletion of the teacher or student would cause the FK constraint to fire — surfacing as an unhandled `DbUpdateException` (HTTP 500). Added an `IsForeignKeyViolation` helper that detects SQL Server error 547 and wrapped `AssignToTeacherAsync` in a `catch (DbUpdateException) when (IsForeignKeyViolation(...))` block that re-throws as `KeyNotFoundException`, which the controller maps to a clean 404.

---

#### VS Code — Third-party TypeScript deprecation warnings suppressed
**File**: `.vscode/settings.json`

Added `"typescript.tsserver.experimental.enableProjectDiagnostics": false` and `"search.exclude": { "**/node_modules": true }` to prevent VS Code's TypeScript language service from scanning `node_modules` and reporting deprecation warnings from `rxjs/tsconfig.json`. These have no impact on the Angular build.

---

## Challenges Faced and How They Were Resolved

---

### Challenge 1 — EF Core migration failed: FK constraint violation on existing rows

**Problem**: Running `dotnet ef database update` for `AddSubjectIdToTeacherStudents` immediately failed with SQL Server error 547 (`ALTER TABLE … conflicted with FOREIGN KEY constraint`). The auto-generated migration added `SubjectId int NOT NULL DEFAULT 0` and then tried to attach a FK to `Subjects.Id` — but `0` is not a valid subject ID, so all existing `TeacherStudents` rows violated the constraint before the backfill could run.

**Resolution**: Edited the generated migration file to follow a three-step pattern:
1. Add the column as `int NULL` (no constraint yet)
2. Run a raw `UPDATE ts SET ts.SubjectId = t.SubjectId FROM TeacherStudents ts INNER JOIN Teachers t ON ts.TeacherId = t.Id` to backfill every existing row from its owning teacher's current subject
3. `ALTER COLUMN` to `NOT NULL`, then create the index and FK

This is the standard EF Core approach for adding a non-nullable FK column to a table that already contains data.

---

### Challenge 2 — Two-step `ThenInclude` needed for the same navigation collection

**Problem**: EF Core's fluent Include API does not allow chaining two different `ThenInclude` calls off the same `Include` path in a single expression chain. The query needed both `.ThenInclude(ta => ta.Teacher).ThenInclude(t => t.SubjectNavigation)` and `.ThenInclude(ta => ta.Subject)` off the same `s.TeacherAssignments` include.

**Resolution**: Repeated the `.Include(s => s.TeacherAssignments)` call as a second branch per the EF Core documented multi-level include pattern:
```csharp
.Include(s => s.TeacherAssignments)
    .ThenInclude(ta => ta.Teacher)
    .ThenInclude(t => t.SubjectNavigation)
.Include(s => s.TeacherAssignments)   // second branch for the join row's own Subject
    .ThenInclude(ta => ta.Subject)
```
EF Core deduplicates the join and produces a single efficient query.

---

### Challenge 3 — N+1 include regression introduced and caught in the same session

**Problem**: When fixing Issue 10 (load student assignments in `TeacherRepository.GetAllAsync`), the `StudentAssignments → Student` includes were added to `GetAllAsync`. In Pass 2 analysis this was identified as an over-fetch: `TeacherResponseDto` has no `Students` property, so every student row for every teacher was loaded in a cartesian join and immediately discarded by AutoMapper.

**Resolution**: Reverted the two extra `Include`/`ThenInclude` lines from `GetAllAsync`. The correct fix for Issue 10 was that the data was already available through the `GetByIdAsync` path; the list endpoint intentionally omits it for performance.

---

### Challenge 4 — Running API process locked the build output during migration

**Problem**: The first `dotnet ef migrations add` attempt failed because the API was already running (from a previous `dotnet run` session), locking `bin/Debug/net8.0/StudentAssessmentTracker.exe`. The `dotnet ef` tooling must build the project before scaffolding a migration, so the file lock caused an `MSB3027` build error.

**Resolution**: Stopped the running process (`Stop-Process -Id 20704 -Force`) to release the lock, then re-ran `dotnet ef migrations add` successfully.

---

### Challenge 5 — VS Code reporting third-party errors from `node_modules/rxjs`

**Problem**: Two TypeScript deprecation errors (`moduleResolution: node10` and `baseUrl` deprecated in TS 7.0) appeared in the Problems panel pointing at `node_modules/rxjs/tsconfig.json`. These were not build errors — they originated from VS Code's background TypeScript language service scanning all `tsconfig.json` files found in the workspace folder tree, including inside `node_modules`.

**Resolution**: Added `"typescript.tsserver.experimental.enableProjectDiagnostics": false` to `.vscode/settings.json`. This prevents the language service from running background project-wide diagnostics scans that would otherwise crawl into `node_modules`. The Angular build (`tsconfig.app.json`) was never affected.

---

## Build Status at End of Day

| Target | Result |
|--------|--------|
| `dotnet build` (ASP.NET Core 8) | ✅ 0 errors · 0 warnings |
| `dotnet ef database update` | ✅ Migration `AddSubjectIdToTeacherStudents` applied |
| Angular TypeScript (app code) | ✅ No errors in project source files |
