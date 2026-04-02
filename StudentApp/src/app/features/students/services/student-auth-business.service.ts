import { Injectable } from '@angular/core';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { StudentApiService } from '../../../core/services/http';
import { StudentAuthStateService, TeacherStateService } from '../../../core/services/state';
import { StudentActivateDto, StudentLoginDto, StudentLoginResponseDto } from '../../../core/models';

/**
 * BUSINESS LOGIC LAYER - Student Auth Business Service
 * Orchestrates student sign-up (activation) and login.
 * Mirrors the pattern used by TeacherBusinessService.
 */
@Injectable({
  providedIn: 'root'
})
export class StudentAuthBusinessService {
  constructor(
    private studentApi: StudentApiService,
    private studentAuthState: StudentAuthStateService,
    private teacherState: TeacherStateService
  ) { }

  /**
   * Activate a student account (first-time sign-up).
   * The student provides their teacher-assigned StudentUniqueId and email to prove identity,
   * then chooses a password for future logins.
   */
  activate(dto: StudentActivateDto): Observable<StudentLoginResponseDto> {
    this.studentAuthState.setLoading(true);

    return this.studentApi.activate(dto).pipe(
      tap(response => {
        this.teacherState.logout(); // Clear any active teacher session
        this.studentAuthState.setToken(response.token);
        this.studentAuthState.setCurrentStudent(response.student);
        this.studentAuthState.setLoading(false);
      }),
      catchError(error => {
        const message = this.extractErrorMessage(error);
        this.studentAuthState.setError(message);
        return throwError(() => error);
      })
    );
  }

  /**
   * Authenticate a student using their StudentUniqueId and password.
   */
  login(dto: StudentLoginDto): Observable<StudentLoginResponseDto> {
    this.studentAuthState.setLoading(true);

    return this.studentApi.loginStudent(dto).pipe(
      tap(response => {
        this.teacherState.logout(); // Clear any active teacher session
        this.studentAuthState.setToken(response.token);
        this.studentAuthState.setCurrentStudent(response.student);
        this.studentAuthState.setLoading(false);
      }),
      catchError(error => {
        const message = this.extractErrorMessage(error);
        this.studentAuthState.setError(message);
        return throwError(() => error);
      })
    );
  }

  /**
   * Logout the current student.
   */
  logout(): void {
    this.studentAuthState.logout();
  }

  clearError(): void {
    this.studentAuthState.clearError();
  }

  private extractErrorMessage(error: any): string {
    if (error?.error?.message) return error.error.message;
    if (error?.message) return error.message;
    return 'An unexpected error occurred';
  }
}
