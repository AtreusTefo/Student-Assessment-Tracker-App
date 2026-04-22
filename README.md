# Student Assessment Tracker

Student Assessment Tracker is a **full-stack web application** built with **ASP.NET Core 8** backend and **Angular 21** frontend following **Clean Architecture** principles. It features a **three-role system** — admins manage the entire platform (create teachers and students, assign teachers, view audit logs); teachers manage assessments for their assigned students; students activate their accounts, log in, and view their own performance through a self-service dashboard. The system supports flexible scoring, file submission uploads, JWT-based authentication for all three roles, automated performance calculations, email notifications, CSV/PDF exports, and a DataTables-powered student list.

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
├── StudentApp/                            ← Frontend Angular 21 SPA
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

**Frontend** – Angular 21 (Standalone Components, Zoneless)
- Reactive forms with comprehensive validation
- RxJS Observables for async operations
- Angular routing with guards
- HTTP communication with backend API
- Responsive UI with modern CSS
- DataTables.net v2 with Buttons plugin for CSV export

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
- **CSV Parsing**: CsvHelper 33.0.1

### Frontend (StudentApp/)
- **Framework**: Angular 21
- **Language**: TypeScript 5.9
- **Reactive**: RxJS 7.8
- **HTTP**: Angular HttpClient (`withFetch()`, function interceptors)
- **Routing**: Angular Router
- **Build**: Angular CLI 21 (`@angular/build:application`)
- **Tests**: Vitest 4
- **Tables**: DataTables.net v2 + Buttons plugin

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
- Admin creates teacher accounts; teachers activate their own accounts at `/activate` by setting a password
- Admin creates student records; students activate their own accounts at `/student/login` using their `StudentUniqueId`
- Three separate JWT roles: `Teacher`, `Student`, and `Admin` with custom claims
- BCrypt password hashing for all stored credentials
- HTTP interceptor auto-attaches JWT to every request and auto-redirects to login on 401
- Route guards for all three roles: `authGuard`, `guestGuard`, `studentAuthGuard`, `studentGuestGuard`, `adminAuthGuard`, `adminGuestGuard`
- FluentValidation enforces all rules server-side; invalid payloads return HTTP 400

**Student Management (Admin Role)**
- Admin creates students with ID/Passport No., name, email, phone, and grade (seeded dropdown, Grades 7–12)
- Auto-generated `StudentUniqueId` (format: `STU-XXXXXXXX`) assigned on creation
- Admin edits student details and deletes students with cascade removal of all assessments and submissions
- Admin assigns and unassigns teachers to students via timetabling endpoints

**Student List (Teacher Role)**
- DataTables-powered list of students assigned to the authenticated teacher, with pagination, column sorting, and global search
- Colour-coded performance level badge per student on both list and detail views

**Assessment Tracking (Teacher Role)**
- Add named assessments with custom `MaxScore`, optional `DueDate`, `Instructions`, and `IsAssigned` flag
- Edit and delete individual assessments inline on the student detail page
- **Bulk create** multiple assessments for a student in a single request (`POST /api/assessments/bulk`)
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

**Admin Panel**
- Dedicated admin account (BCrypt-hashed password, separate `Admin` JWT role)
- Admin login page at `/admin/login` with its own authentication guard (accessible via `/admin` redirect)
- Admin dashboard with tabs: manage Teachers, manage Students, view Audit Log
- Admins create teacher accounts (without a password) and student records
- Admins **edit** teacher and student records via inline edit modals on the dashboard
- Admins assign teachers to students and manage teacher–student relationships
- Admins can delete any teacher or student system-wide with confirmation modal
- Admin can create additional admin accounts (requires existing Admin JWT)
- **Bulk Import**: Admin can import up to 500 students or teachers in a single operation via CSV file upload, paste-in CSV text, or direct JSON; per-row validation with a result table showing which rows succeeded or failed
- `IsActive` badge displayed on each teacher card — derived from whether the teacher has activated their account

