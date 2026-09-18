import { Component, OnInit, OnDestroy, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StudentDetailDto, StudentAssessmentDto, CreateStudentAssessmentDto, UpdateStudentAssessmentDto, AssessmentSubmissionDto } from '../../core/models';
import { StudentStateService } from '../../core/services/state';
import { StudentBusinessService } from '../../features/students/services/student-business.service';
import { StudentAssessmentApiService, AssessmentSubmissionApiService, ReportApiService, ClassGroupApiService, ClassGroupDto } from '../../core/services/http';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

/**
 * PRESENTATION LAYER - Student Detail Component
 * Responsible ONLY for UI presentation
 * Delegates all business logic to StudentBusinessService
 * Subscribes to StudentStateService for reactive data
 */

@Component({
  selector: 'app-student-detail',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './student-detail.component.html',

  styleUrls: ['./student-detail.component.css']
})
export class StudentDetailComponent implements OnInit, OnDestroy {
  student: StudentDetailDto | null = null;
  loading = false;
  error: string | null = null;

  assessmentLoading = false;
  assessmentError: string | null = null;
  assessmentSuccess: string | null = null;
  editingId: number | null = null;
  editForm: UpdateStudentAssessmentDto & { dueDate: string | null; isAssigned: boolean; instructions: string | null } = {
    name: '', maxScore: 20, score: 0, dueDate: null, isAssigned: false, instructions: null
  };
  deletingId: number | null = null;
  newAssessment: { name: string; maxScore: number | null; score: number; dueDate: string | null; isAssigned: boolean; instructions: string | null } = {
    name: '',
    maxScore: null,
    score: 0,
    dueDate: null,
    isAssigned: false,
    instructions: null
  };

  // Bulk assign state
  showBulkModal = false;
  classGroups: ClassGroupDto[] = [];
  classGroupsLoading = false;
  selectedClassGroupId = 0;
  bulkLoading = false;
  bulkError: string | null = null;
  bulkSuccess: string | null = null;

  // Submissions panel
  expandedSubmissionsId: number | null = null;
  submissions: AssessmentSubmissionDto[] = [];
  submissionsLoading = false;

  private currentStudentId = 0;
  private successTimer: ReturnType<typeof setTimeout> | null = null;
  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private studentBusiness: StudentBusinessService,
    private studentState: StudentStateService,
    private assessmentApi: StudentAssessmentApiService,
    private submissionApi: AssessmentSubmissionApiService,
    private reportApi: ReportApiService,
    private classGroupApi: ClassGroupApiService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    // Subscribe to reactive state
    this.studentState.selectedStudent$
      .pipe(takeUntil(this.destroy$))
      .subscribe(student => {
        this.student = student;
        this.cdr.markForCheck();
      });
    
    this.studentState.loading$
      .pipe(takeUntil(this.destroy$))
      .subscribe(loading => {
        this.loading = loading;
        this.cdr.markForCheck();
      });
    
    this.studentState.error$
      .pipe(takeUntil(this.destroy$))
      .subscribe(error => {
        this.error = error;
        this.cdr.markForCheck();
      });
    
