import { Routes } from '@angular/router';
import { StudentListComponent } from './components/student-list.component';
import { StudentDetailComponent } from './components/student-detail.component';
import { StudentFormComponent } from './components/student-form.component';
import { SignUpFormComponent } from './components/signup-form.component';
import { LoginFormComponent } from './components/login-form.component';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';

export const routes: Routes = [
  // Protected routes — require authentication
  { path: '', component: StudentListComponent, canActivate: [authGuard] },
  { path: 'create', component: StudentFormComponent, canActivate: [authGuard] },
  { path: 'edit/:id', component: StudentFormComponent, canActivate: [authGuard] },
  { path: 'detail/:id', component: StudentDetailComponent, canActivate: [authGuard] },

  // Guest-only routes — redirect to home if already logged in
  { path: 'login', component: LoginFormComponent, canActivate: [guestGuard] },
  { path: 'register', component: SignUpFormComponent, canActivate: [guestGuard] },

  { path: '**', redirectTo: '' }
];

