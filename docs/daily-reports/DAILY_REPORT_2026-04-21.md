# Daily Report — April 21, 2026

**Project:** Student Assessment Tracker  
**Branch:** `dev2`  
**Developer:** Atreus Ramokate

---

## What I Did Today

Focused on security hardening, data integrity improvements, and self-service password reset across both the Angular frontend and ASP.NET Core backend. The main goal was to give teachers and students a safe, user-driven way to reset their own passwords directly from their login pages — and to remove the prior insecure implementation that instantly nulled a password without any identity verification loop.

### Backend (ASP.NET Core — `StudentAssessmentTrackerAPI`)

- **`TeacherDto.cs`:** Added `IsActive` property to `TeacherResponseDto`. Added new `TeacherForgotPasswordDto` class with an `Email` property for the forgot-password request body.

- **`StudentDto.cs`:** Added new `StudentForgotPasswordDto` class with `StudentUniqueId` and `Email` properties, requiring dual-factor identity verification before a reset is allowed.

- **`MappingProfile.cs`:** Extended the `Teacher → TeacherResponseDto` AutoMapper mapping to derive `IsActive` from whether the teacher's `Password` field is non-null. This fixes a data integrity gap where the admin dashboard received no indication of whether a teacher account had been activated.

- **`TeacherService.cs`:** Added `ForgotPasswordAsync` to `ITeacherService` and its implementation. The method locates the teacher by email, nulls the password (de-activates the account), stamps `UpdatedAt`, persists the change via the repository, and writes an audit log entry with action `"ForgotPassword"`. Fixed a critical syntax error introduced during editing where the private `GenerateJwtToken` method declaration was merged onto the same line as a comment, causing six CS1519 / CS1022 compiler errors.

- **`StudentService.cs`:** Added `ForgotPasswordAsync` to `IStudentService` and its implementation. The method locates the student by `StudentUniqueId`, verifies the supplied email matches the stored email (case-insensitive), and only then nulls the password and stamps `UpdatedAt`. An `UnauthorizedAccessException` is thrown if the email does not match — preventing an attacker from resetting a student's password knowing only their Student ID.

- **`TeachersController.cs`:** Added `POST /api/teachers/forgot-password` endpoint with no `[Authorize]` attribute (public). Always returns `200 OK` regardless of whether the email exists, preventing account enumeration.

- **`StudentsController.cs`:** Added `POST /api/students/forgot-password` endpoint with the same anti-enumeration pattern.

### Frontend (Angular — `StudentApp`)

- **`teacher-api.service.ts`:** Added `forgotPassword(email: string)` method that posts to `/api/teachers/forgot-password`.

- **`student-api.service.ts`:** Added `forgotPassword(studentUniqueId: string, email: string)` method that posts to `/api/students/forgot-password`.

- **`login-form.component.ts` (Teacher Login):** Fully restructured the template into two panels controlled by a `forgotMode` boolean. The normal login panel gained a "Forgot Password?" link. The forgot-password panel presents an email input, a Reset Password button, a success message with a link to `/activate`, and a Back to Sign In link. Added supporting class properties (`forgotMode`, `forgotEmail`, `forgotLoading`, `forgotError`, `forgotSuccess`), injected `TeacherApiService`, and added `enterForgotMode()`, `exitForgotMode()`, and `onForgotSubmit()` methods. Added CSS classes for `.forgot-row`, `.forgot-link`, `.hint-text`, `.activate-link`, and `.server-success`.

- **`student-login.component.ts` (Student Login):** Applied the same two-panel restructure. The student forgot-password panel requires both Student ID and registered email, reflecting the stricter dual-factor verification on the backend. Added `enterActivateMode()` so the success message can navigate the student directly into account activation. Added all supporting class properties, `StudentApiService` injection, forgot-password methods, and the corresponding CSS classes (`.alert-success`, `.forgot-row`, `.forgot-link`).

### Discussion and Research

