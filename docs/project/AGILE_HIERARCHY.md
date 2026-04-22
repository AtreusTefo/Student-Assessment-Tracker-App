# Student Assessment Tracker  Agile Methodology
## Scrum Framework with Application > Epic > Feature > User Story > Task Hierarchy

## Overview

This document applies the **Scrum framework** to the Student Assessment Tracker project. Scrum is an Agile framework that delivers value in short, time-boxed iterations called **Sprints**. It provides structure through defined roles (Product Owner, Scrum Master, Development Team), artifacts (Product Backlog, Sprint Backlog, Increment), and events (Sprint Planning, Daily Scrum, Sprint Review, Sprint Retrospective).

The project work is organized using a **five-level Agile hierarchy**  **Application  Epics  Features  User Stories  Tasks**  which populates the Scrum **Product Backlog**. User Stories are estimated with **story points** (Fibonacci scale), prioritized by business value, and allocated to four 1-week Sprints. Each User Story is broken down into concrete **Tasks** representing individual implementation steps completed within a Sprint.

---

## Agile Hierarchy: Definitions

### Application
The **Application** is the complete product being built  the entire system delivered at the end of all Sprints. It sits at the top of the hierarchy and represents the overarching project goal.

> **This project**: *Student Assessment Tracker*  a full-stack web application built on a **three-role system (Admin / Teacher / Student)**. Admins onboard teachers and students, assign teachers to students, and oversee the system via an audit log. Teachers manage their assigned students' assessments and performance. Students activate their own accounts and access a self-service performance portal.

---

### Epic
An **Epic** is a large body of work representing a high-level business domain or major capability. Epics span multiple Sprints and are broken down into Features.

> **Format**: `EPIC-XX: <Title>`  A short name describing the business domain area.

---

### Feature
A **Feature** is a service or function that delivers business value to a user. It represents a specific capability within an Epic and is broken down into User Stories.

> **Format**: `FEAT-XX: <Title>`  Describes a specific capability within the Epic.

---

### User Story
A **User Story** describes a single piece of functionality from the end-user's perspective. It is estimated in story points and broken down into Tasks.

> **Format**: `US-XX: As a [role], I want [goal], so that [benefit].`
>
> **Each User Story must include:**
> - **Description**: Clear explanation of what is being built and why.
> - **Acceptance Criteria**: Specific, testable conditions that must be met for the story to be considered done or complete.
> - **Tasks**: The concrete implementation steps required to deliver the story.
> - **App Example**: A real-world scenario from the Student Assessment Tracker App project illustrating the story in action.

---

### Task
A **Task** is the smallest unit of work  a concrete, implementable step required to complete a User Story. Tasks are assigned and completed within a single Sprint day and are not separately estimated in story points.

> **Format**: `TASK-XX: <Action verb> + <specific implementation step>`  e.g., *"Create `StudentCreateDto` with required fields"*.

---

## Hierarchy Map

```
APPLICATION: Student Assessment Tracker

 EPIC-01: Security
    FEAT-01: Teacher Account Lifecycle
       US-01: As an admin, I want to create a teacher account, so that the teacher can be onboarded to the system.
          TASK-01: Create TeacherCreateDto with all registration fields
          TASK-02: Implement POST /api/teachers (Admin JWT) controller action
          TASK-03: Build Angular teacher creation form in the admin panel
          TASK-04: Connect Angular form to API via AdminTeacherService
       US-02: As an admin, I want teacher creation fields to be validated, so that I cannot submit incomplete or invalid information.
           TASK-05: Add Angular validators to admin teacher creation form
           TASK-06: Implement TeacherCreateValidator (FluentValidation)
           TASK-07: Display inline error messages on invalid fields
    FEAT-02: Teacher Login & Activation
       US-03: As a registered teacher, I want to log in using my email and password, so that I can access the student management dashboard.
          TASK-08: Create TeacherLoginDto and login response model
          TASK-09: Implement POST /api/teachers/login controller action
          TASK-10: Build Angular login form component
          TASK-11: Store teacher session in TeacherStateService on success
       US-04: As a teacher, I want to see clear error messages when my login credentials are invalid, so that only registered teachers can log in and I understand why access is denied.
          TASK-12: Return 401 Unauthorized for invalid credentials in API
          TASK-13: Display friendly error message in Angular on failed login
       US-32: As a teacher, I want to activate my account using my email and a new password, so that I can set my credentials and log in for the first time.
           TASK-100: Create TeacherActivateDto with Email and Password fields
           TASK-101: Implement POST /api/teachers/activate (public) controller action
           TASK-102: Build Angular teacher activate component (teacher-activate.component.ts) at /activate
           TASK-103: Redirect to /login on successful teacher account activation
    FEAT-03: Input Validation
        US-06: As a teacher, I want student input fields to be validated, so that I cannot submit incomplete or invalid student data.
           TASK-14: Add Angular validators to student create/edit form
           TASK-15: Implement StudentCreateValidator (FluentValidation)
           TASK-16: Show inline validation errors on form submission
        US-18: As a user, I want the server to reject invalid data, so that only valid information is saved in the system.
            TASK-17: Register FluentValidation in Program.cs
            TASK-18: Create validators for all DTOs (Teacher, Student, Assessment)
            TASK-19: Verify 400 Bad Request with structured error body in Postman

 EPIC-02: Student Management
    FEAT-04: Create Student
       US-05: As an admin, I want to add a new student record, so that the student can be assigned to teachers and tracked in the system.
           TASK-20: Create StudentCreateDto with required fields
           TASK-21: Implement POST /api/students (Admin JWT) controller action
           TASK-22: Generate StudentUniqueId (STU-XXXXXXXX) in service layer
           TASK-23: Build Angular student creation form in the admin panel
           TASK-24: Redirect to student list on successful creation
    FEAT-05: View Students
       US-07: As a teacher, I want to view all my assigned students and their performance, so that I can monitor progress and identify students who need support.
          TASK-25: Create StudentListDto with all display fields
          TASK-26: Implement GET /api/students returning StudentListDto array
          TASK-27: Build Angular student list component
          TASK-28: Initialise DataTables with pagination, sorting, hidden % column
       US-08: As a teacher, I want to view a detailed student profile, so that I can see individual performance and assessment history.
          TASK-29: Create StudentDetailDto with embedded assessment list
          TASK-30: Implement GET /api/students/{id} returning StudentDetailDto
          TASK-31: Build Angular student detail component
          TASK-32: Display performance summary and assessment table on detail page
       US-20: As a teacher, I want to select a grade from a controlled dropdown, so that student records are consistent and accurate.
           TASK-33: Create Grade entity and seed Grade 712 (EF Core migration)
           TASK-34: Implement GET /api/grades read-only endpoint
           TASK-35: Populate grade dropdown in Angular forms from API response
    FEAT-06: Edit Student
       US-09: As an admin, I want to edit a student's personal details, so that I can keep student records accurate and up to date.
           TASK-36: Create StudentUpdateDto for personal details
           TASK-37: Implement PUT /api/students/{id} (Admin JWT) controller action
           TASK-38: Build Angular edit form pre-populated with current student data
           TASK-39: Redirect to detail view on successful update
    FEAT-07: Delete Student
       US-10: As an admin, I want to delete a student record, so that I can permanently remove students who are no longer in the system.
           TASK-40: Implement DELETE /api/students/{id} (Admin JWT) with cascade delete
           TASK-41: Add confirmation modal to Angular student list component
           TASK-42: Remove deleted row from DataTable on confirmed delete
    FEAT-08: Communication
        US-16: As a developer, I want the Angular frontend to communicate with the student and admin API endpoints, so that all data operations are persisted via the backend.
           TASK-43: Implement StudentApiService with all CRUD and assessment HTTP methods
           TASK-44: Wire StudentBusinessService to call StudentApiService methods
           TASK-45: Handle API errors gracefully in Angular components
        US-17: As a developer, I want the Angular frontend to communicate with the teacher API endpoints, so that login and activation functionality works end-to-end.
            TASK-46: Implement TeacherApiService (login and activate HTTP methods)
            TASK-47: Wire TeacherBusinessService to call TeacherApiService methods

 EPIC-03: Assessment
    FEAT-09: Scoring
        US-11: As a teacher, I want to add named assessments with flexible scoring, so that I can track different types of student performance.
           TASK-48: Create StudentAssessment entity and EF Core migration
           TASK-49: Implement POST /api/students/{id}/assessments endpoint
           TASK-50: Build inline Add Assessment form on Angular detail page
           TASK-51: Recalculate and refresh performance summary after add
        US-12: As a teacher, I want to view total, average, and percentage scores, so that I can quickly assess student performance.
           TASK-52: Implement score calculation logic in StudentService (total, avg, %)
           TASK-53: Include calculated fields in StudentDetailDto and StudentListDto
           TASK-54: Display Performance Summary section on Angular detail page
        US-13: As a teacher, I want each student to have a performance level badge, so that I can quickly identify students who need support.
           TASK-55: Implement GetPerformanceLevel(percentage) helper in service layer
           TASK-56: Apply colour-coded badge CSS classes in Angular list and detail templates
        US-21: As a teacher, I want to add a named assessment to a student, so that I can record specific achievements or tests.
           TASK-57: Create StudentAssessmentCreateDto with validation rules
           TASK-58: Build inline add-assessment form on detail page with validation
           TASK-59: Refresh assessment table and performance summary on success
        US-22: As a teacher, I want to edit and delete an individual assessment, so that I can correct mistakes or remove outdated records.
            TASK-60: Implement PUT /api/students/{id}/assessments/{assessmentId} endpoint
            TASK-61: Implement DELETE /api/students/{id}/assessments/{assessmentId} endpoint
            TASK-62: Build inline edit row mode with save/cancel in Angular detail component
            TASK-63: Update performance summary immediately on edit or delete success

 EPIC-04: Data Display & Interaction
    FEAT-10: DataTables Integration
        US-14: As a teacher, I want to sort students by column, so that I can organize the list by performance or other criteria.
           TASK-64: Configure DataTables columnDefs to sort Performance by hidden % value
           TASK-65: Verify all columns sort correctly ascending and descending
        US-15: As a teacher, I want to search and filter students, so that I can quickly find specific students or groups.
            TASK-66: Enable DataTables global search input
            TASK-67: Verify search filters all visible columns in real-time

 EPIC-05: API & Documentation
    FEAT-11: API Documentation
        US-19: As a developer, I want to explore the API via Swagger UI, so that I can understand and test available endpoints.
            TASK-68: Register Swagger/OpenAPI in Program.cs
            TASK-69: Add XML doc comments to all controller actions
            TASK-70: Verify Swagger UI lists all endpoints with request/response models

 EPIC-06: Student Portal
    FEAT-12: Student Account Activation
       US-23: As a student, I want to activate my account using my Student ID and email, so that I can set a password and access my dashboard.
          TASK-71: Create StudentActivateDto with StudentUniqueId, Email, Password fields
          TASK-72: Implement POST /api/students/activate controller action
          TASK-73: Build Angular student activate component (student-activate.component.ts)
          TASK-74: Redirect to /student/dashboard on successful account activation
       US-24: As a student, I want activation form fields to be validated, so that I cannot submit incomplete or invalid information.
           TASK-75: Add Angular validators (required, STU-format pattern, email, minlength 6)
           TASK-76: Implement StudentActivateValidator using FluentValidation rules
           TASK-77: Show confirmPassword mismatch error on frontend before submission
    FEAT-13: Student Login
       US-25: As a student, I want to log in with my Student ID and password, so that I can access my personal performance dashboard.
          TASK-78: Create StudentLoginDto and StudentLoginResponseDto
          TASK-79: Implement POST /api/students/login controller action
          TASK-80: Build Angular student login component (student-login.component.ts)
          TASK-81: Store student session in StudentAuthStateService on successful response
       US-26: As a student, I want to see clear error messages when my login credentials are invalid, so that I understand why access is denied.
           TASK-82: Return 401 Unauthorized for wrong credentials / 400 for unactivated account
           TASK-83: Display friendly error message in Angular on failed student login
    FEAT-14: Student Dashboard
       US-27: As a student, I want to view my personal performance summary, so that I can track my academic progress.
          TASK-84: Create StudentProfileDto with calculated score fields
          TASK-85: Return StudentProfileDto with computed scores on login/activation response
          TASK-86: Build Angular student dashboard with performance summary cards
          TASK-87: Display progress bar with colour-coded performance band legend
       US-28: As a student, I want to view my personal assessment list, so that I can see all my results in one place.
          TASK-88: Include assessments array in StudentProfileDto from API response
          TASK-89: Render assessments table in Angular student dashboard
          TASK-90: Apply Overdue/Submitted status badge based on due date
       US-29: As a student, I want to view my personal profile information, so that I can verify my details are correct.
           TASK-91: Map all student personal fields in StudentProfileDto
           TASK-92: Render My Profile section in Angular student dashboard component
    FEAT-15: Assessment File Submissions
        US-30: As a student, I want to upload a file submission for an assessment, so that I can submit my completed work digitally.
           TASK-93: Create AssessmentSubmission entity and EF Core migration
           TASK-94: Implement POST /api/students/{id}/assessments/{id}/submissions endpoint (Student JWT only)
           TASK-95: Build file upload modal in Angular student dashboard
           TASK-96: Display filename, upload date, and download link in submissions panel
        US-31: As a teacher, I want to download and delete student file submissions, so that I can review and manage submitted work.
            TASK-97: Implement GET /api/students/{id}/assessments/{id}/submissions (Teacher JWT)
            TASK-98: Implement GET .../submissions/{id}/download endpoint (Teacher or owning Student JWT)
            TASK-99: Implement DELETE /api/students/{id}/assessments/{id}/submissions/{id} endpoint

 EPIC-07: Admin Management
    FEAT-16: Admin Authentication
       US-33: As an admin, I want to log in using my email and password, so that I can access the admin management panel.
           TASK-104: Create AdminLoginDto and AdminLoginResponseDto (with Admin JWT)
           TASK-105: Implement POST /api/admins/login controller action
           TASK-106: Build Angular admin login component (admin-login.component.ts) at /admin/login
           TASK-107: Store admin session in AdminAuthStateService; implement adminAuthGuard and adminGuestGuard
    FEAT-17: Teacher and Student Onboarding
       US-34: As an admin, I want to create and manage teacher accounts, so that new teachers can be onboarded and existing ones updated or removed.
          TASK-108: Enforce Admin JWT on POST/GET/PUT/DELETE /api/teachers endpoints
          TASK-109: Build Angular teacher management section in admin panel
          TASK-110: Display teacher list with create / edit / delete actions in admin panel
       US-35: As an admin, I want to assign and unassign teachers to students, so that teachers can access only their relevant students.
           TASK-111: Implement POST /api/students/{sid}/teachers/{tid} (Admin JWT)
           TASK-112: Implement DELETE /api/students/{sid}/teachers/{tid} (Admin JWT)
           TASK-113: Build teacher-assignment UI in admin panel student detail view
    FEAT-18: Audit Logging
        US-36: As an admin, I want to view an immutable audit log of all system changes, so that I can monitor activity and investigate issues.
            TASK-114: Create AuditLog entity and EF Core migration
            TASK-115: Write AuditLog entries on every Create / Update / Delete operation via AuditLogService
            TASK-116: Implement GET /api/audit-logs with pagination and filtering (Admin JWT)
            TASK-117: Build Angular audit log page in admin panel

 EPIC-08: Notifications
    FEAT-19: Email Notifications
        US-37: As a student, I want to receive an email notification when a new assessment is added to my record, so that I am kept informed of my academic progress.
            TASK-118: Configure MailKit SMTP in appsettings.json and register EmailService in DI
            TASK-119: Trigger fire-and-forget email via EmailService when POST /api/students/{id}/assessments succeeds
            TASK-120: Template email with assessment name, score, and performance level

 EPIC-09: Data Export
    FEAT-20: Reports
        US-38: As a teacher, I want to export the full student list to a CSV file, so that I can analyse class performance in spreadsheet tools.
           TASK-121: Implement GET /api/reports/students/csv using CsvHelper (Teacher JWT)
           TASK-122: Build export button in Angular student list that triggers CSV download
        US-39: As a teacher, I want to export an individual student's assessment report as CSV or PDF, so that I can share a detailed performance record.
            TASK-123: Implement GET /api/reports/students/{id}/csv using CsvHelper (Teacher JWT)
            TASK-124: Implement GET /api/reports/students/{id}/pdf using QuestPDF (Teacher JWT)
            TASK-125: Add CSV and PDF download buttons to the Angular student detail page

 EPIC-10: Class Groups
     FEAT-21: Class Group Management
         US-40: As a teacher, I want to create and manage named class groups linked to a subject and grade, so that I can organise students into meaningful teaching units.
            TASK-126: Create ClassGroup entity (Name, Subject, Grade FK) and EF Core migration
            TASK-127: Implement POST /api/class-groups (Teacher JWT) and GET /api/class-groups
            TASK-128: Implement PUT /api/class-groups/{id} and DELETE /api/class-groups/{id} (Teacher JWT)
            TASK-129: Build Angular class-groups component with create / edit / delete UI
         US-41: As a teacher, I want to enrol and unenrol students in my class groups, so that each group reflects its actual class membership.
             TASK-130: Create ClassGroupStudent join entity and EF Core migration
             TASK-131: Implement POST /api/class-groups/{id}/students/{sid} (Teacher JWT)
             TASK-132: Implement DELETE /api/class-groups/{id}/students/{sid} (Teacher JWT)
             TASK-133: Build enrol / unenrol UI on the Angular class group detail page
```

