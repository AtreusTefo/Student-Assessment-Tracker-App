# Student Assessment Tracker - Clean Architecture

## Overview

Student Assessment Tracker follows **Clean Architecture with Separation of Concerns (SoC)** pattern, ensuring the backend API and frontend are completely decoupled and independently deployable.

## Project Structure

```
StudentAssessmentTracker/                  ← Solution Root
│
├── StudentAssessmentTrackerAPI/           ← Backend API Project
│   ├── Domain/                            ← Domain Layer (Core Business Logic)
│   │   ├── Entities/                      
│   │   │   └── Student.cs                 (Entity with business rules)
│   │   └── Interfaces/
│   │       └── IRepository.cs             (Repository contracts)
│   │
│   ├── Application/                       ← Application Layer (Use Cases)
│   │   ├── DTOs/
│   │   │   └── StudentDto.cs              (Data Transfer Objects)
│   │   ├── Services/
│   │   │   └── StudentService.cs          (Business logic orchestration)
│   │   ├── Validators/
│   │   │   └── StudentValidator.cs        (FluentValidation rules)
│   │   └── Mappings/
│   │       └── MappingProfile.cs          (AutoMapper configuration)
│   │
│   ├── Infrastructure/                    ← Infrastructure Layer (External Dependencies)
│   │   ├── Data/
│   │   │   └── ApplicationDbContext.cs    (EF Core DbContext)
│   │   └── Repositories/
│   │       └── StudentRepository.cs       (Data access implementation)
│   │
│   ├── Presentation/                      ← Presentation Layer (API Controllers)
│   │   └── Controllers/
│   │       └── StudentsController.cs      (REST API endpoints)
│   │
│   ├── Properties/
│   │   └── launchSettings.json           
│   ├── Program.cs                         ← Application entry point
│   ├── appsettings.json                   ← Configuration
│   └── StudentAssessmentTracker.csproj    ← Project file
│
├── StudentApp/                            ← Frontend Angular 21 Project
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/               (11 standalone components)
│   │   │   ├── core/
│   │   │   │   ├── guards/               (auth, guest, student-auth, student-guest, admin)
│   │   │   │   ├── interceptors/         (auth.interceptor.ts)
│   │   │   │   ├── models/               (student.model.ts, teacher.model.ts)
│   │   │   │   └── services/
│   │   │   │       ├── http/             (9 API services)
│   │   │   │       └── state/            (3 reactive state services)
│   │   │   └── features/
│   │   │       ├── students/services/    (2 business services)
│   │   │       └── teachers/services/    (1 business service)
│   │   └── assets/
│   ├── proxy.conf.json                   (Development proxy to API)
│   ├── angular.json
│   └── package.json
│
├── docs/                                  ← Documentation
│   ├── API_SETUP_TESTING_GUIDE.md
│   ├── ARCHITECTURE_IMPLEMENTATION.md
│   ├── DEVELOPER_GUIDE.md
│   ├── POSTMAN_TESTING_GUIDE.md
│   └── [other documentation files]
│
├── .gitignore
├── README.md
└── StudentAssessmentTracker.sln           ← Visual Studio Solution

```

## Architecture Layers

### 1. Domain Layer (Core)
**Location:** `StudentAssessmentTrackerAPI/Domain/`

**Purpose:** Contains the core business logic and domain entities. This layer has no dependencies on other layers.

**Components:**
- **Entities**: Core business objects with business rules and calculations
  - `Student.cs`: Student entity with assessment calculations
- **Interfaces**: Contracts for repositories and services
  - `IRepository.cs`: Generic repository interface

**Dependencies:** None (Pure .NET)

---

### 2. Application Layer (Use Cases)
**Location:** `StudentAssessmentTrackerAPI/Application/`

**Purpose:** Orchestrates business logic, implements use cases, and defines DTOs for data transfer.

**Components:**
- **DTOs**: Data Transfer Objects for API communication
  - `StudentDto.cs`, `CreateStudentDto.cs`, `UpdateStudentDto.cs`
- **Services**: Business logic orchestration
  - `StudentService.cs`, `IStudentService.cs`
- **Validators**: FluentValidation rules
  - `CreateStudentValidator.cs`, `UpdateStudentValidator.cs`
- **Mappings**: AutoMapper profiles
  - `MappingProfile.cs`

**Dependencies:** Domain Layer

---

