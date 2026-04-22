# DataTables Integration

## Overview
DataTables.net v2 is integrated into the Angular frontend to provide paginated, sortable, and searchable tables. The **Buttons** plugin is also included to power the CSV export feature on the Student List page.

## Installed Packages

```bash
npm install datatables.net datatables.net-dt datatables.net-buttons datatables.net-buttons-dt --save
```

| Package | Version | Purpose |
|---|---|---|
| `datatables.net` | ^2.3.7 | Core DataTables library |
| `datatables.net-dt` | ^2.3.7 | Default DataTables CSS theme |
| `datatables.net-buttons` | ^3.2.6 | Export buttons plugin |
| `datatables.net-buttons-dt` | ^3.2.6 | Buttons plugin default theme |

> **Note:** `angular-datatables` (the Angular wrapper) was evaluated but replaced with direct DataTables.js integration for full compatibility with Angular standalone components (Angular 21).

## Architecture

DataTables is used **imperatively** via `ViewChild` and `AfterViewInit`  no Angular wrapper library. This is compatible with standalone components and zoneless change detection (Angular 21).

## CSS

The DataTables theme CSS is loaded as a **global style** in `angular.json`:

```json
"styles": [
  "node_modules/datatables.net-dt/css/dataTables.dataTables.css",
  "src/styles.scss"
]
```

No per-component CSS imports are needed.

## Student List (`student-list.component.ts`)

Primary DataTables consumer. Displays all students for the authenticated teacher.

**Features enabled:**
- Pagination (`full_numbers` style, 10 rows per page)
- Global search/filter across all text columns
- Column sorting (disabled on the Actions column)
- CSV export button via the Buttons plugin

**Key implementation pattern:**

```typescript
import DataTable from 'datatables.net-dt';
import 'datatables.net-buttons-dt';

@ViewChild('studentTable') table!: ElementRef;
private dataTable: any = null;

ngAfterViewInit(): void {
  if (this.dataTable) { this.dataTable.destroy(); }
  this.dataTable = new DataTable(this.table.nativeElement, {
    pagingType: 'full_numbers',
    pageLength: 10,
    dom: 'Blfrtip',   // B = Buttons
    buttons: ['csv'],
    columnDefs: [
      { targets: -1, orderable: false, searchable: false }  // Actions column
    ]
  });
}
```

**DataTable lifecycle  destroy before re-init:**
The DataTable instance is destroyed before re-initialising whenever the student list data changes. This prevents duplicate table instances and DOM conflicts.

## Student Detail (`student-detail.component.ts`)

The assessment table on the detail page also uses DataTables for sorting and pagination of assessment records per student.

## Features Maintained

- CRUD action buttons (View, Edit, Delete) preserved in Actions column
- Delete confirmation modal
- Loading and error states
- `StudentStateService` observable subscription

## Build Budget Note

The production bundle may exceed the 500 kB warning budget due to DataTables and its plugins. The error budget is set to 1 MB in `angular.json`. If the warning is undesirable, DataTables can be lazy-loaded or the budget can be adjusted.

### Search/Filter
Users can:
- Type in the search box to filter records in real-time
- Sort columns by clicking headers
- Change page length from dropdown
- Navigate between pages

## Component Lifecycle
```
1. ngOnInit()
   - Initialize student list from API
   - Set up router navigation listener

2. ngAfterViewInit()
   - Call initializeDataTable()
   - Create DataTable instance with configuration

3. loadStudents()
   - Fetch students from API
   - Re-initialize DataTable with new data

4. ngOnDestroy()
   - Destroy DataTable instance
   - Clean up subscriptions
```

## Styling
DataTable functionality is styled using:
- Component-scoped styles for Student List component
- Global table styling in component template
- Default class names for DataTable elements:
  - `.dataTables_wrapper` - Main wrapper
  - `.dataTables_filter` - Search input
  - `.dataTables_paginate` - Pagination controls
  - `.dataTable` - Table element

## Technical Decisions

### Why Direct DataTables.js Instead of angular-datatables?
1. **Standalone Compatibility:** `angular-datatables` is designed for NgModule-based apps and has peer dependency issues with Angular 21+
2. **Simpler Integration:** Direct DataTables.js provides more flexibility for standalone components
3. **Manual Control:** Direct control over table initialization in `AfterViewInit` lifecycle hook
4. **No Wrapper Dependencies:** Fewer dependencies to manage

### CSS Handling
DataTables default CSS was excluded from the build to avoid complex import paths. The component uses its own styling with DataTables functionality working without the default theme (table still displays correctly and all features work).

## Testing Checklist
- [x] Angular frontend builds without errors
- [x] Backend API builds without errors
- [x] Table displays with all student records
- [x] Sorting works on columns
- [x] Search/filter functionality works
- [x] Pagination controls visible and functional
- [x] Action buttons (View, Edit, Delete) still responsive
- [x] Delete confirmation modal still appears
- [x] Page refreshes reload student data
- [x] No console errors in browser

## Future Enhancements
Potential improvements that could be added:
1. Export table data to CSV/Excel
2. Advanced column filtering options
3. Column visibility toggle
4. Custom date/status column rendering
5. Bulk actions on selected rows
6. Server-side pagination for large datasets
7. Custom row styling based on performance level
8. Responsive design optimizations for mobile

## Package.json Updates
The following dependencies are now in studentApp `package.json`:
```json
{
  "datatables.net": "^2.2.0",
  "datatables.net-dt": "^2.2.0"
}
```

## References
- [DataTables Documentation](https://datatables.net/)
- [DataTables API Reference](https://datatables.net/reference/)
- [Angular Standalone Components](https://angular.io/guide/standalone-components)

## Conclusion
DataTables integration is complete and working successfully. The student list component now provides enterprise-grade table functionality while maintaining all existing CRUD operations and UI patterns. Both frontend and backend applications build and run without errors.