---

## Scrum Framework

### What is Scrum?

Scrum is an Agile framework that organizes work into short, time-boxed iterations called **Sprints** (1-week each in this project). At the end of every Sprint, the team delivers a potentially shippable **Increment** of the product. Scrum promotes continuous improvement through three core pillars: **Transparency**, **Inspection**, and **Adaptation**.

---

### Scrum Team

| Role | Person | Responsibilities |
|------|--------|-----------------|
| **Product Owner** | School Administrator | Owns and prioritizes the Product Backlog; defines user stories based on business value; accepts completed increments at Sprint Reviews. |
| **Scrum Master** | Atreus Tefo Ramokate | Facilitates all Scrum events; removes impediments; ensures adherence to Scrum practices; coaches the team on Agile principles. |
| **Development Team** | Atreus Tefo Ramokate | Self-organizing, cross-functional team responsible for designing, building, and testing each Sprint increment  covering backend (ASP.NET Core), frontend (Angular), database (EF Core + SQL Server), and API testing (Postman). |

> **Note**: In this solo student project, Atreus Tefo Ramokate fulfills both the Scrum Master and Development Team roles, while the Product Owner perspective represents the teacher end-user's needs.

---

### Scrum Artifacts

#### 1. Product Backlog
A living, prioritized list of all work needed for the product. All 41 User Stories reside here, estimated in story points and ordered by business priority. The Product Owner is responsible for its content and ordering.

#### 2. Sprint Backlog
The set of Product Backlog items selected for a given Sprint, along with the Sprint Goal and the plan for delivering the Increment. Each Sprint section below contains its own Sprint Backlog.

#### 3. Increment
The sum of all completed and accepted Product Backlog items at the end of a Sprint. Each Increment must satisfy the **Definition of Done** and represent a working, tested build of the application.

---

### Scrum Events

| Event | Timing | Timebox | Purpose |
|-------|--------|---------|---------|
| **Sprint Planning** | Start of Sprint | 1 hour | Define the Sprint Goal; select and commit to Sprint Backlog items from the Product Backlog. |
| **Daily Scrum** | Every working day | 15 minutes | Synchronize progress using three questions: *What did I complete yesterday? What will I work on today? Are there any impediments?* |
| **Sprint Review** | End of Sprint | 30 minutes | Demonstrate the completed Increment to stakeholders; gather feedback; update the Product Backlog as needed. |
| **Sprint Retrospective** | After Sprint Review | 30 minutes | Reflect on team processes; identify improvements using the **Start / Stop / Continue** format. |

---

### Definition of Done (DoD)

A User Story is **Done** only when ALL of the following criteria are satisfied:

- [ ] All Tasks for the User Story are completed.
- [ ] All Acceptance Criteria for the story are met and verified.
- [ ] Code is committed and pushed to the `main` branch on GitHub.
- [ ] Backend endpoint (if applicable) is tested in Postman and returns the correct HTTP status and response body.
- [ ] Frontend component (if applicable) renders correctly at `http://localhost:4200` with no console errors.
- [ ] FluentValidation rules are enforced  invalid input returns HTTP 400 with structured error messages.
- [ ] No new compile errors or runtime exceptions are introduced.
- [ ] Swagger UI is updated to reflect any new or changed endpoints.

---

### Story Point Scale (Fibonacci)

| Points | Effort | Description |
|:------:|--------|-------------|
| 1 | Trivial | Configuration change, text update |
| 2 | Small | Simple UI badge, Swagger setup, minor handler |
| 3 | Medium | Form validation, FluentValidator, API error handling |
| 5 | Standard | Full end-to-end feature (form + API call + Angular service) |
| 8 | Large | Complex feature with multiple components and integration |
| 13 | Very Large | Entire Epic built from scratch |

---

### Product Backlog

All 41 User Stories, prioritized by business value, with story point estimates and Sprint assignments. **Total estimated effort: 144 story points**.

| ID | User Story | Priority | Points | Sprint |
|----|-----------|----------|:------:|:------:|
| US-18 | Reject invalid data server-side | High | 3 | Sprint 1 |
| US-19 | Explore API via Swagger UI | High | 2 | Sprint 1 |
| US-01 | Admin creates teacher account | High | 5 | Sprint 1 |
| US-02 | Validate teacher creation fields | High | 3 | Sprint 1 |
| US-03 | Teacher logs in with email and password | High | 5 | Sprint 2 |
| US-04 | Handle invalid teacher login credentials | High | 2 | Sprint 2 |
| US-05 | Admin adds a new student record | High | 5 | Sprint 2 |
| US-06 | Validate student input fields | High | 3 | Sprint 2 |
| US-16 | Consume student and admin API from frontend | High | 5 | Sprint 2 |
| US-07 | View all students in a table | High | 5 | Sprint 3 |
| US-08 | View detailed student profile | Medium | 3 | Sprint 3 |
| US-09 | Admin updates student information | Medium | 5 | Sprint 3 |
| US-10 | Admin deletes a student record | Medium | 3 | Sprint 3 |
| US-17 | Consume teacher API (login + activation) from frontend | Medium | 3 | Sprint 3 |
| US-20 | Select grade from controlled dropdown | Medium | 2 | Sprint 3 |
| US-23 | Activate student account | High | 3 | Sprint 3 |
| US-24 | Validate student activation form fields | High | 2 | Sprint 3 |
| US-25 | Student logs in with Student ID and password | High | 3 | Sprint 3 |
| US-26 | Handle invalid student credentials | High | 2 | Sprint 3 |
| US-32 | Teacher activates account with email and password | High | 3 | Sprint 3 |
| US-11 | Add named assessments with flexible scoring | Medium | 5 | Sprint 4 |
| US-21 | Add a named assessment to a student | Medium | 3 | Sprint 4 |
| US-22 | Edit and delete an individual assessment | Medium | 3 | Sprint 4 |
| US-12 | View total, average, and percentage | Medium | 3 | Sprint 4 |
| US-13 | View performance level badge | Medium | 2 | Sprint 4 |
| US-14 | Sort students by column | Low | 2 | Sprint 4 |
| US-15 | Search and filter students | Low | 2 | Sprint 4 |
| US-27 | View personal performance summary | Medium | 3 | Sprint 4 |
| US-28 | View personal assessment list | Medium | 3 | Sprint 4 |
| US-29 | View personal profile information | Medium | 2 | Sprint 4 |
| US-30 | Upload file submission for an assessment | Medium | 5 | Sprint 5 |
| US-31 | Download and delete file submissions | Medium | 3 | Sprint 5 |
| US-33 | Admin logs in to management panel | High | 3 | Sprint 6 |
| US-34 | Admin creates and manages teacher accounts | High | 5 | Sprint 6 |
| US-35 | Admin assigns / unassigns teachers to students | High | 3 | Sprint 6 |
| US-36 | Admin views immutable audit log | Medium | 5 | Sprint 6 |
| US-37 | Student receives email on new assessment | Medium | 3 | Sprint 7 |
| US-38 | Export full student list to CSV | Medium | 3 | Sprint 7 |
| US-39 | Export individual student report as CSV or PDF | Medium | 5 | Sprint 7 |
| US-40 | Teacher creates and manages class groups | Medium | 5 | Sprint 7 |
| US-41 | Teacher enrols and unenrols students in class groups | Medium | 3 | Sprint 7 |
| **Total** | | | **144** | |

