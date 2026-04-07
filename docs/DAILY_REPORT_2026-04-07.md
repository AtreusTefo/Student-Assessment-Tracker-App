# Daily Report — April 7, 2026

**Developer**: Atreus Tefo Ramokate
**Sprint**: Sprint 5 (Post-Sprint Documentation)
**Project**: Student Assessment Tracker

---

## What I Did Today

One documentation work stream was completed:

1. **Documentation Audit & Update** — reviewed the entire codebase (all entities, DTOs, services, controllers, Angular components, guards, interceptors, and configuration files) and updated both `docs/AGILE_HIERARCHY.md` and `README.md` to accurately reflect the application as it currently stands after all five Sprints.

---

## What Was Completed

### AGILE_HIERARCHY.md — Full Sync to Current Codebase

#### Hierarchy Map
- `FEAT-14: Student Dashboard` was incorrectly formatted as the last (`└──`) item under EPIC-06, blocking addition of further features. Corrected to a mid-level (`├──`) item.
- Added new **`FEAT-15: Assessment File Submissions`** as the final feature under EPIC-06, containing:
  - `US-30`: As a student, I want to upload a file for one of my assessments, so that I can submit my completed work digitally. (TASK-93 – TASK-96)
  - `US-31`: As a teacher, I want to download and delete student file submissions, so that I can retrieve completed work for marking. (TASK-97 – TASK-99)

#### Scrum Artifacts Section
- Fixed stale count in the Product Backlog description: "All **22** User Stories" → "All **31** User Stories".

#### Product Backlog Table
- Updated intro line: "All 31 User Stories … **Total estimated effort: 102 story points**" (was 29 stories / 94 pts).
- Added two new rows: `US-30` (5 pts, Sprint 5) and `US-31` (3 pts, Sprint 5).
- Updated the **Total** row from 94 → **102 story points**.

#### Sprint 4 Status
- All ten Sprint 4 User Stories updated from `—` (pending) to `Done`.
- Sprint 4 Review replaced "Pending Sprint 4 completion" with a full written review covering: named assessments with custom `MaxScore` and `DueDate`, inline edit/delete, server-side score calculations surfaced through `StudentDetailDto` and `StudentListDto`, performance level badges, DataTables configuration, and the student self-service dashboard (`StudentProfileDto`, summary cards, progress bar, Overdue/Submitted badges, My Profile section).

#### Sprint 5 — New Section Added
- Full Sprint 5 block added: dates March 30 – April 5, 2026; velocity 8 story points.
- Sprint Backlog table with US-30 and US-31 both marked `Done`.
- Sprint 5 Review written: file upload modal, server-side storage, `AssessmentSubmissionsController` role-based access rules, and API-boundary file validation.
- Sprint 5 Retrospective written with Start / Stop / Continue entries.

#### US-30 and US-31 — Full Detail Sections Added
- Each new User Story was given its full section matching the document standard:
  - Story statement, story points, sprint assignment
  - Acceptance Criteria (checkbox list)
  - Tasks (TASK-93 through TASK-99)
  - App Example scenario

#### Summary Table
- Added `FEAT-15`, `US-30`, and `US-31` rows.
- Updated totals row: "**31 User Stories · 99 Tasks · 102 pts · 5 Sprints**" (was 29 stories / 92 tasks / 94 pts / 4 Sprints).

---

### README.md — Full Rewrite to Match Current State

| Section | What Changed |
|---------|-------------|
| **Introduction** | Rewritten to reflect the dual-role system (teachers + students), JWT authentication, file submissions, flexible scoring, and the DataTables-powered student list. |
| **Technology Stack — Backend** | Database changed from "In-Memory" to "SQL Server LocalDB (EF Core Migrations)"; JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) added as a listed technology. |
| **Database section** | Completely rewritten: replaced the in-memory `UseInMemoryDatabase` code block with the SQL Server LocalDB connection string example; added the full list of auto-seeded lookup data (Grades 7–12 and 12 Subjects). |
| **Features** | Replaced three outdated bullet groups (Student Management, Data Validation, Automatic Calculations, User Interface) with five accurate sections: Authentication & Security, Student Management (Teacher Role), Assessment Tracking (Teacher Role), File Submissions, Student Self-Service Portal. |
| **Project Structure** | Updated tree to reflect all 7 entities, 6 DTO files, 4 services, 4 repositories, 6 controllers; updated Angular section to list all 8 components, 4 guards, 1 interceptor, 2 model files, 6 HTTP services, and 3 state services across the correct folder paths (`core/`, `features/`, `components/`). |
| **API Endpoints** | Replaced the 5-row simple list with four fully documented tables covering: Teachers (6 endpoints), Students (7 endpoints), Assessments (5 endpoints), File Submissions (4 endpoints), and Lookups (2 endpoints) — each with method, path, auth requirement, and description. |
| **Usage** | Replaced generic 5-step list with separate **Teacher Workflow** (7 steps) and **Student Workflow** (5 steps) that match actual app routes and features. |
| **Form Validation Rules** | Expanded from one student form to all four forms: Teacher Registration, Student Create/Edit, Assessment, and Student Activation — each with accurate field rules matching the current FluentValidation validators. |
| **Known Limitations** | Removed resolved limitations (in-memory database, no authentication); replaced with accurate current limitations (8-digit phone, read-only student profile, byte array file storage, no email notifications, no audit log). |
| **Future Enhancements** | Removed already-implemented items (SQL Server, authentication); replaced with genuine future work (email notifications, CSV/PDF export, class grouping, admin role, audit logging). |
| **Development Notes** | Updated the "Adding New Features" checklist to include the full backend chain (entity → DTO → validator → service → repository → controller) and the EF Core migration step. |
| **Recent Fixes section** | Removed entirely — contained resolved historical bugs no longer relevant; replaced with a pointer to the `docs/` folder. |
| **Proxy note** | Corrected from `https://localhost:5001` to `http://localhost:5000` to match the actual `proxy.conf.json` target. |

