import { Component, OnInit, OnDestroy, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StudentDetailDto, StudentAssessmentDto, CreateStudentAssessmentDto, UpdateStudentAssessmentDto, AssessmentSubmissionDto } from '../core/models';
import { StudentStateService } from '../core/services/state';
import { StudentBusinessService } from '../features/students/services/student-business.service';
import { StudentAssessmentApiService, AssessmentSubmissionApiService } from '../core/services/http';
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
  template: `
    <div class="container">
      <h2>Student Details</h2>
      
      <div *ngIf="loading" class="loading">Loading student...</div>
      
      <div *ngIf="error" class="error">{{ error }}</div>
      
      <div *ngIf="student && !loading" class="detail-view">
        <h3>{{ student.firstName }} {{ student.lastName }}</h3>
        
        <div class="section">
          <h4>Personal Information</h4>
          <div class="detail-row">
            <label>Student ID:</label>
            <span class="unique-id">{{ student.studentUniqueId }}</span>
          </div>
          <div class="detail-row">
            <label>ID / Passport No.:</label>
            <span>{{ student.idPassportNo }}</span>
          </div>
          <div class="detail-row">
            <label>Email:</label>
            <span>{{ student.email }}</span>
          </div>
          <div class="detail-row">
            <label>Phone:</label>
            <span>{{ student.phone }}</span>
          </div>
          <div class="detail-row">
            <label>Grade:</label>
            <span>{{ student.gradeName }}</span>
          </div>
          <div class="detail-row">
            <label>Created Date:</label>
            <span>{{ student.createdAt | date: 'MM/dd/yyyy' }}</span>
          </div>
        </div>
        
        <div class="section">
          <h4>Assessments</h4>
          <div *ngIf="!student.assessments || student.assessments.length === 0" class="no-assessments">
            No assessments recorded yet.
          </div>
          <table *ngIf="student.assessments && student.assessments.length > 0" class="assessment-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Score</th>
                <th>%</th>
                <th>Due Date</th>
                <th>Assigned</th>
                <th>Submissions</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <ng-container *ngFor="let a of student.assessments">
                <tr *ngIf="editingId !== a.id">
                  <td>{{ a.name }}</td>
                  <td>{{ a.score }} / {{ a.maxScore }}</td>
                  <td>{{ a.maxScore > 0 ? ((a.score / a.maxScore) * 100 | number:'1.1-1') + '%' : '—' }}</td>
                  <td [class.overdue]="isPastDue(a.dueDate)">
                    {{ a.dueDate ? (a.dueDate | date: 'MM/dd/yyyy') : '—' }}
                    <span *ngIf="isPastDue(a.dueDate)" class="overdue-badge">Overdue</span>
                  </td>
                  <td>
                    <span *ngIf="a.isAssigned" class="badge badge-assigned">Yes</span>
                    <span *ngIf="!a.isAssigned" class="badge badge-unassigned">No</span>
                  </td>
                  <td>
                    <button class="btn btn-sm btn-outline" (click)="toggleSubmissions(a)" [disabled]="assessmentLoading">
                      {{ a.submissionCount }} file{{ a.submissionCount !== 1 ? 's' : '' }}
                      <span>{{ expandedSubmissionsId === a.id ? '▲' : '▼' }}</span>
                    </button>
                  </td>
                  <td class="action-btns">
                    <ng-container *ngIf="deletingId !== a.id">
                      <button class="btn btn-sm btn-outline" (click)="startEdit(a)" [disabled]="assessmentLoading">Edit</button>
                      <button class="btn btn-sm btn-danger" (click)="deletingId = a.id" [disabled]="assessmentLoading">Delete</button>
                    </ng-container>
                    <ng-container *ngIf="deletingId === a.id">
                      <span class="confirm-text">Delete?</span>
                      <button class="btn btn-sm btn-danger" (click)="confirmDelete(a.id)" [disabled]="assessmentLoading">Yes</button>
                      <button class="btn btn-sm btn-secondary" (click)="deletingId = null">No</button>
                    </ng-container>
                  </td>
                </tr>
                <!-- Submissions panel (expands below the assessment row) -->
                <tr *ngIf="expandedSubmissionsId === a.id" class="submissions-row">
                  <td colspan="7">
                    <div class="submissions-panel">
                      <div *ngIf="submissionsLoading" class="sub-loading">Loading submissions…</div>
                      <div *ngIf="!submissionsLoading && submissions.length === 0" class="no-submissions">No files submitted yet.</div>
                      <table *ngIf="!submissionsLoading && submissions.length > 0" class="sub-table">
                        <thead><tr><th>File</th><th>Size</th><th>Submitted</th><th></th></tr></thead>
                        <tbody>
                          <tr *ngFor="let s of submissions">
                            <td>{{ s.fileName }}</td>
                            <td>{{ formatFileSize(s.fileSize) }}</td>
                            <td>{{ s.submittedAt | date:'MM/dd/yyyy HH:mm' }}</td>
                            <td>
                              <button class="btn btn-sm btn-outline" (click)="downloadSubmission(a, s)">Download</button>
                              <button class="btn btn-sm btn-danger" (click)="deleteSubmission(a, s)">Delete</button>
                            </td>
                          </tr>
                        </tbody>
                      </table>
                    </div>
                  </td>
                </tr>
                <tr *ngIf="editingId === a.id" class="editing-row">
                  <td><input type="text" [(ngModel)]="editForm.name" maxlength="100" [disabled]="assessmentLoading" class="edit-input" /></td>
                  <td>
                    <input type="number" [(ngModel)]="editForm.score" min="0" [disabled]="assessmentLoading" class="edit-input-sm" />
                    /
                    <input type="number" [(ngModel)]="editForm.maxScore" min="1" [disabled]="assessmentLoading" class="edit-input-sm" />
                  </td>
                  <td>{{ editForm.maxScore > 0 ? ((editForm.score / editForm.maxScore) * 100 | number:'1.1-1') + '%' : '—' }}</td>
                  <td><input type="date" [(ngModel)]="editForm.dueDate" [disabled]="assessmentLoading" class="edit-input" /></td>
                  <td>
                    <label class="checkbox-label">
                      <input type="checkbox" [(ngModel)]="editForm.isAssigned" [disabled]="assessmentLoading" />
                      Assigned
                    </label>
                  </td>
                  <td>—</td>
                  <td class="action-btns">
                    <button class="btn btn-sm btn-primary" (click)="saveEdit(a.id)" [disabled]="assessmentLoading">Save</button>
                    <button class="btn btn-sm btn-secondary" (click)="cancelEdit()" [disabled]="assessmentLoading">Cancel</button>
                  </td>
                </tr>
                <!-- Instructions row shown when editing -->
                <tr *ngIf="editingId === a.id" class="editing-row">
                  <td colspan="7">
                    <textarea [(ngModel)]="editForm.instructions" placeholder="Instructions (optional)" maxlength="2000"
                      [disabled]="assessmentLoading" class="instructions-textarea"></textarea>
                  </td>
                </tr>
              </ng-container>
            </tbody>
          </table>

          <div class="add-assessment">
            <h5>Add Assessment</h5>
            <div *ngIf="assessmentError" class="error small-msg">{{ assessmentError }}</div>
            <div *ngIf="assessmentSuccess" class="success small-msg">{{ assessmentSuccess }}</div>
            <div class="assessment-form">
              <input type="text" [(ngModel)]="newAssessment.name" placeholder="Name (e.g. Test 1)" maxlength="100" [disabled]="assessmentLoading" />
              <input type="number" [(ngModel)]="newAssessment.maxScore" placeholder="Max Score" min="1" [disabled]="assessmentLoading" />
              <input type="number" [(ngModel)]="newAssessment.score" placeholder="Score" min="0" [disabled]="assessmentLoading" />
              <input type="date" [(ngModel)]="newAssessment.dueDate" [disabled]="assessmentLoading" />
              <label class="checkbox-label">
                <input type="checkbox" [(ngModel)]="newAssessment.isAssigned" [disabled]="assessmentLoading" /> Assign to student
              </label>
              <button class="btn btn-primary btn-sm" (click)="addAssessment()" [disabled]="assessmentLoading">Add</button>
            </div>
            <div *ngIf="newAssessment.isAssigned" class="instructions-block">
              <textarea [(ngModel)]="newAssessment.instructions" placeholder="Instructions (optional)" maxlength="2000"
                [disabled]="assessmentLoading" class="instructions-textarea"></textarea>
            </div>
          </div>
        </div>
        
        <div class="section">
          <h4>Performance Summary</h4>
          <div class="detail-row">
            <label>Total Score:</label>
            <span>{{ student.totalScore }} / {{ student.maxPossible }}</span>
          </div>
          <div class="detail-row">
            <label>Average:</label>
            <span>{{ student.averageScore | number: '1.2-2' }}%</span>
          </div>
          <div class="detail-row">
            <label>Percentage:</label>
            <span>{{ student.percentage | number: '1.2-2' }}%</span>
          </div>
          <div class="detail-row">
            <label>Performance Level:</label>
            <span [ngClass]="getPerformanceClass(student.performanceLevel)">
              {{ student.performanceLevel }}
            </span>
          </div>
        </div>
        
        <div class="actions">
          <a [routerLink]="['/edit', student.id]" class="btn btn-warning">Edit</a>
          <a routerLink="/" class="btn btn-secondary">Back to List</a>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .container {
      max-width: 600px;
      margin: 20px auto;
      padding: 20px;
    }
    
    .detail-view {
      background: #f9f9f9;
      padding: 20px;
      border-radius: 8px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }
    
    .section {
      margin: 20px 0;
      padding: 15px;
      background: white;
      border-radius: 4px;
    }
    
    .section h4 {
      margin-top: 0;
      border-bottom: 2px solid #2196F3;
      padding-bottom: 10px;
    }
    
    .detail-row {
      display: flex;
      justify-content: space-between;
      padding: 10px 0;
      border-bottom: 1px solid #eee;
    }
    
    .detail-row:last-child {
      border-bottom: none;
    }
    
    .detail-row label {
      font-weight: bold;
      min-width: 150px;
    }
    
    .unique-id {
      font-family: monospace;
      font-weight: bold;
      background-color: #e3f2fd;
      color: #1565c0;
      padding: 3px 8px;
      border-radius: 4px;
      letter-spacing: 1px;
    }
    
    .performance-excellent {
      background-color: #4caf50;
      color: white;
      padding: 5px 10px;
      border-radius: 4px;
    }
    
    .performance-good {
      background-color: #2196f3;
      color: white;
      padding: 5px 10px;
      border-radius: 4px;
    }
    
    .performance-satisfactory {
      background-color: #ff9800;
      color: white;
      padding: 5px 10px;
      border-radius: 4px;
    }
    
    .performance-needs-support {
      background-color: #f44336;
      color: white;
      padding: 5px 10px;
      border-radius: 4px;
    }
    
    .actions {
      margin-top: 20px;
      text-align: center;
    }
    
    .btn {
      display: inline-block;
      padding: 10px 20px;
      margin: 0 10px;
      border-radius: 4px;
      text-decoration: none;
      cursor: pointer;
      border: none;
    }
    
    .btn-warning {
      background-color: #ff9800;
      color: white;
    }
    
    .btn-secondary {
      background-color: #757575;
      color: white;
    }
    
    .loading, .error {
      padding: 20px;
      margin-top: 20px;
      border-radius: 4px;
      text-align: center;
    }
    
    .loading {
      background-color: #e3f2fd;
      color: #1976d2;
    }
    
    .error {
      background-color: #ffebee;
      color: #c62828;
    }

    .assessment-table {
      width: 100%;
      border-collapse: collapse;
      margin-top: 10px;
    }

    .assessment-table th,
    .assessment-table td {
      padding: 8px 12px;
      text-align: left;
      border-bottom: 1px solid #eee;
    }

    .assessment-table th {
      background-color: #f5f5f5;
      font-weight: bold;
    }

    .no-assessments {
      color: #9e9e9e;
      font-style: italic;
      padding: 10px 0;
    }

    .add-assessment {
      margin-top: 16px;
      padding-top: 12px;
      border-top: 1px dashed #ccc;
    }

    .add-assessment h5 {
      margin: 0 0 8px;
      color: #555;
    }

    .assessment-form {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
      align-items: center;
    }

    .assessment-form input {
      padding: 6px 10px;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 13px;
      flex: 1 1 120px;
    }

    .btn-sm {
      padding: 6px 14px;
      font-size: 13px;
    }

    .btn-danger {
      background-color: #f44336;
      color: white;
    }

    .btn-primary {
      background-color: #2196f3;
      color: white;
    }

    .btn-outline {
      background-color: transparent;
      color: #2196f3;
      border: 1px solid #2196f3;
    }

    .btn-outline:hover {
      background-color: #e3f2fd;
    }

    .success {
      background-color: #e8f5e9;
      color: #2e7d32;
      padding: 20px;
      margin-top: 20px;
      border-radius: 4px;
      text-align: center;
    }

    .small-msg {
      padding: 8px 12px;
      margin-top: 0;
      margin-bottom: 8px;
      text-align: left;
    }

    .editing-row {
      background-color: #fffde7;
    }

    .instructions-textarea {
      width: 100%;
      min-height: 70px;
      padding: 6px 10px;
      border: 1px solid #90caf9;
      border-radius: 3px;
      font-size: 13px;
      box-sizing: border-box;
      resize: vertical;
    }

    .instructions-block {
      margin-top: 8px;
    }

    .badge {
      display: inline-block;
      padding: 2px 8px;
      border-radius: 10px;
      font-size: 11px;
      font-weight: 600;
    }

    .badge-assigned {
      background-color: #e8f5e9;
      color: #2e7d32;
      border: 1px solid #a5d6a7;
    }

    .badge-unassigned {
      background-color: #fafafa;
      color: #9e9e9e;
      border: 1px solid #e0e0e0;
    }

    .submissions-row td {
      padding: 0;
    }

    .submissions-panel {
      padding: 12px 16px;
      background: #f3f7ff;
      border-top: 1px solid #dce8ff;
    }

    .sub-loading {
      color: #1976d2;
      font-style: italic;
      font-size: 13px;
    }

    .no-submissions {
      color: #9e9e9e;
      font-style: italic;
      font-size: 13px;
    }

    .sub-table {
      width: 100%;
      border-collapse: collapse;
      font-size: 13px;
    }

    .sub-table th, .sub-table td {
      padding: 6px 10px;
      border-bottom: 1px solid #dce8ff;
      text-align: left;
    }

    .sub-table th {
      background: #e8f0fe;
      font-weight: 600;
    }

    .checkbox-label {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 13px;
      cursor: pointer;
    }

    .edit-input {
      width: 100%;
      padding: 4px 6px;
      border: 1px solid #90caf9;
      border-radius: 3px;
      font-size: 13px;
      box-sizing: border-box;
    }

    .edit-input-sm {
      width: 55px;
      padding: 4px 6px;
      border: 1px solid #90caf9;
      border-radius: 3px;
      font-size: 13px;
    }

    .action-btns {
      white-space: nowrap;
    }

    .action-btns .btn {
      margin: 0 2px;
    }

    .overdue {
      color: #c62828;
      font-weight: 500;
    }

    .overdue-badge {
      display: inline-block;
      font-size: 10px;
      background-color: #ffebee;
      color: #c62828;
      border: 1px solid #ef9a9a;
      border-radius: 3px;
      padding: 1px 4px;
      margin-left: 4px;
      vertical-align: middle;
    }

    .confirm-text {
      font-size: 12px;
      color: #c62828;
      margin-right: 4px;
      font-weight: bold;
    }

    .performance-no-assessments {
      background-color: #9e9e9e;
      color: white;
      padding: 5px 10px;
      border-radius: 4px;
    }
  `]
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
  newAssessment: { name: string; maxScore: number | null; score: number | null; dueDate: string | null; isAssigned: boolean; instructions: string | null } = {
    name: '',
    maxScore: null,
    score: null,
    dueDate: null,
    isAssigned: false,
    instructions: null
  };

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
    if (this.newAssessment.score === null || this.newAssessment.score < 0 || this.newAssessment.score > this.newAssessment.maxScore!) {
      this.assessmentError = 'Score must be between 0 and Max Score';
      return;
    }
    this.assessmentError = null;
    this.assessmentLoading = true;
    this.assessmentApi.create(this.currentStudentId, this.newAssessment as CreateStudentAssessmentDto).subscribe({
      next: () => {
        this.assessmentLoading = false;
        this.newAssessment = { name: '', maxScore: null, score: null, dueDate: null, isAssigned: false, instructions: null };
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

  private showSuccess(message: string): void {
    this.assessmentSuccess = message;
    if (this.successTimer) clearTimeout(this.successTimer);
    this.successTimer = setTimeout(() => {
      this.assessmentSuccess = null;
      this.cdr.markForCheck();
    }, 3000);
  }
}
