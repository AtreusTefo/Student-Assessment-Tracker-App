# Agile Methodology — Scrum Framework with Epic > Feature > User Story Hierarchy

## Overview

This document applies the **Scrum framework** to the Student Assessment Tracker project. Scrum is an Agile framework that delivers value in short, time-boxed iterations called **Sprints**. It provides structure through defined roles (Product Owner, Scrum Master, Development Team), artifacts (Product Backlog, Sprint Backlog, Increment), and events (Sprint Planning, Daily Scrum, Sprint Review, Sprint Retrospective).

The project work is organized using a three-level Agile hierarchy — **Epics → Features → User Stories** — which populates the Scrum **Product Backlog**. User Stories are estimated with **story points** (Fibonacci scale), prioritized by business value, and allocated to four 1-week Sprints.

---

## Agile Hierarchy: Definitions

### Epic
An **Epic** is a large body of work that represents a high-level business goal or major capability of the system. Epics are too broad to be completed in a single sprint and are broken down into Features.

> **Format**: `EPIC-XX: <Title>` — A short name describing the business domain area.

---

### Feature
A **Feature** is a service or function that delivers business value to a user. It is a chunk of work that can be completed within a few sprints. Features belong to an Epic and are broken down into User Stories.

> **Format**: `FEAT-XX: <Title>` — Describes a specific capability within the Epic.

---

### User Story
A **User Story** is the smallest unit of work, written from the end-user's perspective. It describes a single piece of functionality the user wants to achieve.

> **Format**: `US-XX: As a [role], I want [goal], so that [benefit].`
>
> Each story includes:
> - **Description**: What is being built.
> - **Acceptance Criteria**: The conditions that must be met for the story to be considered done.
> - **App Example**: A concrete example from the Student Assessment Tracker.

---

## Hierarchy Map

