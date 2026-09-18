import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { CreateStudentDto, UpdateStudentDto, GradeDto } from '../../core/models';
import { StudentStateService, TeacherStateService } from '../../core/services/state';
import { StudentBusinessService } from '../../features/students/services/student-business.service';
import { GradeApiService } from '../../core/services/http';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

/**
 * PRESENTATION LAYER - Student Form Component
 * Responsible ONLY for UI presentation and form handling
 * Delegates all business logic to StudentBusinessService
 * Subscribes to StudentStateService for reactive data
 */

@Component({
  selector: 'app-student-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './student-form.component.html',

  styleUrls: ['./student-form.component.css']
})
export class StudentFormComponent implements OnInit, OnDestroy {
  student = {
    studentId: 0,
    idPassportNo: '',
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    gradeId: 0,
    enrollmentDate: new Date().toISOString(),
    createdDate: new Date().toISOString()
  };

  grades: GradeDto[] = [];
  private teacherId = 0;
  
  isEdit = false;
  loading = false;
  error: string | null = null;
  isServerError = false;
  fieldErrors: Record<string, string[]> = {};
  
  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private studentBusiness: StudentBusinessService,
    private studentState: StudentStateService,
    private teacherState: TeacherStateService,
    private gradeApi: GradeApiService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    // Subscribe to reactive state
    this.studentState.loading$
      .pipe(takeUntil(this.destroy$))
      .subscribe(loading => {
        this.loading = loading;
        this.cdr.markForCheck();
      });
    
    this.studentState.error$
      .pipe(takeUntil(this.destroy$))
      .subscribe(error => {
        if (error) {
          this.error = error;
          this.isServerError = true;
        }
        this.cdr.markForCheck();
      });
    
    this.studentState.selectedStudent$
      .pipe(takeUntil(this.destroy$))
      .subscribe(student => {
        if (student && this.isEdit) {
          // Parse phone number: strip "+267 " prefix if present
          let parsedPhone = '';
          if (student.phone) {
            parsedPhone = student.phone.startsWith('+267 ') 
              ? student.phone.substring(5) 
              : student.phone;
          }
          
          this.student = {
            studentId: student.id,
            idPassportNo: student.idPassportNo || '',
            firstName: student.firstName || '',
            lastName: student.lastName || '',
            email: student.email || '',
            phone: parsedPhone,
            gradeId: student.gradeId || 0,
            enrollmentDate: student.createdAt || new Date().toISOString(),
            createdDate: student.createdAt || new Date().toISOString()
          };
          this.cdr.markForCheck();
        }
      });
    
    // Load student if editing
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.studentBusiness.loadStudentById(parseInt(id)).subscribe();
    }

    // Get current teacher id synchronously
    const teacher = this.teacherState.getCurrentTeacher();
    this.teacherId = teacher?.id ?? 0;

    // Load grade options
    this.gradeApi.getAll().subscribe(grades => {
      this.grades = grades;
      this.cdr.markForCheck();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.isEdit) {
      this.studentBusiness.clearSelectedStudent();
    }
  }

  validatePhone(): void {
    if (this.student.phone) {
      // Strip any non-numeric characters (just in case)
      this.student.phone = this.student.phone.replace(/[^0-9]/g, '');
      // Limit to 8 characters
      if (this.student.phone.length > 8) {
        this.student.phone = this.student.phone.substring(0, 8);
      }
    }
  }

  allowOnlyNumbers(event: KeyboardEvent): void {
    const char = String.fromCharCode(event.which);
    if (!/[0-9]/.test(char)) {
      event.preventDefault();
    }
  }

  allowOnlyLetters(event: KeyboardEvent): void {
    const char = String.fromCharCode(event.which);
    if (!/[a-zA-Z\s\-]/.test(char)) {
      event.preventDefault();
    }
  }

  private isValidationErrorResponse(err: any): boolean {
    // Check if this is a validation error response from the server
    // ProblemDetails format will have status 400 and an errors object with field names as keys
    if (err?.status !== 400) {
      return false;
    }
    
    const errors = err?.error?.errors;
    if (!errors || typeof errors !== 'object') {
      return false;
    }
    
    // Check if errors object has at least one field-level error
    // (field validation errors have property names as keys with array values)
    return Object.keys(errors).length > 0;
  }

  private normalizeValidationErrors(errors: Record<string, string[]>): Record<string, string[]> {
    const normalized: Record<string, string[]> = {};

    Object.entries(errors).forEach(([key, messages]) => {
      const lastSegment = key.split('.').pop() ?? key;
      const normalizedKey = lastSegment.charAt(0).toLowerCase() + lastSegment.slice(1);
      normalized[normalizedKey] = messages;
    });

    return normalized;
  }

  private handleServerError(action: 'create' | 'update', err: any): void {
    this.loading = false;

    if (this.isValidationErrorResponse(err)) {
      // Surface FluentValidation errors inline by field.
      this.fieldErrors = this.normalizeValidationErrors(err.error.errors);
      this.isServerError = false;
      this.error = null;
      this.cdr.markForCheck();
      return;
    }

    this.isServerError = true;
    if (err.error && err.error.errors) {
      const errorMessages = Object.values(err.error.errors)
        .flat()
        .join('\n');
      this.error = errorMessages as string;
    } else {
      this.error = `Failed to ${action} student: ` + (err.error?.title || err.message);
    }
    this.cdr.markForCheck();
  }


  onSubmit(form: NgForm): void {
    this.error = null;
    this.isServerError = false;
    this.fieldErrors = {};

    if (form.invalid || this.student.gradeId === 0) {
      if (this.student.gradeId === 0) {
        this.fieldErrors['gradeId'] = ['Please select a grade'];
      }
      return;
    }

    if (this.isEdit) {
      const updateDto: UpdateStudentDto = {
        idPassportNo: this.student.idPassportNo,
        firstName: this.student.firstName,
        lastName: this.student.lastName,
        email: this.student.email,
        phone: this.student.phone,
        gradeId: this.student.gradeId
      };
      
      this.studentBusiness.updateStudent(this.student.studentId, updateDto).subscribe({
        next: () => {
          this.router.navigate(['/detail', this.student.studentId]);
        },
        error: (err) => this.handleServerError('update', err)
      });
    } else {
      const createDto: CreateStudentDto = {
        idPassportNo: this.student.idPassportNo,
        firstName: this.student.firstName,
        lastName: this.student.lastName,
        email: this.student.email,
        phone: this.student.phone,
        gradeId: this.student.gradeId
      };
      
      this.studentBusiness.createStudent(createDto).subscribe({
        next: () => {
          this.router.navigate(['/']);
        },
        error: (err) => this.handleServerError('create', err)
      });
    }
  }
}
