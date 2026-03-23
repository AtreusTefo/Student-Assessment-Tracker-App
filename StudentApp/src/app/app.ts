import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, Router } from '@angular/router';
import { TeacherStateService } from './core/services/state';
import { StudentAuthStateService } from './core/services/state';
import { TeacherBusinessService } from './features/teachers/services/teacher-business.service';
import { StudentAuthBusinessService } from './features/students/services/student-auth-business.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit, OnDestroy {
  title = 'Student Assessment Tracker';
  isAuthenticated = false;
  teacherName = '';
  isStudentAuthenticated = false;
  studentName = '';

  private destroy$ = new Subject<void>();
  private router = inject(Router);
  private teacherState = inject(TeacherStateService);
  private teacherBusiness = inject(TeacherBusinessService);
  private studentAuthState = inject(StudentAuthStateService);
  private studentAuthBusiness = inject(StudentAuthBusinessService);

  ngOnInit(): void {
    this.teacherState.isAuthenticated$
      .pipe(takeUntil(this.destroy$))
      .subscribe(isAuth => {
        this.isAuthenticated = isAuth;
      });

    this.teacherState.currentTeacher$
      .pipe(takeUntil(this.destroy$))
      .subscribe(teacher => {
        this.teacherName = teacher ? `${teacher.firstName} ${teacher.lastName}` : '';
      });

    this.studentAuthState.isAuthenticated$
      .pipe(takeUntil(this.destroy$))
      .subscribe(isAuth => {
        this.isStudentAuthenticated = isAuth;
      });

    this.studentAuthState.currentStudent$
      .pipe(takeUntil(this.destroy$))
      .subscribe(student => {
        this.studentName = student ? `${student.firstName} ${student.lastName}` : '';
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  logout(): void {
    this.teacherBusiness.logout();
    this.router.navigate(['/login']);
  }

  studentLogout(): void {
    this.studentAuthBusiness.logout();
    this.router.navigate(['/student/login']);
  }

  navigateToLogin() {
    this.router.navigate(['/login']);
  }

  navigateToSignUp() {
    this.router.navigate(['/register']);
  }

  navigateToList() {
    this.router.navigate(['/']);
  }

  navigateToCreate() {
    this.router.navigate(['/create']);
  }

  navigateToStudentLogin() {
    this.router.navigate(['/student/login']);
  }

  navigateToStudentActivate() {
    this.router.navigate(['/student/activate']);
  }

  navigateToStudentDashboard() {
    this.router.navigate(['/student/dashboard']);
  }
}
