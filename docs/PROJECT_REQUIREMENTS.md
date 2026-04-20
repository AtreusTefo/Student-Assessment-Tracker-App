# Project Requirements Document (PRD)

## Project Name
Student Assessment Tracker

## Purpose
A full-stack, multi-role web application for managing student assessments. Admins govern the platform (create teachers and students, assign relationships, review audit logs); teachers manage assessments for their assigned students and export reports; students activate their accounts and track their own performance via a self-service dashboard.

## Stakeholders
- Administrators (platform governance)
- Teachers (primary daily users)
- Students (self-service consumers)
- Development/QA team (implementation and maintenance)

## Scope
### In Scope
- Three-role system: Admin, Teacher, Student — each with a separate JWT
- Admin panel: create/delete teachers and students, assign teachers to students, audit log browser
- Teacher account activation workflow (admin creates account, teacher sets password on first login)
- Student account activation workflow (admin creates record, student sets password using their `StudentUniqueId`)
- Flexible named assessments with custom max scores, due dates, and instructions
- Automatic performance calculations (total, average, percentage, performance level) based on actual max possible marks
- File submissions: students upload work, teachers view/download/delete
- Email notifications to students when a new assessment is created
- Data export: CSV and PDF reports per student, CSV for the full student list
- Class groups: teachers create groups linked to a subject and grade, enrol/unenrol students
- Audit logging: immutable log of all create/update/delete actions across all entities
- Responsive UI with real-time form validation, DataTables-powered student list, route guards
- REST API with Swagger documentation

### Out of Scope
- International phone number formats (currently 8-digit local format only)
- External blob/file storage (submissions stored in the database)
- Student-side profile editing (read-only from student dashboard)
- Analytics dashboards beyond per-student performance cards

## Functional Requirements

### Admin Management
1. A seed admin account exists in the database (BCrypt-hashed password).
2. Admin authenticates via `POST /api/admins/login`; receives a JWT with the `Admin` role.
3. An authenticated admin can create additional admin accounts (`POST /api/admins` — Admin JWT required).
4. Admin can change their own password (`PUT /api/admins/{id}/password`).

### Teacher Account Lifecycle
1. Admin creates a teacher account with ID/Passport No., first name, last name, email, phone, and subject (no password at creation time).
2. The teacher receives their registered email address from the admin and navigates to `/activate`.
3. Teacher activates their account via `POST /api/teachers/activate` by providing their email, a new password, and password confirmation.
4. After activation the teacher logs in via `POST /api/teachers/login` with email and password; receives a JWT with the `Teacher` role.
5. Teachers can update their own profile (`PUT /api/teachers/{id}`) and delete their own account (`DELETE /api/teachers/{id}`).
6. Admin can delete any teacher account regardless of ownership (`DELETE /api/admins/teachers/{id}`).

### Student Account Lifecycle
1. Admin creates a student record with ID/Passport No., first name, last name, email, phone, and grade.
2. A system-generated `StudentUniqueId` (format `STU-XXXXXXXX`) is assigned at creation.
3. Admin assigns one or more teachers to the student (`POST /api/students/{sid}/teachers/{tid}`).
4. The student activates their account via `POST /api/students/activate` using their `StudentUniqueId`, registered email, and a chosen password.
5. After activation the student logs in via `POST /api/students/login` with their `StudentUniqueId` and password; receives a JWT with the `Student` role.
6. Admin can update (`PUT /api/students/{id}`) and delete (`DELETE /api/students/{id}`) student records with cascade removal of all assessments and submissions.

### Student List (Teacher Role)
1. The authenticated teacher sees only their assigned students at `GET /api/students`.
2. The student list is rendered with DataTables: pagination, column sorting, and global search.
3. Each student row displays a colour-coded performance level badge.

### Assessment Management (Teacher Role)
1. Teachers add named assessments to any of their assigned students: name (required, max 100 chars), `MaxScore` (required, > 0), `Score` (0 ≤ Score ≤ MaxScore), optional `DueDate`, `Instructions`, and `IsAssigned` flag.
2. Teachers can edit and delete individual assessments.
3. Performance summary (Total Score, Average Score, Percentage, Performance Level) is calculated server-side after every change.

### Performance Calculations
1. **Total Score** = sum of all `Score` values for the student.
2. **Max Possible** = sum of all `MaxScore` values for the student.
3. **Percentage** = `(Total Score / Max Possible) × 100` — returns 0 when no assessments exist.
4. **Average Score** = `Total Score / number of assessments` — returns 0 when no assessments exist.
5. **Performance Level**:
   - **Needs Support**: < 50%
   - **Satisfactory**: 50–55%
   - **Good**: 56–75%
   - **Excellent**: > 75%

