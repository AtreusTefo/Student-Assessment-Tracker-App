# Student Assessment Tracker — Agile Methodology
## Scrum Framework with Application > Epic > Feature > User Story > Task Hierarchy

## Overview

This document applies the **Scrum framework** to the Student Assessment Tracker project. Scrum is an Agile framework that delivers value in short, time-boxed iterations called **Sprints**. It provides structure through defined roles (Product Owner, Scrum Master, Development Team), artifacts (Product Backlog, Sprint Backlog, Increment), and events (Sprint Planning, Daily Scrum, Sprint Review, Sprint Retrospective).

The project work is organized using a **five-level Agile hierarchy** — **Application → Epics → Features → User Stories → Tasks** — which populates the Scrum **Product Backlog**. User Stories are estimated with **story points** (Fibonacci scale), prioritized by business value, and allocated to four 1-week Sprints. Each User Story is broken down into concrete **Tasks** representing individual implementation steps completed within a Sprint.

---

## Agile Hierarchy: Definitions

### Application
The **Application** is the complete product being built — the entire system delivered at the end of all Sprints. It sits at the top of the hierarchy and represents the overarching project goal.

> **This project**: *Student Assessment Tracker* — a full-stack web application enabling teachers to manage students, record assessments, and track performance — and allowing students to log in and view their own results through a self-service portal.

---

### Epic
An **Epic** is a large body of work representing a high-level business domain or major capability. Epics span multiple Sprints and are broken down into Features.

> **Format**: `EPIC-XX: <Title>` — A short name describing the business domain area.

---

### Feature
A **Feature** is a service or function that delivers business value to a user. It represents a specific capability within an Epic and is broken down into User Stories.

> **Format**: `FEAT-XX: <Title>` — Describes a specific capability within the Epic.

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
A **Task** is the smallest unit of work — a concrete, implementable step required to complete a User Story. Tasks are assigned and completed within a single Sprint day and are not separately estimated in story points.

> **Format**: `TASK-XX: <Action verb> + <specific implementation step>` — e.g., *"Create `StudentCreateDto` with required fields"*.

---

## Hierarchy Map

