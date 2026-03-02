# Project Requirements Document (PRD)

## Project Name
Student Assessment Tracker

## Purpose
Provide a simple, responsive web app for teachers to register, log in, and manage student assessments with automatic scoring calculations.

## Stakeholders
- Teachers (primary users)
- School admins (secondary stakeholders)
- Development/QA team (implementation and maintenance)

## Scope
### In Scope
- Teacher registration and login
- Student CRUD (create, read, update, delete)
- Automatic calculation of totals, averages, percentages, and performance levels
- Responsive UI with form validation and error messaging
- REST API for student and teacher operations

### Out of Scope (Current Version)
- Role-based access control and authorization
- Persistent database (currently in-memory)
- Password hashing and token-based authentication (JWT)
- Class/subject management beyond a simple subject string
- Reporting/analytics dashboards and exports

## Functional Requirements

### Teacher Registration
1. Teachers can register with: first name, last name, email, phone, subject, password.
2. Registration validates required fields and formats on client and server.
3. After successful registration, the UI redirects to the login page.

### Teacher Login
1. Teachers can log in with email and password.
2. Invalid credentials return a friendly error.
3. Login returns a token and teacher profile data.

### Student Management
1. Teachers can create a student with:
   - First name, last name, email, phone, grade
   - Assessment1, Assessment2, Assessment3 (0–20)
2. Students are listed in a table view with key identifiers.
3. Teachers can view detailed student information.
4. Teachers can update student records.
5. Teachers can delete student records with confirmation.

### Calculations
1. Total = Assessment1 + Assessment2 + Assessment3
2. Average = Total / 3
3. Percentage = (Total / 60) * 100
4. Performance Level:
   - Needs Support: < 50%
   - Satisfactory: 50–55%
   - Good: 56–75%
   - Excellent: > 75%

### Validation Rules
- Names: required, 2–50 characters
- Email: required, valid email format
- Phone: required, exactly 8 digits
- Subject: required, max 100 characters
- Password: required, 6–20 characters
- Assessments: required, integer, 0–20 inclusive

## Non-Functional Requirements

### Performance
- Typical API responses should complete within 500ms in development.

### Usability
- Inline field validation for user input errors.
- Global error banner only for server/system failures.
- Navigation links for all major actions.

### Reliability
- API should return consistent JSON response shapes.
- The UI must not submit invalid data.

### Security (Current Baseline)
- Basic password comparison (no hashing).
- No authorization enforced on student endpoints.

## Technical Requirements

### Backend
- ASP.NET Core 8
- Entity Framework Core (In-Memory database)
- FluentValidation for model validation
- AutoMapper for DTO mapping
- REST endpoints:
  - Students: GET/POST/PUT/DELETE /api/students
  - Teachers: GET/POST/PUT/DELETE /api/teachers
  - Login: POST /api/teachers/login

### Frontend
- Angular 18 (standalone components)
- Routing: /, /create, /edit/:id, /detail/:id, /register, /login
- Forms: Template-driven with validation feedback
- Build output served from: StudentApp/dist/StudentApp/browser

## User Flows

### Register and Login
1. Teacher navigates to Register.
2. Completes registration form.
3. On success, redirected to Login.
4. Teacher logs in with email and password.

### Create Student
1. Teacher navigates to Add Student.
2. Fills out student info and assessments.
3. Validates inline errors.
4. Submit creates the student and redirects to list.

### Edit Student
1. Teacher selects Edit from list or detail.
2. Form loads existing values.
3. Updates are saved and redirect to detail view.

### Delete Student
1. Teacher clicks Delete in list.
2. Student is removed and list updates.

## Acceptance Criteria
- Registration requires a password and redirects to login on success.
- Login succeeds with correct credentials and fails with invalid credentials.
- Student form cannot submit with empty or invalid assessment values.
- Inline validation displays for required fields.
- Global error banner shows only unexpected server errors.
- Students list updates after create/edit/delete.

## Constraints and Assumptions
- Data is reset on application restart (in-memory DB).
- Authentication is minimal and for demo purposes only.
- App is served by ASP.NET Core from Angular build output.

## Risks
- Plain-text password storage is not production safe.
- In-memory DB is not durable.
- No authorization could expose student data in a real deployment.

## Future Enhancements
- Replace in-memory DB with SQL Server or PostgreSQL.
- Implement hashed passwords and JWT-based auth.
- Role-based access control (teacher/admin).
- Reporting and export (CSV/PDF).
- Class/subject taxonomy and analytics dashboards.

