# API Setup and Testing Guide

## Project Overview

The Student Assessment Tracker is a full-stack application consisting of:
- **Backend**: ASP.NET Core 8 Web API (Clean Architecture)
- **Frontend**: Angular 21 standalone SPA
- **Database**: SQL Server LocalDB (`StudentAssessmentTrackerDev`)

---

## Verified Integrations

### 1. DataTables Integration (Frontend)
- **Status**: COMPLETE
- **Package**: `datatables.net` v2 + Buttons plugin
- **Location**: `StudentApp/src/app/components/student-list.component.ts`
- **Features**:
  - Sorting, global search, and pagination (10 records per page)
  - CSV export via DataTables Buttons plugin
  - Responsive layout

### 2. Swagger UI (Backend)
- **Status**: INSTALLED AND CONFIGURED
- **Package**: `Swashbuckle.AspNetCore`
- **Access URL**: `http://localhost:5000/swagger`
- **Features**:
  - Interactive API documentation with per-operation Bearer security
  - Live testing interface for all endpoints
  - Request and response schema examples

### 3. API Controllers
- **Status**: FULLY IMPLEMENTED
- **Controllers**: `AdminsController`, `TeachersController`, `StudentsController`, `StudentAssessmentsController`, `AssessmentSubmissionsController`, `ReportsController`, `GradesController`, `SubjectsController`, `ClassGroupsController`
- **Auth**: Three JWT roles — Admin, Teacher, Student

---

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 20+
- SQL Server LocalDB (bundled with Visual Studio or installable separately)
- Postman (optional, for API testing)

### Step 1: Start the Backend API

```powershell
# Navigate to the API project folder
cd C:\Users\Developer.03\Desktop\Student-Assessment-Tracker\StudentAssessmentTrackerAPI

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

Expected output:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

The application will automatically apply all pending EF Core migrations and seed the default admin account on first run.

### Step 2: Verify the API

Open a browser and navigate to:
```
http://localhost:5000/swagger
```

Swagger UI should load and show all endpoint groups: Admins, Teachers, Students, StudentAssessments, AssessmentSubmissions, Reports, Grades, Subjects, ClassGroups.

### Step 3: Start the Angular Frontend

```powershell
cd C:\Users\Developer.03\Desktop\Student-Assessment-Tracker\StudentApp
npm install
npm start
```

Frontend runs on `http://localhost:4200`. The proxy configuration in `proxy.conf.json` forwards `/api` calls to the backend at port 5000.

---

## Authentication Flow

The API uses three separate JWT roles. Each role has its own login endpoint.

### Seed Admin Account

A default admin is seeded automatically on first startup:

| Field | Value |
|---|---|
| Email | admin@tracker.local |
| Password | Admin@1234 |

### Getting Tokens

**Admin token:**
```
POST http://localhost:5000/api/admins/login
Body: { "email": "admin@tracker.local", "password": "Admin@1234" }
```

**Teacher token** (after admin creates teacher and teacher activates):
```
POST http://localhost:5000/api/teachers/login
Body: { "email": "teacher@school.com", "password": "your-password" }
```

**Student token** (after student is activated):
```
POST http://localhost:5000/api/students/login
Body: { "studentUniqueId": "STU-XXXXXXXX", "password": "your-password" }
```

---

## Key Endpoint Groups

### Admin Endpoints (Admin JWT required)
```
GET    /api/admins/teachers                  List all teachers
POST   /api/admins/teachers                  Create teacher
PUT    /api/admins/teachers/{id}             Update teacher
DELETE /api/admins/teachers/{id}             Delete teacher
GET    /api/admins/students                  List all students
POST   /api/admins/students                  Create student
PUT    /api/admins/students/{id}             Update student
DELETE /api/admins/students/{id}             Delete student
POST   /api/admins/students/bulk             Bulk import students (JSON)
POST   /api/admins/students/bulk-csv         Bulk import students (CSV file)
POST   /api/admins/teachers/bulk             Bulk import teachers (JSON)
POST   /api/admins/teachers/bulk-csv         Bulk import teachers (CSV file)
GET    /api/admins/audit-logs/{entity}/{id}  View audit log
```

### Teacher Endpoints (Teacher JWT required unless noted)
```
POST /api/teachers/activate                  Activate teacher account (public)
POST /api/teachers/login                     Teacher login (public)
POST /api/teachers/forgot-password           Reset password (public)
GET  /api/students                           List assigned students
GET  /api/students/{id}                      Student detail
POST /api/students/{id}/assessments          Add assessment
PUT  /api/students/{id}/assessments/{aid}    Edit assessment
DEL  /api/students/{id}/assessments/{aid}    Delete assessment
POST /api/assessments/bulk                   Bulk create assessments
GET  /api/reports/students/{id}/csv          Export student CSV
GET  /api/reports/students/{id}/pdf          Export student PDF
```

