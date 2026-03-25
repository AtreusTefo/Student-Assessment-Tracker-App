import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { StudentAuthStateService } from '../core/services/state';
import { StudentAuthUser } from '../core/models';
import { StudentAuthBusinessService } from '../features/students/services/student-auth-business.service';
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
  imports: [CommonModule],
  template: `
    <div class="dashboard" *ngIf="student; else loading">

      <!-- ── Header ──────────────────────────────────────────────────────── -->
      <div class="dash-header">
        <div class="dash-header-left">
          <div class="avatar">{{ initials }}</div>
          <div>
            <h1 class="student-name">{{ student.firstName }} {{ student.lastName }}</h1>
            <div class="student-meta">
              <span class="badge badge-id">{{ student.studentUniqueId }}</span>
              <span class="badge badge-grade">{{ student.gradeName }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- ── Summary Cards ────────────────────────────────────────────────── -->
      <div class="cards-grid">
        <div class="card card-score">
          <div class="card-icon">📊</div>
          <div class="card-body">
            <div class="card-value">{{ student.totalScore | number:'1.0-1' }} / {{ student.maxPossible | number:'1.0-1' }}</div>
            <div class="card-label">Total Score</div>
          </div>
        </div>

        <div class="card card-avg">
          <div class="card-icon">📈</div>
          <div class="card-body">
            <div class="card-value">{{ student.averageScore | number:'1.0-1' }}%</div>
            <div class="card-label">Average Score</div>
          </div>
        </div>

        <div class="card card-pct">
          <div class="card-icon">🎯</div>
          <div class="card-body">
            <div class="card-value">{{ student.percentage | number:'1.0-1' }}%</div>
            <div class="card-label">Overall Percentage</div>
          </div>
        </div>

        <div class="card" [ngClass]="performanceCardClass">
          <div class="card-icon">🏆</div>
          <div class="card-body">
            <div class="card-value">{{ student.performanceLevel }}</div>
            <div class="card-label">Performance Level</div>
          </div>
        </div>
      </div>

      <!-- ── Progress Bar ──────────────────────────────────────────────────── -->
      <div class="progress-section">
        <div class="progress-header">
          <span class="progress-label">Overall Progress</span>
          <span class="progress-pct">{{ student.percentage | number:'1.0-1' }}%</span>
        </div>
        <div class="progress-track">
          <div
            class="progress-fill"
            [style.width.%]="progressWidth"
            [ngClass]="progressClass"
          ></div>
        </div>
        <div class="progress-legend">
          <span class="legend-item danger">Needs Support (&lt;50%)</span>
          <span class="legend-item warning">Satisfactory (50-55%)</span>
          <span class="legend-item info">Good (56-75%)</span>
          <span class="legend-item success">Excellent (&gt;75%)</span>
        </div>
      </div>

      <!-- ── Assessments Table ─────────────────────────────────────────────── -->
      <div class="table-section">
        <h2 class="section-title">My Assessments</h2>

        <div *ngIf="student.assessments.length === 0" class="empty-state">
          <span class="empty-icon">📋</span>
          <p>No assessments recorded yet. Your teacher will add them soon.</p>
        </div>

        <div class="table-responsive" *ngIf="student.assessments.length > 0">
          <table class="assessment-table">
            <thead>
              <tr>
                <th>#</th>
                <th>Assessment</th>
                <th>Score</th>
                <th>Max Score</th>
                <th>Percentage</th>
                <th>Due Date</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let a of student.assessments; let i = index">
                <td>{{ i + 1 }}</td>
                <td class="assessment-name">{{ a.name }}</td>
                <td><strong>{{ a.score | number:'1.0-1' }}</strong></td>
                <td>{{ a.maxScore | number:'1.0-1' }}</td>
                <td>
                  <span class="pct-badge" [ngClass]="getPctClass(a.score, a.maxScore)">
                    {{ getAssessmentPct(a.score, a.maxScore) | number:'1.0-1' }}%
                  </span>
                </td>
                <td>{{ a.dueDate ? (a.dueDate | date:'mediumDate') : '—' }}</td>
                <td>
                  <span *ngIf="isOverdue(a.dueDate)" class="status-badge overdue">Overdue</span>
                  <span *ngIf="!isOverdue(a.dueDate)" class="status-badge submitted">Submitted</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- ── My Profile ──────────────────────────────────────────────────── -->
      <div class="profile-card">

        <div class="profile-card-header">
          <h2 class="section-title">My Profile</h2>
          <span class="profile-status-badge">
            <span class="status-dot"></span> Active
          </span>
        </div>

        <div class="profile-layout">

          <!-- Identity Panel -->
          <div class="profile-identity">
            <div class="profile-avatar-lg">{{ initials }}</div>
            <div class="profile-name-block">
              <div class="profile-full-name">{{ student.firstName }} {{ student.lastName }}</div>
              <div class="profile-uid-chip">
                <span class="uid-prefix">ID</span>
                <span class="uid-value">{{ student.studentUniqueId }}</span>
              </div>
            </div>
            <div class="profile-grade-badge">🎓 {{ student.gradeName }}</div>
            <div class="profile-since">
              <span class="profile-since-icon">📅</span>
              Member since {{ student.createdAt | date:'MMMM yyyy' }}
            </div>
          </div>

          <!-- Details Panel -->
          <div class="profile-details">

            <div class="profile-field">
              <div class="field-icon-wrap email-icon">✉️</div>
              <div class="field-content">
                <div class="field-label">Email Address</div>
                <div class="field-value">{{ student.email }}</div>
              </div>
            </div>

            <div class="profile-field">
              <div class="field-icon-wrap phone-icon">📞</div>
              <div class="field-content">
                <div class="field-label">Phone Number</div>
                <div class="field-value">{{ student.phone }}</div>
              </div>
            </div>

            <div class="profile-field">
              <div class="field-icon-wrap id-icon">🪪</div>
              <div class="field-content">
                <div class="field-label">ID / Passport Number</div>
                <div class="field-value">{{ student.idPassportNo }}</div>
              </div>
            </div>

            <div class="profile-field">
              <div class="field-icon-wrap grade-icon">🎓</div>
              <div class="field-content">
                <div class="field-label">Enrolled Grade</div>
                <div class="field-value">{{ student.gradeName }}</div>
              </div>
            </div>

            <div class="profile-field">
              <div class="field-icon-wrap uid-icon">🔑</div>
              <div class="field-content">
                <div class="field-label">Student Unique ID</div>
                <div class="field-value"><span class="mono-chip">{{ student.studentUniqueId }}</span></div>
              </div>
            </div>

            <div class="profile-field">
              <div class="field-icon-wrap date-icon">📅</div>
              <div class="field-content">
                <div class="field-label">Registration Date</div>
                <div class="field-value">{{ student.createdAt | date:'longDate' }}</div>
              </div>
            </div>

          </div>
        </div>
      </div>

    </div>

    <ng-template #loading>
      <div class="loading-screen">
        <div class="spinner-large"></div>
        <p>Loading your dashboard…</p>
      </div>
    </ng-template>
  `,
  styles: [`
    .dashboard { max-width: 900px; margin: 0 auto; padding: 24px 16px; }

    /* Header */
    .dash-header {
      display: flex; align-items: center; justify-content: space-between;
      background: linear-gradient(135deg, #1a1a2e, #16213e);
      color: #fff; border-radius: 12px; padding: 24px 28px; margin-bottom: 24px;
    }
    .dash-header-left { display: flex; align-items: center; gap: 18px; }
    .avatar {
      width: 60px; height: 60px; border-radius: 50%;
      background: linear-gradient(135deg, #4caf50, #2196f3);
      display: flex; align-items: center; justify-content: center;
      font-size: 22px; font-weight: 700; color: #fff; flex-shrink: 0;
    }
    .student-name { margin: 0 0 8px; font-size: 22px; }
    .student-meta { display: flex; gap: 8px; flex-wrap: wrap; }
    .badge { padding: 3px 10px; border-radius: 20px; font-size: 12px; font-weight: 600; }
    .badge-id { background: rgba(255,255,255,0.15); color: #fff; }
    .badge-grade { background: #4caf50; color: #fff; }


    /* Cards */
    .cards-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px; margin-bottom: 24px; }
    .card {
      background: #fff; border-radius: 10px; padding: 20px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.08);
      display: flex; align-items: center; gap: 14px;
      border-left: 4px solid #ddd;
    }
    .card-score { border-left-color: #2196f3; }
    .card-avg  { border-left-color: #9c27b0; }
    .card-pct  { border-left-color: #ff9800; }
    .card-excellent  { border-left-color: #4caf50; }
    .card-good       { border-left-color: #8bc34a; }
    .card-satisfactory { border-left-color: #ffc107; }
    .card-needs-support { border-left-color: #f44336; }
    .card-icon { font-size: 28px; }
    .card-value { font-size: 20px; font-weight: 700; color: #1a1a2e; }
    .card-label { font-size: 12px; color: #888; margin-top: 2px; }

    /* Progress */
    .progress-section {
      background: #fff; border-radius: 10px; padding: 20px 24px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.08); margin-bottom: 24px;
    }
    .progress-header { display: flex; justify-content: space-between; margin-bottom: 10px; }
    .progress-label { font-weight: 600; color: #333; }
    .progress-pct { font-weight: 700; color: #1a1a2e; font-size: 18px; }
    .progress-track { background: #eee; border-radius: 8px; height: 16px; overflow: hidden; }
    .progress-fill { height: 100%; border-radius: 8px; transition: width 1s ease; min-width: 4px; }
    .fill-danger  { background: linear-gradient(90deg, #f44336, #e57373); }
    .fill-warning { background: linear-gradient(90deg, #ffc107, #ffca28); }
    .fill-info    { background: linear-gradient(90deg, #2196f3, #64b5f6); }
    .fill-success { background: linear-gradient(90deg, #4caf50, #81c784); }
    .progress-legend { display: flex; gap: 12px; flex-wrap: wrap; margin-top: 10px; }
    .legend-item { font-size: 11px; padding: 2px 8px; border-radius: 12px; }
    .legend-item.danger  { background: #fdecea; color: #c62828; }
    .legend-item.warning { background: #fff8e1; color: #e65100; }
    .legend-item.info    { background: #e3f2fd; color: #1565c0; }
    .legend-item.success { background: #e8f5e9; color: #1b5e20; }

    /* Table */
    .table-section {
      background: #fff; border-radius: 10px; padding: 20px 24px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.08); margin-bottom: 24px;
    }
    .section-title { margin: 0 0 16px; font-size: 18px; color: #1a1a2e; }
    .table-responsive { overflow-x: auto; }
    .assessment-table { width: 100%; border-collapse: collapse; font-size: 14px; }
    .assessment-table th {
      background: #f5f5f5; text-align: left; padding: 10px 12px;
      font-weight: 600; color: #555; border-bottom: 2px solid #e0e0e0;
    }
    .assessment-table td { padding: 10px 12px; border-bottom: 1px solid #f0f0f0; }
    .assessment-table tr:hover td { background: #fafafa; }
    .assessment-name { font-weight: 500; }
    .pct-badge { padding: 2px 8px; border-radius: 12px; font-size: 12px; font-weight: 600; }
    .pct-danger  { background: #fdecea; color: #c62828; }
    .pct-warning { background: #fff8e1; color: #e65100; }
    .pct-info    { background: #e3f2fd; color: #1565c0; }
    .pct-success { background: #e8f5e9; color: #1b5e20; }
    .status-badge { padding: 2px 8px; border-radius: 12px; font-size: 11px; font-weight: 600; }
    .overdue   { background: #fdecea; color: #c62828; }
    .submitted { background: #e8f5e9; color: #1b5e20; }

    /* Profile Card */
    .profile-card {
      background: #fff; border-radius: 12px; padding: 24px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.08);
    }
    .profile-card-header {
      display: flex; align-items: center; justify-content: space-between;
      margin-bottom: 24px; padding-bottom: 16px; border-bottom: 1px solid #f0f0f0;
    }
    .profile-card-header .section-title { margin: 0; }
    .profile-status-badge {
      display: inline-flex; align-items: center; gap: 6px;
      background: #e8f5e9; color: #2e7d32;
      padding: 4px 14px; border-radius: 20px; font-size: 13px; font-weight: 600;
      border: 1px solid #a5d6a7;
    }
    .status-dot {
      width: 8px; height: 8px; border-radius: 50%; background: #4caf50;
      box-shadow: 0 0 0 3px rgba(76,175,80,0.25); animation: pulse 2s infinite;
    }
    @keyframes pulse {
      0%, 100% { box-shadow: 0 0 0 3px rgba(76,175,80,0.25); }
      50%       { box-shadow: 0 0 0 6px rgba(76,175,80,0.10); }
    }
    .profile-layout {
      display: grid; grid-template-columns: 260px 1fr; gap: 28px; align-items: start;
    }
    /* Identity panel */
    .profile-identity {
      display: flex; flex-direction: column; align-items: center; gap: 14px;
      text-align: center; padding: 24px 16px;
      background: linear-gradient(160deg, #f8faff 0%, #eef2ff 100%);
      border-radius: 10px; border: 1px solid #dde4f8;
    }
    .profile-avatar-lg {
      width: 72px; height: 72px; border-radius: 50%;
      background: linear-gradient(135deg, #4caf50, #2196f3);
      display: flex; align-items: center; justify-content: center;
      font-size: 26px; font-weight: 700; color: #fff;
      box-shadow: 0 4px 14px rgba(76,175,80,0.35);
    }
    .profile-name-block { display: flex; flex-direction: column; align-items: center; gap: 8px; }
    .profile-full-name { font-size: 17px; font-weight: 700; color: #1a1a2e; }
    .profile-uid-chip {
      display: inline-flex; align-items: stretch; border-radius: 6px;
      overflow: hidden; border: 1px solid #cdd5f0; font-size: 12px;
    }
    .uid-prefix {
      background: #1a1a2e; color: #fff; padding: 3px 8px;
      font-weight: 700; letter-spacing: 0.5px;
    }
    .uid-value {
      background: #f4f6ff; color: #3f51b5; padding: 3px 10px;
      font-family: monospace; font-weight: 600; letter-spacing: 0.5px;
    }
    .profile-grade-badge {
      background: linear-gradient(135deg, #e8f5e9, #c8e6c9);
      color: #1b5e20; padding: 5px 14px; border-radius: 20px;
      font-size: 13px; font-weight: 600; border: 1px solid #a5d6a7;
    }
    .profile-since {
      font-size: 12px; color: #90a4ae;
      display: flex; align-items: center; gap: 5px;
    }
    .profile-since-icon { font-size: 13px; }
    /* Details panel */
    .profile-details { display: flex; flex-direction: column; gap: 2px; }
    .profile-field {
      display: flex; align-items: center; gap: 14px;
      padding: 11px 12px; border-radius: 8px; transition: background 0.15s;
    }
    .profile-field:hover { background: #f8fafc; }
    .field-icon-wrap {
      width: 38px; height: 38px; border-radius: 9px; flex-shrink: 0;
      display: flex; align-items: center; justify-content: center; font-size: 18px;
    }
    .email-icon { background: #e3f2fd; }
    .phone-icon { background: #e8f5e9; }
    .id-icon    { background: #f3e5f5; }
    .grade-icon { background: #fff8e1; }
    .uid-icon   { background: #fce4ec; }
    .date-icon  { background: #e0f7fa; }
    .field-content { display: flex; flex-direction: column; gap: 2px; min-width: 0; }
    .field-label {
      font-size: 10px; color: #90a4ae; font-weight: 700;
      text-transform: uppercase; letter-spacing: 0.7px;
    }
    .field-value { font-size: 14px; color: #1a1a2e; font-weight: 500; word-break: break-word; }
    .mono-chip {
      display: inline-block; background: #f4f6ff; color: #3f51b5;
      font-family: monospace; font-size: 13px; font-weight: 600;
      padding: 2px 8px; border-radius: 5px; border: 1px solid #cdd5f0;
      letter-spacing: 0.5px;
    }

    /* Empty / Loading */
    .empty-state { text-align: center; padding: 32px; color: #999; }
    .empty-icon { font-size: 36px; display: block; margin-bottom: 8px; }
    .loading-screen { text-align: center; padding: 80px 20px; color: #666; }
    .spinner-large {
      width: 40px; height: 40px; border: 3px solid #e0e0e0; border-top-color: #4caf50;
      border-radius: 50%; animation: spin 0.8s linear infinite; margin: 0 auto 16px;
    }
    @keyframes spin { to { transform: rotate(360deg); } }

    @media (max-width: 600px) {
      .cards-grid      { grid-template-columns: 1fr 1fr; }
      .dash-header     { flex-direction: column; gap: 16px; align-items: flex-start; }
      .profile-layout  { grid-template-columns: 1fr; }
      .profile-identity { padding: 20px 12px; }
    }
  `]
})
export class StudentDashboardComponent implements OnInit, OnDestroy {
  student: StudentAuthUser | null = null;
  private destroy$ = new Subject<void>();

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
    private studentAuthBusiness: StudentAuthBusinessService
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
}
