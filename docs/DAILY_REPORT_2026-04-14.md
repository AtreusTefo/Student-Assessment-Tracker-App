# Daily Report — April 14, 2026

**Developer**: Atreus Tefo Ramokate  
**Sprint**: API Fixes, Feature Development & Documentation  
**Project**: Student Assessment Tracker

---

## What I Did Today

Two distinct work streams were completed:

1. **API bug fixes and new features** — fixed a critical runtime exception in audit logging, seeded the database with a default admin account, implemented a change-password endpoint for admins, and rebuilt the Postman collection and Swagger UI documentation.

2. **Frontend documentation audit** — audited the entire Angular frontend codebase (`StudentApp/`) to produce a complete and accurate inventory of all components, services, guards, interceptors, models, routes, and dependencies. Compared the inventory against the existing documentation suite and updated every doc that contained stale or incorrect references to the frontend. No application code was changed in this stream — all changes are documentation only.

---

## What Was Completed

---

### 1 — Audit Log Fire-and-Forget Bug Fixed

**Files**: `Presentation/Controllers/StudentsController.cs`, `TeachersController.cs`, `StudentAssessmentsController.cs`

**Problem:** Deleting (or creating/updating) a student, teacher, or assessment threw a runtime exception: `Failed to write audit log for Delete on Student#8`. The root cause was that audit log calls were written as fire-and-forget:

```csharp
_ = _auditLog.LogAsync(...);  // discards the Task
```

When the HTTP request scope ended, ASP.NET Core disposed the scoped `ApplicationDbContext`. The discarded task then resumed and attempted to call `SaveChangesAsync()` on an already-disposed context, causing the `ObjectDisposedException`.

**Fix:** All 9 fire-and-forget calls across the three controllers were changed to `await`:

```csharp
await _auditLog.LogAsync(...);  // waits before scope ends
```

This is safe because `LogAsync` is wrapped in an internal `try/catch`, so a logging failure never bubbles up to a 500 response.

| Controller | Methods fixed |
|---|---|
| `StudentsController` | Create, Update, Delete student |
| `TeachersController` | Create, Update, Delete teacher |
| `StudentAssessmentsController` | Create, Update, Delete assessment |

---

### 2 — Default Admin Seed Account Created

**Files**: `Infrastructure/Data/ApplicationDbContext.cs`, new migration `20260414122608_SeedDefaultAdmin`

**Problem:** `POST /api/admins` is protected by `[Authorize(Roles = "Admin")]`. On a fresh database there are no admin rows, making the endpoint permanently unreachable — a chicken-and-egg bootstrap problem.

**Fix:**
1. Generated a stable BCrypt hash for the default password `Admin@123` (work factor 11) using a temporary .NET console program, ensuring the hash is deterministic for EF Core seed data.
2. Added a `HasData` seed block inside `ApplicationDbContext.OnModelCreating`:
   ```csharp
   entity.HasData(new Admin {
       Id = 1,
       FirstName = "System",
       LastName = "Admin",
       Email = "admin@school.com",
       Password = "$2a$11$F/NmweY.Jk.ddRIkhzD4Du.pTCIHHaBDr1YArTiX4PR65ddykJ0km",
       CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
       UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
   });
   ```
3. Created and applied migration `SeedDefaultAdmin` (`dotnet ef database update`).

**Default credentials:** `admin@school.com` / `Admin@123`

---

### 3 — Admin Change Password Endpoint Implemented

**Files**: `Application/DTOs/AdminDto.cs`, `Application/Services/AdminService.cs`, `Presentation/Controllers/AdminsController.cs`

Added `PUT /api/admins/{id}/password` so admin credentials can be changed without direct database access.

**DTO added** (`AdminDto.cs`):
```csharp
public class ChangeAdminPasswordDto {
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }       // min 6 chars
    public string? ConfirmNewPassword { get; set; } // must match NewPassword
}
```

**Service** (`IAdminService` + `AdminService`): Added `ChangePasswordAsync(int adminId, ChangeAdminPasswordDto dto)`. Validates minimum length, confirmation match, BCrypt-verifies the current password, hashes the new password, updates `UpdatedAt`, and saves.

**Controller** (`AdminsController`): Added `[Authorize(Roles = "Admin")] [HttpPut("{id:int}/password")]`. Security checks:
- `callerId != id.ToString()` → **403 Forbidden** (own-account-only)
- Current password mismatch → **401 Unauthorized**
- Missing/invalid fields → **400 Bad Request**
- Success → **204 No Content** + audit log entry written

---

### 4 — Postman Collection Rebuilt and Swagger UI Updated

**Files**: `StudentAssessmentTracker.postman_collection.json`, `StudentAssessmentTracker.postman_environment.json`, `Program.cs`

**Postman collection** — fully rebuilt from scratch with 10 folders covering all API endpoints:

