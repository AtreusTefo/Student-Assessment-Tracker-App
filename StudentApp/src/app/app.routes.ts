import { Routes } from '@angular/router';
import { StudentListComponent } from './components/student-list.component';
import { StudentDetailComponent } from './components/student-detail.component';
import { StudentFormComponent } from './components/student-form.component';
import { SignUpFormComponent } from './components/signup-form.component';
import { LoginFormComponent } from './components/login-form.component';
import { StudentLoginComponent } from './components/student-login.component';
import { StudentDashboardComponent } from './components/student-dashboard.component';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { studentAuthGuard } from './core/guards/student-auth.guard';
import { studentGuestGuard } from './core/guards/student-guest.guard';

export const routes: Routes = [
  // Teacher protected routes — require teacher authentication
  { path: '', component: StudentListComponent, canActivate: [authGuard] },
  { path: 'create', component: StudentFormComponent, canActivate: [authGuard] },
  { path: 'edit/:id', component: StudentFormComponent, canActivate: [authGuard] },
  { path: 'detail/:id', component: StudentDetailComponent, canActivate: [authGuard] },

  // Teacher guest-only routes — redirect to home if already logged in
  { path: 'login', component: LoginFormComponent, canActivate: [guestGuard] },
  { path: 'register', component: SignUpFormComponent, canActivate: [guestGuard] },

  // Student guest-only routes — redirect to dashboard if already logged in
  { path: 'student/login', component: StudentLoginComponent, canActivate: [studentGuestGuard] },
  { path: 'student/activate', redirectTo: 'student/login', pathMatch: 'full' },

  // Student protected routes — require student authentication
  { path: 'student/dashboard', component: StudentDashboardComponent, canActivate: [studentAuthGuard] },

  { path: '**', redirectTo: '' }
];

