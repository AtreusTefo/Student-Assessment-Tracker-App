import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { AdminApiService } from '../core/services/http/admin-api.service';

@Component({
  selector: 'app-admin-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="container">
      <div class="login-card">
        <div class="login-header">
          <span class="admin-icon">🛡️</span>
          <h2>Admin Portal</h2>
          <p class="subtitle">System Administrator Access</p>
        </div>

        <div *ngIf="error" class="alert alert-error">{{ error }}</div>

        <form (ngSubmit)="onSubmit(form)" #form="ngForm">
          <div class="form-group">
            <label for="email">Email</label>
            <input type="email" id="email" name="email" [(ngModel)]="credentials.email"
              required email #emailField="ngModel"
              [class.invalid]="(form.submitted || emailField.touched) && emailField.invalid"
              placeholder="admin@example.com" />
            <span class="error-msg"
              *ngIf="(form.submitted || emailField.touched) && emailField.hasError('required')">
              Email is required
            </span>
            <span class="error-msg"
              *ngIf="(form.submitted || emailField.touched) && emailField.hasError('email')">
              Enter a valid email address
            </span>
          </div>

          <div class="form-group">
          <label for="password">Password:</label>
          <div class="input-wrapper">
            <input [type]="showPassword ? 'text' : 'password'" id="password" [(ngModel)]="credentials.password" name="password" #password="ngModel" autocomplete="current-password" required minlength="8" [disabled]="loading" (input)="clearError()" />
            <button type="button" class="toggle-password" (click)="showPassword = !showPassword" tabindex="-1">{{ showPassword ? 'Hide' : 'Show' }}</button>
          </div>
          <span class="error" *ngIf="(form.submitted || password.touched) && password.hasError('required')">Password is required</span>
          <span class="error" *ngIf="(form.submitted || password.touched) && password.hasError('minlength')">Password must be at least 8 characters</span>
          </div>

          <button type="submit" class="btn-login" [disabled]="loading">
            {{ loading ? 'Signing in…' : 'Sign In' }}
          </button>
        </form>

        <div class="back-link">
          <a routerLink="/login">← Teacher Login</a>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .container {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
      padding: 20px;
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
    .login-card {
      background: #fff;
      border-radius: 12px;
      padding: 40px;
      width: 100%;
      max-width: 420px;
      box-shadow: 0 20px 60px rgba(0,0,0,0.3);
    }
    .login-header { text-align: center; margin-bottom: 28px; }
    .admin-icon { font-size: 2.5rem; }
    h2 { margin: 8px 0 4px; font-size: 1.6rem; color: #1a1a2e; }
    .subtitle { color: #666; font-size: 0.9rem; margin: 0; }
    .form-group { margin-bottom: 16px; }
    label { display: block; margin-bottom: 6px; font-weight: 600; font-size: 0.9rem; color: #333; }
    input {
      width: 100%; padding: 10px 14px; border: 1.5px solid #ddd; border-radius: 6px;
      font-size: 0.95rem; box-sizing: border-box; transition: border-color 0.2s;
    }
    input:focus { outline: none; border-color: #0f3460; }
    input.invalid { border-color: #e53e3e; }
    .error-msg { color: #e53e3e; font-size: 0.8rem; margin-top: 4px; display: block; }
    .alert-error {
      background: #fff5f5; border: 1px solid #feb2b2; color: #c53030;
      padding: 10px 14px; border-radius: 6px; margin-bottom: 16px; font-size: 0.9rem;
    }
    .btn-login {
      width: 100%; padding: 12px; background: #0f3460; color: #fff;
      border: none; border-radius: 6px; font-size: 1rem; font-weight: 600;
      cursor: pointer; margin-top: 8px; transition: background 0.2s;
    }
    .btn-login:hover:not(:disabled) { background: #16213e; }
    .btn-login:disabled { opacity: 0.6; cursor: not-allowed; }
    .back-link { text-align: center; margin-top: 20px; }
    .back-link a { color: #0f3460; text-decoration: none; font-size: 0.9rem; }
    .back-link a:hover { text-decoration: underline; }
  `]
})
export class AdminLoginComponent {
  credentials = { email: '', password: '' };
  loading = false;
  error = '';
  showPassword = false;

  constructor(private adminApi: AdminApiService, private router: Router) {}

  clearError(): void {
    this.error = '';
  }

  onSubmit(form: NgForm): void {
    this.error = '';
    if (form.invalid) return;

    this.loading = true;
    this.adminApi.login(this.credentials).subscribe({
      next: res => {
        localStorage.setItem('admin_token', res.token);
        localStorage.setItem('admin_info', JSON.stringify(res.admin));
        this.router.navigate(['/admin/dashboard']);
      },
      error: () => {
        this.loading = false;
        this.error = 'Invalid email or password.';
      }
    });
  }
}
