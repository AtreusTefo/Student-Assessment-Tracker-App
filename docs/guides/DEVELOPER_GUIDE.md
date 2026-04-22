# Developer Guide: Multi-Layered Architecture

## Quick Start for Developers

This guide helps you understand, navigate, and extend the Student Assessment Tracker's multi-layered architecture.

---

##  Where to Find Things

### Adding a New API Feature

Let's say you want to add a "GetStudentsByGrade" endpoint.

**Step 1: Domain Layer** - Add business logic
```csharp
// Domain/Entities/Student.cs
public class Student
{
    // ... existing code ...
    public int GradeId { get; set; }          // FK to Grades table
    public Grade? GradeNavigation { get; set; } // navigation property
}

// Add this to Domain/Interfaces/IRepository.cs if needed new interface
```

**Step 2: Infrastructure Layer** - Add data access
```csharp
// Infrastructure/Repositories/StudentRepository.cs
public async Task<IEnumerable<Student>> GetByGradeAsync(int gradeId)
{
    return await _context.Students
        .Include(s => s.GradeNavigation)
        .Where(s => s.GradeId == gradeId)
        .ToListAsync();
}
```

**Step 3: Application Layer** - Add service method
```csharp
// Application/Services/StudentService.cs
public interface IStudentService
{
    Task<IEnumerable<StudentDto>> GetStudentsByGradeAsync(int gradeId);
}

public class StudentService : IStudentService
{
    public async Task<IEnumerable<StudentDto>> GetStudentsByGradeAsync(int gradeId)
    {
        _logger.LogInformation("Fetching students with gradeId: {GradeId}", gradeId);
        var students = await _repository.GetByGradeAsync(gradeId);
        return _mapper.Map<IEnumerable<StudentDto>>(students);
    }
}
```

**Step 4: Presentation Layer** - Add API endpoint
```csharp
// Presentation/Controllers/StudentsController.cs
[HttpGet("by-grade/{gradeId}")]
public async Task<ActionResult<IEnumerable<StudentDto>>> GetByGrade(int gradeId)
{
    try
    {
        var students = await _studentService.GetStudentsByGradeAsync(gradeId);
        return Ok(students);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error fetching students by grade");
        return StatusCode(500, "Internal server error");
    }
}
```

**Step 5: Program.cs** - Usually no changes needed (already registered)

---

##  Key Files Map

| Task | File Location | What to Edit |
|------|---------------|--------------|
| Add business logic | `Domain/Entities/Student.cs` | Entity properties & methods |
| Add data access | `Infrastructure/Repositories/StudentRepository.cs` | Add query methods |
| Create service | `Application/Services/StudentService.cs` | Add service methods |
| Add validation | `Application/Validators/StudentValidator.cs` | Add/update validation rules |
| Expose API | `Presentation/Controllers/StudentsController.cs` | Add HTTP endpoints |
| Map DTOs | `Application/Mappings/MappingProfile.cs` | Add mapping configurations |

---

##  Layer Responsibilities Quick Reference

### Domain Layer
-  Pure C# classes
-  Business logic methods
-  No database access
-  No HTTP concerns
-  No framework dependencies

### Infrastructure Layer
-  Database operations
-  Repository implementations
-  DbContext configuration
-  Data persistence
-  Business logic

### Application Layer
-  Service orchestration
-  Validation rules
-  DTO definitions
-  Object mapping
-  HTTP details
-  Database specifics

### Presentation Layer
-  HTTP endpoints
-  Request handling
-  Response formatting
-  Business logic
-  Data access

---

##  Dependency Flow (One Direction Only!)

```
Presentation Controller
    
Application Service (via IStudentService)
    
Infrastructure Repository (via IRepository<T>)
    
Domain Entity
```

**Rule**: Lower layers never depend on higher layers. Only upper layers know about lower layers.

---

##  Testing Strategy

### Unit Testing Example

```csharp
// Test Domain Logic
[TestClass]
public class StudentTests
{
    [TestMethod]
    public void GetPercentage_ShouldCalculateCorrectly()
    {
        var student = new Student { Assessment1 = 10, Assessment2 = 15, Assessment3 = 20 };
        var percentage = student.GetPercentage(); // (45/60) * 100 = 75
        Assert.AreEqual(75, percentage);
    }
}

// Test Service Layer
[TestClass]
public class StudentServiceTests
{
    private Mock<IRepository<Student>> _mockRepository;
    private Mock<IMapper> _mockMapper;
    private StudentService _service;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IRepository<Student>>();
        _mockMapper = new Mock<IMapper>();
        _service = new StudentService(_mockRepository.Object, _mockMapper.Object, 
                                      new Mock<ILogger<StudentService>>().Object);
    }

    [TestMethod]
    public async Task CreateStudentAsync_ShouldCallRepository()
    {
        var dto = new CreateStudentDto { FirstName = "John", /* ... */ };
        await _service.CreateStudentAsync(dto);
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<Student>()), Times.Once);
    }
}

// Test API Endpoint
[TestClass]
public class StudentsControllerTests
{
    private Mock<IStudentService> _mockService;
    private StudentsController _controller;

    [TestInitialize]
    public void Setup()
    {
        _mockService = new Mock<IStudentService>();
        _controller = new StudentsController(_mockService.Object, 
                                            new Mock<ILogger<StudentsController>>().Object);
    }

    [TestMethod]
    public async Task GetAllStudents_ShouldReturnOk()
    {
        _mockService.Setup(x => x.GetAllStudentsAsync())
            .ReturnsAsync(new List<StudentDto>());
        
        var result = await _controller.GetAllStudents();
        
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
    }
}
```