```
EPIC-01: Teacher Authentication
    └── FEAT-01: Teacher Registration
            └── US-01: Register with full profile
            └── US-02: Validate registration fields
    └── FEAT-02: Teacher Login
            └── US-03: Log in with email and password
            └── US-04: Handle invalid login credentials

EPIC-02: Student Management
    └── FEAT-03: Create Student
            └── US-05: Add a new student record
            └── US-06: Validate student input fields
    └── FEAT-04: View Students
            └── US-07: View all students in a table
            └── US-08: View detailed student profile
    └── FEAT-05: Edit Student
            └── US-09: Update student information
    └── FEAT-06: Delete Student
            └── US-10: Delete a student record

EPIC-03: Assessment & Scoring
    └── FEAT-07: Score Entry
            └── US-11: Add named assessments with flexible scoring
    └── FEAT-08: Automatic Score Calculations
            └── US-12: View total, average, and percentage
            └── US-13: View performance level badge

EPIC-04: Data Display & Interaction
    └── FEAT-09: DataTables Integration
            └── US-14: Sort students by column
            └── US-15: Search and filter students

EPIC-05: API & Backend
    └── FEAT-10: RESTful API Endpoints
            └── US-16: Consume student API from frontend
            └── US-17: Consume teacher API from frontend
    └── FEAT-11: Input Validation (Backend)
            └── US-18: Reject invalid data server-side
    └── FEAT-12: API Documentation
            └── US-19: Explore API via Swagger UI

EPIC-06: Grade & Assessment Management
    └── FEAT-13: Grade Level Lookup
            └── US-20: Select grade from controlled dropdown
    └── FEAT-14: Individual Assessment CRUD
            └── US-21: Add a named assessment to a student
            └── US-22: Edit and delete an individual assessment
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
| **Scrum Master** | Developer.03 | Facilitates all Scrum events; removes impediments; ensures adherence to Scrum practices; coaches the team on Agile principles. |
| **Development Team** | Developer.03 | Self-organizing, cross-functional team responsible for designing, building, and testing each Sprint increment — covering backend (ASP.NET Core), frontend (Angular), database (EF Core + SQL Server), and API testing (Postman). |

> **Note**: In this solo student project, Developer.03 fulfills both the Scrum Master and Development Team roles, while the Product Owner perspective represents the teacher end-user's needs.

---

### Scrum Artifacts

#### 1. Product Backlog
A living, prioritized list of all work needed for the product. All 22 User Stories reside here, estimated in story points and ordered by business priority. The Product Owner is responsible for its content and ordering.

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

All 22 User Stories, prioritized by business value, with story point estimates and Sprint assignments. **Total estimated effort: 76 story points**.

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
| US-11 | Add named assessments with flexible scoring | Medium | 5 | Sprint 4 |
| US-21 | Add a named assessment to a student | Medium | 3 | Sprint 4 |
| US-22 | Edit and delete an individual assessment | Medium | 3 | Sprint 4 |
| US-12 | View total, average, and percentage | Medium | 3 | Sprint 4 |
| US-13 | View performance level badge | Medium | 2 | Sprint 4 |
| US-14 | Sort students by column | Low | 2 | Sprint 4 |
| US-15 | Search and filter students | Low | 2 | Sprint 4 |
| **Total** | | | **76** | |

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

#### Sprint 3 — Student Lifecycle, Views & Data Model Refactoring
**Dates**: March 16–22, 2026
**Sprint Goal**: *Complete the full student management lifecycle — list view, detail view, edit, and delete — finalise teacher API integration in Angular, and refactor the data model to support grade lookup and flexible per-student assessments.*
**Velocity**: 21 story points

| Story | Title | Points | Status |
|-------|-------|:------:|:------:|
| US-07 | View all students in a table | 5 | Done |
| US-08 | View detailed student profile | 3 | Done |
| US-09 | Update student information | 5 | Done |
| US-10 | Delete a student record | 3 | Done |
| US-17 | Consume teacher API from frontend | 3 | Done |
| US-20 | Select grade from controlled dropdown | 2 | Done |
| **Total** | | **21** | |

**Sprint Review**: Full CRUD lifecycle for students is complete. Teachers can view all students, open individual detail pages, edit records, and delete with a confirmation step. All Angular routes (`/`, `/detail/:id`, `/edit/:id`) are functional. Teacher API integration is wired to the Angular frontend. Data model refactored: Grade is now a seeded lookup table (Grade 7–12), students reference it via `GradeId` FK; `IdPassportNo` and `StudentUniqueId` fields added; assessment scores extracted into the separate `StudentAssessments` table (EF Core migration `AddGradesAndAssessmentsRefactoring` applied March 18). Student list table expanded to show: Student ID (`StudentUniqueId`), Full Name, Email, Grade, Score (`totalScore/maxPossible`), and a colour-coded Performance Level badge; `StudentListDto` updated to carry all these fields from the API; DataTables `columnDefs` configured so the Performance column sorts by hidden numeric percentage.

**Sprint Retrospective**:

| | Notes |
|-|-------|
| Start | Breaking large components into smaller, focused Angular services |
| Stop | Mixing business logic directly into Angular components |
| Continue | Validating API responses in the Angular service layer before rendering |

---

#### Sprint 4 — Assessment CRUD, Scoring & DataTables Polish
**Dates**: March 23–29, 2026
**Sprint Goal**: *Implement the full individual-assessment workflow (add, edit, delete), automated score calculations with performance level labels, and enhance the student table with DataTables sorting, searching, and pagination.*
**Velocity**: 20 story points

| Story | Title | Points | Status |
|-------|-------|:------:|:------:|
| US-11 | Add named assessments with flexible scoring | 5 | — |
| US-21 | Add a named assessment to a student | 3 | — |
| US-22 | Edit and delete an individual assessment | 3 | — |
| US-12 | View total, average, and percentage | 3 | — |
| US-13 | View performance level badge | 2 | — |
| US-14 | Sort students by column | 2 | — |
| US-15 | Search and filter students | 2 | — |
| **Total** | | **20** | |

**Sprint Review**: *(Pending Sprint 4 completion — March 23–29, 2026.)*

**Sprint Retrospective**:

| | Notes |
|-|-------|
| Start | Running the full Postman collection as a regression suite before each release |
| Stop | Manual testing only; introduce smoke tests for critical API paths |
| Continue | Keeping Swagger UI and Postman collection synchronized with the latest API |

---

## EPIC-01: Teacher Authentication

> **Goal**: Enable teachers to securely register and log in to the application so only authorized teachers can access student data.

---

### FEAT-01: Teacher Registration

> **Description**: Allow a new teacher to create an account by filling in their personal and professional details.

---

#### US-01: Register with Full Profile

> **Story Points**: 5 &nbsp;|&nbsp; **Sprint**: Sprint 1

**As a** teacher,
**I want** to register an account with my name, email, phone, subject, and password,
**so that** I can access the Student Assessment Tracker.

**Acceptance Criteria:**
- [ ] Registration form collects: first name, last name, email, phone, subject, password.
- [ ] On successful submission, the user is redirected to the Login page.
- [ ] A success confirmation is shown before redirect.

**App Example:**
> A teacher named "Mrs. Smith" opens the app for the first time. She navigates to `/register`, fills in her details (e.g., email: `smith@school.com`, subject: `Mathematics`, phone: `12345678`), clicks Register, and is redirected to the `/login` page.

---

#### US-02: Validate Registration Fields

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 1

**As a** teacher,
**I want** the registration form to show inline errors for invalid inputs,
**so that** I know exactly what needs to be corrected before submitting.

**Acceptance Criteria:**
- [ ] First/Last name: required, 2–50 characters.
- [ ] Email: required, must be a valid email format.
- [ ] Phone: required, exactly 8 digits.
- [ ] Subject: required, max 100 characters.
- [ ] Password: required, 6–20 characters.
- [ ] Error messages appear inline next to the invalid field.
- [ ] The form cannot be submitted while validation errors exist.

**App Example:**
> Mrs. Smith types only `"A"` for her first name and submits. The form does not post; instead, an inline error appears: *"First name must be 2–50 characters"*. The backend (FluentValidation) also validates and returns a 400 error if incorrect data somehow reaches the API.

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

**App Example:**
> Mrs. Smith accidentally types the wrong password. The page displays: *"Invalid email or password. Please try again."* — she is not logged in and remains on the `/login` page.

---

## EPIC-02: Student Management

> **Goal**: Enable teachers to fully manage student records — creating, viewing, editing, and deleting — through an intuitive interface backed by a RESTful API.

---

### FEAT-03: Create Student

> **Description**: Allow a teacher to add a new student with personal details and initial assessment scores.

---

#### US-05: Add a New Student Record

> **Story Points**: 5 &nbsp;|&nbsp; **Sprint**: Sprint 2

**As a** teacher,
**I want** to add a new student by filling in their details and assessment scores,
**so that** their performance can be tracked in the system.

**Acceptance Criteria:**
- [ ] Create form collects: ID/Passport No., first name, last name, email, phone, grade (selected from a controlled dropdown).
- [ ] A system-generated StudentUniqueId (e.g., `STU-A1B2C3D4`) is assigned automatically on creation.
- [ ] On successful submission, the student appears in the list.
- [ ] The teacher is redirected to the students list after creation.

**App Example:**
> Mrs. Smith clicks "Add Student", fills in: `ID/Passport No.: 123456789, John Doe, john@school.com, 12345678, Grade 10`, and clicks Create. John Doe appears in the students table. She can then open his profile and add individual assessments from the detail page.

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

**App Example:**
> Mrs. Smith leaves the Grade field on "-- Select Grade --" and submits. The form shows: *"A valid grade must be selected"* and blocks submission. If the API is called directly with `GradeId: 0`, FluentValidation returns `400 Bad Request` with the same message.

---

### FEAT-04: View Students

> **Description**: Allow teachers to see an overview of all students and drill into individual student details.

---

#### US-07: View All Students in a Table

> **Story Points**: 5 &nbsp;|&nbsp; **Sprint**: Sprint 3

**As a** teacher,
**I want** to see all students listed in a table,
**so that** I can get a quick overview of the class.

**Acceptance Criteria:**
- [ ] The home page (`/`) displays a table of all students.
- [ ] The table shows the following columns: Student ID (`StudentUniqueId`), Full Name, Email, Grade, Score (`totalScore / maxPossible` or "No assessments"), and Performance Level.
- [ ] Performance Level is displayed as a colour-coded badge (green = Excellent, blue = Good, yellow = Satisfactory, red = Needs Support).
- [ ] Students with no assessments show a muted "No assessments" score and no badge.
- [ ] The table is rendered using the DataTables library with sorting and pagination.
- [ ] The Performance column sorts by the underlying percentage value, not alphabetically by label.

**App Example:**
> Mrs. Smith opens the app and sees a table listing all 25 students in her class, with columns: Student ID (e.g., `STU-A1B2C3D4`), Full Name, Email, Grade, Score (e.g., `144/170`), and a colour-coded Performance badge (e.g., **Excellent** in green). Students with no assessments show "No assessments" in the Score column.

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

**App Example:**
> Mrs. Smith clicks "View" on John Doe's row. She sees `/detail/1` with his ID (`STU-A1B2C3D4`), personal info, and a table of his assessments: *Test 1: 18/20 (90%), Assignment 2: 44/50 (88%)*. The Performance Summary shows *Total: 62/70, Percentage: 88.6%, Level: Excellent*. She can add, edit, or delete individual assessments without leaving the page.

---

### FEAT-05: Edit Student

> **Description**: Allow teachers to update existing student information and assessment scores.

---

#### US-09: Update Student Information

> **Story Points**: 5 &nbsp;|&nbsp; **Sprint**: Sprint 3

**As a** teacher,
**I want** to edit a student's record,
**so that** I can correct mistakes or update their assessment scores.

**Acceptance Criteria:**
- [ ] Edit form at `/edit/:id` pre-populates personal details: ID/Passport No., first name, last name, email, phone, grade.
- [ ] Teacher can change any personal detail field.
- [ ] On save, the record is updated and the teacher is redirected to the detail view.
- [ ] Assessment scores are managed separately via the inline assessment controls on the detail page.

**App Example:**
> Mrs. Smith realizes she mis-entered John Doe's email. She navigates to `/edit/1`, corrects the email, and clicks Update. She is redirected to `/detail/1` where all personal info is updated. His assessment history and performance summary are unchanged.

---

### FEAT-06: Delete Student

> **Description**: Allow teachers to permanently remove a student from the system.

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

**App Example:**
> Mrs. Smith clicks Delete on a student who has left the school. A confirmation dialog appears: *"Are you sure you want to delete this student?"*. She confirms, and the student is removed from the table.

---

## EPIC-03: Assessment & Scoring

> **Goal**: Automatically calculate and display meaningful scoring metrics for each student based on their three assessment scores.

---

### FEAT-07: Score Entry

> **Description**: Allow teachers to add any number of named assessment entries to a student, each with its own teacher-defined maximum score and an optional due date.

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

**App Example:**
> After creating John Doe's record, Mrs. Smith opens his detail page and adds: "Test 1" (MaxScore: 20, Score: 18, Due: 05/03/2026), "Assignment 2" (MaxScore: 50, Score: 44), "Final Exam" (MaxScore: 100, Score: 82). Each is saved immediately and reflected in the performance summary.

---

### FEAT-08: Automatic Score Calculations

> **Description**: Automatically compute and display the total, average, percentage, and performance level from the three assessment scores.

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

**App Example:**
> John Doe's Percentage is 90% → label shows **"Excellent"**.
> A struggling student with Total = 25 → Percentage = 41.7% → label shows **"Needs Support"**.

---

## EPIC-04: Data Display & Interaction

> **Goal**: Provide an interactive, user-friendly table experience so teachers can efficiently browse, search, and sort student data.

---

### FEAT-09: DataTables Integration

> **Description**: Enhance the student list table with sorting, searching, and pagination using the DataTables library.

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

**App Example:**
> Mrs. Smith types `"Doe"` in the search box. The table instantly filters to show only "John Doe", hiding all other students.

---

## EPIC-05: API & Backend

> **Goal**: Provide a robust, validated, and well-documented RESTful API that serves as the backbone of the application.

---

### FEAT-10: RESTful API Endpoints

> **Description**: Expose CRUD endpoints for students and teachers that the Angular frontend consumes.

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

**App Example:**
> When Mrs. Smith loads the homepage, Angular fires `GET /api/students` → the API returns `StudentDto[]` → DataTables renders them. When she opens a detail page, `GET /api/students/{id}` returns the student with embedded assessments. Adding an assessment calls `POST /api/students/{id}/assessments`.

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

**App Example:**
> When Mrs. Smith submits the registration form, Angular sends `POST /api/teachers` with her details. The API returns 201 Created, and she is redirected to `/login`.

---

### FEAT-11: Input Validation (Backend)

> **Description**: Use FluentValidation to enforce data integrity rules on the server side, independent of frontend validation.

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

**App Example:**
> A direct API call via Postman adds an assessment with `Score: 25` and `MaxScore: 20`. The API responds: `400 Bad Request` with body: `{ "errors": { "Score": ["Score cannot exceed the max score for this assessment"] } }`.

---

### FEAT-12: API Documentation

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

**App Example:**
> A new team member opens `http://localhost:5000/swagger/ui`, finds `POST /api/students`, clicks "Try it out", enters a student JSON payload, clicks Execute, and sees the 201 Created response — all without writing a single line of code.

