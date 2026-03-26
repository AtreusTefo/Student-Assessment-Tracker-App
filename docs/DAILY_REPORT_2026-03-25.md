# Daily Report — March 25, 2026

**Project:** Student Assessment Tracker  
**Developer:** Developer.03

---

## What I Did Today

- Fixed a dual-session conflict where a teacher and a student could both appear logged in simultaneously in the navigation bar
- Removed the inline logout button from the student dashboard header and consolidated logout into the global navigation bar
- Fixed invisible DataTables sort arrows on dark-themed table headers across the student list view
- Registered the DataTables CSS stylesheet globally in `angular.json` so it loads consistently across the application
- Expanded the Agile Hierarchy document from a three-level structure (Epic → Feature → User Story) to a full five-level structure (Application → Epic → Feature → User Story → Task), adding task breakdowns for every User Story in the product backlog

---

## What Was Completed

### 1. Dual-Session Conflict Fix — `app.html`
- Updated the student navigation block condition from `*ngIf="isStudentAuthenticated"` to `*ngIf="isStudentAuthenticated && !isAuthenticated"`
- The student greeting, "My Dashboard" button, and student logout button are now only rendered when a student is authenticated **and** no teacher session is active
- Prevents both sets of navigation controls from appearing at the same time

### 2. Mutual Session Clearing — `StudentAuthBusinessService` & `TeacherBusinessService`
- Injected `TeacherStateService` into `StudentAuthBusinessService`
- On student `activate()` and `login()`, `teacherState.logout()` is now called first, clearing any active teacher session before setting the student session
- Injected `StudentAuthStateService` into `TeacherBusinessService`
- On teacher `login()` and `register()`, `studentAuthState.logout()` is now called first, clearing any active student session before setting the teacher session
- Ensures only one user type can be authenticated at any given time

### 3. Student Dashboard Logout Button Removed — `student-dashboard.component.ts`
- Removed the `<button class="btn-logout">Logout</button>` element from the dashboard header template
- Removed the accompanying `.btn-logout` and `.btn-logout:hover` inline component styles
- Logout is now handled exclusively through the global navigation bar, providing a consistent UX

### 4. DataTables Sort Arrow Fix — `student-list.component.ts`
- Added component-level `::ng-deep` CSS rules targeting `.dt-column-order:before` and `.dt-column-order:after` pseudo-elements
- Default inactive arrows rendered at `opacity: 0.55` with `color: white`
- Active sort direction arrow (ascending or descending) rendered at full `opacity: 1` with `color: white`
- Used `!important` to override DataTables' own high-specificity opacity rules that were causing the arrows to be invisible against the dark gradient header

### 5. DataTables Sort Arrow Fix — `styles.scss` (Global Override)
- Added global CSS rules in `styles.scss` targeting all orderable and actively sorted column header states
- Global rules set `color: white` and `opacity: 0.5` for inactive arrows, `opacity: 1` for the active arrow
- Removed duplicate redundant global style blocks (`box-sizing`, `font-family`, second `html, body` block) that were already covered earlier in the file

### 6. DataTables CSS Registered Globally — `angular.json`
- Added `"node_modules/datatables.net-dt/css/dataTables.dataTables.css"` to the `styles` array in the Angular build configuration
- Ensures DataTables base styles are loaded globally rather than relying on component-level imports

### 7. Agile Hierarchy Document Expanded — `docs/AGILE_HIERARCHY.md`
- Upgraded the document title and overview to reflect the new **five-level** hierarchy
- Added a new **Application** level definition at the top of the hierarchy with a description of the Student Assessment Tracker as the root product
- Added a new **Task** level definition — the smallest unit of work within a User Story, assigned and completed within a single Sprint day
- Updated the Hierarchy Map to show all five levels from Application down to individual Tasks
- Added concrete `TASK-XX` entries to every User Story across all four Epics and their Features, covering:
  - EPIC-01: Security (Teacher Registration, Teacher Login, Input Validation)
  - EPIC-02: Student Management (Create, View, Update, Delete)
  - EPIC-03: Assessment & Grading (Record, View, Performance Tracking)
  - EPIC-04: Student Self-Service Portal (Activation, Login, Dashboard)
- Refined Epic, Feature, and User Story prose definitions for clarity and consistency

---

## Challenges Faced and How They Were Resolved

### Challenge 1 — Teacher and Student Sessions Conflicting in the Navigation Bar
**What happened:** Because teacher authentication and student authentication each use independent state services and `localStorage` keys, it was possible for both `isAuthenticated` (teacher) and `isStudentAuthenticated` (student) to be `true` simultaneously. This caused both sets of navigation controls — teacher greeting + logout and student greeting + "My Dashboard" + logout — to render at the same time, creating a confusing and broken UI.  
**Resolution:** Two changes were applied in tandem. First, the `app.html` student nav guard condition was tightened to `isStudentAuthenticated && !isAuthenticated`, so the student controls are hidden whenever a teacher is active. Second, each business service now explicitly clears the opposing session on login: `StudentAuthBusinessService` calls `teacherState.logout()` and `TeacherBusinessService` calls `studentAuthState.logout()`. This means only one session can ever be active at a time, regardless of `localStorage` state from a previous browser visit.

### Challenge 2 — DataTables Sort Arrows Invisible on Dark Headers
**What happened:** The DataTables default stylesheet sets sort arrow opacity to as low as `0.125` for inactive columns. Against the dark header background used in the student list table, this made the arrows completely invisible. Angular's view encapsulation also prevented straightforward component CSS from overriding DataTables' rules.  
**Resolution:** A two-layer fix was applied. Component-level fix: `::ng-deep` selectors were added to `student-list.component.ts` with `!important` to raise inactive arrow opacity to `0.55` and active arrow opacity to `1.0`, both in white. Global fix: matching rules were added to `styles.scss` (which bypasses view encapsulation entirely) and the DataTables base CSS was registered in `angular.json` to guarantee load order. The combination ensures the arrows are visible in all rendering environments.

### Challenge 3 — Logout Button Duplication Between Dashboard and Nav Bar
**What happened:** The student dashboard header contained its own inline logout button styled with component-scoped CSS. After adding a logout button to the global navigation bar as part of the session management improvements, two logout buttons were visible to the student simultaneously — one in the nav bar and one inside the dashboard card.  
**Resolution:** The inline logout button and its associated `.btn-logout` component styles were removed from `student-dashboard.component.ts`. The global navigation bar logout button is now the single authoritative logout control for students, consistent with the pattern already used for the teacher role.