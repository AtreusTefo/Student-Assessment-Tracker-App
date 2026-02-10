import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Router, NavigationEnd, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { StudentService, StudentListDto } from '../services/student.service';
import { Subject } from 'rxjs';
import { takeUntil, filter } from 'rxjs/operators';

@Component({
  selector: 'app-student-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="container">
      <h2>Student List</h2>
      <a routerLink="/create" class="btn btn-primary">Add New Student</a>
      
      <div *ngIf="loading" class="loading">Loading students...</div>
      
      <div *ngIf="error" class="error">{{ error }}</div>
      
      <table *ngIf="students.length > 0 && !loading" class="table">
        <thead>
          <tr>
            <th>Student ID</th>
            <th>First Name</th>
            <th>Last Name</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let student of students">
            <td>{{ student.studentId }}</td>
            <td>{{ student.firstName }}</td>
            <td>{{ student.lastName }}</td>
            <td>
              <a [routerLink]="['/detail', student.studentId]" class="btn btn-info">View</a>
              <a [routerLink]="['/edit', student.studentId]" class="btn btn-warning">Edit</a>
              <button (click)="showDeleteConfirm(student.studentId)" class="btn btn-danger">Delete</button>
            </td>
          </tr>
        </tbody>
      </table>
      
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
      max-width: 1000px;
      margin: 20px auto;
      padding: 20px;
    }
    
    .btn {
      padding: 8px 15px;
      margin: 5px;
      border-radius: 4px;
      text-decoration: none;
      cursor: pointer;
      border: none;
    }
    
    .btn-primary {
      background-color: #4CAF50;
      color: white;
    }
    
    .btn-info {
      background-color: #2196F3;
      color: white;
    }
    
    .btn-warning {
      background-color: #ff9800;
      color: white;
    }
    
    .btn-danger {
      background-color: #f44336;
      color: white;
    }
    
    .btn-secondary {
      background-color: #757575;
      color: white;
    }
    
    .table {
      width: 100%;
      border-collapse: collapse;
      margin-top: 20px;
    }
    
    .table th, .table td {
      padding: 12px;
      text-align: left;
      border-bottom: 1px solid #ddd;
    }
    
    .table th {
      background-color: #f2f2f2;
      font-weight: bold;
    }
    
    .loading, .error, .no-data {
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
    
    .no-data {
      background-color: #f5f5f5;
      color: #666;
    }

    .modal-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background-color: rgba(0, 0, 0, 0.5);
      display: flex;
      justify-content: center;
      align-items: center;
      z-index: 1000;
    }

    .modal {
      background-color: white;
      border-radius: 8px;
      box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
      max-width: 400px;
      width: 90%;
      margin: 0 auto;
    }

    .modal-header {
      padding: 20px;
      border-bottom: 1px solid #e0e0e0;
      background-color: #f5f5f5;
      border-radius: 8px 8px 0 0;
    }

    .modal-header h3 {
      margin: 0;
      font-size: 18px;
      color: #333;
    }

    .modal-body {
      padding: 20px;
    }

    .modal-body p {
      margin: 0;
      color: #666;
      line-height: 1.5;
    }

    .modal-footer {
      padding: 15px 20px;
      border-top: 1px solid #e0e0e0;
      display: flex;
      justify-content: flex-end;
      gap: 10px;
      border-radius: 0 0 8px 8px;
    }

    .modal-footer .btn {
      margin: 0;
      padding: 10px 20px;
    }
  `]
})
export class StudentListComponent implements OnInit, OnDestroy {
  students: StudentListDto[] = [];
  loading = true;
  error: string | null = null;
  showConfirmDialog = false;
  studentToDelete: number | null = null;
  private destroy$ = new Subject<void>();

  constructor(
    private studentService: StudentService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    // Load students when component initializes
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

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadStudents(): void {
    this.loading = true;
    this.error = null;
    
    this.studentService.getStudents().subscribe({
      next: (data) => {
        this.students = data;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.error = 'Failed to load students: ' + (err.error?.title || err.message || 'Unknown error');
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  showDeleteConfirm(id: number): void {
    this.studentToDelete = id;
    this.showConfirmDialog = true;
  }

  confirmDelete(): void {
    if (this.studentToDelete !== null) {
      this.studentService.deleteStudent(this.studentToDelete).subscribe({
        next: () => {
          this.showConfirmDialog = false;
          this.studentToDelete = null;
          this.loadStudents();
        },
        error: (err) => {
          this.error = 'Failed to delete student: ' + (err.message || 'Unknown error');
          this.showConfirmDialog = false;
          this.studentToDelete = null;
        }
      });
    }
  }

  cancelDelete(): void {
    this.showConfirmDialog = false;
    this.studentToDelete = null;
  }
}
