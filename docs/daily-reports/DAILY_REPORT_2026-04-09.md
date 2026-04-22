# Daily Report — April 9, 2026

**Developer**: Atreus Tefo Ramokate  
**Sprint**: Sprint 6 — Feature Enhancements & UI Polish  
**Project**: Student Assessment Tracker

---

## What I Did Today

Completed the remaining Angular frontend work from the previous session's feature enhancement sprint and then designed and implemented a DataTables-native CSV export collection button to give teachers two distinct export workflows directly inside the student list. Investigated and explained the admin account bootstrap problem (chicken-and-egg issue with `POST /api/admins` requiring an Admin JWT that does not yet exist on a fresh database).

---

## What Was Completed

---

### 1 — Angular Export Buttons on Student Detail Page

**Files**: `StudentApp/src/app/components/student-detail.component.ts`

Added CSV and PDF download buttons to the student detail page, completing the final piece of the Data Export feature implemented in the backend the previous session.

**Changes made:**
- Imported `ReportApiService` from the core HTTP barrel
- Injected `ReportApiService` into the constructor alongside existing services
- Added `exportCsv()` method — calls `reportApi.exportStudentCsv(studentId)`, receives a `Blob`, creates an object URL, triggers an `<a>` download, then revokes the URL
- Added `exportPdf()` method — same pattern, downloads as `.pdf`
- Added **↓ Export CSV** and **↓ Export PDF** buttons in the `.actions` bar next to the existing Edit button
- Added `.btn-export-csv` (teal `#00897b`) and `.btn-export-pdf` (dark red `#c62828`) button styles

**Backend endpoints consumed:**
- `GET /api/reports/students/{studentId}/csv` → Teacher JWT
- `GET /api/reports/students/{studentId}/pdf` → Teacher JWT

---

### 2 — Angular Export Button on Student List Page

**Files**: `StudentApp/src/app/components/student-list.component.ts`

Added an "Export All CSV" button in the list header that downloads the full student dataset from the backend.

**Changes made:**
- Imported `ReportApiService` from the core HTTP barrel
- Injected `ReportApiService` into the constructor
- Added `exportAllCsv()` method — calls `reportApi.exportAllStudentsCsv()`, receives `Blob`, triggers browser download as `all-students-report.csv`
- Added a flex `.list-header-actions` wrapper in the template to align "Add New Student" and "Export All CSV" side-by-side
- Added `.btn-export-csv` style block

**Backend endpoint consumed:**
- `GET /api/reports/students/csv` → Teacher JWT

---

### 3 — README.md Features Section Updated

**File**: `README.md`

- Removed the five completed items from "Future Enhancements" and "Known Limitations" (email notifications, data export, class groups, admin panel, audit logging)
- Added five new feature sections under `## Features`: **Admin Panel**, **Audit Logging**, **Email Notifications**, **Data Export (CSV & PDF)**, **Class Groups**
- "Future Enhancements" section replaced with genuinely future items (SignalR, PWA, granular RBAC, bulk CSV import)

---

### 4 — DataTables Buttons Package Installed

**Files**: `StudentApp/package.json`

Installed the official DataTables Buttons extension:

```bash
npm install datatables.net-buttons datatables.net-buttons-dt
```

- `datatables.net-buttons` — core buttons plugin with CSV, Excel, PDF, and print engines
- `datatables.net-buttons-dt` — default DataTables theme stylesheet integration for buttons

---

### 5 — DataTables Collection Button (Export Dropdown)

**File**: `StudentApp/src/app/components/student-list.component.ts`

Replaced the standalone "Export All CSV" header button with a DataTables-native **collection button** (dropdown) embedded in the table toolbar — the cleanest integration because it keeps all table actions co-located with the table controls.

**Imports added:**
```typescript
import 'datatables.net-buttons-dt';
import 'datatables.net-buttons/js/buttons.html5.mjs';
```

**DataTable config changes:**
- `dom` changed from `'lfrtip'` → `'Blfrtip'` (the `B` slot renders the Buttons toolbar)
- `buttons` array configured with one `collection` entry containing two children:

| Button | Type | Behaviour |
|--------|------|-----------|
| Export current view (CSV) | DataTables built-in `csv` | Exports only rows visible after current search/filter/sort; explicitly excludes hidden percentage column (6) and Actions column (7) via `exportOptions.columns` |
| Export all students (CSV) | Custom `action` | Calls `this.exportAllCsv()` which hits `GET /api/reports/students/csv` — always returns the full dataset regardless of what DataTables has filtered |

**Template cleanup:** Removed the standalone `<button class="btn btn-export-csv">` from the header; `.list-header-actions` wrapper left solely for the "Add New Student" anchor.

---

### 6 — Export Dropdown UI Fix (Dropdown Stacking Over Button)

**Files**: `StudentApp/src/app/components/student-list.component.ts`, `StudentApp/src/styles.scss`

**Problem:** When the Export collection button was clicked, the dropdown items rendered stacked directly over the trigger button and were barely visible. The items also inherited DataTables' default `.dt-button` styles (dark gradients, text-shadows, opaque borders) which overrode all custom colours.

