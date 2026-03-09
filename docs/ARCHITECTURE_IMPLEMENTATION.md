# Multi-Layered Architecture Implementation

## 📋 Overview

The Student Assessment Tracker now implements a **professional-grade multi-layered decoupled architecture** following clean architecture and SOLID principles. This ensures proper **separation of concerns** where each layer has a single, well-defined responsibility.

---

## 🏗️ Architecture Layers

### 1. **Domain Layer** (Core Business Logic)
**Location**: `Domain/`  
**Responsibility**: Contains pure business logic, entities, and interfaces independent of any framework.

**Files**:
- [Domain/Entities/Student.cs](Domain/Entities/Student.cs) - Domain entity with business methods
- [Domain/Interfaces/IRepository.cs](Domain/Interfaces/IRepository.cs) - Generic repository abstraction

**Key Features**:
- ✅ Core business rules implemented as methods (GetTotalScore, GetAverageScore, GetPercentage, GetPerformanceLevel)
- ✅ No framework dependencies
- ✅ Highly testable
- ✅ Reusable across different layers

```csharp
// Domain Logic Example: Business rules in entity
public string GetPerformanceLevel()
{
    var percentage = GetPercentage();
    return percentage switch
    {
        < 50 => "Needs Support",
        <= 55 => "Satisfactory",
        <= 75 => "Good",
        _ => "Excellent"
    };
}
```

---

### 2. **Infrastructure Layer** (Data Access)
**Location**: `Infrastructure/`  
**Responsibility**: Handles data persistence, database operations, and external service integrations.

**Files**:
- [Infrastructure/Data/ApplicationDbContext.cs](Infrastructure/Data/ApplicationDbContext.cs) - Entity Framework context
- [Infrastructure/Repositories/StudentRepository.cs](Infrastructure/Repositories/StudentRepository.cs) - Generic and specific repositories

**Key Features**:
- ✅ Generic `Repository<T>` for CRUD operations
- ✅ Specialized `StudentRepository` for student-specific logic
- ✅ Database configuration and entity mappings
- ✅ Abstraction via `IRepository<T>` interface
- ✅ Easy to swap with different database providers

```csharp
// Repository Pattern: Abstraction for data access
public class Repository<T> : IRepository<T>
{
    public async Task<T> GetByIdAsync(int id) { /* ... */ }
    public async Task<IEnumerable<T>> GetAllAsync() { /* ... */ }
    public async Task AddAsync(T entity) { /* ... */ }
    public async Task UpdateAsync(T entity) { /* ... */ }
    public async Task DeleteAsync(int id) { /* ... */ }
}
```

---

### 3. **Application Layer** (Business Logic Orchestration)
**Location**: `Application/`  
**Responsibility**: Contains business logic services, data transfer objects (DTOs), validation, and object mapping.

**Files**:
- [Application/Services/StudentService.cs](Application/Services/StudentService.cs) - Business logic orchestration
- [Application/DTOs/StudentDto.cs](Application/DTOs/StudentDto.cs) - Data transfer objects
- [Application/Validators/StudentValidator.cs](Application/Validators/StudentValidator.cs) - FluentValidation rules
- [Application/Mappings/MappingProfile.cs](Application/Mappings/MappingProfile.cs) - AutoMapper configuration

**Key Features**:
- ✅ Service layer orchestrating domain and infrastructure
- ✅ Separation of input/output DTOs
- ✅ Centralized validation rules
- ✅ Automatic object mapping with AutoMapper
- ✅ Logging for debugging and monitoring

```csharp
// Service Layer: Orchestrates business logic
public class StudentService : IStudentService
{
    private readonly IRepository<Student> _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<StudentService> _logger;

    public async Task<StudentDto> CreateStudentAsync(CreateStudentDto dto)
    {
        var student = _mapper.Map<Student>(dto);
        student.CreatedAt = DateTime.UtcNow;
        await _repository.AddAsync(student);
        return _mapper.Map<StudentDto>(student);
    }
}
```

**DTOs** (Data Transfer Objects):
- `StudentDto` - Full student data (includes computed fields)
- `CreateStudentDto` - For POST requests
- `UpdateStudentDto` - For PUT requests

---

### 4. **Presentation Layer** (API Controllers)
**Location**: `Presentation/Controllers/`  
**Responsibility**: Handles HTTP requests/responses and serves as the API interface.

**Files**:
- [Presentation/Controllers/StudentsController.cs](Presentation/Controllers/StudentsController.cs) - REST API endpoints

**Key Features**:
- ✅ RESTful API design with standard HTTP methods
- ✅ Proper HTTP status codes (200, 201, 204, 400, 404, 500)
- ✅ Dependency injection of services
- ✅ Error handling and logging
- ✅ Input validation via Model State

```csharp
// Presentation Layer: REST API Controller
[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StudentDto>>> GetAllStudents() { /* ... */ }

    [HttpPost]
    public async Task<ActionResult<StudentDto>> CreateStudent([FromBody] CreateStudentDto dto) { /* ... */ }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentDto dto) { /* ... */ }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStudent(int id) { /* ... */ }
}
```

---

## 📊 API Endpoints

All endpoints follow RESTful conventions:

| Method | Endpoint | Purpose |
|--------|----------|---------|
| **GET** | `/api/students` | Get all students |
| **GET** | `/api/students/{id}` | Get a specific student |
| **POST** | `/api/students` | Create a new student |
| **PUT** | `/api/students/{id}` | Update a student |
| **DELETE** | `/api/students/{id}` | Delete a student |

---

## 🔗 Dependency Injection Configuration

