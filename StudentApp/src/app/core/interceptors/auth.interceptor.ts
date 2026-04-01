import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TeacherStateService } from '../services/state/teacher-state.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const teacherState = inject(TeacherStateService);
  const token = teacherState.getToken();

  if (token) {
    return next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
  }

  return next(req);
};
