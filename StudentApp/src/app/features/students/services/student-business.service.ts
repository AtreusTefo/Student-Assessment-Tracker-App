import { Injectable } from '@angular/core';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { StudentApiService } from '../../../core/services/http';
import { StudentStateService } from '../../../core/services/state';
import { 
  StudentListDto, 
  StudentDetailDto, 
  CreateStudentDto, 
  UpdateStudentDto 
} from '../../../core/models';

/**
 * BUSINESS LOGIC LAYER - Student Business Service
 * Orchestrates data access and state management
 * Contains business rules and validation logic
 * Coordinates between API layer and State layer
 */
@Injectable({
  providedIn: 'root'
})
export class StudentBusinessService {
  constructor(
    private studentApi: StudentApiService,
    private studentState: StudentStateService
  ) { }

  /**
   * Load all students and update state
   * Business Logic: Fetch data, update state, handle errors
   */
  loadStudents(): Observable<StudentListDto[]> {
    this.studentState.setLoading(true);
    
    return this.studentApi.getAll().pipe(
      tap(students => {
        this.studentState.setStudents(students);
        this.studentState.setLoading(false);
      }),
      catchError(error => {
        const errorMessage = this.extractErrorMessage(error);
        this.studentState.setError(errorMessage);
        return throwError(() => error);
      })
    );
  }

  /**
   * Load a specific student by ID and update state
   * Business Logic: Fetch student detail, update selected student
   */
  loadStudentById(id: number): Observable<StudentDetailDto> {
    this.studentState.setLoading(true);
    
    return this.studentApi.getById(id).pipe(
      tap(student => {
        this.studentState.setSelectedStudent(student);
        this.studentState.setLoading(false);
      }),
      catchError(error => {
        const errorMessage = this.extractErrorMessage(error);
        this.studentState.setError(errorMessage);
        return throwError(() => error);
      })
    );
  }

  /**
   * Create a new student
   * Business Logic: create, update state
   */
  createStudent(studentData: CreateStudentDto): Observable<StudentDetailDto> {
    this.studentState.setLoading(true);

    return this.studentApi.create(studentData).pipe(
      tap(createdStudent => {
        // Add to list state with all fields required by StudentListDto
        const listStudent: StudentListDto = {
          id: createdStudent.id,
          studentUniqueId: createdStudent.studentUniqueId,
          firstName: createdStudent.firstName,
          lastName: createdStudent.lastName,
          email: createdStudent.email,
          gradeName: createdStudent.gradeName,
          totalScore: createdStudent.totalScore,
          maxPossible: createdStudent.maxPossible,
          percentage: createdStudent.percentage,
          performanceLevel: createdStudent.performanceLevel
        };
        this.studentState.addStudent(listStudent);
        this.studentState.setLoading(false);
      }),
      catchError(error => {
        const errorMessage = this.extractErrorMessage(error);
        this.studentState.setError(errorMessage);
        return throwError(() => error);
      })
    );
  }

  /**
   * Update an existing student
   * Business Logic: update, refresh state
   */
  updateStudent(id: number, studentData: UpdateStudentDto): Observable<void> {
    this.studentState.setLoading(true);

    return this.studentApi.update(id, studentData).pipe(
      tap(() => {
        // Reload the full list to keep all StudentListDto fields in sync after an update
        this.loadStudents().subscribe();
        this.studentState.setLoading(false);
      }),
      catchError(error => {
        const errorMessage = this.extractErrorMessage(error);
        this.studentState.setError(errorMessage);
        return throwError(() => error);
      })
    );
  }

  /**
   * Delete a student by ID
   * Business Logic: Delete, update state
   */
  deleteStudent(id: number): Observable<void> {
    this.studentState.setLoading(true);
    
    return this.studentApi.delete(id).pipe(
      tap(() => {
        this.studentState.removeStudent(id);
        this.studentState.setLoading(false);
      }),
      catchError(error => {
        const errorMessage = this.extractErrorMessage(error);
        this.studentState.setError(errorMessage);
        return throwError(() => error);
      })
    );
  }

  /**
   * Business Rule: Calculate performance level
   * This could be used for client-side calculations if needed
   */
  getPerformanceLevel(percentage: number): string {
    if (percentage < 50) return 'Needs Support';
    if (percentage <= 55) return 'Satisfactory';
    if (percentage <= 75) return 'Good';
    return 'Excellent';
  }

  /**
   * Extract user-friendly error message from HTTP error
   */
  private extractErrorMessage(error: any): string {
    if (error.error?.message) {
      return error.error.message;
    }
    if (error.message) {
      return error.message;
    }
    return 'An unexpected error occurred';
  }

  /**
   * Clear selected student from state
   */
  clearSelectedStudent(): void {
    this.studentState.setSelectedStudent(null);
  }

  /**
   * Clear error from state
   */
  clearError(): void {
    this.studentState.clearError();
  }
}
