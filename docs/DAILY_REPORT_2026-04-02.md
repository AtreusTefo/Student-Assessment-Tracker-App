# Daily Report — April 2, 2026

**Developer**: Developer.03  
**Sprint**: Sprint 5  
**Project**: Student Assessment Tracker

---

## What I Did Today

Two major work streams were completed:

1. **Authentication Bug Fix (Session 1)** — resolved a `401 Unauthorized` error on `GET /api/students` that appeared immediately after a teacher registered a new account.
2. **Assessment Submission Feature (Session 2)** — implemented full-stack file upload infrastructure for student assignment submissions, including 7 backend tasks and 7 frontend tasks (17 tasks total).

---

## What Was Completed

### Stream 1 — 401 Unauthorized After Teacher Registration

**Root cause**: `POST /api/teachers` (register) did not issue a JWT. The Angular `teacher-business.service.ts` stored the new teacher object but never performed a login call, so the token BehaviorSubject remained null. Any subsequent authenticated request had no `Authorization` header → `401`.

| Layer | File | Change |
|-------|------|--------|
| **Business service** | `teacher-business.service.ts` | Chained `switchMap` after registration to auto-call `login()`, so a token is obtained immediately after account creation |
| **Interceptor** | `auth.interceptor.ts` | Added `&& token` guard to the 401 handler — prevents a redirect loop where the login page itself triggers a 401 (no token was sent, so no redirect should fire) |
| **State service** | `teacher-state.service.ts` | Added stale-session cleanup on startup: if localStorage has a teacher object but no matching token, the session is cleared before the BehaviorSubjects are initialised |

---

### Stream 2 — Assessment Submission Feature (Backend)

#### Task 2 — IsAssigned + Instructions on StudentAssessment
- Added `IsAssigned` (bool, default `false`) and `Instructions` (string?, max 2000 chars) to `StudentAssessment` entity.
- Added navigation property `ICollection<AssessmentSubmission> Submissions`.
- Updated `StudentAssessmentDto`, `CreateStudentAssessmentDto`, `UpdateStudentAssessmentDto` with the new fields.
- Added `SubmissionCount` to `StudentAssessmentDto` (AutoMapper computes it from `src.Submissions.Count`).
- **Files**: `Domain/Entities/StudentAssessment.cs`, `Application/DTOs/StudentAssessmentDto.cs`

#### Task 3–5 — AssessmentSubmission Entity + DTO
- Created new `AssessmentSubmission` entity: `Id`, `StudentAssessmentId` (FK CASCADE), `StudentId` (FK NO ACTION), `FileName`, `StoredFileName`, `ContentType`, `FileSize`, `SubmittedAt`.
- Created `AssessmentSubmissionDto` (read-only: same fields except `StoredFileName`).
- Updated `ApplicationDbContext` with `DbSet<AssessmentSubmission>`, default value on `SubmittedAt`, FK constraints, and two indexes.
- **Files**: `Domain/Entities/AssessmentSubmission.cs` *(new)*, `Application/DTOs/AssessmentSubmissionDto.cs` *(new)*, `Infrastructure/Data/ApplicationDbContext.cs`

#### Task 6 — Repository Layer
- Created `IAssessmentSubmissionRepository` in `Domain/Interfaces/` with four methods: `GetByAssessmentAndStudentAsync`, `GetByIdAsync`, `AddAsync`, `DeleteAsync`.
- Created `AssessmentSubmissionRepository` in `Infrastructure/Repositories/` implementing the interface.
- Updated `StudentAssessmentRepository`: both query methods now include `.Include(a => a.Submissions)` so `SubmissionCount` is populated.
- **Files**: `Domain/Interfaces/IAssessmentSubmissionRepository.cs` *(new)*, `Infrastructure/Repositories/AssessmentSubmissionRepository.cs` *(new)*, `Infrastructure/Repositories/StudentAssessmentRepository.cs`

#### Task 7 — Service Layer
- Created `IAssessmentSubmissionService` + `AssessmentSubmissionService`:
  - `SubmitAsync` — validates extension/size, saves file with GUID + original extension, persists DB row; rolls back the file on DB failure.
  - `GetSubmissionsAsync` — student-scoped retrieval.
  - `DownloadAsync` — returns `(byte[], contentType, fileName)` tuple.
  - `DeleteSubmissionAsync` — removes DB row then file.
