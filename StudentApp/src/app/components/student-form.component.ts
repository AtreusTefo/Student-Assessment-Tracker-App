import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { CreateStudentDto, UpdateStudentDto, GradeDto } from '../core/models';
import { StudentStateService, TeacherStateService } from '../core/services/state';
import { StudentBusinessService } from '../features/students/services/student-business.service';
import { GradeApiService } from '../core/services/http';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

/**
 * PRESENTATION LAYER - Student Form Component
 * Responsible ONLY for UI presentation and form handling
 * Delegates all business logic to StudentBusinessService
 * Subscribes to StudentStateService for reactive data
 */

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
          <label for="idPassportNo">ID / Passport No.:</label>
          <input type="text" id="idPassportNo" [(ngModel)]="student.idPassportNo" name="idPassportNo" #idPassportNo="ngModel" placeholder="e.g., 123416789 or PA1234567" autocomplete="off" required minlength="9" maxlength="9" pattern="^[a-zA-Z0-9\-]+$" [disabled]="loading" />
          <span class="error" *ngIf="(form.submitted || idPassportNo.touched || idPassportNo.dirty) && idPassportNo.hasError('required')">ID/Passport No. is required</span>
          <span class="error" *ngIf="(form.submitted || idPassportNo.touched || idPassportNo.dirty) && idPassportNo.hasError('pattern')">ID/Passport No. can only contain letters, numbers, and hyphens</span>
          <span class="error" *ngIf="(form.submitted || idPassportNo.touched || idPassportNo.dirty) && (idPassportNo.hasError('minlength') || idPassportNo.hasError('maxlength'))">ID/Passport No. must be exactly 9 characters</span>
          <span class="error" *ngIf="fieldErrors['idPassportNo'] && fieldErrors['idPassportNo'].length">{{ fieldErrors['idPassportNo'].join('\n') }}</span>
        </div>
        
        <div class="form-group">
          <label for="firstName">First Name:</label>
          <input type="text" id="firstName" [(ngModel)]="student.firstName" name="firstName" #firstName="ngModel" autocomplete="given-name" required minlength="2" maxlength="50" pattern="^[a-zA-Z]+$" (keypress)="allowOnlyLetters($event)" [disabled]="loading" />
          <span class="error" *ngIf="(form.submitted || firstName.touched || firstName.dirty) && firstName.hasError('required')">First name is required</span>
          <span class="error" *ngIf="(form.submitted || firstName.touched || firstName.dirty) && firstName.hasError('minlength')">First name must be at least 2 characters</span>
          <span class="error" *ngIf="(form.submitted || firstName.touched || firstName.dirty) && firstName.hasError('maxlength')">First name cannot exceed 50 characters</span>
          <span class="error" *ngIf="(form.submitted || firstName.touched || firstName.dirty) && firstName.hasError('pattern')">First name can only contain letters</span>
          <span class="error" *ngIf="fieldErrors['firstName'] && fieldErrors['firstName'].length">{{ fieldErrors['firstName'].join('\n') }}</span>
        </div>
        
        <div class="form-group">
          <label for="lastName">Last Name:</label>
          <input type="text" id="lastName" [(ngModel)]="student.lastName" name="lastName" #lastName="ngModel" autocomplete="family-name" required minlength="2" maxlength="50" pattern="^[a-zA-Z]+$" (keypress)="allowOnlyLetters($event)" [disabled]="loading" />
          <span class="error" *ngIf="(form.submitted || lastName.touched || lastName.dirty) && lastName.hasError('required')">Last name is required</span>
          <span class="error" *ngIf="(form.submitted || lastName.touched || lastName.dirty) && lastName.hasError('minlength')">Last name must be at least 2 characters</span>
          <span class="error" *ngIf="(form.submitted || lastName.touched || lastName.dirty) && lastName.hasError('maxlength')">Last name cannot exceed 50 characters</span>
          <span class="error" *ngIf="(form.submitted || lastName.touched || lastName.dirty) && lastName.hasError('pattern')">Last name can only contain letters</span>
          <span class="error" *ngIf="fieldErrors['lastName'] && fieldErrors['lastName'].length">{{ fieldErrors['lastName'].join('\n') }}</span>
        </div>
        
        <div class="form-group">
          <label for="email">Email:</label>
          <input type="email" id="email" [(ngModel)]="student.email" name="email" #email="ngModel" autocomplete="email" required maxlength="100" [disabled]="loading" />
          <span class="error" *ngIf="(form.submitted || email.touched || email.dirty) && email.hasError('required')">Email is required</span>
          <span class="error" *ngIf="(form.submitted || email.touched || email.dirty) && email.hasError('email')">Email must be a valid email address</span>
          <span class="error" *ngIf="(form.submitted || email.touched || email.dirty) && email.hasError('maxlength')">Email cannot exceed 100 characters</span>
          <span class="error" *ngIf="fieldErrors['email'] && fieldErrors['email'].length">{{ fieldErrors['email'].join('\n') }}</span>
        </div>
        
        <div class="form-group">
          <label for="phone">Phone (8 digits, e.g., 72254856):</label>
          <input type="text" id="phone" [(ngModel)]="student.phone" name="phone" #phone="ngModel" placeholder="72254856" minlength="8" maxlength="8" pattern="^\\d{8}$" autocomplete="tel" (input)="validatePhone()" (keypress)="allowOnlyNumbers($event)" required [disabled]="loading" />
          <span class="error" *ngIf="(form.submitted || phone.touched || phone.dirty) && phone.hasError('required')">Phone is required</span>
          <span class="error" *ngIf="(form.submitted || phone.touched || phone.dirty) && (phone.hasError('pattern') || phone.hasError('minlength') || phone.hasError('maxlength'))">Phone must be exactly 8 digits</span>
          <span class="error" *ngIf="fieldErrors['phone'] && fieldErrors['phone'].length">{{ fieldErrors['phone'].join('\n') }}</span>
        </div>
        
        <div class="form-group">
          <label for="gradeId">Grade:</label>
          <select id="gradeId" [(ngModel)]="student.gradeId" name="gradeId" #gradeId="ngModel" required [disabled]="loading">
            <option [value]="0">-- Select Grade --</option>
            <option *ngFor="let g of grades" [value]="g.id">{{ g.name }}</option>
          </select>
          <span class="error" *ngIf="(form.submitted || gradeId.touched || gradeId.dirty) && gradeId.hasError('required')">Grade is required</span>
          <span class="error" *ngIf="fieldErrors['gradeId'] && fieldErrors['gradeId'].length">{{ fieldErrors['gradeId'].join('\n') }}</span>
        </div>
        
        <div class="actions">
          <button type="submit" class="btn btn-primary" [disabled]="loading">
            {{ loading ? 'Saving...' : (isEdit ? 'Update' : 'Create') }}
          </button>
          <a [routerLink]="loading ? null : '/'" class="btn btn-secondary" [class.disabled]="loading">Cancel</a>
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
    
    .form-group input,
    .form-group select {
      width: 100%;
      padding: 10px;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 14px;
      box-sizing: border-box;
    }
    
    .form-group input:focus,
    .form-group select:focus {
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
    
    .btn-secondary.disabled {
      background-color: #bdbdbd;
      cursor: not-allowed;
      pointer-events: none;
    }
    
    .form-group input:disabled,
    .form-group select:disabled {
      background-color: #f5f5f5;
      color: #9e9e9e;
      cursor: not-allowed;
      border-color: #e0e0e0;
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
export class StudentFormComponent implements OnInit, OnDestroy {
  student = {
    studentId: 0,
    idPassportNo: '',
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    gradeId: 0,
    enrollmentDate: new Date().toISOString(),
    createdDate: new Date().toISOString()
  };

  grades: GradeDto[] = [];
  private teacherId = 0;
  
  isEdit = false;
  loading = false;
  error: string | null = null;
  isServerError = false;
  fieldErrors: Record<string, string[]> = {};
  
  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private studentBusiness: StudentBusinessService,
    private studentState: StudentStateService,
    private teacherState: TeacherStateService,
    private gradeApi: GradeApiService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    // Subscribe to reactive state
    this.studentState.loading$
      .pipe(takeUntil(this.destroy$))
      .subscribe(loading => {
        this.loading = loading;
        this.cdr.markForCheck();
      });
    
    this.studentState.error$
      .pipe(takeUntil(this.destroy$))
      .subscribe(error => {
        if (error) {
          this.error = error;
          this.isServerError = true;
        }
        this.cdr.markForCheck();
      });
    
    this.studentState.selectedStudent$
      .pipe(takeUntil(this.destroy$))
      .subscribe(student => {
        if (student && this.isEdit) {
          // Parse phone number: strip "+267 " prefix if present
          let parsedPhone = '';
          if (student.phone) {
            parsedPhone = student.phone.startsWith('+267 ') 
              ? student.phone.substring(5) 
              : student.phone;
          }
          
          this.student = {
            studentId: student.id,
            idPassportNo: student.idPassportNo || '',
            firstName: student.firstName || '',
            lastName: student.lastName || '',
            email: student.email || '',
            phone: parsedPhone,
            gradeId: student.gradeId || 0,
            enrollmentDate: student.createdAt || new Date().toISOString(),
            createdDate: student.createdAt || new Date().toISOString()
          };
          this.cdr.markForCheck();
        }
      });
    
    // Load student if editing
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.studentBusiness.loadStudentById(parseInt(id)).subscribe();
    }

    // Get current teacher id synchronously
    const teacher = this.teacherState.getCurrentTeacher();
    this.teacherId = teacher?.id ?? 0;

    // Load grade options
    this.gradeApi.getAll().subscribe(grades => {
      this.grades = grades;
      this.cdr.markForCheck();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.isEdit) {
      this.studentBusiness.clearSelectedStudent();
    }
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

  allowOnlyLetters(event: KeyboardEvent): void {
    const char = String.fromCharCode(event.which);
    if (!/[a-zA-Z]/.test(char)) {
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


  onSubmit(form: NgForm): void {
    this.error = null;
    this.isServerError = false;
    this.fieldErrors = {};

    if (form.invalid || this.student.gradeId === 0) {
      if (this.student.gradeId === 0) {
        this.fieldErrors['gradeId'] = ['Please select a grade'];
      }
      return;
    }

    if (this.isEdit) {
      const updateDto: UpdateStudentDto = {
        idPassportNo: this.student.idPassportNo,
        firstName: this.student.firstName,
        lastName: this.student.lastName,
        email: this.student.email,
        phone: this.student.phone,
        gradeId: this.student.gradeId
      };
      
      this.studentBusiness.updateStudent(this.student.studentId, updateDto).subscribe({
        next: () => {
          this.router.navigate(['/detail', this.student.studentId]);
        },
        error: (err) => this.handleServerError('update', err)
      });
    } else {
      const createDto: CreateStudentDto = {
        idPassportNo: this.student.idPassportNo,
        firstName: this.student.firstName,
        lastName: this.student.lastName,
        email: this.student.email,
        phone: this.student.phone,
        gradeId: this.student.gradeId,
        teacherId: this.teacherId
      };
      
      this.studentBusiness.createStudent(createDto).subscribe({
        next: () => {
          this.router.navigate(['/']);
        },
        error: (err) => this.handleServerError('create', err)
      });
    }
  }
}
