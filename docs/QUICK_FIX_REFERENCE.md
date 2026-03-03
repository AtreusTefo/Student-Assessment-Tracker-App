# Quick Fix Reference Guide

## 25 Major Issues Fixed & How to Fix Them Again

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

## Full Documentation

See **`ERROR_FIXES_DOCUMENTATION.md`** in docs folder for:
- Detailed explanations of all 19 original issues
- Complete code examples
- Prevention tips
- Testing checklist
- Key learnings

See **`DAILY_REPORT_2026-03-03.md`** for detailed write-up of issues 20–25 (March 3, 2026).

---

**Last Updated**: March 3, 2026  
**Total Issues Documented**: 25 (13 frontend + 6 architecture + 6 infrastructure/tooling)  
**Keep this guide nearby when developing!** 🚀