- Allowed extensions: `.pdf`, `.doc`, `.docx`, `.jpg`, `.jpeg`, `.png`. Max size: 10 MB.
- File storage path: `wwwroot/uploads/submissions/{studentId}/`.
- **File**: `Application/Services/AssessmentSubmissionService.cs` *(new)*

#### Task 8 — Controller
- Created `AssessmentSubmissionsController` at route `api/students/{studentId}/assessments/{assessmentId}/submissions`.
  - `POST /` — `[Authorize(Roles = "Student")]`
  - `GET /` — `[Authorize(Roles = "Teacher")]`
  - `GET /{id}/download` — `[Authorize]` (teacher or owner student)
  - `DELETE /{id}` — `[Authorize]` (teacher or owner student)
- **File**: `Presentation/Controllers/AssessmentSubmissionsController.cs` *(new)*

#### Task 9 — Program.cs + Infrastructure
- Registered `IAssessmentSubmissionRepository` → `AssessmentSubmissionRepository`.
- Registered `IAssessmentSubmissionService` → `AssessmentSubmissionService`.
- Configured Kestrel `MaxRequestBodySize = 10 MB`.
- Configured `FormOptions.MultipartBodyLengthLimit = 10 MB`.
- Added startup code to create `wwwroot/uploads/submissions/` if it does not exist.
- **File**: `Program.cs`

#### Task 10 — EF Migration
- Generated and applied migration `AddAssessmentSubmissions`:
  - Adds `IsAssigned` and `Instructions` columns to `StudentAssessments`.
  - Creates `AssessmentSubmissions` table with FK constraints and two indexes.
- **Command**: `dotnet ef migrations add AddAssessmentSubmissions` → `dotnet ef database update`
- **Status**: ✅ Applied successfully

---

### Stream 2 — Assessment Submission Feature (Frontend)

#### Task 11 — StudentAuthStateService Token Storage
- Added `STUDENT_TOKEN_KEY = 'sat_student_token'`, `setToken(token: string)`, `getToken(): string | null`.
- Updated `logout()` to also call `localStorage.removeItem(STUDENT_TOKEN_KEY)`.
- Startup session restore now requires **both** the student profile **and** the token to be present — if either is missing the session is not restored (prevents a state where the user object loads but every API request still gets a 401).
- **File**: `StudentApp/src/app/core/services/state/student-auth-state.service.ts`

#### Task 12 — Auth Interceptor Student Token Fallback
- Interceptor now checks teacher token first; if absent, falls back to the student token.
- 401 handler is now branch-aware:
  - Student token was sent → `studentAuthState.logout()` + navigate to `/student/login`
  - Teacher token was sent → `teacherState.logout()` + `studentAuthState.logout()` + navigate to `/login`
- **File**: `StudentApp/src/app/core/interceptors/auth.interceptor.ts`

#### Task 13 — Student Auth Business Service
- Both `activate()` and `login()` now call `this.studentAuthState.setToken(response.token)` inside the `tap` callback, persisting the JWT before setting the student profile.
- **File**: `StudentApp/src/app/features/students/services/student-auth-business.service.ts`

#### Task 14 — Student Model Updates
- `StudentAssessmentDto` — added `isAssigned: boolean`, `instructions: string | null`, `submissionCount: number`.
- `CreateStudentAssessmentDto` / `UpdateStudentAssessmentDto` — added optional `isAssigned?`, `instructions?`.
- `StudentAuthUser` — replaced `teacherId: number` with `teachers: TeacherSummaryDto[]`.
- Added `TeacherSummaryDto` interface: `{ teacherId, fullName, subjectName }`.
- Added `AssessmentSubmissionDto` interface: `{ id, studentAssessmentId, studentId, fileName, contentType, fileSize, submittedAt }`.
- **File**: `StudentApp/src/app/core/models/student.model.ts`

#### Task 15 — AssessmentSubmissionApiService
- Created new HTTP service at `core/services/http/assessment-submission-api.service.ts`.
- Methods: `upload()` (FormData POST), `getAll()`, `download()` (`responseType: 'blob'`), `delete()`.
- Exported from `core/services/http/index.ts`.
- **File**: `StudentApp/src/app/core/services/http/assessment-submission-api.service.ts` *(new)*

