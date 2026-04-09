# Student Assessment Tracker

Student Assessment Tracker is a **full-stack web application** built with **ASP.NET Core 8** backend and **Angular 18** frontend following **Clean Architecture** principles. It features a **dual-role system** — teachers can register, log in, and manage students with full CRUD operations and named assessment tracking; students can activate their accounts, log in, and view their own performance through a self-service dashboard. The system supports flexible scoring, file submission uploads, JWT-based authentication for both roles, automated performance calculations, and a DataTables-powered student list.

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
- **Database**: SQL Server LocalDB (EF Core Migrations)
- **Authentication**: JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`)
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

The application uses **SQL Server LocalDB** with **EF Core migrations** that are applied automatically on startup:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
```

The connection string is configured in `appsettings.Development.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=StudentAssessmentTrackerDev;Trusted_Connection=True;"
}
```

**Seeded Lookup Data (auto-seeded on first migration):**
- **Grades**: 7, 8, 9, 10, 11, 12
- **Subjects**: Accounting, Art, Business Studies, English, Geography, History, ICT, Mathematics, Multimedia, Music, Physical Education, Science

## Features

**Authentication & Security**
- Teacher registration with ID/Passport No., name, email, phone, subject (dropdown from API), and password
- Teacher and student JWT Bearer authentication (separate `Teacher` / `Student` roles with custom claims)
- HTTP interceptor auto-attaches JWT to every request and auto-redirects to login on 401
- Route guards for both roles: `authGuard`, `guestGuard`, `studentAuthGuard`, `studentGuestGuard`
- FluentValidation enforces all rules server-side; invalid payloads return HTTP 400

**Student Management (Teacher Role)**
- Add students with ID/Passport No., name, email, phone, and grade (seeded dropdown, Grades 7–12)
- Auto-generated `StudentUniqueId` (format: `STU-XXXXXXXX`) assigned on creation
- Edit student details; delete with confirmation modal and DataTable row removal
- Cascade delete removes all assessments and submissions when a student is deleted
- DataTables-powered student list with pagination, column sorting, and global search
- Colour-coded performance level badge per student on both list and detail views

**Assessment Tracking (Teacher Role)**
- Add named assessments with custom `MaxScore`, optional `DueDate`, `Instructions`, and `IsAssigned` flag
- Edit and delete individual assessments inline on the student detail page
- Performance summary: Total Score, Average Score, Percentage, and Performance Level auto-calculated server-side
  - **Needs Support**: < 50%
  - **Satisfactory**: 50–55%
  - **Good**: 56–75%
  - **Excellent**: > 75%

**File Submissions**
- Students can upload completed work (PDF, DOC, DOCX, JPG, JPEG, PNG; max 10 MB) via dashboard upload modal
- Teachers can view, download, and delete student submissions from the student detail page
- Role-based access: only the owning student may upload; only teachers may list all submissions; download and delete available to both

**Student Self-Service Portal (Student Role)**
- Students activate their account using their `StudentUniqueId` and registered email to set a password
- After login, a personal dashboard shows:
  - Performance summary cards (Total, Average, Percentage, Performance Level)
  - Colour-coded progress bar with performance band legend
  - Assessment table with Overdue/Submitted status badges
  - My Profile section (read-only personal details)
  - File upload modal per assessment

## Project Structure