**Audit Logging**
- Every Create, Update, and Delete across Students, Teachers, and Assessments emits an immutable audit entry
- Audit records store entity name, entity ID, action, old values (JSON), new values (JSON), actor, role, and timestamp
- Admins can browse paginated audit log and filter by entity type / entity ID from the dashboard

**Email Notifications**
- MailKit SMTP integration sends an email to the student whenever a new assessment is created for them
- Email delivery is fire-and-forget and gracefully no-ops when `Email:SmtpHost` is not configured

**Data Export (CSV & PDF)**
- Teachers can export the full student list to CSV from the Student List page
- On any student detail page, teachers can download that student's individual report as CSV or PDF
- PDF reports include styled header, personal info, and colour-coded assessment table (QuestPDF community license)

**Class Groups**
- Teachers can create named class groups linked to a subject, grade, and their own teacher account
- Students can be enrolled and unenrolled from class groups
- REST endpoints at `/api/class-groups` protected by `Teacher` role

## Project Structure

```
StudentAssessmentTracker/                       ← Solution Root
│
├── StudentAssessmentTrackerAPI/                ← Backend API Project
│   ├── Domain/                                 (Core business logic)
│   │   ├── Entities/
│   │   │   ├── Admin.cs
│   │   │   ├── AuditLog.cs
│   │   │   ├── AssessmentSubmission.cs
│   │   │   ├── ClassGroup.cs
│   │   │   ├── ClassGroupStudent.cs
│   │   │   ├── Grade.cs
│   │   │   ├── Student.cs
│   │   │   ├── StudentAssessment.cs
│   │   │   ├── Subject.cs
│   │   │   ├── Teacher.cs
│   │   │   └── TeacherStudent.cs
│   │   └── Interfaces/
│   │       └── IRepository.cs
│   │
│   ├── Application/                            (Use cases & orchestration)
│   │   ├── DTOs/
│   │   │   ├── AdminDto.cs
│   │   │   ├── AssessmentSubmissionDto.cs
│   │   │   ├── AuditLogDto.cs
│   │   │   ├── ClassGroupDto.cs
│   │   │   ├── GradeDto.cs
│   │   │   ├── StudentAssessmentDto.cs
│   │   │   ├── StudentDto.cs
│   │   │   ├── SubjectDto.cs
│   │   │   └── TeacherDto.cs
│   │   ├── Services/
│   │   │   ├── AdminService.cs
│   │   │   ├── AssessmentSubmissionService.cs
│   │   │   ├── AuditLogService.cs
│   │   │   ├── ClassGroupService.cs
│   │   │   ├── EmailService.cs
│   │   │   ├── ExportService.cs
│   │   │   ├── StudentAssessmentService.cs
│   │   │   ├── StudentService.cs
│   │   │   └── TeacherService.cs
│   │   ├── Validators/
│   │   │   ├── StudentAssessmentValidator.cs
│   │   │   ├── StudentValidator.cs
│   │   │   └── TeacherValidator.cs
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
│   │       ├── AdminsController.cs
│   │       ├── AssessmentSubmissionsController.cs
│   │       ├── ClassGroupsController.cs
│   │       ├── GradesController.cs
│   │       ├── ReportsController.cs
│   │       ├── StudentAssessmentsController.cs
│   │       ├── StudentsController.cs
│   │       ├── SubjectsController.cs
│   │       └── TeachersController.cs
│   │
│   ├── Program.cs                              (Entry point)
│   └── appsettings.json                        (Configuration)
│
├── StudentApp/                                 ← Angular 21 Frontend
│   └── src/app/
      ├── components/               (11 standalone components)
      │   ├── login-form.component.ts
      │   ├── signup-form.component.ts
      │   ├── student-list.component.ts
      │   ├── student-detail.component.ts
      │   ├── student-form.component.ts
      │   ├── student-login.component.ts       (login + activation dual-mode)
      │   ├── student-activate.component.ts
      │   ├── student-dashboard.component.ts
      │   ├── teacher-dashboard.component.ts
│       │   ├── admin-login.component.ts
│       │   └── admin-dashboard.component.ts
│       ├── core/
│       │   ├── guards/               (auth, guest, student-auth, student-guest, admin)
│       │   ├── interceptors/         (auth.interceptor.ts — attaches JWT, handles 401)
│       │   ├── models/               (student.model.ts, teacher.model.ts)
│       │   └── services/
│       │       ├── http/             (9 API services: teacher, student, assessments, submissions,
│       │       │                      grades, subjects, reports, admin, class-groups)
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
| `POST` | `/api/teachers` | Admin JWT | Create a new teacher account (admin only) |
| `POST` | `/api/teachers/activate` | Public | Activate teacher account and set password |
| `POST` | `/api/teachers/login` | Public | Login and receive JWT |
| `POST` | `/api/teachers/forgot-password` | Public | Reset password (nulls password, teacher re-activates) |
| `GET` | `/api/teachers` | Teacher JWT | List all teachers |
| `GET` | `/api/teachers/{id}` | Teacher JWT | Get teacher by ID |
| `PUT` | `/api/teachers/{id}` | Teacher JWT | Update own profile |
| `DELETE` | `/api/teachers/{id}` | Teacher JWT | Delete own account |

### Students
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/students` | Teacher JWT | List students assigned to the authenticated teacher |
| `GET` | `/api/students/{id}` | Teacher JWT | Get student detail |
| `POST` | `/api/students` | Admin JWT | Create new student (admin only) |
| `PUT` | `/api/students/{id}` | Admin JWT | Update student (admin only) |
| `DELETE` | `/api/students/{id}` | Admin JWT | Delete student with cascade (admin only) |
| `POST` | `/api/students/{sid}/teachers/{tid}` | Admin JWT | Assign a teacher to a student |
| `DELETE` | `/api/students/{sid}/teachers/{tid}` | Admin JWT | Remove a teacher from a student |
| `POST` | `/api/students/activate` | Public | Activate student account |
| `POST` | `/api/students/login` | Public | Student login and receive JWT |
| `POST` | `/api/students/forgot-password` | Public | Reset password (nulls password, student re-activates) |

