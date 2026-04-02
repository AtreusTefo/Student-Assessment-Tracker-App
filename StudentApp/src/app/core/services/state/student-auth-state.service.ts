import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { StudentAuthUser } from '../../models';

const STUDENT_AUTH_KEY = 'sat_current_student';
const STUDENT_TOKEN_KEY = 'sat_student_token';

/**
 * STATE MANAGEMENT LAYER - Student Auth State Service
 * Manages the logged-in student session.
 * Mirrors the pattern of TeacherStateService.
 * Session is persisted to localStorage so page refresh preserves the login.
 */
@Injectable({
  providedIn: 'root'
})
export class StudentAuthStateService {
  private restoredStudent: StudentAuthUser | null = (() => {
    try {
      const raw = localStorage.getItem(STUDENT_AUTH_KEY);
      const token = localStorage.getItem(STUDENT_TOKEN_KEY);
      // Only restore if both the profile and token are present
      return raw && token ? (JSON.parse(raw) as StudentAuthUser) : null;
    } catch {
      return null;
    }
  })();

  // Private state
  private currentStudentSubject = new BehaviorSubject<StudentAuthUser | null>(this.restoredStudent);
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(!!this.restoredStudent);
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private errorSubject = new BehaviorSubject<string | null>(null);

  // Public observables
  public currentStudent$ = this.currentStudentSubject.asObservable();
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();
  public loading$ = this.loadingSubject.asObservable();
  public error$ = this.errorSubject.asObservable();

  setCurrentStudent(student: StudentAuthUser | null): void {
    if (student) {
      localStorage.setItem(STUDENT_AUTH_KEY, JSON.stringify(student));
    } else {
      localStorage.removeItem(STUDENT_AUTH_KEY);
    }
    this.currentStudentSubject.next(student);
    this.isAuthenticatedSubject.next(!!student);
    this.clearError();
  }

  setLoading(isLoading: boolean): void {
    this.loadingSubject.next(isLoading);
  }

  setError(error: string): void {
    this.errorSubject.next(error);
    this.setLoading(false);
  }

  clearError(): void {
    this.errorSubject.next(null);
  }

  logout(): void {
    localStorage.removeItem(STUDENT_AUTH_KEY);
    localStorage.removeItem(STUDENT_TOKEN_KEY);
    this.currentStudentSubject.next(null);
    this.isAuthenticatedSubject.next(false);
    this.clearError();
  }

  getCurrentStudent(): StudentAuthUser | null {
    return this.currentStudentSubject.value;
  }

  isAuthenticated(): boolean {
    return this.isAuthenticatedSubject.value;
  }

  setToken(token: string): void {
    localStorage.setItem(STUDENT_TOKEN_KEY, token);
  }

  getToken(): string | null {
    return localStorage.getItem(STUDENT_TOKEN_KEY);
  }
}
