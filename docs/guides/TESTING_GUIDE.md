# Testing Guide: FluentValidation and AutoMapper

This guide explains how to verify that FluentValidation and AutoMapper are working correctly in the Student Assessment Tracker REST API.

---

## Overview

The project uses:
- **FluentValidation 12.1** for server-side input validation
- **AutoMapper 12.0** for mapping between domain entities and DTOs

Both are exercised on every API request that creates or updates a resource. You do not need to run unit tests to verify them — sending a request through Swagger UI or Postman is sufficient.

---

## Method 1: Using Swagger UI (Recommended)

### Setup
1. Start the API: `dotnet run` in the `StudentAssessmentTrackerAPI/` folder
2. Open `http://localhost:5000/swagger` in a browser
3. Get an admin token via `POST /api/admins/login` and click **Authorize** to set it

---

### Testing Student Validation (`CreateStudentValidator`)

Endpoint: `POST /api/admins/students`

**Test A — Empty required field:**
```json
{
  "idPassportNo": "",
  "firstName": "John",
  "lastName": "Smith",
  "email": "john@school.com",
  "phone": "12345678",
  "gradeName": "Grade 10"
}
```
Expected: `400 Bad Request` with `IdPassportNo is required`

**Test B — Phone format (must be exactly 8 digits, numeric only):**
```json
{
  "idPassportNo": "ID001",
  "firstName": "John",
  "lastName": "Smith",
  "email": "john@school.com",
  "phone": "123",
  "gradeName": "Grade 10"
}
```
Expected: `400 Bad Request` with `Phone must be exactly 8 digits`

**Test C — Invalid email format:**
```json
{
  "idPassportNo": "ID001",
  "firstName": "John",
  "lastName": "Smith",
  "email": "not-an-email",
  "phone": "12345678",
  "gradeName": "Grade 10"
}
```
Expected: `400 Bad Request` with an email format error

**Test D — Valid data (all fields correct):**
```json
{
  "idPassportNo": "ID001",
  "firstName": "John",
  "lastName": "Smith",
  "email": "john.smith@school.com",
  "phone": "12345678",
  "gradeName": "Grade 10"
}
```
Expected: `201 Created` with the full `StudentResponseDto` in the response body. The response includes `studentUniqueId`, `isActive`, and computed fields — these are AutoMapper-derived values that confirm mapping is working.

---

### Testing Teacher Validation (`TeacherRegisterValidator`)

Endpoint: `POST /api/admins/teachers`

**Test A — Name too short:**
```json
{
  "idPassportNo": "T001",
  "firstName": "A",
  "lastName": "Smith",
  "email": "a.smith@school.com",
  "phone": "12345678",
  "subjectName": "Mathematics"
}
```
Expected: `400 Bad Request` with `First name must be at least 2 characters`

**Test B — Non-existent subject:**
```json
{
  "idPassportNo": "T001",
  "firstName": "Alice",
  "lastName": "Smith",
  "email": "alice.smith@school.com",
  "phone": "12345678",
  "subjectName": "Quantum Physics"
}
```
Expected: `400 Bad Request` or `404` indicating subject not found

---

### Testing Assessment Validation (`StudentAssessmentValidator`)

Endpoint: `POST /api/students/{id}/assessments` (Teacher JWT required)

**Test A — Score exceeds MaxScore:**
```json
{
  "name": "Test 1",
  "score": 95,
  "maxScore": 50
}
```
Expected: `400 Bad Request` — score cannot exceed maxScore

**Test B — MaxScore zero or negative:**
```json
{
  "name": "Test 1",
  "score": 0,
  "maxScore": 0
}
```
Expected: `400 Bad Request` — maxScore must be greater than zero

**Test C — Valid assessment:**
```json
{
  "name": "Test 1",
  "score": 45,
  "maxScore": 50,
  "dueDate": "2026-05-01T00:00:00",
  "instructions": "Complete all sections",
  "isAssigned": true
}
```
Expected: `201 Created` with the full assessment DTO in the response

