import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { StudentLoginDto, StudentActivateDto } from '../../core/models';
import { StudentAuthStateService } from '../../core/services/state';
import { StudentAuthBusinessService } from '../../features/students/services/student-auth-business.service';
import { StudentApiService } from '../../core/services/http';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

/**
 * PRESENTATION LAYER - Student Login Component
 * Allows a student to authenticate using their StudentUniqueId and password.
 * First-time students can toggle to "Activate" mode to set up their password.
 */
@Component({
  selector: 'app-student-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './student-login.component.html',

  styleUrls: ['./student-login.component.css']
})
export class StudentLoginComponent implements OnInit, OnDestroy {
  studentUniqueId = '';
  email = '';
  password = '';
  confirmPassword = '';
  loading = false;
  error: string | null = null;
  showPassword = false;
  showConfirmPassword = false;
  activateMode = false;

  // Forgot-password state
  forgotMode = false;
  forgotStudentUniqueId = '';
  forgotEmail = '';
  forgotLoading = false;
  forgotError: string | null = null;
  forgotSuccess: string | null = null;

  get passwordMismatch(): boolean {
    return this.password !== this.confirmPassword;
  }

  private destroy$ = new Subject<void>();

  constructor(
    private router: Router,
    private studentAuthState: StudentAuthStateService,
    private studentAuthBusiness: StudentAuthBusinessService,
    private studentApi: StudentApiService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.studentAuthState.loading$.pipe(takeUntil(this.destroy$)).subscribe(v => {
      this.loading = v;
      this.cdr.markForCheck();
    });
    this.studentAuthState.error$.pipe(takeUntil(this.destroy$)).subscribe(e => {
      this.error = e;
      this.cdr.markForCheck();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  clearError(): void {
    this.studentAuthBusiness.clearError();
  }

  toggleMode(): void {
    this.activateMode = !this.activateMode;
    this.clearError();
  }

  enterForgotMode(): void {
    this.forgotMode = true;
    this.forgotStudentUniqueId = this.studentUniqueId;
    this.forgotEmail = '';
    this.forgotError = null;
    this.forgotSuccess = null;
  }

  exitForgotMode(): void {
    this.forgotMode = false;
    this.forgotError = null;
    this.forgotSuccess = null;
  }

  enterActivateMode(): void {
    this.forgotMode = false;
    this.activateMode = true;
    this.forgotError = null;
    this.forgotSuccess = null;
  }

  onForgotSubmit(form: NgForm): void {
    if (form.invalid) return;
    this.forgotError = null;
    this.forgotLoading = true;
    this.cdr.markForCheck();

    this.studentApi.forgotPassword(
      this.forgotStudentUniqueId.toUpperCase().trim(),
      this.forgotEmail.trim()
    ).subscribe({
      next: (res) => {
        this.forgotSuccess = res.message;
        this.forgotLoading = false;
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        this.forgotError = err?.error?.message || 'Something went wrong. Please try again.';
        this.forgotLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  onSubmit(form: NgForm): void {
    if (form.invalid) return;

    if (this.activateMode) {
      if (this.passwordMismatch) return;

      const dto: StudentActivateDto = {
        studentUniqueId: this.studentUniqueId.toUpperCase().trim(),
        email: this.email.trim(),
        password: this.password,
        confirmPassword: this.confirmPassword
      };

      this.studentAuthBusiness.activate(dto).subscribe({
        next: () => this.router.navigate(['/student/dashboard']),
        error: () => { /* error handled by state service */ }
      });
    } else {
      const dto: StudentLoginDto = {
        studentUniqueId: this.studentUniqueId,
        password: this.password
      };

      this.studentAuthBusiness.login(dto).subscribe({
        next: () => this.router.navigate(['/student/dashboard']),
        error: () => { /* error handled by state service */ }
      });
    }
  }
}
