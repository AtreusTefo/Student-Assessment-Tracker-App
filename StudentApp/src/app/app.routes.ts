import { Routes } from '@angular/router';
import { StudentListComponent } from './components/student-list.component';
import { StudentDetailComponent } from './components/student-detail.component';
import { StudentFormComponent } from './components/student-form.component';
import { SignUpFormComponent } from './components/signup-form.component';
import { LoginFormComponent } from './components/login-form.component';
import { StudentLoginComponent } from './components/student-login.component';
import { StudentDashboardComponent } from './components/student-dashboard.component';
import { AdminLoginComponent } from './components/admin-login.component';
import { AdminDashboardComponent } from './components/admin-dashboard.component';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { studentAuthGuard } from './core/guards/student-auth.guard';
import { studentGuestGuard } from './core/guards/student-guest.guard';
import { adminAuthGuard, adminGuestGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  // Teacher protected routes — require teacher authentication
  { path: '', component: StudentListComponent, canActivate: [authGuard] },
  { path: 'create', component: StudentFormComponent, canActivate: [authGuard] },
  { path: 'edit/:id', component: StudentFormComponent, canActivate: [authGuard] },
  { path: 'detail/:id', component: StudentDetailComponent, canActivate: [authGuard] },

  // Teacher guest-only routes — redirect to home if already logged in
  { path: 'login', component: LoginFormComponent, canActivate: [guestGuard] },
  { path: 'activate', component: SignUpFormComponent, canActivate: [guestGuard] },
  // Backward-compat redirect: old /register links → /activate
  { path: 'register', redirectTo: 'activate', pathMatch: 'full' },

  // Student guest-only routes — redirect to dashboard if already logged in
  { path: 'student/login', component: StudentLoginComponent, canActivate: [studentGuestGuard] },
  { path: 'student/activate', redirectTo: 'student/login', pathMatch: 'full' },

  // Student protected routes — require student authentication
  { path: 'student/dashboard', component: StudentDashboardComponent, canActivate: [studentAuthGuard] },

  // Admin routes
  { path: 'admin', redirectTo: 'admin/login', pathMatch: 'full' },
  { path: 'admin/login', component: AdminLoginComponent, canActivate: [adminGuestGuard] },
  { path: 'admin/dashboard', component: AdminDashboardComponent, canActivate: [adminAuthGuard] },

  { path: '**', redirectTo: '' }
];
