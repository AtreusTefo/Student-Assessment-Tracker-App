import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { TeacherStateService } from '../services/state/teacher-state.service';
import { StudentAuthStateService } from '../services/state/student-auth-state.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const teacherState = inject(TeacherStateService);
  const studentAuthState = inject(StudentAuthStateService);
  const router = inject(Router);

  // Admin token takes priority for /api/admins routes
  const adminToken = localStorage.getItem('admin_token');
  const isAdminRoute = req.url.includes('/api/admins');

  if (isAdminRoute && adminToken) {
    const adminReq = req.clone({ setHeaders: { Authorization: `Bearer ${adminToken}` } });
    return next(adminReq).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          localStorage.removeItem('admin_token');
          localStorage.removeItem('admin_info');
          router.navigate(['/admin/login']);
        }
        return throwError(() => error);
      })
    );
  }

  // Prefer teacher token; fall back to student token for student-facing routes
  const teacherToken = teacherState.getToken();
  const studentToken = studentAuthState.getToken();
  const token = teacherToken ?? studentToken;
  const isStudentToken = !teacherToken && !!studentToken;

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Only auto-redirect when a token was actually sent but the server rejected it
      // (expired or invalidated). If no token was sent, let the component handle the error
      // (e.g. wrong credentials on the login page must not trigger a redirect loop).
      if (error.status === 401 && token) {
        if (isStudentToken) {
          studentAuthState.logout();
          router.navigate(['/student/login']);
        } else {
          teacherState.logout();
          studentAuthState.logout();
          router.navigate(['/login']);
        }
      }
      return throwError(() => error);
    })
  );
};
