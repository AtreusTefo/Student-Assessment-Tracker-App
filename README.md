# Student Assessment Tracker

Student Assessment Tracker is a **full-stack web application** built with **ASP.NET Core 8** backend and **Angular 18** frontend following **Clean Architecture** principles. It provides an intuitive interface to manage and track student assessments, allowing teachers to add, edit, delete, and view student scores with automatic calculations for totals, averages, percentages, and performance levels.

## Architecture Overview

This system follows **Clean Architecture with Separation of Concerns (SoC)** pattern, ensuring the backend API and frontend are completely decoupled and independently deployable.

### Project Structure

```
StudentAssessmentTracker/                  ← Solution Root
│
├── StudentAssessmentTrackerAPI/           ← Backend API (Clean Architecture)
│   ├── Domain/                            (Core business logic)
│   ├── Application/                       (Use cases & services)
│   ├── Infrastructure/                    (Data access & external dependencies)
│   ├── Presentation/                      (REST API controllers)
│   └── Program.cs
│
├── StudentApp/                            ← Frontend Angular 18 SPA
│   └── src/app/
│
├── docs/                                  ← Documentation
└── ARCHITECTURE.md                        ← Detailed architecture guide

**Backend** – ASP.NET Core 8 Web API (Clean Architecture)
- **Domain Layer**: Entities with business rules
- **Application Layer**: Services, DTOs, Validators, AutoMapper
- **Infrastructure Layer**: EF Core, Repositories
- **Presentation Layer**: REST API Controllers
- FluentValidation for input validation
- Serilog for structured logging
- Swagger for API documentation

**Frontend** – Angular 18 (Standalone Components)
- Reactive forms with comprehensive validation
- RxJS Observables for async operations
- Angular routing with guards
- HTTP communication with backend API
- Responsive UI with modern CSS

**Why Clean Architecture:**
- **Separation of Concerns**: Each layer has a single responsibility
- **Independence**: Frontend and backend are fully decoupled
- **Testability**: Each layer can be tested in isolation
- **Maintainability**: Clear structure makes code easy to understand
- **Scalability**: Layers can scale independently
- **Flexibility**: Easy to swap implementations without affecting other layers

**For detailed architecture information, see [ARCHITECTURE.md](ARCHITECTURE.md)**

## Technology Stack

### Backend (StudentAssessmentTrackerAPI/)
- **Runtime**: .NET 8.0
- **Framework**: ASP.NET Core Web API
- **ORM**: Entity Framework Core 8.0
- **Database**: In-Memory (Development)
- **Validation**: FluentValidation 12.1
- **Mapping**: AutoMapper 12.0
- **Logging**: Serilog 8.0
- **API Docs**: Swashbuckle (Swagger)

### Frontend (StudentApp/)
- **Framework**: Angular 18
- **Language**: TypeScript 5
- **Reactive**: RxJS
- **HTTP**: Angular HttpClient
- **Routing**: Angular Router
- **Build**: Angular CLI

## Database

The application uses **Entity Framework Core In-Memory Database**:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("StudentDb"));
```

**Benefits:**
- Zero setup required; no SQL Server or SQLite installation
- Perfect for development, testing, and learning
- Data resets when the application restarts
- Full support for LINQ queries

## Features

**Student Management**
- Add new students with assessments (scores 0-20)
- Edit student information
- Delete students with confirmation
- View all students in a table

**Data Validation**
- Frontend validation (name format, email, phone)
- Backend validation with FluentValidation
- Real-time error messages
- User-friendly form feedback

**Automatic Calculations**
- Total Score: Sum of 3 assessments
- Average Score: Total ÷ 3
- Percentage: (Total ÷ 60) × 100
- Performance Level:
  - **Needs Support**: < 50%
  - **Satisfactory**: 50-55%
  - **Good**: 56-75%
  - **Excellent**: > 75%

**User Interface**
- Responsive design
- Student list with sorting
- Interactive forms
- Real-time validation feedback
- Loading states and error handling

## Project Structure

```
StudentAssessmentTracker/                       ← Solution Root
│
├── StudentAssessmentTrackerAPI/                ← Backend API Project
│   ├── Domain/                                 (Core business logic)
│   │   ├── Entities/
│   │   │   └── Student.cs
│   │   └── Interfaces/
│   │       └── IRepository.cs
│   │
│   ├── Application/                            (Use cases & orchestration)
│   │   ├── DTOs/
│   │   │   └── StudentDto.cs
│   │   ├── Services/
│   │   │   └── StudentService.cs
│   │   ├── Validators/
│   │   │   └── StudentValidator.cs
│   │   └── Mappings/
│   │       └── MappingProfile.cs
│   │
│   ├── Infrastructure/                         (Data access)
│   │   ├── Data/
│   │   │   └── ApplicationDbContext.cs
│   │   └── Repositories/
│   │       └── StudentRepository.cs
│   │
│   ├── Presentation/                           (REST API)
│   │   └── Controllers/
│   │       └── StudentsController.cs
│   │
│   ├── Program.cs                              (Entry point)
│   └── appsettings.json                        (Configuration)
│
├── StudentApp/                                 ← Angular Frontend
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/
│   │   │   │   ├── student-form.component.ts
│   │   │   │   ├── student-list.component.ts
│   │   │   │   └── student-detail.component.ts
│   │   │   ├── services/
│   │   │   │   └── student.service.ts
│   │   │   ├── models/
│   │   │   │   └── student.model.ts
│   │   │   └── app.routes.ts
│   │   └── main.ts
│   ├── angular.json
│   ├── package.json
│   ├── proxy.conf.json                         (Dev proxy to API)
│   └── dist/                                   (Build output)
│
├── docs/                                       ← Documentation
│   ├── API_SETUP_TESTING_GUIDE.md
│   ├── DEVELOPER_GUIDE.md
│   └── POSTMAN_TESTING_GUIDE.md
│
├── ARCHITECTURE.md                             ← Architecture details
├── README.md                                   ← This file
└── StudentAssessmentTracker.sln                ← Visual Studio Solution
```

