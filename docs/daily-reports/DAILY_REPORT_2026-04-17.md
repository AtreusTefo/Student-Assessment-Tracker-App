# Daily Report — April 17, 2026

**Developer**: Atreus Tefo Ramokate  
**Sprint**: Bug Fixes — Routing, Swagger & Frontend Activation Flow  
**Project**: Student Assessment Tracker  
**Branch**: dev2

---

## What I Did Today

Four areas of work were completed:

1. **Admin route not accessible via `/admin`** — the Angular routing table had no entry for the bare path, so the wildcard catch-all redirected it to the teacher homepage.

2. **Swagger UI not sending HTTP requests** — three separate bugs in `Program.cs` collectively prevented any CRUD operation from executing in the Swagger UI.

3. **Frontend not updated to match backend activation model** — the frontend still used the old self-registration flow for teachers and had broken API URLs for student–teacher assignment. Full frontend alignment with the Admin-only data model was implemented.

4. **Build error TS2339** — a dead `password` reference in `teacher-business.service.ts` (leftover from the old registration flow) broke the Angular build after the model change.

---

## What Was Completed

---

### 1 — Admin Route Missing (`/admin` → 404/Wrong Page)

**Files**: `StudentApp/src/app/app.routes.ts`

**Problem:** Navigating to `http://localhost:4200/admin` did not open the admin login. Instead the user was silently redirected to the teacher homepage. The admin area was inaccessible via the intuitive short URL.

**Root Cause:** The Angular route table had entries for `/admin/login` and `/admin/dashboard` but nothing for the bare path `/admin`. The wildcard route `{ path: '**', redirectTo: '' }` caught the request and sent the user to `/`, which is the teacher-protected homepage — completely wrong role and completely wrong page.

**Fix:**
```typescript
// Added to app.routes.ts — Admin routes block:
{ path: 'admin', redirectTo: 'admin/login', pathMatch: 'full' },  // ← NEW
{ path: 'admin/login', component: AdminLoginComponent, canActivate: [adminGuestGuard] },
{ path: 'admin/dashboard', component: AdminDashboardComponent, canActivate: [adminAuthGuard] },
```

The `adminGuestGuard` on `/admin/login` already handles the case where an admin token exists in `localStorage` — it automatically redirects to `/admin/dashboard` — so no additional logic was needed.

**Navigation flow:**
- `http://localhost:4200/admin` → redirects to `/admin/login`
- Already logged in admin → `adminGuestGuard` redirects to `/admin/dashboard`
- After successful login → `/admin/dashboard` (protected by `adminAuthGuard`)

---

### 2 — Swagger UI Not Sending HTTP Requests (3 Bugs)

**Files**:
- `StudentAssessmentTrackerAPI/Program.cs` *(modified)*
- `StudentAssessmentTrackerAPI/Presentation/Swagger/SwaggerAuthOperationFilter.cs` *(new)*

**Problem:** Opening `http://localhost:5000/swagger` shows the API documentation but the Execute button either doesn't appear or produces no HTTP request. All CRUD operations (GET, POST, PUT, DELETE) were broken from Swagger UI.

#### Bug 1 — Swagger Middleware After Auth Middleware (Critical)

**Root Cause:** `UseSwagger()` and `UseSwaggerUI()` were registered in the pipeline *after* `UseAuthentication()` and `UseAuthorization()`. ASP.NET Core processes middlewares in registration order. Requests to `/swagger/**` entered the auth middleware, got matched against the `MapFallbackToFile("index.html")` SPA catch-all endpoint, and the request context was corrupted before Swagger's own middleware could handle them.

**Fix:** Moved `UseSwagger()` and `UseSwaggerUI()` to before the auth middlewares:

```csharp
// Pipeline order in Program.cs — AFTER fix:
app.UseRouting();
app.UseCors("AllowAngular");
app.UseSwagger(...);      // ← Swagger BEFORE auth
app.UseSwaggerUI(...);    // ← Swagger UI BEFORE auth
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");
```

#### Bug 2 — Global `AddSecurityRequirement` Locking All Endpoints

**Root Cause:** `options.AddSecurityRequirement(...)` was called inside `AddSwaggerGen`, which applies the Bearer token requirement to *every single operation* in the spec — including public endpoints such as `/api/admins/login`, `/api/teachers/login`, and `/api/students/login`. These endpoints appeared locked with a padlock icon in Swagger UI, creating a chicken-and-egg problem: you need to call login to get a token, but login appeared to require a token.