### 3. Infrastructure Layer (External Dependencies)
**Location:** `StudentAssessmentTrackerAPI/Infrastructure/`

**Purpose:** Implements external dependencies like database access, file systems, external APIs.

**Components:**
- **Data**: Entity Framework Core DbContext
  - `ApplicationDbContext.cs`: Database configuration and mappings
- **Repositories**: Data access implementations
  - `StudentRepository.cs`: Implements `IRepository<Student>`

**Dependencies:** Domain Layer, Application Layer

---

### 4. Presentation Layer (API)
**Location:** `StudentAssessmentTrackerAPI/Presentation/`

**Purpose:** Exposes REST API endpoints, handles HTTP requests/responses.

**Components:**
- **Controllers**: API endpoints
  - `StudentsController.cs`: Student CRUD operations

**Dependencies:** Application Layer

---

### 5. Frontend Layer (Angular)
**Location:** `StudentApp/`

**Purpose:** User interface, independent SPA application running Angular 21 in zoneless mode.

**Three-tier internal structure:**
- **`components/`**: 10 standalone UI components (presentational layer). All use inline templates and delegate business logic to services.
- **`core/`**: Shared infrastructure — route guards (5), HTTP interceptor, TypeScript models/interfaces, HTTP API services (9), and reactive state services (3).
- **`features/`**: Business logic services co-located with the feature domain — `students/` (auth + CRUD business services) and `teachers/` (auth + registration business service).

**Key design decisions:**
- No `NgModule` — all components are standalone
- No `zone.js` — Angular 21 zoneless change detection
- No lazy loading — all routes eagerly loaded
- No environment files — API URLs are relative paths (`/api/...`) resolved via `proxy.conf.json` in dev
- Test runner: Vitest (not Karma/Jest)

**Communication:** HTTP/JSON to backend API on ports 5000 (HTTP) / 5001 (HTTPS)

---

## Dependency Flow

```
┌─────────────────────────────────────────────────────────────┐
│                     Angular Frontend                        │
│                    (StudentApp/)                            │
└────────────────────────┬────────────────────────────────────┘
                         │ HTTP/JSON
                         ↓
┌─────────────────────────────────────────────────────────────┐
│              Presentation Layer                             │
│           (Presentation/Controllers/)                       │
│                                                             │
│  ┌───────────────────────────────────────────────────────┐ │
│  │          Application Layer                            │ │
│  │        (Application/)                                 │ │
│  │                                                       │ │
│  │  ┌─────────────────────────────────────────────────┐ │ │
│  │  │     Infrastructure Layer                        │ │ │
│  │  │     (Infrastructure/)                           │ │ │
│  │  │                                                 │ │ │
│  │  │  ┌───────────────────────────────────────────┐ │ │ │
│  │  │  │      Domain Layer                         │ │ │ │
│  │  │  │      (Domain/)                            │ │ │ │
│  │  │  │   - Entities                              │ │ │ │
│  │  │  │   - Interfaces                            │ │ │ │
│  │  │  └───────────────────────────────────────────┘ │ │ │
│  │  │                                                 │ │ │
│  │  │  - Repositories (implements Domain interfaces) │ │ │
│  │  │  - Data Access                                 │ │ │
│  │  └─────────────────────────────────────────────────┘ │ │
│  │                                                       │ │
│  │  - Services (uses Domain & Infrastructure)           │ │
│  │  - DTOs                                               │ │
│  │  - Validators                                         │ │
│  │  - Mappings                                           │ │
│  └───────────────────────────────────────────────────────┘ │
│                                                             │
│  - Controllers (uses Application services)                 │
│  - HTTP Endpoints                                          │
└─────────────────────────────────────────────────────────────┘
```

## Key Principles

### 1. Separation of Concerns
- Each layer has a single, well-defined responsibility
- Changes in one layer don't affect others (loose coupling)

### 2. Dependency Inversion
- Outer layers depend on inner layers
- Inner layers don't depend on outer layers
- Dependencies point inward toward the Domain

### 3. Independent Deployability
- **Backend API**: Can be deployed independently as a web service
- **Frontend**: Can be deployed to a CDN or separate web server
- Communication through well-defined REST API contracts

### 4. Testability
- Each layer can be unit tested in isolation
- Mock dependencies using interfaces
- Clear separation enables comprehensive testing

## Development Workflow

### Running the Backend API

```bash
cd StudentAssessmentTrackerAPI
dotnet clean
dotnet build
dotnet run
```

