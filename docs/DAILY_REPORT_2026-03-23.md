# Daily Report — March 23, 2026

**Project:** Student Assessment Tracker  
**Developer:** Developer.03

---

## What I Did Today

- Designed and implemented a complete student authentication system across both the Angular frontend and the .NET API backend
- Added a student account activation flow (sign-up using a teacher-assigned StudentUniqueId and email verification)
- Added a student login flow (StudentUniqueId + password)
- Built a student personal performance dashboard displaying scores, percentages, performance level, and assessments table
- Created dedicated Angular route guards to protect student-authenticated routes and prevent re-login when already authenticated
- Added a `Password` column to the Students database table via an Entity Framework migration
- Added new API endpoints for student activation and login
- Committed and pushed all changes to the `main` branch on GitHub

---

## What Was Completed

### 1. Student Account Activation — `StudentActivateComponent`
- New standalone Angular component (`student-activate.component.ts`)
- Form collects: teacher-assigned **StudentUniqueId**, registered **Email**, and a new **Password** with confirmation
- Password mismatch validation and visibility toggles on both password fields
- Routes to `/student/dashboard` on success, or `/student/login` if the account is already activated

### 2. Student Login — `StudentLoginComponent`
- New standalone Angular component (`student-login.component.ts`)
- Reactive form accepting **StudentUniqueId** and **Password**
- Password visibility toggle, real-time error clearing on input
- Routes to `/student/dashboard` on successful authentication

### 3. Student Performance Dashboard — `StudentDashboardComponent`
- New standalone Angular component (`student-dashboard.component.ts`)
- Displays student profile (name, ID, grade, contact details, membership date)
- Performance summary cards: Total Score, Average Score, Percentage, Performance Level
- Dynamic colour-coded progress bar based on score percentage range
- Full assessments table with individual scores, due dates, and status badges
- Logout button that clears state and session

### 4. Route Guards
- **`student-auth.guard.ts`** — Protects `/student/dashboard`; redirects unauthenticated users to `/student/login`
- **`student-guest.guard.ts`** — Prevents logged-in students from revisiting `/student/login` or `/student/activate`; redirects to `/student/dashboard`

### 5. Student Auth State Service — `StudentAuthStateService`
- Manages `currentStudent`, `isAuthenticated`, `loading`, and `error` state via BehaviorSubjects
- Persists session to `localStorage` key `sat_current_student` for page-refresh resilience
- Restores session automatically on service initialisation
- Provides public observables for component subscriptions and helper methods (`logout()`, `setError()`, etc.)

### 6. Student Auth Business Service — `StudentAuthBusinessService`
- Orchestrates `activate()` and `login()` operations by combining `StudentApiService` calls with `StudentAuthStateService` state updates
- Handles error extraction and propagation
- `logout()` clears state and redirects

### 7. Angular Routes — `app.routes.ts`
- Added three new routes:
  - `/student/login` → `StudentLoginComponent` with `studentGuestGuard`
  - `/student/activate` → `StudentActivateComponent` with `studentGuestGuard`
  - `/student/dashboard` → `StudentDashboardComponent` with `studentAuthGuard`

### 8. Backend — Student Entity Update (`Student.cs`)
- Added `Password` field (nullable `string`) to the `Student` domain entity
- Retained helper methods: `GetTotalScore()`, `GetMaxPossible()`, `GetPercentage()`, `GetAverageScore()`, `GetPerformanceLevel()`

### 9. Backend — New DTOs (`StudentDto.cs`)
- **`StudentProfileDto`** — Safe public profile returned to students (no password exposed)
- **`StudentActivateDto`** — Carries `StudentUniqueId`, `Email`, and `Password` for activation
- **`StudentLoginDto`** — Carries `StudentUniqueId` and `Password` for login
- **`StudentLoginResponseDto`** — Wraps a demo token and `StudentProfileDto` as the login/activation response

### 10. Backend — Student Service (`StudentService.cs`)
- **`ActivateStudentAsync()`** — Validates that `StudentUniqueId` + `Email` match an existing record, checks the account is not already activated, stores the hashed password, and returns a `StudentLoginResponseDto`
- **`LoginStudentAsync()`** — Validates the `StudentUniqueId` exists, verifies the account is activated, checks the password, and returns a `StudentLoginResponseDto`
- Full logging and error handling throughout

### 11. Backend — Students Controller (`StudentsController.cs`)
- **`POST /api/students/activate`** — Returns `400` if already activated, `401` if credentials don't match, `200` with login response on success
- **`POST /api/students/login`** — Returns `400` if account is not yet activated, `401` for invalid credentials, `200` with login response on success

### 12. Database Migration — `AddStudentPassword`
- Migration `20260323100016_AddStudentPassword` adds a nullable `nvarchar(255)` `Password` column to the `Students` table
- `Down()` rolls back by dropping the column
- `ApplicationDbContextModelSnapshot.cs` updated accordingly

---

## Challenges Faced and How They Were Resolved

### Challenge 1 — Keeping Student Authentication Separate from Teacher Authentication
**What happened:** The project already had a teacher authentication flow (login, state service, guards). Introducing student authentication risked code conflicts, route collisions, and shared-state confusion.  
**Resolution:** The student auth system was built as a completely parallel implementation — separate state service (`StudentAuthStateService`), separate business service (`StudentAuthBusinessService`), separate guards, separate localStorage key (`sat_current_student`), and separate route namespace (`/student/*`). This kept all concerns isolated and the teacher flow untouched.

### Challenge 2 — Student Activation vs. Registration
**What happened:** Students are pre-created by teachers (they already exist in the database) and are not allowed to self-register. A standard sign-up flow would incorrectly allow students to create new records.  
**Resolution:** The activation endpoint (`POST /api/students/activate`) does not create a new student. Instead, it validates the supplied `StudentUniqueId` + `Email` against an existing record and only stores the password if they match. If no match is found, it returns `401 Unauthorized`. If the account is already activated, it returns `400 Bad Request` with a descriptive message.

### Challenge 3 — Exposing Password in API Responses
**What happened:** The existing `StudentDto` contained all student fields and was used across all existing endpoints. Adding a `Password` field to the entity risked it being inadvertently serialised and returned in GET responses.  
**Resolution:** A new `StudentProfileDto` was created that explicitly omits the `Password` field. This DTO is the only type returned by the login and activation endpoints, and existing GET endpoints continue to use the original DTO (which does not map the password field). No password data ever appears in API responses.

### Challenge 4 — Route Guard Redirect Loops
**What happened:** Without careful guard logic, a logged-in student hitting `/student/login` could be redirected, and an unauthenticated student hitting `/student/dashboard` could cause a redirect loop between guards.  
**Resolution:** `studentAuthGuard` only redirects non-authenticated users to `/student/login` (no further guard on that route). `studentGuestGuard` only redirects authenticated users to `/student/dashboard` (protected by `studentAuthGuard`, not `studentGuestGuard`). The two guards are applied to mutually exclusive route sets, preventing any loop.

---

*End of Daily Report — March 23, 2026*
