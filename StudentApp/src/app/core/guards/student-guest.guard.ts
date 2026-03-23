import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { StudentAuthStateService } from '../services/state';

/**
 * STUDENT GUEST GUARD
 * Prevents authenticated students from visiting /student/login or /student/activate.
 * Redirects them to the dashboard instead.
 */
export const studentGuestGuard: CanActivateFn = () => {
  const studentAuthState = inject(StudentAuthStateService);
  const router = inject(Router);

  if (!studentAuthState.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/student/dashboard']);
};
