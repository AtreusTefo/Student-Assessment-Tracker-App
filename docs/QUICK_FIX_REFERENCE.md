# Quick Fix Reference Guide

## 50 Major Issues Fixed & How to Fix Them Again

---

## 🚨 CRITICAL ISSUES (March 2, 2026)

### 14. App Won't Start - DI Resolution Failure (EXIT CODE 1)
**Problem**: Application crashes immediately on startup  
**Error**: `Unable to resolve service for type... while attempting to activate...`  
**Fix**: Remove legacy code that's injecting unregistered dependencies
```powershell
# Delete legacy folders
cd StudentAssessmentTrackerAPI
Remove-Item Controllers, Models, Data, Validators, Mappings -Recurse -Force
dotnet build
dotnet run  # Should start successfully
```
**Root Cause**: Legacy controllers injecting `Data.ApplicationDbContext` but `Program.cs` only registers `Infrastructure.Data.ApplicationDbContext`  
**Files**: `StudentAssessmentTrackerAPI/Controllers/*` (delete), `Program.cs` (verify registrations)

---

### 15. Duplicate Class Names Across Namespaces
**Problem**: Multiple classes with same name in different folders  
**Examples**: `ApplicationDbContext`, `MappingProfile`, `StudentValidator`  
**Fix**: Keep only Clean Architecture versions, delete legacy
```powershell
# Check for duplicates
Get-ChildItem -Recurse -Filter *.cs | Select-String "public class ApplicationDbContext"
# Should return ONLY ONE result

# Remove legacy versions
Remove-Item Data, Mappings, Validators -Recurse -Force
```
**Impact**: AutoMapper/FluentValidation may register wrong class  
**Prevention**: Delete old code immediately after creating replacement

---

### 16. Missing Feature in Clean Architecture
**Problem**: Teacher endpoints missing — only existed in legacy code  
**Fix**: Implement across all 4 layers
```
1. Domain/Entities/Teacher.cs         - Entity with business rules
2. Application/DTOs/TeacherDto.cs     - Request/Response DTOs
3. Application/Validators/             - FluentValidation rules
4. Application/Services/               - ITeacherService + implementation
5. Infrastructure/Data/                - DbSet<Teacher> + config
6. Application/Mappings/               - AutoMapper profiles
7. Presentation/Controllers/           - REST API endpoints
8. Program.cs                          - DI registration
```
**Files Created**: 5 new files  
**Files Updated**: 3 existing files  
**Prevention**: Feature parity checklist before deleting legacy code

---

---

## 🚨 CRITICAL ISSUES (March 3, 2026)

### 20. Postman: Collection Shows "Empty" After Import
**Problem**: Postman imports the collection but shows zero requests — "This collection is empty"  
**Root Cause**: `_postman_id` was a human-readable slug (`"student-assessment-api"`) not a valid UUID — Postman silently discards collections with non-UUID IDs  
**Fix**: Use a proper UUID for `_postman_id`
```json
// WRONG ❌
"_postman_id": "student-assessment-api"

// CORRECT ✅
"_postman_id": "a3f2c1d4-b5e6-4789-abcd-ef1234567890"
```
**File**: `docs/StudentAssessmentTracker.postman_collection.json`  
**Prevention**: Always generate a real UUID when creating a Postman collection JSON manually

---

### 21. Postman: Requests Hang Forever at "Sending request..."
**Problem**: Every Postman request spins indefinitely with no response or error  
**Root Cause (two layers)**:
1. URL used `host: ["{{base_url}}"]` — unresolved environment variable silently hangs
2. `localhost` on Windows 11 resolves to IPv6 `::1` first; API only bound to `127.0.0.1` (IPv4) — connection refused, Postman times out silently  

**Fix**: Use hardcoded `127.0.0.1` in all URLs (not `localhost`, not a variable):
```json
// WRONG ❌ — variable as host
"url": {
  "host": ["{{base_url}}"],
  "port": "5000"
}

// CORRECT ✅ — hardcoded IPv4
"url": {
  "protocol": "http",
  "host": ["127","0","0","1"],
  "port": "5000",
  "path": ["api","students"]
}
```
Also bind the API to IPv4 only in `launchSettings.json`:
```json
"applicationUrl": "http://127.0.0.1:5000"
```
**Files**: `docs/StudentAssessmentTracker.postman_collection.json`, `StudentAssessmentTrackerAPI/Properties/launchSettings.json`  
**Prevention**: Never use `localhost` in Postman on Windows — always use `127.0.0.1`

---

### 22. Postman: DELETE Returns No Body ("204 No Content")
**Problem**: DELETE `/api/students/{id}` returns HTTP 204 with no response body — no confirmation visible in Postman  
**Root Cause**: Controller used `return NoContent()` — HTTP 204 is spec-defined as having no body; Postman was behaving correctly  
**Fix**: Return 200 OK with a message body instead
```csharp
// WRONG ❌
return NoContent();

// CORRECT ✅
return Ok(new { message = $"Student with ID {id} successfully deleted" });
```
Also update `[ProducesResponseType]`:
```csharp
// Before
[ProducesResponseType(StatusCodes.Status204NoContent)]

// After
[ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
```
**File**: `StudentAssessmentTrackerAPI/Presentation/Controllers/StudentsController.cs`  
**Prevention**: Use 200 OK + message body for DELETE if consumers need confirmation

---

### 23. Angular Frontend: "Unexpected token '<', <!doctype... is not valid JSON"
**Problem**: All Angular pages crash immediately — every API call returns HTML instead of JSON  
**Root Cause (two bugs)**:
1. `proxy.conf.json` targeted `https://localhost:5001` (non-existent; API is on `http://127.0.0.1:5000`)
2. `angular.json` had **no `proxyConfig` entry** — the proxy file was never loaded; Angular served its own `index.html` for every `/api/...` request  

**Fix**:
```json
// proxy.conf.json — corrected target
{
  "/api": {
    "target": "http://127.0.0.1:5000",
    "secure": false,
    "changeOrigin": true,
    "logLevel": "debug"
  }
}
```
```json
// angular.json — add the missing proxyConfig entry
"serve": {
  "configurations": {
    "development": {
      "proxyConfig": "proxy.conf.json"
    }
  }
}
```
**Files**: `StudentApp/proxy.conf.json`, `StudentApp/angular.json`  
**Prevention**: Verify `proxyConfig` is in `angular.json` and test proxy with a direct browser request to `http://localhost:4200/api/students`

---

### 24. EF Core In-Memory Database: Data Lost on Restart
**Problem**: All data vanishes every time the API restarts — In-Memory provider stores data in RAM only  
**Fix**: Migrate to SQL Server LocalDB
```powershell
# 1. Install EF CLI tools (once per machine)
dotnet tool install --global dotnet-ef --version 8.0.0

# 2. Update .csproj — remove InMemory, add SqlServer
# Remove:  <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
# Add:
#   <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
#   <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0">
#     <PrivateAssets>all</PrivateAssets>
#   </PackageReference>

# 3. Restore packages
dotnet restore

# 4. Add connection string to appsettings.Development.json
# "ConnectionStrings": {
#   "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=StudentAssessmentTrackerDev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
# }

# 5. Update Program.cs: UseSqlServer instead of UseInMemory
# builder.Services.AddDbContext<ApplicationDbContext>(options =>
#     options.UseSqlServer(connectionString,
#         o => o.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

# 6. Create and apply migration
dotnet ef migrations add InitialCreate --project StudentAssessmentTrackerAPI --output-dir Infrastructure/Data/Migrations
dotnet ef database update --project StudentAssessmentTrackerAPI
```
**Database Created**: `StudentAssessmentTrackerDev` on `(localdb)\mssqllocaldb`  
**Files Changed**: `.csproj`, `appsettings.json`, `appsettings.Development.json`, `ApplicationDbContext.cs`, `Program.cs`  
**Prevention**: Never use `UseInMemoryDatabase` outside of unit tests

---

