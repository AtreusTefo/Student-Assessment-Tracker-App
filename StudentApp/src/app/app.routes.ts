import { Routes } from '@angular/router';
import { StudentListComponent } from './components/student-list.component';
import { StudentDetailComponent } from './components/student-detail.component';
import { StudentFormComponent } from './components/student-form.component';

export const routes: Routes = [
  { path: '', component: StudentListComponent },
  { path: 'create', component: StudentFormComponent },
  { path: 'edit/:id', component: StudentFormComponent },
  { path: 'detail/:id', component: StudentDetailComponent },
  { path: '**', redirectTo: '' }
];
