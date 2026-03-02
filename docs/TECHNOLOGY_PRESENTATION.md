# Student Assessment Tracker - Technology Stack Presentation

## 1. FluentValidation & AutoMapper

### What They Are
- **FluentValidation**: A .NET library that provides a clean, fluent interface for building validation rules
- **AutoMapper**: A .NET library that automatically maps data between objects (e.g., from database entities to DTOs)

### Where Used in Code
**FluentValidation** - [Application/Validators/StudentValidator.cs](Application/Validators/StudentValidator.cs)
```csharp
public class CreateStudentValidator : AbstractValidator<CreateStudentDto>
{
    public CreateStudentValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .Length(2, 50).WithMessage("First name must be 2-50 characters");
        
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress().WithMessage("Email must be valid");
    }
}
```

**AutoMapper** - [Application/Mappings/MappingProfile.cs](Application/Mappings/MappingProfile.cs)
```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Maps Student entity to StudentDto automatically
        CreateMap<Student, StudentDto>()
            .ForMember(dest => dest.TotalScore, opt => opt.MapFrom(src => src.GetTotalScore()));
        
        CreateMap<CreateStudentDto, Student>();
    }
}
```

**Registered in** [Program.cs](Program.cs) - Lines 30-45

---

## 2. Data Transfer Objects (DTOs)

### What They Are
Objects designed to transfer data between layers. They expose only the data needed for a specific use case, hiding internal business logic.

### Where Used in Code
**StudentDto Variants** - [Application/DTOs/StudentDto.cs](Application/DTOs/StudentDto.cs)

```csharp
// For GET responses - shows all fields
public class StudentDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public decimal Assessment1 { get; set; }
    public decimal TotalScore { get; set; }  // Calculated field
    public decimal AverageScore { get; set; }  // Calculated field
}

// For POST requests - only input fields
public class CreateStudentDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    // No Id, no calculated fields
}

// For PUT requests - similar to Create
public class UpdateStudentDto
{
    // Fields that can be updated
}
```

**Why DTOs?**
- ✅ Security: Hide internal database fields
- ✅ Flexibility: Different shapes for different operations
- ✅ Performance: Only send necessary data
- ✅ Decoupling: API contract doesn't depend on database schema

---

## 3. Serilog Logging Framework

### What It Is
A .NET logging library that structures logs and sends them to multiple destinations (files, console, etc.)

### Where Used in Code
**Configured in** [Program.cs](Program.cs) - Lines 32-38

```csharp
builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});
```

**Used Throughout Controllers**
```csharp
_logger.LogInformation("Fetching all students with sort order: {SortOrder}", sortOrder);
_logger.LogError("Error creating student: {Error}", ex.Message);
```

**Logs Destination**: `~/Logs/` folder (created automatically)

**Benefits:**
- 📝 Structured logging (JSON format)
- 📂 Automatic file rotation
- 🔍 Easy debugging and monitoring
- 📊 Production diagnostics

---

## 4. Datatables

### What It Is
A JavaScript library that adds interactive features to HTML tables: sorting, filtering, pagination and search.

### Where Used in Code
**Package.json Dependency** - [StudentApp/package.json](StudentApp/package.json)
```json
{
  "dependencies": {
    "datatables.net": "^2.3.7",
    "datatables.net-dt": "^2.3.7"
  }
}
```

### How It Works
1. Frontend fetches student data from API (Application Programming Interface)
2. Datatables renders it in an interactive table
3. Users can:
   - 📊 Sort by any column (Grade, Score, etc.)
   - 🔍 Search/filter students
   - 📄 View data in pages
   - 📥 Export to CSV/Excel

**Frontend Integration**: Used in Angular components to display the student grid

---

## 5. Angular Frontend Tool

### What It Is
A modern JavaScript framework for building interactive web applications. It handles the user interface and communication with the backend API.

### Where Used in Code
**Main Application** - [StudentApp/](StudentApp/)

**Project Structure:**
```
StudentApp/
├── src/
│   ├── main.ts          (Application entry point)
│   ├── index.html       (Main page)
│   └── app/             (Components, services, etc.)
├── package.json         (Dependencies)
├── angular.json         (Configuration)
└── tsconfig.json        (TypeScript settings)
```

**Key Technologies:**
- 📱 Components: Reusable UI pieces
- 🔄 Services: Handle API communication
- 🎨 Routing: Navigate between pages
- 📋 Forms: Handle user input

**Frontend to Backend Communication:**
```
Angular App → HTTP Requests → ASP.NET Core API → Database
(StudentApp)                  (Port 5000)
```

---

## 6. Multilayered Architecture

### What It Is
A software design pattern that separates code into distinct layers, each with a specific responsibility.

### The Four Layers in Your Project

#### **Layer 1: Presentation Layer** 📱
Shows data to users and collects input
- Location: [Presentation/Controllers/](Presentation/Controllers/)
- Handles HTTP requests/responses
- Returns DTOs to frontend

#### **Layer 2: Application Layer** 🔧
Contains business logic and validation rules
- Location: [Application/](Application/)
- Contains:
  - **Services** - Business logic (calculate grades, averages)
  - **Validators** - FluentValidation rules
  - **DTOs** - Data transfer objects
  - **Mappings** - AutoMapper profiles

#### **Layer 3: Domain Layer** 💼
Defines core business entities and interfaces
- Location: [Domain/](Domain/)
- Contains:
  - **Entities** - Student, Teacher classes
  - **Interfaces** - IRepository contract

#### **Layer 4: Infrastructure Layer** 🗄️
Handles data access and external services
- Location: [Infrastructure/](Infrastructure/)
- Contains:
  - **DbContext** - Database communication
  - **Repositories** - Data access logic
  - Implements Domain interfaces