### Student Endpoints (Student JWT required unless noted)
```
POST /api/students/activate                  Activate student account (public)
POST /api/students/login                     Student login (public)
POST /api/students/forgot-password           Reset password (public)
GET  /api/students/profile                   View own profile
```

---

## Troubleshooting

| Problem | Solution |
|---|---|
| Cannot open database `StudentAssessmentTrackerDev` | SQL Server LocalDB is not running. Run `sqllocaldb start mssqllocaldb` in a terminal |
| 401 Unauthorized | Run the login request for your role first and attach the returned token as `Bearer <token>` in the Authorization header |
| 400 Bad Request | Check the Swagger UI schema for required fields. Phone must be exactly 8 digits. Email must be lowercase |
| 500 Internal Server Error | Check the dotnet console output. Look in `StudentAssessmentTrackerAPI/Logs/` for structured log files |
| Migration error on startup | Run `dotnet ef database update` manually in the API folder |


---

## ✅ Verified Integrations

### 1. DataTables Integration (Frontend)
- **Status**: ✅ COMPLETE
- **Package**: `datatables.net` v2.3.7
- **Location**: [StudentApp/src/app/components/student-list.component.ts](StudentApp/src/app/components/student-list.component.ts)
- **Features**:
  - Advanced table sorting and searching
  - Pagination (10 records per page)
  - Responsive layout
  - Custom styling with professional UI

### 2. Scalar API Documentation (Backend)
- **Status**: ✅ INSTALLED & CONFIGURED
- **Package**: `Scalar.AspNetCore` v2.0.0
- **Configuration File**: [Program.cs](Program.cs)
- **Access URL**: `http://localhost:5000/scalar/v1`
- **Features**:
  - Interactive API documentation
  - Live testing interface
  - Request/response examples
  - Modern, user-friendly UI from scalar.com

### 3. API Controllers
- **Status**: ✅ FULLY DOCUMENTED
- **Main Controller**: [Presentation/Controllers/StudentsController.cs](Presentation/Controllers/StudentsController.cs)
- **Endpoints**: 5 REST API endpoints with full CRUD operations
- **Documentation**: XML comments with OpenAPI attributes

---

## 🚀 Getting Started

### Prerequisites
- **.NET 8 SDK**: Required for running the backend
- **Node.js 20+**: Required for Angular frontend
- **Postman**: Optional, for API testing
- **Scalar.com Account**: Optional, for advanced features

### Step 1: Start the Backend API

```powershell
# Navigate to project root
cd c:\Users\User\Desktop\StudentAssessmentTracker

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

**Expected Output:**
```
╔═══════════════════════════════════════════════════════════════════════════════╗
║         Student Assessment Tracker - Multi-Layered Architecture              ║
║                                                                               ║
║   🚀 Running on: http://localhost:5000                                       ║
║   📊 API Base: http://localhost:5000/api                                     ║
║   🏗️  Architecture: Domain → Infrastructure → Application → Presentation   ║
║                                                                               ║
║   ✅ Dependency Injection: Configured                                        ║
║   ✅ FluentValidation: Active                                                ║
║   ✅ AutoMapper: Configured                                                  ║
║   ✅ CORS: Enabled for Angular frontend                                      ║
║   ✅ Serilog: Logging active                                                 ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### Step 2: Access Scalar API Documentation

Once the backend is running, open your browser and navigate to:

```
http://localhost:5000/scalar/v1
```

This will open the interactive Scalar API documentation interface where you can:
- View all available endpoints
- Inspect request/response schemas
- Test endpoints directly from the UI
- View authentication requirements
- See example requests and responses

### Step 3: Start the Angular Frontend

In a new terminal window:

```powershell
cd StudentApp
npm install
npm start
```

The application will be available at: `http://localhost:4200`

---

## 📋 API Endpoints

### Base URL
```
http://localhost:5000/api/students
```

### Endpoints

#### 1. Get All Students
```
GET /api/students
```
- **Description**: Retrieves all students
- **Response**: 200 OK - Array of StudentDto
- **Example Response**:
```json
[
  {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "totalScore": 85.5,
    "averageScore": 85.5,
    "performanceLevel": "Good"
  }
]
```

#### 2. Get Student by ID
```
GET /api/students/{id}
```
- **Description**: Retrieves a specific student
- **Parameters**: `id` (integer) - Student ID
- **Response**: 200 OK - StudentDto | 404 Not Found
- **Example**: `GET /api/students/1`

#### 3. Create Student
```
POST /api/students
```
- **Description**: Creates a new student
- **Content-Type**: application/json
- **Request Body**:
```json
{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane@example.com",
  "phoneNumber": "+1-555-0100",
  "enrollmentDate": "2024-01-15",
  "program": "Computer Science",
  "totalScore": 90.0,
  "averageScore": 90.0,
  "performanceLevel": "Good"
}
```
- **Response**: 201 Created - StudentDto with assigned ID