### Assessments
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/students/{id}/assessments` | Teacher JWT | List assessments for a student |
| `GET` | `/api/students/{id}/assessments/{aid}` | Teacher JWT | Get assessment |
| `POST` | `/api/students/{id}/assessments` | Teacher JWT | Add assessment |
| `PUT` | `/api/students/{id}/assessments/{aid}` | Teacher JWT | Update assessment |
| `DELETE` | `/api/students/{id}/assessments/{aid}` | Teacher JWT | Delete assessment |
| `POST` | `/api/assessments/bulk` | Teacher JWT | Bulk-create multiple assessments for a student |

### File Submissions
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/students/{id}/assessments/{aid}/submissions` | Student JWT | Upload submission file |
| `GET` | `/api/students/{id}/assessments/{aid}/submissions` | Teacher JWT | List submissions |
| `GET` | `/api/students/{id}/assessments/{aid}/submissions/{sid}/download` | Teacher or Student JWT | Download file |
| `DELETE` | `/api/students/{id}/assessments/{aid}/submissions/{sid}` | Teacher or Student JWT | Delete submission |

### Reports
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/reports/students/csv` | Teacher JWT | Export all teacher's students to CSV |
| `GET` | `/api/reports/students/{id}/csv` | Teacher JWT | Export one student's report to CSV |
| `GET` | `/api/reports/students/{id}/pdf` | Teacher JWT | Export one student's report to PDF |

### Class Groups
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/class-groups` | Teacher JWT | List teacher's class groups |
| `GET` | `/api/class-groups/{id}` | Teacher JWT | Get class group by ID |
| `POST` | `/api/class-groups` | Teacher JWT | Create a class group |
| `PUT` | `/api/class-groups/{id}` | Teacher JWT | Update a class group |
| `DELETE` | `/api/class-groups/{id}` | Teacher JWT | Delete a class group |
| `POST` | `/api/class-groups/{id}/students/{sid}` | Teacher JWT | Enrol a student in the group |
| `DELETE` | `/api/class-groups/{id}/students/{sid}` | Teacher JWT | Remove a student from the group |