### 25. API Won't Start: "address already in use" on Port 5000
**Problem**: `dotnet run` fails immediately with:
```
System.IO.IOException: Failed to bind to address http://127.0.0.1:5000: address already in use
```
**Root Cause**: A previous `dotnet run` process from an earlier session is still running in a background terminal  
**Fix**:
```powershell
# Kill all dotnet processes immediately
Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -Force

# Wait for OS TIME_WAIT to clear (2 seconds is usually enough)
Start-Sleep -Seconds 2

# Restart
dotnet run

# --- OR: identify the specific PID first ---
netstat -ano | findstr :5000
# Find PID in last column, then:
Stop-Process -Id <PID> -Force
```
**Prevention**: Always stop `dotnet run` with `Ctrl+C` before closing a terminal; use `Get-Process dotnet` to check for orphaned processes

---

## 🔧 BUILD/RUNTIME ERRORS

### 17. Method Signature Mismatch - DeleteAsync
**Problem**: Build error - wrong parameter type  
**Error**: `cannot convert from 'Teacher' to 'int'`  
**Fix**: Pass ID, not entity object
```csharp
// WRONG ❌
await _repository.DeleteAsync(teacher);

// CORRECT ✅
await _repository.DeleteAsync(id);
```
**File**: `Application/Services/TeacherService.cs`  
**Prevention**: Check interface signature (F12) before implementing

---

## 📝 GIT ISSUES

### 18. Rename/Delete Merge Conflict
**Problem**: Git conflict - file renamed locally but deleted on remote  
**Error**: `CONFLICT (rename/delete): # Code Citations.md`  
**Fix**: Choose to keep or delete
```bash
# Option 1: Keep file at new location
git add "docs/# Code Citations.md"
git commit --no-edit

# Option 2: Accept deletion
git rm "docs/# Code Citations.md"
git commit -m "Accept remote deletion"
```
**Prevention**: Pull before major reorganizations, communicate with team

---

### 19. Git Rebase Stuck in Interactive Editor
**Problem**: Terminal stuck in alternate buffer after `git rebase --continue`  
**Fix**: Abort and use merge strategy instead
```bash
# From fresh terminal
git rebase --abort

# Use merge instead of rebase
git pull --no-rebase --no-edit origin main
```
**Prevention**: Set non-interactive editor
```bash
git config --global core.editor "code --wait"
# or for automation
$env:GIT_EDITOR = "true"
```

---

## 🔹 FRONTEND ISSUES (Original 13)

### 1️⃣ Student List Table Shows Wrong Columns
**Problem**: Table displays columns that don't exist in the DTO  
**Fix**: Match template properties to `StudentListDto` fields
```typescript
// Correct: StudentId, FirstName, LastName only
<td>{{ student.studentId }}</td>
<td>{{ student.firstName }}</td>
<td>{{ student.lastName }}</td>
```
**Files**: `student-list.component.ts`, `student.service.ts`, `StudentListDto.cs`

---

### 2️⃣ API Returns Wrong Data Type
**Problem**: Service expects `Student[]` but API returns `StudentListDto[]`  
**Fix**: Update service method return type
```typescript
// WRONG ❌
getStudents(): Observable<Student[]>

// CORRECT ✅
getStudents(): Observable<StudentListDto[]>
```
**File**: `student.service.ts`

---

### 3️⃣ "Loading..." Message Takes Too Long
**Problem**: 500ms+ delay before data appears  
**Fix**: Remove artificial delay in form submit
```typescript
// REMOVE THIS:
setTimeout(() => {
  this.router.navigate(['/']);
}, 300);

// JUST DO THIS:
this.router.navigate(['/']);
```
**File**: `student-form.component.ts` → `onSubmit()` method

---

### 4️⃣ Student Details Page Blank (Data Loaded But Not Displayed)
**Problem**: Component receives data but template is empty  
**Fix**: Add Angular change detection
```typescript
import { ChangeDetectorRef } from '@angular/core';

constructor(private cdr: ChangeDetectorRef) { }

// After assigning data:
this.student = data;
this.cdr.markForCheck();  // Force view update
```
**File**: `student-detail.component.ts`

---

