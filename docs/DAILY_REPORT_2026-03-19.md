# Daily Report — March 19, 2026

**Project**: Student Assessment Tracker
**Developer**: Developer.03
**Branch**: main

---

## What I Did Today

1. Committed and pushed all local code changes (28 files) from the previous session to the remote GitHub repository.
2. Performed a major update of `docs/AGILE_HIERARCHY.md` to reflect the new Grade and Assessment entities added to the codebase.
3. Analysed the Student list table to identify areas of improvement.
4. Implemented the priority recommendation — expanding the student list table with seven visible columns, colour-coded performance badges, and smart DataTables sorting.
5. Fixed `StudentBusinessService` mapping and state-refresh logic to correctly handle the new, wider `StudentListDto`.
6. Performed a final sync of `AGILE_HIERARCHY.md` to document the list table improvement in US-07, Sprint 3, and the overall story count.

---

## What Was Completed

### Git Commit & Push
- Staged and committed 28 files with message:
  > `feat: add grades & assessments, refactor student/teacher entities and DTOs`
- Resolved a merge conflict on `docs/DATABASE_BACKUP_ROLLBACK_GUIDE.md` during `git pull --rebase` (remote had independently added the same file — resolved by keeping the remote version).
- Successfully pushed to `main` at: https://github.com/AtreusTefo/Student-Assessment-Tracker-App

---

### AGILE_HIERARCHY.md — Major Update (Phase 1)

Rewrote significant portions of the document to reflect the March 18 migration (`AddGradesAndAssessmentsRefactoring`) and the new codebase state.

| Area | Change |
|------|--------|
| EPIC-06 | New: Grade & Assessment Management |
| FEAT-13, FEAT-14 | New features under EPIC-06 |
| US-20, US-21, US-22 | New user stories (Grade lookup, Assessment CRUD, Scoring display) |
| US-05, US-06, US-08, US-09 | Updated acceptance criteria to reflect refactored Student entity (`StudentUniqueId`, `IdPassportNo`, `GradeId` FK) |
| US-11 | Rewritten — flexible named assessments with custom `MaxScore`, replacing legacy fixed 0–20 scores |
| US-12 | Updated — `MaxPossible` is now the dynamic sum of all assessment `MaxScore` values |
| US-16 | Updated — added `/api/grades` and nested `/api/students/{id}/assessments` endpoints |
| US-18 | Updated — new FluentValidation rules for `StudentAssessmentValidator` |
| Product Backlog | 19 → 22 stories; 64 → 76 story points |
| Sprint 3 | Velocity 19 → 21; added US-20; updated sprint review |
| Sprint 4 | Velocity 12 → 20; added US-21, US-22; status updated to pending |
| Summary Table | Updated to 22 stories / 76 points |

---

### Student List Table Improvement

**Analysis identified** that the API was already returning rich student data (email, grade name, scores, performance level) but the frontend was silently discarding it due to a narrow `StudentListDto`.

**Changes implemented across three files:**

#### `StudentApp/src/app/core/models/student.model.ts`
- Expanded `StudentListDto` from 3 fields to 12 fields:
  - Added: `studentUniqueId`, `email`, `gradeName`, `totalScore`, `maxPossible`, `percentage`, `performanceLevel`, `assessmentCount`

#### `StudentApp/src/app/components/student-list.component.ts`
- Expanded table from 3 columns to 7 visible columns + 1 hidden:

| Column | Field | Notes |
|--------|-------|-------|
| Student ID | `studentUniqueId` | — |
| Name | `firstName + lastName` | — |
| Email | `email` | — |
| Grade | `gradeName` | — |
| Score | `totalScore / maxPossible` | Shows "No assessments" when none exist |
| Performance | `performanceLevel` | Colour-coded badge |
| *(hidden)* | `percentage` | Used for sorting only |
| Actions | — | Edit / Delete (not sortable) |