```
StudentAssessmentTracker/                       ← Solution Root
│
├── StudentAssessmentTrackerAPI/                ← Backend API Project
│   ├── Domain/                                 (Core business logic)
│   │   ├── Entities/
│   │   │   ├── Student.cs
│   │   │   ├── Teacher.cs
│   │   │   ├── Grade.cs
│   │   │   ├── Subject.cs
│   │   │   ├── StudentAssessment.cs
│   │   │   ├── AssessmentSubmission.cs
│   │   │   └── TeacherStudent.cs
│   │   └── Interfaces/
│   │       └── IRepository.cs
│   │
│   ├── Application/                            (Use cases & orchestration)
│   │   ├── DTOs/
│   │   │   ├── StudentDto.cs
│   │   │   ├── TeacherDto.cs
│   │   │   ├── StudentAssessmentDto.cs
│   │   │   ├── AssessmentSubmissionDto.cs
│   │   │   ├── GradeDto.cs
│   │   │   └── SubjectDto.cs
│   │   ├── Services/
│   │   │   ├── StudentService.cs
│   │   │   ├── TeacherService.cs
│   │   │   ├── StudentAssessmentService.cs
│   │   │   └── AssessmentSubmissionService.cs
│   │   ├── Validators/
│   │   │   ├── StudentValidator.cs
│   │   │   ├── TeacherValidator.cs
│   │   │   └── StudentAssessmentValidator.cs
│   │   └── Mappings/
│   │       └── MappingProfile.cs
│   │
│   ├── Infrastructure/                         (Data access)
│   │   ├── Data/
│   │   │   └── ApplicationDbContext.cs
│   │   └── Repositories/
│   │       ├── StudentRepository.cs
│   │       ├── TeacherRepository.cs
│   │       ├── StudentAssessmentRepository.cs
│   │       └── AssessmentSubmissionRepository.cs
│   │
│   ├── Presentation/                           (REST API)
│   │   └── Controllers/
│   │       ├── StudentsController.cs
│   │       ├── TeachersController.cs
│   │       ├── StudentAssessmentsController.cs
│   │       ├── AssessmentSubmissionsController.cs
│   │       ├── GradesController.cs
│   │       └── SubjectsController.cs
│   │
│   ├── Program.cs                              (Entry point)
│   └── appsettings.json                        (Configuration)
│
├── StudentApp/                                 ← Angular Frontend
│   └── src/app/
│       ├── components/               (8 standalone components)
│       │   ├── login-form.component.ts
│       │   ├── signup-form.component.ts
│       │   ├── student-list.component.ts
│       │   ├── student-detail.component.ts
│       │   ├── student-form.component.ts
│       │   ├── student-login.component.ts
│       │   ├── student-activate.component.ts
│       │   └── student-dashboard.component.ts
│       ├── core/
│       │   ├── guards/               (auth, guest, student-auth, student-guest)
│       │   ├── interceptors/         (auth.interceptor.ts — attaches JWT, handles 401)
│       │   ├── models/               (student.model.ts, teacher.model.ts)
│       │   └── services/
│       │       ├── http/             (6 API services: teacher, student, assessments, submissions, grades, subjects)
│       │       └── state/            (teacher-state, student-state, student-auth-state)
│       └── features/
│           ├── students/services/    (student-business.service.ts, student-auth-business.service.ts)
│           └── teachers/services/    (teacher-business.service.ts)
│
├── docs/                                       ← Documentation
├── ARCHITECTURE.md
├── README.md
└── StudentAssessmentTracker.sln
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
   
   > **Note:** The Angular app uses a proxy configuration (`proxy.conf.json`) to forward `/api` requests to the backend at `http://localhost:5000`

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

All endpoints requiring authentication use **JWT Bearer** (`Authorization: Bearer <token>`).

### Teachers
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/teachers` | Public | Register a new teacher |
| `POST` | `/api/teachers/login` | Public | Login and receive JWT |
| `GET` | `/api/teachers` | Teacher JWT | List all teachers |
| `GET` | `/api/teachers/{id}` | Teacher JWT | Get teacher by ID |
| `PUT` | `/api/teachers/{id}` | Teacher JWT | Update own profile |
| `DELETE` | `/api/teachers/{id}` | Teacher JWT | Delete own account |

### Students
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/students` | Teacher JWT | List teacher's students |
| `GET` | `/api/students/{id}` | Teacher JWT | Get student detail |
| `POST` | `/api/students` | Teacher JWT | Create new student |
| `PUT` | `/api/students/{id}` | Teacher JWT | Update student |
| `DELETE` | `/api/students/{id}` | Teacher JWT | Delete student (cascade) |
| `POST` | `/api/students/activate` | Public | Activate student account |
| `POST` | `/api/students/login` | Public | Student login and receive JWT |