**Fix:** Removed the global `AddSecurityRequirement` call and replaced it with a custom `IOperationFilter` that scans for `[Authorize]` attributes and only applies the Bearer requirement per-operation:

```csharp
// AddSwaggerGen — REMOVED:
options.AddSecurityRequirement(new OpenApiSecurityRequirement { ... });

// AddSwaggerGen — ADDED:
options.OperationFilter<SwaggerAuthOperationFilter>();
```

New file `Presentation/Swagger/SwaggerAuthOperationFilter.cs`:
```csharp
public class SwaggerAuthOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorize = context.MethodInfo.GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>().Any()
            || (context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>().Any() ?? false);

        if (!hasAuthorize) return;  // Public endpoints — no lock icon

        operation.Security = new List<OpenApiSecurityRequirement> { ... };
    }
}
```

**Result:** Public login endpoints show no padlock; protected endpoints show a padlock. Users can now call login first, copy the token, and then authorise.

#### Bug 3 — `EnableTryItOutByDefault()` Not Set

**Root Cause:** Swagger UI renders all operations in read-only view by default. Each operation requires the user to click a "Try it out" button before the **Execute** button and request body fields appear. Without this setting, users who had never used Swagger before saw no Execute button and reported that "Swagger is not sending requests."

**Fix:** Added two options to `UseSwaggerUI()`:

```csharp
app.UseSwaggerUI(options =>
{
    // ... existing options ...
    options.EnableTryItOutByDefault();                 // Execute button visible immediately
    options.ConfigObject.PersistAuthorization = true;  // Bearer token survives page refresh
});
```

---

### 3 — Frontend Not Updated to Match Backend Activation Model

**Files**: 10 frontend files modified (see Files Changed table)

**Problem:** The backend was fully updated to an Admin-only data model (admin creates teachers and students, both self-activate via separate endpoints), but the Angular frontend still reflected the old self-registration flow. Specifically:

- The teacher signup form was a full 7-field registration form that called `POST /api/teachers` directly — which now returns 401 (Admin role required).
- `student-api.service.ts` called `POST /api/students/{id}/teachers` without a `teacherId`, but the backend route had changed to `POST /api/students/{id}/teachers/{teacherId}` — all assign/unassign calls returned 404.
- The admin dashboard had no UI for creating teachers or students, and no way to assign teachers to students.
- All teacher-facing links pointed to `/register`, which no longer mapped to a valid route.

**Root Cause:** Backend changes were implemented correctly and the build passed, but the corresponding frontend layers (models, HTTP services, business services, components, routes) were not updated in the same session.

**Fixes applied:**

**`core/models/teacher.model.ts`**
- Removed `password` from `CreateTeacherDto` (admin creates passwordless accounts)
- Added `TeacherActivateDto { email, password, confirmPassword }`

**`core/services/http/teacher-api.service.ts`**
- Added `activate(dto: TeacherActivateDto)` → `POST /api/teachers/activate`

**`features/teachers/services/teacher-business.service.ts`**
- Added `activate(dto)` — validates, calls API, stores JWT + teacher session
- Removed old auto-login `switchMap` from `register()` (no password at creation time)

**`components/signup-form.component.ts`** — transformed from registration to activation:
- Heading: "Register as Teacher" → "Activate Your Teacher Account"
- Profile fields (ID, name, phone, subject) wrapped in `*ngIf="isEdit"` — not shown during activation
- `onSubmit()`: calls `teacherBusiness.activate()` when `!isEdit`, `teacherBusiness.register()` for profile edits
- Button: "Register" → "Activate Account"

**`core/services/http/student-api.service.ts`** — critical URL bug fix:
```typescript
// Before (404):
assignTeacher(studentId: number)  →  POST /api/students/{id}/teachers
// After (correct):
assignTeacher(studentId: number, teacherId: number)  →  POST /api/students/{id}/teachers/{teacherId}
// Same for unassignTeacher
```

**`core/services/http/admin-api.service.ts`** — added missing methods:
- `createTeacher(dto)` → `POST /api/teachers`
- `createStudent(dto)` → `POST /api/students`
- `assignStudentToTeacher(studentId, teacherId)` → `POST /api/students/{sid}/teachers/{tid}`
- `unassignStudentFromTeacher(studentId, teacherId)` → `DELETE /api/students/{sid}/teachers/{tid}`
- `getSubjects()` → `GET /api/subjects`
- `getGrades()` → `GET /api/grades`