```
REQUEST FLOW:
───────────────────────────────────────────────
Angular App
    ↓ HTTP
Presentation Layer (StudentsController)
    ↓
Application Layer (StudentService, StudentValidator)
    ↓
Domain Layer (Student Entity, IRepository)
    ↓
Infrastructure Layer (StudentRepository, DbContext)
    ↓
Database
───────────────────────────────────────────────
```

**Benefits:**
- ✅ Separation of concerns (each layer has one job)
- ✅ Testability (can test each layer independently)
- ✅ Maintainability (easy to find and modify code)
- ✅ Scalability (easy to add new features)

---

## 7. Swagger UI

### What It Is
An interactive API documentation tool. It reads your API code and automatically generates documentation that developers can test directly in the browser.

### Where Used in Code
**Configured in** [Program.cs](Program.cs) - Lines 48-64

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Student Assessment Tracker API",
        Version = "v1",
        Description = "REST API for Student Assessment Tracking System"
    });

    // Include XML documentation
    var xmlFile = Path.Combine(AppContext.BaseDirectory, "StudentAssessmentTracker.xml");
    if (File.Exists(xmlFile))
    {
        options.IncludeXmlComments(xmlFile);
    }
});
```

### Access It
When your application runs:
- 🔗 Visit: `http://localhost:5000/swagger/ui`
- 📖 See all API endpoints
- ✅ Test requests directly
- 📋 View response models

**What It Shows:**
```
GET    /api/students          - Fetch all students
POST   /api/students          - Create a student
GET    /api/students/{id}     - Get one student
PUT    /api/students/{id}     - Update a student
DELETE /api/students/{id}     - Delete a student
```

---

## 8. Postman

### What It Is
A tool for testing APIs. Instead of writing code, you click buttons to send requests and see responses.

### How It's Used
**Postman Collection** - [StudentAssessmentTracker.postman_collection.json](StudentAssessmentTracker.postman_collection.json)

### Testing Workflow
1. 📝 Create a request (GET, POST, PUT, DELETE)
2. 🎯 Enter the endpoint URL: `http://localhost:5000/api/students`
3. 📤 Add request body (JSON):
   ```json
   {
     "firstName": "John",
     "lastName": "Doe",
     "email": "john@example.com",
     "assessment1": 18,
     "assessment2": 19,
     "assessment3": 17
   }
   ```
4. 🚀 Click "Send"
5. 👀 View the response (with status code, headers, body)

### Benefits
- ✅ Manual API testing before frontend is ready
- ✅ Debug issues without running full application
- ✅ Share test collection with team
- ✅ Automate API tests

---

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────┐
│                   ANGULAR FRONTEND                      │
│              (StudentApp - TypeScript)                   │
│                                                          │
│  Components → Services → Datatables (Display)          │
└────────────────────┬────────────────────────────────────┘
                     │ HTTP Requests/Responses
                     ↓
┌─────────────────────────────────────────────────────────┐
│           PRESENTATION LAYER (Controllers)              │
│     StudentsController.cs, TeacherController.cs         │
└────────────────────┬────────────────────────────────────┘
                     │
                     ↓
┌─────────────────────────────────────────────────────────┐
│           APPLICATION LAYER (Services)                  │
│  Validators (FluentValidation)                          │
│  Mappings (AutoMapper)                                  │
│  Services (Business Logic)                              │
│  DTOs (Data Transfer Objects)                           │
└────────────────────┬────────────────────────────────────┘
                     │
                     ↓
┌─────────────────────────────────────────────────────────┐
│            DOMAIN LAYER (Entities)                      │
│  Student.cs, Teacher.cs                                 │
│  IRepository (Interface)                                │
└────────────────────┬────────────────────────────────────┘
                     │
                     ↓
┌─────────────────────────────────────────────────────────┐
│        INFRASTRUCTURE LAYER (Data Access)               │
│  StudentRepository.cs                                   │
│  ApplicationDbContext.cs (Entity Framework)             │
└────────────────────┬────────────────────────────────────┘
                     │
                     ↓
                ┌─────────┐
                │ DATABASE│
                └─────────┘

TOOLS & FEATURES:
├─ Serilog: Logs all operations to ~/Logs/
├─ Swagger UI: http://localhost:5000/swagger/ui
└─ Postman: Test API endpoints before frontend
```

---

## Summary Table

| Technology | Purpose | Location |
|-----------|---------|----------|
| **FluentValidation** | Validate user input | `Application/Validators/` |
| **AutoMapper** | Map entities to DTOs | `Application/Mappings/` |
| **DTOs** | Transfer data between layers | `Application/DTOs/` |
| **Serilog** | Log application events | `Program.cs` → `Logs/` |
| **Datatables** | Interactive table UI | `StudentApp/package.json` |
| **Angular** | Frontend web app | `StudentApp/src/` |
| **Multilayered Architecture** | Organize code by responsibility | Presentation → Application → Domain → Infrastructure |
| **Swagger** | API documentation & testing | `Program.cs` → `/swagger/ui` |
| **Postman** | Manual API testing | `StudentAssessmentTracker.postman_collection.json` |

---

## How To Use Everything Together

### 1. **Start the Backend**
   - Runs ASP.NET Core API on port 5000
   - Serilog logs all activity

### 2. **Start the Frontend**
   - Angular app on port 4200
   - Makes HTTP calls to port 5000

### 3. **Test with Swagger**
   - Visit `/swagger/ui` to test each endpoint
   - See what data is sent/received

### 4. **Verify with Postman**
   - Import the collection
   - Test each endpoint manually

### 5. **Frontend Communication**
   - Angular Services call the API
   - ValidationRules check input
   - AutoMapper converts responses
   - Datatables displays the results
   - Serilog tracks everything

---

**Everything works together to create a modern, professional, scalable application!** 🚀