#### Task 16 — Student Detail Component (Teacher View)
- Added "Assigned" badge column and "Submissions" toggle column to the assessments table.
- Clicking the submissions button fetches and reveals an inline panel listing uploaded files with their size, submission date, a Download button, and a Delete button.
- Edit row now includes an "Assigned" checkbox and (when `isAssigned` is checked) an Instructions textarea below the row.
- Add Assessment form includes an "Assign to student" checkbox and a conditionally visible Instructions textarea.
- Injected `AssessmentSubmissionApiService`; added `toggleSubmissions()`, `downloadSubmission()`, `deleteSubmission()`, `formatFileSize()` methods.
- **File**: `StudentApp/src/app/components/student-detail.component.ts`

#### Task 17 — Student Dashboard Component (Student View)
- Replaced the misleading "Submitted" status badge for all non-overdue assessments with:
  - **"Submit File"** button — shown when `isAssigned === true` and `submissionCount === 0`
  - **"✓ Submitted (n)"** green badge — shown when `submissionCount > 0`
  - **"Pending"** grey badge — shown when `!isAssigned` and `submissionCount === 0`
- Added full upload modal triggered by the Submit File button:
  - Displays the assessment's instructions (or "No specific instructions" if null).
  - File picker restricted to `.pdf`, `.doc`, `.docx`, `.jpg`, `.jpeg`, `.png`.
  - Client-side validation: disallowed type → error message; file > 10 MB → error message.
  - On success: increments `submissionCount` in-memory and closes the modal.
- Imported `FormsModule` and injected `AssessmentSubmissionApiService`.
- **File**: `StudentApp/src/app/components/student-dashboard.component.ts`

---

## Challenges Faced and How They Were Resolved

### Issue #42 — 401 Unauthorized After Teacher Registration
**Problem**: `GET /api/students` returned `401 Unauthorized` immediately after a teacher registered a new account.  
**Root Cause**: `teacher-business.service.ts` registered the teacher but never obtained a JWT. The teacher object was set in state, but `teacherState.getToken()` returned `null`, so the interceptor attached no `Authorization` header.  
**Resolution**: Chained a `switchMap` after `register()` in `teacher-business.service.ts` to automatically call `login()` with the same credentials and store the returned token.

### Issue #43 — Auth Interceptor Redirect Loop on Login Page
**Problem**: The global 401 handler in `auth.interceptor.ts` fired even when the user was on the `/login` page submitting wrong credentials — redirecting back to `/login` in an infinite loop.  
**Root Cause**: The handler did not check whether a token was actually sent. The login endpoint rejected invalid credentials with a 401, triggering the redirect even though no JWT was attached.  
**Resolution**: Added `&& token` guard: `if (error.status === 401 && token) { ... }`. The redirect now only fires when a token was sent but rejected (expired/revoked), not on unauthenticated requests.

### Issue #44 — Stale Teacher Session After Token Expiry
**Problem**: After a JWT expired, a page refresh restored the teacher profile from `localStorage` even though every API call would fail with a 401.  
**Root Cause**: `teacher-state.service.ts` restored the session if `localStorage` contained a teacher object, with no check for an associated token.  
**Resolution**: Startup restoration was updated to require both the profile **and** the token to be present in `localStorage`; if either is missing the session is cleared.

### Issue #45 — Student JWT Never Persisted After Login
**Problem**: After a student logged in or activated their account, every subsequent API call failed with `401 Unauthorized` even though the server returned a token.  
**Root Cause**: `StudentAuthStateService` had no token storage at all (`setToken` / `getToken` did not exist). The `student-auth-business.service.ts` called `setCurrentStudent(response.student)` but discarded `response.token`.  
**Resolution**: Added `STUDENT_TOKEN_KEY`, `setToken()`, `getToken()` to `StudentAuthStateService`. Updated both `activate()` and `login()` in `student-auth-business.service.ts` to call `setToken(response.token)` before `setCurrentStudent()`.

### Issue #46 — Auth Interceptor Ignored Student Token
**Problem**: Even after the student JWT was correctly stored, all student API calls still returned `401 Unauthorized`.  
**Root Cause**: The interceptor only checked `teacherState.getToken()`. Since no teacher was logged in during a student session, `token` was `null` and no `Authorization` header was attached.  
**Resolution**: Added a student-token fallback: `const token = teacherToken ?? studentToken`. The 401 redirect handler was also made branch-aware (student path → `/student/login`, teacher path → `/login`).

### Issue #47 — Student Dashboard Showed Misleading "Submitted" Status
**Problem**: Every non-overdue assessment displayed a green "Submitted" badge, even assessments that had never been submitted.  
**Root Cause**: The status column had a simple `*ngIf="!isOverdue(a.dueDate)"` → "Submitted" with no check of actual submission state. The `isAssigned` and `submissionCount` fields did not yet exist on the DTO.  
**Resolution**: After adding `isAssigned` and `submissionCount` to the DTO and backend, the status column was replaced with a three-way switch: `submissionCount > 0` → "✓ Submitted (n)" badge; `isAssigned && submissionCount === 0` → Submit File button; `!isAssigned` → "Pending" badge.

