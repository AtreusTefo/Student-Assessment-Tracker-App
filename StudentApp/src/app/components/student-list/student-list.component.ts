import { Component, OnInit, OnDestroy, ChangeDetectorRef, ViewChild, ElementRef, AfterViewInit, NgZone } from '@angular/core';
import { Router, NavigationEnd, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { StudentListDto } from '../../core/models';
import { StudentStateService } from '../../core/services/state';
import { StudentBusinessService } from '../../features/students/services/student-business.service';
import { ReportApiService } from '../../core/services/http';
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
  templateUrl: './student-list.component.html',

  styleUrls: ['./student-list.component.css']
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