**File**: [Program.cs](Program.cs#L60-L77)

All dependencies are registered in the DI container following the layer order:

```csharp
// Infrastructure Layer
builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IRepository<Student>, StudentRepository>();

// Application Layer
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateStudentValidator>();
builder.Services.AddAutoMapper(typeof(MappingProfile));
```

---

## 🎯 Separation of Concerns Benefits

| Concern | Layer | Benefit |
|---------|-------|---------|
| **Business Rules** | Domain | Core logic independent of frameworks |
| **Data Access** | Infrastructure | Easy to swap databases or ORMs |
| **Service Logic** | Application | Reusable across multiple APIs |
| **HTTP** | Presentation | Clean API endpoints |
| **Cross-Cutting** | Program.cs | DI, Logging, CORS, Middleware |

---

## 🧪 Key Design Patterns Used

### 1. **Repository Pattern**
Abstracts data access, making it easy to swap implementations.

### 2. **Dependency Injection**
Loosely coupled components, easier to test and maintain.

### 3. **Data Transfer Objects (DTOs)**
Separate domain models from API contracts.

### 4. **Service Layer Pattern**
Orchestrates business logic and coordinates between layers.

### 5. **Mapping Pattern** (AutoMapper)
Automatic transformation between entities and DTOs.

### 6. **Validation Pattern** (FluentValidation)
Centralized, reusable validation rules.

---

## 📁 Complete Folder Structure

```
StudentAssessmentTracker/
├── Domain/
│   ├── Entities/
│   │   └── Student.cs
│   └── Interfaces/
│       └── IRepository.cs
│
├── Infrastructure/
│   ├── Data/
│   │   └── ApplicationDbContext.cs
│   └── Repositories/
│       └── StudentRepository.cs
│
├── Application/
│   ├── DTOs/
│   │   └── StudentDto.cs
│   ├── Services/
│   │   └── StudentService.cs
│   ├── Validators/
│   │   └── StudentValidator.cs
│   └── Mappings/
│       └── MappingProfile.cs
│
├── Presentation/
│   └── Controllers/
│       └── StudentsController.cs
│
├── StudentApp/                  # Angular Frontend
│   ├── src/
│   │   ├── app/
│   │   ├── main.ts
│   │   └── ...
│   └── angular.json
│
├── Program.cs                   # Dependency Injection & Configuration
├── StudentAssessmentTracker.csproj
└── README.md
```

---

## 🚀 How the Data Flows

### Request Flow (Create Student):

```
1. Angular Frontend (StudentApp)
   ↓ HTTP POST /api/students
2. Presentation Layer (StudentsController)
   ↓ Validates ModelState
3. Application Layer (IStudentService)
   ↓ Maps DTO → Entity, Applies FluentValidation
4. Infrastructure Layer (StudentRepository)
   ↓ Saves to database via DbContext
5. Database
   ↓ Returns created entity with ID
6. Application Layer
   ↓ Maps Entity → DTO with calculations
7. Presentation Layer
   ↓ Returns 201 Created + StudentDto
8. Angular Frontend
   ↓ Receives full student data with totals/performance
```

---

## 🔍 Comparison: Before vs After

### Before (Monolithic)
```
Program.cs
├── Controllers/ (mixed concerns)
├── Models/ (entities + DTOs mixed)
├── Data/ (DbContext)
├── Validators/ (scattered)
└── Mappings/ (scattered)
```

### After (Multi-Layered)
```
Domain/ → Infrastructure/ → Application/ → Presentation/
   ↑            ↑              ↑              ↑
Business    Data Access    Services        HTTP
Logic       & Context       & DTOs          Endpoints
```

---

## ✅ Validation Strategy

### Level 1: Fluent Validation (Backend)
Rules in `Application/Validators/StudentValidator.cs`:
- First Name: 2-50 chars, letters/spaces/hyphens only
- Last Name: 2-50 chars, letters/spaces/hyphens only
- Email: Valid email format
- Phone: Exactly 8 digits
- Assessments: 0-20 range

### Level 2: Angular Forms (Frontend)
Reactive validation in components mirror backend rules.

### Result
- Frontend validation for immediate feedback
- Backend validation for security and data integrity

---

## 🎓 Clean Architecture Metrics

✅ **Single Responsibility**: Each layer has one reason to change  
✅ **Open/Closed Principle**: Open for extension, closed for modification  
✅ **Liskov Substitution**: Repositories implement IRepository<T>  
✅ **Interface Segregation**: Small focused interfaces  
✅ **Dependency Inversion**: Depend on abstractions, not concretions  

---

## 🔧 Extending the Architecture

### Adding a New Feature (e.g., Teachers)

1. **Domain**: Create `Teacher.cs` entity, `ITeacherRepository` interface
2. **Infrastructure**: Create `TeacherRepository`, update `DbContext`
3. **Application**: Create `TeacherService`, `TeacherDto`, `TeacherValidator`
4. **Presentation**: Create `TeachersController`
5. **Program.cs**: Register new services in DI container

Each layer is independent—changes in one don't cascade to others!

---

## 📚 Technology Stack Used

| Layer | Technology |
|-------|-----------|
| Domain | C# 12 |
| Infrastructure | Entity Framework Core 8, In-Memory Database |
| Application | AutoMapper, FluentValidation |
| Presentation | ASP.NET Core 8, C# |
| Logging | Serilog |
| Frontend | Angular 18, TypeScript |

---

## 🎯 Summary

This multi-layered architecture provides:
- ✅ **Maintainability**: Clear separation, easy to locate code
- ✅ **Scalability**: Easy to add new features
- ✅ **Testability**: Each layer can be tested independently
- ✅ **Flexibility**: Change implementations without affecting others
- ✅ **Professionalism**: Enterprise-grade design patterns
- ✅ **Type Safety**: Strong typing throughout

🚀 **Your application is now production-ready with professional architecture!**