---

##  Running the Application

### Start the Backend API
```bash
cd StudentAssessmentTracker
dotnet run
# API runs at http://localhost:5000
# Swagger available at http://localhost:5000/swagger (if configured)
```

### Start the Angular Frontend (Development)
```bash
cd StudentApp
npm start
# Frontend runs at http://localhost:4200
# Automatically proxies API calls to http://localhost:5000
```

### Run Tests
```bash
dotnet test StudentAssessmentTracker.Tests
```

---

##  Common Tasks

### Task 1: Add a New Database Field to Student

1. **Domain**: Add property to `Student.cs`
   ```csharp
   public string Subject { get; set; }
   ```

2. **Infrastructure**: Update `ApplicationDbContext.cs` OnModelCreating
   ```csharp
   entity.Property(e => e.Subject).HasMaxLength(50);
   ```

3. **Application**: Update `StudentDto.cs`
   ```csharp
   public string Subject { get; set; }
   ```

4. **Presentation**: Update controller input/output as needed

### Task 2: Add New Validation Rule

1. Open `Application/Validators/StudentValidator.cs`
2. Add rule in the constructor:
   ```csharp
   RuleFor(x => x.Subject)
       .NotEmpty().WithMessage("Subject is required")
       .Length(3, 30).WithMessage("Subject must be 3-30 characters");
   ```

### Task 3: Add Calculated Field to DTO

1. Add property to `Application/DTOs/StudentDto.cs`
2. Update mapping in `Application/Mappings/MappingProfile.cs`
   ```csharp
   CreateMap<Student, StudentDto>()
       .ForMember(dest => dest.NewField, opt => opt.MapFrom(src => src.GetNewField()));
   ```

### Task 4: Add Custom Repository Method

1. Add method to `StudentRepository.cs`
   ```csharp
   public async Task<IEnumerable<Student>> GetTopStudentsAsync(int count)
   {
       return await _context.Students
           .OrderByDescending(s => s.GetTotalScore())
           .Take(count)
           .ToListAsync();
   }
   ```

2. Call from `StudentService.cs`
3. Expose via `StudentsController.cs`

---

##  Architecture Rules to Remember

###  DO:
-  Use dependency injection for all dependencies
-  Keep each layer focused on its responsibility
-  Use interfaces for abstraction
-  Log important operations
-  Handle exceptions gracefully
-  Validate data at both frontend and backend

###  DON'T:
-  Have Presentation layer call Repository directly
-  Put database logic in Domain layer
-  Mix concerns in a single class
-  Create circular dependencies
-  Skip error handling
-  Hardcode configuration values

---

##  Debugging Tips

### 1. Check Dependency Injection
Use breakpoints in `Program.cs` to verify services are registered correctly.

### 2. Trace Request Flow
- Add breakpoint in controller
- Step through service
- Check data in repository

### 3. Database Debugging
```csharp
// In StudentRepository
public override async Task<IEnumerable<Student>> GetAllAsync()
{
    var query = _context.Students.AsQueryable();
    var result = await query.ToListAsync();
    Console.WriteLine($"Retrieved {result.Count()} students"); // Debug output
    return result;
}
```

### 4. Validation Issues
Check `StudentValidator.cs` rules match frontend expectations.

### 5. Mapping Problems
Add debugging in `MappingProfile.cs`:
```csharp
CreateMap<Student, StudentDto>()
    .ForMember(dest => dest.TotalScore, opt => 
        opt.MapFrom(src => 
        {
            var total = src.GetTotalScore();
            Debug.WriteLine($"Total: {total}");
            return total;
        }));
```

---

##  Performance Considerations

### Repository Queries
```csharp
//  Inefficient: Returns all then filters
var allStudents = await _context.Students.ToListAsync();
var filtered = allStudents.Where(s => s.Grade == "10A").ToList();

//  Efficient: Filters in database
var filtered = await _context.Students
    .Where(s => s.Grade == "10A")
    .ToListAsync();
```

### Include Related Data
```csharp
// When fetching related entities (if needed in future)
var students = await _context.Students
    .Include(s => s.Assessments)
    .ToListAsync();
```

---

##  Learning Resources

- **Clean Architecture**: Read "Clean Architecture" by Robert C. Martin
- **SOLID Principles**: https://en.wikipedia.org/wiki/SOLID
- **Repository Pattern**: Microsoft Docs on Entity Framework
- **Dependency Injection**: ASP.NET Core DI documentation

---

##  FAQ

**Q: Where should I put a new method?**  
A: If it's business logic  Domain, if it's data access  Repository, if it's orchestration  Service

**Q: Why separate DTOs?**  
A: Decouples API contract from domain model. You can change the API without changing the database schema.

**Q: Can I skip a layer?**  
A: No. The layers provide clear separation even if some are thin.

**Q: How do I test this?**  
A: Mock the dependencies below each layer and test in isolation.

---

##  Support

For questions about the architecture, refer to [ARCHITECTURE_IMPLEMENTATION.md](ARCHITECTURE_IMPLEMENTATION.md)

Happy coding! 