---

### Sprint Plan

#### Sprint 1  Foundation & Authentication
**Dates**: March 28, 2026
**Sprint Goal**: *Establish project infrastructure, configure server-side validation and API documentation, and enable admins to create teacher accounts.*
**Velocity**: 13 story points

| Story | Title | Points | Status |
|-------|-------|:------:|:------:|
| US-18 | Reject invalid data server-side | 3 | Done |
| US-19 | Explore API via Swagger UI | 2 | Done |
| US-01 | Admin creates a teacher account | 5 | Done |
| US-02 | Validate teacher creation fields | 3 | Done |
| **Total** | | **13** | |

**Sprint Review**: Infrastructure and validation are fully functional. FluentValidation rejects invalid data with HTTP 400. Swagger UI documents all available endpoints. Backend infrastructure (layered Clean Architecture, EF Core, SQL Server LocalDB) is fully configured and running. *Note: The initial sprint planned teacher self-registration; the architecture subsequently evolved so that admins create teacher accounts and teachers activate them  the underlying validation and infrastructure work from this sprint remained valid and was reused.*

**Sprint Retrospective**:

| | Notes |
|-|-------|
| Start | Writing validation tests alongside each new endpoint |
| Stop | Skipping inline code comments  makes debugging harder later |
| Continue | Daily commits to GitHub to maintain a clear, traceable history |

---

#### Sprint 2  Authentication & Student CRUD
**Dates**: March 915, 2026
**Sprint Goal**: *Complete teacher login and enable teachers to create and manage students through the Angular frontend, fully connected to the RESTful API.*
**Velocity**: 20 story points

| Story | Title | Points | Status |
|-------|-------|:------:|:------:|
| US-03 | Log in with email and password | 5 | Done |
| US-04 | Handle invalid login credentials | 2 | Done |
| US-05 | Admin adds a new student record | 5 | Done |
| US-06 | Validate student input fields | 3 | Done |
| US-16 | Consume student and admin API from frontend | 5 | Done |
| **Total** | | **20** | |

**Sprint Review**: Teachers can log in and students can be created and managed via the Angular frontend. The Angular `StudentService` calls real API endpoints. Student creation triggers live feedback and redirects to the list. Node.js PATH fix applied and npm dependencies resolved; Angular dev server stable at `localhost:4200`. *Note: Student CRUD (create, edit, delete) is enforced with Admin JWT on the backend; the Angular admin panel manages these operations.*

**Sprint Retrospective**:

| | Notes |
|-|-------|
| Start | Using `proxy.conf.json` to avoid CORS issues during local development |
| Stop | Hard-coding API base URLs in components; move to Angular environment files |
| Continue | Committing working increments daily to maintain sprint momentum |

---

#### Sprint 3  Student Lifecycle, Views, Data Model Refactoring & Student Auth
**Dates**: March 1622, 2026
**Sprint Goal**: *Complete the full student management lifecycle  list view, detail view, edit, and delete  finalise teacher API integration in Angular, refactor the data model to support grade lookup and flexible per-student assessments, and deliver the student authentication portal (account activation and login).*
**Velocity**: 31 story points

| Story | Title | Points | Status |
|-------|-------|:------:|:------:|
| US-07 | View all students in a table | 5 | Done |
| US-08 | View detailed student profile | 3 | Done |
| US-09 | Admin updates student information | 5 | Done |
| US-10 | Admin deletes a student record | 3 | Done |
| US-17 | Consume teacher API (login + activation) from frontend | 3 | Done |
| US-20 | Select grade from controlled dropdown | 2 | Done |
| US-23 | Activate student account | 3 | Done |
| US-24 | Validate activation form fields | 2 | Done |
| US-25 | Log in with Student ID and password | 3 | Done |
| US-26 | Handle invalid student credentials | 2 | Done |
| **Total** | | **31** | |

**Sprint Review**: Full CRUD lifecycle for students is complete. Teachers can view all students, open individual detail pages, edit records, and delete with a confirmation step. All Angular routes (`/`, `/detail/:id`, `/edit/:id`) are functional. Teacher API integration is wired to the Angular frontend. Data model refactored: Grade is now a seeded lookup table (Grade 712), students reference it via `GradeId` FK; `IdPassportNo` and `StudentUniqueId` fields added; assessment scores extracted into the separate `StudentAssessments` table (EF Core migration `AddGradesAndAssessmentsRefactoring` applied March 18). Student list table expanded to show: Student ID (`StudentUniqueId`), Full Name, Email, Grade, Score (`totalScore/maxPossible`), and a colour-coded Performance Level badge; `StudentListDto` updated to carry all these fields from the API; DataTables `columnDefs` configured so the Performance column sorts by hidden numeric percentage. Student authentication portal completed: students activate their accounts at `/student/activate` using their teacher-assigned `StudentUniqueId` and registered email to set a password; students log in at `/student/login` using their `StudentUniqueId` and password. `StudentAuthStateService`, `studentAuthGuard`, and `studentGuestGuard` implemented for student session management.

**Sprint Retrospective**:

| | Notes |
|-|-------|
| Start | Breaking large components into smaller, focused Angular services |
| Stop | Mixing business logic directly into Angular components |
| Continue | Validating API responses in the Angular service layer before rendering |

---

#### Sprint 4  Assessment CRUD, Scoring, DataTables & Student Portal
**Dates**: March 2329, 2026
**Sprint Goal**: *Implement the full individual-assessment workflow (add, edit, delete), automated score calculations with performance level labels, enhance the student table with DataTables sorting, searching, and pagination, and deliver the student self-service performance dashboard.*
**Velocity**: 28 story points

| Story | Title | Points | Status |
|-------|-------|:------:|:------:|
| US-11 | Add named assessments with flexible scoring | 5 | Done |
| US-21 | Add a named assessment to a student | 3 | Done |
| US-22 | Edit and delete an individual assessment | 3 | Done |
| US-12 | View total, average, and percentage | 3 | Done |
| US-13 | View performance level badge | 2 | Done |
| US-14 | Sort students by column | 2 | Done |
| US-15 | Search and filter students | 2 | Done |
| US-27 | View personal performance summary | 3 | Done |
| US-28 | View personal assessment list | 3 | Done |
| US-29 | View personal profile information | 2 | Done |
| **Total** | | **28** | |

**Sprint Review**: All ten Sprint 4 User Stories delivered. Teachers can now add named assessments to any student with a custom `MaxScore` and optional `DueDate`; existing assessments are editable and deletable inline on the student detail page. Score calculations (total, average, percentage) are computed server-side in `StudentAssessmentService` and surfaced through `StudentDetailDto` and `StudentListDto`. Performance level badges (Needs Support / Satisfactory / Good / Excellent) are colour-coded and visible on both the student list and detail views. DataTables pagination, column sorting, and global search are fully configured; the Performance column sorts by a hidden numeric percentage value. The student self-service portal is complete: the dashboard displays summary cards (Total Score, Average, Percentage, Performance Level), a colour-coded progress bar, a personal assessments table with Overdue/Submitted badges, and a My Profile section  all populated from `StudentProfileDto` returned on login.

**Sprint Retrospective**:

| | Notes |
|-|-------|
| Start | Running the full Postman collection as a regression suite before each release |
| Stop | Manual testing only; introduce smoke tests for critical API paths |
| Continue | Keeping Swagger UI and Postman collection synchronized with the latest API |

---

#### Sprint 5  Assessment File Submissions
**Dates**: March 30  April 5, 2026
**Sprint Goal**: *Deliver the assessment file submission feature, allowing students to upload completed work and teachers to download and manage submitted files.*
**Velocity**: 8 story points

| Story | Title | Points | Status |
|-------|-------|:------:|:------:|
| US-30 | Upload file submission for an assessment | 5 | Done |
| US-31 | Download and delete file submissions | 3 | Done |
| **Total** | | **8** | |

**Sprint Review**: Students can upload PDF, DOC, DOCX, JPG, JPEG, or PNG files (max 10 MB) for any assigned assessment via the file upload modal on their dashboard. Each submission is stored server-side and associated with the relevant student and assessment. Teachers can view a list of all submissions for a given assessment on the student detail page, download any file, and delete submissions as needed. The `AssessmentSubmissionsController` enforces role-based access: only the owning student may upload; only teachers may list submissions; download and delete are available to both the owning student and any teacher. File size and type validation is enforced at the API boundary.

**Sprint Retrospective**:

| | Notes |
|-|-------|
| Start | Adding integration tests to cover file upload edge cases (size limit, unsupported type) |
| Stop | Storing raw file bytes without validating MIME type server-side |
| Continue | Clearing and reviewing the full Postman collection after each Sprint to catch any broken requests |

---

#### Sprint 6  Admin Management Platform
**Dates**: April 612, 2026
**Sprint Goal**: *Deliver the full admin management platform  admin login, teacher and student lifecycle management, teacher-student assignment, and an immutable audit log.*
**Velocity**: 16 story points

| Story | Title | Points | Status |
|-------|-------|:------:|:------:|
| US-33 | Admin logs in to management panel | 3 | Done |
| US-34 | Admin creates and manages teacher accounts | 5 | Done |
| US-35 | Admin assigns / unassigns teachers to students | 3 | Done |
| US-36 | Admin views immutable audit log | 5 | Done |
| **Total** | | **16** | |

**Sprint Review**: The admin management platform is fully operational. Admins log in at `/admin/login` using a dedicated Admin JWT (`POST /api/admins/login`). The admin panel exposes teacher creation and management (POST/GET/PUT/DELETE `/api/teachers` all require Admin JWT), teacher-to-student assignment/unassignment (`POST` and `DELETE /api/students/{sid}/teachers/{tid}`), and student record management. An `AuditLog` entity records all Create, Update, and Delete operations across the system; admins can browse the paginated log at `GET /api/audit-logs`. `AdminAuthStateService`, `adminAuthGuard`, and `adminGuestGuard` manage admin session state in Angular. Teacher activation (`POST /api/teachers/activate`) is public-access, allowing onboarded teachers to set their own passwords before their first login.

**Sprint Retrospective**:

| | Notes |
|-|-------|
| Start | Automating audit log assertions in the Postman collection as post-response tests |
| Stop | Sharing JWT secret configuration between roles without per-role isolation |
| Continue | Keeping all admin-scoped endpoints grouped in a dedicated Swagger tag for clarity |

---

#### Sprint 7  Notifications, Exports & Class Groups
**Dates**: April 1319, 2026
**Sprint Goal**: *Deliver automated email notifications on assessment creation, CSV and PDF data exports, and the class group management feature so teachers can organise students into teaching units.*
**Velocity**: 19 story points

| Story | Title | Points | Status |
|-------|-------|:------:|:------:|
| US-37 | Student receives email on new assessment | 3 | Done |
| US-38 | Export full student list to CSV | 3 | Done |
| US-39 | Export individual student report as CSV or PDF | 5 | Done |
| US-40 | Teacher creates and manages class groups | 5 | Done |
| US-41 | Teacher enrols and unenrols students in class groups | 3 | Done |
| **Total** | | **19** | |

**Sprint Review**: Email notifications are delivered fire-and-forget via MailKit SMTP whenever a new assessment is created (`POST /api/students/{id}/assessments`); the email includes the assessment name, score, and performance level. Data export endpoints are live: `GET /api/reports/students/csv` returns all students as CSV (CsvHelper); `GET /api/reports/students/{id}/csv` and `GET /api/reports/students/{id}/pdf` return per-student reports; the PDF is generated with QuestPDF. Angular download buttons on the student list and detail pages trigger these exports. Class group management is complete: teachers create named groups linked to a subject and grade (`POST /api/class-groups`), update or delete them, and enrol/unenrol students (`POST`/`DELETE /api/class-groups/{id}/students/{sid}`); the Angular class-groups component renders the full CRUD UI.

**Sprint Retrospective**:

