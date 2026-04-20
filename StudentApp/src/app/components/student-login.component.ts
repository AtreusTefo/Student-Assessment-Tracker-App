import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { StudentLoginDto, StudentActivateDto } from '../core/models';
import { StudentAuthStateService } from '../core/services/state';
import { StudentAuthBusinessService } from '../features/students/services/student-auth-business.service';
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
  template: `
    <div class="auth-wrapper">
      <div class="auth-card">
        <div class="auth-header">
          <div class="auth-icon">🎓</div>
          <h2>{{ activateMode ? 'Activate Your Account' : 'Student Login' }}</h2>
          <p class="auth-subtitle">
            {{ activateMode
              ? 'First time? Set up your password using the Student ID given by your teacher'
              : 'Sign in to view your marks and performance' }}
          </p>
        </div>

        <div *ngIf="error" class="alert alert-error">{{ error }}</div>

        <form (ngSubmit)="onSubmit(form)" #form="ngForm" class="auth-form" novalidate>

          <div class="form-group">
            <label for="studentId">Student ID</label>
            <input
              type="text"
              id="studentId"
              [(ngModel)]="studentUniqueId"
              name="studentUniqueId"
              #studentIdRef="ngModel"
              placeholder="e.g. STU-AB12CD34"
              required
              maxlength="12"
              autocomplete="username"
              [disabled]="loading"
              (input)="clearError()"
              [style.text-transform]="activateMode ? 'uppercase' : 'none'"
            />
            <span class="field-error" *ngIf="(form.submitted || studentIdRef.touched) && studentIdRef.hasError('required')">
              Student ID is required
            </span>
          </div>

          <!-- Email — only shown in activate mode -->
          <div class="form-group" *ngIf="activateMode">
            <label for="email">Email <span class="hint">(registered by your school admin)</span></label>
            <input
              type="email"
              id="email"
              [(ngModel)]="email"
              name="email"
              #emailRef="ngModel"
              placeholder="your.email@example.com"
              required
              email
              maxlength="100"
              autocomplete="email"
              [disabled]="loading"
              (input)="clearError()"
            />
            <span class="field-error" *ngIf="(form.submitted || emailRef.touched) && emailRef.hasError('required')">
              Email is required
            </span>
            <span class="field-error" *ngIf="(form.submitted || emailRef.touched) && emailRef.hasError('email')">
              Enter a valid email address
            </span>
          </div>

          <div class="form-group">
            <label for="password">{{ activateMode ? 'Choose a Password' : 'Password' }}</label>
            <div class="input-group">
              <input
                [type]="showPassword ? 'text' : 'password'"
                id="password"
                [(ngModel)]="password"
                name="password"
                #passwordRef="ngModel"
                [autocomplete]="activateMode ? 'new-password' : 'current-password'"
                required
                minlength="6"
                maxlength="50"
                [disabled]="loading"
                (input)="clearError()"
              />
              <button type="button" class="toggle-btn" (click)="showPassword = !showPassword" tabindex="-1">
                {{ showPassword ? 'Hide' : 'Show' }}
              </button>
            </div>
            <span class="field-error" *ngIf="(form.submitted || passwordRef.touched) && passwordRef.hasError('required')">
              Password is required
            </span>
            <span class="field-error" *ngIf="(form.submitted || passwordRef.touched) && passwordRef.hasError('minlength')">
              Password must be at least 6 characters
            </span>
          </div>

          <!-- Confirm Password — only shown in activate mode -->
          <div class="form-group" *ngIf="activateMode">
            <label for="confirmPassword">Confirm Password</label>
            <div class="input-group">
              <input
                [type]="showConfirmPassword ? 'text' : 'password'"
                id="confirmPassword"
                [(ngModel)]="confirmPassword"
                name="confirmPassword"
                #confirmPwdRef="ngModel"
                autocomplete="new-password"
                required
                minlength="6"
                maxlength="50"
                [disabled]="loading"
                (input)="clearError()"
              />
              <button type="button" class="toggle-btn" (click)="showConfirmPassword = !showConfirmPassword" tabindex="-1">
                {{ showConfirmPassword ? 'Hide' : 'Show' }}
              </button>
            </div>
            <span class="field-error" *ngIf="(form.submitted || confirmPwdRef.touched) && confirmPwdRef.hasError('required')">
              Please confirm your password
            </span>
            <span class="field-error" *ngIf="(form.submitted || confirmPwdRef.touched) && !confirmPwdRef.hasError('required') && passwordMismatch">
              Passwords do not match
            </span>
          </div>

          <button type="submit" class="btn-primary" [disabled]="loading">
            <span *ngIf="loading" class="spinner"></span>
            {{ loading
              ? (activateMode ? 'Activating…' : 'Signing in…')
              : (activateMode ? 'Activate Account' : 'Sign In') }}
          </button>
        </form>

        <div class="auth-links">
          <p *ngIf="!activateMode">First time logging in? <a (click)="toggleMode()" class="mode-link">Activate your account</a></p>
          <p *ngIf="activateMode">Already activated? <a (click)="toggleMode()" class="mode-link">Sign in here</a></p>
          <p class="divider-text">Are you a teacher? <a routerLink="/login">Teacher login</a></p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-wrapper {
      min-height: 70vh;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 20px;
    }
    .auth-card {
      background: #fff;
      border-radius: 12px;
      box-shadow: 0 4px 20px rgba(0,0,0,0.12);
      padding: 40px;
      width: 100%;
      max-width: 440px;
    }
    .auth-header {
      text-align: center;
      margin-bottom: 28px;
    }
    .auth-icon { font-size: 48px; margin-bottom: 8px; }
    .auth-header h2 { margin: 0 0 6px; font-size: 24px; color: #1a1a2e; }
    .auth-subtitle { color: #666; font-size: 14px; margin: 0; }
    .hint { font-weight: 400; color: #999; font-size: 12px; }
    .alert { padding: 12px 16px; border-radius: 6px; margin-bottom: 20px; font-size: 14px; }
    .alert-error { background: #fdecea; color: #c62828; border: 1px solid #ef9a9a; }
    .auth-form { display: flex; flex-direction: column; gap: 18px; }
    .form-group { display: flex; flex-direction: column; gap: 6px; }
    .form-group label { font-weight: 600; font-size: 14px; color: #333; }
    .form-group input {
      padding: 11px 14px;
      border: 1px solid #ddd;
      border-radius: 6px;
      font-size: 15px;
      transition: border-color 0.2s;
      box-sizing: border-box;
      width: 100%;
    }
    .form-group input:focus { outline: none; border-color: #4caf50; box-shadow: 0 0 0 3px rgba(76,175,80,0.15); }
    .form-group input:disabled { background: #f5f5f5; cursor: not-allowed; }
    .input-group { position: relative; display: flex; }
    .input-group input { flex: 1; padding-right: 60px; }
    .toggle-btn {
      position: absolute; right: 10px; top: 50%; transform: translateY(-50%);
      background: none; border: none; color: #4caf50; cursor: pointer; font-size: 12px; font-weight: 700;
    }
    .field-error { color: #e53935; font-size: 12px; }
    .btn-primary {
      background: linear-gradient(135deg, #4caf50, #388e3c);
      color: #fff;
      border: none;
      padding: 13px;
      border-radius: 6px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      margin-top: 6px;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      transition: opacity 0.2s;
    }
    .btn-primary:disabled { opacity: 0.65; cursor: not-allowed; }
    .spinner {
      width: 16px; height: 16px;
      border: 2px solid rgba(255,255,255,0.4);
      border-top-color: #fff;
      border-radius: 50%;
      animation: spin 0.7s linear infinite;
      display: inline-block;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
    .auth-links { text-align: center; margin-top: 24px; }
    .auth-links p { margin: 8px 0; font-size: 14px; color: #555; }
    .auth-links a { color: #4caf50; text-decoration: none; font-weight: 600; }
    .auth-links a:hover { text-decoration: underline; }
    .mode-link { cursor: pointer; }
    .divider-text { color: #999 !important; }
  `]
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

  get passwordMismatch(): boolean {
    return this.password !== this.confirmPassword;
  }

  private destroy$ = new Subject<void>();

  constructor(
    private router: Router,
    private studentAuthState: StudentAuthStateService,
    private studentAuthBusiness: StudentAuthBusinessService,
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
