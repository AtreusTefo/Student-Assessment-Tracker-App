# Report: FluentValidation & AutoMapper

## Executive summary
- Added two industry-standard libraries: **FluentValidation** and **AutoMapper**.
- FluentValidation centralizes and standardizes input validation rules so invalid submissions are rejected cleanly.
- AutoMapper automates copying/converting data between objects (reducing manual, repetitive assignment code).
- Both are registered in the app and wired into the request flow so that the exact files listed below can be reviewed.

## What is FluentValidation?
- Analogy: FluentValidation is like a set of quality checks or a security guard for incoming data it inspects each incoming form or object and rejects it if rules aren't met.
- Purpose: Keep validation rules in one place, make them readable (they "flow" like sentences) and avoid scattered if-statements throughout controllers.

How it's used in this project
- Registered in the app startup: [Program.cs](Program.cs) (I used `AddFluentValidationAutoValidation()` so validators run automatically during model binding).
- Rules live in: [Validators/StudentValidator.cs](Validators/StudentValidator.cs). Example rules:
  - `RuleFor(s => s.FirstName).NotEmpty().MinimumLength(2).MaximumLength(50)`
  - `RuleFor(s => s.Assessment1).InclusiveBetween(0, 20)`
- The controller relies on model validation results via `ModelState.IsValid`: see [Controllers/StudentsController.cs](Controllers/StudentsController.cs).
- Validation messages are shown in the UI: [Views/Students/Create.cshtml](Views/Students/Create.cshtml).
- Note: I removed overlapping DataAnnotation `[Range]` attributes from `Models/Student.cs` so FluentValidation is the single source for assessment ranges. See [Models/Student.cs](Models/Student.cs).

Why this helps
- Rules are easier to change and test.
- Validation runs automatically during model binding and populates `ModelState`, so controllers stay clean.

## What is AutoMapper?
- Analogy: AutoMapper is like a translator or conveyor belt that moves matching data from one object to another so you don't write repetitive field copy code.
- Purpose: Remove boilerplate mapping code (e.g., `a.FirstName = dto.FirstName; a.LastName = dto.LastName; ...`).

How it's used in this project
- Registered in the app startup: [Program.cs](Program.cs) via `AddAutoMapper(typeof(MappingProfile))`.
- Profile/configuration: [Mappings/MappingProfile.cs](Mappings/MappingProfile.cs) — contains mapping rules (for now `CreateMap<Student, Student>().ReverseMap()` as an example).
- Usage: In the controller we call `_mapper.Map<Student>(student)` before saving — see [Controllers/StudentsController.cs](Controllers/StudentsController.cs).

Why this helps
- Less repetitive, fewer mistakes when copying fields.
- Easier to adapt when we introduce DTOs or different shapes of data (e.g., API models vs database models).

## Files to review (quick links)
- App registration: [Program.cs](Program.cs)
- FluentValidation rules: [Validators/StudentValidator.cs](Validators/StudentValidator.cs)
- AutoMapper profile: [Mappings/MappingProfile.cs](Mappings/MappingProfile.cs)
- Controller usage: [Controllers/StudentsController.cs](Controllers/StudentsController.cs)
- View error display: [Views/Students/Create.cshtml](Views/Students/Create.cshtml)
- Model: [Models/Student.cs](Models/Student.cs)
- Project file (NuGet packages): [StudentAssessmentTracker.csproj](StudentAssessmentTracker.csproj)

## How to test (quick steps)
1. Stop and run the app locally:
```bash
dotnet run
```
2. Open browser: `http://localhost:5000/Students/Create`
3. Test cases:
   - Leave `First Name` blank → Expect a single error: "First name is required" (FluentValidation).
   - Set `Assessment1` to `25` → Expect a single error: "Assessment 1 must be between 0 and 20" (FluentValidation).
   - Provide valid data → Student saved and redirected to list.