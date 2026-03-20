# Daily Report — March 20, 2026

**Project:** Student Assessment Tracker  
**Developer:** Developer.03

---

## What I Did Today

- Investigated and resolved an invalid Postman collection JSON file that was preventing imports
- Improved API response consistency for the Teachers endpoints (DELETE and PUT)
- Added an informative empty-state response for GET All Teachers
- Updated the Postman collection descriptions to accurately reflect all API response behaviours
- Diagnosed a stale API process locking the build output and resolved it
- Enforced letters-only validation on First Name and Last Name fields for both Student and Teacher across all forms (frontend + backend)

---

## What Was Completed

### 1. Postman Collection — Invalid JSON Fix
- **Problem:** Importing `StudentAssessmentTracker.postman_collection.json` into Postman failed with *"Incorrect format — We don't recognize/support this format."*
- **Resolution:** Rewrote the collection file using Python to guarantee clean, valid JSON. The file now contains 5 folders: Students, Student Assessments, Teachers, Grades, Subjects — all with correct up-to-date request bodies.

### 2. Teachers DELETE — Success Message
- **Change:** `DELETE /api/teachers/{id}` previously returned `204 No Content` with an empty body.
- **Updated to:** `200 OK` with `{ "message": "Teacher with ID {id} successfully deleted." }` on success, or `404` if the teacher does not exist.
- **File:** `Presentation/Controllers/TeachersController.cs`

### 3. Teachers PUT — Success Message
- **Change:** `PUT /api/teachers/{id}` previously returned `204 No Content` with an empty body.
- **Updated to:** `200 OK` with `{ "message": "Teacher with ID {id} successfully updated." }` on success, or `404` if the teacher does not exist.
- **File:** `Presentation/Controllers/TeachersController.cs`

### 4. GET All Teachers — Empty-State Response
- **Change:** `GET /api/teachers` previously returned `200 OK` with an empty array `[]` when no teachers existed.
- **Updated to:** `404 Not Found` with `{ "message": "No teachers found." }` when the table is empty.
- **File:** `Presentation/Controllers/TeachersController.cs`

### 5. Postman Collection Descriptions Updated
- Descriptions for Get All Teachers, Update Teacher, and Delete Teacher updated to document the new response shapes (`200` with message, `404` with message).

---

## Challenges Faced and How They Were Resolved

### Challenge 1 — Postman Import Failure (Invalid JSON)
**What happened:** After updating the Postman collection, Postman refused to import it with an "Incorrect format" error.  
**Root cause:** A previous file edit had appended the old stale collection content after the closing `}` of the new JSON, producing a document with two root objects — not valid JSON.  
**Resolution:** Used a Python script to construct and write the collection from a Python dictionary using `json.dump()`, which guaranteed structurally valid JSON. The file was verified by round-tripping it through `json.load()` before the script exited.

### Challenge 2 — API Change Not Taking Effect (Stale Process)
**What happened:** After updating `TeachersController.cs` to return `404` for an empty teacher list, testing in Postman still returned `200 OK` with an empty array.  
**Root cause:** The previous `dotnet run` process (PID 8220) was still running and had locked the output binary (`StudentAssessmentTracker.exe`). The new `dotnet run` could not overwrite the file, so the old code remained in use without any error visible to the user.  
**Resolution:** Used `Stop-Process -Id 8220 -Force` to terminate the locked process, then restarted the API. The updated controller loaded correctly and the `404` response was verified.

### Challenge 3 — Null-Safety on Empty Collection
**What happened:** `!teachers.Any()` could throw a `NullReferenceException` if the service returned `null` instead of an empty enumerable.  
**Resolution:** Added null-coalescing to materialise the result before checking: `var list = teachers?.ToList() ?? new List<TeacherResponseDto>()`, then checked `list.Count == 0`. This makes the check safe regardless of whether the service returns `null` or an empty list.

---

## What Was Completed (Continued)

### 6. Letters-Only Validation — First Name & Last Name (Student and Teacher)

#### Problem
First Name and Last Name fields on the Student form and Teacher registration/edit form accepted any characters — numbers, symbols, spaces, and hyphens — both on the frontend (no pattern restriction or keypress blocking) and on the backend validators (no regex rule enforced).

#### Backend Fixes
**Files:** `Application/Validators/StudentValidator.cs`, `Application/Validators/TeacherValidator.cs`

- **`StudentValidator.cs`** (`CreateStudentValidator` + `UpdateStudentValidator`):
  - Changed `FirstName` and `LastName` regex from `^[a-zA-Z\s\-]+$` → `^[a-zA-Z]+$`
  - Updated validation messages to: *"First/Last name can only contain letters"*

- **`TeacherValidator.cs`** (`TeacherRegisterValidator` + `TeacherUpdateValidator`):
  - Added `.Matches(@"^[a-zA-Z]+$")` rule to `FirstName` and `LastName` (previously had no regex at all)
  - Validation messages: *"First/Last name can only contain letters."*

#### Frontend Fixes
**Files:** `StudentApp/src/app/components/student-form.component.ts`, `StudentApp/src/app/components/signup-form.component.ts`

- Added `pattern="^[a-zA-Z]+$"` attribute to `firstName` and `lastName` inputs on both forms — Angular's built-in pattern validator marks the field invalid if non-letter characters are present
- Added `(keypress)="allowOnlyLetters($event)"` event binding to both name inputs on both forms — blocks non-letter characters from being typed at all (first line of defence)
- Added `allowOnlyLetters()` method to both component classes
- Added pattern error messages under each input: *"First/Last name can only contain letters"*

#### Result
Two-layer defence: keypress handler blocks non-letter input at the keyboard level; pattern validation catches anything bypassing it (e.g. paste); backend FluentValidation rejects any non-compliant value that reaches the API.
