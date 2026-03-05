import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { CreateTeacherDto } from '../core/models';
import { TeacherStateService } from '../core/services/state';
import { TeacherBusinessService } from '../features/teachers/services/teacher-business.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

/**
 * PRESENTATION LAYER - Signup Form Component
 * Responsible ONLY for UI presentation and form handling
 * Delegates registration logic to TeacherBusinessService
 */

@Component({
  selector: 'app-signup-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="container">
      <h2>{{ isEdit ? 'Edit Teacher Profile' : 'Register as Teacher' }}</h2>
      
      <div *ngIf="error && isServerError" class="server-error">{{ error }}</div>
      
      <form (ngSubmit)="onSubmit(form)" #form="ngForm" class="form">
        <div class="form-group">
          <label for="firstName">First Name:</label>
          <input type="text" id="firstName" [(ngModel)]="teacher.firstName" name="firstName" #firstName="ngModel" autocomplete="given-name" required minlength="2" maxlength="50" [disabled]="loading" (input)="clearError()" />
          <span class="error" *ngIf="(form.submitted || firstName.touched || firstName.dirty) && firstName.hasError('required')">First name is required</span>
          <span class="error" *ngIf="(form.submitted || firstName.touched || firstName.dirty) && firstName.hasError('minlength')">First name must be at least 2 characters</span>
          <span class="error" *ngIf="(form.submitted || firstName.touched || firstName.dirty) && firstName.hasError('maxlength')">First name cannot exceed 50 characters</span>
        </div>
        
        <div class="form-group">
          <label for="lastName">Last Name:</label>
          <input type="text" id="lastName" [(ngModel)]="teacher.lastName" name="lastName" #lastName="ngModel" autocomplete="family-name" required minlength="2" maxlength="50" [disabled]="loading" (input)="clearError()" />
          <span class="error" *ngIf="(form.submitted || lastName.touched || lastName.dirty) && lastName.hasError('required')">Last name is required</span>
          <span class="error" *ngIf="(form.submitted || lastName.touched || lastName.dirty) && lastName.hasError('minlength')">Last name must be at least 2 characters</span>
          <span class="error" *ngIf="(form.submitted || lastName.touched || lastName.dirty) && lastName.hasError('maxlength')">Last name cannot exceed 50 characters</span>
        </div>
        
        <div class="form-group">
          <label for="email">Email:</label>
          <input type="email" id="email" [(ngModel)]="teacher.email" name="email" #email="ngModel" autocomplete="email" required email maxlength="100" [disabled]="loading || isEdit" (input)="clearError()" />
          <span class="hint" *ngIf="isEdit">Email cannot be changed after registration</span>
          <span class="error" *ngIf="(form.submitted || email.touched || email.dirty) && email.hasError('required')">Email is required</span>
          <span class="error" *ngIf="(form.submitted || email.touched || email.dirty) && email.hasError('email')">Email must be a valid email address</span>
          <span class="error" *ngIf="(form.submitted || email.touched || email.dirty) && email.hasError('maxlength')">Email cannot exceed 100 characters</span>
        </div>
        
        <div class="form-group">
          <label for="phone">Phone (8 digits, e.g., 77754256):</label>
          <input type="text" id="phone" [(ngModel)]="teacher.phone" name="phone" #phone="ngModel" placeholder="77754256" minlength="8" maxlength="8" pattern="^\\d{8}$" autocomplete="tel" (input)="validatePhone(); clearError()" (keypress)="allowOnlyNumbers($event)" required [disabled]="loading" />
          <span class="error" *ngIf="(form.submitted || phone.touched || phone.dirty) && phone.hasError('required')">Phone is required</span>
          <span class="error" *ngIf="(form.submitted || phone.touched || phone.dirty) && (phone.hasError('pattern') || phone.hasError('minlength') || phone.hasError('maxlength'))">Phone must be exactly 8 digits</span>
        </div>
        
        <div class="form-group">
          <label for="subject">Subject:</label>
          <input type="text" id="subject" [(ngModel)]="teacher.subject" name="subject" #subject="ngModel" placeholder="e.g., ICT, Multimedia" autocomplete="off" required maxlength="100" [disabled]="loading" (input)="clearError()" />
          <span class="error" *ngIf="(form.submitted || subject.touched || subject.dirty) && subject.hasError('required')">Subject is required</span>
          <span class="error" *ngIf="(form.submitted || subject.touched || subject.dirty) && subject.hasError('maxlength')">Subject cannot exceed 100 characters</span>
        </div>

        <div class="form-group" *ngIf="!isEdit">
          <label for="password">Password:</label>
          <div class="input-wrapper">
            <input [type]="showPassword ? 'text' : 'password'" id="password" [(ngModel)]="teacher.password" name="password" #password="ngModel" autocomplete="new-password" required minlength="6" maxlength="20" [disabled]="loading" (input)="clearError()" />
            <button type="button" class="toggle-password" (click)="showPassword = !showPassword" tabindex="-1">{{ showPassword ? 'Hide' : 'Show' }}</button>
          </div>
          <span class="error" *ngIf="(form.submitted || password.touched || password.dirty) && password.hasError('required')">Password is required</span>
          <span class="error" *ngIf="(form.submitted || password.touched || password.dirty) && password.hasError('minlength')">Password must be at least 6 characters</span>
          <span class="error" *ngIf="(form.submitted || password.touched || password.dirty) && password.hasError('maxlength')">Password cannot exceed 20 characters</span>
        </div>

        <div class="form-group" *ngIf="!isEdit">
          <label for="confirmPassword">Confirm Password:</label>
          <div class="input-wrapper">
            <input [type]="showConfirmPassword ? 'text' : 'password'" id="confirmPassword" [(ngModel)]="teacher.confirmPassword" name="confirmPassword" #confirmPassword="ngModel" autocomplete="new-password" required [disabled]="loading" (input)="clearError()" />
            <button type="button" class="toggle-password" (click)="showConfirmPassword = !showConfirmPassword" tabindex="-1">{{ showConfirmPassword ? 'Hide' : 'Show' }}</button>
          </div>
          <span class="error" *ngIf="(form.submitted || confirmPassword.touched) && confirmPassword.hasError('required')">Please confirm your password</span>
          <span class="error" *ngIf="(form.submitted || confirmPassword.touched) && !confirmPassword.hasError('required') && passwordMismatch">Passwords do not match</span>
        </div>
        
        <div class="actions">
          <button type="submit" class="btn btn-primary" [disabled]="loading">
            {{ loading ? 'Saving...' : (isEdit ? 'Update' : 'Register') }}
          </button>
          <a routerLink="/login" class="btn btn-secondary">Cancel</a>
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

    .form-group input:disabled {
      background-color: #f5f5f5;
      cursor: not-allowed;
    }

    .input-wrapper {
      position: relative;
      display: flex;
      align-items: center;
    }

    .input-wrapper input {
      flex: 1;
      padding-right: 60px;
    }

    .toggle-password {
      position: absolute;
      right: 8px;
      background: none;
      border: none;
      color: #2196F3;
      cursor: pointer;
      font-size: 12px;
      font-weight: bold;
      padding: 4px 6px;
    }

    .toggle-password:hover {
      text-decoration: underline;
    }

    .hint {
      color: #888;
      font-size: 11px;
      margin-top: 4px;
      display: block;
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
export class SignUpFormComponent implements OnInit, OnDestroy {
  teacher = {
    teacherId: 0,
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    subject: '',
    password: '',
    confirmPassword: ''
  };

  isEdit = false;
  loading = false;
  error: string | null = null;
  isServerError = false;
  showPassword = false;
  showConfirmPassword = false;

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private teacherBusiness: TeacherBusinessService,
    private teacherState: TeacherStateService,
    private cdr: ChangeDetectorRef
  ) { }

  get passwordMismatch(): boolean {
    return (
      !!this.teacher.password &&
      !!this.teacher.confirmPassword &&
      this.teacher.password !== this.teacher.confirmPassword
    );
  }

  ngOnInit(): void {
    // Detect edit mode from route param
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.teacherBusiness.loadTeacherById(+id).subscribe();
    }

    // Subscribe to reactive state
    this.teacherState.loading$
      .pipe(takeUntil(this.destroy$))
      .subscribe(loading => {
        this.loading = loading;
        this.cdr.markForCheck();
      });
    
    this.teacherState.error$
      .pipe(takeUntil(this.destroy$))
      .subscribe(error => {
        if (error) {
          this.error = error;
          this.isServerError = true;
          this.cdr.markForCheck();
        }
      });
    
    this.teacherState.currentTeacher$
      .pipe(takeUntil(this.destroy$))
      .subscribe(teacher => {
        if (teacher && this.isEdit) {
          this.teacher = {
            teacherId: teacher.id,
            firstName: teacher.firstName || '',
            lastName: teacher.lastName || '',
            email: teacher.email || '',
            phone: teacher.phone || '',
            subject: teacher.subject || '',
            password: '',
            confirmPassword: ''
          };
          this.cdr.markForCheck();
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  clearError(): void {
    this.error = null;
    this.isServerError = false;
  }

  validatePhone(): void {
    if (this.teacher.phone) {
      // Strip any non-numeric characters (just in case)
      this.teacher.phone = this.teacher.phone.replace(/[^0-9]/g, '');
      // Limit to 8 characters
      if (this.teacher.phone.length > 8) {
        this.teacher.phone = this.teacher.phone.substring(0, 8);
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

  private handleServerError(action: 'register' | 'update', err: any): void {
    this.loading = false;

    if (this.isValidationErrorResponse(err)) {
      // Inline field errors already guide the user for validation issues.
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
      this.error = `Failed to ${action} teacher: ` + (err.error?.title || err.message);
    }
    this.cdr.markForCheck();
  }

  onSubmit(form: NgForm): void {
    this.clearError();

    if (form.invalid) {
      return;
    }

    if (!this.isEdit && this.passwordMismatch) {
      return;
    }

    const createDto: CreateTeacherDto = {
      firstName: this.teacher.firstName,
      lastName: this.teacher.lastName,
      email: this.teacher.email,
      phone: this.teacher.phone,
      subject: this.teacher.subject,
      password: this.teacher.password
    };

    this.teacherBusiness.register(createDto).subscribe({
      next: () => {
        this.router.navigate(['/login']);
      },
      error: (err: any) => this.handleServerError('register', err)
    });
  }
}
