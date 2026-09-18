import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { StudentActivateDto } from '../../core/models';
import { StudentAuthStateService } from '../../core/services/state';
import { StudentAuthBusinessService } from '../../features/students/services/student-auth-business.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

/**
 * PRESENTATION LAYER - Student Activate (Sign-Up) Component
 * A student proves their identity using the teacher-assigned StudentUniqueId
 * and their registered email, then sets a password to activate the account.
 */
@Component({
  selector: 'app-student-activate',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './student-activate.component.html',

  styleUrls: ['./student-activate.component.css']
})
export class StudentActivateComponent implements OnInit, OnDestroy {
  formData: StudentActivateDto = { studentUniqueId: '', email: '', password: '', confirmPassword: '' };
  confirmPassword = '';
  loading = false;
  error: string | null = null;
  showPassword = false;
  showConfirmPassword = false;

  get passwordMismatch(): boolean {
    return this.formData.password !== this.confirmPassword;
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

  isPasswordComplex(password: string): boolean {
    return /[A-Z]/.test(password) && /[0-9]/.test(password) && /[^a-zA-Z0-9]/.test(password);
  }

  onSubmit(form: NgForm): void {
    if (form.invalid || this.passwordMismatch) return;
    if (!this.isPasswordComplex(this.formData.password)) return;

    // Normalise StudentUniqueId to uppercase before sending
    this.formData.studentUniqueId = this.formData.studentUniqueId.toUpperCase().trim();
    // Mirror the separate confirmPassword field into the DTO so the backend validator receives it
    this.formData.confirmPassword = this.confirmPassword;

    this.studentAuthBusiness.activate(this.formData).subscribe({
      next: () => this.router.navigate(['/student/dashboard']),
      error: () => { /* error handled by state service */ }
    });
  }
}
