import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { AdminApiService } from '../../core/services/http/admin-api.service';

@Component({
  selector: 'app-admin-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './admin-login.component.html',
  styleUrls: ['./admin-login.component.css']
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
