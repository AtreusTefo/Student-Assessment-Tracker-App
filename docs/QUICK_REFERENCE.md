# Quick Reference Card - Multi-Layered Architecture

## 🏗️ The Four Layers (Bottom to Top)

```
┌─────────────────────────────────┐
│   PRESENTATION (HTTP/REST)      │  ← Controllers, Endpoints
├─────────────────────────────────┤
│   APPLICATION (Services)        │  ← Business Logic Coordination
├─────────────────────────────────┤
│   INFRASTRUCTURE (Data Access)  │  ← Database, Repositories
├─────────────────────────────────┤
│   DOMAIN (Business Rules)       │  ← Entities, Core Logic
└─────────────────────────────────┘
```

**Key Rule**: Upper layers know about layers below. Lower layers never know about upper layers. ⬇️ Only! ❌ Not Up!

---

## 📁 Where to Find What

| Need to... | Go to... | File |
|-----------|----------|------|
| **Add business logic** | Domain | `Domain/Entities/Student.cs` |
| **Add database operation** | Infrastructure | `Infrastructure/Repositories/StudentRepository.cs` |
| **Add API validation** | Application | `Application/Validators/StudentValidator.cs` |
| **Add HTTP endpoint** | Presentation | `Presentation/Controllers/StudentsController.cs` |
| **Create new service** | Application | `Application/Services/` |
| **Map DTO ↔ Entity** | Application | `Application/Mappings/MappingProfile.cs` |
| **Register services** | Project Root | `Program.cs` |

---

## 💻 Adding a New Feature (Step by Step)

### Example: Add "Get Top Students" feature

#### Step 1️⃣: Domain (Business Logic)
```csharp
// Domain/Entities/Student.cs
public class Student
{
    public decimal GetTotalScore() => Assessment1 + Assessment2 + Assessment3;
    // Already has the logic needed!
}
```

#### Step 2️⃣: Infrastructure (Data Access)
```csharp
// Infrastructure/Repositories/StudentRepository.cs
public async Task<IEnumerable<Student>> GetTopStudentsAsync(int count)
{
    return await _context.Students
        .OrderByDescending(s => s.Assessment1 + s.Assessment2 + s.Assessment3)
        .Take(count)
        .ToListAsync();
}
```

#### Step 3️⃣: Application (Service)
```csharp
// Application/Services/StudentService.cs
public interface IStudentService
{
    Task<IEnumerable<StudentDto>> GetTopStudentsAsync(int count);
}

public class StudentService : IStudentService
{
    public async Task<IEnumerable<StudentDto>> GetTopStudentsAsync(int count)
    {
        var students = await _repository.GetTopStudentsAsync(count);
        return _mapper.Map<IEnumerable<StudentDto>>(students);
    }
}
```

#### Step 4️⃣: Presentation (API)
```csharp
// Presentation/Controllers/StudentsController.cs
[HttpGet("top/{count}")]
public async Task<ActionResult<IEnumerable<StudentDto>>> GetTopStudents(int count)
{
    var students = await _studentService.GetTopStudentsAsync(count);
    return Ok(students);
}
```

#### Step 5️⃣: Program.cs
Usually no changes needed! Services already registered.

**Done! Now test**: `GET /api/students/top/5`

---

## 🔌 Dependency Injection Pattern

```csharp
// How to inject dependencies
public class StudentService
{
    private readonly IRepository<Student> _repository;      // Injected!
    private readonly IMapper _mapper;                        // Injected!
    private readonly ILogger<StudentService> _logger;        // Injected!

    // Constructor Injection (ASP.NET Core magic)
    public StudentService(
        IRepository<Student> repository,
        IMapper mapper,
        ILogger<StudentService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }
}
```

**Registered in Program.cs:**
```csharp
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddAutoMapper(typeof(MappingProfile));
```

---

## 📊 API Endpoints Quick Reference

```
GET    /api/students          ← Get all
GET    /api/students/1        ← Get by ID
POST   /api/students          ← Create
PUT    /api/students/1        ← Update
DELETE /api/students/1        ← Delete
```

**Response Format:**
```json
{
  "id": 1,
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "phone": "12345678",
  "grade": "10A",
  "assessment1": 15,
  "assessment2": 18,
  "assessment3": 16,
  "totalScore": 49,           // ← Calculated by Domain
  "averageScore": 16.33,      // ← Calculated by Domain
  "percentage": 81.67,         // ← Calculated by Domain
  "performanceLevel": "Excellent"  // ← Calculated by Domain
}
```

---

## ✅ Architecture Checklist

When adding a new feature, ask:

- [ ] **Domain**: Does it have core business logic? Add to entity or create interface.
- [ ] **Infrastructure**: Does it need data access? Add to repository.
- [ ] **Application**: Does it need validation? Add to validators.
- [ ] **Application**: Does it coordinate logic? Add to service.
- [ ] **Application**: Does it need mapping? Add to MappingProfile.
- [ ] **Presentation**: Does it become an endpoint? Add to controller.
- [ ] **Program.cs**: Do services need registration? Register in DI.
- [ ] **Test**: Does each layer work independently?