API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger: `http://localhost:5000/swagger`

### Running the Frontend

```bash
cd StudentApp
npm install
npm start
```

Angular app will be available at:
- `http://localhost:4200`

The Angular proxy (configured in `proxy.conf.json`) will forward API calls to `https://localhost:5001`.

### Building for Production

**Backend:**
```bash
cd StudentAssessmentTrackerAPI
dotnet publish -c Release -o ./publish
```

**Frontend:**
```bash
cd StudentApp
npm run build
# Output: StudentApp/dist/StudentApp/browser/
```

## API Endpoints

### Students API
- `GET /api/students` - Get all students
- `GET /api/students/{id}` - Get student by ID
- `POST /api/students` - Create new student
- `PUT /api/students/{id}` - Update student
- `DELETE /api/students/{id}` - Delete student

## Technology Stack

### Backend (StudentAssessmentTrackerAPI/)
- **.NET 8.0** - Modern cross-platform framework
- **ASP.NET Core Web API** - REST API framework
- **Entity Framework Core** - ORM for data access
- **SQLite (In-Memory)** - Development database
- **FluentValidation** - Input validation
- **AutoMapper** - Object-to-object mapping
- **Serilog** - Structured logging
- **Swashbuckle (Swagger)** - API documentation

### Frontend (StudentApp/)
- **Angular 18** - Modern SPA framework
- **TypeScript** - Type-safe JavaScript
- **Standalone Components** - No modules required
- **RxJS** - Reactive programming
- **HttpClient** - HTTP communication

## Benefits of This Architecture

✅ **Maintainability**: Clear structure makes code easy to understand and modify
✅ **Scalability**: Each layer can scale independently
✅ **Testability**: Isolated layers enable comprehensive unit testing
✅ **Flexibility**: Easy to swap implementations (e.g., change database)
✅ **Reusability**: Services and components can be reused
✅ **Team Collaboration**: Teams can work on different layers simultaneously
✅ **Documentation**: Self-documenting structure
✅ **Future-Proof**: Easy to add new features without breaking existing code

## Migration Notes

This project was restructured from a mixed-layer approach to Clean Architecture:

### Before (Old Structure)
```
StudentAssessmentTracker/
├── Application/
├── Domain/
├── Infrastructure/
├── Presentation/
├── Controllers/ (legacy)
├── Models/ (legacy)
├── Program.cs
└── StudentApp/
```

### After (Clean Architecture)
```
StudentAssessmentTracker/
├── StudentAssessmentTrackerAPI/    ← Backend isolated
│   ├── Application/
│   ├── Domain/
│   ├── Infrastructure/
│   ├── Presentation/
│   ├── Controllers/ (legacy)
│   ├── Models/ (legacy)
│   └── Program.cs
├── StudentApp/                     ← Frontend isolated
└── docs/                           ← Documentation organized
```

### What Changed
1. ✅ Backend code moved to `StudentAssessmentTrackerAPI/`
2. ✅ Frontend remains in `StudentApp/`
3. ✅ Documentation organized in `docs/`
4. ✅ Solution file updated to reference new locations
5. ✅ Program.cs updated to locate Angular app from parent directory
6. ✅ Build artifacts separated per project

### Legacy Code
Legacy folders (`Controllers/`, `Models/`, `Data/`, `Validators/`, `Mappings/`) are kept for reference but are deprecated. Use the Clean Architecture layers instead.

---

## Quick Reference

| What | Where |
|------|-------|
| **Domain Entities** | `StudentAssessmentTrackerAPI/Domain/Entities/` |
| **Business Logic** | `StudentAssessmentTrackerAPI/Application/Services/` |
| **API Controllers** | `StudentAssessmentTrackerAPI/Presentation/Controllers/` |
| **Database Context** | `StudentAssessmentTrackerAPI/Infrastructure/Data/` |
| **DTOs** | `StudentAssessmentTrackerAPI/Application/DTOs/` |
| **Validation Rules** | `StudentAssessmentTrackerAPI/Application/Validators/` |
| **Angular Components** | `StudentApp/src/app/components/` |
| **Angular Services** | `StudentApp/src/app/services/` |
| **Documentation** | `docs/` |

---

**Last Updated:** March 2, 2026  
**Architecture Pattern:** Clean Architecture + Separation of Concerns  
**Status:** ✅ Production Ready
