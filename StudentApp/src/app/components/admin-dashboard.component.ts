import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminApiService, AuditLogDto } from '../core/services/http/admin-api.service';

type TabType = 'teachers' | 'students' | 'audit';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="admin-layout">
      <!-- Sidebar -->
      <aside class="sidebar">
        <div class="sidebar-header">
          <span class="admin-icon">🛡️</span>
          <h1>Admin Portal</h1>
          <p class="admin-name">{{ adminName }}</p>
        </div>
        <nav>
          <button [class.active]="activeTab === 'teachers'" (click)="activeTab = 'teachers'; loadTeachers()">
            👩‍🏫 Teachers
          </button>
          <button [class.active]="activeTab === 'students'" (click)="activeTab = 'students'; loadStudents()">
            🎓 Students
          </button>
          <button [class.active]="activeTab === 'audit'" (click)="activeTab = 'audit'; loadAuditLogs()">
            📋 Audit Log
          </button>
        </nav>
        <button class="logout-btn" (click)="logout()">Sign Out</button>
      </aside>

      <!-- Main content -->
      <main class="main-content">

        <!-- Loading / Error -->
        <div *ngIf="loading" class="status-msg">Loading…</div>
        <div *ngIf="error" class="alert-error">{{ error }}</div>

        <!-- Success toast -->
        <div *ngIf="successMsg" class="alert-success">{{ successMsg }}</div>

        <!-- Teachers Tab -->
        <section *ngIf="activeTab === 'teachers'">
          <h2>All Teachers <span class="count-badge">{{ teachers.length }}</span></h2>
          <div *ngIf="!loading && teachers.length === 0" class="empty-state">No teachers found.</div>
          <table *ngIf="teachers.length > 0" class="data-table">
            <thead>
              <tr>
                <th>ID</th><th>Name</th><th>Email</th><th>Subject</th><th>Enrolled</th><th></th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let t of teachers">
                <td>{{ t.teacherId }}</td>
                <td>{{ t.firstName }} {{ t.lastName }}</td>
                <td>{{ t.email }}</td>
                <td>{{ t.subjectName }}</td>
                <td>{{ t.enrollmentDate | date:'dd MMM yyyy' }}</td>
                <td>
                  <button class="btn-danger-sm" (click)="confirmDeleteTeacher(t)">Delete</button>
                </td>
              </tr>
            </tbody>
          </table>
        </section>

        <!-- Students Tab -->
        <section *ngIf="activeTab === 'students'">
          <h2>All Students <span class="count-badge">{{ students.length }}</span></h2>
          <div class="search-bar">
            <input placeholder="Search by name or ID…" [(ngModel)]="studentSearch" />
          </div>
          <div *ngIf="!loading && filteredStudents.length === 0" class="empty-state">No students found.</div>
          <table *ngIf="filteredStudents.length > 0" class="data-table">
            <thead>
              <tr>
                <th>Unique ID</th><th>Name</th><th>Email</th><th>Grade</th><th>Performance</th><th></th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let s of filteredStudents">
                <td><code>{{ s.studentUniqueId }}</code></td>
                <td>{{ s.firstName }} {{ s.lastName }}</td>
                <td>{{ s.email }}</td>
                <td>{{ s.gradeName }}</td>
                <td><span class="perf-badge" [attr.data-level]="s.performanceLevel">{{ s.performanceLevel }}</span></td>
                <td>
                  <button class="btn-danger-sm" (click)="confirmDeleteStudent(s)">Delete</button>
                </td>
              </tr>
            </tbody>
          </table>
        </section>

        <!-- Audit Log Tab -->
        <section *ngIf="activeTab === 'audit'">
          <h2>Audit Log</h2>
          <div class="audit-filters">
            <select [(ngModel)]="auditEntityFilter" (change)="applyAuditFilter()">
              <option value="">All Entities</option>
              <option value="Student">Student</option>
              <option value="Teacher">Teacher</option>
              <option value="StudentAssessment">Assessment</option>
              <option value="ClassGroup">Class Group</option>
              <option value="Admin">Admin</option>
            </select>
            <select [(ngModel)]="auditActionFilter" (change)="applyAuditFilter()">
              <option value="">All Actions</option>
              <option value="Create">Create</option>
              <option value="Update">Update</option>
              <option value="Delete">Delete</option>
            </select>
          </div>
          <div *ngIf="!loading && filteredAuditLogs.length === 0" class="empty-state">No audit records found.</div>
          <table *ngIf="filteredAuditLogs.length > 0" class="data-table audit-table">
            <thead>
              <tr>
                <th>Timestamp (UTC)</th><th>Action</th><th>Entity</th><th>ID</th>
                <th>Changed By</th><th>Role</th><th>Old Values</th><th>New Values</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let log of filteredAuditLogs" [attr.data-action]="log.action">
                <td>{{ log.changedAt | date:'dd MMM yyyy HH:mm:ss' }}</td>
                <td><span class="action-badge" [attr.data-action]="log.action">{{ log.action }}</span></td>
                <td>{{ log.entityName }}</td>
                <td>{{ log.entityId }}</td>
                <td>{{ log.changedBy || '—' }}</td>
                <td>{{ log.changedByRole || '—' }}</td>
                <td><pre class="json-pre">{{ formatJson(log.oldValues) }}</pre></td>
                <td><pre class="json-pre">{{ formatJson(log.newValues) }}</pre></td>
              </tr>
            </tbody>
          </table>
        </section>

      </main>

      <!-- Confirm Delete Modal -->
      <div *ngIf="deleteTarget" class="modal-overlay" (click)="cancelDelete()">
        <div class="modal" (click)="$event.stopPropagation()">
          <h3>Confirm Delete</h3>
          <p>Are you sure you want to permanently delete <strong>{{ deleteTargetName }}</strong>?
             This action cannot be undone.</p>
          <div class="modal-actions">
            <button class="btn-danger" (click)="executeDelete()">Delete</button>
            <button class="btn-secondary" (click)="cancelDelete()">Cancel</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .admin-layout { display: flex; min-height: 100vh; background: #f0f2f5; }

    /* Sidebar */
    .sidebar {
      width: 240px; background: #1a1a2e; color: #fff; padding: 0;
      display: flex; flex-direction: column; flex-shrink: 0;
    }
    .sidebar-header { padding: 24px 20px 16px; border-bottom: 1px solid rgba(255,255,255,0.1); }
    .admin-icon { font-size: 2rem; }
    .sidebar-header h1 { font-size: 1.1rem; margin: 8px 0 4px; }
    .admin-name { font-size: 0.8rem; color: rgba(255,255,255,0.6); margin: 0; }
    nav { flex: 1; padding: 12px 0; }
    nav button {
      display: block; width: 100%; padding: 12px 20px; background: none;
      border: none; color: rgba(255,255,255,0.75); text-align: left; cursor: pointer;
      font-size: 0.9rem; transition: all 0.2s;
    }
    nav button:hover { background: rgba(255,255,255,0.08); color: #fff; }
    nav button.active { background: rgba(255,255,255,0.15); color: #fff; font-weight: 600; }
    .logout-btn {
      margin: 16px; padding: 10px 16px; background: rgba(255,255,255,0.08);
      border: 1px solid rgba(255,255,255,0.2); border-radius: 6px; color: #fff;
      cursor: pointer; font-size: 0.875rem; transition: all 0.2s;
    }
    .logout-btn:hover { background: rgba(255,80,80,0.3); }

    /* Main */
    .main-content { flex: 1; padding: 32px; overflow-x: auto; }
    h2 { margin: 0 0 20px; color: #1a1a2e; font-size: 1.4rem; display: flex; align-items: center; gap: 10px; }
    .count-badge {
      background: #0f3460; color: #fff; border-radius: 20px;
      padding: 2px 10px; font-size: 0.8rem;
    }
    .status-msg { color: #666; padding: 20px; }
    .alert-error { background: #fff5f5; border: 1px solid #feb2b2; color: #c53030; padding: 12px 16px; border-radius: 6px; margin-bottom: 16px; }
    .alert-success { background: #f0fff4; border: 1px solid #9ae6b4; color: #276749; padding: 12px 16px; border-radius: 6px; margin-bottom: 16px; }
    .empty-state { color: #999; padding: 20px 0; }

    /* Search */
    .search-bar { margin-bottom: 16px; }
    .search-bar input {
      padding: 8px 12px; border: 1.5px solid #ddd; border-radius: 6px;
      width: 300px; font-size: 0.9rem;
    }

    /* Table */
    .data-table { width: 100%; border-collapse: collapse; background: #fff; border-radius: 8px; overflow: hidden; box-shadow: 0 1px 4px rgba(0,0,0,0.08); }
    .data-table th { background: #f7f8fa; padding: 12px 14px; text-align: left; font-size: 0.8rem; color: #666; text-transform: uppercase; letter-spacing: 0.05em; }
    .data-table td { padding: 12px 14px; border-top: 1px solid #f0f0f0; font-size: 0.9rem; vertical-align: top; }
    .data-table tr:hover td { background: #fafbff; }

    /* Audit */
    .audit-filters { display: flex; gap: 12px; margin-bottom: 16px; }
    .audit-filters select { padding: 8px 12px; border: 1.5px solid #ddd; border-radius: 6px; font-size: 0.9rem; }
    .audit-table { font-size: 0.8rem; }
    .audit-table td { max-width: 200px; overflow: hidden; text-overflow: ellipsis; }
    .json-pre { margin: 0; white-space: pre-wrap; word-break: break-word; font-size: 0.75rem; color: #555; max-width: 180px; }
    .action-badge {
      display: inline-block; padding: 2px 8px; border-radius: 4px; font-size: 0.75rem; font-weight: 600;
    }
    .action-badge[data-action="Create"] { background: #e6f4ea; color: #1e7e34; }
    .action-badge[data-action="Update"] { background: #fff3cd; color: #856404; }
    .action-badge[data-action="Delete"] { background: #fde8e8; color: #b91c1c; }

    /* Performance badge */
    .perf-badge { padding: 3px 10px; border-radius: 12px; font-size: 0.8rem; font-weight: 600; }
    .perf-badge[data-level="Excellent"] { background: #d1fae5; color: #065f46; }
    .perf-badge[data-level="Good"] { background: #dbeafe; color: #1e40af; }
    .perf-badge[data-level="Satisfactory"] { background: #fef3c7; color: #92400e; }
    .perf-badge[data-level="Needs Support"] { background: #fee2e2; color: #991b1b; }

    /* Buttons */
    .btn-danger-sm { padding: 4px 12px; background: #dc2626; color: #fff; border: none; border-radius: 4px; cursor: pointer; font-size: 0.8rem; }
    .btn-danger-sm:hover { background: #b91c1c; }
    .btn-danger { padding: 8px 20px; background: #dc2626; color: #fff; border: none; border-radius: 6px; cursor: pointer; font-weight: 600; }
    .btn-secondary { padding: 8px 20px; background: #6b7280; color: #fff; border: none; border-radius: 6px; cursor: pointer; font-weight: 600; }

    /* Modal */
    .modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 9999; }
    .modal { background: #fff; border-radius: 10px; padding: 28px; max-width: 420px; width: 90%; box-shadow: 0 20px 60px rgba(0,0,0,0.2); }
    .modal h3 { margin: 0 0 12px; color: #1a1a2e; }
    .modal p { color: #555; margin-bottom: 24px; }
    .modal-actions { display: flex; gap: 12px; }

    code { font-family: monospace; font-size: 0.85rem; background: #f3f4f6; padding: 2px 6px; border-radius: 3px; }
  `]
})
export class AdminDashboardComponent implements OnInit {
  activeTab: TabType = 'teachers';

  teachers: any[] = [];
  students: any[] = [];
  auditLogs: AuditLogDto[] = [];
  filteredAuditLogs: AuditLogDto[] = [];

  studentSearch = '';
  auditEntityFilter = '';
  auditActionFilter = '';

  loading = false;
  error = '';
  successMsg = '';

  deleteTarget: { type: 'teacher' | 'student'; id: number; data: any } | null = null;
  deleteTargetName = '';

  adminName = '';

  constructor(private adminApi: AdminApiService, private router: Router) {}

  ngOnInit(): void {
    const info = localStorage.getItem('admin_info');
    if (info) {
      const a = JSON.parse(info);
      this.adminName = `${a.firstName} ${a.lastName}`;
    }
    if (!localStorage.getItem('admin_token')) {
      this.router.navigate(['/admin/login']);
      return;
    }
    this.loadTeachers();
  }

  loadTeachers(): void {
    this.loading = true; this.error = '';
    this.adminApi.getAllTeachers().subscribe({
      next: data => { this.teachers = data; this.loading = false; },
      error: () => { this.error = 'Failed to load teachers.'; this.loading = false; }
    });
  }

  loadStudents(): void {
    this.loading = true; this.error = '';
    this.adminApi.getAllStudents().subscribe({
      next: data => { this.students = data; this.loading = false; },
      error: () => { this.error = 'Failed to load students.'; this.loading = false; }
    });
  }

  loadAuditLogs(): void {
    this.loading = true; this.error = '';
    this.adminApi.getAuditLogs(1, 200).subscribe({
      next: data => {
        this.auditLogs = data;
        this.applyAuditFilter();
        this.loading = false;
      },
      error: () => { this.error = 'Failed to load audit logs.'; this.loading = false; }
    });
  }

  get filteredStudents(): any[] {
    if (!this.studentSearch.trim()) return this.students;
    const q = this.studentSearch.toLowerCase();
    return this.students.filter(s =>
      (s.firstName + ' ' + s.lastName).toLowerCase().includes(q) ||
      (s.studentUniqueId || '').toLowerCase().includes(q) ||
      (s.email || '').toLowerCase().includes(q)
    );
  }

  applyAuditFilter(): void {
    this.filteredAuditLogs = this.auditLogs.filter(log =>
      (!this.auditEntityFilter || log.entityName === this.auditEntityFilter) &&
      (!this.auditActionFilter || log.action === this.auditActionFilter)
    );
  }

  confirmDeleteTeacher(teacher: any): void {
    this.deleteTarget = { type: 'teacher', id: teacher.teacherId, data: teacher };
    this.deleteTargetName = `${teacher.firstName} ${teacher.lastName} (teacher)`;
  }

  confirmDeleteStudent(student: any): void {
    this.deleteTarget = { type: 'student', id: student.id, data: student };
    this.deleteTargetName = `${student.firstName} ${student.lastName} (student)`;
  }

  cancelDelete(): void { this.deleteTarget = null; }

  executeDelete(): void {
    if (!this.deleteTarget) return;
    const { type, id } = this.deleteTarget;
    const obs = type === 'teacher'
      ? this.adminApi.deleteTeacher(id)
      : this.adminApi.deleteStudent(id);

    obs.subscribe({
      next: () => {
        this.showSuccess(`${type === 'teacher' ? 'Teacher' : 'Student'} deleted successfully.`);
        this.deleteTarget = null;
        if (type === 'teacher') {
          this.teachers = this.teachers.filter(t => t.teacherId !== id);
        } else {
          this.students = this.students.filter(s => s.id !== id);
        }
      },
      error: err => {
        this.error = err?.error?.message || 'Delete failed.';
        this.deleteTarget = null;
      }
    });
  }

  formatJson(val: string | null): string {
    if (!val) return '—';
    try { return JSON.stringify(JSON.parse(val), null, 2); }
    catch { return val; }
  }

  logout(): void {
    localStorage.removeItem('admin_token');
    localStorage.removeItem('admin_info');
    this.router.navigate(['/admin/login']);
  }

  private showSuccess(msg: string): void {
    this.successMsg = msg;
    setTimeout(() => this.successMsg = '', 3000);
  }
}