- Added `getPerformanceClass()` method returning CSS class bindings:
  - `badge-excellent` (green) — Excellent
  - `badge-good` (blue) — Good
  - `badge-satisfactory` (yellow) — Satisfactory
  - `badge-needs-support` (red) — Needs Support
- Updated DataTables `columnDefs`:
  - Column 6 (percentage) hidden and non-searchable
  - Column 5 (Performance) sorts by column 6 (numeric percentage) — prevents alphabetical sort
  - Column 7 (Actions) non-sortable and non-searchable

#### `StudentApp/src/app/features/students/services/student-business.service.ts`
- **`createStudent`** — fixed to map all 10 fields from `StudentDetailDto` response into `StudentListDto` (previously only mapped `id`, `firstName`, `lastName`).
- **`updateStudent`** — replaced partial in-memory patch (which only updated `firstName`/`lastName`) with a full list reload from the API, ensuring all fields stay in sync after an edit.

---

### AGILE_HIERARCHY.md — Final Sync (Phase 2)

Applied 5 targeted updates to align the document with the just-completed list table changes:

1. **Scrum Artifacts section** — "All 19 User Stories" corrected to "All 22 User Stories".
2. **Sprint 3 table** — removed stray `ye` typo.
3. **US-07 Acceptance Criteria** — rewritten to match the real table output (7 visible columns, colour-coded badge, "No assessments" empty state, percentage-based sort).
4. **US-07 App Example** — updated to show real data format (`STU-A1B2C3D4`, `144/170`, green Excellent badge).
5. **Sprint 3 Review** — added paragraph documenting the `StudentListDto` expansion, new columns, badge styles, and DataTables percentage-sort fix.

---

### TypeScript Validation
- Ran `npx tsc --noEmit` across the Angular project — exited with code 0 (no type errors).

---

## Challenges Faced

### Challenge 1: Git Merge Conflict During Rebase
- **Problem**: `git pull --rebase` failed because the remote had independently added `docs/DATABASE_BACKUP_ROLLBACK_GUIDE.md` and the local branch also had a copy, causing an add/add conflict.
- **Resolution**: Used `git checkout --ours` to keep the remote version of the file, staged it with `git add`, and completed the rebase with `git rebase --continue`. Push succeeded cleanly.

### Challenge 2: StudentListDto Too Narrow — Data Was Being Discarded
- **Problem**: The frontend `StudentListDto` interface only had 3 fields (`id`, `firstName`, `lastName`), but the API was returning a full 12-field response. Every extra field (email, grade, scores, performance level) was silently dropped when the response was mapped into state.
- **Resolution**: Expanded the `StudentListDto` interface to include all 12 fields. Updated the component template to display them, and fixed the business service to ensure the full data is mapped and preserved during both create and update operations.

### Challenge 3: Performance Badge Sorting Alphabetically Instead of Numerically
- **Problem**: "Excellent" sorts before "Good" alphabetically. Sorting the Performance column by text would produce an incorrect order (Excellent → Good → Needs Support → Satisfactory), not reflecting actual performance rank.
- **Resolution**: Added a hidden numeric column (index 6) containing the raw `percentage` value, then used DataTables' `orderData: [6]` on the Performance column (index 5) to force sorting by the underlying number rather than the badge text.

### Challenge 4: updateStudent Only Syncing Two Fields
- **Problem**: After a student edit, the business service was manually patching only `firstName` and `lastName` into the local state. All new fields (`email`, `gradeName`, `totalScore`, etc.) would revert to `undefined` or stale values until the page was refreshed.
- **Resolution**: Replaced the manual patch with a full `loadStudents()` call after a successful update, so the state always reflects the complete, authoritative data from the API.

### Challenge 5: Dev Servers Not Starting (Pre-existing)
- **Problem**: Both `dotnet run` (API) and `npm start` (Angular) are exiting with code 1. This is a pre-existing issue carried over from a previous session and was not introduced by today's changes.
- **Status**: Not resolved today — diagnosed as a pre-existing environment issue. To be investigated in the next session.