## Setup Instructions

### Prerequisites
- .NET 8 SDK ([Download](https://dotnet.microsoft.com/download))
- Node.js 18+ with npm ([Download](https://nodejs.org/))
- VS Code or Visual Studio 2022

> **Windows Note:** After installing Node.js, if you get `npm is not recognized` errors, Node.js may not be in your PATH. Fix it by either:
> - **Restarting your terminal** (picks up the updated PATH automatically), or
> - **Adding it manually for the current session:**
>   ```powershell
>   $env:PATH = "C:\Program Files\nodejs;" + $env:PATH
>   ```
> - **Making it permanent** (run PowerShell as Administrator):
>   ```powershell
>   [System.Environment]::SetEnvironmentVariable("PATH", "C:\Program Files\nodejs;" + [System.Environment]::GetEnvironmentVariable("PATH", "Machine"), "Machine")
>   ```
>   Then restart your terminal.

### Backend Setup (API)

1. **Navigate to the API project**
   ```bash
   cd StudentAssessmentTrackerAPI
   ```

2. **Restore .NET packages**
   ```bash
   dotnet restore
3. **Build the project**
   ```bash
   dotnet build
   ```

4. **Run the API**
   ```bash
   dotnet run
   ```
   
   The API will start on:
   - HTTP: `http://localhost:5000`
   - HTTPS: `https://localhost:5001`
   - Swagger: `http://localhost:5000/swagger`

### Frontend Setup (Angular)

1. **Navigate to the StudentApp directory**
   ```bash
   cd StudentApp
   ```

2. **Install npm dependencies**
   ```bash
   npm install
   ```

3. **Run the Angular development server**
   ```bash
   npm start
   ```
   
   The application will start on `http://localhost:4200`
   
   > **Note:** The Angular app uses a proxy configuration to forward API requests to the backend at `https://localhost:5001`

4. **Build for production** (optional)
   ```bash
   npm run build
   ```
   
   Output will be in `dist/StudentApp/browser/`
   
   This creates optimized files in `dist/StudentApp/browser/`

5. **Copy built files to wwwroot**
   ```bash
   copy dist/StudentApp/browser/* ../wwwroot/ /Y
   ```

## Running the Application

### Start Backend Only (Frontend is served from wwwroot)
```bash
dotnet run
```

Open your browser and navigate to: **http://localhost:5000**

### Develop Frontend (with live reload)
```bash
cd StudentApp
npm start
```

This runs Angular in development mode with live reloading. The frontend proxies API calls to the backend.

## API Endpoints

The backend provides the following REST API endpoints:

- `GET /api/students` – Get all students
- `GET /api/students/{id}` – Get a specific student
- `POST /api/students` – Create a new student
- `PUT /api/students/{id}` – Update a student
- `DELETE /api/students/{id}` – Delete a student

All endpoints return JSON responses and validate input using FluentValidation.

## Usage

1. **View Students** – Home page displays a table of all students
2. **Add Student** – Click "Create Student" button to add a new student
3. **Edit Student** – Click "Edit" on a student row to modify their information
4. **View Details** – Click on a student name to see full details including performance level
5. **Delete Student** – Click "Delete" to remove a student (confirmation required)

## Form Validation Rules

**Student Form:**
- **First Name**: Required, 2-50 characters, letters/spaces/hyphens only
- **Last Name**: Required, 2-50 characters, letters/spaces/hyphens only
- **Email**: Required, valid email format
- **Phone**: Required, exactly 8 digits
- **Grade**: Required, text format (e.g., 10A, 11B)
- **Assessments**: Required, values between 0-20

Validation occurs both on the frontend (real-time) and backend (API submission).

## Known Limitations

- In-memory database clears all data when the application stops
- Single-user application (no user authentication)
- Phone number validation requires 8 digits (no international format support)
- No audit logging of changes

## Future Enhancements

- Replace In-Memory database with SQL Server/PostgreSQL
- Add user authentication and authorization
- Implement audit logging for student changes
- Add data export (CSV, PDF)
- Student performance analytics and reporting
- Class and subject categorization

## Development Notes

### Adding New Features

1. **Backend API**: Add controller methods and DTOs
2. **Validation**: Add rules in FluentValidation validators
3. **Frontend**: Create Angular components and services
4. **Build**: Rebuild Angular with `npm run build` and copy to wwwroot

### Debugging

- Backend: Set breakpoints in Visual Studio or VS Code with C# extension
- Frontend: Use browser DevTools (F12) and Angular DevTools extension
- Logs: Check terminal output for error messages

## Recent Fixes

This project has been updated to resolve the following issues:

- **Phone Validation Duplicate Error** – Fixed template to show only one error message
- **Application Blank Screen** – Fixed Angular build deployment to wwwroot root directory
- **Form Change Detection** – Added ChangeDetectorRef for proper async handling in Angular
- **API Response Type Mismatch** – Implemented proper DTO mapping with AutoMapper
- **Validation Error Handling** – Separated frontend and backend validation concerns

For detailed information about fixes and known issues, see the documentation files in the project root.
