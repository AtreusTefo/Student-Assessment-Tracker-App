# Student Assessment Tracker — Error Fixes & Session Log (March 5, 2026)

## Overview
This document records all issues encountered and resolved during the March 5, 2026 development session, covering issues \#26 through \#37. Includes root cause analysis, code examples, files changed, and prevention tips.

**Session Date**: March 5, 2026  
**Tech Stack**: ASP.NET Core 8 + Angular 17+ (Standalone Components) + DataTables.net-dt + EF Core + SQL Server

---

## Table of Contents
1. [Issue #26: Missing StudentUniqueId and IdPassportNo Fields](#issue-26-missing-studentuniqueid-and-idpassportno-fields)
2. [Issue #27: TypeScript Double-Comma Syntax Error (TS1136)](#issue-27-typescript-double-comma-syntax-error-ts1136)
3. [Issue #28: ID/Passport Validation Length Inconsistency](#issue-28-idpassport-validation-length-inconsistency)
4. [Issue #29: Login & Signup Form — 9 UX/Validation Problems](#issue-29-login--signup-form--9-uxvalidation-problems)
5. [Issue #30: `loadTeacherById` Missing from TeacherBusinessService](#issue-30-loadteacherbyid-missing-from-teacherbusinessservice)
6. [Issue #31: Navbar Always Shows Login/Sign Up After Authentication](#issue-31-navbar-always-shows-loginsign-up-after-authentication)
7. [Issue #32: DataTables Action Buttons Not Working After Re-render](#issue-32-datatables-action-buttons-not-working-after-re-render)
8. [Issue #33: "Welcome, undefined undefined" After Login](#issue-33-welcome-undefined-undefined-after-login)
9. [Issue #34: Delete Button in Student List Does Nothing](#issue-34-delete-button-in-student-list-does-nothing)
10. [Issue #35: Student Form Inputs Stay Editable During Submission](#issue-35-student-form-inputs-stay-editable-during-submission)
11. [Issue #36: Server-Side Field Errors Never Clear on Student Form](#issue-36-server-side-field-errors-never-clear-on-student-form)
12. [Issue #37: No Route Protection — Unauthenticated Users Can Access All Pages](#issue-37-no-route-protection--unauthenticated-users-can-access-all-pages)

---

## Issue #26: Missing StudentUniqueId and IdPassportNo Fields

### Problem
The Student entity had no system-generated unique identifier and no field for storing a student's national ID or passport number. Teachers had no way to record official identification documents, and students had no stable human-readable reference code beyond the internal database integer ID.

### Root Cause
These fields were not part of the original schema design and needed to be added across all architecture layers.

### Solution Implemented

**Backend — `Domain/Entities/Student.cs`:**
```csharp
public string StudentUniqueId { get; set; } = string.Empty; // e.g. STU-A4X9B2KL
public string IdPassportNo { get; set; } = string.Empty;
```

**Backend — `Infrastructure/Data/ApplicationDbContext.cs`:**
```csharp
entity.Property(s => s.StudentUniqueId).IsRequired().HasMaxLength(20);
entity.Property(s => s.IdPassportNo).IsRequired().HasMaxLength(20);
entity.HasIndex(s => s.StudentUniqueId).IsUnique();
```

**Backend — `Application/DTOs/StudentDto.cs`:**
- `StudentDto`: added `StudentUniqueId` + `IdPassportNo`
- `CreateStudentDto`: added `IdPassportNo` only (unique ID is system-generated)
- `UpdateStudentDto`: added `IdPassportNo` only

**Backend — `Application/Validators/StudentValidator.cs`:**
```csharp
RuleFor(s => s.IdPassportNo)
    .NotEmpty().WithMessage("ID/Passport No. is required.")
    .Length(9).WithMessage("ID/Passport No. must be exactly 9 characters.")
    .Matches(@"^[a-zA-Z0-9\-]+$").WithMessage("ID/Passport No. can only contain letters, numbers, and hyphens.");
```

**Backend — `Application/Mappings/MappingProfile.cs`:**
```csharp
// CreateStudentDto → Student
.ForMember(dest => dest.StudentUniqueId, opt => opt.Ignore())

// UpdateStudentDto → Student
.ForMember(dest => dest.StudentUniqueId, opt => opt.Ignore())
```

**Backend — `Application/Services/StudentService.cs`:**
```csharp
public async Task<StudentDto> CreateStudentAsync(CreateStudentDto createDto)
{
    var student = _mapper.Map<Student>(createDto);
    student.StudentUniqueId = GenerateStudentUniqueId(); // ← generated here
    // ...
}

private static string GenerateStudentUniqueId()
{
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    var random = new Random();
    var suffix = new string(Enumerable.Repeat(chars, 8)
        .Select(s => s[random.Next(s.Length)]).ToArray());
    return $"STU-{suffix}";
}
```

**EF Migration**: `20260304125258_AddStudentUniqueIdAndPassportNo` — adds both columns and unique index.

**Frontend — `core/models/student.model.ts`:**
```typescript
// Added to Student, StudentDetailDto:
studentUniqueId: string;

// Added to Student, StudentDetailDto, CreateStudentDto, UpdateStudentDto:
idPassportNo: string;
```

**Frontend — `components/student-form.component.ts`:**
```html
<input [(ngModel)]="student.idPassportNo" name="idPassportNo"
       minlength="9" maxlength="9"
       pattern="^[a-zA-Z0-9\-]+$" required />
```

**Frontend — `components/student-detail.component.ts`:**
```html
<tr><td>Student ID</td><td><span class="unique-id">{{ student.studentUniqueId }}</span></td></tr>
<tr><td>ID/Passport No.</td><td>{{ student.idPassportNo }}</td></tr>
```

### Files Changed
- `Domain/Entities/Student.cs`
- `Infrastructure/Data/ApplicationDbContext.cs`
- `Application/DTOs/StudentDto.cs`
- `Application/Validators/StudentValidator.cs`
- `Application/Mappings/MappingProfile.cs`
- `Application/Services/StudentService.cs`
- New: `Migrations/20260304125258_AddStudentUniqueIdAndPassportNo.cs`
- `core/models/student.model.ts`
- `components/student-form.component.ts`
- `components/student-detail.component.ts`

### Prevention Tips
- When adding new entity fields, always follow this checklist: Entity → DbContext config → DTOs → Validators → Mapping → Service → Migration → Angular models → Components.
- Use `Ignore()` in AutoMapper for server-generated fields so client inputs can never overwrite them.
- Always add migrations after changing the entity schema: `dotnet ef migrations add <Name>` then `dotnet ef database update`.

---

## Issue #27: TypeScript Double-Comma Syntax Error (TS1136)

### Problem
Angular build failed with compiler error `TS1136: Property assignment expected` inside `student-form.component.ts`.

### Root Cause
A trailing comma was left after a property assignment that was already followed by another comma — creating a double comma `,,` which is a syntax error in TypeScript object literals.

```typescript
// BROKEN ❌
firstName: student.firstName || '',  ,  // ← double comma
```

### Solution Implemented
Removed the extra comma:

```typescript
// FIXED ✅
firstName: student.firstName || '',
```

### Files Changed
- `components/student-form.component.ts`

### Prevention Tips
- Enable ESLint with `@typescript-eslint` — it catches trailing commas and syntax errors before build.
- Use Prettier auto-formatting — it removes extra commas on save.
- Read compiler error messages carefully: `TS1136` always means a syntax error in an object literal or similar structure.

---

## Issue #28: ID/Passport Validation Length Inconsistency

### Problem
The `CreateStudentValidator` correctly enforced exactly 9 characters (`Length(9)`) for `IdPassportNo`, but `UpdateStudentValidator` only enforced a maximum of 20 characters (`MaximumLength(20)`). The Angular form also only set `maxlength="20"`, not a minimum. This meant a student could be created with a 9-character ID but updated to have a shorter (invalid) one.

### Root Cause
The `UpdateStudentValidator` was written separately from the `CreateStudentValidator` and the rules were not kept in sync.

### Solution Implemented

**Backend — `UpdateStudentValidator` in `Application/Validators/StudentValidator.cs`:**
```csharp
// BEFORE ❌
RuleFor(s => s.IdPassportNo)
    .NotEmpty()
    .MaximumLength(20);

// AFTER ✅
RuleFor(s => s.IdPassportNo)
    .NotEmpty().WithMessage("ID/Passport No. is required.")
    .Length(9).WithMessage("ID/Passport No. must be exactly 9 characters.")
    .Matches(@"^[a-zA-Z0-9\-]+$").WithMessage("ID/Passport No. can only contain letters, numbers, and hyphens.");
```

**Frontend — `components/student-form.component.ts`:**
```html
<!-- BEFORE ❌ -->
<input maxlength="20" />

<!-- AFTER ✅ -->
<input minlength="9" maxlength="9" />
```

```typescript
// Error display
// BEFORE: only checked hasError('maxlength')
// AFTER:
hasError('minlength') || hasError('maxlength')
```

### Files Changed
- `Application/Validators/StudentValidator.cs`
- `components/student-form.component.ts`

### Prevention Tips
- Keep `CreateValidator` and `UpdateValidator` rules in sync. Consider extracting shared rules into a base method or static helper.
- When changing a validation rule, always update both the Create and Update validators in the same commit.
- Test both the create AND update flows after any validation change.

---

## Issue #29: Login & Signup Form — 9 UX/Validation Problems

### Problem
Both the login and signup forms lacked proper template-driven validation, UX affordances, and correct navigation. Specifically:
1. No `NgForm` / `#ref="ngModel"` — no form-level valid/dirty tracking
2. No email format validation on login
3. No minimum password length enforcement
4. No confirm password field on signup
5. No password show/hide toggle on either form
6. Input fields stayed enabled while loading (user could submit twice)
7. Submit button did not guard against invalid form state
8. `clearError()` not called on input change — stale server errors persisted
9. Cancel buttons navigated to wrong routes (`/` instead of `/register` and `/login`)

### Root Cause
The forms were initial prototypes without full validation wiring. Issues accumulated over multiple implementation iterations.

### Solution Implemented

**`components/login-form.component.ts` changes:**
```typescript
import { NgForm } from '@angular/forms';

// In template:
// <form #loginForm="ngForm" (ngSubmit)="onSubmit(loginForm)">
// <input #email="ngModel" name="email" type="email" email required>
// <input #password="ngModel" name="password" minlength="6" required>
// <button [disabled]="loading || loginForm.invalid">Login</button>
// <input ... [disabled]="loading" (input)="clearError()">

showPassword = false;

clearError(): void {
  this.teacherBusiness.clearError();
}

onSubmit(form: NgForm): void {
  if (form.invalid) return;
  // ... proceed with login
}
```

**`components/signup-form.component.ts` changes:**
```typescript
// isEdit detection (from route param):
ngOnInit(): void {
  const id = this.route.snapshot.paramMap.get('id');
  this.isEdit = !!id;
  // ...
}

// Confirm password validation:
confirmPassword = '';
showPassword = false;
showConfirmPassword = false;

get passwordMismatch(): boolean {
  return this.teacher.password !== this.confirmPassword && this.confirmPassword.length > 0;
}

// Cancel navigates to /login (not /)
onCancel(): void {
  this.router.navigate(['/login']);
}

// Email disabled when editing:
// <input [disabled]="loading || isEdit" />
// <small *ngIf="isEdit">Email cannot be changed</small>
```

### Files Changed
- `components/login-form.component.ts`
- `components/signup-form.component.ts`

### Prevention Tips
- Always import `NgForm` and use `#formRef="ngForm"` from the start on every form.
- Add `email` validator to all email inputs, `minlength` to all password inputs.
- Add confirm password for any registration form.
- Set `[disabled]="loading"` on all inputs during async operations.
- Call `clearError()` in `(input)` handler of every field that might show server errors.

---

## Issue #30: `loadTeacherById` Missing from TeacherBusinessService

### Problem
When navigating to the signup form in edit mode (`/register/:id`), the component called `this.teacherBusiness.loadTeacherById(id)` which did not exist, causing a runtime error: `this.teacherBusiness.loadTeacherById is not a function`.

### Root Cause
The `TeacherBusinessService` had `loadTeachers()` and `createTeacher()` and `updateTeacher()` methods but the method to load a single teacher by ID for the edit form was never implemented.

### Solution Implemented

**`features/teachers/services/teacher-business.service.ts`:**
```typescript
loadTeacherById(id: number): void {
  this.stateService.setLoading(true);
  this.stateService.setError(null);

  this.teacherApi.getById(id).subscribe({
    next: (teacher) => {
      this.stateService.setCurrentTeacher(teacher);
      this.stateService.setLoading(false);
    },
    error: (err) => {
      this.stateService.setError(err.message || 'Failed to load teacher');
      this.stateService.setLoading(false);
    }
  });
}
```

### Files Changed
- `features/teachers/services/teacher-business.service.ts`

### Prevention Tips
- When implementing a CRUD feature, always implement all four operations (list, get by ID, create, update) in the business service at the same time.
- Consider defining a `ITeacherBusinessService` interface that lists all required methods — this catches missing implementations at compile time.

---

## Issue #31: Navbar Always Shows Login/Sign Up After Authentication

### Problem
After logging in successfully, the navbar still showed "Login" and "Sign Up" buttons. The user's name appeared correctly in the route-rendered component but the nav bar never updated. There was also no Logout button.

### Root Cause
The root `App` component (`app.ts`) never subscribed to `TeacherStateService.isAuthenticated$`. The `isAuthenticated` property on the component never changed from its initial `false` value, so `*ngIf="isAuthenticated"` always evaluated as false and the authenticated nav block never rendered.

### Solution Implemented

**`app.ts`:**
```typescript
export class App implements OnInit, OnDestroy {
  isAuthenticated = false;
  teacherName = '';
  private destroy$ = new Subject<void>();

  constructor(
    private teacherBusiness: TeacherBusinessService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.teacherBusiness.isAuthenticated$
      .pipe(takeUntil(this.destroy$))
      .subscribe(auth => this.isAuthenticated = auth);

    this.teacherBusiness.currentTeacher$
      .pipe(takeUntil(this.destroy$))
      .subscribe(teacher => {
        this.teacherName = teacher
          ? `${teacher.firstName} ${teacher.lastName}`
          : '';
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  logout(): void {
    this.teacherBusiness.logout();
    this.router.navigate(['/login']);
  }
}
```

**`app.html`:**
```html
<!-- Unauthenticated block -->
<div *ngIf="!isAuthenticated">
  <a routerLink="/login">Login</a>
  <a routerLink="/register">Sign Up</a>
</div>

<!-- Authenticated block -->
<div *ngIf="isAuthenticated">
  <span class="nav-greeting">Welcome, {{ teacherName }}</span>
  <a routerLink="/students">View Students</a>
  <a routerLink="/students/new">Add Student</a>
  <button class="logout-btn" (click)="logout()">Logout</button>
</div>
```

### Files Changed
- `app.ts`
- `app.html`
- `app.scss`

### Prevention Tips
- The root component must subscribe to all global state it uses in templates. Don't rely on child components to update the parent's template variables.
- Always implement `OnDestroy` and unsubscribe (use `takeUntil(destroy$)`) to prevent memory leaks.
- Test auth-sensitive nav items immediately after implementing login — it's easy to forget to wire up the component subscription.

---

## Issue #32: DataTables Action Buttons Not Working After Re-render

### Problem
In the Student List, the View, Edit, and Delete action buttons (rendered inside DataTables rows) appeared correctly on initial load. However, after performing any sort, search, or pagination action, clicking the buttons did nothing — no navigation occurred and no delete was triggered.

### Root Cause
DataTables re-renders the table body (replaces DOM nodes) on every sort, filter, or page event. Angular's `(click)` event bindings are attached to specific DOM element instances. When DataTables replaces those elements, the bindings are gone. Clicking the new buttons has no Angular handler attached.

### Solution Implemented
Replaced direct `(click)` bindings with **event delegation** using `data-*` attributes. A single listener on the persistent `<table>` element detects clicks anywhere in the table.

**`components/student-list.component.ts`:**
```typescript
import { NgZone } from '@angular/core';

// In template — buttons use data attributes instead of (click):
// <button data-action="view" [attr.data-id]="student.id">View</button>
// <button data-action="edit" [attr.data-id]="student.id">Edit</button>
// <button data-action="delete" [attr.data-id]="student.id">Delete</button>

private tableElement: HTMLElement | null = null;
private onTableClick = (event: Event) => {
  const btn = (event.target as HTMLElement).closest('[data-action]') as HTMLElement;
  if (!btn) return;
  const action = btn.getAttribute('data-action');
  const id = Number(btn.getAttribute('data-id'));
  if (!id) return;

  this.ngZone.run(() => {
    if (action === 'view') this.viewStudent(id);
    else if (action === 'edit') this.editStudent(id);
    else if (action === 'delete') this.deleteStudent(id);
  });
};

private attachActionListeners(): void {
  if (this.tableElement) {
    this.tableElement.removeEventListener('click', this.onTableClick);
  }
  this.tableElement = document.querySelector('#studentsTable') as HTMLElement;
  if (this.tableElement) {
    this.tableElement.addEventListener('click', this.onTableClick);
  }
}

// In DataTables config:
dtOptions = {
  // ... other options
  drawCallback: () => {
    this.attachActionListeners();
  }
};

// In ngOnDestroy:
ngOnDestroy(): void {
  if (this.tableElement) {
    this.tableElement.removeEventListener('click', this.onTableClick);
  }
}
```

### Files Changed
- `components/student-list.component.ts`

### Prevention Tips
- **Never use `(click)` bindings on elements inside DataTables rows.** They will break on re-render.
- Always use event delegation (`data-action` + `data-id` + table-level listener) for any dynamic table library.
- Always wrap delegated event handlers in `NgZone.run()` — without it, Angular's change detection does not trigger and the UI won't update.
- Remove event listeners in `ngOnDestroy` to prevent memory leaks.
- Re-attach listeners in `drawCallback` so they survive every DataTables re-render.

---

## Issue #33: "Welcome, undefined undefined" After Login

### Problem
After a successful login, the navbar displayed "Welcome, undefined undefined" instead of the teacher's actual name. The JWT token was stored correctly and the user was considered authenticated, but the teacher object's name properties were all `undefined`.

### Root Cause
The login API returns a **nested** JSON response:
```json
{
  "token": "eyJhbGci...",
  "teacher": {
    "teacherId": 3,
    "firstName": "Jane",
    "lastName": "Smith",
    "email": "jane@school.edu",
    ...
  }
}
```

The original Angular login code in `TeacherBusinessService` called `tap(response => this.state.setCurrentTeacher(response))`. Here `response` is the full `{ token, teacher }` object — it has no `firstName` property, so the template read `undefined`.

Additionally, the API returns `teacherId` while the Angular `Teacher` interface uses `id`, so the ID was also `undefined`.

### Solution Implemented

**New interface — `core/models/teacher.model.ts`:**
```typescript
export interface TeacherLoginResponse {
  token: string;
  teacher: {
    teacherId: number;
    firstName: string;
    lastName: string;
    email: string;
    phone: string;
    subject: string;
    enrollmentDate: string;
    createdDate: string;
  };
}
```

**Updated HTTP service — `core/services/http/teacher-api.service.ts`:**
```typescript
// BEFORE ❌
login(credentials: TeacherLoginDto): Observable<Teacher> {

// AFTER ✅
login(credentials: TeacherLoginDto): Observable<TeacherLoginResponse> {
```

**Updated business service — `features/teachers/services/teacher-business.service.ts`:**
```typescript
// BEFORE ❌
this.teacherApi.login(credentials).pipe(
  tap(response => {
    localStorage.setItem('token', response.token); // worked
    this.state.setCurrentTeacher(response as any); // WRONG — response is not a Teacher
  })
)

// AFTER ✅
this.teacherApi.login(credentials).pipe(
  tap((response: TeacherLoginResponse) => {
    localStorage.setItem('token', response.token);
    const teacher: Teacher = {
      ...response.teacher,
      id: response.teacher.teacherId  // map teacherId → id
    };
    this.state.setCurrentTeacher(teacher);
  })
)
```

### Files Changed
- `core/models/teacher.model.ts`
- `core/services/http/teacher-api.service.ts`
- `features/teachers/services/teacher-business.service.ts`

### Prevention Tips
- **Always inspect the actual API response in DevTools Network tab** before writing the Angular-side mapping. It takes 30 seconds and prevents this class of bug entirely.
- Define an explicit response interface (like `TeacherLoginResponse`) for any endpoint that returns a shape different from the base entity type.
- Never cast API responses with `as any` — use a proper typed interface.
- When API property names differ from frontend property names (e.g., `teacherId` vs `id`), always remap explicitly in the business service, not in the template.

---

## Issue #34: Delete Button in Student List Does Nothing

### Problem
Clicking the Delete button in the Student List had no visible effect. No confirmation modal appeared, no error was shown, and no delete was triggered. The failure was completely silent — nothing logged to the console either.

### Root Cause
Three separate bugs combined to produce the failure:

**Bug 1 (Critical):** The click listener for all action buttons is attached via raw `addEventListener` inside DataTables' `drawCallback`, which runs **outside Angular's zone**. While `ngZone.run()` re-enters the zone for routing and deletion calls, it does **not** force a synchronous flush of the current view. `showDeleteConfirm()` set `showConfirmDialog = true` but Angular never re-evaluated the `*ngIf="showConfirmDialog"` binding — the modal was never rendered.

**Bug 2 (Minor):** `confirmDelete()` (`next` and `error` paths) and `cancelDelete()` both set `showConfirmDialog = false` without calling `cdr.detectChanges()`. The modal could fail to dismiss cleanly from async Observable callbacks.

**Bug 3 (Minor):** `loadStudents()` was called **twice** in `ngOnInit()` — a stale duplicate left from an earlier refactor — causing two redundant API requests on every page load.

### Solution Implemented

**`components/student-list.component.ts`:**
```typescript
// BEFORE ❌
showDeleteConfirm(id: number): void {
  this.studentToDelete = id;
  this.showConfirmDialog = true;
  // Angular never re-renders — modal stays hidden
}

// AFTER ✅
showDeleteConfirm(id: number): void {
  this.studentToDelete = id;
  this.showConfirmDialog = true;
  this.cdr.detectChanges(); // Force Angular to render the modal immediately
}

// BEFORE ❌
confirmDelete(): void {
  this.studentBusiness.deleteStudent(this.studentToDelete!).subscribe({
    next: () => { this.showConfirmDialog = false; this.studentToDelete = null; },
    error: () => { this.showConfirmDialog = false; this.studentToDelete = null; }
  });
}
cancelDelete(): void {
  this.showConfirmDialog = false;
  this.studentToDelete = null;
}

// AFTER ✅
confirmDelete(): void {
  this.studentBusiness.deleteStudent(this.studentToDelete!).subscribe({
    next: () => {
      this.showConfirmDialog = false;
      this.studentToDelete = null;
      this.cdr.detectChanges();
    },
    error: () => {
      this.showConfirmDialog = false;
      this.studentToDelete = null;
      this.cdr.detectChanges();
    }
  });
}
cancelDelete(): void {
  this.showConfirmDialog = false;
  this.studentToDelete = null;
  this.cdr.detectChanges();
}

// Removed duplicate loadStudents() call in ngOnInit (was called twice)
```

### Files Changed
- `components/student-list.component.ts`

### Prevention Tips
- Any state change triggered from a raw DOM event listener (outside Angular zone) **must** be followed by `cdr.detectChanges()` to flush the view — even inside `ngZone.run()`.
- Always call `cdr.detectChanges()` after setting flags that control modal visibility (`*ngIf`) when the trigger comes from outside Angular's zone.
- When async Observable callbacks (`.subscribe({ next, error })`) modify component state, use `cdr.detectChanges()` to guarantee the DOM is updated.
- Search for duplicate method calls after any refactor — tools like ESLint cannot detect semantic duplicates.

---

## Issue #35: Student Form Inputs Stay Editable During Submission

### Problem
When the Create or Edit Student form was submitted, the Submit button was correctly disabled (showing "Saving...") but all nine input fields remained fully editable. A user could change values mid-flight, creating a race condition where the displayed data differed from what was actually saved. The Cancel link also remained active, allowing accidental navigation away from a pending request.

### Root Cause
Only the Submit button had `[disabled]="loading"`. The `loading` property was already correctly tied to the state service and updated — it was simply never applied to the individual input elements or the Cancel anchor.

### Solution Implemented

**`components/student-form.component.ts`:**
```html
<!-- BEFORE ❌ — only button was disabled -->
<input type="text" id="firstName" [(ngModel)]="student.firstName" ... />
<button type="submit" [disabled]="loading">{{ loading ? 'Saving...' : 'Create' }}</button>
<a routerLink="/" class="btn btn-secondary">Cancel</a>

<!-- AFTER ✅ — all inputs + Cancel are locked during loading -->
<!-- Applied to all 9 inputs: idPassportNo, firstName, lastName,
     email, phone, grade, assessment1, assessment2, assessment3 -->
<input type="text" id="firstName" [(ngModel)]="student.firstName" ... [disabled]="loading" />
<button type="submit" [disabled]="loading">{{ loading ? 'Saving...' : 'Create' }}</button>
<a [routerLink]="loading ? null : '/'" class="btn btn-secondary" [class.disabled]="loading">Cancel</a>
```

CSS added:
```scss
.btn-secondary.disabled {
  background-color: #bdbdbd;
  cursor: not-allowed;
  pointer-events: none;
}
.form-group input:disabled {
  background-color: #f5f5f5;
  color: #9e9e9e;
  cursor: not-allowed;
  border-color: #e0e0e0;
}
```

### Files Changed
- `components/student-form.component.ts`

### Prevention Tips
- When adding `[disabled]="loading"` to a submit button, **always apply it to every input in the same form at the same time** — treat the entire form as a single lockable unit.
- For `<a>` anchor Cancel buttons: `[routerLink]="loading ? null : '/path'"` suppresses navigation (null disables routerLink); combine with `[class.disabled]="loading"` for visual feedback.
- Consider a form-level `<fieldset [disabled]="loading">` wrapper as a single-point alternative to disabling each input individually.

---

## Issue #36: Server-Side Field Errors Never Clear on Student Form

### Problem
When the server returned a 400 validation error (e.g., duplicate ID/Passport No.), field-level error messages appeared correctly inline. However, they **never disappeared** — even after the user cleared the offending field and typed a new valid value. The stale error persisted until the next form submission. The top-level red banner had the same problem.

### Root Cause
`fieldErrors` (populated from server `ProblemDetails.errors`) was only reset to `{}` at the start of `onSubmit()`. There was no `(input)` handler on any field to reactively clear individual errors as the user typed corrections.

### Solution Implemented

**`components/student-form.component.ts`:**
```typescript
// NEW METHOD ✅
clearFieldError(field: string): void {
  if (this.fieldErrors[field]) {
    delete this.fieldErrors[field];
  }
  // Auto-clear banner when no field errors remain
  if (Object.keys(this.fieldErrors).length === 0) {
    this.error = null;
    this.isServerError = false;
  }
}
```

```html
<!-- BEFORE ❌ — no clearing on input -->
<input type="text" id="firstName" [(ngModel)]="student.firstName" [disabled]="loading" />

<!-- AFTER ✅ — clears the field's server error on each keystroke -->
<input type="text" id="firstName" [(ngModel)]="student.firstName" [disabled]="loading"
  (input)="clearFieldError('firstName')" />

<!-- Phone: chain with existing validatePhone() call -->
<input type="text" id="phone" ... (input)="validatePhone(); clearFieldError('phone')" />
```

Applied to all 9 inputs: `idPassportNo`, `firstName`, `lastName`, `email`, `phone`, `grade`, `assessment1`, `assessment2`, `assessment3`.

### Files Changed
- `components/student-form.component.ts`

### Prevention Tips
- Every input that can display a **server-side** error must have a `(input)` handler that clears that specific error — otherwise the user has no feedback that their correction was accepted.
- Delete only the targeted key from `fieldErrors`, not all errors — other fields' messages remain visible until they are individually corrected.
- The server error banner should auto-clear when `fieldErrors` becomes empty to prevent a stale red box persisting after all inline errors are resolved.
- Chain multiple `(input)` handlers with semicolons: `(input)="validatePhone(); clearFieldError('phone')"`.

---

## Issue #37: No Route Protection — Unauthenticated Users Can Access All Pages

### Problem
All application routes (Student List, Create Student, Edit Student, Student Detail) were publicly accessible without authentication. Any user who knew the URL could navigate directly to protected pages without logging in. Additionally, authenticated teachers could still navigate back to `/login` and `/register`, and page refresh logged users out (in-memory state was lost on reload).

### Root Cause
1. No Angular route guards existed — `app.routes.ts` had no `canActivate` on any route.
2. `TeacherStateService` stored auth state only in a `BehaviorSubject` (in-memory). A page refresh reset it to `false` regardless of prior login.
3. No `guestGuard` prevented authenticated users from revisiting the login/register pages.

### Solution Implemented

**New file — `core/guards/auth.guard.ts`:**
```typescript
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TeacherStateService } from '../services/state';

export const authGuard: CanActivateFn = () => {
  const teacherState = inject(TeacherStateService);
  const router = inject(Router);
  return teacherState.isAuthenticated()
    ? true
    : router.createUrlTree(['/login']);
};
```

**New file — `core/guards/guest.guard.ts`:**
```typescript
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TeacherStateService } from '../services/state';

export const guestGuard: CanActivateFn = () => {
  const teacherState = inject(TeacherStateService);
  const router = inject(Router);
  return !teacherState.isAuthenticated()
    ? true
    : router.createUrlTree(['/']);
};
```

**Updated `app.routes.ts`:**
```typescript
export const routes: Routes = [
  // Protected — must be logged in
  { path: '', component: StudentListComponent, canActivate: [authGuard] },
  { path: 'create', component: StudentFormComponent, canActivate: [authGuard] },
  { path: 'edit/:id', component: StudentFormComponent, canActivate: [authGuard] },
  { path: 'detail/:id', component: StudentDetailComponent, canActivate: [authGuard] },
  // Guest-only — redirect home if already authenticated
  { path: 'login', component: LoginFormComponent, canActivate: [guestGuard] },
  { path: 'register', component: SignUpFormComponent, canActivate: [guestGuard] },
  { path: '**', redirectTo: '' }
];
```

**Updated `core/services/state/teacher-state.service.ts` — localStorage persistence:**
```typescript
const AUTH_STORAGE_KEY = 'sat_current_teacher';

// Restore session on service init:
private restoredTeacher = (() => {
  try { return JSON.parse(localStorage.getItem(AUTH_STORAGE_KEY) ?? 'null'); }
  catch { return null; }
})();

// BehaviorSubjects initialised with restored value:
private currentTeacherSubject = new BehaviorSubject<Teacher | null>(this.restoredTeacher);
private isAuthenticatedSubject = new BehaviorSubject<boolean>(!!this.restoredTeacher);

// setCurrentTeacher — write/remove localStorage:
setCurrentTeacher(teacher: Teacher | null): void {
  teacher
    ? localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(teacher))
    : localStorage.removeItem(AUTH_STORAGE_KEY);
  this.currentTeacherSubject.next(teacher);
  this.isAuthenticatedSubject.next(!!teacher);
  this.clearError();
}
// logout() and clearState() also call localStorage.removeItem()
```

**Updated `components/signup-form.component.ts`:**
```typescript
// BEFORE ❌ — registration auto-authenticates but sent to /login
// which guestGuard redirects away from → double-redirect
next: () => this.router.navigate(['/login'])

// AFTER ✅ — go directly to protected home
next: () => this.router.navigate(['/'])
```

### Files Changed
- New: `core/guards/auth.guard.ts`
- New: `core/guards/guest.guard.ts`
- `app.routes.ts`
- `core/services/state/teacher-state.service.ts`
- `components/signup-form.component.ts`

### Prevention Tips
- **Apply route guards at project setup time**, not as an afterthought — add `canActivate: [authGuard]` to every protected route the moment it is created.
- Always persist auth state to `localStorage` so page refresh preserves the session.
- Add a `guestGuard` to login/register routes to prevent double-login confusion.
- When a registration flow auto-authenticates the user (calls `setCurrentTeacher`), navigate to the protected home route — not back to `/login`.
- Use functional guards (`CanActivateFn` + `inject()`) — the modern Angular pattern, no `providers` registration required.

---

## Summary Table

| # | Issue | Severity | Files Changed | Time to Fix |
|---|-------|----------|---------------|-------------|
| 26 | Missing StudentUniqueId + IdPassportNo | Medium (Feature) | 10 | ~2 hours |
| 27 | TypeScript double-comma TS1136 | Low (Build Error) | 1 | <5 min |
| 28 | ID/Passport validation length inconsistency | Medium | 2 | 10 min |
| 29 | Login/Signup form — 9 UX problems | Medium | 2 | ~1 hour |
| 30 | loadTeacherById missing | Medium (Runtime Error) | 1 | 15 min |
| 31 | Navbar not reactive to auth state | High | 3 | 20 min |
| 32 | DataTables action buttons broken after re-render | High | 1 | 30 min |
| 33 | "Welcome, undefined undefined" after login | High | 3 | 20 min |
| 34 | Delete button does nothing (modal never appears) | High | 1 | 15 min |
| 35 | Form inputs stay editable during submission | Medium | 1 | 10 min |
| 36 | Server field errors never clear on keystroke | Medium | 1 | 10 min |
| 37 | No route protection — all routes publicly accessible | Critical | 5 | 30 min |

**Total Issues**: 12  
**Feature Additions**: 1 (#26)  
**Build Errors**: 1 (#27)  
**Validation Bugs**: 2 (#28, #36)  
**UX Improvements**: 2 (#29, #35)  
**Runtime/Logic Errors**: 4 (#30, #31, #32, #34)  
**Data Mapping Bugs**: 1 (#33)  
**Security/Auth Bugs**: 1 (#37)

---

## Key Patterns Established

### Pattern 1: Event Delegation for Dynamic Tables
Any table rendered by DataTables, ag-Grid, or similar library that replaces DOM on interaction:
```
1. Add data-action="view|edit|delete" + [attr.data-id]="item.id" to buttons
2. Attach ONE click listener to the table element (not rows)
3. Use event.target.closest('[data-action]') to find the triggered button
4. Wrap handler in NgZone.run() for change detection
5. Re-attach in drawCallback/afterRender callback
6. Remove listener in ngOnDestroy
```

### Pattern 2: Nested API Response Mapping
When an API returns `{ token, entity: { entityId, ... } }`:
```
1. Define a dedicated response interface (XxxLoginResponse)
2. Update HTTP service return type to that interface
3. In business service tap(): extract entity from response.entity
4. Remap any mismatched property names (entityId → id)
5. Pass the correctly-shaped object to state service
```

### Pattern 3: ChangeDetectorRef.detectChanges() for Out-of-Zone State Changes
When component state is mutated from a raw DOM event listener (e.g., DataTables `drawCallback`, third-party library callbacks), `ngZone.run()` alone is insufficient to flush conditional directives like `*ngIf`:
```
1. Inject ChangeDetectorRef
2. After setting any flag that controls *ngIf visibility, call this.cdr.detectChanges()
3. Also call detectChanges() in all code paths that reset visibility flags
   (both .subscribe next AND error, and in cancel handlers)
```

### Pattern 4: Form Lockdown During Async Submission
```
1. Apply [disabled]="loading" to every <input> in the form
2. For <a> Cancel anchors: [routerLink]="loading ? null : '/path'" + [class.disabled]="loading"
3. Style disabled state clearly: muted background, not-allowed cursor
4. Alternatively, wrap all inputs in <fieldset [disabled]="loading">
```

### Pattern 5: Per-Field Server Error Clearing
```
1. Store server errors in fieldErrors: Record<string, string[]>
2. Implement clearFieldError(field: string): delete fieldErrors[field]
3. Auto-clear server banner when fieldErrors becomes empty
4. Add (input)="clearFieldError('fieldName')" to every input with a fieldErrors display
5. Chain with existing (input) handlers using semicolons
```

### Pattern 6: Route Guards + Session Persistence
```
1. Create authGuard (CanActivateFn): isAuthenticated() ? true : createUrlTree(['/login'])
2. Create guestGuard (CanActivateFn): !isAuthenticated() ? true : createUrlTree(['/'])
3. Apply authGuard to all protected routes at the time they are created
4. Apply guestGuard to /login and /register
5. Persist Teacher to localStorage in setCurrentTeacher(); restore on service init
6. Remove from localStorage in logout() and clearState()
7. After registration (which auto-authenticates), navigate to '/' not '/login'
```

---

**Last Updated**: March 5, 2026  
**Issues Covered**: #26 through #37 (12 total)  
**Status**: All resolved