**Root cause:** DataTables appends `.dt-button-collection` directly to `<body>` at runtime — outside the Angular component tree entirely. Angular's `:host ::ng-deep` selectors are scoped to the component's DOM subtree and cannot target elements living at the body level, so the component-level styles silently had no effect on the dropdown.

**Fix — two-part:**

1. **`styles.scss` (global stylesheet)** — Added a full `.dt-button-collection` rule block at global scope, the only stylesheet that can reliably target body-level elements. Key properties set:
   - `position: absolute !important` with `z-index: 9999` — positions the dropdown below the trigger, not on top of it
   - `border-radius: 8px`, `box-shadow: 0 8px 24px rgba(0,0,0,0.14)` — clean card appearance
   - Each `.dt-button` child inside the collection gets all DataTables defaults stripped: `background: white`, `background-image: none`, `border: none`, `border-radius: 0`, `box-shadow: none`, `text-shadow: none`
   - Teal hover state (`#f0faf7` background, `#16a085` text) on each item
   - `.dt-button-background` (the click-outside backdrop) set to `position: fixed` covering the full viewport with `z-index: 9998`

2. **Component styles** — Updated collection trigger button to use a gradient (`#16a085 → #1abc9c`) with `!important` flags to win specificity against DataTables' default button stylesheet; kept as a fallback alongside the global rules.

---

### 7 — Admin Bootstrap Problem Investigated & Explained

Identified and documented the chicken-and-egg problem with admin account creation:

- `POST /api/admins` (create a new admin) requires `[Authorize(Roles = "Admin")]`
- On a fresh database there are no admin rows → no Admin JWT can be obtained → the endpoint is permanently unreachable

**Solution identified:** Seed a default admin row via `modelBuilder.Entity<Admin>().HasData(...)` in `ApplicationDbContext.OnModelCreating`. The BCrypt hash for the seed password must be pre-computed and hardcoded as a static string (not `BCrypt.HashPassword()` at runtime) because EF Core compares seed values between migration snapshots — a runtime-computed hash would generate a spurious "data changed" diff on every `dotnet ef migrations add`. `CreatedAt`/`UpdatedAt` must also be hardcoded `DateTime` literals for the same reason.

**Status:** Explained and ready to implement — awaiting confirmation to generate the hash, add `HasData`, and create the migration.

---

## Challenges Faced and How They Were Resolved

---

### Challenge 1 — DataTables Dropdown Rendered Outside Angular's DOM

**Problem:** The `.dt-button-collection` element is dynamically created by the DataTables JS library and appended to `<body>` — not inside the Angular component's host element. Angular's view encapsulation scopes all CSS (including `:host ::ng-deep`) to the component subtree. The dropdown was visually broken: items stacked on top of the trigger button rather than below it, and all custom colours were invisible.

**Investigation:** Inspecting the DOM in DevTools confirmed the dropdown was a direct child of `<body>` with class `dt-button-collection`. DataTables also injects a `dt-button-background` overlay div at the same level. Both were outside the Angular shadow boundary.

**Resolution:** Moved all dropdown styling into `src/styles.scss` (the global stylesheet). These rules apply to the entire document regardless of component boundaries. Used the nested SCSS syntax to keep `.dt-button` child rules scoped inside `.dt-button-collection` for specificity. Added `position: fixed` + full-viewport dimensions to `.dt-button-background` so clicking outside the dropdown correctly dismisses it without leaving a phantom overlay.

---

### Challenge 2 — DataTables Default `.dt-button` Styles Overriding Customs

**Problem:** Even after moving styles globally, the dropdown items were still rendering with a grey gradient background and dark text because DataTables' own stylesheet (`datatables.net-buttons-dt`) ships `.dt-button` rules with medium-to-high specificity that were winning over the custom rules.

**Resolution:** Added `!important` to all overriding properties on `.dt-button-collection .dt-button` (background, border, box-shadow, text-shadow, border-radius, color). This is the accepted pattern when overriding third-party component libraries that don't expose a theming API. The `!important` flags are tightly scoped to `.dt-button-collection .dt-button` — they do not bleed into any other part of the application.

---

### Challenge 3 — Admin Account Bootstrap (Chicken-and-Egg)

**Problem:** On a fresh database there is no way to reach `POST /api/admins` because the endpoint requires an existing Admin JWT. There is no public registration endpoint for admins (by design — open admin self-registration would be a security vulnerability).

**Resolution approach identified:** Use EF Core's `HasData` seeding mechanism to insert a default admin row inside the migration itself. The migration SQL is applied exactly once by `Database.MigrateAsync()` on first startup. The BCrypt hash must be pre-computed outside the application (not at startup) to produce a stable, deterministic value that EF Core's migration snapshot system can track correctly.

**Status:** Solution fully designed. Implementation pending.

---

## Summary

| Area | Status |
|------|--------|
| Angular — Student Detail export buttons (CSV + PDF) | ✅ Complete |
| Angular — Student List export button | ✅ Complete |
| DataTables Buttons package installed | ✅ Complete |
| DataTables collection export dropdown | ✅ Complete |
| Export dropdown UI fix (z-index, body-level styles) | ✅ Complete |
| README.md features section updated | ✅ Complete |
| Admin bootstrap seeding | ⏳ Designed, pending implementation |
