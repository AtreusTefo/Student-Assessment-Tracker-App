# Daily Report — March 17, 2026

**Project**: Student Assessment Tracker
**Developer**: Developer.03
**Branch**: main

---

## What I Did Today

1. Created a summarized technology stack presentation covering all major technologies used in the application.
2. Extended the presentation to include the ELMAH error logging framework.
3. Analyzed the application's existing documentation and codebase to determine Agile methodology integration status.
4. Designed and created a formal Agile hierarchy document following the Epic > Feature > User Story structure.
5. Committed all local changes and pushed them to the remote GitHub repository.

---

## What Was Completed

### Technology Presentation (`docs/TECHNOLOGY_PRESENTATION.md`)
- Rewrote the document as a clean, structured presentation covering all 10+ technologies.
- Organized into two sections: **Core Technologies & Architecture** and **Key Libraries & Tools / Testing & Documentation**.
- Technologies documented:
  - Multilayered Architecture (Presentation, Application, Domain, Infrastructure layers)
  - ASP.NET Core (RESTful API backend)
  - Entity Framework Core with SQL Server LocalDB
  - Angular (Single-Page Application frontend)
  - DTOs (Data Transfer Objects)
  - AutoMapper
  - FluentValidation
  - DataTables
  - Swagger UI
  - Postman
  - **ELMAH** (Error Logging Modules and Handlers) — added as item #11

### Agile Hierarchy Document (`docs/AGILE_HIERARCHY.md`) New File
- Created a comprehensive Agile hierarchy document from scratch, fully grounded in the app's real functionality.
- Defined all three levels of the hierarchy with descriptions and format rules:
  - **Epic** — high-level business capability
  - **Feature** — specific deliverable within an Epic
  - **User Story** — smallest unit of work, user-perspective
- Delivered **5 Epics**, **12 Features**, and **19 User Stories**.
- Every User Story includes:
  - Standard format: *"As a [role], I want [goal], so that [benefit]."*
  - Acceptance Criteria (testable checkboxes)
  - App Example (real scenario from the Student Assessment Tracker)

| Epic | Features | User Stories |
|------|----------|-------------|
| EPIC-01: Teacher Authentication | FEAT-01, FEAT-02 | US-01 to US-04 |
| EPIC-02: Student Management | FEAT-03 to FEAT-06 | US-05 to US-10 |
| EPIC-03: Assessment & Scoring | FEAT-07, FEAT-08 | US-11 to US-13 |
| EPIC-04: Data Display & Interaction | FEAT-09 | US-14 to US-15 |
| EPIC-05: API & Backend | FEAT-10 to FEAT-12 | US-16 to US-19 |

### GitHub Push
- All changes committed with message:
  > `docs: add Agile hierarchy (Epic/Feature/User Story), update technology presentation with ELMAH, refresh docs and implementation checklist`
- Successfully pushed to `main` at: https://github.com/AtreusTefo/Student-Assessment-Tracker-App

---

## Challenges Faced and How They Were Resolved

### Challenge 1: Technology Presentation File Already Existed
- **Problem**: When attempting to create `TECHNOLOGY_PRESENTATION.md`, the file already existed with outdated content from a previous session.
- **Resolution**: Read the existing file contents first, then used a targeted replace to overwrite the entire content with the new, cleaner version — preserving the file rather than creating a duplicate.

### Challenge 2: No Agile Artifacts in the Project
- **Problem**: The project had no Epics, Features, or User Stories defined anywhere. Requirements were written as numbered functional steps, not in Agile format.
- **Resolution**: Cross-referenced `PROJECT_REQUIREMENTS.md`, `README.md`, and the app's routing/feature structure to map all existing functionality into a proper Agile hierarchy from scratch. Every story was grounded in real app behaviour, not invented.

### Challenge 3: Git Push Rejected (Non-Fast-Forward)
- **Problem**: `git push origin main` was rejected because the remote repository had commits that were not present locally (changes pushed directly to GitHub between sessions).
- **Resolution**: Ran `git pull --rebase origin main` to replay local commits on top of the remote state, keeping a clean linear history without a merge commit.

### Challenge 4: Merge Conflict During Rebase
- **Problem**: During the rebase, `StudentAssessmentTracker.postman_environment.json` had an add/add conflict — both the local and remote had added the same file independently.
- **Resolution**: Inspected both conflict sides and confirmed the content was identical in both versions. Removed the conflict markers, kept the single clean version, staged the file with `git add`, and ran `git rebase --continue`. The rebase completed successfully and the push went through cleanly.