**`components/admin-dashboard.component.ts`** — new UI:
- Teachers tab: "+ New Teacher" toggle → inline form with all required fields, subjects loaded on demand; table shows Pending/Active status badge
- Students tab: "+ New Student" toggle → inline form with grade dropdown; table has "Assigned Teachers" column with teacher chips, unassign (×) button, and "+ Assign" inline expand row with teacher dropdown

**`app.routes.ts`**: `/register` → `/activate` + `{ path: 'register', redirectTo: 'activate' }` for backward compatibility

**`login-form.component.ts`**: both `/register` links → `/activate`, button text updated

**`app.ts`**: `navigateToSignUp()` → `/activate`

---

### 4 — Build Error TS2339: `password` Property Missing on `CreateTeacherDto`

**Files**: `StudentApp/src/app/features/teachers/services/teacher-business.service.ts`

**Problem:** After removing `password` from `CreateTeacherDto` in item 3, the Angular build failed with two TypeScript errors:

```
TS2339: Property 'password' does not exist on type 'CreateTeacherDto'.
  teacher-business.service.ts:115  — isValidPassword(teacherData.password)
  teacher-business.service.ts:127  — password: teacherData.password
```

**Root Cause:** The `register()` method in `TeacherBusinessService` contained two legacy code paths inherited from the old self-registration flow:
1. A `isValidPassword(teacherData.password)` guard — redundant because teachers no longer set a password at creation time.
2. A `switchMap(() => this.teacherApi.login({ ..., password: teacherData.password }))` auto-login call — impossible since no password exists on the new account.

Both references compiled cleanly before the DTO change but became type errors once `password` was removed from the interface.

**Fix:** Simplified `register()` to remove both dead code paths. The method now creates the teacher account and stores the returned object — no password validation, no auto-login:

```typescript
register(teacherData: CreateTeacherDto): Observable<Teacher> {
  if (!this.isValidEmail(teacherData.email)) { ... }

  this.teacherState.setLoading(true);

  return this.teacherApi.create(teacherData).pipe(
    map(teacher => {
      this.teacherState.setCurrentTeacher(teacher);
      this.teacherState.setLoading(false);
      return teacher;
    }),
    catchError(...)
  );
}
```

Also removed unused `switchMap` from the RxJS imports.

**Result:** `npx tsc --noEmit` exits clean; Angular build succeeds.

---

## Files Changed

| File | Change Type | Description |
|------|-------------|-------------|
| `StudentApp/src/app/app.routes.ts` | Modified | Added `/admin` → `/admin/login` redirect; renamed `/register` → `/activate` with backward-compat redirect |
| `StudentAssessmentTrackerAPI/Program.cs` | Modified | Moved Swagger before auth middleware; replaced global security requirement with filter; added `EnableTryItOutByDefault` and `PersistAuthorization` |
| `StudentAssessmentTrackerAPI/Presentation/Swagger/SwaggerAuthOperationFilter.cs` | New | `IOperationFilter` that applies Bearer token requirement only to `[Authorize]`-decorated endpoints |
| `StudentApp/src/app/core/models/teacher.model.ts` | Modified | Removed `password` from `CreateTeacherDto`; added `TeacherActivateDto { email, password, confirmPassword }` |
| `StudentApp/src/app/core/services/http/teacher-api.service.ts` | Modified | Added `activate(dto)` → `POST /api/teachers/activate` |
| `StudentApp/src/app/features/teachers/services/teacher-business.service.ts` | Modified | Added `activate()` method; removed dead `password` references from `register()` |
| `StudentApp/src/app/components/signup-form.component.ts` | Modified | Transformed from teacher registration form to activation form; profile fields hidden behind `*ngIf="isEdit"` |
| `StudentApp/src/app/core/services/http/student-api.service.ts` | Modified | Fixed `assignTeacher`/`unassignTeacher` URL signatures to include `teacherId` |
| `StudentApp/src/app/core/services/http/admin-api.service.ts` | Modified | Added `createTeacher`, `createStudent`, `assignStudentToTeacher`, `unassignStudentFromTeacher`, `getSubjects`, `getGrades` |
| `StudentApp/src/app/components/admin-dashboard.component.ts` | Modified | Added Create Teacher panel, Create Student panel, teacher assignment column with assign/unassign UI |
| `StudentApp/src/app/components/login-form.component.ts` | Modified | Updated `/register` links to `/activate`; updated button/link text |
| `StudentApp/src/app/app.ts` | Modified | `navigateToSignUp()` navigates to `/activate` |