| | Notes |
|-|-------|
| Start | Throttling email notifications to prevent repeated sends for bulk assessment imports |
| Stop | Generating PDFs synchronously on the request thread  move to background task for large reports |
| Continue | Keeping CsvHelper and QuestPDF configuration centralized in ExportService for maintainability |

---

## EPIC-01: Security

> **Goal**: Protect the application through a three-role authentication system (Admin / Teacher / Student), enforce data integrity rules on both the frontend and backend, and ensure that only authorised users can access sensitive operations.

---

### FEAT-01: Teacher Account Lifecycle

> **Description**: Admin creates a teacher account with personal details; the teacher then activates the account by setting their own password before logging in for the first time.

---

#### US-01: Admin Creates a Teacher Account

> **Story Points**: 5 &nbsp;|&nbsp; **Sprint**: Sprint 1

**As an** admin,
**I want** to create a teacher account by supplying the teacher's personal and professional details,
**so that** the teacher can be onboarded to the system and sent their credentials.

**Acceptance Criteria:**
- [ ] Admin panel form collects: ID/Passport No., first name, last name, email, phone, subject.
- [ ] ID/Passport No. is required, exactly 9 alphanumeric characters (letters and digits only).
- [ ] `POST /api/teachers` requires a valid Admin JWT; unauthenticated requests return 401.
- [ ] On successful submission, the new teacher record appears in the teacher list.
- [ ] A `201 Created` response is returned with the created teacher details.

**Tasks:**
- TASK-01: Create `TeacherCreateDto` with `IdPassportNo` and all required fields
- TASK-02: Implement `POST /api/teachers` (Admin JWT) controller action
- TASK-03: Build Angular teacher creation form in the admin panel
- TASK-04: Connect Angular form to API via `AdminTeacherService` and handle success feedback

**App Example:**
> The admin opens the Admin Panel, navigates to Teachers, clicks "Add Teacher", fills in Mrs. Smith's details (ID: `AB1234567`, email: `smith@school.com`, subject: `Mathematics`, phone: `12345678`), and clicks Create. Mrs. Smith appears in the teacher list with status "Pending Activation".

---

#### US-02: Validate Teacher Creation Fields

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 1

**As an** admin,
**I want** the teacher creation form to show inline errors for invalid inputs,
**so that** I know exactly what needs to be corrected before submitting.

**Acceptance Criteria:**
- [ ] ID/Passport No.: required, exactly 9 alphanumeric characters (letters and digits only).
- [ ] First/Last name: required, 250 characters.
- [ ] Email: required, must be a valid email format.
- [ ] Phone: required, exactly 8 digits.
- [ ] Subject: required, max 100 characters.
- [ ] Error messages appear inline next to the invalid field.
- [ ] The form cannot be submitted while validation errors exist.
- [ ] Backend (FluentValidation) also rejects invalid payloads with HTTP 400.

**Tasks:**
- TASK-05: Add Angular validators (required, minlength, pattern including `^[a-zA-Z0-9]+$` for IdPassportNo) to the admin teacher form
- TASK-06: Implement `TeacherCreateValidator` using FluentValidation rules
- TASK-07: Display inline error messages next to each invalid field

**App Example:**
> The admin types `"A"` for the teacher's first name and clicks Create. The form shows: *"First name must be 250 characters"*. Entering `"AB-12345"` as the ID is also rejected  only letters and digits are allowed.

---

### FEAT-02: Teacher Login & Activation

> **Description**: Allow a teacher who has been created by an admin to activate their account (setting their own password), and subsequently log in with email and password to access the student management dashboard.

---

#### US-03: Log In with Email and Password

> **Story Points**: 5 &nbsp;|&nbsp; **Sprint**: Sprint 2

**As a** registered teacher,
**I want** to log in using my email and password,
**so that** I can access the student management dashboard.

**Acceptance Criteria:**
- [ ] Login form collects email and password.
- [ ] On success, teacher is redirected to the students list (`/`).
- [ ] A session token/profile is stored for the authenticated session.

**Tasks:**
- TASK-08: Create `TeacherLoginDto` and login response model
- TASK-09: Implement `POST /api/teachers/login` controller action
- TASK-10: Build Angular login form component (`login-form.component.ts`)
- TASK-11: Store teacher session in `TeacherStateService` on successful response

**App Example:**
> Mrs. Smith navigates to `/login`, enters `smith@school.com` and her password, and clicks Login. She is redirected to the homepage showing all students.

---

#### US-04: Handle Invalid Login Credentials

> **Story Points**: 2 &nbsp;|&nbsp; **Sprint**: Sprint 2

**As a** teacher,
**I want** to see a friendly error message when I enter wrong credentials,
**so that** I understand my login failed and can try again.

**Acceptance Criteria:**
- [ ] If email or password is incorrect, a clear error message is displayed.
- [ ] The form does not redirect on failure.
- [ ] Error message does not reveal whether the email or password was wrong (security best practice).

**Tasks:**
- TASK-12: Return `401 Unauthorized` for invalid credentials in the API
- TASK-13: Display friendly error message in Angular on a failed login response

**App Example:**
> Mrs. Smith accidentally types the wrong password. The page displays: *"Invalid email or password. Please try again."*  she is not logged in and remains on the `/login` page.

---

#### US-32: Activate Teacher Account

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 3

**As a** teacher,
**I want** to activate my account using my registered email and a new password of my choice,
**so that** I can set my own credentials and log in to the system for the first time.

**Acceptance Criteria:**
- [ ] Activation form at `/activate` collects: email address, new password, and confirm password.
- [ ] Account can only be activated once  attempting to re-activate a previously activated account returns a clear error.
- [ ] On successful activation, the teacher is redirected to `/login`.
- [ ] `POST /api/teachers/activate` is a public endpoint (no JWT required).
- [ ] Password: required, minimum 6 characters; confirm password must match.

**Tasks:**
- TASK-100: Create `TeacherActivateDto` with `Email`, `Password`, and `ConfirmPassword` fields
- TASK-101: Implement `POST /api/teachers/activate` (public) controller action; hash password with BCrypt
- TASK-102: Build Angular teacher activate component (`teacher-activate.component.ts`) at `/activate`
- TASK-103: Redirect to `/login` on successful teacher account activation

**App Example:**
> Mrs. Smith receives her school email with the activation link. She opens `/activate`, enters `smith@school.com`, chooses a password, confirms it, and clicks Activate. She is redirected to `/login` and can now log in with her new credentials.

---

### FEAT-03: Input Validation

> **Description**: Enforce data integrity rules on both the Angular frontend and the ASP.NET Core backend, ensuring only clean, valid data enters the system.

---

#### US-06: Validate Student Input Fields

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 2

**As a** teacher,
**I want** the student form to validate my input before submitting,
**so that** only clean and correct data is saved to the system.

**Acceptance Criteria:**
- [ ] ID/Passport No.: required, exactly 9 characters, letters/numbers/hyphens only.
- [ ] First/Last name: required, 250 characters, letters/spaces/hyphens only.
- [ ] Email: required, valid email format, max 100 characters.
- [ ] Phone: required, exactly 8 digits.
- [ ] Grade: required, must select a valid grade level from the dropdown (GradeId > 0).
- [ ] Invalid fields display inline error messages.
- [ ] Backend (FluentValidation) rejects invalid payloads with a 400 response.

**Tasks:**
- TASK-14: Add Angular validators (pattern, required, min/maxlength) to student create/edit form
- TASK-15: Implement `StudentCreateValidator` using FluentValidation rules
- TASK-16: Show inline validation error messages on form submission attempt

**App Example:**
> Mrs. Smith leaves the Grade field on "-- Select Grade --" and submits. The form shows: *"A valid grade must be selected"* and blocks submission. If the API is called directly with `GradeId: 0`, FluentValidation returns `400 Bad Request` with the same message.

---

#### US-18: Reject Invalid Data Server-Side

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 1

**As a** system,
**I want** the API to validate all incoming data and reject invalid requests,
**so that** bad data never reaches the database regardless of frontend state.

**Acceptance Criteria:**
- [ ] Any request violating validation rules returns HTTP 400 Bad Request.
- [ ] The response body contains a structured list of validation errors.
- [ ] Student validation rules: IdPassportNo (9 chars), names (250 chars), email, phone (8 digits), GradeId (> 0).
- [ ] Assessment validation rules: Name required (max 100 chars), MaxScore > 0, Score  0 and  MaxScore.

**Tasks:**
- TASK-17: Register FluentValidation in `Program.cs` with `AddFluentValidationAutoValidation`
- TASK-18: Create validators for all DTOs (Teacher, Student, Assessment)
- TASK-19: Verify `400 Bad Request` with structured error body via Postman

**App Example:**
> A direct API call via Postman adds an assessment with `Score: 25` and `MaxScore: 20`. The API responds: `400 Bad Request` with body: `{ "errors": { "Score": ["Score cannot exceed the max score for this assessment"] } }`.

---

## EPIC-02: Student Management

> **Goal**: Allow admins to fully manage student records  creating, editing, and deleting  and enable teachers to view, search, and add assessments to their assigned students through an intuitive Angular interface backed by a RESTful API.

---

### FEAT-04: Create Student

> **Description**: Allow an admin to add a new student record with personal details and grade assignment. The student is then assigned to teachers and can activate their own account.

---

#### US-05: Admin Adds a New Student Record

> **Story Points**: 5 &nbsp;|&nbsp; **Sprint**: Sprint 2

**As an** admin,
**I want** to add a new student by filling in their details,
**so that** the student can be assigned to teachers and their performance can be tracked.

**Acceptance Criteria:**
- [ ] Admin panel form collects: ID/Passport No., first name, last name, email, phone, grade (selected from a controlled dropdown).
- [ ] A system-generated StudentUniqueId (e.g., `STU-A1B2C3D4`) is assigned automatically on creation.
- [ ] `POST /api/students` requires a valid Admin JWT; unauthenticated requests return 401.
- [ ] On successful submission, the student appears in the student list.

**Tasks:**
- TASK-20: Create `StudentCreateDto` with required fields
- TASK-21: Implement `POST /api/students` (Admin JWT) controller action
- TASK-22: Generate `StudentUniqueId` (`STU-XXXXXXXX`) in the service layer
- TASK-23: Build Angular student creation form in the admin panel
- TASK-24: Redirect to student list on successful creation

**App Example:**
> The admin opens the Admin Panel, clicks "Add Student", fills in: `ID/Passport No.: 123456789, John Doe, john@school.com, 12345678, Grade 10`, and clicks Create. John Doe appears in the students table with a generated `STU-A1B2C3D4` ID.

---

### FEAT-05: View Students

> **Description**: Allow teachers to see an overview of all students and drill into individual student details.

---

#### US-07: View All Students in a Table

> **Story Points**: 5 &nbsp;|&nbsp; **Sprint**: Sprint 3

**As a** teacher,
**I want** to see all students listed in a table,
**so that** I can get a quick overview of the class.

**Acceptance Criteria:**
- [ ] The home page (`/`) displays a table of all students.
- [ ] The table shows: Student ID (`StudentUniqueId`), Full Name, Email, Grade, Score (`totalScore / maxPossible` or "No assessments"), and Performance Level.
- [ ] Performance Level is displayed as a colour-coded badge (green = Excellent, blue = Good, yellow = Satisfactory, red = Needs Support).
- [ ] Students with no assessments show a muted "No assessments" score and no badge.
- [ ] The table is rendered using the DataTables library with sorting and pagination.
- [ ] The Performance column sorts by the underlying percentage value, not alphabetically by label.

**Tasks:**
- TASK-25: Create `StudentListDto` with all required display fields
- TASK-26: Implement `GET /api/students` returning `StudentListDto[]`
- TASK-27: Build Angular student list component (`student-list.component.ts`)
- TASK-28: Initialise DataTables with pagination, sorting, and hidden % sort column

**App Example:**
> Mrs. Smith opens the app and sees a table listing all 25 students  columns: Student ID (`STU-A1B2C3D4`), Full Name, Email, Grade, Score (`144/170`), and a colour-coded Performance badge (**Excellent** in green).

---

#### US-08: View Detailed Student Profile

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 3

**As a** teacher,
**I want** to click on a student and view their full profile,
**so that** I can see all their details and individual assessment scores.

