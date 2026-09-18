import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { CreateTeacherDto, TeacherActivateDto, SubjectDto } from '../../core/models';
import { TeacherStateService } from '../../core/services/state';
import { TeacherBusinessService } from '../../features/teachers/services/teacher-business.service';
import { SubjectApiService } from '../../core/services/http';
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
  templateUrl: './teacher-activate.component.html',

  styleUrls: ['./teacher-activate.component.css']
})
export class SignUpFormComponent implements OnInit, OnDestroy {
  teacher = {
    teacherId: 0,
    idPassportNo: '',
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    subjectId: 0,
    password: '',
    confirmPassword: ''
  };

  subjects: SubjectDto[] = [];

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
    private subjectApi: SubjectApiService,
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
    // Load subjects for the dropdown
    this.subjectApi.getAll().subscribe(subjects => {
      this.subjects = subjects;
      this.cdr.markForCheck();
    });

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
            idPassportNo: teacher.idPassportNo || '',
            firstName: teacher.firstName || '',
            lastName: teacher.lastName || '',
            email: teacher.email || '',
            phone: teacher.phone || '',
            subjectId: teacher.subjectId || 0,
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

  allowOnlyLetters(event: KeyboardEvent): void {
    const char = String.fromCharCode(event.which);
    if (!/[a-zA-Z]/.test(char)) {
      event.preventDefault();
    }
  }

  allowOnlyAlphanumeric(event: KeyboardEvent): void {
    const char = String.fromCharCode(event.which);
    if (!/[a-zA-Z0-9]/.test(char)) {
      event.preventDefault();
    }
  }

  allowOnlyAlphanumericHyphen(event: KeyboardEvent): void {
    const char = String.fromCharCode(event.which);
    if (!/[a-zA-Z0-9\-]/.test(char)) {
      event.preventDefault();
    }
  }

  isPasswordComplex(password: string): boolean {
    return /[A-Z]/.test(password) && /[0-9]/.test(password) && /[^a-zA-Z0-9]/.test(password);
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

  private handleServerError(action: string, err: any): void {
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

    if (!this.isEdit && !this.isPasswordComplex(this.teacher.password)) {
      return;
    }

    if (!this.isEdit && this.passwordMismatch) {
      return;
    }

    if (!this.isEdit) {
      // Activation path: email + password only
      const activateDto: TeacherActivateDto = {
        email: this.teacher.email,
        password: this.teacher.password,
        confirmPassword: this.teacher.confirmPassword
      };

      this.teacherBusiness.activate(activateDto).subscribe({
        next: () => this.router.navigate(['/']),
        error: (err: any) => this.handleServerError('activate', err)
      });
    } else {
      // Edit profile path: all fields required
      if (this.teacher.subjectId === 0) {
        return;
      }

      const createDto: CreateTeacherDto = {
        idPassportNo: this.teacher.idPassportNo,
        firstName: this.teacher.firstName,
        lastName: this.teacher.lastName,
        email: this.teacher.email,
        phone: this.teacher.phone,
        subjectId: this.teacher.subjectId
      };

      this.teacherBusiness.register(createDto).subscribe({
        next: () => this.router.navigate(['/']),
        error: (err: any) => this.handleServerError('update', err)
      });
    }
  }
}
