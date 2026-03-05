import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { LoginDto } from '../core/models';
import { TeacherStateService } from '../core/services/state';
import { TeacherBusinessService } from '../features/teachers/services/teacher-business.service';
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
  template: `
    <div class="container">
      <h2>Teacher Login</h2>
      
      <div *ngIf="error && isServerError" class="server-error">{{ error }}</div>
      
      <form (ngSubmit)="onSubmit(form)" #form="ngForm" class="form">
        <div class="form-group">
          <label for="email">Email:</label>
          <input type="email" id="email" [(ngModel)]="credentials.email" name="email" #email="ngModel" autocomplete="email" required email maxlength="100" [disabled]="loading" (input)="clearError()" />
          <span class="error" *ngIf="(form.submitted || email.touched) && email.hasError('required')">Email is required</span>
          <span class="error" *ngIf="(form.submitted || email.touched) && email.hasError('email')">Email must be a valid email address</span>
        </div>
        
        <div class="form-group">
          <label for="password">Password:</label>
          <div class="input-wrapper">
            <input [type]="showPassword ? 'text' : 'password'" id="password" [(ngModel)]="credentials.password" name="password" #password="ngModel" autocomplete="current-password" required minlength="6" maxlength="20" [disabled]="loading" (input)="clearError()" />
            <button type="button" class="toggle-password" (click)="showPassword = !showPassword" tabindex="-1">{{ showPassword ? 'Hide' : 'Show' }}</button>
          </div>
          <span class="error" *ngIf="(form.submitted || password.touched) && password.hasError('required')">Password is required</span>
          <span class="error" *ngIf="(form.submitted || password.touched) && password.hasError('minlength')">Password must be at least 6 characters</span>
        </div>
        
        <div class="actions">
          <button type="submit" class="btn btn-primary" [disabled]="loading">
            {{ loading ? 'Logging in...' : 'Login' }}
          </button>
          <a routerLink="/register" class="btn btn-secondary">Register</a>
        </div>
      </form>
      
      <div class="signup-link">
        <p>Don't have an account? <a routerLink="/register">Sign up here</a></p>
      </div>
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

    .signup-link {
      text-align: center;
      margin-top: 20px;
      font-size: 14px;
    }

    .signup-link a {
      color: #2196F3;
      text-decoration: none;
      font-weight: bold;
    }

    .signup-link a:hover {
      text-decoration: underline;
    }
  `]
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

  private destroy$ = new Subject<void>();

  constructor(
    private router: Router,
    private teacherBusiness: TeacherBusinessService,
    private teacherState: TeacherStateService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
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
