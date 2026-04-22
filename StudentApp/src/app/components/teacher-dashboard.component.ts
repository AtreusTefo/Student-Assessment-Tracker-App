import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Teacher } from '../core/models';
import { TeacherStateService } from '../core/services/state';
import { ClassGroupApiService, ClassGroupDto } from '../core/services/http';

@Component({
  selector: 'app-teacher-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="dashboard-container">
      <div class="dashboard-header">
        <h2>Teacher Dashboard</h2>
        <a routerLink="/" class="btn btn-secondary btn-sm">&#8592; My Students</a>
      </div>

      <div *ngIf="loading" class="loading">Loading dashboard…</div>
      <div *ngIf="error" class="alert-error">{{ error }}</div>

      <div *ngIf="teacher && !loading">
        <!-- Profile Card -->
        <div class="card profile-card">
          <div class="card-header">
            <span class="avatar">{{ initials }}</span>
            <div>
              <h3>{{ teacher.firstName }} {{ teacher.lastName }}</h3>
              <span class="subject-badge">{{ teacher.subjectName }}</span>
            </div>
          </div>
          <div class="profile-details">
            <div class="detail-row">
              <span class="label">Email</span>
              <span>{{ teacher.email }}</span>
            </div>
            <div class="detail-row">
              <span class="label">Phone</span>
              <span>{{ teacher.phone || '—' }}</span>
            </div>
            <div class="detail-row">
              <span class="label">Subject</span>
              <span>{{ teacher.subjectName }}</span>
            </div>
          </div>
        </div>

        <!-- Class Groups -->
        <div class="card">
          <div class="card-header-plain">
            <h4>My Class Groups</h4>
            <span class="count-badge">{{ classGroups.length }} group{{ classGroups.length !== 1 ? 's' : '' }}</span>
          </div>

          <div *ngIf="groupsLoading" class="loading-small">Loading class groups…</div>

          <div *ngIf="!groupsLoading && classGroups.length === 0" class="empty-state">
            No class groups assigned yet.
          </div>

          <div *ngIf="!groupsLoading && classGroups.length > 0" class="groups-grid">
            <div *ngFor="let group of classGroups" class="group-card">
              <div class="group-name">{{ group.name }}</div>
              <div class="group-meta">
                <span class="meta-tag subject">{{ group.subjectName }}</span>
                <span class="meta-tag grade">{{ group.gradeName }}</span>
              </div>
              <div class="group-footer">
                <span class="student-count">{{ group.studentCount }} student{{ group.studentCount !== 1 ? 's' : '' }}</span>
              </div>
              <details *ngIf="group.students && group.students.length > 0" class="student-list">
                <summary>View students</summary>
                <ul>
                  <li *ngFor="let s of group.students">{{ s.fullName }} <span class="uid">{{ s.studentUniqueId }}</span></li>
                </ul>
              </details>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container {
      max-width: 960px;
      margin: 24px auto;
      padding: 0 20px;
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
    }
    .dashboard-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: 24px;
    }
    .dashboard-header h2 {
      margin: 0;
      font-size: 1.8rem;
      color: #1a237e;
    }
    .card {
      background: #fff;
      border-radius: 10px;
      box-shadow: 0 2px 10px rgba(0,0,0,0.08);
      margin-bottom: 24px;
      overflow: hidden;
    }
    .card-header {
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 20px 24px;
      background: linear-gradient(135deg, #1a237e 0%, #3949ab 100%);
      color: #fff;
    }
    .card-header h3 {
      margin: 0 0 4px;
      font-size: 1.3rem;
    }
    .avatar {
      width: 56px;
      height: 56px;
      border-radius: 50%;
      background: rgba(255,255,255,0.25);
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.4rem;
      font-weight: 700;
      flex-shrink: 0;
    }
    .subject-badge {
      font-size: 0.78rem;
      background: rgba(255,255,255,0.2);
      border-radius: 10px;
      padding: 2px 10px;
    }
    .profile-details {
      padding: 12px 24px;
    }
    .detail-row {
      display: flex;
      gap: 16px;
      padding: 10px 0;
      border-bottom: 1px solid #f0f0f0;
      font-size: 0.95rem;
    }
    .detail-row:last-child { border-bottom: none; }
    .label {
      font-weight: 600;
      color: #555;
      min-width: 80px;
    }
    .card-header-plain {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 16px 24px;
      border-bottom: 1px solid #f0f0f0;
    }
    .card-header-plain h4 {
      margin: 0;
      font-size: 1.1rem;
      color: #1a237e;
    }
    .count-badge {
      font-size: 0.78rem;
      background: #e8eaf6;
      color: #3949ab;
      border-radius: 10px;
      padding: 2px 10px;
      font-weight: 600;
    }
    .groups-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
      gap: 16px;
      padding: 16px 24px;
    }
    .group-card {
      border: 1px solid #e8eaf6;
      border-radius: 8px;
      padding: 14px 16px;
      background: #fafafa;
    }
    .group-name {
      font-weight: 700;
      font-size: 1rem;
      color: #1a237e;
      margin-bottom: 8px;
    }
    .group-meta {
      display: flex;
      gap: 6px;
      flex-wrap: wrap;
      margin-bottom: 8px;
    }
    .meta-tag {
      font-size: 0.75rem;
      border-radius: 8px;
      padding: 2px 8px;
      font-weight: 600;
    }
    .meta-tag.subject { background: #e8f5e9; color: #2e7d32; }
    .meta-tag.grade { background: #e3f2fd; color: #1565c0; }
    .group-footer {
      font-size: 0.82rem;
      color: #777;
    }
    .student-count { font-style: italic; }
    .student-list {
      margin-top: 10px;
      font-size: 0.85rem;
    }
    .student-list summary {
      cursor: pointer;
      color: #3949ab;
      font-weight: 600;
    }
    .student-list ul {
      list-style: none;
      padding: 8px 0 0 8px;
      margin: 0;
    }
    .student-list li {
      padding: 3px 0;
      color: #444;
    }
    .uid {
      font-size: 0.75rem;
      color: #999;
      margin-left: 6px;
    }
    .loading, .loading-small {
      padding: 16px 24px;
      color: #1976d2;
      font-style: italic;
    }
    .empty-state {
      padding: 16px 24px;
      color: #9e9e9e;
      font-style: italic;
    }
    .alert-error {
      background: #fff5f5;
      border: 1px solid #feb2b2;
      color: #c53030;
      padding: 12px 16px;
      border-radius: 8px;
      margin-bottom: 20px;
    }
    .btn {
      display: inline-block;
      padding: 8px 16px;
      border-radius: 6px;
      text-decoration: none;
      cursor: pointer;
      border: none;
      font-size: 0.9rem;
      font-weight: 600;
    }
    .btn-secondary { background: #757575; color: #fff; }
    .btn-sm { padding: 6px 12px; font-size: 0.82rem; }
  `]
})
export class TeacherDashboardComponent implements OnInit, OnDestroy {
  teacher: Teacher | null = null;
  classGroups: ClassGroupDto[] = [];
  loading = true;
  groupsLoading = false;
  error = '';
  private destroy$ = new Subject<void>();

  get initials(): string {
    if (!this.teacher) return '';
    return (this.teacher.firstName[0] ?? '') + (this.teacher.lastName[0] ?? '');
  }

  constructor(
    private teacherState: TeacherStateService,
    private classGroupApi: ClassGroupApiService
  ) {}

  ngOnInit(): void {
    this.teacherState.currentTeacher$
      .pipe(takeUntil(this.destroy$))
      .subscribe(teacher => {
        this.teacher = teacher;
        this.loading = false;
        if (teacher) this.loadClassGroups();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadClassGroups(): void {
    this.groupsLoading = true;
    this.classGroupApi.getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: groups => {
          this.classGroups = groups;
          this.groupsLoading = false;
        },
        error: () => {
          this.groupsLoading = false;
          this.error = 'Failed to load class groups.';
        }
      });
  }
}
