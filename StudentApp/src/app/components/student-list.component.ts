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
              <button (click)="deleteStudent(student.studentId)" class="btn btn-danger">Delete</button>
            </td>
          </tr>
        </tbody>
      </table>
      
      <div *ngIf="students.length === 0 && !loading && !error" class="no-data">
        No students found. <a routerLink="/create">Create one</a>
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
  `]
})
export class StudentListComponent implements OnInit, OnDestroy {
  students: StudentListDto[] = [];
  loading = true;
  error: string | null = null;
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

  deleteStudent(id: number): void {
    if (confirm('Are you sure you want to delete this student?')) {
      this.studentService.deleteStudent(id).subscribe({
        next: () => {
          this.loadStudents();
        },
        error: (err) => {
          this.error = 'Failed to delete student: ' + (err.message || 'Unknown error');
        }
      });
    }
  }
}
