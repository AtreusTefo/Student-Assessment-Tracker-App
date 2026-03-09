import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TeacherStateService } from '../services/state';

/**
 * AUTH GUARD
 * Protects routes that require an authenticated teacher.
 * Redirects unauthenticated users to /login.
 */
export const authGuard: CanActivateFn = () => {
  const teacherState = inject(TeacherStateService);
  const router = inject(Router);

  if (teacherState.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/login']);
};
