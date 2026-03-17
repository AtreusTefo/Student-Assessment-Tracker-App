# Agile Methodology — Epic > Feature > User Story Hierarchy

## Overview

This document maps the Student Assessment Tracker application to a formal Agile hierarchy. Each level of the hierarchy is defined below, followed by a full breakdown of all Epics, Features, and User Stories derived directly from the app's requirements and functionality.

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

## EPIC-01: Teacher Authentication

> **Goal**: Enable teachers to securely register and log in to the application so only authorized teachers can access student data.

---

### FEAT-01: Teacher Registration

> **Description**: Allow a new teacher to create an account by filling in their personal and professional details.

---

#### US-01: Register with Full Profile

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

| ID | Level | Title | Parent |
|----|-------|-------|--------|
| EPIC-01 | Epic | Teacher Authentication | — |
| FEAT-01 | Feature | Teacher Registration | EPIC-01 |
| US-01 | User Story | Register with Full Profile | FEAT-01 |
| US-02 | User Story | Validate Registration Fields | FEAT-01 |
| FEAT-02 | Feature | Teacher Login | EPIC-01 |
| US-03 | User Story | Log In with Email and Password | FEAT-02 |
| US-04 | User Story | Handle Invalid Login Credentials | FEAT-02 |
| EPIC-02 | Epic | Student Management | — |
| FEAT-03 | Feature | Create Student | EPIC-02 |
| US-05 | User Story | Add a New Student Record | FEAT-03 |
| US-06 | User Story | Validate Student Input Fields | FEAT-03 |
| FEAT-04 | Feature | View Students | EPIC-02 |
| US-07 | User Story | View All Students in a Table | FEAT-04 |
| US-08 | User Story | View Detailed Student Profile | FEAT-04 |
| FEAT-05 | Feature | Edit Student | EPIC-02 |
| US-09 | User Story | Update Student Information | FEAT-05 |
| FEAT-06 | Feature | Delete Student | EPIC-02 |
| US-10 | User Story | Delete a Student Record | FEAT-06 |
| EPIC-03 | Epic | Assessment & Scoring | — |
| FEAT-07 | Feature | Score Entry | EPIC-03 |
| US-11 | User Story | Enter Assessment Scores (0–20) | FEAT-07 |
| FEAT-08 | Feature | Automatic Score Calculations | EPIC-03 |
| US-12 | User Story | View Total, Average, and Percentage | FEAT-08 |
| US-13 | User Story | View Performance Level Badge | FEAT-08 |
| EPIC-04 | Epic | Data Display & Interaction | — |
| FEAT-09 | Feature | DataTables Integration | EPIC-04 |
| US-14 | User Story | Sort Students by Column | FEAT-09 |
| US-15 | User Story | Search and Filter Students | FEAT-09 |
| EPIC-05 | Epic | API & Backend | — |
| FEAT-10 | Feature | RESTful API Endpoints | EPIC-05 |
| US-16 | User Story | Consume Student API from Frontend | FEAT-10 |
| US-17 | User Story | Consume Teacher API from Frontend | FEAT-10 |
| FEAT-11 | Feature | Input Validation (Backend) | EPIC-05 |
| US-18 | User Story | Reject Invalid Data Server-Side | FEAT-11 |
| FEAT-12 | Feature | API Documentation | EPIC-05 |
| US-19 | User Story | Explore API via Swagger UI | FEAT-12 |