```
APPLICATION: Student Assessment Tracker
│
├── EPIC-01: Security
│   ├── FEAT-01: Teacher Registration
│   │   ├── US-01: As a teacher, I want to register with my full profile details, so that I can access the system securely.
│   │   │   ├── TASK-01: Create TeacherRegisterDto with IdPassportNo and all registration fields
│   │   │   ├── TASK-02: Implement POST /api/teachers controller action
│   │   │   ├── TASK-03: Build Angular registration form component
│   │   │   └── TASK-04: Connect Angular form to API via TeacherService
│   │   └── US-02: As a teacher, I want registration fields to be validated, so that I cannot submit incomplete or invalid information.
│   │       ├── TASK-05: Add Angular template-driven form validators
│   │       ├── TASK-06: Implement TeacherRegisterValidator (FluentValidation)
│   │       └── TASK-07: Display inline error messages on invalid fields
│   ├── FEAT-02: Teacher Login
│   │   ├── US-03: As a registered teacher, I want to log in using my email and password, so that I can access the student management dashboard.
│   │   │   ├── TASK-08: Create TeacherLoginDto and login response model
│   │   │   ├── TASK-09: Implement POST /api/teachers/login controller action
│   │   │   ├── TASK-10: Build Angular login form component
│   │   │   └── TASK-11: Store teacher session in TeacherStateService on success
│   │   └── US-04: As a teacher, I want to see clear error messages when my login credentials are invalid, so that only registered teachers can log in and I understand why access is denied.
│   │       ├── TASK-12: Return 401 Unauthorized for invalid credentials in API
│   │       └── TASK-13: Display friendly error message in Angular on failed login
│   └── FEAT-03: Input Validation
│       ├── US-06: As a teacher, I want student input fields to be validated, so that I cannot submit incomplete or invalid student data.
│       │   ├── TASK-14: Add Angular validators to student create/edit form
│       │   ├── TASK-15: Implement StudentCreateValidator (FluentValidation)
│       │   └── TASK-16: Show inline validation errors on form submission
│       └── US-18: As a user, I want the server to reject invalid data, so that only valid information is saved in the system.
│           ├── TASK-17: Register FluentValidation in Program.cs
│           ├── TASK-18: Create validators for all DTOs (Teacher, Student, Assessment)
│           └── TASK-19: Verify 400 Bad Request with structured error body in Postman
│
├── EPIC-02: Student Management
│   ├── FEAT-04: Create Student
│   │   └── US-05: As a teacher, I want to add a new student record, so that I can manage my class list.
│   │       ├── TASK-20: Create StudentCreateDto with required fields
│   │       ├── TASK-21: Implement POST /api/students controller action
│   │       ├── TASK-22: Generate StudentUniqueId (STU-XXXXXXXX) in service layer
│   │       ├── TASK-23: Build Angular student creation form component
│   │       └── TASK-24: Redirect to student list on successful creation
│   ├── FEAT-05: View Students
│   │   ├── US-07: As a teacher, I want to view all my students and their performance, so that I can monitor progress and identify students who need support.
│   │   │   ├── TASK-25: Create StudentListDto with all display fields
│   │   │   ├── TASK-26: Implement GET /api/students returning StudentListDto array
│   │   │   ├── TASK-27: Build Angular student list component
│   │   │   └── TASK-28: Initialise DataTables with pagination, sorting, hidden % column
│   │   ├── US-08: As a teacher, I want to view a detailed student profile, so that I can see individual performance and assessment history.
│   │   │   ├── TASK-29: Create StudentDetailDto with embedded assessment list
│   │   │   ├── TASK-30: Implement GET /api/students/{id} returning StudentDetailDto
│   │   │   ├── TASK-31: Build Angular student detail component
│   │   │   └── TASK-32: Display performance summary and assessment table on detail page
│   │   └── US-20: As a teacher, I want to select a grade from a controlled dropdown, so that student records are consistent and accurate.
│   │       ├── TASK-33: Create Grade entity and seed Grade 7–12 (EF Core migration)
│   │       ├── TASK-34: Implement GET /api/grades read-only endpoint
│   │       └── TASK-35: Populate grade dropdown in Angular forms from API response
│   ├── FEAT-06: Edit Student
│   │   └── US-09: As a teacher, I want to edit student details and assessments, so that I can keep student information up to date.
│   │       ├── TASK-36: Create StudentUpdateDto for personal details
│   │       ├── TASK-37: Implement PUT /api/students/{id} controller action
│   │       ├── TASK-38: Build Angular edit form pre-populated with current student data
│   │       └── TASK-39: Redirect to detail view on successful update
│   ├── FEAT-07: Delete Student
│   │   └── US-10: As a teacher, I want to delete a student record, so that I can remove students who are no longer in my class.
│   │       ├── TASK-40: Implement DELETE /api/students/{id} with cascade delete
│   │       ├── TASK-41: Add confirmation modal to Angular student list component
│   │       └── TASK-42: Remove deleted row from DataTable on confirmed delete
│   └── FEAT-08: Communication
│       ├── US-16: As a developer, I want the Angular frontend to communicate with the student API endpoints, so that all student data operations are persisted via the backend.
│       │   ├── TASK-43: Implement StudentApiService with all CRUD and assessment HTTP methods
│       │   ├── TASK-44: Wire StudentBusinessService to call StudentApiService methods
│       │   └── TASK-45: Handle API errors gracefully in Angular components
│       └── US-17: As a developer, I want the Angular frontend to communicate with the teacher API endpoints, so that registration and login functionality works end-to-end.
│           ├── TASK-46: Implement TeacherApiService (register and login HTTP methods)
│           └── TASK-47: Wire TeacherBusinessService to call TeacherApiService methods
│
├── EPIC-03: Assessment
│   └── FEAT-09: Scoring
│       ├── US-11: As a teacher, I want to add named assessments with flexible scoring, so that I can track different types of student performance.
│       │   ├── TASK-48: Create StudentAssessment entity and EF Core migration
│       │   ├── TASK-49: Implement POST /api/students/{id}/assessments endpoint
│       │   ├── TASK-50: Build inline Add Assessment form on Angular detail page
│       │   └── TASK-51: Recalculate and refresh performance summary after add
│       ├── US-12: As a teacher, I want to view total, average, and percentage scores, so that I can quickly assess student performance.
│       │   ├── TASK-52: Implement score calculation logic in StudentService (total, avg, %)
│       │   ├── TASK-53: Include calculated fields in StudentDetailDto and StudentListDto
│       │   └── TASK-54: Display Performance Summary section on Angular detail page
│       ├── US-13: As a teacher, I want each student to have a performance level badge, so that I can quickly identify students who need support.
│       │   ├── TASK-55: Implement GetPerformanceLevel(percentage) helper in service layer
│       │   └── TASK-56: Apply colour-coded badge CSS classes in Angular list and detail templates
│       ├── US-21: As a teacher, I want to add a named assessment to a student, so that I can record specific achievements or tests.
│       │   ├── TASK-57: Create StudentAssessmentCreateDto with validation rules
│       │   ├── TASK-58: Build inline add-assessment form on detail page with validation
│       │   └── TASK-59: Refresh assessment table and performance summary on success
│       └── US-22: As a teacher, I want to edit and delete an individual assessment, so that I can correct mistakes or remove outdated records.
│           ├── TASK-60: Implement PUT /api/students/{id}/assessments/{assessmentId} endpoint
│           ├── TASK-61: Implement DELETE /api/students/{id}/assessments/{assessmentId} endpoint
│           ├── TASK-62: Build inline edit row mode with save/cancel in Angular detail component
│           └── TASK-63: Update performance summary immediately on edit or delete success
│
├── EPIC-04: Data Display & Interaction
│   └── FEAT-10: DataTables Integration
│       ├── US-14: As a teacher, I want to sort students by column, so that I can organize the list by performance or other criteria.
│       │   ├── TASK-64: Configure DataTables columnDefs to sort Performance by hidden % value
│       │   └── TASK-65: Verify all columns sort correctly ascending and descending
│       └── US-15: As a teacher, I want to search and filter students, so that I can quickly find specific students or groups.
│           ├── TASK-66: Enable DataTables global search input
│           └── TASK-67: Verify search filters all visible columns in real-time
│
├── EPIC-05: API & Documentation
│   └── FEAT-11: API Documentation
│       └── US-19: As a developer, I want to explore the API via Swagger UI, so that I can understand and test available endpoints.
│           ├── TASK-68: Register Swagger/OpenAPI in Program.cs
│           ├── TASK-69: Add XML doc comments to all controller actions
│           └── TASK-70: Verify Swagger UI lists all endpoints with request/response models
│
└── EPIC-06: Student Portal
    ├── FEAT-12: Student Account Activation
    │   ├── US-23: As a student, I want to activate my account using my Student ID and email, so that I can set a password and access my dashboard.
    │   │   ├── TASK-71: Create StudentActivateDto with StudentUniqueId, Email, Password fields
    │   │   ├── TASK-72: Implement POST /api/students/activate controller action
    │   │   ├── TASK-73: Build Angular student activate component (student-activate.component.ts)
    │   │   └── TASK-74: Redirect to /student/dashboard on successful account activation
    │   └── US-24: As a student, I want activation form fields to be validated, so that I cannot submit incomplete or invalid information.
    │       ├── TASK-75: Add Angular validators (required, STU-format pattern, email, minlength 6)
    │       ├── TASK-76: Implement StudentActivateValidator using FluentValidation rules
    │       └── TASK-77: Show confirmPassword mismatch error on frontend before submission
    ├── FEAT-13: Student Login
    │   ├── US-25: As a student, I want to log in with my Student ID and password, so that I can access my personal performance dashboard.
    │   │   ├── TASK-78: Create StudentLoginDto and StudentLoginResponseDto
    │   │   ├── TASK-79: Implement POST /api/students/login controller action
    │   │   ├── TASK-80: Build Angular student login component (student-login.component.ts)
    │   │   └── TASK-81: Store student session in StudentAuthStateService on successful response
    │   └── US-26: As a student, I want to see clear error messages when my login credentials are invalid, so that I understand why access is denied.
    │       ├── TASK-82: Return 401 Unauthorized for wrong credentials / 400 for unactivated account
    │       └── TASK-83: Display friendly error message in Angular on failed student login
    ├── FEAT-14: Student Dashboard
    │   ├── US-27: As a student, I want to view my personal performance summary, so that I can track my academic progress.
    │   │   ├── TASK-84: Create StudentProfileDto with calculated score fields
    │   │   ├── TASK-85: Return StudentProfileDto with computed scores on login/activation response
    │   │   ├── TASK-86: Build Angular student dashboard with performance summary cards
    │   │   └── TASK-87: Display progress bar with colour-coded performance band legend
    │   ├── US-28: As a student, I want to view my personal assessment list, so that I can see all my results in one place.
    │   │   ├── TASK-88: Include assessments array in StudentProfileDto from API response
    │   │   ├── TASK-89: Render assessments table in Angular student dashboard
    │   │   └── TASK-90: Apply Overdue/Submitted status badge based on due date
    │   └── US-29: As a student, I want to view my personal profile information, so that I can verify my details are correct.
    │       ├── TASK-91: Map all student personal fields in StudentProfileDto
    │       └── TASK-92: Render My Profile section in Angular student dashboard component
    └── FEAT-15: Assessment File Submissions
        ├── US-30: As a student, I want to upload a file submission for an assessment, so that I can submit my completed work digitally.
        │   ├── TASK-93: Create AssessmentSubmission entity and EF Core migration
        │   ├── TASK-94: Implement POST /api/students/{id}/assessments/{id}/submissions endpoint (Student JWT only)
        │   ├── TASK-95: Build file upload modal in Angular student dashboard
        │   └── TASK-96: Display filename, upload date, and download link in submissions panel
        └── US-31: As a teacher, I want to download and delete student file submissions, so that I can review and manage submitted work.
            ├── TASK-97: Implement GET /api/students/{id}/assessments/{id}/submissions (Teacher JWT)
            ├── TASK-98: Implement GET .../submissions/{id}/download endpoint (Teacher or owning Student JWT)
            └── TASK-99: Implement DELETE /api/students/{id}/assessments/{id}/submissions/{id} endpoint
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
| **Development Team** | Atreus Tefo Ramokate | Self-organizing, cross-functional team responsible for designing, building, and testing each Sprint increment — covering backend (ASP.NET Core), frontend (Angular), database (EF Core + SQL Server), and API testing (Postman). |

> **Note**: In this solo student project, Atreus Tefo Ramokate fulfills both the Scrum Master and Development Team roles, while the Product Owner perspective represents the teacher end-user's needs.

---

### Scrum Artifacts

#### 1. Product Backlog
A living, prioritized list of all work needed for the product. All 31 User Stories reside here, estimated in story points and ordered by business priority. The Product Owner is responsible for its content and ordering.

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
- [ ] FluentValidation rules are enforced — invalid input returns HTTP 400 with structured error messages.
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

All 31 User Stories, prioritized by business value, with story point estimates and Sprint assignments. **Total estimated effort: 102 story points**.

| ID | User Story | Priority | Points | Sprint |
|----|-----------|----------|:------:|:------:|
| US-18 | Reject invalid data server-side | High | 3 | Sprint 1 |
| US-19 | Explore API via Swagger UI | High | 2 | Sprint 1 |
| US-01 | Register with full profile | High | 5 | Sprint 1 |
| US-02 | Validate registration fields | High | 3 | Sprint 1 |
| US-03 | Log in with email and password | High | 5 | Sprint 2 |
| US-04 | Handle invalid login credentials | High | 2 | Sprint 2 |
| US-05 | Add a new student record | High | 5 | Sprint 2 |
| US-06 | Validate student input fields | High | 3 | Sprint 2 |
| US-16 | Consume student API from frontend | High | 5 | Sprint 2 |
| US-07 | View all students in a table | High | 5 | Sprint 3 |
| US-08 | View detailed student profile | Medium | 3 | Sprint 3 |
| US-09 | Update student information | Medium | 5 | Sprint 3 |
| US-10 | Delete a student record | Medium | 3 | Sprint 3 |
| US-17 | Consume teacher API from frontend | Medium | 3 | Sprint 3 |
| US-20 | Select grade from controlled dropdown | Medium | 2 | Sprint 3 |
| US-23 | Activate student account | High | 3 | Sprint 3 |
| US-24 | Validate activation form fields | High | 2 | Sprint 3 |
| US-25 | Log in with Student ID and password | High | 3 | Sprint 3 |
| US-26 | Handle invalid student credentials | High | 2 | Sprint 3 |
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
| **Total** | | | **102** | |

---

### Sprint Plan

#### Sprint 1 — Foundation & Authentication
**Dates**: March 2–8, 2026
**Sprint Goal**: *Establish project infrastructure, configure server-side validation and API documentation, and deliver a fully working teacher registration system.*
**Velocity**: 13 story points

| Story | Title | Points | Status |
|-------|-------|:------:|:------:|
| US-18 | Reject invalid data server-side | 3 | Done |
| US-19 | Explore API via Swagger UI | 2 | Done |
| US-01 | Register with full profile | 5 | Done |
| US-02 | Validate registration fields | 3 | Done |
| **Total** | | **13** | |

**Sprint Review**: Teacher registration form is functional end-to-end. FluentValidation rejects invalid data with HTTP 400. Swagger UI documents all available endpoints. Backend infrastructure (layered architecture, EF Core, SQL Server LocalDB) is fully configured and running.

**Sprint Retrospective**:

| | Notes |
|-|-------|
| Start | Writing validation tests alongside each new endpoint |
| Stop | Skipping inline code comments — makes debugging harder later |
| Continue | Daily commits to GitHub to maintain a clear, traceable history |

---

#### Sprint 2 — Authentication & Student CRUD
**Dates**: March 9–15, 2026
**Sprint Goal**: *Complete teacher login and enable teachers to create and manage students through the Angular frontend, fully connected to the RESTful API.*
**Velocity**: 20 story points

| Story | Title | Points | Status |
|-------|-------|:------:|:------:|
| US-03 | Log in with email and password | 5 | Done |
| US-04 | Handle invalid login credentials | 2 | Done |
| US-05 | Add a new student record | 5 | Done |
| US-06 | Validate student input fields | 3 | Done |
| US-16 | Consume student API from frontend | 5 | Done |
| **Total** | | **20** | |

**Sprint Review**: Teachers can register, log in, and add students via the Angular frontend. The Angular `StudentService` calls real API endpoints. Student creation triggers live feedback and redirects to the list. Node.js PATH fix applied and npm dependencies resolved; Angular dev server stable at `localhost:4200`.

**Sprint Retrospective**:

| | Notes |
|-|-------|
| Start | Using `proxy.conf.json` to avoid CORS issues during local development |
| Stop | Hard-coding API base URLs in components; move to Angular environment files |
| Continue | Committing working increments daily to maintain sprint momentum |

---

#### Sprint 3 — Student Lifecycle, Views, Data Model Refactoring & Student Auth
**Dates**: March 16–22, 2026
**Sprint Goal**: *Complete the full student management lifecycle — list view, detail view, edit, and delete — finalise teacher API integration in Angular, refactor the data model to support grade lookup and flexible per-student assessments, and deliver the student authentication portal (account activation and login).*
**Velocity**: 31 story points

| Story | Title | Points | Status |
|-------|-------|:------:|:------:|
| US-07 | View all students in a table | 5 | Done |
| US-08 | View detailed student profile | 3 | Done |
| US-09 | Update student information | 5 | Done |
| US-10 | Delete a student record | 3 | Done |
| US-17 | Consume teacher API from frontend | 3 | Done |
| US-20 | Select grade from controlled dropdown | 2 | Done |
| US-23 | Activate student account | 3 | Done |
| US-24 | Validate activation form fields | 2 | Done |
| US-25 | Log in with Student ID and password | 3 | Done |
| US-26 | Handle invalid student credentials | 2 | Done |
| **Total** | | **31** | |

**Sprint Review**: Full CRUD lifecycle for students is complete. Teachers can view all students, open individual detail pages, edit records, and delete with a confirmation step. All Angular routes (`/`, `/detail/:id`, `/edit/:id`) are functional. Teacher API integration is wired to the Angular frontend. Data model refactored: Grade is now a seeded lookup table (Grade 7–12), students reference it via `GradeId` FK; `IdPassportNo` and `StudentUniqueId` fields added; assessment scores extracted into the separate `StudentAssessments` table (EF Core migration `AddGradesAndAssessmentsRefactoring` applied March 18). Student list table expanded to show: Student ID (`StudentUniqueId`), Full Name, Email, Grade, Score (`totalScore/maxPossible`), and a colour-coded Performance Level badge; `StudentListDto` updated to carry all these fields from the API; DataTables `columnDefs` configured so the Performance column sorts by hidden numeric percentage. Student authentication portal completed: students activate their accounts at `/student/activate` using their teacher-assigned `StudentUniqueId` and registered email to set a password; students log in at `/student/login` using their `StudentUniqueId` and password. `StudentAuthStateService`, `studentAuthGuard`, and `studentGuestGuard` implemented for student session management.

**Sprint Retrospective**:

| | Notes |
|-|-------|
| Start | Breaking large components into smaller, focused Angular services |
| Stop | Mixing business logic directly into Angular components |
| Continue | Validating API responses in the Angular service layer before rendering |

---

#### Sprint 4 — Assessment CRUD, Scoring, DataTables & Student Portal
**Dates**: March 23–29, 2026
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

**Sprint Review**: All ten Sprint 4 User Stories delivered. Teachers can now add named assessments to any student with a custom `MaxScore` and optional `DueDate`; existing assessments are editable and deletable inline on the student detail page. Score calculations (total, average, percentage) are computed server-side in `StudentAssessmentService` and surfaced through `StudentDetailDto` and `StudentListDto`. Performance level badges (Needs Support / Satisfactory / Good / Excellent) are colour-coded and visible on both the student list and detail views. DataTables pagination, column sorting, and global search are fully configured; the Performance column sorts by a hidden numeric percentage value. The student self-service portal is complete: the dashboard displays summary cards (Total Score, Average, Percentage, Performance Level), a colour-coded progress bar, a personal assessments table with Overdue/Submitted badges, and a My Profile section — all populated from `StudentProfileDto` returned on login.

**Sprint Retrospective**:

| | Notes |
|-|-------|
| Start | Running the full Postman collection as a regression suite before each release |
| Stop | Manual testing only; introduce smoke tests for critical API paths |
| Continue | Keeping Swagger UI and Postman collection synchronized with the latest API |

---

#### Sprint 5 — Assessment File Submissions
**Dates**: March 30 – April 5, 2026
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

## EPIC-01: Security

> **Goal**: Protect the application through validated registration and login, and enforce data integrity rules on both the frontend and backend so bad data never reaches the database.

---

### FEAT-01: Teacher Registration

> **Description**: Allow a new teacher to create an account by filling in their personal and professional details.

---

#### US-01: Register with Full Profile

> **Story Points**: 5 &nbsp;|&nbsp; **Sprint**: Sprint 1

**As a** teacher,
**I want** to register an account with my ID/Passport No., name, email, phone, subject, and password,
**so that** I can access the Student Assessment Tracker.

**Acceptance Criteria:**
- [ ] Registration form collects: ID/Passport No., first name, last name, email, phone, subject, password.
- [ ] ID/Passport No. is required, exactly 9 alphanumeric characters (letters and digits only — no hyphens or special characters).
- [ ] On successful submission, the user is redirected to the Login page.
- [ ] A success confirmation is shown before redirect.

**Tasks:**
- TASK-01: Create `TeacherRegisterDto` with `IdPassportNo` and all registration fields
- TASK-02: Implement `POST /api/teachers` controller action
- TASK-03: Build Angular registration form component (`signup-form.component.ts`)
- TASK-04: Connect Angular form to API via `TeacherService` and handle redirect

**App Example:**
> A teacher named "Mrs. Smith" opens the app for the first time. She navigates to `/register`, fills in her details (e.g., ID: `AB1234567`, email: `smith@school.com`, subject: `Mathematics`, phone: `12345678`), clicks Register, and is redirected to the `/login` page.

---

#### US-02: Validate Registration Fields

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 1

**As a** teacher,
**I want** the registration form to show inline errors for invalid inputs,
**so that** I know exactly what needs to be corrected before submitting.

**Acceptance Criteria:**
- [ ] ID/Passport No.: required, exactly 9 alphanumeric characters (letters and digits only).
- [ ] First/Last name: required, 2–50 characters.
- [ ] Email: required, must be a valid email format.
- [ ] Phone: required, exactly 8 digits.
- [ ] Subject: required, max 100 characters.
- [ ] Password: required, 6–20 characters.
- [ ] Error messages appear inline next to the invalid field.
- [ ] The form cannot be submitted while validation errors exist.

**Tasks:**
- TASK-05: Add Angular template-driven form validators (required, minlength, pattern — including `^[a-zA-Z0-9]+$` for IdPassportNo)
- TASK-06: Implement `TeacherRegisterValidator` using FluentValidation rules (NotEmpty, Length(9), Matches alphanumeric)
- TASK-07: Display inline error messages next to each invalid field

**App Example:**
> Mrs. Smith types only `"A"` for her first name and submits. The form does not post; instead, an inline error appears: *"First name must be 2–50 characters"*. If she enters `"AB-12345"` as her ID/Passport No. it is rejected as hyphens are not allowed — only letters and digits. The backend (FluentValidation) also validates and returns a 400 error if incorrect data somehow reaches the API.

---

### FEAT-02: Teacher Login

> **Description**: Allow a registered teacher to authenticate with their email and password.

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
> Mrs. Smith accidentally types the wrong password. The page displays: *"Invalid email or password. Please try again."* — she is not logged in and remains on the `/login` page.

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
- [ ] First/Last name: required, 2–50 characters, letters/spaces/hyphens only.
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
- [ ] Student validation rules: IdPassportNo (9 chars), names (2–50 chars), email, phone (8 digits), GradeId (> 0).
- [ ] Assessment validation rules: Name required (max 100 chars), MaxScore > 0, Score ≥ 0 and ≤ MaxScore.

**Tasks:**
- TASK-17: Register FluentValidation in `Program.cs` with `AddFluentValidationAutoValidation`
- TASK-18: Create validators for all DTOs (Teacher, Student, Assessment)
- TASK-19: Verify `400 Bad Request` with structured error body via Postman

**App Example:**
> A direct API call via Postman adds an assessment with `Score: 25` and `MaxScore: 20`. The API responds: `400 Bad Request` with body: `{ "errors": { "Score": ["Score cannot exceed the max score for this assessment"] } }`.

---

## EPIC-02: Student Management

> **Goal**: Enable teachers to fully manage student records — creating, viewing, editing, and deleting — through an intuitive Angular interface backed by a RESTful API.

---

### FEAT-04: Create Student

> **Description**: Allow a teacher to add a new student with personal details and grade assignment.

---

#### US-05: Add a New Student Record

> **Story Points**: 5 &nbsp;|&nbsp; **Sprint**: Sprint 2

**As a** teacher,
**I want** to add a new student by filling in their details,
**so that** their performance can be tracked in the system.

**Acceptance Criteria:**
- [ ] Create form collects: ID/Passport No., first name, last name, email, phone, grade (selected from a controlled dropdown).
- [ ] A system-generated StudentUniqueId (e.g., `STU-A1B2C3D4`) is assigned automatically on creation.
- [ ] On successful submission, the student appears in the list.
- [ ] The teacher is redirected to the students list after creation.

**Tasks:**
- TASK-20: Create `StudentCreateDto` with required fields
- TASK-21: Implement `POST /api/students` controller action
- TASK-22: Generate `StudentUniqueId` (`STU-XXXXXXXX`) in the service layer
- TASK-23: Build Angular student creation form component (`student-form.component.ts`)
- TASK-24: Redirect to student list on successful creation

**App Example:**
> Mrs. Smith clicks "Add Student", fills in: `ID/Passport No.: 123456789, John Doe, john@school.com, 12345678, Grade 10`, and clicks Create. John Doe appears in the students table.

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
> Mrs. Smith opens the app and sees a table listing all 25 students — columns: Student ID (`STU-A1B2C3D4`), Full Name, Email, Grade, Score (`144/170`), and a colour-coded Performance badge (**Excellent** in green).

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
**I want** to select a student's grade level from a predefined dropdown (Grade 7–12),
**so that** grade data is consistent across all student records.

**Acceptance Criteria:**
- [ ] Student create and edit forms load available grades from `GET /api/grades`.
- [ ] The dropdown shows grade labels (e.g., "Grade 7", "Grade 8") ordered by level.
- [ ] A GradeId FK is stored on the student record instead of a free-text string.
- [ ] Selecting "-- Select Grade --" (GradeId = 0) is blocked by frontend and backend validation.

**Tasks:**
- TASK-33: Create `Grade` entity and seed Grade 7–12 via EF Core migration
- TASK-34: Implement `GET /api/grades` read-only endpoint
- TASK-35: Populate grade dropdown in Angular forms from the API response

**App Example:**
> Mrs. Smith opens the Create Student form. The Grade dropdown shows Grade 7 through Grade 12. She selects "Grade 10". The student record links to the `Grades` table row for Grade 10, not a string.

---

### FEAT-06: Edit Student

> **Description**: Allow teachers to update existing student personal information.

---

#### US-09: Update Student Information

> **Story Points**: 5 &nbsp;|&nbsp; **Sprint**: Sprint 3

**As a** teacher,
**I want** to edit a student's record,
**so that** I can correct mistakes or update their personal details.

**Acceptance Criteria:**
- [ ] Edit form at `/edit/:id` pre-populates personal details: ID/Passport No., first name, last name, email, phone, grade.
- [ ] Teacher can change any personal detail field.
- [ ] On save, the record is updated and the teacher is redirected to the detail view.
- [ ] Assessment scores are managed separately via the inline assessment controls on the detail page.

**Tasks:**
- TASK-36: Create `StudentUpdateDto` for personal detail fields
- TASK-37: Implement `PUT /api/students/{id}` controller action
- TASK-38: Build Angular edit form pre-populated with current student data
- TASK-39: Redirect to detail view on successful update

**App Example:**
> Mrs. Smith realizes she mis-entered John Doe's email. She navigates to `/edit/1`, corrects the email, and clicks Update. She is redirected to `/detail/1` where all personal info is updated.

---

### FEAT-07: Delete Student

> **Description**: Allow teachers to permanently remove a student from the system with a confirmation step.

---

#### US-10: Delete a Student Record

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 3

**As a** teacher,
**I want** to delete a student record with a confirmation step,
**so that** I don't accidentally remove students from the system.

**Acceptance Criteria:**
- [ ] A "Delete" button is available on the student list.
- [ ] Clicking Delete requests confirmation before proceeding.
- [ ] After deletion, the student is removed from the list immediately.
- [ ] The API returns a 204 No Content on successful deletion.

**Tasks:**
- TASK-40: Implement `DELETE /api/students/{id}` with cascade delete for assessments
- TASK-41: Add confirmation modal to Angular student list component
- TASK-42: Remove the deleted row from the DataTable on confirmed delete

**App Example:**
> Mrs. Smith clicks Delete on a student who has left the school. A confirmation dialog appears: *"Are you sure you want to delete this student?"*. She confirms, and the student is removed from the table.

---

### FEAT-08: Communication

> **Description**: Wire the Angular frontend to all RESTful API endpoints so every data operation is persisted through the backend.

---

#### US-16: Consume Student API from Frontend

> **Story Points**: 5 &nbsp;|&nbsp; **Sprint**: Sprint 2

**As a** developer,
**I want** the Angular frontend to communicate with the student API endpoints,
**so that** all student data operations are persisted via the backend.

**Acceptance Criteria:**
- [ ] `GET /api/students` — retrieves all students.
- [ ] `POST /api/students` — creates a new student.
- [ ] `GET /api/students/{id}` — retrieves a single student with their assessments.
- [ ] `PUT /api/students/{id}` — updates a student's personal details.
- [ ] `DELETE /api/students/{id}` — deletes a student and all their assessments (cascade).
- [ ] `GET /api/grades` — retrieves all grade levels for dropdown population.
- [ ] `GET /api/students/{id}/assessments` — retrieves all assessments for a student.
- [ ] `POST /api/students/{id}/assessments` — adds a new assessment to a student.
- [ ] `PUT /api/students/{id}/assessments/{assessmentId}` — updates a single assessment.
- [ ] `DELETE /api/students/{id}/assessments/{assessmentId}` — deletes a single assessment.
- [ ] All responses use consistent JSON shapes (StudentDto, GradeDto, StudentAssessmentDto).

**Tasks:**
- TASK-43: Implement `StudentApiService` with all CRUD and assessment HTTP methods
- TASK-44: Wire `StudentBusinessService` to call `StudentApiService` methods
- TASK-45: Handle API errors gracefully in Angular components

**App Example:**
> When Mrs. Smith loads the homepage, Angular fires `GET /api/students` → the API returns `StudentDto[]` → DataTables renders them. Adding an assessment calls `POST /api/students/{id}/assessments`.

---

#### US-17: Consume Teacher API from Frontend

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 3

**As a** developer,
**I want** the Angular frontend to communicate with the teacher API endpoints,
**so that** registration and login functionality works end-to-end.

**Acceptance Criteria:**
- [ ] `POST /api/teachers` — registers a new teacher.
- [ ] `POST /api/teachers/login` — authenticates a teacher.
- [ ] Login response includes teacher profile data.

**Tasks:**
- TASK-46: Implement `TeacherApiService` with register and login HTTP methods
- TASK-47: Wire `TeacherBusinessService` to call `TeacherApiService` methods

**App Example:**
> When Mrs. Smith submits the registration form, Angular sends `POST /api/teachers` with her details. The API returns 201 Created, and she is redirected to `/login`.

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
- [ ] Score must be ≥ 0 and ≤ MaxScore for that assessment.
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
> John Doe has three assessments: Test 1 (18/20), Assignment 2 (44/50), Final Exam (82/100). The system displays: *Total: 144/170, Percentage: 84.7%, Average: 85.2%, Level: Excellent* — updated automatically each time an assessment is added, edited, or deleted.

---

#### US-13: View Performance Level Badge

> **Story Points**: 2 &nbsp;|&nbsp; **Sprint**: Sprint 4

**As a** teacher,
**I want** each student to have a performance level label,
**so that** I can quickly identify students who need support.

**Acceptance Criteria:**
- [ ] Performance levels are calculated as follows:
  - **Needs Support**: Percentage < 50%
  - **Satisfactory**: Percentage 50–55%
  - **Good**: Percentage 56–75%
  - **Excellent**: Percentage > 75%
- [ ] The label is visible in both the list table and the detail view.

**Tasks:**
- TASK-55: Implement `GetPerformanceLevel(percentage)` helper method in the service layer
- TASK-56: Apply colour-coded badge CSS classes in Angular list and detail templates

**App Example:**
> John Doe's Percentage is 90% → label shows **"Excellent"**.
> A struggling student with Total = 25 → Percentage = 41.7% → label shows **"Needs Support"**.

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
- [ ] FluentValidation rejects: empty name, MaxScore ≤ 0, Score < 0, Score > MaxScore.

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
> A new team member opens `http://localhost:5000/swagger/ui`, finds `POST /api/students`, clicks "Try it out", enters a student JSON payload, clicks Execute, and sees the 201 Created response — all without writing a single line of code.

