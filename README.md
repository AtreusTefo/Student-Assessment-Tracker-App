# Student Assessment Tracker

Student Assessment Tracker is a **full-stack web application** built with **ASP.NET Core 8** backend and **Angular 18** frontend. It provides an intuitive interface to manage and track student assessments, allowing teachers to add, edit, delete, and view student scores with automatic calculations for totals, averages, percentages, and performance levels.

## Architecture Overview

This system uses a **modern full-stack architecture**:

**Backend** – ASP.NET Core 8 REST API
- Entity Framework Core (In-Memory Database)
- FluentValidation for input validation
- AutoMapper for DTO mapping
- CORS enabled for frontend communication

**Frontend** – Angular 18 (Standalone Components)
- Reactive forms with comprehensive validation
- RxJS Observables for async operations
- Angular routing for navigation
- Responsive UI with CSS styling

**Why This Stack:**
- **Separation of Concerns**: Backend handles business logic, frontend handles presentation
- **Scalability**: Easy to extend backend APIs or add new frontend features
- **Best Practices**: Enterprise-grade patterns (DTOs, AutoMapper, FluentValidation)
- **Modern Framework**: Angular provides powerful tools for building dynamic interfaces
- **Type Safety**: Both languages use strong typing for reliability

## Technology Stack

- **Backend**: ASP.NET Core 8, Entity Framework Core, FluentValidation, AutoMapper
- **Frontend**: Angular 18, TypeScript, RxJS, FormsModule
- **Database**: In-Memory (Entity Framework Core)
- **Build Tools**: npm, Angular CLI, .NET CLI

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

✅ **Student Management**
- Add new students with assessments (scores 0-20)
- Edit student information
- Delete students with confirmation
- View all students in a table

✅ **Data Validation**
- Frontend validation (name format, email, phone)
- Backend validation with FluentValidation
- Real-time error messages
- User-friendly form feedback

✅ **Automatic Calculations**
- Total Score: Sum of 3 assessments
- Average Score: Total ÷ 3
- Percentage: (Total ÷ 60) × 100
- Performance Level:
  - **Needs Support**: < 50%
  - **Satisfactory**: 50-55%
  - **Good**: 56-75%
  - **Excellent**: > 75%

✅ **User Interface**
- Responsive design
- Student list with sorting
- Interactive forms
- Real-time validation feedback
- Loading states and error handling

## Project Structure

```
StudentAssessmentTracker/
├── StudentApp/                          # Angular Frontend
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/
│   │   │   │   ├── student-form.component.ts
│   │   │   │   ├── student-list.component.ts
│   │   │   │   └── student-detail.component.ts
│   │   │   ├── services/
│   │   │   │   └── student.service.ts
│   │   │   └── app.routes.ts
│   │   └── main.ts
│   ├── angular.json
│   ├── package.json
│   └── dist/                            # Built Angular app
│
├── Controllers/                         # ASP.NET Core Backend
│   └── StudentsController.cs
├── Models/
│   └── Student.cs
├── DTO/
│   └── StudentDto.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Validators/
│   └── StudentValidator.cs
├── Mappings/
│   └── MappingProfile.cs
├── wwwroot/                             # Static files (Angular build output)
│   ├── index.html
│   ├── main-*.js
│   ├── styles-*.css
│   └── favicon.ico
├── Program.cs                           # Application startup configuration
├── StudentAssessmentTracker.csproj
└── README.md
```

## Setup Instructions

### Prerequisites
- .NET 8 SDK
- Node.js 18+ with npm
- VS Code or Visual Studio

### Backend Setup

1. **Install .NET dependencies**
   ```bash
   dotnet restore
   ```

2. **Build the project**
   ```bash
   dotnet build
   ```

3. **Run the application**
   ```bash
   dotnet run
   ```
   
   The application will start on `http://localhost:5000`

### Frontend Setup

The Angular frontend is pre-built and included in the `wwwroot/` directory. If you need to rebuild or modify the frontend:

1. **Navigate to the StudentApp directory**
   ```bash
   cd StudentApp
   ```

2. **Install npm dependencies**
   ```bash
   npm install
   ```

3. **Build the Angular application**
   ```bash
   npm run build
   ```
   
   This creates optimized files in `dist/StudentApp/browser/`

4. **Copy built files to wwwroot**
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

- ✅ **Phone Validation Duplicate Error** – Fixed template to show only one error message
- ✅ **Application Blank Screen** – Fixed Angular build deployment to wwwroot root directory
- ✅ **Form Change Detection** – Added ChangeDetectorRef for proper async handling in Angular
- ✅ **API Response Type Mismatch** – Implemented proper DTO mapping with AutoMapper
- ✅ **Validation Error Handling** – Separated frontend and backend validation concerns

For detailed information about fixes and known issues, see the documentation files in the project root.