### Admin
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/admins/login` | Public | Admin login and receive JWT |
| `POST` | `/api/admins` | Admin JWT | Create a new admin account |
| `GET` | `/api/admins/{id}` | Admin JWT | Get admin profile |
| `PUT` | `/api/admins/{id}/password` | Admin JWT | Change own password |
| `GET` | `/api/admins/teachers` | Admin JWT | List all teachers |
| `DELETE` | `/api/admins/teachers/{id}` | Admin JWT | Delete a teacher (override) |
| `GET` | `/api/admins/students` | Admin JWT | List all students |
| `DELETE` | `/api/admins/students/{id}` | Admin JWT | Delete a student (override) |
| `GET` | `/api/admins/audit-logs` | Admin JWT | Get paginated audit log |
| `GET` | `/api/admins/audit-logs/{entity}/{id}` | Admin JWT | Get audit log for an entity |
| `PUT` | `/api/admins/teachers/{id}` | Admin JWT | Update a teacher (admin override) |
| `PUT` | `/api/admins/students/{id}` | Admin JWT | Update a student (admin override) |
| `POST` | `/api/admins/students/bulk` | Admin JWT | Bulk-import up to 500 students (JSON body) |
| `POST` | `/api/admins/students/bulk-csv` | Admin JWT | Bulk-import students from CSV file (multipart) |
| `POST` | `/api/admins/teachers/bulk` | Admin JWT | Bulk-import up to 500 teachers (JSON body) |
| `POST` | `/api/admins/teachers/bulk-csv` | Admin JWT | Bulk-import teachers from CSV file (multipart) |

### Lookups
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/grades` | Public | List all grades (7–12) |
| `GET` | `/api/subjects` | Public | List all subjects |

## Usage

**Admin Workflow**
1. Navigate to `/admin/login` and log in with your admin credentials
2. From the **Teachers** tab: create teacher accounts (ID/Passport No., name, email, phone, subject — no password; the teacher sets it on first login)
3. From the **Students** tab: create student records (ID/Passport No., name, email, phone, grade); assign teachers to students; edit records via the edit modal
4. Use the **⬆ Bulk Import** button on either the Teachers or Students tab to import many records at once — paste CSV text, upload a `.csv` file, or download the provided template
5. From the **Audit Log** tab: browse all create/update/delete events system-wide

**Teacher Workflow**
1. Contact your admin to have an account created for you
2. Navigate to `/activate` — enter your registered email and choose a password to activate your account
3. Navigate to `/login` — log in with your email and password to receive a JWT session
4. The student list (`/`) shows all students assigned to you, with DataTables sorting, search, and pagination
5. Click a student's name to open the detail page with full assessment history and file submissions
6. Add, edit, or delete assessments inline; performance summary updates immediately
7. Download or delete student file submissions from the detail page
8. Export the student list to CSV, or download an individual student report as CSV or PDF

**Student Workflow**
1. Navigate to `/student/login` and click the activation tab
2. Enter your `StudentUniqueId` (provided by your teacher), registered email, and choose a password
3. After activation, log in with your `StudentUniqueId` and password
4. Your dashboard shows performance summary cards, assessments table, and your profile
5. Upload completed work using the file upload modal next to any assessment

## Form Validation Rules

**Teacher Account (Admin-created — no password at creation time):**
- **ID/Passport No.**: Required, exactly 9 alphanumeric characters (letters and digits only)
- **First / Last Name**: Required, max 50 characters
- **Email**: Required, valid email format, unique
- **Phone**: Required, exactly 8 digits
- **Subject**: Required, selected from API-seeded dropdown

**Teacher Activation (self-service, first login):**
- **Email**: Required, valid email format (must match the admin-registered email)
- **Password**: Required, minimum 6 characters
- **Confirm Password**: Must exactly match password

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

## Future Enhancements

- Role-based access control with granular permissions per subject
- Real-time notifications via SignalR
- Mobile-responsive PWA support
- External blob/file storage for assessment submissions (currently stored as byte arrays in the database)
- International phone number format support (currently 8-digit local format only)

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