**Acceptance Criteria:**
- [ ] A detail page at `/detail/:id` shows all student fields: StudentUniqueId, ID/Passport No., name, email, phone, grade.
- [ ] Shows the student's dynamic assessment list as a table: Name, Score/MaxScore, Percentage, Due Date.
- [ ] Overdue assessments are flagged with an "Overdue" badge.
- [ ] An inline Add Assessment form allows adding a new named assessment with a custom MaxScore and optional due date.
- [ ] Edit and Delete buttons on each assessment row allow updating or removing assessments inline.
- [ ] Performance Summary shows: Total Score/MaxPossible, Average %, Percentage, and Performance Level badge.
- [ ] Provides navigation back to the list and to the edit page.

**Tasks:**
- TASK-29: Create `StudentDetailDto` with embedded assessment list
- TASK-30: Implement `GET /api/students/{id}` returning `StudentDetailDto`
- TASK-31: Build Angular student detail component (`student-detail.component.ts`)
- TASK-32: Display performance summary and assessment table on detail page

**App Example:**
> Mrs. Smith clicks "View" on John Doe's row. She sees `/detail/1` with his assessments: *Test 1: 18/20 (90%), Assignment 2: 44/50 (88%)*. The Performance Summary shows *Total: 62/70, Percentage: 88.6%, Level: Excellent*.

---

#### US-20: Select Grade from Controlled Dropdown

> **Story Points**: 2 &nbsp;|&nbsp; **Sprint**: Sprint 3

**As a** teacher,
**I want** to select a student's grade level from a predefined dropdown (Grade 712),
**so that** grade data is consistent across all student records.

**Acceptance Criteria:**
- [ ] Student create and edit forms load available grades from `GET /api/grades`.
- [ ] The dropdown shows grade labels (e.g., "Grade 7", "Grade 8") ordered by level.
- [ ] A GradeId FK is stored on the student record instead of a free-text string.
- [ ] Selecting "-- Select Grade --" (GradeId = 0) is blocked by frontend and backend validation.

**Tasks:**
- TASK-33: Create `Grade` entity and seed Grade 712 via EF Core migration
- TASK-34: Implement `GET /api/grades` read-only endpoint
- TASK-35: Populate grade dropdown in Angular forms from the API response

**App Example:**
> Mrs. Smith opens the Create Student form. The Grade dropdown shows Grade 7 through Grade 12. She selects "Grade 10". The student record links to the `Grades` table row for Grade 10, not a string.

---

### FEAT-06: Edit Student

> **Description**: Allow an admin to update a student's personal information to correct mistakes or reflect changes.

---

#### US-09: Admin Updates Student Information

> **Story Points**: 5 &nbsp;|&nbsp; **Sprint**: Sprint 3

**As an** admin,
**I want** to edit a student's personal details,
**so that** I can correct mistakes or update their information.

**Acceptance Criteria:**
- [ ] Edit form at `/edit/:id` pre-populates personal details: ID/Passport No., first name, last name, email, phone, grade.
- [ ] Admin can change any personal detail field.
- [ ] `PUT /api/students/{id}` requires a valid Admin JWT.
- [ ] On save, the record is updated and the admin is redirected to the detail view.
- [ ] Assessment scores are managed separately via the inline assessment controls on the detail page.

**Tasks:**
- TASK-36: Create `StudentUpdateDto` for personal detail fields
- TASK-37: Implement `PUT /api/students/{id}` (Admin JWT) controller action
- TASK-38: Build Angular edit form pre-populated with current student data
- TASK-39: Redirect to detail view on successful update

**App Example:**
> The admin realizes Mrs. Smith mis-entered John Doe's email. They navigate to `/edit/1`, correct the email, and click Update. The record is immediately updated.

---

### FEAT-07: Delete Student

> **Description**: Allow an admin to permanently remove a student from the system with a confirmation step.

---

#### US-10: Admin Deletes a Student Record

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 3

**As an** admin,
**I want** to delete a student record with a confirmation step,
**so that** I don't accidentally remove students from the system.

**Acceptance Criteria:**
- [ ] A "Delete" button is available on the student list (Admin JWT required).
- [ ] Clicking Delete requests confirmation before proceeding.
- [ ] After deletion, the student is removed from the list immediately.
- [ ] `DELETE /api/students/{id}` requires a valid Admin JWT and returns 204 No Content.

**Tasks:**
- TASK-40: Implement `DELETE /api/students/{id}` (Admin JWT) with cascade delete for assessments
- TASK-41: Add confirmation modal to Angular student list component
- TASK-42: Remove the deleted row from the DataTable on confirmed delete

**App Example:**
> The admin clicks Delete on a student who has left the school. A confirmation dialog appears: *"Are you sure you want to delete this student?"*. They confirm, and the student is removed from the table.

---

### FEAT-08: Communication

> **Description**: Wire the Angular frontend to all RESTful API endpoints so every data operation is persisted through the backend.

---

#### US-16: Consume Student and Admin API from Frontend

> **Story Points**: 5 &nbsp;|&nbsp; **Sprint**: Sprint 2

**As a** developer,
**I want** the Angular frontend to communicate with the student, assessment, and admin API endpoints,
**so that** all data operations are persisted via the backend.

**Acceptance Criteria:**
- [ ] `GET /api/students`  retrieves all students (Teacher JWT).
- [ ] `POST /api/students`  creates a new student (Admin JWT).
- [ ] `GET /api/students/{id}`  retrieves a single student with their assessments (Teacher JWT).
- [ ] `PUT /api/students/{id}`  updates a student's personal details (Admin JWT).
- [ ] `DELETE /api/students/{id}`  deletes a student and all their assessments (Admin JWT).
- [ ] `GET /api/grades`  retrieves all grade levels for dropdown population.
- [ ] `GET /api/students/{id}/assessments`  retrieves all assessments for a student (Teacher JWT).
- [ ] `POST /api/students/{id}/assessments`  adds a new assessment (Teacher JWT).
- [ ] `PUT /api/students/{id}/assessments/{assessmentId}`  updates a single assessment (Teacher JWT).
- [ ] `DELETE /api/students/{id}/assessments/{assessmentId}`  deletes a single assessment (Teacher JWT).
- [ ] `POST /api/students/{sid}/teachers/{tid}`  assigns a teacher to a student (Admin JWT).
- [ ] `DELETE /api/students/{sid}/teachers/{tid}`  unassigns a teacher from a student (Admin JWT).
- [ ] All responses use consistent JSON shapes (StudentDto, GradeDto, StudentAssessmentDto).

**Tasks:**
- TASK-43: Implement `StudentApiService` with all CRUD and assessment HTTP methods
- TASK-44: Wire `StudentBusinessService` to call `StudentApiService` methods
- TASK-45: Handle API errors gracefully in Angular components

**App Example:**
> When a teacher loads the homepage, Angular fires `GET /api/students`  the API returns `StudentDto[]`  DataTables renders them. Adding an assessment calls `POST /api/students/{id}/assessments`. The admin creating a student calls `POST /api/students` with an Admin JWT.

---

#### US-17: Consume Teacher API (Login + Activation) from Frontend

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 3

**As a** developer,
**I want** the Angular frontend to communicate with the teacher API endpoints,
**so that** login and account activation functionality work end-to-end.

**Acceptance Criteria:**
- [ ] `POST /api/teachers/activate`  activates a teacher account (public).
- [ ] `POST /api/teachers/login`  authenticates a teacher and returns a JWT.
- [ ] Login response includes teacher profile data and the JWT stored in `TeacherStateService`.
- [ ] Angular `teacherAuthGuard` and `teacherGuestGuard` protect teacher-scoped routes.

**Tasks:**
- TASK-46: Implement `TeacherApiService` with activate and login HTTP methods
- TASK-47: Wire `TeacherBusinessService` to call `TeacherApiService` methods

**App Example:**
> When Mrs. Smith submits the activation form, Angular sends `POST /api/teachers/activate` with her email and new password. The API returns 200, and she is redirected to `/login`. On login, `POST /api/teachers/login` returns a JWT stored in `TeacherStateService`.

---

## EPIC-03: Assessment

> **Goal**: Enable teachers to record any number of individually named assessments per student with flexible scoring, and automatically compute total, average, percentage, and performance level.

---

### FEAT-09: Scoring

> **Description**: Manage individual assessment entries (add, edit, delete) and automatically calculate running performance metrics.

---

#### US-11: Add Named Assessments with Flexible Scoring

> **Story Points**: 5 &nbsp;|&nbsp; **Sprint**: Sprint 4

**As a** teacher,
**I want** to add individually named assessment entries to a student's profile, each with its own maximum score,
**so that** I can record any assessment type (test, assignment, exam) on any marking scale I choose.

**Acceptance Criteria:**
- [ ] From the student detail page, a teacher can add an assessment with a custom Name (e.g., "Test 1", "Final Exam").
- [ ] Each assessment has a teacher-defined MaxScore (any positive value: 20, 50, 100, etc.).
- [ ] Score must be  0 and  MaxScore for that assessment.
- [ ] An optional DueDate can be recorded; overdue assessments are flagged with an "Overdue" badge in the UI.
- [ ] Assessments are stored independently and retrievable via `GET /api/students/{id}/assessments`.

**Tasks:**
- TASK-48: Create `StudentAssessment` entity and EF Core migration
- TASK-49: Implement `POST /api/students/{id}/assessments` endpoint
- TASK-50: Build inline Add Assessment form on the Angular detail page
- TASK-51: Recalculate and refresh the performance summary after a successful add

**App Example:**
> Mrs. Smith opens John Doe's detail page and adds: "Test 1" (MaxScore: 20, Score: 18, Due: 05/03/2026), "Assignment 2" (MaxScore: 50, Score: 44), "Final Exam" (MaxScore: 100, Score: 82). Each is saved immediately and reflected in the performance summary.

---

#### US-12: View Total, Average, and Percentage

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 4

**As a** teacher,
**I want** the system to automatically calculate a student's total, average, and percentage score,
**so that** I don't have to calculate them manually.

**Acceptance Criteria:**
- [ ] Total Score = sum of Score across all student assessments.
- [ ] Max Possible = sum of MaxScore across all student assessments.
- [ ] Percentage = (Total Score / Max Possible) × 100.
- [ ] Average = mean of per-assessment percentage: avg((score / maxScore) × 100) across all assessments.
- [ ] Results are displayed in the Performance Summary section on the student detail page and in the list table.
- [ ] When no assessments exist the system displays "No Assessments" instead of calculated values.

**Tasks:**
- TASK-52: Implement score calculation logic in `StudentService` (total, average, percentage)
- TASK-53: Include calculated fields in `StudentDetailDto` and `StudentListDto`
- TASK-54: Display the Performance Summary section on the Angular detail page

**App Example:**
> John Doe has three assessments: Test 1 (18/20), Assignment 2 (44/50), Final Exam (82/100). The system displays: *Total: 144/170, Percentage: 84.7%, Average: 85.2%, Level: Excellent*  updated automatically each time an assessment is added, edited, or deleted.

---

#### US-13: View Performance Level Badge

> **Story Points**: 2 &nbsp;|&nbsp; **Sprint**: Sprint 4

**As a** teacher,
**I want** each student to have a performance level label,
**so that** I can quickly identify students who need support.

**Acceptance Criteria:**
- [ ] Performance levels are calculated as follows:
  - **Needs Support**: Percentage < 50%
  - **Satisfactory**: Percentage 5055%
  - **Good**: Percentage 5675%
  - **Excellent**: Percentage > 75%
- [ ] The label is visible in both the list table and the detail view.

**Tasks:**
- TASK-55: Implement `GetPerformanceLevel(percentage)` helper method in the service layer
- TASK-56: Apply colour-coded badge CSS classes in Angular list and detail templates

**App Example:**
> John Doe's Percentage is 90%  label shows **"Excellent"**.
> A struggling student with Total = 25  Percentage = 41.7%  label shows **"Needs Support"**.

---

#### US-21: Add a Named Assessment to a Student

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 4

**As a** teacher,
**I want** to add a named assessment entry to a student's profile from the detail page,
**so that** I can record any test, assignment, or exam result using the scale I choose.