### 5️⃣ Student List Stuck on "Loading..." After Creating Student
**Problem**: After create redirect, list shows "Loading..." forever (data loads but view doesn't update)  
**Fix**: Add Angular change detection to StudentListComponent
```typescript
import { ChangeDetectorRef } from '@angular/core';

constructor(
  private studentService: StudentService,
  private router: Router,
  private cdr: ChangeDetectorRef
) { }

loadStudents(): void {
  this.loading = true;
  this.studentService.getStudents().subscribe({
    next: (data) => {
      this.students = data;
      this.loading = false;
      this.cdr.markForCheck();  // Force view update
    },
    error: (err) => {
      this.error = err.message;
      this.loading = false;
      this.cdr.markForCheck();  // Force view update
    }
  });
}
```
**File**: `student-list.component.ts`

---

### 6️⃣ Edit Form Fields Empty When Loading Student
**Problem**: Click Edit button, form shows but all input fields are blank  
**Fix**: Add Angular change detection to StudentFormComponent
```typescript
import { ChangeDetectorRef } from '@angular/core';

constructor(
  private route: ActivatedRoute,
  private router: Router,
  private studentService: StudentService,
  private cdr: ChangeDetectorRef
) { }

loadStudent(id: number): void {
  this.loading = true;
  this.studentService.getStudent(id).subscribe({
    next: (data) => {
      this.student = { /* assign data */ };
      this.loading = false;
      this.cdr.markForCheck();  // Force view update
    },
    error: (err) => {
      this.error = err.message;
      this.loading = false;
      this.cdr.markForCheck();  // Force view update
    }
  });
}
```
**File**: `student-form.component.ts`

---

### 7️⃣ Phone Field Shows Duplicate Validation Error Messages
**Problem**: Error message appears twice - once in field, once at top of form  
**Fix**: Only set global error for server errors, not frontend validations
```typescript
onSubmit(): void {
  this.error = null; // Clear at start
  
  // Format validations: Set global error (shown in error div)
  if (this.student.firstName && !this.isValidName(this.student.firstName)) {
    this.error = 'Please enter a valid First Name (letters only)';
    return;
  }

  // Field validations: DON'T set global error (shown in template)
  if (!this.student.phone || this.student.phone.length !== 8) {
    return; // Template will show error inline
  }

  // Phone is valid, continue with API call...
}
```

**Template (Phone Field):**
```html
<ng-container *ngIf="form.submitted">
  <span class="error" *ngIf="!student.phone">Phone is required</span>
  <span class="error" *ngIf="student.phone && student.phone.length < 8">Phone must be exactly 8 digits</span>
</ng-container>
```

**Why**: 
- Field-level errors display inline (where user is looking)
- Global error div for unexpected server/API errors only
- Prevents duplicate messages

**File**: `student-form.component.ts`, `StudentValidator.cs`

---

### 8ï¸âƒ£ Top-of-Form Validation Error Banner for Empty Assessments
**Problem**: Submitting empty assessments shows model binding errors at the top (duplicates inline errors)  
**Fix**: Suppress global banner for validation responses and validate assessments before submit
```typescript
private isValidationErrorResponse(err: any): boolean {
  return err?.status === 400 && !!err?.error?.errors;
}

private handleServerError(action: 'create' | 'update', err: any): void {
  this.loading = false;
  if (this.isValidationErrorResponse(err)) {
    this.isServerError = false;
    this.error = null;
    this.cdr.markForCheck();
    return;
  }
  this.isServerError = true;
  this.error = `Failed to ${action} student: ` + (err.error?.title || err.message);
  this.cdr.markForCheck();
}
```
**File**: `student-form.component.ts`

---

### 9️⃣ Native Confirm Dialog Not Working in VS Code Simple Browser
**Problem**: Delete button shows browser confirm popup that doesn't respond in Simple Browser  
**Fix**: Replace JavaScript `confirm()` with custom Angular modal dialog
```typescript
// Template: Add modal overlay
<div *ngIf="showConfirmDialog" class="modal-overlay">
  <div class="modal">
    <div class="modal-header"><h3>Confirm Delete</h3></div>
    <div class="modal-body">
      <p>Are you sure you want to delete this student? This action cannot be undone.</p>
    </div>
    <div class="modal-footer">
      <button (click)="confirmDelete()" class="btn btn-danger">Delete</button>
      <button (click)="cancelDelete()" class="btn btn-secondary">Cancel</button>
    </div>
  </div>
</div>

// Component: Add modal logic
showConfirmDialog = false;
studentToDelete: number | null = null;

showDeleteConfirm(id: number): void {
  this.studentToDelete = id;
  this.showConfirmDialog = true;
}

confirmDelete(): void {
  if (this.studentToDelete !== null) {
    this.studentService.deleteStudent(this.studentToDelete).subscribe({
      next: () => {
        this.showConfirmDialog = false;
        this.studentToDelete = null;
        this.loadStudents();
      }
    });
  }
}

cancelDelete(): void {
  this.showConfirmDialog = false;
  this.studentToDelete = null;
}
```
**File**: `student-list.component.ts`

---

### 🔟 Missing HTML5 Autocomplete Attributes
**Problem**: Form fields don't suggest browser autofill, reducing usability  
**Fix**: Add semantic `autocomplete` attributes to inputs
```html
<!-- Login Form -->
<input type="email" autocomplete="email" />
<input type="password" autocomplete="current-password" />

<!-- Registration Form -->
<input type="text" autocomplete="given-name" placeholder="First Name" />
<input type="text" autocomplete="family-name" placeholder="Last Name" />
<input type="email" autocomplete="email" />
<input type="tel" autocomplete="tel" placeholder="Phone" />
<input type="text" autocomplete="off" placeholder="Subject" /> <!-- Custom field -->
<input type="password" autocomplete="new-password" />

<!-- Student Form -->
<input type="text" autocomplete="given-name" placeholder="First Name" />
<input type="text" autocomplete="family-name" placeholder="Last Name" />
<input type="email" autocomplete="email" />
<input type="tel" autocomplete="tel" placeholder="Phone" />
<input type="text" autocomplete="off" placeholder="Grade" /> <!-- Custom field -->
```
**Files**: `login-form.component.ts`, `signup-form.component.ts`, `student-form.component.ts`

---

### 1️⃣1️⃣ Duplicate Startup Log Messages in Console
**Problem**: Application startup messages appear twice with same timestamp  
**Fix**: Remove duplicate Serilog console sink configuration
```csharp
// Program.cs - BEFORE (causes duplicate):
builder.Host.UseSerilog((context, logger) => {
  logger.ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console(outputTemplate: "...");  // ❌ DUPLICATE
});

// AFTER (correct):
builder.Host.UseSerilog((context, logger) => {
  logger.ReadFrom.Configuration(context.Configuration);
  // Console output configured ONLY in appsettings.json
});

// Also use static Log facade for startup messages:
Log.Information("Application Started");  // ✅ Uses already-configured Serilog
```
**File**: `Program.cs`

---

### 1️⃣2️⃣ Undefined Student ID in View/Edit/Delete Operations (CRITICAL) ⚡ NEW
**Problem**: Delete, View, Edit buttons fail with `Http failure response for .../students/undefined: 400 Bad Request`  
**Fix**: Synchronize StudentListDto interface to match backend API property names
```typescript
// StudentListDto interface - BEFORE (WRONG):
export interface StudentListDto {
  studentId: number;  // ❌ Backend returns 'id', not 'studentId'
  firstName: string;
  lastName: string;
}

// StudentListDto interface - AFTER (CORRECT):
export interface StudentListDto {
  id: number;         // ✅ Matches backend JSON property name
  firstName: string;
  lastName: string;
}
```

**Update template references:**
```html
<!-- BEFORE: -->
<td>{{ student.studentId }}</td>
<button (click)="viewStudent(student.studentId)">View</button>
<button (click)="editStudent(student.studentId)">Edit</button>
<button (click)="deleteStudent(student.studentId)">Delete</button>

<!-- AFTER: -->
<td>{{ student.id }}</td>
<button (click)="viewStudent(student.id)">View</button>
<button (click)="editStudent(student.id)">Edit</button>
<button (click)="deleteStudent(student.id)">Delete</button>
```

**Add RxJS mapping in service as fallback:**
```typescript
getStudents(): Observable<StudentListDto[]> {
  return this.http.get<StudentListDto[]>(this.apiUrl).pipe(
    map(students => students.map(s => ({
      ...s,
      id: s.id || (s as any).studentId
    })))
  );
}
```

**Root Cause**: Backend returns `{id: 1, ...}` but frontend expected `{studentId: 1, ...}`. TypeScript HttpClient deserializes JSON by exact property name match → creates `id` property, but code referenced `studentId` → undefined resulting in API calls like `/api/students/undefined`.

**Files**: `student.service.ts`, `student-list.component.ts`

---

### 1️⃣3️⃣ Phone Field Shows "856" Instead of Full Number When Editing (NEW) 🔧
**Problem**: Edit Student form displays "856" instead of the full 8-digit phone number when loading student data  
**Fix**: Add defensive check before removing country code prefix from phone
```typescript
// BEFORE (WRONG - removes 5 chars unconditionally):
phone: data.phone ? data.phone.substring(5) : ''

// AFTER (CORRECT - only removes "+267 " if present):
let parsedPhone = '';
if (data.phone) {
  parsedPhone = data.phone.startsWith('+267 ') 
    ? data.phone.substring(5) 
    : data.phone;
}
phone: parsedPhone
```

**Root Cause**: 
- Code assumed phone always came with "+267 " prefix (5 characters)
- API actually returns just the 8-digit number: "72254856"
- `substring(5)` on "72254856" removes "72254" → leaves only "856"
- The phone number ends in "856", so that's what showed up

**Why It Happened**: The display view might add the country code format, but the API response doesn't include it.

**File**: `student-form.component.ts` → `loadStudent()` method

---

## 📋 Diagnostic Checklist

**🚨 App Won't Start (Exit Code 1)?**
1. ✅ Check for DI resolution errors in terminal output
2. ✅ Verify all injected types are registered in `Program.cs`
3. ✅ Remove legacy code that uses unregistered dependencies
4. ✅ Search for duplicate class names: `Get-ChildItem -Recurse -Filter *.cs | Select-String "public class YourClassName"`
5. ✅ Run `dotnet clean` then `dotnet build`

**🔨 Build Errors?**
1. ✅ Read error message for file path and line number
2. ✅ Check method signatures match interface (F12 to navigate)
3. ✅ Verify parameter types are correct (e.g., `int id` not `Teacher entity`)
4. ✅ Check all using statements are correct
5. ✅ Run `dotnet restore` to refresh NuGet packages

**🌿 Git Issues?**
1. ✅ **Merge conflict (rename/delete)**: Choose keep or delete → `git add` → `git commit`
2. ✅ **Stuck rebase**: Open fresh terminal → `git rebase --abort`
3. ✅ **Avoid interactive editors**: Use `git pull --no-rebase --no-edit origin main`
4. ✅ **Configure editor**: `git config --global core.editor "code --wait"`

**📦 Missing Features After Migration?**
1. ✅ List all features in legacy code
2. ✅ Verify each feature exists in Clean Architecture
3. ✅ Create feature across all 4 layers: Domain → Infrastructure → Application → Presentation
4. ✅ Register services in `Program.cs` DI container
5. ✅ Update AutoMapper profiles and validators

**📊 Data not showing?**
   - ✅ Check API response in browser DevTools (Network tab)
   - ✅ Verify TypeScript types match API response
   - ✅ Add `console.log()` to verify data assignment
   - ✅ Call `cdr.markForCheck()` in async callbacks

**🗂️ Table columns showing wrong data?**
   - ✅ Verify template properties exist in DTO
   - ✅ Check service method return type
   - ✅ Rebuild Angular: `npm run build`

**⏳ Loading message never goes away?**
   - ✅ Check backend logs: `dotnet run`
   - ✅ Verify API status code (200, 404, 500?)
   - ✅ Make sure `loading = false` in subscribe error handler
   - ✅ Add `cdr.markForCheck()` in async callbacks
   - ✅ Check browser console for JavaScript errors

**✏️ Create/Edit not working?**
   - ✅ Check form validation (FluentValidation on backend)
   - ✅ Check API response in DevTools Network tab
   - ✅ Look for 400 Bad Request (validation errors)
   - ✅ Show error messages in template: `{{ error }}`

**🔌 Postman collection empty / requests hanging?**
1. ✅ Verify `_postman_id` is a valid UUID (not a slug)
2. ✅ Use `127.0.0.1` instead of `localhost` on Windows (IPv6 issue)
3. ✅ Hardcode URL in collection — don't use `{{base_url}}` variable without an active Postman environment
4. ✅ Confirm API is actually running: `netstat -ano | findstr :5000`

**🗄️ API "address already in use" on startup?**
1. ✅ Kill stale processes: `Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -Force`
2. ✅ Wait 2 seconds for TIME_WAIT: `Start-Sleep -Seconds 2`
3. ✅ Then re-run: `dotnet run`

**💾 Data lost after API restart?**
1. ✅ Replace `UseInMemoryDatabase` with `UseSqlServer` in `Program.cs`
2. ✅ Add connection string to `appsettings.Development.json`
3. ✅ Run `dotnet ef migrations add <Name>` and `dotnet ef database update`
4. ✅ Verify auto-migrate block in `Program.cs` runs on startup

**🔗 Angular shows HTML instead of JSON (parse error)?**
1. ✅ Confirm `proxyConfig` is set in `angular.json` under `serve > configurations > development`
2. ✅ Confirm `proxy.conf.json` target points to `http://127.0.0.1:5000`
3. ✅ Restart Angular with `--proxy-config proxy.conf.json` or via `ng serve`
4. ✅ Test proxy: open `http://localhost:4200/api/students` in browser — should return JSON

---

## 🚨 CRITICAL ISSUES (March 5, 2026)

### 26. StudentUniqueId & IdPassportNo Fields Missing
**Problem**: No auto-generated student reference code; no field for national ID / passport  
**Fix**: Full-stack addition — entity → DbContext → DTOs → validators → mapping → service → migration → Angular models → components
```csharp
// Service — auto-generate before save:
student.StudentUniqueId = GenerateStudentUniqueId(); // "STU-A4X9B2KL"

// Mapping — ignore on input DTOs:
.ForMember(dest => dest.StudentUniqueId, opt => opt.Ignore())
```
```typescript
// Form — exactly 9 chars:
<input minlength="9" maxlength="9" pattern="^[a-zA-Z0-9\-]+$" />
```
**Migration**: `20260304125258_AddStudentUniqueIdAndPassportNo`  
**Files**: `Student.cs`, `ApplicationDbContext.cs`, `StudentDto.cs`, `StudentValidator.cs`, `MappingProfile.cs`, `StudentService.cs`, `student.model.ts`, `student-form.component.ts`, `student-detail.component.ts`

---

### 27. TypeScript Double-Comma Syntax Error (TS1136)
**Problem**: Angular build fails with `TS1136: Property assignment expected`  
**Root Cause**: Extra comma after a property in an object literal — `firstName: value,  ,`  
**Fix**: Remove the duplicate comma
```typescript
// WRONG ❌
firstName: student.firstName || '',  ,

// CORRECT ✅
firstName: student.firstName || '',
```
**File**: `student-form.component.ts`  
**Detection**: ESLint + Prettier catch this automatically

---

### 28. ID/Passport Validation Inconsistency (Create vs Update)
**Problem**: Create validates exactly 9 chars; Update validated max 20 — inconsistent enforcement  
**Fix**: Apply `.Length(9)` in **both** validators and `minlength="9" maxlength="9"` in the form
```csharp
// Both CreateStudentValidator AND UpdateStudentValidator:
RuleFor(s => s.IdPassportNo)
    .NotEmpty()
    .Length(9).WithMessage("ID/Passport No. must be exactly 9 characters.")
    .Matches(@"^[a-zA-Z0-9\-]+$");
```
**Files**: `StudentValidator.cs`, `student-form.component.ts`  
**Prevention**: When editing one validator, always check the other

---

### 29. Login/Signup Form UX Problems (9 Items)
**Problem**: Missing validation, no show/hide password, stale errors, wrong Cancel routes, inputs enabled during loading  
**Quick Fix List**:
1. Add `NgForm` + `#ref="ngModel"` on every input
2. Add `email` attribute to email input
3. Add `minlength="6"` to password inputs
4. Add confirm password field to signup with `passwordMismatch` getter
5. Add `showPassword` bool + toggle button for each password field
6. `[disabled]="loading"` on all inputs and submit button
7. Guard `onSubmit()` with `if (form.invalid) return;`
8. Add `(input)="clearError()"` to every field
9. Cancel on login → `/register`; Cancel on signup → `/login`

**Files**: `login-form.component.ts`, `signup-form.component.ts`

---

### 30. `loadTeacherById` Missing from TeacherBusinessService
**Problem**: Runtime error — `this.teacherBusiness.loadTeacherById is not a function`  
**Fix**: Add the missing method to `TeacherBusinessService`
```typescript
loadTeacherById(id: number): void {
  this.stateService.setLoading(true);
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
**File**: `teacher-business.service.ts`

---

### 31. Navbar Not Reactive to Auth State
**Problem**: After login, navbar still shows Login/Sign Up; no Logout button visible  
**Root Cause**: Root `App` component never subscribed to `isAuthenticated$`  
**Fix**: Subscribe in `ngOnInit`, expose boolean property, use `*ngIf` in template
```typescript
// app.ts
ngOnInit(): void {
  this.teacherBusiness.isAuthenticated$
    .pipe(takeUntil(this.destroy$))
    .subscribe(auth => this.isAuthenticated = auth);
}
```
```html
<!-- app.html -->
<div *ngIf="!isAuthenticated">...</div>  <!-- Login / Sign Up -->
<div *ngIf="isAuthenticated">...</div>   <!-- Welcome + Logout -->
```
**Files**: `app.ts`, `app.html`, `app.scss`

---

### 32. DataTables Action Buttons Dead After Sort/Search/Page ⚡
**Problem**: View / Edit / Delete buttons stop working after any DataTables re-render  
**Root Cause**: DataTables replaces DOM rows, destroying Angular's `(click)` bindings  
**Fix**: Event delegation with `data-action` attributes + `NgZone.run()`
```typescript
// Buttons in template:
// <button data-action="view" [attr.data-id]="student.id">View</button>

private onTableClick = (event: Event) => {
  const btn = (event.target as HTMLElement).closest('[data-action]') as HTMLElement;
  if (!btn) return;
  const action = btn.getAttribute('data-action');
  const id = Number(btn.getAttribute('data-id'));
  this.ngZone.run(() => {
    if (action === 'view') this.viewStudent(id);
    else if (action === 'edit') this.editStudent(id);
    else if (action === 'delete') this.deleteStudent(id);
  });
};

dtOptions = {
  drawCallback: () => { this.attachActionListeners(); }
};

ngOnDestroy(): void {
  this.tableElement?.removeEventListener('click', this.onTableClick);
}
```
**Rule**: NEVER use `(click)` on DataTables rows — always use event delegation  
**File**: `student-list.component.ts`

---

### 33. "Welcome, undefined undefined" After Login 🔑
**Problem**: Teacher name shows as `undefined undefined` after successful login  
**Root Cause (two bugs)**:
1. API returns `{ token, teacher: {...} }` — Angular stored the whole object as `Teacher`, not `response.teacher`
2. API uses `teacherId` but Angular `Teacher` interface expects `id`

**Fix**:
```typescript
// 1. Add response interface (teacher.model.ts):
export interface TeacherLoginResponse {
  token: string;
  teacher: { teacherId: number; firstName: string; lastName: string; ... };
}

// 2. Update HTTP service:
login(...): Observable<TeacherLoginResponse>

// 3. Fix business service tap():
tap((response: TeacherLoginResponse) => {
  localStorage.setItem('token', response.token);
  const teacher: Teacher = {
    ...response.teacher,
    id: response.teacher.teacherId  // remap key
  };
  this.state.setCurrentTeacher(teacher);
})
```
**Files**: `teacher.model.ts`, `teacher-api.service.ts`, `teacher-business.service.ts`  
**Prevention**: Always check actual API response shape in DevTools Network tab before writing mapping code

---

## Common Diagnostics (Updated March 5, 2026)

**🖱️ Buttons inside DataTables not working?**
1. ✅ Replace `(click)` with `data-action` + `data-id` attributes
2. ✅ Add table-level delegated listener in `attachActionListeners()`
3. ✅ Ensure `drawCallback` calls `attachActionListeners()`
4. ✅ Wrap all handler logic in `ngZone.run()`

**🔐 Login works but name shows as `undefined`?**
1. ✅ Open DevTools Network tab and inspect the login response body
2. ✅ If nested (`{ token, teacher: {...} }`): define `XxxLoginResponse` interface
3. ✅ Extract `response.teacher` in the business service `tap()`
4. ✅ Remap any property name differences (e.g., `teacherId` → `id`)

**🔒 Navbar not updating after login/logout?**
1. ✅ Root `App` component must implement `OnInit` and subscribe to `isAuthenticated$`
2. ✅ Use `*ngIf="isAuthenticated"` / `*ngIf="!isAuthenticated"` in navbar
3. ✅ Subscribe in `ngOnInit`, unsubscribe with `takeUntil(destroy$)` in `ngOnDestroy`

**📋 Form validation not working?**
1. ✅ Import `NgForm` from `@angular/forms`
2. ✅ Use `#formRef="ngForm"` on form element
3. ✅ Add `#fieldRef="ngModel"` on each input
4. ✅ Guard `onSubmit(form)` with `if (form.invalid) return;`
5. ✅ Call `clearError()` in every `(input)` handler

---

## Full Documentation

See **`ERROR_FIXES_DOCUMENTATION.md`** in docs folder for:
- Detailed explanations of all 19 original issues
- Complete code examples
- Prevention tips
- Testing checklist
- Key learnings

See **`DAILY_REPORT_2026-03-03.md`** for detailed write-up of issues 20–25 (March 3, 2026).

See **`ERROR_FIXES_SESSION_2026-03-05.md`** for detailed write-up of issues 26–33 (March 5, 2026).

See **`DAILY_REPORT_2026-03-05.md`** for the full session summary (March 5, 2026).

---

## 🚨 CRITICAL ISSUES (April 1, 2026)

### 34. TeachersController PUT & DELETE Were Unauthenticated 🔐
**Problem**: `PUT /api/teachers/{id}` and `DELETE /api/teachers/{id}` had no `[Authorize]` attribute — any anonymous caller could overwrite or delete any teacher record, including changing their password hash.  
**Root Cause**: `[Authorize]` was applied to other controllers but accidentally omitted on the `Update` and `Delete` actions in `TeachersController`.  
**Fix**:
```csharp
// Add [Authorize] to both actions
[Authorize]
[HttpPut("{id:int}")]
public async Task<IActionResult> Update(int id, [FromBody] TeacherUpdateDto dto)
{
    // Extract JWT claim and enforce self-scope
    if (!TryGetTeacherId(out var teacherId))
        return Unauthorized(new { message = "Invalid or missing token." });
    if (id != teacherId)
        return StatusCode(403, new { message = "You may only update your own profile." });
    // ...
}

[Authorize]
[HttpDelete("{id:int}")]
public async Task<IActionResult> Delete(int id)
{
    if (!TryGetTeacherId(out var teacherId))
        return Unauthorized(new { message = "Invalid or missing token." });
    if (id != teacherId)
        return StatusCode(403, new { message = "You may only delete your own account." });
    // ...
}

// Add helper at bottom of controller class
private bool TryGetTeacherId(out int teacherId)
{
    teacherId = 0;
    var value = User.FindFirstValue("teacherId");
    return value != null && int.TryParse(value, out teacherId);
}
```
Also add `using Microsoft.AspNetCore.Authorization;` and `using System.Security.Claims;` at the top of the file.  
**File**: `Presentation/Controllers/TeachersController.cs`  
**Prevention**: Add `[Authorize]` to the controller class level and only apply `[AllowAnonymous]` on public endpoints like `/login` and `/register`

---

### 35. UpdateTeacherAsync Had No Duplicate Email / ID-Passport Check
**Problem**: Creating a teacher correctly rejects duplicate email/ID-Passport, but updating did not. Submitting a value that belongs to another teacher caused EF Core to throw a `DbUpdateException` → HTTP 500 with no useful message.  
**Root Cause**: `CreateTeacherAsync` had the duplicate-detection logic; `UpdateTeacherAsync` did not.  
**Fix**: Add exclude-self overloads to the repository and call them in the service:
```csharp
// ITeacherRepository — add two new method signatures:
Task<bool> ExistsByEmailAsync(string email, int excludeTeacherId = 0);
Task<bool> ExistsByIdPassportNoAsync(string idPassportNo, int excludeTeacherId);

// TeacherRepository — implement both:
public async Task<bool> ExistsByEmailAsync(string email, int excludeTeacherId = 0)
{
    var normalized = email.ToLowerInvariant();
    return await _context.Teachers.AsNoTracking()
        .AnyAsync(t => t.Email.ToLower() == normalized && t.Id != excludeTeacherId);
}

// TeacherService.UpdateTeacherAsync — add before mapping:
if (await _repository.ExistsByEmailAsync(dto.Email, excludeTeacherId: id))
    throw new InvalidOperationException($"A teacher with email '{dto.Email}' is already registered.");
if (await _repository.ExistsByIdPassportNoAsync(dto.IdPassportNo, excludeTeacherId: id))
    throw new InvalidOperationException($"A teacher with ID/Passport No. '{dto.IdPassportNo}' is already registered.");
```
Controller catches `InvalidOperationException` → returns `409 Conflict`.  
**Files**: `Domain/Interfaces/ITeacherRepository.cs`, `Infrastructure/Repositories/TeacherRepository.cs`, `Application/Services/TeacherService.cs`  
**Prevention**: Any service that has create+update paths must apply the same duplicate checks to both, using an `excludeId` parameter on the update path

---

### 36. SubjectId Not Validated Against the Subjects Table
**Problem**: Validator only checked `SubjectId > 0`. A valid integer like `999` that doesn't correspond to any row in the `Subjects` table caused EF to throw an FK violation → HTTP 500.  
**Root Cause**: Same gap that was previously fixed for `GradeId` on students was never applied to `SubjectId` on teachers.  
**Fix**: Inject `IRepository<Subject>` into `TeacherService` and validate before saving:
```csharp
// TeacherService constructor — add IRepository<Subject>
public TeacherService(
    ITeacherRepository repository,
    IRepository<Subject> subjectRepository,
    // ...
)

// CreateTeacherAsync & UpdateTeacherAsync — add before duplicate checks:
if (await _subjectRepository.GetByIdAsync(dto.SubjectId) is null)
    throw new ArgumentException($"Subject with ID {dto.SubjectId} does not exist.");
```
Controller catches `ArgumentException` → returns `400 Bad Request`.  
**Files**: `Application/Services/TeacherService.cs`  
**Note**: The `IRepository<Subject>` registration `AddScoped(typeof(IRepository<>), typeof(Repository<>))` in `Program.cs` already covers this — no new DI registration needed  
**Prevention**: For every FK field on any DTO, add a service-layer existence check that throws a descriptive exception rather than leaking DB errors

---

### 37. Last Teacher Unassign Leaves Orphaned Student
**Problem**: `UnassignStudentFromTeacherAsync` removed the join row without checking if it was the student's final teacher. A student with zero entries in `TeacherStudents` becomes invisible — no teacher can query, update, or delete them.  
**Root Cause**: Repository only removed the row; no minimum-1 constraint was enforced.  
**Fix**: Add a count check before unassigning:
```csharp
// IStudentRepository — add:
Task<int> CountTeacherAssignmentsAsync(int studentId);

// StudentRepository — implement:
public async Task<int> CountTeacherAssignmentsAsync(int studentId)
{
    return await _context.TeacherStudents
        .AsNoTracking()
        .CountAsync(ts => ts.StudentId == studentId);
}

// StudentService.UnassignStudentFromTeacherAsync — add guard:
var assignmentCount = await _repository.CountTeacherAssignmentsAsync(studentId);
if (assignmentCount <= 1)
    throw new InvalidOperationException(
        $"Cannot unassign: student {studentId} would have no teachers remaining. " +
        "Assign another teacher first, or delete the student.");
```
**Files**: `Domain/Interfaces/IStudentRepository.cs`, `Infrastructure/Repositories/StudentRepository.cs`, `Application/Services/StudentService.cs`  
**Prevention**: Any time you remove an element from a required-minimum collection, check the count first and return a descriptive error

---

### 38. StudentUniqueId Collision Not Handled at Application Level
**Problem (1)**: `new Random()` created per call is not thread-safe — concurrent requests can produce identical sequences.  
**Problem (2)**: No pre-check against the DB before saving — on a collision `SaveChangesAsync` throws `DbUpdateException` → HTTP 500.  
**Root Cause**: Initial implementation used a simple one-line generator with no concurrency or uniqueness guarantees.  
**Fix**:
```csharp
// BEFORE (WRONG):
private static string GenerateStudentUniqueId()
{
    var random = new Random();  // ❌ not thread-safe
    var suffix = new string(Enumerable.Repeat(chars, 8)
        .Select(s => s[random.Next(s.Length)]).ToArray());
    return $"STU-{suffix}";
}

// AFTER (CORRECT):
private static string GenerateStudentUniqueId()
{
    // Random.Shared is thread-safe and available since .NET 6
    var suffix = new string(Enumerable.Repeat(chars, 8)
        .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
    return $"STU-{suffix}";
}

// In CreateStudentAsync — retry until the generated ID is unique in the DB:
string uniqueId;
do
{
    uniqueId = GenerateStudentUniqueId();
} while (await _repository.FindByUniqueIdAsync(uniqueId) is not null);
student.StudentUniqueId = uniqueId;
```
**File**: `Application/Services/StudentService.cs`  
**Prevention**: Always use `Random.Shared` instead of `new Random()` in .NET 6+. For any auto-generated unique key, add a DB uniqueness check with a retry loop

---

### 39. DeleteTeacherAsync Called SaveChangesAsync Twice
**Problem**: The service called `await _repository.SaveChangesAsync()` immediately after `await _repository.DeleteAsync(id)`. The base `Repository<T>.DeleteAsync` already calls `SaveChangesAsync` internally, so the second call was a redundant empty DB roundtrip.  
**Root Cause**: Inconsistency between the base repository implementation and the service code — `DeleteAsync` was treated as if it didn't persist like the other methods do.  
**Fix**:
```csharp
// BEFORE (WRONG):
await _repository.DeleteAsync(id);       // ← already saves
await _repository.SaveChangesAsync();    // ← redundant second commit

// AFTER (CORRECT):
await _repository.DeleteAsync(id);       // ← saves, done
```
**File**: `Application/Services/TeacherService.cs`  
**Prevention**: Check whether a repository method calls `SaveChangesAsync` internally before adding an explicit call. Convention: `AddAsync`, `UpdateAsync`, and `DeleteAsync` all call save internally in this codebase

---

### 40. StudentAssessmentService Directly Injected ApplicationDbContext
**Problem**: `StudentAssessmentService` (Application layer) depended directly on `ApplicationDbContext` (Infrastructure layer). This violates Clean Architecture and couples the business logic to EF Core implementation details.  
**Root Cause**: The assessment service was written before the repository pattern was consistently applied; it used `_context.StudentAssessments.Add/Remove/SaveChanges` directly.  
**Fix**: Introduce a proper repository:
```csharp
// 1. Create Domain/Interfaces/IStudentAssessmentRepository.cs:
public interface IStudentAssessmentRepository : IRepository<StudentAssessment>
{
    Task<IEnumerable<StudentAssessment>> GetByStudentIdAsync(int studentId);
    Task<StudentAssessment?> GetByIdForStudentAsync(int studentId, int assessmentId);
}

// 2. Create Infrastructure/Repositories/StudentAssessmentRepository.cs:
public class StudentAssessmentRepository : Repository<StudentAssessment>, IStudentAssessmentRepository
{
    public async Task<IEnumerable<StudentAssessment>> GetByStudentIdAsync(int studentId)
        => await _context.StudentAssessments.AsNoTracking()
            .Where(a => a.StudentId == studentId)
            .OrderBy(a => a.DueDate).ThenBy(a => a.Name).ToListAsync();

    public async Task<StudentAssessment?> GetByIdForStudentAsync(int studentId, int assessmentId)
        => await _context.StudentAssessments
            .FirstOrDefaultAsync(a => a.Id == assessmentId && a.StudentId == studentId);
}

// 3. Rewrite StudentAssessmentService constructor:
public StudentAssessmentService(
    IStudentAssessmentRepository assessmentRepository,
    IStudentRepository studentRepository,   // for ownership check
    IMapper mapper,
    ILogger<StudentAssessmentService> logger)

// 4. Register in Program.cs:
builder.Services.AddScoped<IStudentAssessmentRepository, StudentAssessmentRepository>();
```
Remove `using StudentAssessmentTracker.Infrastructure.Data;` from the service — the Application layer must not reference Infrastructure namespaces.  
**Files**: `Domain/Interfaces/IStudentAssessmentRepository.cs` *(new)*, `Infrastructure/Repositories/StudentAssessmentRepository.cs` *(new)*, `Application/Services/StudentAssessmentService.cs`, `Program.cs`  
**Prevention**: Services in the Application layer must only inject Domain interfaces (`IRepository<T>`, `IXxxRepository`). If you find yourself typing `ApplicationDbContext` in a service file, stop and create a repository method instead

---

### 41. TeacherUpdateDto Allowed Blind Password Overwrite
**Problem**: `TeacherUpdateDto` contained a `Password` field. While `MappingProfile` ignored it on the destination, callers could still submit any value and see it silently dropped — with no feedback and no secure change-password flow.  
**Root Cause**: The original DTO was created by duplicating `TeacherRegisterDto`; the `Password` field was never explicitly removed when the update shape was finalized.  
**Fix**: Remove `Password` from `TeacherUpdateDto`:
```csharp
// BEFORE (WRONG — password field present):
public class TeacherUpdateDto
{
    public string IdPassportNo { get; set; } = string.Empty;
    // ...
    public string Password { get; set; } = string.Empty;  // ❌ REMOVED
    public DateTime EnrollmentDate { get; set; }
}

// AFTER (CORRECT — password field absent):
public class TeacherUpdateDto
{
    public string IdPassportNo { get; set; } = string.Empty;
    // ...
    public DateTime EnrollmentDate { get; set; }
}
```
The `MappingProfile` `TeacherUpdateDto → Teacher` mapping retains `.ForMember(dest => dest.Password, opt => opt.Ignore())` as a defence-in-depth measure.  
**File**: `Application/DTOs/TeacherDto.cs`  
**Prevention**: Password changes require a dedicated endpoint that takes both `currentPassword` and `newPassword` and verifies the current password before hashing and persisting the new one. Never include `Password` on a general-purpose update DTO

---

## Diagnostic Checklist — Updated April 1, 2026

**🔐 Anonymous callers can hit PUT/DELETE?**
1. ✅ Add `[Authorize]` to the action (or the whole controller class)
2. ✅ Add `[AllowAnonymous]` only on public endpoints (login, register)
3. ✅ Add a self-scope check using `TryGetTeacherId()` + `if (id != teacherId) return Forbid()`

**♻️ Update endpoint returns 500 on duplicate data?**
1. ✅ Add an `excludeId` parameter to `ExistsByEmailAsync` / `ExistsByIdPassportNoAsync`
2. ✅ Call both checks in the service before mapping/saving
3. ✅ Catch `InvalidOperationException` in the controller → return `409 Conflict`

**🏷️ FK field returns 500 on invalid ID?**
1. ✅ Add `await _repository.GetByIdAsync(dto.FkId)` check in service
2. ✅ Throw `ArgumentException` when null → controller returns `400 Bad Request`
3. ✅ Apply the same check to both Create and Update paths

**👤 Student becoming invisible/unmanageable?**
1. ✅ Before unassigning a teacher, call `CountTeacherAssignmentsAsync`
2. ✅ If count ≤ 1, throw `InvalidOperationException` → `400 Bad Request`

**🎲 Auto-generated IDs colliding under load?**
1. ✅ Replace `new Random()` with `Random.Shared` (thread-safe)
2. ✅ Wrap generation in a `do { } while (DbAlreadyHasId)` retry loop
3. ✅ Ensure the column has a unique index so the DB also rejects duplicates

**🔁 DB showing double-commit behaviour / extra roundtrips?**
1. ✅ Check if the repository method calls `SaveChangesAsync` internally
2. ✅ Remove explicit `await _repository.SaveChangesAsync()` calls in the service
3. ✅ Convention: `AddAsync`, `UpdateAsync`, `DeleteAsync` all save in this codebase

**🏗️ Service file has `using ...Infrastructure.Data`?**
1. ✅ Create `IXxxRepository` interface in `Domain/Interfaces/`
2. ✅ Create concrete class in `Infrastructure/Repositories/` extending `Repository<T>`
3. ✅ Replace `_context.*` calls with repository method calls
4. ✅ Register `IXxxRepository → XxxRepository` in `Program.cs`
5. ✅ Remove the Infrastructure `using` from the Application-layer service file

**🔑 DTO exposes Password on a non-auth endpoint?**
1. ✅ Remove `Password` from the update DTO entirely
2. ✅ Keep `.ForMember(dest => dest.Password, opt => opt.Ignore())` in MappingProfile
3. ✅ Implement a dedicated `POST /api/teachers/change-password` endpoint that verifies the current password

---

## 🚨 CRITICAL ISSUES (April 2, 2026)

### 42. 401 Unauthorized After Teacher Registration 🔑
**Problem**: `GET /api/students` returned `401 Unauthorized` immediately after a teacher registered a new account.  
**Root Cause**: `teacher-business.service.ts` stored the teacher profile but never obtained a JWT. The token BehaviorSubject remained `null` so the interceptor attached no `Authorization` header.  
**Fix**: Chain a `switchMap` after registration to auto-call `login()` with the same credentials:
```typescript
return this.teacherApi.register(dto).pipe(
  switchMap(newTeacher =>
    this.teacherApi.login({ email: dto.email, password: dto.password })
  ),
  tap(response => {
    this.teacherState.setToken(response.token);
    this.teacherState.setCurrentTeacher(response.teacher);
  })
);
```
**File**: `StudentApp/src/app/features/students/services/teacher-business.service.ts`  
**Prevention**: Registration flows that don't return a JWT must immediately call login. Never leave the token as `null` after creating an authenticated account.

---

### 43. Auth Interceptor Redirect Loop on Login Page 🔄
**Problem**: Wrong credentials on `/login` caused an infinite redirect loop back to `/login`.  
**Root Cause**: The 401 handler had no check for whether a token was actually sent. The login endpoint returns 401 for bad credentials; the handler redirected on every 401 regardless.  
**Fix**: Add `&& token` guard:
```typescript
if (error.status === 401 && token) {  // only fire when a token WAS sent
  teacherState.logout();
  router.navigate(['/login']);
}
```
**File**: `StudentApp/src/app/core/interceptors/auth.interceptor.ts`  
**Prevention**: The 401 redirect is for expired/revoked tokens only. An unauthenticated 401 (no token sent) is a normal credential failure — let the component handle it.

---

### 44. Stale Teacher Session After Token Expiry 👻
**Problem**: After JWT expiry, page refresh restored the teacher profile from `localStorage` but every API call still failed with 401.  
**Root Cause**: Startup restore in `teacher-state.service.ts` only checked for the profile object, not the token.  
**Fix**: Require both profile AND token during startup:
```typescript
private restoredTeacher = (() => {
  try {
    const raw   = localStorage.getItem(TEACHER_AUTH_KEY);
    const token = localStorage.getItem(TOKEN_KEY);
    return raw && token ? JSON.parse(raw) : null; // both required
  } catch { return null; }
})();
```
**File**: `StudentApp/src/app/core/services/state/teacher-state.service.ts`  
**Prevention**: Any state service that restores a session from localStorage must also verify the token exists.

---

### 45. Student JWT Never Persisted After Login/Activate 🎓
**Problem**: After student login, all subsequent API calls returned 401 even though the server returned a token in the response.  
**Root Cause**: `StudentAuthStateService` had no `setToken()` / `getToken()` methods. The business service called `setCurrentStudent(response.student)` but discarded `response.token`.  
**Fix**:
```typescript
// student-auth-state.service.ts:
const STUDENT_TOKEN_KEY = 'sat_student_token';

setToken(token: string): void { localStorage.setItem(STUDENT_TOKEN_KEY, token); }
getToken(): string | null    { return localStorage.getItem(STUDENT_TOKEN_KEY); }
// Also: logout() must removeItem(STUDENT_TOKEN_KEY)

// student-auth-business.service.ts — inside tap():
this.studentAuthState.setToken(response.token);     // add before setCurrentStudent
this.studentAuthState.setCurrentStudent(response.student);
```
**Files**: `StudentApp/src/app/core/services/state/student-auth-state.service.ts`, `StudentApp/src/app/features/students/services/student-auth-business.service.ts`  
**Prevention**: After adding any auth state service, verify `setToken()` is called in the business service `tap()` for every login/activate path.

---

### 46. Auth Interceptor Ignored Student Token 🔍
**Problem**: Student JWT was stored but all student API calls still returned 401 — the interceptor only attached the teacher token.  
**Root Cause**: `auth.interceptor.ts` only read `teacherState.getToken()`. During a student-only session this is `null`, so no `Authorization` header was attached.  
**Fix**: Fall back to student token and make the 401 handler branch-aware:
```typescript
const teacherToken = teacherState.getToken();
const studentToken = studentAuthState.getToken();
const token = teacherToken ?? studentToken;
const isStudentToken = !teacherToken && !!studentToken;

// 401 handler:
if (error.status === 401 && token) {
  if (isStudentToken) {
    studentAuthState.logout();
    router.navigate(['/student/login']);
  } else {
    teacherState.logout();
    studentAuthState.logout();
    router.navigate(['/login']);
  }
}
```
**File**: `StudentApp/src/app/core/interceptors/auth.interceptor.ts`  
**Prevention**: A single shared interceptor must handle all user types. Use a precedence chain and make error redirects aware of which token triggered the 401.

---

### 47. Student Dashboard Showed Misleading "Submitted" Status 📋
**Problem**: Every non-overdue assessment showed a green "Submitted" badge even if no file had been uploaded.  
**Root Cause**: Status column used `*ngIf="!isOverdue"` → "Submitted" with no check of actual submission data. The `isAssigned` and `submissionCount` fields did not exist on the DTO.  
**Fix**: After adding both fields to the backend DTO, replace the binary status with a three-way switch:
```html
<span *ngIf="a.submissionCount > 0" class="status-badge submitted">
  ✓ Submitted ({{ a.submissionCount }})
</span>
<button *ngIf="a.isAssigned && a.submissionCount === 0"
  class="btn-upload" (click)="openUploadModal(a.id)">Submit File</button>
<span *ngIf="!a.isAssigned && a.submissionCount === 0"
  class="status-badge pending">Pending</span>
```
**File**: `StudentApp/src/app/components/student-dashboard.component.ts`  
**Prevention**: Never derive submission state from the due date. Always use `submissionCount` returned by the API.

---

### 48. replace_string_in_file Mismatch Due to XML Doc Comments 📝
**Problem**: File edit tool returned "old string not found" — blocking the automated editing pipeline.  
**Root Cause**: Subagent codebase summaries omit `/// <summary>` XML doc blocks. The edit tool needs an exact literal match including all surrounding lines.  
**Fix**: Read the actual file with `read_file` immediately before each edit:
```
1. read_file(filePath, target line range)
2. Copy the exact literal text including XML doc comments
3. Use that literal text as oldString
4. Batch multiple edits in multi_replace_string_in_file
```
**Prevention**: Never rely on a summary view for file editing context. Always read the live file. Use `multi_replace_string_in_file` to batch independent edits and reduce roundtrips.

---

## Diagnostic Checklist — Updated April 2, 2026

**🔑 Student API calls returning 401 after login?**
1. ✅ Check `localStorage` in DevTools → Application tab for `sat_student_token`
2. ✅ Verify `StudentAuthStateService.setToken()` exists
3. ✅ Verify `student-auth-business.service.ts` calls `setToken(response.token)` in `tap()`
4. ✅ Verify interceptor falls back to `studentAuthState.getToken()` when teacher token is null

**🔄 Auth interceptor redirect loop on login page?**
1. ✅ Check the interceptor 401 handler — does it have `&& token` guard?
2. ✅ Fix: `if (error.status === 401 && token) { redirect... }`

**👻 Session restored from localStorage but all requests fail with 401?**
1. ✅ Startup restore must read BOTH the profile key AND the token key
2. ✅ If either is missing, return `null` — do not restore the session
3. ✅ `logout()` must remove both keys

**📊 Status badge showing wrong state ("Submitted" when nothing uploaded)?**
1. ✅ Verify DTO includes `submissionCount` and `isAssigned` from the backend
2. ✅ Use three-way logic: `submissionCount > 0` → Submitted; `isAssigned` → Submit button; else → Pending
3. ✅ Never derive submission state from the due date alone

**✏️ replace_string_in_file failing with "old string not found"?**
1. ✅ Read the actual file with `read_file` before editing
2. ✅ Look for XML `/// <summary>` doc comments that summaries may have omitted
3. ✅ Include at least 3 lines of unchanged context above and below the target
4. ✅ Use `multi_replace_string_in_file` to batch multiple edits

---

---

## 🚨 CRITICAL ISSUES (April 17, 2026)

### 49. `/admin` Route Not Found — Admin Dashboard Inaccessible 🔗
**Problem**: Navigating to `http://localhost:4200/admin` does not open the admin dashboard or login — it silently redirects to the teacher homepage or shows a blank page.  
**Root Cause**: No route for the path `admin` existed in `app.routes.ts`. The Angular wildcard `{ path: '**', redirectTo: '' }` caught the request and redirected to `/` (teacher homepage, protected by `authGuard`). There was no way to reach the admin login or admin dashboard via the short `/admin` URL.  
**Fix**: Add a redirect entry above the wildcard:
```typescript
// app.routes.ts — Admin routes block
{ path: 'admin', redirectTo: 'admin/login', pathMatch: 'full' },  // ✅ ADD THIS
{ path: 'admin/login', component: AdminLoginComponent, canActivate: [adminGuestGuard] },
{ path: 'admin/dashboard', component: AdminDashboardComponent, canActivate: [adminAuthGuard] },
```
**How it now works**:  
- `/admin` → redirects to `/admin/login`  
- If `admin_token` exists in `localStorage` → `adminGuestGuard` redirects to `/admin/dashboard`  
- After login → lands on `/admin/dashboard` (guarded by `adminAuthGuard`)  
**File**: `StudentApp/src/app/app.routes.ts`  
**Prevention**: When adding login/dashboard route pairs, always add a bare-name redirect entry (`admin` → `admin/login`). The wildcard catch-all silences missing-route errors making them hard to notice.

---

### 50. Swagger UI Not Sending HTTP Requests (CRUD Broken) 🔧
**Problem**: Opening Swagger UI at `http://localhost:5000/swagger` shows the API documentation but clicking **Execute** either does nothing or returns no body. All CRUD operations (GET, POST, PUT, DELETE) fail to fire.  
**Root Cause — three separate bugs**:

**Bug 1 — Wrong middleware order (Critical)**  
`UseSwagger()` and `UseSwaggerUI()` were placed *after* `UseAuthentication()` / `UseAuthorization()`. The auth middleware pipeline intercepted `/swagger/**` requests and matched them against the `MapFallbackToFile` SPA catch-all, corrupting the request before Swagger's own middleware could respond.

**Bug 2 — Global security requirement on all operations**  
`AddSecurityRequirement` in `AddSwaggerGen` locked *every* endpoint — including public ones like `/api/admins/login`. This made all operations appear locked in the UI, preventing unauthenticated calls to obtain a token (chicken-and-egg problem).

**Bug 3 — No `EnableTryItOutByDefault()`**  
Swagger UI requires a manual "Try it out" click per operation before the Execute button appears. Users never saw the Execute button, which directly caused the "not sending requests" symptom.

**Fix — three changes in `Program.cs`**:

**(a) Move Swagger before auth middleware:**
```csharp
// BEFORE (WRONG):
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger(...);
app.UseSwaggerUI(...);

// AFTER (CORRECT):
app.UseCors("AllowAngular");
app.UseSwagger(...);     // ← BEFORE auth
app.UseSwaggerUI(...);  // ← BEFORE auth
app.UseAuthentication();
app.UseAuthorization();
```

**(b) Replace global `AddSecurityRequirement` with an operation filter:**
```csharp
// REMOVE from AddSwaggerGen:
options.AddSecurityRequirement(new OpenApiSecurityRequirement { ... });

// ADD instead:
options.OperationFilter<SwaggerAuthOperationFilter>();
```
Create `Presentation/Swagger/SwaggerAuthOperationFilter.cs`:
```csharp
public class SwaggerAuthOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Only apply Bearer requirement to [Authorize] actions
        var hasAuthorize = context.MethodInfo.GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>().Any()
            || (context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>().Any() ?? false);
        if (!hasAuthorize) return;

        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme {
                        Reference = new OpenApiReference {
                            Type = ReferenceType.SecurityScheme, Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            }
        };
    }
}
```

**(c) Enable TryItOut and persist the token:**
```csharp
app.UseSwaggerUI(options =>
{
    // ... existing options ...
    options.EnableTryItOutByDefault();         // ✅ Execute button visible immediately
    options.ConfigObject.PersistAuthorization = true; // ✅ Token survives page refresh
});
```
**Files**: `StudentAssessmentTrackerAPI/Program.cs`, `StudentAssessmentTrackerAPI/Presentation/Swagger/SwaggerAuthOperationFilter.cs` *(new)*  
**Prevention**:  
- Swagger middleware must always be placed before `UseAuthentication()` to prevent SPA catch-all from intercepting `/swagger/**` paths.  
- Never use global `AddSecurityRequirement` — use an `IOperationFilter` that reads `[Authorize]` attributes so public endpoints stay unlocked.  
- Always call `EnableTryItOutByDefault()` during project setup.

---

## Diagnostic Checklist — Updated April 17, 2026

**🔗 Navigating to `/admin` goes to wrong page?**
1. ✅ Check `app.routes.ts` — is there a `{ path: 'admin', redirectTo: 'admin/login', pathMatch: 'full' }` entry?
2. ✅ Make sure it appears *before* the `{ path: '**', redirectTo: '' }` wildcard
3. ✅ Verify `adminGuestGuard` redirects to `/admin/dashboard` when `admin_token` is present in `localStorage`

**🔧 Swagger UI Execute button absent / sends no requests?**
1. ✅ Check middleware order in `Program.cs` — `UseSwagger()` and `UseSwaggerUI()` must come **before** `UseAuthentication()` and `UseAuthorization()`
2. ✅ Check `AddSwaggerGen` — remove global `AddSecurityRequirement`; replace with `options.OperationFilter<SwaggerAuthOperationFilter>()`
3. ✅ Add `options.EnableTryItOutByDefault()` to `UseSwaggerUI()` options
4. ✅ Add `options.ConfigObject.PersistAuthorization = true` so the Bearer token survives page refreshes
5. ✅ Public endpoints (login, register) should have no padlock icon — if they do, the global security requirement is still present

---

**Last Updated**: April 17, 2026  
**Total Issues Documented**: 50 (13 frontend + 6 architecture + 6 infrastructure/tooling + 8 auth/UX/DataTables + 8 security/integrity/architecture Apr-1 + 7 auth/interceptor/feature Apr-2 + 2 routing/swagger Apr-17)  
**Keep this guide nearby when developing!** 🚀
