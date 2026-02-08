import { Routes } from '@angular/router';
import { StudentListComponent } from './components/student-list.component';
import { StudentDetailComponent } from './components/student-detail.component';
import { StudentFormComponent } from './components/student-form.component';
import { SignUpFormComponent } from './components/signup-form.component';
import { LoginFormComponent } from './components/login-form.component';

export const routes: Routes = [
  { path: '', component: StudentListComponent },
  { path: 'register', component: SignUpFormComponent },
  { path: 'login', component: LoginFormComponent },
  { path: 'create', component: StudentFormComponent },
  { path: 'edit/:id', component: StudentFormComponent },
  { path: 'detail/:id', component: StudentDetailComponent },
  { path: '**', redirectTo: '' }
];
