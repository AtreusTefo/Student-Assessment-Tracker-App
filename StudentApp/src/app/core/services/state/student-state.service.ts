import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { StudentListDto, StudentDetailDto } from '../../models';

/**
 * STATE MANAGEMENT LAYER - Student State Service
 * Centralized state management for student data
 * Uses RxJS BehaviorSubject for reactive state
 * Components subscribe to state changes
 */
@Injectable({
  providedIn: 'root'
})
export class StudentStateService {
  // Private state
  private studentsSubject = new BehaviorSubject<StudentListDto[]>([]);
  private selectedStudentSubject = new BehaviorSubject<StudentDetailDto | null>(null);
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private errorSubject = new BehaviorSubject<string | null>(null);

  // Public observables
  public students$ = this.studentsSubject.asObservable();
  public selectedStudent$ = this.selectedStudentSubject.asObservable();
  public loading$ = this.loadingSubject.asObservable();
  public error$ = this.errorSubject.asObservable();

  /**
   * Set the list of students in state
   * @param students - Array of students
   */
  setStudents(students: StudentListDto[]): void {
    this.studentsSubject.next(students);
    this.clearError();
  }

  /**
   * Set the selected student in state
   * @param student - Student detail data
   */
  setSelectedStudent(student: StudentDetailDto | null): void {
    this.selectedStudentSubject.next(student);
    this.clearError();
  }

  /**
   * Add a new student to the list
   * @param student - Student to add
   */
  addStudent(student: StudentListDto): void {
    const currentStudents = this.studentsSubject.value;
    this.studentsSubject.next([...currentStudents, student]);
    this.clearError();
  }

  /**
   * Update a student in the list
   * @param id - Student ID
   * @param updatedStudent - Updated student data
   */
  updateStudent(id: number, updatedStudent: Partial<StudentListDto>): void {
    const currentStudents = this.studentsSubject.value;
    const updatedStudents = currentStudents.map(student =>
      student.id === id ? { ...student, ...updatedStudent } : student
    );
    this.studentsSubject.next(updatedStudents);
    this.clearError();
  }

  /**
   * Remove a student from the list
   * @param id - Student ID to remove
   */
  removeStudent(id: number): void {
    const currentStudents = this.studentsSubject.value;
    const filteredStudents = currentStudents.filter(student => student.id !== id);
    this.studentsSubject.next(filteredStudents);
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
   * Clear all state
   */
  clearState(): void {
    this.studentsSubject.next([]);
    this.selectedStudentSubject.next(null);
    this.loadingSubject.next(false);
    this.errorSubject.next(null);
  }

  /**
   * Get current students value (synchronous)
   */
  getCurrentStudents(): StudentListDto[] {
    return this.studentsSubject.value;
  }

  /**
   * Get current selected student value (synchronous)
   */
  getCurrentSelectedStudent(): StudentDetailDto | null {
    return this.selectedStudentSubject.value;
  }
}
