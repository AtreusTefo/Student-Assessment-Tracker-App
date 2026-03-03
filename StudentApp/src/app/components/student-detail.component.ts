import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { StudentDetailDto } from '../core/models';
import { StudentStateService } from '../core/services/state';
import { StudentBusinessService } from '../features/students/services/student-business.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

/**
 * PRESENTATION LAYER - Student Detail Component
 * Responsible ONLY for UI presentation
 * Delegates all business logic to StudentBusinessService
 * Subscribes to StudentStateService for reactive data
 */

@Component({
  selector: 'app-student-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="container">
      <h2>Student Details</h2>
      
      <div *ngIf="loading" class="loading">Loading student...</div>
      
      <div *ngIf="error" class="error">{{ error }}</div>
      
      <div *ngIf="student && !loading" class="detail-view">
        <h3>{{ student.firstName }} {{ student.lastName }}</h3>
        
        <div class="section">
          <h4>Personal Information</h4>
          <div class="detail-row">
            <label>Email:</label>
            <span>{{ student.email }}</span>
          </div>
          <div class="detail-row">
            <label>Phone:</label>
            <span>{{ student.phone }}</span>
          </div>
          <div class="detail-row">
            <label>Grade:</label>
            <span>{{ student.grade }}</span>
          </div>
          <div class="detail-row">
            <label>Created Date:</label>
            <span>{{ student.createdAt | date: 'MM/dd/yyyy' }}</span>
          </div>
        </div>
        
        <div class="section">
          <h4>Assessment Scores (Out of 20)</h4>
          <div class="detail-row">
            <label>Assessment 1:</label>
            <span>{{ student.assessment1 }}/20</span>
          </div>
          <div class="detail-row">
            <label>Assessment 2:</label>
            <span>{{ student.assessment2 }}/20</span>
          </div>
          <div class="detail-row">
            <label>Assessment 3:</label>
            <span>{{ student.assessment3 }}/20</span>
          </div>
        </div>
        
        <div class="section">
          <h4>Performance Summary</h4>
          <div class="detail-row">
            <label>Total Score:</label>
            <span>{{ student.totalScore }}/60</span>
          </div>
          <div class="detail-row">
            <label>Average:</label>
            <span>{{ student.averageScore | number: '1.2-2' }}/20</span>
          </div>
          <div class="detail-row">
            <label>Percentage:</label>
            <span>{{ student.percentage | number: '1.2-2' }}%</span>
          </div>
          <div class="detail-row">
            <label>Performance Level:</label>
            <span [ngClass]="'performance-' + (student.performanceLevel ? student.performanceLevel.toLowerCase().replace(' ', '-') : '')">
              {{ student.performanceLevel }}
            </span>
          </div>
        </div>
        
        <div class="actions">
          <a [routerLink]="['/edit', student.id]" class="btn btn-warning">Edit</a>
          <a routerLink="/" class="btn btn-secondary">Back to List</a>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .container {
      max-width: 600px;
      margin: 20px auto;
      padding: 20px;
    }
    
    .detail-view {
      background: #f9f9f9;
      padding: 20px;
      border-radius: 8px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }
    
    .section {
      margin: 20px 0;
      padding: 15px;
      background: white;
      border-radius: 4px;
    }
    
    .section h4 {
      margin-top: 0;
      border-bottom: 2px solid #2196F3;
      padding-bottom: 10px;
    }
    
    .detail-row {
      display: flex;
      justify-content: space-between;
      padding: 10px 0;
      border-bottom: 1px solid #eee;
    }
    
    .detail-row:last-child {
      border-bottom: none;
    }
    
    .detail-row label {
      font-weight: bold;
      min-width: 150px;
    }
    
    .performance-excellent {
      background-color: #4caf50;
      color: white;
      padding: 5px 10px;
      border-radius: 4px;
    }
    
    .performance-good {
      background-color: #2196f3;
      color: white;
      padding: 5px 10px;
      border-radius: 4px;
    }
    
    .performance-satisfactory {
      background-color: #ff9800;
      color: white;
      padding: 5px 10px;
      border-radius: 4px;
    }
    
    .performance-needs-support {
      background-color: #f44336;
      color: white;
      padding: 5px 10px;
      border-radius: 4px;
    }
    
    .actions {
      margin-top: 20px;
      text-align: center;
    }
    
    .btn {
      display: inline-block;
      padding: 10px 20px;
      margin: 0 10px;
      border-radius: 4px;
      text-decoration: none;
      cursor: pointer;
      border: none;
    }
    
    .btn-warning {
      background-color: #ff9800;
      color: white;
    }
    
    .btn-secondary {
      background-color: #757575;
      color: white;
    }
    
    .loading, .error {
      padding: 20px;
      margin-top: 20px;
      border-radius: 4px;
      text-align: center;
    }
    
    .loading {
      background-color: #e3f2fd;
      color: #1976d2;
    }
    
    .error {
      background-color: #ffebee;
      color: #c62828;
    }
  `]
})
export class StudentDetailComponent implements OnInit, OnDestroy {
  // Reactive state dari StateService
  student: StudentDetailDto | null = null;
  loading = false;
  error: string | null = null;
  
  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private studentBusiness: StudentBusinessService,
    private studentState: StudentStateService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    // Subscribe to reactive state
    this.studentState.selectedStudent$
      .pipe(takeUntil(this.destroy$))
      .subscribe(student => {
        this.student = student;
        this.cdr.markForCheck();
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
    
    // Load student by ID from route params
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadStudent(parseInt(id));
    } else {
      this.studentState.setError('No student ID provided');
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.studentBusiness.clearSelectedStudent();
  }

  /**
   * Load student using business service
   * State is automatically updated via reactive streams
   */
  loadStudent(id: number): void {
    this.studentBusiness.loadStudentById(id).subscribe();
  }
}
