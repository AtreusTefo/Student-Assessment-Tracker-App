import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { StudentAuthStateService } from '../../core/services/state';
import { StudentAuthUser, AssessmentSubmissionDto } from '../../core/models';
import { StudentAuthBusinessService } from '../../features/students/services/student-auth-business.service';
import { AssessmentSubmissionApiService } from '../../core/services/http';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

/**
 * PRESENTATION LAYER - Student Dashboard Component
 * Read-only personal performance view for the logged-in student.
 * Shows: profile info, performance summary cards, progress bar, and assessments table.
 */
@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './student-dashboard.component.html',

  styleUrls: ['./student-dashboard.component.css']
})
export class StudentDashboardComponent implements OnInit, OnDestroy {
  student: StudentAuthUser | null = null;
  private destroy$ = new Subject<void>();

  // Upload modal state
  uploadModalOpen = false;
  uploadModalAssessmentId: number | null = null;
  uploadModalInstructions: string | null = null;
  selectedFile: File | null = null;
  uploadError: string | null = null;
  uploadLoading = false;

  get initials(): string {
    if (!this.student) return '?';
    return `${(this.student.firstName[0] || '').toUpperCase()}${(this.student.lastName[0] || '').toUpperCase()}`;
  }

  get progressWidth(): number {
    return Math.min(this.student?.percentage ?? 0, 100);
  }

  get progressClass(): string {
    const pct = this.student?.percentage ?? 0;
    if (pct < 50) return 'fill-danger';
    if (pct <= 55) return 'fill-warning';
    if (pct <= 75) return 'fill-info';
    return 'fill-success';
  }

  get performanceCardClass(): string {
    const level = this.student?.performanceLevel ?? '';
    if (level === 'Excellent') return 'card card-excellent';
    if (level === 'Good') return 'card card-good';
    if (level === 'Satisfactory') return 'card card-satisfactory';
    return 'card card-needs-support';
  }

  constructor(
    private router: Router,
    private studentAuthState: StudentAuthStateService,
    private studentAuthBusiness: StudentAuthBusinessService,
    private submissionApi: AssessmentSubmissionApiService
  ) { }

  ngOnInit(): void {
    this.studentAuthState.currentStudent$.pipe(takeUntil(this.destroy$)).subscribe(s => {
      this.student = s;
      if (!s) {
        this.router.navigate(['/student/login']);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  logout(): void {
    this.studentAuthBusiness.logout();
    this.router.navigate(['/student/login']);
  }

  getAssessmentPct(score: number, maxScore: number): number {
    return maxScore === 0 ? 0 : Math.round((score / maxScore) * 10000) / 100;
  }

  getPctClass(score: number, maxScore: number): string {
    const pct = this.getAssessmentPct(score, maxScore);
    if (pct < 50) return 'pct-badge pct-danger';
    if (pct <= 55) return 'pct-badge pct-warning';
    if (pct <= 75) return 'pct-badge pct-info';
    return 'pct-badge pct-success';
  }

  isOverdue(dueDate: string | null): boolean {
    if (!dueDate) return false;
    return new Date(dueDate) < new Date();
  }

  openUploadModal(assessmentId: number): void {
    const a = this.student?.assessments?.find(x => x.id === assessmentId);
    this.uploadModalAssessmentId = assessmentId;
    this.uploadModalInstructions = a?.instructions ?? null;
    this.selectedFile = null;
    this.uploadError = null;
    this.uploadLoading = false;
    this.uploadModalOpen = true;
  }

  closeUploadModal(): void {
    this.uploadModalOpen = false;
    this.uploadModalAssessmentId = null;
    this.selectedFile = null;
    this.uploadError = null;
    this.uploadLoading = false;
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;
    const file = input.files[0];
    const allowed = ['application/pdf', 'application/msword',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
      'image/jpeg', 'image/png'];
    if (!allowed.includes(file.type)) {
      this.uploadError = 'Only PDF, Word (.doc/.docx) and image (.jpg/.png) files are allowed.';
      this.selectedFile = null;
      return;
    }
    if (file.size > 10 * 1024 * 1024) {
      this.uploadError = 'File size must not exceed 10 MB.';
      this.selectedFile = null;
      return;
    }
    this.uploadError = null;
    this.selectedFile = file;
  }

  submitFile(): void {
    if (!this.selectedFile || !this.uploadModalAssessmentId || !this.student) return;
    this.uploadLoading = true;
    this.uploadError = null;
    this.submissionApi.upload(this.student.id, this.uploadModalAssessmentId, this.selectedFile).subscribe({
      next: () => {
        // Increment the in-memory submission count so the UI reflects the upload
        const a = this.student?.assessments?.find(x => x.id === this.uploadModalAssessmentId);
        if (a) a.submissionCount++;
        this.closeUploadModal();
      },
      error: (err) => {
        this.uploadLoading = false;
        this.uploadError = err.error?.title || err.error?.message || 'Upload failed. Please try again.';
      }
    });
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }
}