**Acceptance Criteria:**
- [ ] An inline "Add Assessment" form on the detail page collects: Name, MaxScore, Score, and optional DueDate.
- [ ] `POST /api/students/{id}/assessments` creates the entry and returns 201 Created.
- [ ] The assessment table and performance summary update immediately after adding.
- [ ] FluentValidation rejects: empty name, MaxScore  0, Score < 0, Score > MaxScore.

**Tasks:**
- TASK-57: Create `StudentAssessmentCreateDto` with validation rules
- TASK-58: Build inline add-assessment form on the detail page with validation messages
- TASK-59: Refresh assessment table and performance summary on successful add

**App Example:**
> Mrs. Smith opens John Doe's detail page and fills in: Name = "Test 1", MaxScore = 20, Score = 17. She clicks Add. The assessment appears in the table and the Performance Summary recalculates instantly.

---

#### US-22: Edit and Delete an Individual Assessment

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 4

**As a** teacher,
**I want** to edit or delete an individual assessment record inline on the student detail page,
**so that** I can correct entry mistakes without affecting the rest of the student's record.

**Acceptance Criteria:**
- [ ] An "Edit" button on each assessment row switches the row into an editable form.
- [ ] `PUT /api/students/{id}/assessments/{assessmentId}` updates the record on save.
- [ ] A "Delete" button on each row requests confirmation before calling `DELETE /api/students/{id}/assessments/{assessmentId}`.
- [ ] On success, the table and performance summary update immediately.
- [ ] The API returns 404 if the assessment does not belong to the specified student.

**Tasks:**
- TASK-60: Implement `PUT /api/students/{id}/assessments/{assessmentId}` endpoint
- TASK-61: Implement `DELETE /api/students/{id}/assessments/{assessmentId}` endpoint
- TASK-62: Build inline edit row mode with save/cancel buttons in Angular detail component
- TASK-63: Update the performance summary immediately on edit or delete success

**App Example:**
> Mrs. Smith notices "Test 1" was entered with Score 17 instead of 19. She clicks Edit on that row, changes Score to 19, and clicks Save. The row updates and the Performance Summary increases.

---

## EPIC-04: Data Display & Interaction

> **Goal**: Provide an interactive, user-friendly table experience so teachers can efficiently browse, search, and sort student data.

---

### FEAT-10: DataTables Integration

> **Description**: Enhance the student list table with client-side sorting, real-time searching, and pagination using the DataTables library.

---

#### US-14: Sort Students by Column

> **Story Points**: 2 &nbsp;|&nbsp; **Sprint**: Sprint 4

**As a** teacher,
**I want** to sort the students table by any column (e.g., name or total score),
**so that** I can quickly find top-performing or struggling students.

**Acceptance Criteria:**
- [ ] All table columns are sortable (ascending/descending).
- [ ] Sorting is applied client-side via DataTables.
- [ ] Sort arrows are shown in the column headers.

**Tasks:**
- TASK-64: Configure DataTables `columnDefs` so the Performance column sorts by hidden numeric % value
- TASK-65: Verify all columns sort correctly ascending and descending

**App Example:**
> Mrs. Smith clicks the "Total Score" column header. The table re-orders from highest to lowest score, immediately showing her top-performing students.

---

#### US-15: Search and Filter Students

> **Story Points**: 2 &nbsp;|&nbsp; **Sprint**: Sprint 4

**As a** teacher,
**I want** to search for a student by name or email in the table,
**so that** I can find a specific student quickly without scrolling.

**Acceptance Criteria:**
- [ ] A search input box is shown above the table.
- [ ] Typing filters the table rows in real-time.
- [ ] Pagination is updated to reflect filtered results.

**Tasks:**
- TASK-66: Enable the DataTables global search input
- TASK-67: Verify the search filters all visible columns in real-time

**App Example:**
> Mrs. Smith types `"Doe"` in the search box. The table instantly filters to show only "John Doe", hiding all other students.

---

## EPIC-05: API & Documentation

> **Goal**: Provide a well-documented, interactive RESTful API that is easy for developers and testers to explore and verify.

---

### FEAT-11: API Documentation

> **Description**: Provide interactive API documentation via Swagger UI so developers and testers can explore and test all endpoints without writing code.

---

#### US-19: Explore API via Swagger UI

> **Story Points**: 2 &nbsp;|&nbsp; **Sprint**: Sprint 1

**As a** developer or tester,
**I want** to view and test all API endpoints in Swagger UI,
**so that** I can understand the API contract and verify endpoint behavior without needing Postman.

**Acceptance Criteria:**
- [ ] Swagger UI is available at `/swagger/ui` when the app is running.
- [ ] All endpoints (students, teachers, login) are listed.
- [ ] Each endpoint shows expected request body and response model.
- [ ] Requests can be executed directly from the Swagger UI.

**Tasks:**
- TASK-68: Register Swagger/OpenAPI in `Program.cs`
- TASK-69: Add XML doc comments to all controller actions
- TASK-70: Verify Swagger UI at `/swagger` lists all endpoints with request/response models

**App Example:**
> A new team member opens `http://localhost:5000/swagger/ui`, finds `POST /api/students`, clicks "Try it out", enters a student JSON payload, clicks Execute, and sees the 201 Created response  all without writing a single line of code.

---

## EPIC-06: Student Portal

> **Goal**: Allow students to activate their own accounts, securely log in, and access a self-service dashboard showing their personal performance summary, individual assessment results, and profile information  without any teacher involvement after the initial account creation.

---

### FEAT-12: Student Account Activation

> **Description**: Enable students to activate their system account using the teacher-assigned Student ID and their registered email, setting a password for future logins.

---

#### US-23: Activate Student Account

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 3

**As a** student,
**I want** to activate my account using my teacher-given Student ID and registered email,
**so that** I can set a password and gain access to my personal performance dashboard.

**Acceptance Criteria:**
- [ ] Activation form collects: Student ID (`STU-XXXXXXXX` format), email address, password, and confirm password.
- [ ] Account can only be activated once  attempting to re-activate a previously activated account shows a clear error.
- [ ] On successful activation, the student is automatically logged in and redirected to `/student/dashboard`.
- [ ] The students session is persisted in `localStorage` so page refresh maintains the login state.

**Tasks:**
- TASK-71: Create `StudentActivateDto` with `StudentUniqueId`, `Email`, and `Password` fields
- TASK-72: Implement `POST /api/students/activate` controller action in `StudentsController`
- TASK-73: Build Angular student activate component (`student-activate.component.ts`) at `/student/activate`
- TASK-74: Redirect to `/student/dashboard` on successful account activation

**App Example:**
> A student receives their `STU-AB12CD34` ID from their teacher. They open `/student/activate`, enter their ID, registered email (`student@school.com`), and choose a password. After clicking Activate, they are logged in and land on their personal dashboard showing their grades and assessments.

---

#### US-24: Validate Activation Form Fields

> **Story Points**: 2 &nbsp;|&nbsp; **Sprint**: Sprint 3

**As a** student,
**I want** the activation form to validate my inputs before submitting,
**so that** I am guided through entering the correct details the first time.

**Acceptance Criteria:**
- [ ] Student ID: required, must match `STU-XXXXXXXX` format (8 uppercase alphanumeric characters after `STU-`).
- [ ] Email: required, must be a valid email format.
- [ ] Password: required, minimum 6 characters.
- [ ] Confirm Password: required, must match password exactly.
- [ ] Inline error messages appear next to invalid fields.
- [ ] Form cannot be submitted while any validation error exists.

**Tasks:**
- TASK-75: Add Angular validators (`required`, `STU-format` pattern, `email`, `minlength 6`) to activation form
- TASK-76: Implement `StudentActivateValidator` using FluentValidation rules in backend
- TASK-77: Show confirmPassword mismatch error message on frontend before form submission

**App Example:**
> A student types `abc123` as their Student ID (wrong format). The form shows: *"Student ID must be in the format STU-XXXXXXXX"* and blocks submission. If they then type mismatched passwords, a second error appears: *"Passwords do not match"*.

---

### FEAT-13: Student Login

> **Description**: Allow a student who has already activated their account to log in using their Student ID and password to access their dashboard.

---

#### US-25: Log In with Student ID and Password

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 3

**As a** student,
**I want** to log in using my Student ID and password,
**so that** I can access my personal performance dashboard on return visits.

**Acceptance Criteria:**
- [ ] Login form collects Student ID and password.
- [ ] On successful login, student is redirected to `/student/dashboard`.
- [ ] Student session is stored in `localStorage` via `StudentAuthStateService`.
- [ ] An already-authenticated student visiting `/student/login` is redirected directly to the dashboard.

**Tasks:**
- TASK-78: Create `StudentLoginDto` (StudentUniqueId, Password) and `StudentLoginResponseDto` (token + profile)
- TASK-79: Implement `POST /api/students/login` controller action
- TASK-80: Build Angular student login component (`student-login.component.ts`) at `/student/login`
- TASK-81: Store student session in `StudentAuthStateService` on successful login response

**App Example:**
> A student who activated their account last week opens the app, navigates to `/student/login`, enters `STU-AB12CD34` and their password, and is taken straight to their dashboard without any teacher interaction.

---

#### US-26: Handle Invalid Student Credentials

> **Story Points**: 2 &nbsp;|&nbsp; **Sprint**: Sprint 3

**As a** student,
**I want** to see a user-friendly error message when my login fails,
**so that** I know whether I have the wrong credentials or need to activate my account first.

**Acceptance Criteria:**
- [ ] Wrong Student ID or password: displays *"Invalid Student ID or password. Please try again."*
- [ ] Account not yet activated: displays a message directing the student to the activation page.
- [ ] Error message does not reveal whether the Student ID or password specifically was wrong.
- [ ] The form remains on the login page  no redirect on failure.

**Tasks:**
- TASK-82: Return `401 Unauthorized` for wrong credentials and `400 Bad Request` for unactivated account in API
- TASK-83: Display context-appropriate friendly error message in Angular on failed student login

**App Example:**
> A student types the wrong password. The form shows: *"Invalid Student ID or password. Please try again."*  they remain on `/student/login`. If the account was never activated, a different message appears: *"Account not activated. Please sign up here."*

---

### FEAT-14: Student Dashboard

> **Description**: Provide a read-only personal performance view where a logged-in student can see their assessment results, calculated scores, performance level, and profile details.

---

#### US-27: View Personal Performance Summary

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 4

**As a** student,
**I want** to see a performance summary with my total score, average score, overall percentage, and performance level,
**so that** I can quickly understand my current academic standing.

**Acceptance Criteria:**
- [ ] Dashboard displays four summary cards: Total Score (`totalScore / maxPossible`), Average Score (`%`), Overall Percentage, and Performance Level badge.
- [ ] A progress bar visually represents the overall percentage.
- [ ] Progress bar includes a legend: *Needs Support (<50%)*, *Satisfactory (5055%)*, *Good (5675%)*, *Excellent (>75%)*.
- [ ] Performance level and progress bar change colour based on the percentage band.

**Tasks:**
- TASK-84: Create `StudentProfileDto` with calculated fields (`totalScore`, `maxPossible`, `averageScore`, `percentage`, `performanceLevel`)
- TASK-85: Return `StudentProfileDto` with computed scores in `StudentLoginResponseDto` from both login and activation endpoints
- TASK-86: Build Angular student dashboard component with four performance summary cards
- TASK-87: Display progress bar with colour-coded performance band legend

**App Example:**
> A student logs in and sees four cards: *"42.5 / 60  Total Score"*, *"70.8%  Average Score"*, *"70.8%  Overall Percentage"*, and *"Good  Performance Level"*, along with a green progress bar at 70.8%.

---

#### US-28: View Personal Assessment List

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 4

**As a** student,
**I want** to see a table of all my individual assessments including name, score, max score, percentage, due date, and submission status,
**so that** I can review each result and see which assessments are outstanding.

**Acceptance Criteria:**
- [ ] Assessments table shows columns: `#`, `Assessment`, `Score`, `Max Score`, `Percentage`, `Due Date`, `Status`.
- [ ] Status badge shows *Overdue* (red) if the due date has passed, or *Submitted* (green) otherwise.
- [ ] Each assessment row shows a per-assessment percentage badge styled by performance band.
- [ ] If no assessments exist, an empty state message is shown: *"No assessments recorded yet. Your teacher will add them soon."*

