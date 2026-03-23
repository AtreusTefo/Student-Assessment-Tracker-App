import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { StudentAuthStateService } from '../services/state';

/**
 * STUDENT AUTH GUARD
 * Protects routes that require an authenticated student.
 * Redirects unauthenticated students to /student/login.
 */
export const studentAuthGuard: CanActivateFn = () => {
  const studentAuthState = inject(StudentAuthStateService);
  const router = inject(Router);

  if (studentAuthState.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/student/login']);
};