---

## Testing Checklist

### Admin Route
- [ ] Navigate to `http://localhost:4200/admin` → should redirect to `/admin/login`
- [ ] Login with `admin@school.com` / `Admin@123` → should land on `/admin/dashboard`
- [ ] Navigate to `http://localhost:4200/admin` while already logged in as admin → should go directly to `/admin/dashboard`
- [ ] Logout → should be redirected back to `/admin/login`

### Swagger CRUD
- [ ] Open `http://localhost:5000/swagger` → Execute button visible on all operations without clicking "Try it out"
- [ ] Call `POST /api/admins/login` with `{ "email": "admin@school.com", "password": "Admin@123" }` → token returned, no auth required
- [ ] Click Authorize button → paste token → click Authorize
- [ ] Call `GET /api/admins/teachers` → 200 OK with teacher list
- [ ] Call `GET /api/admins/students` → 200 OK with student list
- [ ] Call `DELETE /api/admins/teachers/{id}` → teacher deleted
- [ ] Refresh Swagger UI page → Bearer token still set (PersistAuthorization)
- [ ] Public endpoints (`/api/teachers/login`, `/api/students/activate`) have no padlock icon

### Teacher Activation Flow
- [ ] Navigate to `http://localhost:4200/activate` → shows "Activate Your Teacher Account" with email + password fields only
- [ ] Navigate to old `http://localhost:4200/register` → redirects to `/activate`
- [ ] Admin creates teacher in dashboard → teacher receives email, navigates to `/activate`, submits email + password → logged in
- [ ] Navigating to `/activate` while already logged in as teacher → `guestGuard` redirects to `/`

### Admin Dashboard — Create Teacher
- [ ] Click "+ New Teacher" → inline form appears with fields: ID/Passport, First Name, Last Name, Email, Phone, Subject
- [ ] Submit form → teacher account created, row appears in table with "Pending Activation" status badge
- [ ] Submit with missing fields → validation error shown

### Admin Dashboard — Create Student
- [ ] Click "+ New Student" → inline form appears with fields: ID/Passport, First Name, Last Name, Email, Phone, Grade
- [ ] Submit form → student account created, row appears in table with generated Unique ID
- [ ] Submit with missing fields → validation error shown

### Admin Dashboard — Teacher Assignment
- [ ] Student row shows "None" when no teachers assigned
- [ ] Click "+ Assign" → inline row expands with teacher dropdown
- [ ] Select teacher, click Assign → teacher chip appears on student row
- [ ] Click × on a chip → teacher unassigned, chip removed

### Student–Teacher API URLs
- [ ] `POST /api/students/{studentId}/teachers/{teacherId}` → 200 OK (old `/{studentId}/teachers` must return 404/405)
- [ ] `DELETE /api/students/{studentId}/teachers/{teacherId}` → 200 OK

---

## Prevention Notes

1. **Angular routing:** Always add a bare-path redirect (`admin → admin/login`) alongside every login/dashboard route pair. The wildcard `**` catch-all swallows all unmatched paths silently — no 404 is shown to the developer.

2. **Swagger + ASP.NET Core middleware order:** Swagger middleware must always be registered *before* `UseAuthentication()` and `UseAuthorization()`. The SPA fallback `MapFallbackToFile` will match `/swagger/**` if middleware order is wrong.

3. **Swagger auth:** Never use `AddSecurityRequirement` globally. Use an `IOperationFilter` implementation so public endpoints (login, activate, public lookups) remain unlocked and directly callable without a pre-existing token.

4. **Swagger UX:** Always call `EnableTryItOutByDefault()` on any project so developers can execute requests immediately without hunting for a "Try it out" toggle.

5. **Model–service alignment:** When a field is removed from a DTO, search for every reference in service methods — not just the call sites. Business logic methods (validation, auto-login flows) may retain references that the compiler won't catch until build time.

6. **API URL contracts:** When the backend changes a route signature (e.g., adding a path parameter), always update every frontend service method that calls that URL. A mismatch returns 404/405 with no compile-time warning.

---

## Documentation Updated

- `docs/QUICK_FIX_REFERENCE.md` — Added issues **#49** (admin route) and **#50** (Swagger CRUD), updated issue count to 50, added new diagnostic checklist section, updated footer date.
- `docs/ERROR_FIXES_COMPLETE.md` — Added April 17, 2026 session with full root-cause analysis and code examples for both fixes.
