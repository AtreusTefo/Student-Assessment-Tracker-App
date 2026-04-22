# Daily Report — April 20, 2026

**Project:** Student Assessment Tracker  
**Branch:** `dev2`  
**Developer:** Atreus Ramokate

---

## What I Did Today

Worked across the Angular frontend and ASP.NET Core backend to fix authentication behaviour, improve form validation UX, and harden the data layer. Updated project documentation to reflect the current state of the system.

### Frontend (Angular — `StudentApp`)

- **Auth Interceptor (`auth.interceptor.ts`):** Reworked the JWT injection logic so that the admin token is attached to **all** outgoing API requests when an admin is logged in, not just `/api/admins` routes. This ensures admin-initiated calls to `/api/teachers`, `/api/students`, and other endpoints are properly authorised.

- **Admin Login (`admin-login.component.ts`):** Added a show/hide password toggle button inside the password field. Introduced `maxlength="20"` validation and the corresponding inline error message. Added a `clearError()` helper that resets the error message on every keystroke.

- **Login Form (`login-form.component.ts`):** Added a missing "Password cannot exceed 20 characters" validation error message to match the `maxlength` constraint already present on the input.

- **Student Activate (`student-activate.component.ts`):** Added `confirmPassword` to the `StudentActivateDto` object and ensured it is populated from the separate form field before the activation request is sent to the API.

- **Student Login (`student-login.component.ts`):** Updated the inline activation DTO construction to include `confirmPassword`, keeping it consistent with the dedicated activation component.

- **Student Model (`student.model.ts`):** Added `confirmPassword: string` to the `StudentActivateDto` interface so both activation components compile correctly.

- **Admin Dashboard (`admin-dashboard.component.ts`):** Fixed a logic bug in the teacher-creation error handler — the previous ternary expression always evaluated the first operand, meaning `err.error.message` was ignored when `err.error.errors` was also present. Corrected the operator precedence with explicit null checks.

### Backend (ASP.NET Core — `StudentAssessmentTrackerAPI`)

- **ClassGroupService (`ClassGroupService.cs`):** Added a null-guard (`cgs.ClassGroup != null`) to the subject-conflict LINQ query to prevent a potential `NullReferenceException` when `ClassGroup` navigation property is not loaded.

- **SwaggerAuthOperationFilter (`SwaggerAuthOperationFilter.cs`):** Added the missing `/// <inheritdoc />` XML doc comment to suppress the CS1591 compiler warning on the `Apply` method.

### Documentation

- **`README.md`:** Expanded the project overview with updated setup instructions, API endpoint reference, and tech-stack details.
- **`docs/AGILE_HIERARCHY.md`:** Major expansion — added sprint backlog, user stories, and team hierarchy diagrams.
- **`docs/PROJECT_REQUIREMENTS.md`:** Updated functional and non-functional requirements to reflect the current feature set.

---

## What Was Completed

| # | Item | Status |
|---|------|--------|
| 1 | Admin token applied globally to all API routes (not just `/api/admins`) | ✅ Done |
| 2 | Show/hide password toggle on Admin Login form | ✅ Done |
| 3 | `maxlength` validation error message on Admin Login and Login Form | ✅ Done |
| 4 | `confirmPassword` field wired through `StudentActivateDto` end-to-end | ✅ Done |
| 5 | Teacher-creation error-handler logic bug fixed | ✅ Done |
| 6 | Null-guard on `ClassGroup` navigation property in subject-conflict check | ✅ Done |
| 7 | Swagger XML doc warning resolved on `SwaggerAuthOperationFilter` | ✅ Done |
| 8 | README, AGILE_HIERARCHY, PROJECT_REQUIREMENTS docs updated | ✅ Done |
| 9 | All changes committed and pushed to `origin/dev2` (commit `35df95c`) | ✅ Done |

---

## Challenges Faced and How They Were Resolved

### 1. Admin token not sent to non-admin API routes
**Problem:** The auth interceptor only attached the admin JWT to requests containing `/api/admins` in the URL. As a result, admin dashboard calls to `/api/teachers` and `/api/students` returned 401 Unauthorised errors.  
**Resolution:** Removed the `isAdminRoute` condition. The interceptor now attaches the admin token to every outgoing request when one is present in `localStorage`, which is safe because the admin is the only role that stores a token under the `admin_token` key.

### 2. `confirmPassword` not reaching the backend
**Problem:** The `StudentActivateDto` interface did not include `confirmPassword`, so the field existed on the form but was silently dropped before the HTTP request was sent. The backend validator therefore always received an empty string for that field.  
**Resolution:** Added `confirmPassword: string` to the `StudentActivateDto` TypeScript interface and explicitly assigned the local form field value to the DTO in both `StudentActivateComponent` and `StudentLoginComponent` before calling the activate API.

### 3. Teacher-creation error message not displayed correctly
**Problem:** A faulty ternary expression in the admin dashboard error handler caused `err.error.message` to be ignored whenever `err.error.errors` was also truthy. The displayed error was always the raw JSON dump of validation errors rather than the human-readable message.  
**Resolution:** Rewrote the expression using explicit `||` chaining with null checks so `err.error.message` is preferred, `err.error.errors` is used as a fallback, and the generic string is the last resort.

### 4. Potential `NullReferenceException` in `ClassGroupService`
**Problem:** The subject-conflict LINQ query accessed `cgs.ClassGroup.SubjectId` without first confirming that the `ClassGroup` navigation property was loaded. In cases where EF Core lazy-loading is disabled this could throw a `NullReferenceException`.  
**Resolution:** Added `&& cgs.ClassGroup != null` as a guard condition before the `SubjectId` comparison.