| Folder | Requests |
|---|---|
| Auth | Admin Login, Teacher Login, Student Login |
| Students | Get All, Get by ID, Create, Update, Delete, Assign Teacher |
| Student Assessments | Get All, Get by ID, Create, Update, Delete |
| Assessment Submissions | Get All, Get by ID, Create, Update, Delete |
| Class Groups | Get All, Get by ID, Create, Update, Delete |
| Teachers | Get All, Get by ID, Create, Update, Delete |
| Admins | Get All, Get by ID, Create, Change Password |
| Reports | Student Report, Teacher Report |
| Grades | Get All |
| Subjects | Get All |

All protected requests include `Authorization: Bearer {{variable}}` headers. Login requests auto-save tokens to environment variables using `pm.environment.set()`. Create requests auto-save returned IDs (`studentId`, `assessmentId`, `classGroupId`, `submissionId`).

**Postman environment** — expanded from 4 to 10 variables: `baseUrl`, `teacherToken`, `adminToken`, `studentToken`, `teacherId`, `studentId`, `assessmentId`, `classGroupId`, `submissionId`, `adminId`.

**Swagger UI** (`Program.cs`) — updated with:
- Full authentication table documenting which roles can log in at which endpoint (`/api/admins/login`, `/api/teachers/login`, `/api/students/activate`)
- `DisplayRequestDuration()` — shows response time per request
- `DocExpansion(List)` — collapsed by default for readability
- `DefaultModelsExpandDepth(1)` — schema models partially collapsed
- `EnableDeepLinking()` — allows direct URL linking to specific operations

---

### 5 — Postman JSON Corruption Resolved

**File**: `StudentAssessmentTracker.postman_collection.json`

During the collection rebuild a `replace_string_in_file` call matched only a partial range, causing the old file content to be appended after the new JSON's closing `}`. This produced a "End of file expected" parse error at line 917.

**Fix:** Used PowerShell `Get-Content` + `Set-Content` to read only lines 1–916 (the complete valid JSON) and write them back, discarding the appended garbage.

---

### 6 — Root `README.md` Updated

**File**: `README.md`

Corrected multiple stale references to the Angular frontend that had not been updated since Angular 18:

| Section | Before | After |
|---|---|---|
| Intro sentence | Angular 18 | Angular 21 |
| Frontend bullet list | No mention of zoneless, Vitest, or DataTables Buttons | Includes zoneless mode, Vitest 4, DataTables Buttons plugin |
| Frontend tech stack table | Angular 18, TypeScript 5, no test runner | Angular 21, TypeScript 5.9, RxJS 7.8, Vitest 4, DataTables Buttons |
| Project structure — `StudentApp/` | 8 components, 4 flat dirs (`components/`, `services/`, `models/`, `guards/`) | 10 components, accurate 3-tier structure (`components/`, `core/`, `features/`) |
| `core/` services count | 6 HTTP services | 9 HTTP services (added `report-api`, `admin-api`, `class-group-api`) |
| Guards listed | auth, guest, student-auth, student-guest | + admin guard |

---

### 7 — `ARCHITECTURE.md` Frontend Section Updated

**File**: `README.md`

Corrected multiple stale references to the Angular frontend that had not been updated since Angular 18:

| Section | Before | After |
|---|---|---|
| Intro sentence | Angular 18 | Angular 21 |
| Frontend bullet list | No mention of zoneless, Vitest, or DataTables Buttons | Includes zoneless mode, Vitest 4, DataTables Buttons plugin |
| Frontend tech stack table | Angular 18, TypeScript 5, no test runner | Angular 21, TypeScript 5.9, RxJS 7.8, Vitest 4, DataTables Buttons |
| Project structure — `StudentApp/` | 8 components, 4 flat dirs (`components/`, `services/`, `models/`, `guards/`) | 10 components, accurate 3-tier structure (`components/`, `core/`, `features/`) |
| `core/` services count | 6 HTTP services | 9 HTTP services (added `report-api`, `admin-api`, `class-group-api`) |
| Guards listed | auth, guest, student-auth, student-guest | + admin guard |

---

### 8 — `ARCHITECTURE.md` Frontend Section Updated

**File**: `ARCHITECTURE.md`

Updated both the project structure directory tree and the "Frontend Layer" description:

- Directory tree entry changed from Angular 18 with flat `services/`, `models/`, `guards/` folders to the correct `core/` and `features/` nested structure with all 9 HTTP services and 3 state services shown.
- "Frontend Layer" description rewritten from a bullet list of 4 flat folders to an accurate three-tier description (`components/` → `core/` → `features/`) with Angular 21 design decisions documented: no NgModule, no zone.js, no lazy loading, no environment files, Vitest test runner.

---

### 9 — `StudentApp/README.md` Rewritten

**File**: `StudentApp/README.md`

Replaced the default Angular CLI-generated boilerplate (`ng serve`, `ng generate`, `ng build`, `ng e2e`) with project-specific documentation:

- **Tech stack table** — all runtime and dev dependencies with versions and purpose
- **Development commands** — `npm install`, `npm start`, `npm run build` with proxy explanation and `wwwroot` copy instruction
- **Full project structure tree** — every file and folder in `src/app/` annotated with its responsibility
- **Routes table** — all 11 routes with component name and guard
- **Authentication section** — all 6 `localStorage` keys documented (teacher, student, admin) with which service owns each
- **"Adding a New Feature" guide** — 5-step checklist covering API service → state → business logic → component → guard
- **CSS/styling notes** — explains global DataTables CSS loaded via `angular.json`

