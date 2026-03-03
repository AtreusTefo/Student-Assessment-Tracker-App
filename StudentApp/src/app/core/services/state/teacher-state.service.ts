import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Teacher } from '../../models';

/**
 * STATE MANAGEMENT LAYER - Teacher State Service
 * Centralized state management for teacher/authentication data
 * Uses RxJS BehaviorSubject for reactive state
 */
@Injectable({
  providedIn: 'root'
})
export class TeacherStateService {
  // Private state
  private currentTeacherSubject = new BehaviorSubject<Teacher | null>(null);
  private teachersSubject = new BehaviorSubject<Teacher[]>([]);
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private errorSubject = new BehaviorSubject<string | null>(null);

  // Public observables
  public currentTeacher$ = this.currentTeacherSubject.asObservable();
  public teachers$ = this.teachersSubject.asObservable();
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();
  public loading$ = this.loadingSubject.asObservable();
  public error$ = this.errorSubject.asObservable();

  /**
   * Set the currently authenticated teacher
   * @param teacher - Authenticated teacher data
   */
  setCurrentTeacher(teacher: Teacher | null): void {
    this.currentTeacherSubject.next(teacher);
    this.isAuthenticatedSubject.next(!!teacher);
    this.clearError();
  }

  /**
   * Set the list of teachers
   * @param teachers - Array of teachers
   */
  setTeachers(teachers: Teacher[]): void {
    this.teachersSubject.next(teachers);
    this.clearError();
  }

  /**
   * Set loading state
   * @param isLoading - Loading status
   */
  setLoading(isLoading: boolean): void {
    this.loadingSubject.next(isLoading);
  }

  /**
   * Set error message
   * @param error - Error message
   */
  setError(error: string): void {
    this.errorSubject.next(error);
    this.setLoading(false);
  }

  /**
   * Clear error message
   */
  clearError(): void {
    this.errorSubject.next(null);
  }

  /**
   * Logout - clear authentication state
   */
  logout(): void {
    this.currentTeacherSubject.next(null);
    this.isAuthenticatedSubject.next(false);
    this.clearError();
  }

  /**
   * Clear all state
   */
  clearState(): void {
    this.currentTeacherSubject.next(null);
    this.teachersSubject.next([]);
    this.isAuthenticatedSubject.next(false);
    this.loadingSubject.next(false);
    this.errorSubject.next(null);
  }

  /**
   * Get current teacher value (synchronous)
   */
  getCurrentTeacher(): Teacher | null {
    return this.currentTeacherSubject.value;
  }

  /**
   * Check if user is authenticated (synchronous)
   */
  isAuthenticated(): boolean {
    return this.isAuthenticatedSubject.value;
  }
}
