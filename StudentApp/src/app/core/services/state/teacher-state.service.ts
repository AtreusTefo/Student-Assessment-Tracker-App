import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Teacher } from '../../models';

const AUTH_STORAGE_KEY = 'sat_current_teacher';
const TOKEN_STORAGE_KEY = 'sat_teacher_token';

/**
 * STATE MANAGEMENT LAYER - Teacher State Service
 * Centralized state management for teacher/authentication data
 * Uses RxJS BehaviorSubject for reactive state
 * Auth is persisted to localStorage so page refresh preserves the session
 */
@Injectable({
  providedIn: 'root'
})
export class TeacherStateService {
  // Restore session from localStorage on service init
  private restoredTeacher: Teacher | null = (() => {
    try {
      const raw = localStorage.getItem(AUTH_STORAGE_KEY);
      return raw ? (JSON.parse(raw) as Teacher) : null;
    } catch {
      return null;
    }
  })();

  // Private state
  private currentTeacherSubject = new BehaviorSubject<Teacher | null>(this.restoredTeacher);
  private teachersSubject = new BehaviorSubject<Teacher[]>([]);
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(!!this.restoredTeacher);
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
   * Persists to localStorage so the session survives page refresh
   * @param teacher - Authenticated teacher data
   */
  setCurrentTeacher(teacher: Teacher | null): void {
    if (teacher) {
      localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(teacher));
    } else {
      localStorage.removeItem(AUTH_STORAGE_KEY);
    }
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
   * Logout - clear authentication state and localStorage
   */
  logout(): void {
    localStorage.removeItem(AUTH_STORAGE_KEY);
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    this.currentTeacherSubject.next(null);
    this.isAuthenticatedSubject.next(false);
    this.clearError();
  }

  /**
   * Clear all state
   */
  clearState(): void {
    localStorage.removeItem(AUTH_STORAGE_KEY);
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    this.currentTeacherSubject.next(null);
    this.teachersSubject.next([]);
    this.isAuthenticatedSubject.next(false);
    this.loadingSubject.next(false);
    this.errorSubject.next(null);
  }

  setToken(token: string): void {
    localStorage.setItem(TOKEN_STORAGE_KEY, token);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_STORAGE_KEY);
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
