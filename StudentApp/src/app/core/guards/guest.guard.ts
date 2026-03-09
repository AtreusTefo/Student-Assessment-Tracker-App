import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TeacherStateService } from '../services/state';

/**
 * GUEST GUARD
 * Prevents already-authenticated teachers from visiting /login or /register.
 * Redirects them to the student list instead.
 */
export const guestGuard: CanActivateFn = () => {
  const teacherState = inject(TeacherStateService);
  const router = inject(Router);

  if (!teacherState.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/']);
};