### Assessments
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/students/{id}/assessments` | Teacher JWT | List assessments for a student |
| `GET` | `/api/students/{id}/assessments/{aid}` | Teacher JWT | Get assessment |
| `POST` | `/api/students/{id}/assessments` | Teacher JWT | Add assessment |
| `PUT` | `/api/students/{id}/assessments/{aid}` | Teacher JWT | Update assessment |
| `DELETE` | `/api/students/{id}/assessments/{aid}` | Teacher JWT | Delete assessment |

### File Submissions
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/students/{id}/assessments/{aid}/submissions` | Student JWT | Upload submission file |
| `GET` | `/api/students/{id}/assessments/{aid}/submissions` | Teacher JWT | List submissions |
| `GET` | `/api/students/{id}/assessments/{aid}/submissions/{sid}/download` | Teacher or Student JWT | Download file |
| `DELETE` | `/api/students/{id}/assessments/{aid}/submissions/{sid}` | Teacher or Student JWT | Delete submission |

### Lookups
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/grades` | Public | List all grades (7–12) |
| `GET` | `/api/subjects` | Public | List all subjects |

## Usage

**Teacher Workflow**
1. Navigate to `/register` — fill in your details (ID/Passport No., name, email, phone, subject, password) and register
2. Navigate to `/login` — log in with your email and password to receive a JWT session
3. The student list (`/`) shows all your students with DataTables sorting, search, and pagination
4. Click **Create Student** to add a new student; a unique `StudentUniqueId` is generated automatically
5. Click a student’s name to open the detail page with full assessment history and file submissions
6. Add, edit, or delete assessments inline; performance summary updates immediately
7. Download or delete student file submissions from the detail page

**Student Workflow**
1. Navigate to `/student/login` and click the activation tab
2. Enter your `StudentUniqueId` (provided by your teacher), registered email, and choose a password
3. After activation, log in with your `StudentUniqueId` and password
4. Your dashboard shows performance summary cards, assessments table, and your profile
5. Upload completed work using the file upload modal next to any assessment

## Form Validation Rules

**Teacher Registration:**
- **ID/Passport No.**: Required, exactly 9 alphanumeric characters (letters and digits only)
- **First / Last Name**: Required, 2–50 characters, letters/spaces/hyphens only
- **Email**: Required, valid email format, unique
- **Phone**: Required, exactly 8 digits
- **Subject**: Required, selected from API-seeded dropdown
- **Password**: Required, 6–20 characters

**Student Form (Create/Edit):**
- **ID/Passport No.**: Required, 9 characters
- **First / Last Name**: Required, 2–50 characters
- **Email**: Required, valid email format, unique
- **Phone**: Required, exactly 8 digits
- **Grade**: Required, must select a valid grade from the dropdown (`GradeId > 0`)

**Assessment Form:**
- **Name**: Required, max 100 characters
- **Max Score**: Required, must be > 0
- **Score**: Required, must be ≥ 0 and ≤ MaxScore

**Student Activation:**
- **Student ID**: Required, must match `STU-XXXXXXXX` format
- **Email**: Required, valid email format
- **Password**: Required, minimum 6 characters
- **Confirm Password**: Must exactly match password

Validation occurs both on the frontend (real-time, template-driven forms) and backend (FluentValidation, HTTP 400 on failure).

## Known Limitations

- Phone number validation accepts exactly 8 digits (no international format support)
- Student profile details are read-only from the student dashboard; only teachers can update student records
- File submissions are stored as raw byte arrays in the database (no external blob storage)
- No email notifications for assessment assignments or new submissions
- No audit log of data changes (create/update/delete history)

## Future Enhancements

- Email notifications for assignment creation and submission deadlines
- Data export (CSV, PDF) for student reports
- Class and subject grouping for assessment management
- Admin role for managing all teachers and students
- Audit logging of all create/update/delete operations

## Development Notes

### Adding New Features

1. **Backend API**: Add entity, DTO, validator, service, repository, and controller
2. **Migrations**: Run `dotnet ef migrations add <MigrationName>` and `dotnet ef database update`
3. **Frontend**: Create Angular component, wire HTTP service, update routes and guards
4. **Build**: Rebuild Angular with `npm run build` if serving from wwwroot

### Debugging

- Backend: Set breakpoints in Visual Studio or VS Code with C# extension
- Frontend: Use browser DevTools (F12) and Angular DevTools extension
- Logs: Check terminal output for error messages

For detailed documentation, see the `docs/` folder in the project root.
