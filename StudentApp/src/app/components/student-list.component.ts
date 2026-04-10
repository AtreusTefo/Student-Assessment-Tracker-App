import { Component, OnInit, OnDestroy, ChangeDetectorRef, ViewChild, ElementRef, AfterViewInit, NgZone } from '@angular/core';
import { Router, NavigationEnd, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { StudentListDto } from '../core/models';
import { StudentStateService } from '../core/services/state';
import { StudentBusinessService } from '../features/students/services/student-business.service';
import { ReportApiService } from '../core/services/http';
import { Subject } from 'rxjs';
import { takeUntil, filter } from 'rxjs/operators';

// Import DataTables
import DataTable from 'datatables.net-dt';
import 'datatables.net-buttons-dt';
import 'datatables.net-buttons/js/buttons.html5.mjs';

/**
 * PRESENTATION LAYER - Student List Component
 * Responsible ONLY for UI presentation and user interactions
 * Delegates all business logic to StudentBusinessService
 * Subscribes to StudentStateService for reactive data
 */

@Component({
  selector: 'app-student-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="container">
      <h2>Student List</h2>
      <div class="list-header-actions">
        <a routerLink="/create" class="btn btn-primary">Add New Student</a>
      </div>
      
      <div *ngIf="loading" class="loading">Loading students...</div>
      
      <div *ngIf="error" class="error">{{ error }}</div>
      
      <div *ngIf="!loading && !error && students.length > 0">
        <table #studentsTable class="table table-striped table-bordered display" style="width:100%">
          <thead>
            <tr>
              <th>Student ID</th>
              <th>Name</th>
              <th>Email</th>
              <th>Grade</th>
              <th>Score</th>
              <th>Performance</th>
              <!-- hidden column used by DataTables to sort Performance by percentage -->
              <th></th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let student of students">
              <td>{{ student.studentUniqueId }}</td>
              <td>{{ student.firstName }} {{ student.lastName }}</td>
              <td>{{ student.email }}</td>
              <td>{{ student.gradeName }}</td>
              <td>
                <span *ngIf="student.maxPossible > 0">{{ student.totalScore | number:'1.0-1' }} / {{ student.maxPossible | number:'1.0-1' }}</span>
                <span *ngIf="student.maxPossible === 0" class="muted">No assessments</span>
              </td>
              <td>
                <span *ngIf="student.maxPossible > 0" class="badge" [ngClass]="getPerformanceClass(student.performanceLevel)">{{ student.performanceLevel }}</span>
                <span *ngIf="student.maxPossible === 0" class="muted">—</span>
              </td>
              <!-- hidden percentage value for sorting -->
              <td style="display:none">{{ student.percentage }}</td>
              <td class="action-cell">
                <button data-action="view" [attr.data-id]="student.id" class="btn btn-info btn-sm">View</button>
                <button data-action="edit" [attr.data-id]="student.id" class="btn btn-warning btn-sm">Edit</button>
                <button data-action="delete" [attr.data-id]="student.id" class="btn btn-danger btn-sm">Delete</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      
      <div *ngIf="students.length === 0 && !loading && !error" class="no-data">
        No students found. <a routerLink="/create">Create one</a>
      </div>

      <!-- Confirmation Modal -->
      <div *ngIf="showConfirmDialog" class="modal-overlay">
        <div class="modal">
          <div class="modal-header">
            <h3>Confirm Delete</h3>
          </div>
          <div class="modal-body">
            <p>Are you sure you want to delete this student? This action cannot be undone.</p>
          </div>
          <div class="modal-footer">
            <button (click)="confirmDelete()" class="btn btn-danger">Delete</button>
            <button (click)="cancelDelete()" class="btn btn-secondary">Cancel</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .container {
      max-width: 1400px;
      margin: 20px auto;
      padding: 20px;
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Helvetica Neue', sans-serif;
    }
    
    h2 {
      color: #2c3e50;
      margin-bottom: 25px;
      font-size: 28px;
      font-weight: 600;
    }
    
    /* Header action row */
    .list-header-actions {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-bottom: 20px;
    }

    /* Add New Student Button */
    .btn-primary {
      background-color: #27ae60;
      color: white;
      padding: 12px 24px;
      border-radius: 6px;
      text-decoration: none;
      cursor: pointer;
      border: none;
      font-size: 15px;
      font-weight: 500;
      transition: all 0.3s ease;
      display: inline-block;
    }
    
    .btn-primary:hover {
      background-color: #229954;
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(39, 174, 96, 0.3);
    }

    /* Status Messages */
    .loading, .error, .no-data {
      padding: 20px;
      margin: 20px 0;
      border-radius: 8px;
      text-align: center;
      font-size: 16px;
    }
    
    .loading {
      background-color: #e8f4f8;
      color: #0c5460;
      border-left: 4px solid #0c5460;
    }
    
    .error {
      background-color: #f8d7da;
      color: #721c24;
      border-left: 4px solid #721c24;
    }
    
    .no-data {
      background-color: #f5f5f5;
      color: #666;
      border-left: 4px solid #ddd;
    }
    
    /* DataTables Buttons toolbar */
    :host ::ng-deep .dt-buttons {
      display: inline-block;
      margin-bottom: 14px;
    }

    /* Collection trigger button */
    :host ::ng-deep button.dt-btn-collection.dt-button {
      background: linear-gradient(135deg, #16a085 0%, #1abc9c 100%) !important;
      color: white !important;
      border: none !important;
      border-radius: 6px !important;
      padding: 10px 20px !important;
      font-size: 13px !important;
      font-weight: 600 !important;
      cursor: pointer !important;
      box-shadow: 0 2px 6px rgba(22, 160, 133, 0.4) !important;
      transition: all 0.2s ease !important;
      letter-spacing: 0.3px;
    }

    :host ::ng-deep button.dt-btn-collection.dt-button:hover {
      background: linear-gradient(135deg, #117a65 0%, #16a085 100%) !important;
      transform: translateY(-1px) !important;
      box-shadow: 0 4px 12px rgba(22, 160, 133, 0.5) !important;
    }

    /* Dropdown container — rendered in <body> by DataTables, so use a global override */
    :host ::ng-deep .dt-button-collection,
    .dt-button-collection {
      position: absolute !important;
      z-index: 9999 !important;
      background: white !important;
      border: 1px solid #d0e8e3 !important;
      border-radius: 8px !important;
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.14), 0 2px 6px rgba(0, 0, 0, 0.08) !important;
      padding: 6px 0 !important;
      min-width: 230px !important;
      overflow: hidden !important;
    }

    /* Dropdown items — override ALL DataTables default button styles inside collection */
    :host ::ng-deep .dt-button-collection .dt-button,
    .dt-button-collection .dt-button {
      display: block !important;
      width: 100% !important;
      text-align: left !important;
      background: white !important;
      background-image: none !important;
      border: none !important;
      border-bottom: 1px solid #f0f0f0 !important;
      border-radius: 0 !important;
      padding: 11px 18px !important;
      font-size: 13.5px !important;
      font-weight: 500 !important;
      color: #2c3e50 !important;
      cursor: pointer !important;
      transition: background 0.15s ease, color 0.15s ease !important;
      box-shadow: none !important;
      text-shadow: none !important;
    }

    :host ::ng-deep .dt-button-collection .dt-button:last-child,
    .dt-button-collection .dt-button:last-child {
      border-bottom: none !important;
    }

    :host ::ng-deep .dt-button-collection .dt-button:hover,
    .dt-button-collection .dt-button:hover {
      background: #f0faf7 !important;
      color: #16a085 !important;
    }

    /* Leading icon per item */
    :host ::ng-deep .dt-button-collection .dt-button:first-child::before,
    .dt-button-collection .dt-button:first-child::before {
      content: '⬇ ';
      opacity: 0.7;
    }

    :host ::ng-deep .dt-button-collection .dt-button:last-child::before,
    .dt-button-collection .dt-button:last-child::before {
      content: '⬇ ';
      opacity: 0.7;
    }

    /* DataTables collection backdrop — make it subtly dim the page */
    :host ::ng-deep .dt-button-background,
    .dt-button-background {
      background: rgba(0, 0, 0, 0.08) !important;
      z-index: 9998 !important;
    }

    /* DataTables Wrapper and Controls */
    :host ::ng-deep .dataTables_wrapper {
      margin-top: 0;
      padding: 0;
    }
    
    :host ::ng-deep .dataTables_filter {
      margin-bottom: 15px;
      text-align: left;
      font-size: 14px;
    }
    
    :host ::ng-deep .dataTables_filter input {
      border: 2px solid #e0e0e0;
      padding: 10px 12px;
      border-radius: 6px;
      margin-left: 8px;
      font-size: 14px;
      width: 280px;
      transition: all 0.3s ease;
    }
    
    :host ::ng-deep .dataTables_filter input:focus {
      outline: none;
      border-color: #27ae60;
      box-shadow: 0 0 0 3px rgba(39, 174, 96, 0.1);
    }
    
    :host ::ng-deep .dataTables_length {
      margin-bottom: 15px;
      font-size: 14px;
    }
    
    :host ::ng-deep .dataTables_length select {
      border: 2px solid #e0e0e0;
      padding: 8px 10px;
      border-radius: 6px;
      margin: 0 8px;
      font-size: 14px;
      cursor: pointer;
      transition: all 0.3s ease;
    }
    
    :host ::ng-deep .dataTables_length select:focus {
      outline: none;
      border-color: #27ae60;
    }
    
    /* Table Styling */
    .table {
      width: 100%;
      border-collapse: collapse;
      background-color: white;
      border-radius: 8px;
      overflow: hidden;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
      margin: 20px 0;
    }
    
    .table thead {
      background: linear-gradient(135deg, #34495e 0%, #2c3e50 100%);
      color: white;
    }
    
    .table th {
      padding: 16px;
      text-align: left;
      font-weight: 600;
      font-size: 14px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      border-bottom: 2px solid #27ae60;
      cursor: pointer;
      user-select: none;
      white-space: nowrap;
    }
    
    .table th:hover {
      background-color: rgba(255, 255, 255, 0.1);
    }
    
    .table tbody tr {
      border-bottom: 1px solid #ecf0f1;
      transition: all 0.2s ease;
    }
    
    .table tbody tr:nth-child(even) {
      background-color: #f9f9f9;
    }
    
    .table tbody tr:hover {
      background-color: #eef7ff;
      box-shadow: inset 0 0 10px rgba(39, 174, 96, 0.1);
    }
    
    .table td {
      padding: 14px 16px;
      font-size: 14px;
      color: #34495e;
    }
    
    /* Action Cell Styling */
    .action-cell {
      white-space: nowrap;
      display: flex;
      gap: 6px;
      align-items: center;
      justify-content: flex-start;
    }
    
    /* Button Styles */
    .btn {
      padding: 8px 12px;
      border-radius: 5px;
      text-decoration: none;
      cursor: pointer;
      border: none;
      font-size: 12px;
      font-weight: 500;
      transition: all 0.2s ease;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-width: 70px;
      text-align: center;
    }
    
    .btn-sm {
      padding: 6px 10px;
      font-size: 11px;
      min-width: 60px;
    }
    
    .btn-info {
      background-color: #3498db;
      color: white;
    }
    
    .btn-info:hover {
      background-color: #2980b9;
      transform: translateY(-1px);
      box-shadow: 0 2px 6px rgba(52, 152, 219, 0.3);
    }
    
    .btn-warning {
      background-color: #f39c12;
      color: white;
    }
    
    .btn-warning:hover {
      background-color: #d68910;
      transform: translateY(-1px);
      box-shadow: 0 2px 6px rgba(243, 156, 18, 0.3);
    }
    
    .btn-danger {
      background-color: #e74c3c;
      color: white;
    }
    
    .btn-danger:hover {
      background-color: #c0392b;
      transform: translateY(-1px);
      box-shadow: 0 2px 6px rgba(231, 76, 60, 0.3);
    }
    
    .btn-secondary {
      background-color: #95a5a6;
      color: white;
    }
    
    .btn-secondary:hover {
      background-color: #7f8c8d;
      transform: translateY(-1px);
      box-shadow: 0 2px 6px rgba(149, 165, 166, 0.3);
    }
    
    /* Pagination Controls */
    :host ::ng-deep .dataTables_paginate {
      margin-top: 20px;
      text-align: center;
    }
    
    :host ::ng-deep .paginate_button {
      padding: 8px 12px;
      margin: 0 3px;
      border: 1px solid #ddd;
      border-radius: 5px;
      background-color: white;
      cursor: pointer;
      font-size: 13px;
      transition: all 0.2s ease;
      color: #34495e;
      font-weight: 500;
    }
    
    :host ::ng-deep .paginate_button:hover:not(.disabled) {
      background-color: #f0f0f0;
      border-color: #27ae60;
    }
    
    :host ::ng-deep .paginate_button.current {
      background-color: #27ae60;
      color: white;
      border-color: #27ae60;
      font-weight: 600;
    }
    
    :host ::ng-deep .paginate_button.disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
    
    /* Info Text */
    :host ::ng-deep .dataTables_info {
      margin-top: 15px;
      font-size: 13px;
      color: #7f8c8d;
    }
    
    /* Modal Styling */
    .modal-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background-color: rgba(0, 0, 0, 0.6);
      display: flex;
      justify-content: center;
      align-items: center;
      z-index: 1000;
      animation: fadeIn 0.2s ease;
    }
    
    @keyframes fadeIn {
      from {
        opacity: 0;
      }
      to {
        opacity: 1;
      }
    }
    
    .modal {
      background-color: white;
      border-radius: 10px;
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.3);
      max-width: 410px;
      width: 90%;
      margin: 0 auto;
      animation: slideIn 0.3s ease;
    }
    
    @keyframes slideIn {
      from {
        transform: translateY(-50px);
        opacity: 0;
      }
      to {
        transform: translateY(0);
        opacity: 1;
      }
    }
    
    .modal-header {
      padding: 24px 24px 18px;
      border-bottom: 2px solid #ecf0f1;
      background: linear-gradient(135deg, #34495e 0%, #2c3e50 100%);
      border-radius: 10px 10px 0 0;
      color: white;
    }
    
    .modal-header h3 {
      margin: 0;
      font-size: 20px;
      color: white;
      font-weight: 600;
    }
    
    .modal-body {
      padding: 24px;
      font-size: 15px;
    }
    
    .modal-body p {
      margin: 0;
      color: #555;
      line-height: 1.6;
    }
    
    .modal-footer {
      padding: 16px 24px;
      border-top: 1px solid #ecf0f1;
      display: flex;
      justify-content: flex-end;
      gap: 12px;
      border-radius: 0 0 10px 10px;
      background-color: #f9f9f9;
    }
    
    .modal-footer .btn {
      margin: 0;
      padding: 10px 20px;
    }
    
    /* Performance Badge */
    .badge {
      display: inline-block;
      padding: 4px 10px;
      border-radius: 12px;
      font-size: 12px;
      font-weight: 600;
      white-space: nowrap;
    }

    .badge-excellent    { background-color: #d4edda; color: #155724; }
    .badge-good         { background-color: #cce5ff; color: #004085; }
    .badge-satisfactory { background-color: #fff3cd; color: #856404; }
    .badge-needs-support{ background-color: #f8d7da; color: #721c24; }

    .muted {
      color: #aaa;
      font-style: italic;
      font-size: 12px;
    }

    /* DataTables Sort Arrows — use !important to override DataTables' higher-specificity
       opacity rules, and explicitly set color to white so arrows are visible on the
       dark header (linear-gradient #ffffff → #ffffff). */
    :host ::ng-deep table.dataTable thead > tr > th .dt-column-order:before,
    :host ::ng-deep table.dataTable thead > tr > th .dt-column-order:after {
      opacity: 0.55 !important;
      color: white !important;
    }
    :host ::ng-deep table.dataTable thead > tr > th.dt-ordering-asc .dt-column-order:before,
    :host ::ng-deep table.dataTable thead > tr > th.dt-ordering-desc .dt-column-order:after {
      opacity: 1 !important;
      color: white !important;
    }

    /* Responsive Design */
    @media (max-width: 768px) {
      .container {
        padding: 15px;
      }
      
      .table {
        font-size: 13px;
      }
      
      .table th, .table td {
        padding: 12px 8px;
      }
      
      .action-cell {
        flex-wrap: wrap;
        gap: 4px;
      }
      
      .btn-sm {
        padding: 5px 8px;
        font-size: 10px;
        min-width: 50px;
      }
      
      :host ::ng-deep .dataTables_filter input {
        width: 100%;
        margin-top: 8px;
      }
      
      :host ::ng-deep .dataTables_filter {
        text-align: left;
      }
    }
    
    @media (max-width: 480px) {
      h2 {
        font-size: 22px;
        margin-bottom: 15px;
      }
      
      .table {
        font-size: 12px;
      }
      
      .table th, .table td {
        padding: 10px 6px;
      }
      
      .btn-sm {
        padding: 4px 6px;
        font-size: 9px;
        min-width: 45px;
      }
      
      :host ::ng-deep .paginate_button {
        padding: 6px 9px;
        font-size: 11px;
      }
    }
  `]
})
export class StudentListComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('studentsTable') table!: ElementRef;
  
  // Reactive state from state service
  students: StudentListDto[] = [];
  loading = false;
  error: string | null = null;
  
  // Component state (not shared)
  showConfirmDialog = false;
  studentToDelete: number | null = null;
  
  private dataTable: any = null;
  private destroy$ = new Subject<void>();

  constructor(
    private studentBusiness: StudentBusinessService,
    private studentState: StudentStateService,
    private router: Router,
    private reportApi: ReportApiService,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) { }

  ngOnInit(): void {
    // Subscribe to reactive state
    this.studentState.students$
      .pipe(takeUntil(this.destroy$))
      .subscribe(students => {
        this.students = students;
        this.cdr.markForCheck();
        
        // Reinitialize DataTable when data changes
        setTimeout(() => {
          this.initializeDataTable();
        }, 100);
      });
    
    this.studentState.loading$
      .pipe(takeUntil(this.destroy$))
      .subscribe(loading => {
        this.loading = loading;
        this.cdr.markForCheck();
      });
    
    this.studentState.error$
      .pipe(takeUntil(this.destroy$))
      .subscribe(error => {
        this.error = error;
        this.cdr.markForCheck();
      });
    
    // Load students on init
    this.loadStudents();
    
    // Also reload students when navigating back to this route
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        filter((event: any) => event.url === '/'),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.loadStudents();
      });
  }

  ngAfterViewInit(): void {
    // Initialize DataTable after view is initialized
    this.initializeDataTable();
  }

  ngOnDestroy(): void {
    if (this.table) {
      this.table.nativeElement.removeEventListener('click', this.onTableClick);
    }
    if (this.dataTable) {
      this.dataTable.destroy();
    }
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeDataTable(): void {
    if (this.table && this.students.length > 0) {
      if (this.dataTable) {
        this.dataTable.destroy();
      }

      this.dataTable = new DataTable(this.table.nativeElement, {
        pagingType: 'full_numbers',
        pageLength: 10,
        processing: true,
        dom: 'Blfrtip',
        buttons: [
          {
            extend: 'collection',
            text: '&#x2193; Export',
            className: 'dt-btn-collection',
            buttons: [
              {
                extend: 'csv',
                text: 'Export current view (CSV)',
                filename: 'students-current-view',
                className: 'dt-btn-csv-item',
                exportOptions: {
                  columns: [0, 1, 2, 3, 4, 5] // exclude hidden % col (6) and Actions col (7)
                }
              },
              {
                text: 'Export all students (CSV)',
                className: 'dt-btn-csv-item',
                action: () => { this.exportAllCsv(); }
              }
            ]
          }
        ],
        language: {
          search: 'Search records:',
          lengthMenu: 'Display _MENU_ records per page',
          info: 'Showing _START_ to _END_ of _TOTAL_ records',
          infoEmpty: 'No records available',
          zeroRecords: 'No matching records found'
        },
        columnDefs: [
          {
            targets: 6, // hidden percentage column
            visible: false,
            searchable: false
          },
          {
            targets: 5, // Performance column — sort by hidden percentage col
            orderData: [6]
          },
          {
            targets: 7, // Actions column
            orderable: false,
            searchable: false
          }
        ],
        drawCallback: () => {
          // Re-attach delegated click listener after every DataTables draw
          // (sort, search, page) so action buttons always work
          this.attachActionListeners();
        }
      });

      // Attach listeners for the initial render
      this.attachActionListeners();
    }
  }

  private attachActionListeners(): void {
    if (!this.table) return;
    const tableEl: HTMLElement = this.table.nativeElement;

    // Remove previous listener to avoid duplicates, then re-add
    tableEl.removeEventListener('click', this.onTableClick);
    tableEl.addEventListener('click', this.onTableClick);
  }

  // Arrow function so `this` is always the component instance
  private onTableClick = (event: Event): void => {
    const btn = (event.target as HTMLElement).closest('[data-action]') as HTMLElement | null;
    if (!btn) return;

    const action = btn.getAttribute('data-action');
    const id = parseInt(btn.getAttribute('data-id') || '0', 10);
    if (!id) return;

    // Run inside NgZone so Angular's change detection picks up navigation
    this.ngZone.run(() => {
      if (action === 'view') this.viewStudent(id);
      else if (action === 'edit') this.editStudent(id);
      else if (action === 'delete') this.showDeleteConfirm(id);
    });
  };

  /**
   * Load students using business service
   * State is automatically updated via reactive streams
   */
  loadStudents(): void {
    this.studentBusiness.loadStudents().subscribe();
  }

  viewStudent(id: number): void {
    this.router.navigate(['/detail', id]);
  }

  editStudent(id: number): void {
    this.router.navigate(['/edit', id]);
  }

  showDeleteConfirm(id: number): void {
    this.studentToDelete = id;
    this.showConfirmDialog = true;
    this.cdr.detectChanges(); // Force Angular to render the modal immediately
  }

  /**
   * Confirm and execute delete operation
   * Uses business service for delete logic
   */
  confirmDelete(): void {
    if (this.studentToDelete !== null) {
      this.studentBusiness.deleteStudent(this.studentToDelete).subscribe({
        next: () => {
          this.showConfirmDialog = false;
          this.studentToDelete = null;
          this.cdr.detectChanges(); // Ensure modal is dismissed
        },
        error: () => {
          // Error already handled by business service and state
          this.showConfirmDialog = false;
          this.studentToDelete = null;
          this.cdr.detectChanges(); // Ensure modal is dismissed on error too
        }
      });
    }
  }

  cancelDelete(): void {
    this.showConfirmDialog = false;
    this.studentToDelete = null;
    this.cdr.detectChanges(); // Ensure modal is dismissed
  }

  exportAllCsv(): void {
    this.reportApi.exportAllStudentsCsv().subscribe(blob => {
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'all-students-report.csv';
      a.click();
      URL.revokeObjectURL(url);
    });
  }

  getPerformanceClass(level: string): { [key: string]: boolean } {
    return {
      'badge-excellent': level === 'Excellent',
      'badge-good': level === 'Good',
      'badge-satisfactory': level === 'Satisfactory',
      'badge-needs-support': level === 'Needs Support'
    };
  }
}