**Tasks:**
- TASK-88: Include `assessments` array in `StudentProfileDto` returned from the API on login/activation
- TASK-89: Render assessments table in Angular student dashboard with all seven columns
- TASK-90: Apply Overdue/Submitted status badge logic based on `dueDate` compared to todays date

**App Example:**
> Mrs. Smith recorded three assessments for a student. The student logs in and sees a table with *"Math Test 1"*, *"Essay"*, and *"Science Quiz"*, each showing score, percentage badge, due date, and a green *Submitted* badge.

---

#### US-29: View Personal Profile Information

> **Story Points**: 2 &nbsp;|&nbsp; **Sprint**: Sprint 4

**As a** student,
**I want** to see my personal profile information including my name, grade, contact details, and registration date,
**so that** I can verify that my details on record are correct.

**Acceptance Criteria:**
- [ ] Profile section displays: Full Name, Student Unique ID, Grade, Email, Phone Number, ID/Passport Number, Registration Date.
- [ ] Profile is read-only  students cannot edit their own details (only teachers can).
- [ ] An *Active* status indicator is shown alongside the profile header.

**Tasks:**
- TASK-91: Map all student personal fields in `StudentProfileDto` (`firstName`, `lastName`, `studentUniqueId`, `gradeName`, `email`, `phone`, `idPassportNo`, `createdAt`)
- TASK-92: Render *My Profile* section in Angular student dashboard component with all personal fields displayed

**App Example:**
> A student scrolls down on their dashboard and sees a *My Profile* card showing their name, `STU-AB12CD34` chip, Grade 10 badge, email, phone, ID/Passport number, and *"Member since March 2026"*  all read-only with no edit buttons.

---

### FEAT-15: Assessment File Submissions

> **Description**: Allow students to upload file submissions for their assigned assessments, and give teachers the ability to view, download, and delete those submissions.

---

#### US-30: Upload File Submission for an Assessment

> **Story Points**: 5 &nbsp;|&nbsp; **Sprint**: Sprint 5

**As a** student,
**I want** to upload a file for one of my assessments,
**so that** I can submit my completed work digitally to my teacher.

**Acceptance Criteria:**
- [ ] Student can upload a file (PDF, DOC, DOCX, JPG, JPEG, PNG) up to 10 MB via the dashboard upload modal.
- [ ] Only the authenticated student whose ID matches the route `studentId` may upload (Student JWT required).
- [ ] Submission is stored server-side and linked to the correct student and assessment.
- [ ] File upload modal in Angular dashboard is dismissed on success; the submissions panel refreshes automatically.
- [ ] Unsupported file types or files exceeding 10 MB return HTTP 400 Bad Request.

**Tasks:**
- TASK-93: Create `AssessmentSubmission` entity with `StudentId`, `AssessmentId`, `FileName`, `ContentType`, `FileData` (byte[]), `UploadedAt` and add EF Core migration
- TASK-94: Implement `POST /api/students/{studentId}/assessments/{assessmentId}/submissions` endpoint (Student JWT only; validates file type and size)
- TASK-95: Build file upload modal component in Angular student dashboard with file-picker input and progress feedback
- TASK-96: Refresh submissions panel in Angular dashboard and display filename, upload date, and a download link after successful upload

**App Example:**
> A student opens their dashboard, clicks *Upload* next to an assessment named "History Essay", selects `history_essay_final.pdf`, and submits. The modal closes and the submissions panel below the assessment row shows the new entry with the filename and today's date.

---

#### US-31: Download and Delete File Submissions

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 5

**As a** teacher,
**I want** to download and delete student file submissions,
**so that** I can retrieve completed work for marking and remove any incorrect or duplicate files.

**Acceptance Criteria:**
- [ ] Teacher can view a list of all submissions for a given assessment on the student detail page.
- [ ] Teacher can download any submission file; the browser initiates a file download with the original filename.
- [ ] Teacher can delete any submission; the row is removed from the submissions table immediately.
- [ ] A student may also download their own submission (owning student JWT).
- [ ] Unauthorized access (wrong role or wrong student) returns HTTP 403 Forbidden.

**Tasks:**
- TASK-97: Implement `GET /api/students/{studentId}/assessments/{assessmentId}/submissions` endpoint (Teacher JWT only; returns list of `AssessmentSubmissionDto`)
- TASK-98: Implement `GET .../submissions/{id}/download` endpoint (Teacher or owning Student JWT; streams file bytes with correct `Content-Disposition` header)
- TASK-99: Implement `DELETE /api/students/{studentId}/assessments/{assessmentId}/submissions/{id}` endpoint (Teacher or owning Student JWT)

**App Example:**
> Mrs. Smith opens a student's detail page, scrolls to the Submissions section for "History Essay", sees a PDF listed, clicks *Download* and the file saves to her computer. She then clicks *Delete* on an incorrectly uploaded image and it disappears from the table.

---

## EPIC-07: Admin Management

> **Goal**: Provide a dedicated admin platform so that a system administrator can log in, onboard teachers and students, manage teacher-student assignments, and review an immutable audit trail of all system changes.

---

### FEAT-16: Admin Authentication

> **Description**: Allow a system administrator to log in using their email and password and receive a dedicated Admin JWT, which is required for all admin-scoped endpoints.

---

#### US-33: Admin Logs In to the Management Panel

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 6

**As an** admin,
**I want** to log in using my email and password,
**so that** I can access the admin management panel and perform system administration tasks.

**Acceptance Criteria:**
- [ ] Login form at `/admin/login` collects email and password.
- [ ] `POST /api/admins/login` returns an Admin JWT and admin profile on success.
- [ ] Admin session is stored via `AdminAuthStateService`.
- [ ] `adminAuthGuard` protects all admin panel routes; `adminGuestGuard` redirects authenticated admins away from `/admin/login`.
- [ ] Invalid credentials return a friendly error message.

**Tasks:**
- TASK-104: Create `AdminLoginDto` and `AdminLoginResponseDto` (containing Admin JWT and profile)
- TASK-105: Implement `POST /api/admins/login` controller action
- TASK-106: Build Angular admin login component (`admin-login.component.ts`) at `/admin/login`
- TASK-107: Implement `AdminAuthStateService`, `adminAuthGuard`, and `adminGuestGuard`

**App Example:**
> The school IT admin opens `/admin/login`, enters their credentials, and is redirected to the Admin Panel dashboard. All admin-scoped actions (create teachers, assign students, view audit log) are now accessible.

---

### FEAT-17: Teacher and Student Onboarding

> **Description**: Allow admins to create and manage teacher accounts, and assign or unassign teachers to students so that each teacher can access only their relevant students.

---

#### US-34: Admin Creates and Manages Teacher Accounts

> **Story Points**: 5 &nbsp;|&nbsp; **Sprint**: Sprint 6

**As an** admin,
**I want** to create, view, edit, and delete teacher accounts from the admin panel,
**so that** I can manage the full lifecycle of teacher access to the system.

**Acceptance Criteria:**
- [ ] `POST /api/teachers` creates a new teacher (Admin JWT required).
- [ ] `GET /api/teachers` lists all teachers (Admin JWT required).
- [ ] `PUT /api/teachers/{id}` updates a teacher's details (Admin JWT required).
- [ ] `DELETE /api/teachers/{id}` removes a teacher (Admin JWT required).
- [ ] The admin panel displays a teacher list with Create / Edit / Delete actions.
- [ ] Newly created teachers are marked "Pending Activation" until they activate their accounts.

**Tasks:**
- TASK-108: Enforce Admin JWT on POST/GET/PUT/DELETE `/api/teachers` endpoints
- TASK-109: Build Angular teacher management section in the admin panel
- TASK-110: Display teacher list with create / edit / delete actions and activation status

**App Example:**
> The admin adds Mrs. Smith via the Admin Panel. She appears in the teacher list as "Pending Activation". The admin can later edit her subject or delete her account if she leaves.

---

#### US-35: Admin Assigns and Unassigns Teachers to Students

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 6

**As an** admin,
**I want** to assign and unassign teachers to individual students,
**so that** each teacher can see and manage only their own students.

**Acceptance Criteria:**
- [ ] `POST /api/students/{sid}/teachers/{tid}` assigns a teacher to a student (Admin JWT required).
- [ ] `DELETE /api/students/{sid}/teachers/{tid}` removes the assignment (Admin JWT required).
- [ ] The admin panel student detail view shows currently assigned teachers and provides assign/unassign controls.
- [ ] A teacher can only access students they are assigned to.

**Tasks:**
- TASK-111: Implement `POST /api/students/{sid}/teachers/{tid}` (Admin JWT)
- TASK-112: Implement `DELETE /api/students/{sid}/teachers/{tid}` (Admin JWT)
- TASK-113: Build teacher-assignment UI in the admin panel student detail view

**App Example:**
> The admin opens John Doe's record in the admin panel and assigns Mrs. Smith as his teacher. Mrs. Smith can now see John Doe in her students list. If she is unassigned, John Doe disappears from her view.

---

### FEAT-18: Audit Logging

> **Description**: Record an immutable entry for every Create, Update, and Delete operation in the system, and expose a paginated admin-only endpoint so administrators can review all system activity.

---

#### US-36: Admin Views Immutable Audit Log

> **Story Points**: 5 &nbsp;|&nbsp; **Sprint**: Sprint 6

**As an** admin,
**I want** to view a paginated, immutable audit log of all system changes,
**so that** I can monitor activity, track who changed what, and investigate any issues.

**Acceptance Criteria:**
- [ ] An `AuditLog` entry is written on every Create, Update, and Delete operation across all entities.
- [ ] Each entry records: entity type, entity ID, operation type, performed-by user, and timestamp.
- [ ] `GET /api/audit-logs` returns a paginated list of audit entries (Admin JWT required).
- [ ] The admin panel displays the audit log as a filterable table.
- [ ] Audit entries are immutable  they cannot be edited or deleted.

**Tasks:**
- TASK-114: Create `AuditLog` entity and EF Core migration
- TASK-115: Write `AuditLog` entries in `AuditLogService` and call it from all service Create/Update/Delete operations
- TASK-116: Implement `GET /api/audit-logs` with pagination and filtering (Admin JWT)
- TASK-117: Build Angular audit log page in the admin panel

**App Example:**
> The admin navigates to the Audit Log page in the admin panel and sees all recent changes: *"Student 'John Doe' created by admin@school.com at 14:30"*, *"Assessment 'Math Test 1' added to student 42 by teacher@school.com at 15:05"*.

---

## EPIC-08: Notifications

> **Goal**: Keep students informed of new academic activity by sending automatic email notifications when assessments are added to their record.

---

### FEAT-19: Email Notifications

> **Description**: Send a fire-and-forget email notification to a student whenever a new assessment is created for them, using MailKit and SMTP.

---

#### US-37: Student Receives Email on New Assessment

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 7

**As a** student,
**I want** to receive an email notification when a new assessment is added to my record,
**so that** I am immediately informed of new academic activity without having to check my dashboard.

**Acceptance Criteria:**
- [ ] An email is sent automatically when a teacher successfully creates a new assessment (`POST /api/students/{id}/assessments`).
- [ ] The email is sent asynchronously (fire-and-forget) so it does not block the API response.
- [ ] The email includes: student name, assessment name, score, max score, and performance level.
- [ ] SMTP settings (host, port, sender address) are configured in `appsettings.json`.
- [ ] A failed email send does not cause the API request to return an error.

**Tasks:**
- TASK-118: Configure MailKit SMTP client and register `EmailService` in the DI container
- TASK-119: Trigger fire-and-forget `EmailService.SendAssessmentNotificationAsync` when a new assessment is saved
- TASK-120: Template the email body with assessment name, score, max score, and performance level label

**App Example:**
> Mrs. Smith adds "Math Test 1" (Score: 18/20) to John Doe's profile. Within seconds, John Doe receives an email: *"A new assessment has been added: Math Test 1  18/20 (90%  Excellent)"*.

---

## EPIC-09: Data Export

> **Goal**: Allow teachers to export student performance data as CSV files and individual student reports as CSV or professionally formatted PDF documents.

---

### FEAT-20: Reports

> **Description**: Expose report endpoints that generate CSV and PDF exports of student data for use in spreadsheets, meetings, and parent communications.

---

#### US-38: Export Full Student List to CSV

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 7

**As a** teacher,
**I want** to export the full student list to a CSV file,
**so that** I can analyse class performance in spreadsheet tools such as Excel.

