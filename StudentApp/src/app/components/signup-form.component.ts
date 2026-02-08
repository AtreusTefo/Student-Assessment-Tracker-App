import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TeacherService, Teacher } from '../services/teacher.service';

@Component({
  selector: 'app-signup-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="container">
      <h2>{{ isEdit ? 'Edit Teacher Profile' : 'Register as Teacher' }}</h2>
      
      <div *ngIf="error && isServerError" class="server-error">{{ error }}</div>
      
      <form (ngSubmit)="onSubmit()" #form="ngForm" class="form">
        <div class="form-group">
          <label for="firstName">First Name:</label>
          <input type="text" id="firstName" [(ngModel)]="teacher.firstName" name="firstName" required />
          <span class="error" *ngIf="form.submitted && !teacher.firstName">First name is required</span>
        </div>
        
        <div class="form-group">
          <label for="lastName">Last Name:</label>
          <input type="text" id="lastName" [(ngModel)]="teacher.lastName" name="lastName" required />
          <span class="error" *ngIf="form.submitted && !teacher.lastName">Last name is required</span>
        </div>
        
        <div class="form-group">
          <label for="email">Email:</label>
          <input type="email" id="email" [(ngModel)]="teacher.email" name="email" required />
          <span class="error" *ngIf="form.submitted && !teacher.email">Email is required</span>
        </div>
        
        <div class="form-group">
          <label for="phone">Phone (8 digits, e.g., 77754256):</label>
          <input type="text" id="phone" [(ngModel)]="teacher.phone" name="phone" placeholder="77754256" maxlength="8" (input)="validatePhone()" (keypress)="allowOnlyNumbers($event)" required />
          <span class="error" *ngIf="form.submitted && !teacher.phone">Phone is required</span>
        </div>
        
        <div class="form-group">
          <label for="subject">Subject:</label>
          <input type="text" id="subject" [(ngModel)]="teacher.subject" name="subject" placeholder="e.g., ICT, Multimedia" required />
          <span class="error" *ngIf="form.submitted && !teacher.subject">Subject is required</span>
        </div>

        <div class="form-group">
          <label for="password">Password:</label>
          <input type="password" id="password" [(ngModel)]="teacher.password" name="password" required />
          <span class="error" *ngIf="form.submitted && !teacher.password">Password is required</span>
        </div>
        
        <div class="actions">
          <button type="submit" class="btn btn-primary" [disabled]="loading">
            {{ loading ? 'Saving...' : (isEdit ? 'Update' : 'Register') }}
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
export class SignUpFormComponent implements OnInit {
  teacher: Teacher = {
    teacherId: 0,
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    subject: '',
    password: '',
    enrollmentDate: new Date().toISOString(),
    createdDate: new Date().toISOString()
  };
  
  isEdit = false;
  loading = false;
  error: string | null = null;
  isServerError = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private teacherService: TeacherService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.loadTeacher(parseInt(id));
    }
  }

  loadTeacher(id: number): void {
    this.loading = true;
    this.teacherService.getTeacher(id).subscribe({
      next: (data: Teacher) => {
        this.teacher = {
          ...data,
          phone: data.phone.replace(/^\+267\s?/, '') // Strip country code for editing
        };
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        this.error = 'Failed to load teacher: ' + err.message;
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
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



  onSubmit(): void {
    this.error = null;
    this.isServerError = false;
    
    this.loading = true;

    if (this.isEdit) {
      this.teacherService.updateTeacher(this.teacher.teacherId, this.teacher).subscribe({
        next: () => {
          this.loading = false;
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
            this.error = 'Failed to update teacher: ' + (err.error?.title || err.message);
          }
          this.cdr.markForCheck();
        }
      });
    } else {
      this.teacherService.createTeacher(this.teacher).subscribe({
        next: () => {
          this.loading = false;
          this.router.navigate(['/login']);
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
            this.error = 'Failed to register teacher: ' + (err.error?.title || err.message);
          }
          this.cdr.markForCheck();
        }
      });
    }
  }
}