    // Load student by ID from route params
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.currentStudentId = parseInt(id);
      this.loadStudent(this.currentStudentId);
    } else {
      this.studentState.setError('No student ID provided');
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.studentBusiness.clearSelectedStudent();
    if (this.successTimer) clearTimeout(this.successTimer);
  }

  toggleSubmissions(a: StudentAssessmentDto): void {
    if (this.expandedSubmissionsId === a.id) {
      this.expandedSubmissionsId = null;
      this.submissions = [];
      return;
    }
    this.expandedSubmissionsId = a.id;
    this.submissions = [];
    this.submissionsLoading = true;
    this.submissionApi.getAll(this.currentStudentId, a.id).subscribe({
      next: (subs) => {
        this.submissions = subs;
        this.submissionsLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.submissionsLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  downloadSubmission(a: StudentAssessmentDto, s: AssessmentSubmissionDto): void {
    this.submissionApi.download(this.currentStudentId, a.id, s.id).subscribe(blob => {
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = s.fileName;
      link.click();
      URL.revokeObjectURL(url);
    });
  }

  deleteSubmission(a: StudentAssessmentDto, s: AssessmentSubmissionDto): void {
    this.submissionApi.delete(this.currentStudentId, a.id, s.id).subscribe({
      next: () => {
        this.submissions = this.submissions.filter(x => x.id !== s.id);
        // Decrement count on the live assessment object
        const assessment = this.student?.assessments?.find(x => x.id === a.id);
        if (assessment && assessment.submissionCount > 0) assessment.submissionCount--;
        this.cdr.markForCheck();
      },
      error: () => { /* Ignore silently; file already gone or permission denied */ }
    });
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  loadStudent(id: number): void {
    this.studentBusiness.loadStudentById(id).subscribe();
  }

  addAssessment(): void {
    if (!this.newAssessment.name.trim() || !this.newAssessment.maxScore || this.newAssessment.maxScore <= 0) {
      this.assessmentError = 'Name and Max Score are required (Max Score must be > 0)';
      return;
    }
    if (this.newAssessment.score < 0 || this.newAssessment.score > this.newAssessment.maxScore!) {
      this.assessmentError = 'Score must be between 0 and Max Score';
      return;
    }
    this.assessmentError = null;
    this.assessmentLoading = true;
    this.assessmentApi.create(this.currentStudentId, this.newAssessment as CreateStudentAssessmentDto).subscribe({
      next: () => {
        this.assessmentLoading = false;
        this.newAssessment = { name: '', maxScore: null, score: 0, dueDate: null, isAssigned: false, instructions: null };
        this.showSuccess('Assessment added.');
        this.studentBusiness.loadStudentById(this.currentStudentId).subscribe();
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.assessmentLoading = false;
        this.assessmentError = err.error?.title || 'Failed to add assessment';
        this.cdr.markForCheck();
      }
    });
  }

  confirmDelete(assessmentId: number): void {
    this.deletingId = null;
    this.assessmentLoading = true;
    this.assessmentApi.delete(this.currentStudentId, assessmentId).subscribe({
      next: () => {
        this.assessmentLoading = false;
        this.showSuccess('Assessment deleted.');
        this.studentBusiness.loadStudentById(this.currentStudentId).subscribe();
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.assessmentLoading = false;
        this.assessmentError = err.error?.title || 'Failed to delete assessment';
        this.cdr.markForCheck();
      }
    });
  }

  startEdit(a: StudentAssessmentDto): void {
    this.editingId = a.id;
    this.editForm = {
      name: a.name, maxScore: a.maxScore, score: a.score, dueDate: a.dueDate ?? null,
      isAssigned: a.isAssigned ?? false, instructions: a.instructions ?? null
    };
    this.deletingId = null;
    this.assessmentError = null;
  }

  cancelEdit(): void {
    this.editingId = null;
    this.assessmentError = null;
  }

  saveEdit(assessmentId: number): void {
    if (!this.editForm.name.trim() || this.editForm.maxScore <= 0) {
      this.assessmentError = 'Name and Max Score are required (Max Score must be > 0)';
      return;
    }
    if (this.editForm.score < 0 || this.editForm.score > this.editForm.maxScore) {
      this.assessmentError = 'Score must be between 0 and Max Score';
      return;
    }
    this.assessmentError = null;
    this.assessmentLoading = true;
    this.assessmentApi.update(this.currentStudentId, assessmentId, this.editForm).subscribe({
      next: () => {
        this.assessmentLoading = false;
        this.editingId = null;
        this.showSuccess('Assessment updated.');
        this.studentBusiness.loadStudentById(this.currentStudentId).subscribe();
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.assessmentLoading = false;
        this.assessmentError = err.error?.title || 'Failed to update assessment';
        this.cdr.markForCheck();
      }
    });
  }

  getPerformanceClass(level: string | null | undefined): string {
    if (!level) return '';
    return 'performance-' + level.toLowerCase().replaceAll(' ', '-');
  }

  isPastDue(dueDate: string | null | undefined): boolean {
    if (!dueDate) return false;
    return new Date(dueDate) < new Date();
  }

  exportCsv(): void {
    this.reportApi.exportStudentCsv(this.currentStudentId).subscribe(blob => {
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `student-${this.currentStudentId}-report.csv`;
      a.click();
      URL.revokeObjectURL(url);
    });
  }

  exportPdf(): void {
    this.reportApi.exportStudentPdf(this.currentStudentId).subscribe(blob => {
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `student-${this.currentStudentId}-report.pdf`;
      a.click();
      URL.revokeObjectURL(url);
    });
  }

  private showSuccess(message: string): void {
    this.assessmentSuccess = message;
    if (this.successTimer) clearTimeout(this.successTimer);
    this.successTimer = setTimeout(() => {
      this.assessmentSuccess = null;
      this.cdr.markForCheck();
    }, 3000);
  }

  openBulkModal(): void {
    if (!this.newAssessment.name.trim() || !this.newAssessment.maxScore || this.newAssessment.maxScore <= 0) {
      this.assessmentError = 'Fill in Name and Max Score before bulk assigning.';
      return;
    }
    this.showBulkModal = true;
    this.bulkError = null;
    this.bulkSuccess = null;
    this.selectedClassGroupId = 0;
    if (this.classGroups.length === 0) {
      this.classGroupsLoading = true;
      this.classGroupApi.getAll().pipe(takeUntil(this.destroy$)).subscribe({
        next: groups => { this.classGroups = groups; this.classGroupsLoading = false; this.cdr.markForCheck(); },
        error: () => { this.bulkError = 'Failed to load class groups.'; this.classGroupsLoading = false; this.cdr.markForCheck(); }
      });
    }
  }

  closeBulkModal(): void {
    this.showBulkModal = false;
    this.bulkError = null;
    this.bulkSuccess = null;
  }

  executeBulkAssign(): void {
    if (this.bulkLoading || this.selectedClassGroupId === 0) return;
    const group = this.classGroups.find(g => g.id === this.selectedClassGroupId);
    if (!group || group.students.length === 0) {
      this.bulkError = 'Selected class group has no students.';
      return;
    }
    this.bulkLoading = true; this.bulkError = null;
    const dto = {
      name: this.newAssessment.name,
      maxScore: this.newAssessment.maxScore!,
      score: this.newAssessment.score,
      dueDate: this.newAssessment.dueDate,
      isAssigned: this.newAssessment.isAssigned,
      instructions: this.newAssessment.instructions,
      studentIds: group.students.map(s => s.studentId)
    };
    this.assessmentApi.bulkCreate(dto).pipe(takeUntil(this.destroy$)).subscribe({
      next: results => {
        this.bulkLoading = false;
        this.bulkSuccess = `Assessment assigned to ${results.length} student(s) in ${group.name}.`;
        this.newAssessment = { name: '', maxScore: null, score: 0, dueDate: null, isAssigned: false, instructions: null };
        this.cdr.markForCheck();
        setTimeout(() => this.closeBulkModal(), 2000);
      },
      error: err => {
        this.bulkLoading = false;
        this.bulkError = err?.error?.message || 'Bulk assign failed.';
        this.cdr.markForCheck();
      }
    });
  }
}
