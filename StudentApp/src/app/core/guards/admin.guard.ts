import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

/**
 * Admin Auth Guard — protects routes that require an authenticated admin.
 * Redirects to /admin/login if no admin token is found in localStorage.
 */
export const adminAuthGuard: CanActivateFn = () => {
  const router = inject(Router);
  const token = localStorage.getItem('admin_token');
  return token ? true : router.createUrlTree(['/admin/login']);
};

/**
 * Admin Guest Guard — redirects to admin dashboard if already logged in.
 */
export const adminGuestGuard: CanActivateFn = () => {
  const router = inject(Router);
  const token = localStorage.getItem('admin_token');
  return token ? router.createUrlTree(['/admin/dashboard']) : true;
};
