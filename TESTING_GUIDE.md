# Testing FluentValidation and AutoMapper

## How to Test Them

### **METHOD 1: Using the Web Application (Easiest for Beginners)**

#### **Test FluentValidation:**

1. **Run your application:**
   ```
   dotnet run
   ```

2. **Go to:** `https://localhost:5001/Students/Create`

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