**Acceptance Criteria:**
- [ ] `GET /api/reports/students/csv` returns a downloadable CSV file (Teacher JWT required).
- [ ] The CSV includes all students with columns: Student ID, Name, Email, Grade, Total Score, Max Possible, Percentage, Performance Level.
- [ ] The browser initiates a file download when the Angular export button is clicked.
- [ ] The endpoint uses CsvHelper for mapping and serialisation.

**Tasks:**
- TASK-121: Implement `GET /api/reports/students/csv` using CsvHelper (Teacher JWT)
- TASK-122: Build "Export CSV" button in the Angular student list that triggers the download

**App Example:**
> Mrs. Smith clicks "Export CSV" on the students list page. Her browser downloads `students_export.csv` containing all 25 students with their scores and performance levels, ready to open in Excel.

---

#### US-39: Export Individual Student Report as CSV or PDF

> **Story Points**: 5 &nbsp;|&nbsp; **Sprint**: Sprint 7

**As a** teacher,
**I want** to export an individual student's assessment report as a CSV or a formatted PDF,
**so that** I can share a detailed performance record with the student or their parents.

**Acceptance Criteria:**
- [ ] `GET /api/reports/students/{id}/csv` returns a per-student CSV (Teacher JWT).
- [ ] `GET /api/reports/students/{id}/pdf` returns a professionally formatted PDF (Teacher JWT) generated by QuestPDF.
- [ ] The PDF includes: student profile header, assessment table, performance summary, and performance level badge.
- [ ] CSV and PDF download buttons are available on the Angular student detail page.

**Tasks:**
- TASK-123: Implement `GET /api/reports/students/{id}/csv` using CsvHelper (Teacher JWT)
- TASK-124: Implement `GET /api/reports/students/{id}/pdf` using QuestPDF (Teacher JWT)
- TASK-125: Add "Export CSV" and "Export PDF" download buttons to the Angular student detail page

**App Example:**
> Mrs. Smith opens John Doe's detail page and clicks "Export PDF". Her browser downloads a formatted PDF showing John's name, grade, all assessment results in a table, and a summary card with his overall performance level of "Excellent".

---

## EPIC-10: Class Groups

> **Goal**: Allow teachers to organise students into named class groups linked to a subject and grade level, and to manage group membership by enrolling and unenrolling students.

---

### FEAT-21: Class Group Management

> **Description**: Provide full CRUD management for class groups (create, read, update, delete) and membership controls (enrol / unenrol students), backed by a dedicated set of RESTful endpoints.

---

#### US-40: Teacher Creates and Manages Class Groups

> **Story Points**: 5 &nbsp;|&nbsp; **Sprint**: Sprint 7

**As a** teacher,
**I want** to create and manage named class groups linked to a subject and grade,
**so that** I can organise my students into meaningful teaching units.

**Acceptance Criteria:**
- [ ] `POST /api/class-groups` creates a new class group with Name, Subject, and Grade FK (Teacher JWT).
- [ ] `GET /api/class-groups` returns all class groups for the authenticated teacher (Teacher JWT).
- [ ] `PUT /api/class-groups/{id}` updates a class group's details (Teacher JWT).
- [ ] `DELETE /api/class-groups/{id}` removes a class group (Teacher JWT).
- [ ] The Angular class-groups component renders create / edit / delete UI for managing groups.

**Tasks:**
- TASK-126: Create `ClassGroup` entity (Name, Subject, Grade FK) and EF Core migration
- TASK-127: Implement `POST /api/class-groups` (Teacher JWT) and `GET /api/class-groups`
- TASK-128: Implement `PUT /api/class-groups/{id}` and `DELETE /api/class-groups/{id}` (Teacher JWT)
- TASK-129: Build Angular class-groups component with create / edit / delete UI

**App Example:**
> Mrs. Smith creates a group called "Grade 10 Mathematics". The group appears in her class groups list. She can edit the name later or delete the group at the end of the year.

---

#### US-41: Teacher Enrols and Unenrols Students in Class Groups

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 7

**As a** teacher,
**I want** to enrol and unenrol students in my class groups,
**so that** each group accurately reflects its current class membership.

**Acceptance Criteria:**
- [ ] `POST /api/class-groups/{id}/students/{sid}` enrols a student in a group (Teacher JWT).
- [ ] `DELETE /api/class-groups/{id}/students/{sid}` unenrols a student from a group (Teacher JWT).
- [ ] `GET /api/class-groups/{id}` returns the group details including the list of enrolled students.
- [ ] The Angular class group detail page shows enrolled students and provides enrol/unenrol controls.

**Tasks:**
- TASK-130: Create `ClassGroupStudent` join entity and EF Core migration
- TASK-131: Implement `POST /api/class-groups/{id}/students/{sid}` (Teacher JWT)
- TASK-132: Implement `DELETE /api/class-groups/{id}/students/{sid}` (Teacher JWT)
- TASK-133: Build enrol / unenrol UI on the Angular class group detail page

**App Example:**
> Mrs. Smith opens "Grade 10 Mathematics" and clicks "Add Student". She selects John Doe from the dropdown and clicks Enrol. John appears in the group's student list. At the end of the term she can unenrol students who have moved classes.

---

## Summary Table

| ID | Level | Title | Parent | Points | Sprint |
|----|-------|-------|--------|:------:|:------:|
| EPIC-01 | Epic | Security | Application |  |  |
| FEAT-01 | Feature | Teacher Account Lifecycle | EPIC-01 |  |  |
| US-01 | User Story | Admin Creates a Teacher Account | FEAT-01 | 5 | Sprint 1 |
| US-02 | User Story | Validate Teacher Creation Fields | FEAT-01 | 3 | Sprint 1 |
| FEAT-02 | Feature | Teacher Login & Activation | EPIC-01 |  |  |
| US-03 | User Story | Log In with Email and Password | FEAT-02 | 5 | Sprint 2 |
| US-04 | User Story | Handle Invalid Login Credentials | FEAT-02 | 2 | Sprint 2 |
| US-32 | User Story | Activate Teacher Account | FEAT-02 | 3 | Sprint 3 |
| FEAT-03 | Feature | Input Validation | EPIC-01 |  |  |
| US-06 | User Story | Validate Student Input Fields | FEAT-03 | 3 | Sprint 2 |
| US-18 | User Story | Reject Invalid Data Server-Side | FEAT-03 | 3 | Sprint 1 |
| EPIC-02 | Epic | Student Management | Application |  |  |
| FEAT-04 | Feature | Create Student | EPIC-02 |  |  |
| US-05 | User Story | Admin Adds a New Student Record | FEAT-04 | 5 | Sprint 2 |
| FEAT-05 | Feature | View Students | EPIC-02 |  |  |
| US-07 | User Story | View All Students in a Table | FEAT-05 | 5 | Sprint 3 |
| US-08 | User Story | View Detailed Student Profile | FEAT-05 | 3 | Sprint 3 |
| US-20 | User Story | Select Grade from Controlled Dropdown | FEAT-05 | 2 | Sprint 3 |
| FEAT-06 | Feature | Edit Student | EPIC-02 |  |  |
| US-09 | User Story | Admin Updates Student Information | FEAT-06 | 5 | Sprint 3 |
| FEAT-07 | Feature | Delete Student | EPIC-02 |  |  |
| US-10 | User Story | Admin Deletes a Student Record | FEAT-07 | 3 | Sprint 3 |
| FEAT-08 | Feature | Communication | EPIC-02 |  |  |
| US-16 | User Story | Consume Student and Admin API from Frontend | FEAT-08 | 5 | Sprint 2 |
| US-17 | User Story | Consume Teacher API (Login + Activation) from Frontend | FEAT-08 | 3 | Sprint 3 |
| EPIC-03 | Epic | Assessment | Application |  |  |
| FEAT-09 | Feature | Scoring | EPIC-03 |  |  |
| US-11 | User Story | Add Named Assessments with Flexible Scoring | FEAT-09 | 5 | Sprint 4 |
| US-12 | User Story | View Total, Average, and Percentage | FEAT-09 | 3 | Sprint 4 |
| US-13 | User Story | View Performance Level Badge | FEAT-09 | 2 | Sprint 4 |
| US-21 | User Story | Add a Named Assessment to a Student | FEAT-09 | 3 | Sprint 4 |
| US-22 | User Story | Edit and Delete an Individual Assessment | FEAT-09 | 3 | Sprint 4 |
| EPIC-04 | Epic | Data Display & Interaction | Application |  |  |
| FEAT-10 | Feature | DataTables Integration | EPIC-04 |  |  |
| US-14 | User Story | Sort Students by Column | FEAT-10 | 2 | Sprint 4 |
| US-15 | User Story | Search and Filter Students | FEAT-10 | 2 | Sprint 4 |
| EPIC-05 | Epic | API & Documentation | Application |  |  |
| FEAT-11 | Feature | API Documentation | EPIC-05 |  |  |
| US-19 | User Story | Explore API via Swagger UI | FEAT-11 | 2 | Sprint 1 |
| EPIC-06 | Epic | Student Portal | Application |  |  |
| FEAT-12 | Feature | Student Account Activation | EPIC-06 |  |  |
| US-23 | User Story | Activate Student Account | FEAT-12 | 3 | Sprint 3 |
| US-24 | User Story | Validate Activation Form Fields | FEAT-12 | 2 | Sprint 3 |
| FEAT-13 | Feature | Student Login | EPIC-06 |  |  |
| US-25 | User Story | Log In with Student ID and Password | FEAT-13 | 3 | Sprint 3 |
| US-26 | User Story | Handle Invalid Student Credentials | FEAT-13 | 2 | Sprint 3 |
| FEAT-14 | Feature | Student Dashboard | EPIC-06 |  |  |
| US-27 | User Story | View Personal Performance Summary | FEAT-14 | 3 | Sprint 4 |
| US-28 | User Story | View Personal Assessment List | FEAT-14 | 3 | Sprint 4 |
| US-29 | User Story | View Personal Profile Information | FEAT-14 | 2 | Sprint 4 |
| FEAT-15 | Feature | Assessment File Submissions | EPIC-06 |  |  |
| US-30 | User Story | Upload File Submission for an Assessment | FEAT-15 | 5 | Sprint 5 |
| US-31 | User Story | Download and Delete File Submissions | FEAT-15 | 3 | Sprint 5 |
| EPIC-07 | Epic | Admin Management | Application |  |  |
| FEAT-16 | Feature | Admin Authentication | EPIC-07 |  |  |
| US-33 | User Story | Admin Logs In to the Management Panel | FEAT-16 | 3 | Sprint 6 |
| FEAT-17 | Feature | Teacher and Student Onboarding | EPIC-07 |  |  |
| US-34 | User Story | Admin Creates and Manages Teacher Accounts | FEAT-17 | 5 | Sprint 6 |
| US-35 | User Story | Admin Assigns and Unassigns Teachers to Students | FEAT-17 | 3 | Sprint 6 |
| FEAT-18 | Feature | Audit Logging | EPIC-07 |  |  |
| US-36 | User Story | Admin Views Immutable Audit Log | FEAT-18 | 5 | Sprint 6 |
| EPIC-08 | Epic | Notifications | Application |  |  |
| FEAT-19 | Feature | Email Notifications | EPIC-08 |  |  |
| US-37 | User Story | Student Receives Email on New Assessment | FEAT-19 | 3 | Sprint 7 |
| EPIC-09 | Epic | Data Export | Application |  |  |
| FEAT-20 | Feature | Reports | EPIC-09 |  |  |
| US-38 | User Story | Export Full Student List to CSV | FEAT-20 | 3 | Sprint 7 |
| US-39 | User Story | Export Individual Student Report as CSV or PDF | FEAT-20 | 5 | Sprint 7 |
| EPIC-10 | Epic | Class Groups | Application |  |  |
| FEAT-21 | Feature | Class Group Management | EPIC-10 |  |  |
| US-40 | User Story | Teacher Creates and Manages Class Groups | FEAT-21 | 5 | Sprint 7 |
| US-41 | User Story | Teacher Enrols and Unenrols Students in Class Groups | FEAT-21 | 3 | Sprint 7 |
| **Totals** | | **41 User Stories · 133 Tasks** | | **144 pts** | **7 Sprints** |
