# Daily Report — March 26, 2026

**Developer**: Developer.03
**Sprint**: Sprint 4 (March 23–29, 2026)
**Project**: Student Assessment Tracker

---

## What I Did Today

Implemented the **ID/Passport No. field for teacher registration** end-to-end, covering all layers of the multi-layered architecture — domain entity, database, DTOs, validation, AutoMapper mappings, Angular frontend form, and documentation.

---

## What Was Completed

### Backend (ASP.NET Core / EF Core)

- **Domain Entity** — Added `IdPassportNo` string property to `Teacher.cs`.
- **DTOs** — Added `IdPassportNo` to `TeacherResponseDto`, `TeacherRegisterDto`, and `TeacherUpdateDto` in `TeacherDto.cs`.
- **FluentValidation** — Added `IdPassportNo` validation rules to both `TeacherRegisterValidator` and `TeacherUpdateValidator`:
  - `NotEmpty` — field is required.
  - `Length(9)` — must be exactly 9 characters.
  - `Matches("^[a-zA-Z0-9]+$")` — only letters and digits allowed; hyphens and special characters explicitly rejected.
- **DbContext** — Configured `IdPassportNo` in `ApplicationDbContext.cs` with `IsRequired()`, `HasMaxLength(20)`, and a unique index (`IX_Teachers_IdPassportNo`).
- **EF Core Migration** — Created and applied migration `AddIdPassportNoToTeacher` (timestamp `20260326131549`). Migration includes a SQL backfill step to assign unique placeholder values to any existing rows before creating the unique index.
- **AutoMapper** — No mapping changes needed; convention-based mapping resolved `IdPassportNo` automatically.

### Frontend (Angular)

- **Models** (`teacher.model.ts`) — Added `idPassportNo: string` to `Teacher`, `CreateTeacherDto`, `UpdateTeacherDto`, and `TeacherLoginResponse` interfaces.
- **Signup Form** (`signup-form.component.ts`) — Added the ID/Passport No. input field with:
  - `required`, `minlength="9"`, `maxlength="9"`, `pattern="^[a-zA-Z0-9]+$"` validators.
  - `(keypress)="allowOnlyAlphanumeric($event)"` handler to block hyphens and special characters at the keyboard level.
  - Inline error messages for required, pattern, and length violations.
- **Component Model** — Added `idPassportNo: ''` to the `teacher` object and wired it into the `CreateTeacherDto` on submit.
- **Edit Mode** — `idPassportNo` is populated from state when loading teacher data for editing.
- **Business Service** (`teacher-business.service.ts`) — Mapped `idPassportNo` from the login API response onto the `Teacher` interface in the `login()` method.
- **New Method** — Added `allowOnlyAlphanumeric()` method that calls `preventDefault()` on any non-alphanumeric keypress.

### Documentation & Testing

- **AGILE_HIERARCHY.md** — Updated:
  - Hierarchy map: TASK-01 updated to reference `TeacherRegisterDto` with `IdPassportNo`; TASK-05 and TASK-06 updated to mention alphanumeric pattern.
  - US-01: Story text, acceptance criteria, tasks, and app example updated to include ID/Passport No.
  - US-02: Acceptance criteria updated with the 9-character alphanumeric rule; tasks and app example updated.
- **Postman Collection** — Added `"idPassportNo": "AB1234567"` to both Register Teacher and Update Teacher request bodies; updated endpoint descriptions to document the validation rule.
- **Postman Environment** — Updated `_postman_exported_at` date to `2026-03-26`.

### GitHub

- Committed 13 changed/new files with message:
  > `feat: add IdPassportNo field to teacher registration`
- Pushed to `main` branch at:
  > https://github.com/AtreusTefo/Student-Assessment-Tracker-App

---

## Challenges Faced and How They Were Resolved

### 1. EF Core Migration Failed — Unique Index on Existing Empty Rows
**Challenge**: When running `dotnet ef database update`, the migration failed with:
> *"The CREATE UNIQUE INDEX statement terminated because a duplicate key was found — duplicate key value is ()."*

Existing teacher rows all had `IdPassportNo = ''` (the default value), which caused a unique constraint violation when the index was created.

**Resolution**: Added a `migrationBuilder.Sql()` backfill step inside the migration's `Up()` method to assign unique placeholder values (`T00000001`, `T00000002`, etc.) to all existing rows before the `CREATE UNIQUE INDEX` statement executes:
```sql
UPDATE Teachers SET IdPassportNo = 'T' + RIGHT('00000000' + CAST(Id AS VARCHAR(8)), 8) WHERE IdPassportNo = ''
```

---

### 2. Database in Partially-Applied State After Failed Migration
**Challenge**: The first migration attempt added the `IdPassportNo` column but then failed on the index step. This left the database in a partially applied state — the column existed in the table but the migration was not recorded in `__EFMigrationsHistory`. Subsequent migration attempts then failed for different reasons.

**Resolution**: Dropped the development database entirely (`dotnet ef database drop --force`) and re-ran `dotnet ef database update` to recreate it cleanly from scratch. Since this is a local development database with no production data, this was the safest and fastest approach.

---

### 3. TypeScript Compile Error — `idPassportNo` Missing from `Teacher` Interface
**Challenge**: After adding `idPassportNo` as a required field on the `Teacher` interface, the Angular build produced:
> *"TS2741: Property 'idPassportNo' is missing in type '{ id: number; firstName: string; ... }' but required in type 'Teacher'."*

The error originated in `teacher-business.service.ts` inside the `login()` method, where a `Teacher` object was constructed from the API login response without the new field.

**Resolution**: Added `idPassportNo: response.teacher.idPassportNo || ''` to the object literal in the `login()` method.

---

### 4. `dotnet run` Failing Due to Locked Executable (API Already Running)
**Challenge**: When attempting to build for the EF migration, the build failed because another process (`StudentAssessmentTracker.exe`, PID 16932) still had the compiled output file locked — the API was still running from a previous terminal session.

**Resolution**: Used `Stop-Process -Id 16932 -Force` in PowerShell to terminate the running API process, then retried `dotnet ef migrations add`.

---

### 5. Multiple Regex Pattern Matches Causing `replace_string_in_file` Failure
**Challenge**: When attempting to update the `IdPassportNo` regex pattern from `^[a-zA-Z0-9\-]+$` to `^[a-zA-Z0-9]+$` in `TeacherValidator.cs`, the tool failed because both `TeacherRegisterValidator` and `TeacherUpdateValidator` contained the identical string, making the match non-unique.

**Resolution**: Updated `TeacherRegisterValidator` in the first pass (which succeeded as it matched one occurrence), then updated `TeacherUpdateValidator` separately in a targeted replacement using additional surrounding context to uniquely identify the correct match.

---

### 6. `git push` Rejected — Remote Ahead of Local
**Challenge**: After committing locally, `git push origin main` was rejected:
> *"Updates were rejected because the remote contains work that you do not have locally."*

The GitHub remote had commits made outside of the current local session.

**Resolution**: Stashed the unstaged working changes (`git stash`), pulled and rebased onto the remote (`git pull --rebase origin main`), restored the stash (`git stash pop`), then pushed successfully.