---

### 10 — `docs/DATATABLES_INTEGRATION.md` Updated

**File**: `docs/DATATABLES_INTEGRATION.md`

Updated to reflect the current DataTables setup, which has grown since the original integration:

| Section | Before | After |
|---|---|---|
| Title | "DataTables Integration Summary" | "DataTables Integration" |
| Packages listed | `datatables.net`, `datatables.net-dt` | + `datatables.net-buttons`, `datatables.net-buttons-dt` |
| Version info | Vague "installed versions" | Exact versions from `package.json` |
| CSS loading | Described as component-level | Correctly documented as global in `angular.json` |
| Implementation section | Stale `dom: 'lfrtip'` (no Buttons), `initializeDataTable()` private method | Updated to `dom: 'Blfrtip'`, `buttons: ['csv']`, inline pattern |
| Files modified list | Listed specific line-by-line changes to older code | Replaced with current architectural description |
| Build status | Hard-coded bundle sizes | Replaced with a general build budget note |

---

## Challenges Faced and How They Were Resolved

---

### Challenge 1 — Audit Log Disposed-Context Exception

**Problem:** The audit log calls in all three CRUD controllers used the fire-and-forget pattern (`_ = _auditLog.LogAsync(...)`). The scoped `ApplicationDbContext` was disposed when the HTTP request scope ended, but the discarded Task continued executing asynchronously and attempted `SaveChangesAsync()` on the now-disposed context, throwing `ObjectDisposedException`.

**Resolution:** Changed all 9 calls to `await _auditLog.LogAsync(...)`. The `await` ensures the log write completes before the request scope (and its DI-managed `DbContext`) is disposed.

---

### Challenge 2 — Admin Bootstrap Chicken-and-Egg

**Problem:** `POST /api/admins` requires an existing admin JWT to authorize. On a fresh database there are no admins, making the endpoint forever unreachable.

**Resolution:** Added EF Core `HasData` seed with a pre-computed BCrypt hash. The hash had to be generated outside the codebase (temp console app) because `HasData` requires a deterministic, compile-time value — BCrypt's random salt means calling `BCrypt.HashPassword()` at runtime produces a different hash on every migration run.

---

### Challenge 3 — Postman JSON Corruption After Partial Replace

**Problem:** A `replace_string_in_file` call on the Postman collection matched a partial range, and the tool appended the remaining original file content after the new JSON's closing `}`. Postman reported "End of file expected" at line 917.

**Resolution:** Used PowerShell `Get-Content` to read the file and `Select-Object -First 916` to isolate valid lines, then `Set-Content` to write them back. The appended garbage was truncated cleanly.

---

### Challenge 4 — PowerShell Heredoc Syntax Caused File-Write Failures

**Problem:** Two attempts were made to overwrite `DATATABLES_INTEGRATION.md` using PowerShell here-strings (`@' ... '@`) and Python inline `-c` strings. Both failed — the PowerShell here-string terminator (`'@`) was interpreted as part of a pipeline rather than a string end, and the Python inline string was corrupted by backtick escaping in the multi-line `-c` argument.

**Resolution:** Abandoned whole-file overwrites via terminal. Instead used targeted `replace_string_in_file` and `multi_replace_string_in_file` calls to surgically replace specific sections of the file. The file was read first to obtain exact text matches, then each outdated section was replaced individually.

---

### Challenge 5 — Documentation Was Spread Across Many Files With Overlapping Content

**Problem:** The project contains 30+ documentation files. Many describe the same frontend (e.g., `ARCHITECTURE.md`, `README.md`, `ARCHITECTURE_IMPLEMENTATION.md`, `IMPLEMENTATION_SUMMARY.md`, `DEVELOPER_GUIDE.md`, `DATATABLES_INTEGRATION.md`) and had diverged from each other and from the actual code over multiple development sessions.

**Resolution:** Performed a single comprehensive codebase inventory up front (full directory tree, all key file contents, all routes, all services, all guards) before touching any documentation. This produced one authoritative source of truth. Only the four files most likely to be read by a developer onboarding to the project were updated — `README.md`, `ARCHITECTURE.md`, `StudentApp/README.md`, and `DATATABLES_INTEGRATION.md` — to keep the change set focused and reviewable.

---

### Challenge 6 — Angular Version Mismatch Across All Docs

**Problem:** Every documentation file that mentioned "Angular 18" was outdated. The project is running Angular CLI 21.1.2 with significant differences from v18: zoneless mode, `@angular/build:application` (esbuild-based), no `zone.js`, Vitest replacing Karma, and `withFetch()` / functional interceptors replacing class-based interceptors.

**Resolution:** All Angular version references in the updated documents were corrected to Angular 21 and the relevant architectural implications (zoneless, no NgModule, Vitest) were documented where appropriate.
