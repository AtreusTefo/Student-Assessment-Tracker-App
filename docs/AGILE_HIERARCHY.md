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
            └── US-11: Enter assessment scores (0–20)
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
A living, prioritized list of all work needed for the product. All 19 User Stories reside here, estimated in story points and ordered by business priority. The Product Owner is responsible for its content and ordering.

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

All 19 User Stories, prioritized by business value, with story point estimates and Sprint assignments. **Total estimated effort: 64 story points**.

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
| US-11 | Enter assessment scores (0–20) | Medium | 3 | Sprint 4 |
| US-12 | View total, average, and percentage | Medium | 3 | Sprint 4 |
| US-13 | View performance level badge | Medium | 2 | Sprint 4 |
| US-14 | Sort students by column | Low | 2 | Sprint 4 |
| US-15 | Search and filter students | Low | 2 | Sprint 4 |
| **Total** | | | **64** | |

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

#### Sprint 3 — Student Lifecycle & Views
**Dates**: March 16–22, 2026
**Sprint Goal**: *Complete the full student management lifecycle — list view, detail view, edit, and delete — and finalize teacher API integration in the Angular frontend.*
**Velocity**: 19 story points

| Story | Title | Points | Status |
|-------|-------|:------:|:------:|
| US-07 | View all students in a table | 5 | Done |
| US-08 | View detailed student profile | 3 | Done |
| US-09 | Update student information | 5 | Done |
| US-10 | Delete a student record | 3 | Done |
| US-17 | Consume teacher API from frontend | 3 | Done |
| **Total** | | **19** | |

**Sprint Review**: Full CRUD lifecycle for students is complete. Teachers can view all students, open individual detail pages, edit records, and delete with a confirmation step. All Angular routes (`/`, `/detail/:id`, `/edit/:id`) are functional. Teacher API integration (registration and login) is wired to the Angular frontend.

**Sprint Retrospective**:

| | Notes |
|-|-------|
| Start | Breaking large components into smaller, focused Angular services |
| Stop | Mixing business logic directly into Angular components |
| Continue | Validating API responses in the Angular service layer before rendering |

---

#### Sprint 4 — Assessment Scoring & DataTables Polish
**Dates**: March 23–29, 2026
**Sprint Goal**: *Implement automated assessment score calculations with performance level labels, and enhance the student table with DataTables sorting, searching, and pagination.*
**Velocity**: 12 story points

| Story | Title | Points | Status |
|-------|-------|:------:|:------:|
| US-11 | Enter assessment scores (0–20) | 3 | Done |
| US-12 | View total, average, and percentage | 3 | Done |
| US-13 | View performance level badge | 2 | Done |
| US-14 | Sort students by column | 2 | Done |
| US-15 | Search and filter students | 2 | Done |
| **Total** | | **12** | |

**Sprint Review**: Assessment scores (0–20 each) are captured, stored, and validated. Total, average, and percentage are auto-calculated. Performance level badges (Needs Support / Satisfactory / Good / Excellent) display correctly in both list and detail views. DataTables is integrated, providing real-time search, column sorting, and pagination across the student table.

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
- [ ] Create form collects: first name, last name, email, phone, grade, Assessment1, Assessment2, Assessment3.
- [ ] On successful submission, the student appears in the list.
- [ ] The teacher is redirected to the students list after creation.

**App Example:**
> Mrs. Smith clicks "Add Student", fills in: `John Doe, john@school.com, 12345678, Grade 10, Assessment1: 18, Assessment2: 15, Assessment3: 17`, and clicks Submit. John Doe now appears in the students table with a Total of 50, Average of 16.67, and a performance level of "Excellent".

---

#### US-06: Validate Student Input Fields

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 2

**As a** teacher,
**I want** the student form to validate my input before submitting,
**so that** only clean and correct data is saved to the system.

**Acceptance Criteria:**
- [ ] Assessment scores: required, integer, 0–20 inclusive.
- [ ] Names, email, phone follow the same rules as registration.
- [ ] Invalid fields display inline error messages.
- [ ] Backend (FluentValidation) rejects invalid payloads with a 400 response.