---

## EPIC-06: Student Portal

> **Goal**: Allow students to activate their own accounts, securely log in, and access a self-service dashboard showing their personal performance summary, individual assessment results, and profile information — without any teacher involvement after the initial account creation.

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
- [ ] Account can only be activated once — attempting to re-activate a previously activated account shows a clear error.
- [ ] On successful activation, the student is automatically logged in and redirected to `/student/dashboard`.
- [ ] The student’s session is persisted in `localStorage` so page refresh maintains the login state.

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
- [ ] The form remains on the login page — no redirect on failure.

**Tasks:**
- TASK-82: Return `401 Unauthorized` for wrong credentials and `400 Bad Request` for unactivated account in API
- TASK-83: Display context-appropriate friendly error message in Angular on failed student login

**App Example:**
> A student types the wrong password. The form shows: *"Invalid Student ID or password. Please try again."* — they remain on `/student/login`. If the account was never activated, a different message appears: *"Account not activated. Please sign up here."*

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
- [ ] Progress bar includes a legend: *Needs Support (<50%)*, *Satisfactory (50–55%)*, *Good (56–75%)*, *Excellent (>75%)*.
- [ ] Performance level and progress bar change colour based on the percentage band.

**Tasks:**
- TASK-84: Create `StudentProfileDto` with calculated fields (`totalScore`, `maxPossible`, `averageScore`, `percentage`, `performanceLevel`)
- TASK-85: Return `StudentProfileDto` with computed scores in `StudentLoginResponseDto` from both login and activation endpoints
- TASK-86: Build Angular student dashboard component with four performance summary cards
- TASK-87: Display progress bar with colour-coded performance band legend