---

## EPIC-06: Grade & Assessment Management

> **Goal**: Prevent free-text data inconsistency by using a controlled grade-level lookup, and give teachers the flexibility to record any number of individually named assessments on any marking scale.

---

### FEAT-13: Grade Level Lookup

> **Description**: Expose a read-only Grades API endpoint that the frontend consumes to populate the grade dropdown, replacing the old free-text grade field with a validated FK reference.

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

**App Example:**
> Mrs. Smith opens the Create Student form. The Grade dropdown is pre-populated with Grade 7 through Grade 12. She selects "Grade 10". The saved student record links to the `Grades` table row for Grade 10, not a string.

---

### FEAT-14: Individual Assessment CRUD

> **Description**: Allow teachers to add, edit, and delete individually named assessment entries on a student's detail page, each with a teacher-defined maximum score and an optional due date.

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

**App Example:**
> Mrs. Smith notices "Test 1" was entered with Score 17 instead of 19. She clicks Edit on that row, changes Score to 19, and clicks Save. The row updates and the Performance Summary increases.

---

## Summary Table

| ID | Level | Title | Parent | Points | Sprint |
|----|-------|-------|--------|:------:|:------:|
| EPIC-01 | Epic | Teacher Authentication | — | — | — |
| FEAT-01 | Feature | Teacher Registration | EPIC-01 | — | — |
| US-01 | User Story | Register with Full Profile | FEAT-01 | 5 | Sprint 1 |
| US-02 | User Story | Validate Registration Fields | FEAT-01 | 3 | Sprint 1 |
| FEAT-02 | Feature | Teacher Login | EPIC-01 | — | — |
| US-03 | User Story | Log In with Email and Password | FEAT-02 | 5 | Sprint 2 |
| US-04 | User Story | Handle Invalid Login Credentials | FEAT-02 | 2 | Sprint 2 |
| EPIC-02 | Epic | Student Management | — | — | — |
| FEAT-03 | Feature | Create Student | EPIC-02 | — | — |
| US-05 | User Story | Add a New Student Record | FEAT-03 | 5 | Sprint 2 |
| US-06 | User Story | Validate Student Input Fields | FEAT-03 | 3 | Sprint 2 |
| FEAT-04 | Feature | View Students | EPIC-02 | — | — |
| US-07 | User Story | View All Students in a Table | FEAT-04 | 5 | Sprint 3 |
| US-08 | User Story | View Detailed Student Profile | FEAT-04 | 3 | Sprint 3 |
| FEAT-05 | Feature | Edit Student | EPIC-02 | — | — |
| US-09 | User Story | Update Student Information | FEAT-05 | 5 | Sprint 3 |
| FEAT-06 | Feature | Delete Student | EPIC-02 | — | — |
| US-10 | User Story | Delete a Student Record | FEAT-06 | 3 | Sprint 3 |
| EPIC-03 | Epic | Assessment & Scoring | — | — | — |
| FEAT-07 | Feature | Score Entry | EPIC-03 | — | — |
| US-11 | User Story | Add Named Assessments with Flexible Scoring | FEAT-07 | 5 | Sprint 4 |
| FEAT-08 | Feature | Automatic Score Calculations | EPIC-03 | — | — |
| US-12 | User Story | View Total, Average, and Percentage | FEAT-08 | 3 | Sprint 4 |
| US-13 | User Story | View Performance Level Badge | FEAT-08 | 2 | Sprint 4 |
| EPIC-04 | Epic | Data Display & Interaction | — | — | — |
| FEAT-09 | Feature | DataTables Integration | EPIC-04 | — | — |
| US-14 | User Story | Sort Students by Column | FEAT-09 | 2 | Sprint 4 |
| US-15 | User Story | Search and Filter Students | FEAT-09 | 2 | Sprint 4 |
| EPIC-05 | Epic | API & Backend | — | — | — |
| FEAT-10 | Feature | RESTful API Endpoints | EPIC-05 | — | — |
| US-16 | User Story | Consume Student API from Frontend | FEAT-10 | 5 | Sprint 2 |
| US-17 | User Story | Consume Teacher API from Frontend | FEAT-10 | 3 | Sprint 3 |
| FEAT-11 | Feature | Input Validation (Backend) | EPIC-05 | — | — |
| US-18 | User Story | Reject Invalid Data Server-Side | FEAT-11 | 3 | Sprint 1 |
| FEAT-12 | Feature | API Documentation | EPIC-05 | — | — |
| US-19 | User Story | Explore API via Swagger UI | FEAT-12 | 2 | Sprint 1 |
| EPIC-06 | Epic | Grade & Assessment Management | — | — | — |
| FEAT-13 | Feature | Grade Level Lookup | EPIC-06 | — | — |
| US-20 | User Story | Select Grade from Controlled Dropdown | FEAT-13 | 2 | Sprint 3 |
| FEAT-14 | Feature | Individual Assessment CRUD | EPIC-06 | — | — |
| US-21 | User Story | Add a Named Assessment to a Student | FEAT-14 | 3 | Sprint 4 |
| US-22 | User Story | Edit and Delete an Individual Assessment | FEAT-14 | 3 | Sprint 4 |
| **Totals** | | **22 User Stories** | | **76 pts** | **4 Sprints** |