---

## Challenges Faced and How They Were Resolved

### Challenge #1 — Hierarchy Map Tree Formatting (FEAT-15 Placement)
**Problem**: The `FEAT-14: Student Dashboard` node used the `└──` (last-child) box-drawing character, meaning any content appended after it would appear outside the tree rather than as a sibling node. Adding FEAT-15 as a visual sibling was not possible without first fixing the branch character.

**Resolution**: Used `replace_string_in_file` to change `└── FEAT-14` to `├── FEAT-14` and simultaneously append the full `└── FEAT-15` block with all its user story and task lines in the same operation. The tree rendered correctly after the single replacement.

---

### Challenge #2 — Stale Count References Scattered Across the Document
**Problem**: The document referenced "All 22 User Stories" in the Scrum Artifacts section and "94 story points" in two separate places (the Product Backlog intro and the Summary Table totals row). These three references were inconsistent with each other and with the actual backlog, which now contains 31 stories across 5 sprints totalling 102 points.

**Resolution**: Used `multi_replace_string_in_file` to update all three stale references in a single call, ensuring consistency across the entire document in one operation without risking partial updates.

---

### Challenge #3 — Sprint 4 Review Was a Placeholder
**Problem**: The Sprint 4 Review read *"Pending Sprint 4 completion — March 23–29, 2026"* — a placeholder that was never filled in after Sprint 4 was delivered. All ten stories were also marked `—` instead of `Done`.

**Resolution**: Wrote a complete Sprint 4 Review paragraph based on the implemented code (named assessments with `MaxScore`/`DueDate`, inline edit/delete, server-side score calculations, performance badges, DataTables, and the `StudentProfileDto`-backed student dashboard). All ten story status cells updated to `Done` in the same multi-replace operation.

---

### Challenge #4 — README Technology Stack Was Outdated
**Problem**: The README still described an **In-Memory database** with no authentication — the original prototype state. The application had since been migrated to SQL Server LocalDB with EF Core migrations and full JWT Bearer authentication added for two separate roles.

**Resolution**: The Database section was rewritten using the actual `appsettings.Development.json` connection string. The Backend technology stack table was updated to list SQL Server LocalDB and JWT Bearer. The Known Limitations section had "In-memory database clears data on restart" and "Single-user application (no authentication)" removed, as both have been resolved.

---

### Challenge #5 — README Project Structure Was Outdated
**Problem**: The Project Structure tree only listed a single `Student.cs` entity, a single `StudentRepository.cs`, and a single `StudentsController.cs` — reflecting the original single-entity prototype. The actual codebase has 7 entities, 4 repositories, 6 controllers, and a significantly more complex Angular structure with `core/`, `features/`, guards, interceptors, and state services.

**Resolution**: The tree was fully redrawn to accurately represent every file and folder in both the backend and frontend, matching the structure confirmed by codebase exploration.

---

## Files Changed

| File | Type | Changes |
|------|------|---------|
| `docs/AGILE_HIERARCHY.md` | Modified | FEAT-15 added to hierarchy map and detail sections; US-30 & US-31 full stories added; Sprint 4 status marked Done and review written; Sprint 5 section added; Product Backlog updated to 31 stories / 102 pts; Summary Table updated to 31 stories / 99 tasks / 5 Sprints |
| `README.md` | Modified | Introduction, technology stack, database, features, project structure, API endpoints, usage, form validation, known limitations, future enhancements, development notes — all rewritten to match the current codebase |

---

## Summary

| Metric | Value |
|--------|-------|
| Files modified | 2 |
| Sections rewritten / added | 18 |
| New User Stories documented | 2 (US-30, US-31) |
| New Tasks documented | 7 (TASK-93 – TASK-99) |
| New Sprint documented | 1 (Sprint 5) |
| Stale references corrected | 6 |
| Resolved challenges | 5 |
