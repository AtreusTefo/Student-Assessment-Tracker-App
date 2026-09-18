import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { LoginDto } from '../../core/models';
import { TeacherStateService } from '../../core/services/state';
import { TeacherBusinessService } from '../../features/teachers/services/teacher-business.service';
import { TeacherApiService } from '../../core/services/http';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

/**
 * PRESENTATION LAYER - Login Form Component
 * Responsible ONLY for UI presentation and form handling
 * Delegates authentication logic to TeacherBusinessService
 */

@Component({
  selector: 'app-login-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './teacher-login-form.component.html',

  styleUrls: ['./teacher-login-form.component.css']
})
export class LoginFormComponent implements OnInit, OnDestroy {
  credentials: LoginDto = {
    email: '',
    password: ''
  };

  loading = false;
  error: string | null = null;
  isServerError = false;
  showPassword = false;

  // Forgot-password state
  forgotMode = false;
  forgotEmail = '';
  forgotLoading = false;
  forgotError: string | null = null;
  forgotSuccess: string | null = null;

  private destroy$ = new Subject<void>();

  constructor(
    private router: Router,
    private teacherBusiness: TeacherBusinessService,
    private teacherState: TeacherStateService,
    private teacherApi: TeacherApiService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
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

    this.teacherState.isAuthenticated$
      .pipe(takeUntil(this.destroy$))
      .subscribe(isAuth => {
        if (isAuth) {
          this.router.navigate(['/']);
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

  enterForgotMode(): void {
    this.forgotMode = true;
    this.forgotEmail = this.credentials.email;
    this.forgotError = null;
    this.forgotSuccess = null;
  }

  exitForgotMode(): void {
    this.forgotMode = false;
    this.forgotError = null;
    this.forgotSuccess = null;
  }

  onForgotSubmit(form: NgForm): void {
    if (form.invalid) return;
    this.forgotError = null;
    this.forgotLoading = true;
    this.cdr.markForCheck();

    this.teacherApi.forgotPassword(this.forgotEmail.trim()).subscribe({
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
    this.clearError();

    if (form.invalid) {
      return;
    }

    this.teacherBusiness.login(this.credentials).subscribe({
      error: (err: any) => {
        this.isServerError = true;
        this.error = err?.error?.message || err?.error?.title || 'Invalid email or password';
        this.cdr.markForCheck();
      }
    });
  }
}