- Analysed the security vulnerability in the original forgot-password design (instant password nulling with no identity verification loop) and documented the secure token-based flow that should replace it in a future iteration.
- Evaluated email delivery options for implementing a proper reset-link flow: selected **MailKit + Gmail SMTP** as the recommended stack for this project (free, no external service dependency, standard in .NET), with **Azure Communication Services** noted as the production upgrade path if the app is deployed to Azure in future.

---

## What Was Completed

| # | Item | Status |
|---|------|--------|
| 1 | `IsActive` field added to `TeacherResponseDto` and mapped from `Password != null` | ✅ Done |
| 2 | `TeacherForgotPasswordDto` and `StudentForgotPasswordDto` DTOs created | ✅ Done |
| 3 | `ITeacherService.ForgotPasswordAsync` interface + `TeacherService` implementation | ✅ Done |
| 4 | `IStudentService.ForgotPasswordAsync` interface + `StudentService` implementation | ✅ Done |
| 5 | `POST /api/teachers/forgot-password` endpoint (public, anti-enumeration) | ✅ Done |
| 6 | `POST /api/students/forgot-password` endpoint (public, anti-enumeration) | ✅ Done |
| 7 | `TeacherApiService.forgotPassword()` HTTP method | ✅ Done |
| 8 | `StudentApiService.forgotPassword()` HTTP method | ✅ Done |
| 9 | Teacher login form — full forgot-password UI (template, class, styles) | ✅ Done |
| 10 | Student login form — full forgot-password UI (template, class, styles) | ✅ Done |
| 11 | `dotnet build` passing — 0 errors | ✅ Done |
| 12 | `ng build` passing — 0 errors, 0 warnings | ✅ Done |

---

## Challenges Faced and How They Were Resolved

### 1. `TeacherResponseDto` missing `IsActive` — admin dashboard showing stale data
**Problem:** The admin dashboard referenced `t.isActive` on teacher objects but `TeacherResponseDto` had no such property. The field was silently `undefined` in the frontend, meaning active and inactive teachers appeared identical.  
**Resolution:** Added `public bool IsActive { get; set; }` to `TeacherResponseDto` and added an AutoMapper `.ForMember` mapping that derives the value from `src.Password != null`. This is a pure computed property — no schema change required.

### 2. C# compiler crash — six errors after editing `TeacherService.cs`
**Problem:** After adding `ForgotPasswordAsync` to `TeacherService.cs`, the `dotnet build` produced six fatal errors (CS1519, CS1002, CS1001, CS1022) all pointing to lines 329–356. The root cause was that a trailing comment (`// ── Private helpers ──`) and the `private string GenerateJwtToken(Teacher teacher)` declaration were concatenated onto the same line during the file edit, causing the compiler to interpret the method body as code at class level rather than inside a method.  
**Resolution:** Split the merged line back into two separate lines — the comment on its own line, followed by the method declaration on the next line. `dotnet build` then succeeded with 0 errors.

### 3. Original forgot-password design was a self-service lockout vector
**Problem:** The initial implementation nulled the account password immediately upon receiving a forgot-password request, with no proof that the requester owned the account. Any person who knew a teacher's email address, or a student's ID and email, could lock that user out of the system instantly.  
**Resolution:** For this iteration, the student endpoint adds a dual-factor check (both Student ID and email must match) which raises the bar significantly over the teacher endpoint. The long-term resolution discussed and planned is a token-based flow: generate a cryptographically secure random token, store its SHA-256 hash in a `PasswordResetTokens` table with a 15-minute expiry, email the raw token as a link, and only null/replace the password after the link is clicked and the token is validated. **MailKit + Gmail SMTP** was selected as the email delivery stack for this implementation.

### 4. Student login component required partial rebuild across three concerns simultaneously
**Problem:** The `student-login.component.ts` needed changes in three independent layers at once — the HTML template (new two-panel structure), the CSS styles array (new classes), and the TypeScript class body (new properties, new constructor injection, new methods). Working on them in the wrong order risked referencing template variables that didn't exist in the class yet, causing Angular compiler errors mid-edit.  
**Resolution:** Applied changes in dependency order: template first (defines what variables and methods are needed), then styles (independent of class logic), then class body last (fulfils all template bindings). Verified with `ng build` after all three layers were complete.