### File Submissions
1. Students upload completed work (PDF, DOC, DOCX, JPG, JPEG, PNG; max 10 MB) per assessment via their dashboard.
2. Only the owning student may upload a submission for their own assessment.
3. Teachers can list, download, and delete submissions for their assigned students.
4. Both teachers and students can download or delete a specific submission.

### Student Self-Service Dashboard
1. After login, the student sees performance summary cards (Total, Average, Percentage, Performance Level).
2. A colour-coded progress bar with performance band legend is shown.
3. An assessments table displays Overdue/Submitted status badges per assessment.
4. A read-only "My Profile" section shows the student's personal details.
5. A file upload modal is available per assessment row.

### Email Notifications
1. When a teacher creates a new assessment for a student, the system sends an email notification to the student's registered address.
2. Email delivery is fire-and-forget; the system continues normally if the SMTP host is unconfigured.

### Data Export (Teacher Role)
1. Teachers can export their full student list to CSV.
2. Teachers can export an individual student's assessment report to CSV or PDF.
3. PDF reports include a styled header, student personal info, and a colour-coded assessment table.

### Class Groups (Teacher Role)
1. Teachers can create named class groups linked to a subject, grade, and their own account.
2. Students can be enrolled and unenrolled from class groups.
3. All class group endpoints are scoped to the authenticated teacher.

### Audit Logging
1. Every Create, Update, and Delete action across Students, Teachers, Assessments, and Class Groups writes an immutable audit entry.
2. Each entry stores: entity name, entity ID, action, old values (JSON), new values (JSON), actor ID, role, and UTC timestamp.
3. Admins can browse a paginated audit log and filter by entity type and entity ID.

## Validation Rules

### Teacher Account (Admin creates — no password)
- **ID/Passport No.**: Required, exactly 9 alphanumeric characters (letters and digits only)
- **First / Last Name**: Required, max 50 characters
- **Email**: Required, valid email format, unique
- **Phone**: Required, exactly 8 digits
- **Subject**: Required, selected from API-seeded dropdown

### Teacher Activation
- **Email**: Required, valid email format (must match the admin-registered email)
- **Password**: Required, minimum 6 characters
- **Confirm Password**: Must exactly match password

### Student Form (Admin creates)
- **ID/Passport No.**: Required, exactly 9 characters (letters, digits, hyphens)
- **First / Last Name**: Required, 2–50 characters, letters/spaces/hyphens only
- **Email**: Required, valid email format, unique
- **Phone**: Required, exactly 8 digits
- **Grade**: Required, must be a valid grade from the seeded dropdown

### Student Activation
- **StudentUniqueId**: Required, must match an existing `STU-XXXXXXXX` record
- **Email**: Required, must match the student's registered email
- **Password**: Required, minimum 6 characters
- **Confirm Password**: Must exactly match password

### Assessment Form
- **Name**: Required, max 100 characters
- **MaxScore**: Required, must be > 0
- **Score**: Required, must be ≥ 0 and ≤ MaxScore

## Non-Functional Requirements

### Performance
- Typical API responses complete within 500 ms in development.

### Usability
- Real-time inline field validation for all form inputs.
- Global error banner only for unexpected server failures.
- Navigation and route guards prevent unauthorised access to role-specific views.

### Reliability
- All API responses return consistent JSON shapes.
- The UI does not submit invalid data (client-side + server-side validation).
- EF Core migrations are applied automatically on startup; database is never in an inconsistent schema state.

### Security
- All passwords are BCrypt-hashed before storage; plain-text passwords are never persisted.
- JWT tokens carry role claims (`Admin`, `Teacher`, `Student`); endpoints are protected with `[Authorize(Roles = "...")]`.
- FluentValidation rejects invalid payloads with HTTP 400 before they reach business logic.
- File upload size is capped at 10 MB; only allowed MIME types are accepted.
- Audit logs provide a tamper-evident trail for all data mutations.

## Technical Requirements