---

## Method 2: Using Postman

1. Import `StudentAssessmentTracker.postman_collection.json` (project root) into Postman
2. Import `StudentAssessmentTracker.postman_environment.json` and select it
3. Run `POST /api/admins/login` — the admin token is saved automatically
4. Navigate to the relevant request in the collection
5. Modify the request body to include invalid data and click **Send**
6. The response body will contain the FluentValidation error messages

See `api-postman/POSTMAN_QUICK_REFERENCE.md` for a full endpoint reference.

---

## Method 3: Confirming AutoMapper via Response Shape

AutoMapper is confirmed working when:
- A `POST /api/admins/students` `201` response body contains `studentUniqueId`, `isActive`, `grade`, and `teachers` — these are DTO-only fields derived from entity relationships by AutoMapper
- A `GET /api/admins/teachers` response body contains `isActive` — a DTO-only computed property derived from whether `Password` is non-null
- A `GET /api/students/{id}` response body contains `totalScore`, `averageScore`, `percentage`, and `performanceLevel` — fields computed by domain logic and mapped through AutoMapper

If any of these fields are missing or `null` when they should have values, check `Application/Mappings/MappingProfile.cs`.

---

## Validation Rules Reference

### Student (CreateStudentValidator / UpdateStudentValidator)
| Field | Rule |
|---|---|
| IdPassportNo | Required, max 20 chars, must be unique |
| FirstName | Required, 2–50 chars |
| LastName | Required, 2–50 chars |
| Email | Required, valid email format |
| Phone | Required, exactly 8 digits, numeric only |
| GradeName | Required, must match a seeded Grade record |

### Teacher (TeacherRegisterValidator)
| Field | Rule |
|---|---|
| IdPassportNo | Required, max 20 chars |
| FirstName | Required, 2–50 chars |
| LastName | Required, 2–50 chars |
| Email | Required, valid email format |
| Phone | Required, exactly 8 digits |
| SubjectName | Required, must match a seeded Subject record |

### Assessment (StudentAssessmentValidator)
| Field | Rule |
|---|---|
| Name | Required, max 100 chars |
| Score | Required, 0 ≤ Score ≤ MaxScore |
| MaxScore | Required, must be > 0 |


3. **Try these tests:**

   **Test A - Empty First Name:**
   - Leave `First Name` blank
   - Fill in `Last Name`: "Smith"
   - Fill in `Assessment1`: 15
   - Click Create
   - ✅ You should see error: **"First name is required"**

   **Test B - First Name Too Short:**
   - First Name: "A" (only 1 character)
   - Last Name: "Smith"
   - Assessment1: 15
   - Click Create
   - ✅ You should see error: **"First name must be at least 2 characters"**

   **Test C - Assessment Score Out of Range:**
   - First Name: "John"
   - Last Name: "Smith"
   - Assessment1: 25 (more than 20!)
   - Click Create
   - ✅ You should see error: **"Assessment 1 must be between 0 and 20"**

   **Test D - Valid Data (Everything Correct):**
   - First Name: "John"
   - Last Name: "Smith"
   - Assessment1: 15
   - Assessment2: 18
   - Assessment3: 20
   - Click Create
   - ✅ Student should be created and redirect to the list

---

### **METHOD 2: Testing with Console Logs (Advanced)**

Add debug messages to see what's happening:

#### **Step 1: Update StudentsController.cs**

Add this code to see FluentValidation and AutoMapper working:

