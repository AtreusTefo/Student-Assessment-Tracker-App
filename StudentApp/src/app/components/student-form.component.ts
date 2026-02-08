import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { StudentService, Student } from '../services/student.service';

@Component({
  selector: 'app-student-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="container">
      <h2>{{ isEdit ? 'Edit Student' : 'Create Student' }}</h2>
      
      <div *ngIf="error && isServerError" class="server-error">{{ error }}</div>
      
      <form (ngSubmit)="onSubmit(form)" #form="ngForm" class="form">
        <div class="form-group">
          <label for="firstName">First Name:</label>
          <input type="text" id="firstName" [(ngModel)]="student.firstName" name="firstName" #firstName="ngModel" required minlength="2" maxlength="50" />
          <span class="error" *ngIf="(form.submitted || firstName.touched || firstName.dirty) && firstName.hasError('required')">First name is required</span>
          <span class="error" *ngIf="(form.submitted || firstName.touched || firstName.dirty) && firstName.hasError('minlength')">First name must be at least 2 characters</span>
          <span class="error" *ngIf="(form.submitted || firstName.touched || firstName.dirty) && firstName.hasError('maxlength')">First name cannot exceed 50 characters</span>
          <span class="error" *ngIf="fieldErrors['firstName'] && fieldErrors['firstName'].length">{{ fieldErrors['firstName'].join('\n') }}</span>
        </div>
        
        <div class="form-group">
          <label for="lastName">Last Name:</label>
          <input type="text" id="lastName" [(ngModel)]="student.lastName" name="lastName" #lastName="ngModel" required minlength="2" maxlength="50" />
          <span class="error" *ngIf="(form.submitted || lastName.touched || lastName.dirty) && lastName.hasError('required')">Last name is required</span>
          <span class="error" *ngIf="(form.submitted || lastName.touched || lastName.dirty) && lastName.hasError('minlength')">Last name must be at least 2 characters</span>
          <span class="error" *ngIf="(form.submitted || lastName.touched || lastName.dirty) && lastName.hasError('maxlength')">Last name cannot exceed 50 characters</span>
          <span class="error" *ngIf="fieldErrors['lastName'] && fieldErrors['lastName'].length">{{ fieldErrors['lastName'].join('\n') }}</span>
        </div>
        
        <div class="form-group">
          <label for="email">Email:</label>
          <input type="email" id="email" [(ngModel)]="student.email" name="email" #email="ngModel" required maxlength="100" />
          <span class="error" *ngIf="(form.submitted || email.touched || email.dirty) && email.hasError('required')">Email is required</span>
          <span class="error" *ngIf="(form.submitted || email.touched || email.dirty) && email.hasError('email')">Email must be a valid email address</span>
          <span class="error" *ngIf="(form.submitted || email.touched || email.dirty) && email.hasError('maxlength')">Email cannot exceed 100 characters</span>
          <span class="error" *ngIf="fieldErrors['email'] && fieldErrors['email'].length">{{ fieldErrors['email'].join('\n') }}</span>
        </div>
        
        <div class="form-group">
          <label for="phone">Phone (8 digits, e.g., 72254856):</label>
          <input type="text" id="phone" [(ngModel)]="student.phone" name="phone" #phone="ngModel" placeholder="72254856" minlength="8" maxlength="8" pattern="^\\d{8}$" (input)="validatePhone()" (keypress)="allowOnlyNumbers($event)" required />
          <span class="error" *ngIf="(form.submitted || phone.touched || phone.dirty) && phone.hasError('required')">Phone is required</span>
          <span class="error" *ngIf="(form.submitted || phone.touched || phone.dirty) && (phone.hasError('pattern') || phone.hasError('minlength') || phone.hasError('maxlength'))">Phone must be exactly 8 digits</span>
          <span class="error" *ngIf="fieldErrors['phone'] && fieldErrors['phone'].length">{{ fieldErrors['phone'].join('\n') }}</span>
        </div>
        
        <div class="form-group">
          <label for="grade">Grade:</label>
          <input type="text" id="grade" [(ngModel)]="student.grade" name="grade" #grade="ngModel" placeholder="e.g., 10A, 11B" required maxlength="10" />
          <span class="error" *ngIf="(form.submitted || grade.touched || grade.dirty) && grade.hasError('required')">Grade is required</span>
          <span class="error" *ngIf="(form.submitted || grade.touched || grade.dirty) && grade.hasError('maxlength')">Grade cannot exceed 10 characters</span>
          <span class="error" *ngIf="fieldErrors['grade'] && fieldErrors['grade'].length">{{ fieldErrors['grade'].join('\n') }}</span>
        </div>
        
        <div class="form-group">
          <label for="assessment1">Assessment 1 (0-20):</label>
          <input type="number" id="assessment1" [(ngModel)]="student.assessment1" name="assessment1" #assessment1="ngModel" min="0" max="20" required />
          <span class="error" *ngIf="(form.submitted || assessment1.touched || assessment1.dirty) && assessment1.hasError('required')">Assessment 1 is required</span>
          <span class="error" *ngIf="(form.submitted || assessment1.touched || assessment1.dirty) && (assessment1.hasError('min') || assessment1.hasError('max'))">Assessment 1 must be between 0 and 20</span>
          <span class="error" *ngIf="fieldErrors['assessment1'] && fieldErrors['assessment1'].length">{{ fieldErrors['assessment1'].join('\n') }}</span>
        </div>
        
        <div class="form-group">
          <label for="assessment2">Assessment 2 (0-20):</label>
          <input type="number" id="assessment2" [(ngModel)]="student.assessment2" name="assessment2" #assessment2="ngModel" min="0" max="20" required />
          <span class="error" *ngIf="(form.submitted || assessment2.touched || assessment2.dirty) && assessment2.hasError('required')">Assessment 2 is required</span>
          <span class="error" *ngIf="(form.submitted || assessment2.touched || assessment2.dirty) && (assessment2.hasError('min') || assessment2.hasError('max'))">Assessment 2 must be between 0 and 20</span>
          <span class="error" *ngIf="fieldErrors['assessment2'] && fieldErrors['assessment2'].length">{{ fieldErrors['assessment2'].join('\n') }}</span>
        </div>
        
        <div class="form-group">
          <label for="assessment3">Assessment 3 (0-20):</label>
          <input type="number" id="assessment3" [(ngModel)]="student.assessment3" name="assessment3" #assessment3="ngModel" min="0" max="20" required />
          <span class="error" *ngIf="(form.submitted || assessment3.touched || assessment3.dirty) && assessment3.hasError('required')">Assessment 3 is required</span>
          <span class="error" *ngIf="(form.submitted || assessment3.touched || assessment3.dirty) && (assessment3.hasError('min') || assessment3.hasError('max'))">Assessment 3 must be between 0 and 20</span>
          <span class="error" *ngIf="fieldErrors['assessment3'] && fieldErrors['assessment3'].length">{{ fieldErrors['assessment3'].join('\n') }}</span>
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
      line-height: 1.6;
      white-space: pre-wrap;
      word-wrap: break-word;
    }

    .server-error {
      background-color: #ffebee;
      border: 1px solid #f44336;
      border-radius: 4px;
      color: #c62828;
      padding: 12px;
      margin-bottom: 20px;
      font-size: 13px;
      line-height: 1.6;
      white-space: pre-wrap;
      word-wrap: break-word;
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
  isServerError = false;
  fieldErrors: Record<string, string[]> = {};

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

  private isValidationErrorResponse(err: any): boolean {
    // Check if this is a validation error response from the server
    // ProblemDetails format will have status 400 and an errors object with field names as keys
    if (err?.status !== 400) {
      return false;
    }
    
    const errors = err?.error?.errors;
    if (!errors || typeof errors !== 'object') {
      return false;
    }
    
    // Check if errors object has at least one field-level error
    // (field validation errors have property names as keys with array values)
    return Object.keys(errors).length > 0;
  }

  private normalizeValidationErrors(errors: Record<string, string[]>): Record<string, string[]> {
    const normalized: Record<string, string[]> = {};

    Object.entries(errors).forEach(([key, messages]) => {
      const lastSegment = key.split('.').pop() ?? key;
      const normalizedKey = lastSegment.charAt(0).toLowerCase() + lastSegment.slice(1);
      normalized[normalizedKey] = messages;
    });

    return normalized;
  }

  private handleServerError(action: 'create' | 'update', err: any): void {
    this.loading = false;

    if (this.isValidationErrorResponse(err)) {
      // Surface FluentValidation errors inline by field.
      this.fieldErrors = this.normalizeValidationErrors(err.error.errors);
      this.isServerError = false;
      this.error = null;
      this.cdr.markForCheck();
      return;
    }

    this.isServerError = true;
    if (err.error && err.error.errors) {
      const errorMessages = Object.values(err.error.errors)
        .flat()
        .join('\n');
      this.error = errorMessages as string;
    } else {
      this.error = `Failed to ${action} student: ` + (err.error?.title || err.message);
    }
    this.cdr.markForCheck();
  }


  private parseAssessment(value: unknown): number | null {
    if (value === null || value === undefined || value === '') {
      return null;
    }

    const parsed = typeof value === 'number' ? value : Number(value);
    return Number.isFinite(parsed) ? parsed : null;
  }

  onSubmit(form: NgForm): void {
    this.error = null;
    this.isServerError = false;
    this.fieldErrors = {};

    const assessment1 = this.parseAssessment(this.student.assessment1);
    const assessment2 = this.parseAssessment(this.student.assessment2);
    const assessment3 = this.parseAssessment(this.student.assessment3);

    const assessmentsValid =
      assessment1 !== null &&
      assessment2 !== null &&
      assessment3 !== null &&
      assessment1 >= 0 && assessment1 <= 20 &&
      assessment2 >= 0 && assessment2 <= 20 &&
      assessment3 >= 0 && assessment3 <= 20;

    if (form.invalid || !assessmentsValid) {
      return;
    }

    this.loading = true;

    const payload: Student = {
      ...this.student,
      assessment1,
      assessment2,
      assessment3
    };

    if (this.isEdit) {
      this.studentService.updateStudent(this.student.studentId, payload).subscribe({
        next: () => {
          this.loading = false;
          this.router.navigate(['/detail', this.student.studentId]);
        },
        error: (err) => this.handleServerError('update', err)
      });
    } else {
      this.studentService.createStudent(payload).subscribe({
        next: (response) => {
          this.loading = false;
          this.router.navigate(['/']);
        },
        error: (err) => this.handleServerError('create', err)
      });
    }
  }
}