**App Example:**
> Mrs. Smith enters `25` for Assessment1 (exceeding the max of 20). The form shows: *"Assessment score must be between 0 and 20"* and blocks submission.

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
- [ ] The table shows key columns: name, email, grade, total score, performance level.
- [ ] The table is rendered using the DataTables library with sorting and pagination.

**App Example:**
> Mrs. Smith opens the app and sees a table listing all 25 students in her class, with columns for Name, Email, Grade, Total Score (e.g., 50/60), and Performance Level (e.g., "Excellent").

---

#### US-08: View Detailed Student Profile

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 3

**As a** teacher,
**I want** to click on a student and view their full profile,
**so that** I can see all their details and individual assessment scores.

**Acceptance Criteria:**
- [ ] A detail page at `/detail/:id` shows all student fields.
- [ ] Shows Assessment1, Assessment2, Assessment3 scores individually.
- [ ] Shows calculated Total, Average, Percentage, and Performance Level.
- [ ] Provides navigation back to the list and to the edit page.

**App Example:**
> Mrs. Smith clicks "View" on John Doe's row. She is taken to `/detail/1` where she sees all his details: *Assessment1: 18, Assessment2: 15, Assessment3: 17, Total: 50, Average: 16.67, Percentage: 83.3%, Level: Excellent*.

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
- [ ] Edit form at `/edit/:id` pre-populates with the student's current values.
- [ ] Teacher can change any field.
- [ ] On save, the record is updated and the teacher is redirected to the detail view.
- [ ] Calculated fields (total, average, percentage, level) automatically update.

**App Example:**
> Mrs. Smith realizes she entered John Doe's Assessment2 score as 15 instead of 19. She navigates to `/edit/1`, changes Assessment2 to 19, and saves. John's new Total is 54, Percentage is 90%, and Level is still "Excellent".

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

> **Description**: Allow teachers to record up to three assessment scores per student.

---

#### US-11: Enter Assessment Scores (0–20)

> **Story Points**: 3 &nbsp;|&nbsp; **Sprint**: Sprint 4

**As a** teacher,
**I want** to enter three separate assessment scores for each student,
**so that** the system can calculate their overall performance.

**Acceptance Criteria:**
- [ ] Three score fields: Assessment1, Assessment2, Assessment3.
- [ ] Each score must be an integer between 0 and 20.
- [ ] Scores are stored and retrievable via the API.

**App Example:**
> When adding John Doe, Mrs. Smith enters: `Assessment1: 18, Assessment2: 19, Assessment3: 17`. These are saved with his record.

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
- [ ] Total = Assessment1 + Assessment2 + Assessment3 (max: 60)
- [ ] Average = Total / 3
- [ ] Percentage = (Total / 60) × 100
- [ ] Results are displayed on the student detail page and in the list table.

**App Example:**
> John Doe's scores: 18, 19, 17. The system displays: *Total: 54, Average: 18.00, Percentage: 90.0%* automatically — no manual input needed.

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
- [ ] `GET /api/students/{id}` — retrieves a single student.
- [ ] `PUT /api/students/{id}` — updates a student.
- [ ] `DELETE /api/students/{id}` — deletes a student.
- [ ] All responses use consistent JSON shapes (StudentDto).

**App Example:**
> When Mrs. Smith loads the homepage, the Angular `StudentService` fires `GET /api/students` → the API returns a JSON array of `StudentDto` objects → DataTables renders them in the table.

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
- [ ] Validation rules mirror the frontend rules (names, email, phone, scores 0–20).

**App Example:**
> If a direct API call is made via Postman with `Assessment1: -5`, the API responds: `400 Bad Request` with body: `{ "errors": { "Assessment1": ["Assessment score must be between 0 and 20"] } }`.

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
| US-11 | User Story | Enter Assessment Scores (0–20) | FEAT-07 | 3 | Sprint 4 |
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
| **Totals** | | **19 User Stories** | | **64 pts** | **4 Sprints** |
