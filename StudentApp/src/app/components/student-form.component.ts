import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StudentService, Student } from '../services/student.service';

@Component({
  selector: 'app-student-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="container">
      <h2>{{ isEdit ? 'Edit Student' : 'Create Student' }}</h2>
      
      <div *ngIf="error" class="error">{{ error }}</div>
      
      <form (ngSubmit)="onSubmit()" #form="ngForm" class="form">
        <div class="form-group">
          <label for="firstName">First Name:</label>
          <input type="text" id="firstName" [(ngModel)]="student.firstName" name="firstName" required />
          <span class="error" *ngIf="form.submitted && !student.firstName">First name is required</span>
          <span class="error" *ngIf="form.submitted && student.firstName && !isValidName(student.firstName)">Enter valid First Name</span>
        </div>
        
        <div class="form-group">
          <label for="lastName">Last Name:</label>
          <input type="text" id="lastName" [(ngModel)]="student.lastName" name="lastName" required />
          <span class="error" *ngIf="form.submitted && !student.lastName">Last name is required</span>
          <span class="error" *ngIf="form.submitted && student.lastName && !isValidName(student.lastName)">Enter valid Last Name</span>
        </div>
        
        <div class="form-group">
          <label for="email">Email:</label>
          <input type="email" id="email" [(ngModel)]="student.email" name="email" required />
          <span class="error" *ngIf="form.submitted && !student.email">Email is required</span>
          <span class="error" *ngIf="form.submitted && student.email && !isValidEmail(student.email)">Enter valid email</span>
        </div>
        
        <div class="form-group">
          <label for="phone">Phone (8 digits, e.g., 72254856):</label>
          <input type="text" id="phone" [(ngModel)]="student.phone" name="phone" placeholder="72254856" maxlength="8" (input)="validatePhone()" (keypress)="allowOnlyNumbers($event)" required />
          <span class="error" *ngIf="form.submitted && !student.phone">Phone is required</span>
          <span class="error" *ngIf="form.submitted && student.phone && student.phone.length < 8">Phone must be exactly 8 digits</span>
        </div>
        
        <div class="form-group">
          <label for="grade">Grade:</label>
          <input type="text" id="grade" [(ngModel)]="student.grade" name="grade" placeholder="e.g., 10A, 11B" required />
          <span class="error" *ngIf="form.submitted && !student.grade">Grade is required</span>
        </div>
        
        <div class="form-group">
          <label for="assessment1">Assessment 1 (0-20):</label>
          <input type="number" id="assessment1" [(ngModel)]="student.assessment1" name="assessment1" min="0" max="20" required />
          <span class="error" *ngIf="form.submitted && (student.assessment1 === null || student.assessment1 === undefined)">Assessment 1 is required</span>
          <span class="error" *ngIf="form.submitted && student.assessment1 !== null && student.assessment1 !== undefined && (student.assessment1 < 0 || student.assessment1 > 20)">Must be between 0 and 20</span>
        </div>
        
        <div class="form-group">
          <label for="assessment2">Assessment 2 (0-20):</label>
          <input type="number" id="assessment2" [(ngModel)]="student.assessment2" name="assessment2" min="0" max="20" required />
          <span class="error" *ngIf="form.submitted && (student.assessment2 === null || student.assessment2 === undefined)">Assessment 2 is required</span>
          <span class="error" *ngIf="form.submitted && student.assessment2 !== null && student.assessment2 !== undefined && (student.assessment2 < 0 || student.assessment2 > 20)">Must be between 0 and 20</span>
        </div>
        
        <div class="form-group">
          <label for="assessment3">Assessment 3 (0-20):</label>
          <input type="number" id="assessment3" [(ngModel)]="student.assessment3" name="assessment3" min="0" max="20" required />
          <span class="error" *ngIf="form.submitted && (student.assessment3 === null || student.assessment3 === undefined)">Assessment 3 is required</span>
          <span class="error" *ngIf="form.submitted && student.assessment3 !== null && student.assessment3 !== undefined && (student.assessment3 < 0 || student.assessment3 > 20)">Must be between 0 and 20</span>
        </div>
        
        <div class="actions">
          <button type="submit" class="btn btn-primary" [disabled]="loading">
            {{ loading ? 'Saving...' : (isEdit ? 'Update' : 'Create') }}
          </button>
          <a routerLink="/" class="btn btn-secondary">Cancel</a>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .container {
      max-width: 500px;
      margin: 20px auto;
      padding: 20px;
    }
    
    .form {
      background: #f9f9f9;
      padding: 20px;
      border-radius: 8px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }
    
    .form-group {
      margin-bottom: 15px;
    }
    
    .form-group label {
      display: block;
      margin-bottom: 5px;
      font-weight: bold;
      color: #333;
    }
    
    .form-group input {
      width: 100%;
      padding: 10px;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 14px;
      box-sizing: border-box;
    }
    
    .form-group input:focus {
      outline: none;
      border-color: #2196F3;
      box-shadow: 0 0 5px rgba(33, 150, 243, 0.3);
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
      font-size: 14px;
    }
    
    .btn-primary {
      background-color: #4CAF50;
      color: white;
    }
    
    .btn-primary:disabled {
      background-color: #ccc;
      cursor: not-allowed;
    }
    
    .btn-secondary {
      background-color: #757575;
      color: white;
    }
    
    .error {
      color: #f44336;
      font-size: 12px;
      margin-top: 5px;
      display: block;
    }
  `]
})
export class StudentFormComponent implements OnInit {
  student: Student = {
    studentId: 0,
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    grade: '',
    enrollmentDate: new Date().toISOString(),
    assessment1: null as any,
    assessment2: null as any,
    assessment3: null as any,
    createdDate: new Date().toISOString()
  };
  
  isEdit = false;
  loading = false;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private studentService: StudentService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.loadStudent(parseInt(id));
    }
  }

  loadStudent(id: number): void {
    this.loading = true;
    this.studentService.getStudent(id).subscribe({
      next: (data) => {
        this.student = {
          studentId: data.studentId,
          firstName: data.firstName,
          lastName: data.lastName,
          email: data.email,
          phone: data.phone.substring(5), // Strip "+267 " for editing
          grade: data.grade,
          enrollmentDate: data.enrollmentDate,
          assessment1: data.assessment1,
          assessment2: data.assessment2,
          assessment3: data.assessment3,
          createdDate: new Date().toISOString()
        };
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.error = 'Failed to load student: ' + err.message;
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  validatePhone(): void {
    if (this.student.phone) {
      // Strip any non-numeric characters (just in case)
      this.student.phone = this.student.phone.replace(/[^0-9]/g, '');
      // Limit to 8 characters
      if (this.student.phone.length > 8) {
        this.student.phone = this.student.phone.substring(0, 8);
      }
    }
  }

  allowOnlyNumbers(event: KeyboardEvent): void {
    const char = String.fromCharCode(event.which);
    if (!/[0-9]/.test(char)) {
      event.preventDefault();
    }
  }

  isValidName(name: string): boolean {
    // Only allow letters, spaces, and hyphens
    const nameRegex = /^[a-zA-Z\s\-']+$/;
    return nameRegex.test(name);
  }

  isValidEmail(email: string): boolean {
    // Simple email regex validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  onSubmit(): void {
    this.error = null; // Clear previous errors
    
    // Validate firstName format
    if (this.student.firstName && !this.isValidName(this.student.firstName)) {
      this.error = 'Please enter a valid First Name (letters only)';
      return;
    }
    
    // Validate lastName format
    if (this.student.lastName && !this.isValidName(this.student.lastName)) {
      this.error = 'Please enter a valid Last Name (letters only)';
      return;
    }

    // Validate email format
    if (this.student.email && !this.isValidEmail(this.student.email)) {
      this.error = 'Please enter a valid email address';
      return;
    }
    
    // Phone validation is handled in the template - don't set error here
    // Just prevent submission if validation fails
    if (!this.student.phone || this.student.phone.length !== 8) {
      return; // Template will show the appropriate error
    }

    // Assessment validation is handled in the template - don't set error here
    // Just prevent submission if validation fails
    if (this.student.assessment1 === null || this.student.assessment1 === undefined ||
        this.student.assessment1 < 0 || this.student.assessment1 > 20 ||
        this.student.assessment2 === null || this.student.assessment2 === undefined ||
        this.student.assessment2 < 0 || this.student.assessment2 > 20 ||
        this.student.assessment3 === null || this.student.assessment3 === undefined ||
        this.student.assessment3 < 0 || this.student.assessment3 > 20) {
      return; // Template will show the appropriate error
    }

    this.loading = true;
    this.error = null;

    if (this.isEdit) {
      this.studentService.updateStudent(this.student.studentId, this.student).subscribe({
        next: () => {
          this.loading = false;
          this.router.navigate(['/detail', this.student.studentId]);
        },
        error: (err) => {
          this.loading = false;
          if (err.error && err.error.errors) {
            const errorMessages = Object.values(err.error.errors)
              .flat()
              .join('\n');
            this.error = errorMessages as string;
          } else {
            this.error = 'Failed to update student: ' + (err.error?.title || err.message);
          }
        }
      });
    } else {
      this.studentService.createStudent(this.student).subscribe({
        next: (response) => {
          this.loading = false;
          this.router.navigate(['/']);
        },
        error: (err) => {
          this.loading = false;
          if (err.error && err.error.errors) {
            const errorMessages = Object.values(err.error.errors)
              .flat()
              .join('\n');
            this.error = errorMessages as string;
          } else {
            this.error = 'Failed to create student: ' + (err.error?.title || err.message);
          }
        }
      });
    }
  }
}