```csharp
[HttpPost]
public async Task<IActionResult> Create(Student student)
{
    Console.WriteLine("🔍 TESTING FLUENTVALIDATION & AUTOMAPPER");
    Console.WriteLine($"📥 Received Student: FirstName={student.FirstName}, LastName={student.LastName}");

    // TEST 1: FluentValidation
    Console.WriteLine("⚙️  Running FluentValidation...");
    var validationResult = await _validator.ValidateAsync(student);
    
    Console.WriteLine($"✓ Validation Result: IsValid = {validationResult.IsValid}");
    
    if (!validationResult.IsValid)
    {
        Console.WriteLine("❌ Validation FAILED!");
        foreach (var error in validationResult.Errors)
        {
            Console.WriteLine($"   ❌ {error.PropertyName}: {error.ErrorMessage}");
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
        return View(student);
    }

    Console.WriteLine("✅ Validation PASSED!");

    // TEST 2: AutoMapper
    Console.WriteLine("⚙️  Running AutoMapper...");
    var mappedStudent = _mapper.Map<Student>(student);
    Console.WriteLine($"✓ AutoMapper Result: FirstName={mappedStudent.FirstName}, LastName={mappedStudent.LastName}");

    _context.Students.Add(mappedStudent);
    _context.SaveChanges();
    
    Console.WriteLine("✅ Student saved successfully!");
    return RedirectToAction(nameof(Index));
}
```

#### **Step 2: Run and Watch Console**

1. Open terminal: `dotnet run`
2. Create a student with invalid data
3. Look at the console output - you'll see:
   ```
   🔍 TESTING FLUENTVALIDATION & AUTOMAPPER
   📥 Received Student: FirstName=A, LastName=Smith
   ⚙️  Running FluentValidation...
   ❌ Validation FAILED!
      ❌ FirstName: First name must be at least 2 characters
   ```

4. Try again with valid data - you'll see:
   ```
   🔍 TESTING FLUENTVALIDATION & AUTOMAPPER
   📥 Received Student: FirstName=John, LastName=Smith
   ⚙️  Running FluentValidation...
   ✓ Validation Result: IsValid = True
   ✅ Validation PASSED!
   ⚙️  Running AutoMapper...
   ✓ AutoMapper Result: FirstName=John, LastName=Smith
   ✅ Student saved successfully!
   ```

---

### **METHOD 3: Unit Testing (Professional Approach)**

Create a test file to verify everything works:

#### **Create: `StudentValidatorTests.cs`**

```csharp
using Xunit;
using FluentValidation;
using StudentAssessmentTracker.Models;
using StudentAssessmentTracker.Validators;

namespace StudentAssessmentTracker.Tests;

public class StudentValidatorTests
{
    private readonly StudentValidator _validator = new();

    [Fact]
    public async Task Should_FailValidation_When_FirstNameIsEmpty()
    {
        var student = new Student 
        { 
            FirstName = "", 
            LastName = "Smith", 
            Assessment1 = 15 
        };

        var result = await _validator.ValidateAsync(student);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "FirstName");
    }

    [Fact]
    public async Task Should_PassValidation_When_AllFieldsAreValid()
    {
        var student = new Student 
        { 
            FirstName = "John", 
            LastName = "Smith", 
            Assessment1 = 15,
            Assessment2 = 18,
            Assessment3 = 20
        };

        var result = await _validator.ValidateAsync(student);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Should_FailValidation_When_AssessmentOutOfRange()
    {
        var student = new Student 
        { 
            FirstName = "John", 
            LastName = "Smith", 
            Assessment1 = 25 // Out of range!
        };

        var result = await _validator.ValidateAsync(student);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Assessment1");
    }
}
```

---

## Quick Summary

| Test Method | Best For | Difficulty |
|------------|----------|-----------|
| **Method 1 (Web)** | Seeing it work in real app | ⭐ Easy - Start here! |
| **Method 2 (Console)** | Understanding what's happening | ⭐⭐ Medium |
| **Method 3 (Unit Tests)** | Professional testing | ⭐⭐⭐ Advanced |

---

## ✅ What You're Looking For

**FluentValidation is working** when:
- ❌ Invalid data → Shows error messages
- ✅ Valid data → Creates student successfully

**AutoMapper is working** when:
- Data automatically converts from form → Student object
- No manual copying of fields needed

**Both together** = Professional, validated, clean data flow! 🎉