**App Example:**
> A student logs in and sees four cards: *"42.5 / 60 — Total Score"*, *"70.8% — Average Score"*, *"70.8% — Overall Percentage"*, and *"Good — Performance Level"*, along with a green progress bar at 70.8%.

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
- TASK-90: Apply Overdue/Submitted status badge logic based on `dueDate` compared to today’s date

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
- [ ] Profile is read-only — students cannot edit their own details (only teachers can).
- [ ] An *Active* status indicator is shown alongside the profile header.

**Tasks:**
- TASK-91: Map all student personal fields in `StudentProfileDto` (`firstName`, `lastName`, `studentUniqueId`, `gradeName`, `email`, `phone`, `idPassportNo`, `createdAt`)
- TASK-92: Render *My Profile* section in Angular student dashboard component with all personal fields displayed

**App Example:**
> A student scrolls down on their dashboard and sees a *My Profile* card showing their name, `STU-AB12CD34` chip, Grade 10 badge, email, phone, ID/Passport number, and *"Member since March 2026"* — all read-only with no edit buttons.

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

## Summary Table

| ID | Level | Title | Parent | Points | Sprint |
|----|-------|-------|--------|:------:|:------:|
| EPIC-01 | Epic | Security | Application | — | — |
| FEAT-01 | Feature | Teacher Registration | EPIC-01 | — | — |
| US-01 | User Story | Register with Full Profile | FEAT-01 | 5 | Sprint 1 |
| US-02 | User Story | Validate Registration Fields | FEAT-01 | 3 | Sprint 1 |
| FEAT-02 | Feature | Teacher Login | EPIC-01 | — | — |
| US-03 | User Story | Log In with Email and Password | FEAT-02 | 5 | Sprint 2 |
| US-04 | User Story | Handle Invalid Login Credentials | FEAT-02 | 2 | Sprint 2 |
| FEAT-03 | Feature | Input Validation | EPIC-01 | — | — |
| US-06 | User Story | Validate Student Input Fields | FEAT-03 | 3 | Sprint 2 |
| US-18 | User Story | Reject Invalid Data Server-Side | FEAT-03 | 3 | Sprint 1 |
| EPIC-02 | Epic | Student Management | Application | — | — |
| FEAT-04 | Feature | Create Student | EPIC-02 | — | — |
| US-05 | User Story | Add a New Student Record | FEAT-04 | 5 | Sprint 2 |
| FEAT-05 | Feature | View Students | EPIC-02 | — | — |
| US-07 | User Story | View All Students in a Table | FEAT-05 | 5 | Sprint 3 |
| US-08 | User Story | View Detailed Student Profile | FEAT-05 | 3 | Sprint 3 |
| US-20 | User Story | Select Grade from Controlled Dropdown | FEAT-05 | 2 | Sprint 3 |
| FEAT-06 | Feature | Edit Student | EPIC-02 | — | — |
| US-09 | User Story | Update Student Information | FEAT-06 | 5 | Sprint 3 |
| FEAT-07 | Feature | Delete Student | EPIC-02 | — | — |
| US-10 | User Story | Delete a Student Record | FEAT-07 | 3 | Sprint 3 |
| FEAT-08 | Feature | Communication | EPIC-02 | — | — |
| US-16 | User Story | Consume Student API from Frontend | FEAT-08 | 5 | Sprint 2 |
| US-17 | User Story | Consume Teacher API from Frontend | FEAT-08 | 3 | Sprint 3 |
| EPIC-03 | Epic | Assessment | Application | — | — |
| FEAT-09 | Feature | Scoring | EPIC-03 | — | — |
| US-11 | User Story | Add Named Assessments with Flexible Scoring | FEAT-09 | 5 | Sprint 4 |
| US-12 | User Story | View Total, Average, and Percentage | FEAT-09 | 3 | Sprint 4 |
| US-13 | User Story | View Performance Level Badge | FEAT-09 | 2 | Sprint 4 |
| US-21 | User Story | Add a Named Assessment to a Student | FEAT-09 | 3 | Sprint 4 |
| US-22 | User Story | Edit and Delete an Individual Assessment | FEAT-09 | 3 | Sprint 4 |
| EPIC-04 | Epic | Data Display & Interaction | Application | — | — |
| FEAT-10 | Feature | DataTables Integration | EPIC-04 | — | — |
| US-14 | User Story | Sort Students by Column | FEAT-10 | 2 | Sprint 4 |
| US-15 | User Story | Search and Filter Students | FEAT-10 | 2 | Sprint 4 |
| EPIC-05 | Epic | API & Documentation | Application | — | — |
| FEAT-11 | Feature | API Documentation | EPIC-05 | — | — |
| US-19 | User Story | Explore API via Swagger UI | FEAT-11 | 2 | Sprint 1 |
| EPIC-06 | Epic | Student Portal | Application | — | — |
| FEAT-12 | Feature | Student Account Activation | EPIC-06 | — | — |
| US-23 | User Story | Activate Student Account | FEAT-12 | 3 | Sprint 3 |
| US-24 | User Story | Validate Activation Form Fields | FEAT-12 | 2 | Sprint 3 |
| FEAT-13 | Feature | Student Login | EPIC-06 | — | — |
| US-25 | User Story | Log In with Student ID and Password | FEAT-13 | 3 | Sprint 3 |
| US-26 | User Story | Handle Invalid Student Credentials | FEAT-13 | 2 | Sprint 3 |
| FEAT-14 | Feature | Student Dashboard | EPIC-06 | — | — |
| US-27 | User Story | View Personal Performance Summary | FEAT-14 | 3 | Sprint 4 |
| US-28 | User Story | View Personal Assessment List | FEAT-14 | 3 | Sprint 4 |
| US-29 | User Story | View Personal Profile Information | FEAT-14 | 2 | Sprint 4 |
| FEAT-15 | Feature | Assessment File Submissions | EPIC-06 | — | — |
| US-30 | User Story | Upload File Submission for an Assessment | FEAT-15 | 5 | Sprint 5 |
| US-31 | User Story | Download and Delete File Submissions | FEAT-15 | 3 | Sprint 5 |
| **Totals** | | **31 User Stories · 99 Tasks** | | **102 pts** | **5 Sprints** |