#### 4. Update Student
```
PUT /api/students/{id}
```
- **Description**: Updates an existing student
- **Parameters**: `id` (integer) - Student ID
- **Content-Type**: application/json
- **Request Body**: Same as Create Student
- **Response**: 200 OK - Updated StudentDto | 404 Not Found

#### 5. Delete Student
```
DELETE /api/students/{id}
```
- **Description**: Deletes a student
- **Parameters**: `id` (integer) - Student ID
- **Response**: 204 No Content | 404 Not Found

---

## 🧪 Testing with Postman

### Import the Collection

1. **Download**: [StudentAssessmentTracker.postman_collection.json](StudentAssessmentTracker.postman_collection.json)
2. **Open Postman** and click **Import**
3. **Select the downloaded JSON file**
4. **Collection will be imported** with all endpoints

### Configure Environment

The collection uses a variable `{{base_url}}` which defaults to `http://localhost:5000`

**To modify:**
1. Open the collection
2. Click **Variables** tab
3. Update `base_url` value if your API runs on different host/port

### Run Requests

1. **Expand** the "Students" folder in the collection
2. **Select** an endpoint (e.g., "Get All Students")
3. **Click Send** to execute the request
4. **View response** in the response panel

### Example Test Workflow

```
1. Create Student
   - POST /api/students
   - Copy the returned student ID

2. Get Student (Using ID from step 1)
   - GET /api/students/{id}

3. Update Student (Using ID from step 1)
   - PUT /api/students/{id}
   - Modify data in request body

4. Get All Students
   - GET /api/students
   - Verify updates appear

5. Delete Student (Using ID from step 1)
   - DELETE /api/students/{id}

6. Verify Deletion
   - GET /api/students
   - Confirm student no longer exists
```

---

## Testing with Scalar UI

### Direct Browser Testing

1. **Start the backend**: `dotnet run`
2. **Open browser**: `http://localhost:5000/scalar/v1`
3. **Select endpoint** from the left sidebar
4. **Fill in parameters** (if required)
5. **Click Send Request**
6. **View response** in the right panel

### Advantages of Scalar UI

- No client installation required
- Interactive documentation
- Real-time request/response examples
- Schema validation
- Parameter suggestions
- Response formatting

---

## Project Architecture

The project follows a **clean architecture** with 4 layers:

### 1. Domain Layer
- **Location**: `Domain/`
- **Contents**: Business entities, interfaces
- **Example**: `Student.cs`, `IRepository.cs`

### 2. Infrastructure Layer
- **Location**: `Infrastructure/`
- **Contents**: Database context, repositories
- **Example**: `ApplicationDbContext.cs`, `StudentRepository.cs`

### 3. Application Layer
- **Location**: `Application/`
- **Contents**: Business logic, DTOs, validators, mappers
- **Example**: `StudentService.cs`, `StudentValidator.cs`, `MappingProfile.cs`

### 4. Presentation Layer
- **Location**: `Presentation/Controllers/`
- **Contents**: API controllers, HTTP handling
- **Example**: `StudentsController.cs`

---

## 🛠️ Technology Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 8.0 | Backend framework |
| Entity Framework Core | 8.0.0 | ORM |
| FluentValidation | 12.1.1 | Data validation |
| AutoMapper | 12.0.1 | DTO mapping |
| Serilog | 8.0.0 | Logging |
| Angular | 21.1.0 | Frontend framework |
| DataTables | 2.3.7 | Table functionality |
| Scalar | 2.0.0 | API documentation |

---

## 🐛 Troubleshooting

### Port Already in Use
If port 5000 is already in use:
```powershell
# Find process using port 5000
Get-Process | Where-Object {$_.Name -like "*dotnet*"}

# Kill the process (if needed)
Stop-Process -Id <PID> -Force

# Or run on different port
dotnet run --urls="http://localhost:5001"
```

### CORS Issues
If frontend cannot connect to API:
- Verify CORS is enabled in `Program.cs`
- Check that API is running
- Verify base URL in Angular service matches API URL

### Validation Errors
The API uses FluentValidation for input validation:
- Check request body structure
- Ensure required fields are provided
- Verify data types match schema

### Database Issues
The app uses in-memory database:
- Data is lost when application restarts
- Reset: Stop and restart the application
- To use persistent database: Replace `UseInMemoryDatabase()` in `Program.cs`

---

## 📚 Additional Resources

- [Scalar.com Documentation](https://scalar.com/docs)
- [Postman Learning Center](https://learning.postman.com)
- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [Angular Documentation](https://angular.io/docs)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)

---

## ✨ Summary

Your Student Assessment Tracker API is fully configured and ready for testing with:

✅ **Scalar** - Interactive API documentation (scalar.com)  
✅ **Postman** - Professional API testing tool  
✅ **DataTables** - Advanced table functionality in frontend  
✅ **Clean Architecture** - Maintainable and scalable design  
✅ **Full Documentation** - XML comments and examples  

**Get Started**:
1. Run: `dotnet run`
2. Access: http://localhost:5000/scalar/v1
3. Test your API endpoints!