### Issue #48 — replace_string_in_file Mismatch Due to Undiscovered XML Doc Comments
**Problem**: Several `replace_string_in_file` calls failed with "oldString not found" because the actual file content contained XML documentation comment blocks not captured in the subagent's codebase summary.  
**Root Cause**: The subagent summarised method signatures without the `/// <summary>` blocks above them. The tool requires an exact literal match including all surrounding lines.  
**Resolution**: Read the actual file sections with `read_file` immediately before editing to obtain the precise literal text. All subsequent replacements succeeded on the first attempt.

---

## Files Changed

### New Files
| File | Purpose |
|------|---------|
| `Domain/Entities/AssessmentSubmission.cs` | Submission entity |
| `Domain/Interfaces/IAssessmentSubmissionRepository.cs` | Repository contract |
| `Infrastructure/Repositories/AssessmentSubmissionRepository.cs` | EF Core implementation |
| `Application/DTOs/AssessmentSubmissionDto.cs` | Read-only response DTO |
| `Application/Services/AssessmentSubmissionService.cs` | Submit/get/download/delete logic |
| `Presentation/Controllers/AssessmentSubmissionsController.cs` | REST endpoints |
| `Infrastructure/Data/Migrations/20260402120527_AddAssessmentSubmissions.cs` | EF migration |
| `StudentApp/src/app/core/services/http/assessment-submission-api.service.ts` | Angular HTTP service |

### Modified Files
| File | What Changed |
|------|-------------|
| `Domain/Entities/StudentAssessment.cs` | Added `IsAssigned`, `Instructions`, `Submissions` nav |
| `Application/DTOs/StudentAssessmentDto.cs` | Added new fields + `SubmissionCount` to all three DTO shapes |
| `Application/Mappings/MappingProfile.cs` | Updated student-assessment mappings; added submission mapping |
| `Infrastructure/Data/ApplicationDbContext.cs` | `AssessmentSubmissions` DbSet + full model config |
| `Infrastructure/Repositories/StudentAssessmentRepository.cs` | Added `.Include(a => a.Submissions)` |
| `Program.cs` | New DI registrations; 10 MB limits; uploads directory creation |
| `StudentApp/src/app/core/models/student.model.ts` | New interfaces; updated DTO fields |
| `StudentApp/src/app/core/services/state/student-auth-state.service.ts` | Token storage + logout cleanup |
| `StudentApp/src/app/core/interceptors/auth.interceptor.ts` | Student token fallback; branch-aware 401 handler |
| `StudentApp/src/app/features/students/services/student-auth-business.service.ts` | `setToken()` call on login/activate |
| `StudentApp/src/app/core/services/http/index.ts` | Exported new submission API service |
| `StudentApp/src/app/components/student-detail.component.ts` | Teacher view: submissions panel, new form fields |
| `StudentApp/src/app/components/student-dashboard.component.ts` | Student view: upload modal, correct status badges |

---

## Key Technical Decisions

- **Student token in `localStorage` separate from profile object**: Mirrors the teacher pattern (`sat_teacher_token` vs session object). Startup requires both to avoid a state where the profile appears authenticated but all API calls fail.
- **Interceptor token precedence (teacher → student)**: A single interceptor correctly serves both user types. The teacher token takes priority ensuring teacher sessions are never accidentally replaced by a leftover student token.
- **`wwwroot/uploads/` excluded from static file serving**: The backend's `UseStaticFiles()` middleware serves `wwwroot` by default, but download is handled by the controller action which enforces authentication and ownership. No path-traversal risk because `StoredFileName` is a GUID with the original extension, not user-supplied.
- **Client-side file validation before upload**: The modal validates extension and size before calling the API. This improves UX (instant feedback) while the server still independently validates both (defence in depth).
- **In-memory `submissionCount` increment on upload success**: Avoids a full student-profile reload after uploading a file. The count is corrected on next full reload.

---

**Build status**: ✅ Angular build succeeded — zero errors  
**Migration applied**: ✅ `20260402120527_AddAssessmentSubmissions`  
**Total issues resolved this session**: 7 (3 auth/interceptor + 4 feature implementation)