### Backend
- **Runtime**: .NET 8.0
- **Framework**: ASP.NET Core 8 Web API (Clean Architecture)
- **ORM**: Entity Framework Core 8.0 with SQL Server LocalDB
- **Auth**: JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`), BCrypt.Net-Next
- **Validation**: FluentValidation 12.1
- **Mapping**: AutoMapper 12.0
- **Logging**: Serilog 8.0 (console + rolling file sinks)
- **Email**: MailKit 4.x (fire-and-forget SMTP)
- **Export**: CsvHelper 33 (CSV), QuestPDF 2024 (PDF)
- **API Docs**: Swashbuckle (Swagger UI with Bearer auth support)
- REST endpoints:
  - Students: `GET/POST/PUT/DELETE /api/students`, activate/login, teacher assignment
  - Teachers: `GET/POST/PUT/DELETE /api/teachers`, activate, login
  - Assessments: nested under `/api/students/{id}/assessments`
  - Submissions: nested under `/api/students/{id}/assessments/{aid}/submissions`
  - Reports: `GET /api/reports/students/csv|pdf`
  - Class Groups: `GET/POST/PUT/DELETE /api/class-groups`
  - Admin: `GET/POST/DELETE /api/admins`, teachers/students oversight, audit logs
  - Lookups: `GET /api/grades`, `GET /api/subjects`

### Frontend
- **Framework**: Angular 21 (standalone components, zoneless)
- **Language**: TypeScript 5.9
- **Reactive**: RxJS 7.8
- **HTTP**: Angular HttpClient with function-based interceptors
- **Routing**: Angular Router with `canActivate` guards for all three roles
- **Build**: Angular CLI 21 (`@angular/build:application`)
- **Tests**: Vitest 4
- **Tables**: DataTables.net v2 + Buttons plugin (CSV export)
- Routes:
  - `/` — Student list (Teacher)
  - `/create`, `/edit/:id`, `/detail/:id` — Student management (Teacher)
  - `/login`, `/activate` — Teacher auth
  - `/student/login` — Student login + activation (dual-mode)
  - `/student/dashboard` — Student dashboard
  - `/admin/login`, `/admin/dashboard` — Admin panel
- Build output served by ASP.NET Core from `StudentApp/dist/StudentApp/browser`

## User Flows

### Admin: Onboard a Teacher
1. Admin logs in at `/admin/login`.
2. On the Teachers tab, admin fills in teacher details and submits.
3. Admin provides the teacher with their registered email address.

### Teacher: Activate and Log In
1. Teacher navigates to `/activate`, enters their registered email, a new password, and confirms it.
2. After activation, teacher navigates to `/login` and logs in with email and password.

### Admin: Onboard a Student and Assign a Teacher
1. Admin creates the student record from the Students tab; `StudentUniqueId` is generated.
2. Admin assigns the relevant teacher(s) to the student.
3. Admin provides the student with their `StudentUniqueId` and registered email.

### Student: Activate and Log In
1. Student navigates to `/student/login`, selects the Activation tab.
2. Student enters their `StudentUniqueId`, registered email, and a new password.
3. After activation, student logs in with their `StudentUniqueId` and password.

### Teacher: Manage Assessments
1. Teacher opens the student detail page (`/detail/:id`).
2. Teacher adds an assessment (name, max score, score, optional due date / instructions).
3. Performance summary cards update immediately.
4. Teacher can edit or delete assessments inline.
5. Teacher exports the student's report as CSV or PDF if needed.

### Student: Submit Work
1. Student logs in and opens their dashboard.
2. Student clicks the upload icon next to an assessment.
3. Student selects an allowed file type (PDF, DOC, DOCX, JPG, JPEG, PNG ≤ 10 MB) and submits.

## Acceptance Criteria
- Admin can create teacher and student accounts; those accounts cannot be created from public-facing routes.
- Teacher activation fails if the email is not registered; login fails if the account is not yet activated.
- Student activation fails if the `StudentUniqueId` / email combination does not match; login fails with incorrect credentials.
- Assessment `Score` cannot exceed `MaxScore`; invalid payloads return HTTP 400.
- Performance calculations update server-side after every assessment change and reflect accurately in the dashboard.
- File uploads exceeding 10 MB or with disallowed extensions are rejected.
- All mutating operations (create / update / delete) produce an audit log entry visible in the admin dashboard.
- Route guards redirect unauthenticated or wrong-role users to the correct login page.
- Student list updates after teacher-side assessment changes.

## Constraints and Assumptions
- Phone number validation accepts exactly 8 digits (no international format).
- File submissions are stored as binary in the database; no external blob storage is configured.
- The seed admin account must be created via database seed or migration before the application can be used.
- SMTP email delivery is optional; the system degrades gracefully when `Email:SmtpHost` is not set.
- The application is served by ASP.NET Core; the Angular build output is copied to `wwwroot` or served from the `dist` folder directly by the development proxy.