---

## 🧪 Testing Layers (Pseudo-code)

```csharp
// Test Domain (Business Logic)
[Test] void GetPercentage_Should_Calculate_Correctly()
{
    var student = new Student { Assessment1=15, Assessment2=18, Assessment3=16 };
    Assert.AreEqual(81.67, student.GetPercentage());
}

// Test Service (Business Orchestration)
[Test] async Task CreateStudent_Should_Call_Repository()
{
    var mockRepo = new Mock<IRepository<Student>>();
    var service = new StudentService(mockRepo.Object, ...);
    
    await service.CreateStudentAsync(dto);
    
    mockRepo.Verify(r => r.AddAsync(It.IsAny<Student>()), Times.Once);
}

// Test Controller (HTTP Layer)
[Test] async Task GetAllStudents_Should_Return_Ok()
{
    var mockService = new Mock<IStudentService>();
    mockService.Setup(x => x.GetAllStudentsAsync())
        .ReturnsAsync(new List<StudentDto>());
    
    var controller = new StudentsController(mockService.Object, ...);
    var result = await controller.GetAllStudents();
    
    Assert.IsInstanceOf<OkObjectResult>(result.Result);
}
```

---

## 🚀 Running Locally

```powershell
# Terminal 1: Start Backend API
cd StudentAssessmentTracker
dotnet run
# 🚀 Runs on http://localhost:5000

# Terminal 2: Start Frontend (if needed for development)
cd StudentApp
npm start
# 🚀 Runs on http://localhost:4200 (with proxy to :5000)
```

**Test Endpoints:**
```powershell
# Get all students
curl http://localhost:5000/api/students

# Create student
$body = @{
    firstName="Jane"
    lastName="Smith"
    email="jane@example.com"
    phone="87654321"
    grade="11A"
    assessment1=20; assessment2=19; assessment3=18
} | ConvertTo-Json

curl -X POST http://localhost:5000/api/students `
     -Header "Content-Type: application/json" `
     -Body $body
```

---

## 📚 Files You'll Edit Most Often

1. **Adding business logic** → `Domain/Entities/Student.cs`
2. **Adding data queries** → `Infrastructure/Repositories/StudentRepository.cs`
3. **Adding validation** → `Application/Validators/StudentValidator.cs`
4. **Adding endpoints** → `Presentation/Controllers/StudentsController.cs`
5. **Adding services** → `Application/Services/StudentService.cs`

---

## ⚠️ Common Mistakes to Avoid

❌ **DON'T**: Put database logic directly in controllers
```csharp
// ❌ BAD
[HttpGet]
public IActionResult Get()
{
    var students = _context.Students.ToList();  // ❌ Direct DB access!
}
```

✅ **DO**: Use service pattern
```csharp
// ✅ GOOD
[HttpGet]
public async Task<IActionResult> Get()
{
    var students = await _studentService.GetAllStudentsAsync();  // ✅ Via service
}
```

---

❌ **DON'T**: Have lower layers import upper layer namespaces
```csharp
// ❌ BAD - Infrastructure importing from Presentation!
using StudentAssessmentTracker.Presentation.Controllers;
```

✅ **DO**: Only upper layers import lower layers
```csharp
// ✅ GOOD - Only Presentation and Application import Infrastructure
using StudentAssessmentTracker.Infrastructure.Repositories;
using StudentAssessmentTracker.Application.Services;
```

---

❌ **DON'T**: Put validation in domain entities
```csharp
// ❌ BAD
public class Student
{
    public string Validate() { /* ... */ }
}
```

✅ **DO**: Use service validators
```csharp
// ✅ GOOD
public class StudentValidator : AbstractValidator<CreateStudentDto>
{
    public StudentValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
    }
}
```

---

## 🎯 Remember

| Principle | Benefit |
|-----------|---------|
| **Each layer has one job** | Easy to understand and test |
| **Layers talk through interfaces** | Easy to swap implementations |
| **DTOs separate API from database** | Frontend changes don't break backend |
| **Business logic in entities** | Reusable across services |
| **Services orchestrate logic** | Clear entry points for operations |

---

## 📞 When in Doubt, Ask:

> **Q**: Where should I put this code?  
> **A**: Ask yourself...
> - Is it business logic? → **Domain**  
> - Is it database access? → **Infrastructure**  
> - Is it validation? → **Application**  
> - Is it HTTP handling? → **Presentation**  

---

## 📖 Full Documentation

- [ARCHITECTURE_IMPLEMENTATION.md](ARCHITECTURE_IMPLEMENTATION.md) - Detailed architecture
- [DEVELOPER_GUIDE.md](DEVELOPER_GUIDE.md) - How-to guide with examples
- [IMPLEMENTATION_CHECKLIST.md](IMPLEMENTATION_CHECKLIST.md) - What was done

---

**Happy Coding! 🚀**

The architecture is clean, the code is organized, and everything is documented.  
You're ready to build amazing features! 💪
