import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TeacherService, LoginDto } from '../services/teacher.service';

@Component({
  selector: 'app-login-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="container">
      <h2>Teacher Login</h2>
      
      <div *ngIf="error && isServerError" class="server-error">{{ error }}</div>
      
      <form (ngSubmit)="onSubmit()" #form="ngForm" class="form">
        <div class="form-group">
          <label for="email">Email:</label>
          <input type="email" id="email" [(ngModel)]="credentials.email" name="email" required />
          <span class="error" *ngIf="form.submitted && !credentials.email">Email is required</span>
        </div>
        
        <div class="form-group">
          <label for="password">Password:</label>
          <input type="password" id="password" [(ngModel)]="credentials.password" name="password" required />
          <span class="error" *ngIf="form.submitted && !credentials.password">Password is required</span>
        </div>
        
        <div class="actions">
          <button type="submit" class="btn btn-primary" [disabled]="loading">
            {{ loading ? 'Logging in...' : 'Login' }}
          </button>
          <a routerLink="/" class="btn btn-secondary">Cancel</a>
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
export class LoginFormComponent implements OnInit {
  credentials: LoginDto = {
    email: '',
    password: ''
  };
  
  loading = false;
  error: string | null = null;
  isServerError = false;

  constructor(
    private router: Router,
    private teacherService: TeacherService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    // Login form doesn't need to load any data
  }

  onSubmit(): void {
    this.error = null;
    this.isServerError = false;
    
    if (!this.credentials.email || !this.credentials.password) {
      this.error = 'Email and password are required';
      return;
    }

    this.loading = true;

    this.teacherService.login(this.credentials).subscribe({
      next: (response: any) => {
        this.loading = false;
        // Store token in localStorage
        localStorage.setItem('authToken', response.token);
        localStorage.setItem('teacher', JSON.stringify(response.teacher));
        this.router.navigate(['/']);
      },
      error: (err: any) => {
        this.loading = false;
        this.isServerError = true;
        if (err.error && err.error.errors) {
          const errorMessages = Object.values(err.error.errors)
            .flat()
            .join('\n');
          this.error = errorMessages as string;
        } else {
          this.error = 'Login failed: ' + (err.error?.title || err.message || 'Invalid credentials');
        }
        this.cdr.markForCheck();
      }
    });
  }
}
