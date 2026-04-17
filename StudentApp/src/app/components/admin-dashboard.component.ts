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
          <div class="tab-header">
            <h2>All Teachers <span class="count-badge">{{ teachers.length }}</span></h2>
            <button class="btn-primary-sm" (click)="toggleTeacherForm()">{{ showTeacherForm ? '✕ Cancel' : '+ New Teacher' }}</button>
          </div>

          <!-- Create Teacher Panel -->
          <div *ngIf="showTeacherForm" class="panel-form">
            <h3>Create Teacher Account</h3>
            <p class="panel-hint">A passwordless account will be created. Share the email with the teacher — they activate at <strong>/activate</strong>.</p>
            <div class="form-row">
              <div class="form-field"><label>ID / Passport No.</label><input [(ngModel)]="newTeacher.idPassportNo" placeholder="123456789" maxlength="9" /></div>
              <div class="form-field"><label>First Name</label><input [(ngModel)]="newTeacher.firstName" placeholder="First name" /></div>
              <div class="form-field"><label>Last Name</label><input [(ngModel)]="newTeacher.lastName" placeholder="Last name" /></div>
            </div>
            <div class="form-row">
              <div class="form-field"><label>Email</label><input type="email" [(ngModel)]="newTeacher.email" placeholder="teacher@school.edu" /></div>
              <div class="form-field"><label>Phone (8 digits)</label><input [(ngModel)]="newTeacher.phone" placeholder="77754256" maxlength="8" /></div>
              <div class="form-field">
                <label>Subject</label>
                <select [(ngModel)]="newTeacher.subjectId">
                  <option [ngValue]="0" disabled>-- Select --</option>
                  <option *ngFor="let s of subjects" [ngValue]="s.id">{{ s.name }}</option>
                </select>
              </div>
            </div>
            <div class="panel-actions">
              <button class="btn-primary" (click)="createTeacher()" [disabled]="loading">Create Account</button>
            </div>
          </div>

          <div *ngIf="!loading && teachers.length === 0" class="empty-state">No teachers found.</div>
          <table *ngIf="teachers.length > 0" class="data-table">
            <thead>
              <tr><th>ID</th><th>Name</th><th>Email</th><th>Subject</th><th>Enrolled</th><th>Status</th><th></th></tr>
            </thead>
            <tbody>
              <tr *ngFor="let t of teachers">
                <td>{{ t.teacherId }}</td>
                <td>{{ t.firstName }} {{ t.lastName }}</td>
                <td>{{ t.email }}</td>
                <td>{{ t.subjectName }}</td>
                <td>{{ t.enrollmentDate | date:'dd MMM yyyy' }}</td>
                <td><span class="status-badge" [class.pending]="!t.isActive">{{ t.isActive ? 'Active' : 'Pending Activation' }}</span></td>
                <td><button class="btn-danger-sm" (click)="confirmDeleteTeacher(t)">Delete</button></td>
              </tr>
            </tbody>
          </table>
        </section>

        <!-- Students Tab -->
        <section *ngIf="activeTab === 'students'">
          <div class="tab-header">
            <h2>All Students <span class="count-badge">{{ students.length }}</span></h2>
            <button class="btn-primary-sm" (click)="toggleStudentForm()">{{ showStudentForm ? '✕ Cancel' : '+ New Student' }}</button>
          </div>

          <!-- Create Student Panel -->
          <div *ngIf="showStudentForm" class="panel-form">
            <h3>Create Student Account</h3>
            <p class="panel-hint">A passwordless account will be created. Share the student's <strong>Unique ID</strong> + email with them — they activate at <strong>/student/login</strong>.</p>
            <div class="form-row">
              <div class="form-field"><label>ID / Passport No.</label><input [(ngModel)]="newStudent.idPassportNo" placeholder="123456789" maxlength="9" /></div>
              <div class="form-field"><label>First Name</label><input [(ngModel)]="newStudent.firstName" placeholder="First name" /></div>
              <div class="form-field"><label>Last Name</label><input [(ngModel)]="newStudent.lastName" placeholder="Last name" /></div>
            </div>
            <div class="form-row">
              <div class="form-field"><label>Email</label><input type="email" [(ngModel)]="newStudent.email" placeholder="student@school.edu" /></div>
              <div class="form-field"><label>Phone (8 digits)</label><input [(ngModel)]="newStudent.phone" placeholder="77754256" maxlength="8" /></div>
              <div class="form-field">
                <label>Grade</label>
                <select [(ngModel)]="newStudent.gradeId">
                  <option [ngValue]="0" disabled>-- Select --</option>
                  <option *ngFor="let g of grades" [ngValue]="g.id">{{ g.name }}</option>
                </select>
              </div>
            </div>
            <div class="panel-actions">
              <button class="btn-primary" (click)="createStudent()" [disabled]="loading">Create Account</button>
            </div>
          </div>

          <div class="search-bar">
            <input placeholder="Search by name or ID…" [(ngModel)]="studentSearch" />
          </div>
          <div *ngIf="!loading && filteredStudents.length === 0" class="empty-state">No students found.</div>
          <table *ngIf="filteredStudents.length > 0" class="data-table">
            <thead>
              <tr><th>Unique ID</th><th>Name</th><th>Email</th><th>Grade</th><th>Assigned Teachers</th><th>Performance</th><th></th></tr>
            </thead>
            <tbody>
              <ng-container *ngFor="let s of filteredStudents">
                <tr>
                  <td><code>{{ s.studentUniqueId }}</code></td>
                  <td>{{ s.firstName }} {{ s.lastName }}</td>
                  <td>{{ s.email }}</td>
                  <td>{{ s.gradeName }}</td>
                  <td class="teachers-cell">
                    <div *ngIf="s.teachers?.length > 0" class="teacher-chips">
                      <span *ngFor="let t of s.teachers" class="teacher-chip">
                        {{ t.firstName }} {{ t.lastName }}
                        <button class="chip-remove" title="Unassign" (click)="unassignTeacher(s.id, t.teacherId)">✕</button>
                      </span>
                    </div>
                    <span *ngIf="!s.teachers?.length" class="no-assign">None</span>
                    <button class="btn-assign-sm" (click)="toggleAssignPanel(s.id)">
                      {{ assigningStudentId === s.id ? '✕' : '+ Assign' }}
                    </button>
                  </td>
                  <td><span class="perf-badge" [attr.data-level]="s.performanceLevel">{{ s.performanceLevel }}</span></td>
                  <td><button class="btn-danger-sm" (click)="confirmDeleteStudent(s)">Delete</button></td>
                </tr>
                <!-- Inline assign row -->
                <tr *ngIf="assigningStudentId === s.id" class="assign-row">
                  <td colspan="7">
                    <div class="assign-panel">
                      <label>Assign teacher to {{ s.firstName }} {{ s.lastName }}:</label>
                      <select [(ngModel)]="teacherToAssignId">
                        <option [ngValue]="0" disabled>-- Select teacher --</option>
                        <option *ngFor="let t of teachers" [ngValue]="t.teacherId">{{ t.firstName }} {{ t.lastName }} ({{ t.subjectName }})</option>
                      </select>
                      <button class="btn-primary" (click)="assignTeacher(s.id)" [disabled]="teacherToAssignId === 0">Assign</button>
                    </div>
                  </td>
                </tr>
              </ng-container>
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

    /* Tab header with action button */
    .tab-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 20px; }
    .tab-header h2 { margin: 0; }
    .btn-primary-sm { padding: 6px 16px; background: #0f3460; color: #fff; border: none; border-radius: 6px; cursor: pointer; font-size: 0.85rem; font-weight: 600; }
    .btn-primary-sm:hover { background: #1a5276; }
    .btn-primary { padding: 8px 20px; background: #0f3460; color: #fff; border: none; border-radius: 6px; cursor: pointer; font-weight: 600; }
    .btn-primary:disabled { background: #ccc; cursor: not-allowed; }

    /* Create form panel */
    .panel-form { background: #f7f8fa; border: 1.5px solid #dde1e7; border-radius: 8px; padding: 20px; margin-bottom: 20px; }
    .panel-form h3 { margin: 0 0 6px; color: #1a1a2e; font-size: 1rem; }
    .panel-hint { color: #555; font-size: 0.82rem; margin: 0 0 16px; line-height: 1.5; }
    .form-row { display: flex; gap: 12px; margin-bottom: 12px; flex-wrap: wrap; }
    .form-field { flex: 1; min-width: 160px; display: flex; flex-direction: column; gap: 4px; }
    .form-field label { font-size: 0.78rem; font-weight: 600; color: #555; text-transform: uppercase; letter-spacing: 0.04em; }
    .form-field input, .form-field select { padding: 7px 10px; border: 1.5px solid #ddd; border-radius: 5px; font-size: 0.875rem; }
    .panel-actions { display: flex; gap: 10px; margin-top: 8px; }

    /* Status badge */
    .status-badge { display: inline-block; padding: 2px 8px; border-radius: 10px; font-size: 0.75rem; font-weight: 600; background: #d1fae5; color: #065f46; }
    .status-badge.pending { background: #fef3c7; color: #92400e; }

    /* Teacher chips in student row */
    .teachers-cell { min-width: 180px; }
    .teacher-chips { display: flex; flex-wrap: wrap; gap: 5px; margin-bottom: 5px; }
    .teacher-chip { display: inline-flex; align-items: center; gap: 4px; background: #dbeafe; color: #1e40af; padding: 2px 8px; border-radius: 12px; font-size: 0.78rem; font-weight: 500; }
    .chip-remove { background: none; border: none; color: #1e40af; cursor: pointer; padding: 0 2px; font-size: 0.85rem; line-height: 1; }
    .chip-remove:hover { color: #dc2626; }
    .no-assign { font-size: 0.8rem; color: #999; display: block; margin-bottom: 4px; }
    .btn-assign-sm { padding: 2px 10px; background: #0f3460; color: #fff; border: none; border-radius: 4px; cursor: pointer; font-size: 0.75rem; margin-top: 2px; }
    .btn-assign-sm:hover { background: #1a5276; }

    /* Inline assign row */
    .assign-row td { background: #eef2ff; padding: 10px 14px !important; }
    .assign-panel { display: flex; align-items: center; gap: 10px; flex-wrap: wrap; }
    .assign-panel label { font-size: 0.85rem; color: #1a1a2e; font-weight: 500; }
    .assign-panel select { padding: 6px 10px; border: 1.5px solid #ddd; border-radius: 5px; font-size: 0.85rem; min-width: 220px; }
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

  // Create Teacher form state
  showTeacherForm = false;
  subjects: any[] = [];
  newTeacher = { idPassportNo: '', firstName: '', lastName: '', email: '', phone: '', subjectId: 0 };

  // Create Student form state
  showStudentForm = false;
  grades: any[] = [];
  newStudent = { idPassportNo: '', firstName: '', lastName: '', email: '', phone: '', gradeId: 0 };

  // Teacher assignment state
  assigningStudentId: number | null = null;
  teacherToAssignId = 0;

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

  // ─── Create Teacher ───────────────────────────────────────────────────────

  toggleTeacherForm(): void {
    this.showTeacherForm = !this.showTeacherForm;
    if (this.showTeacherForm && this.subjects.length === 0) {
      this.adminApi.getSubjects().subscribe({ next: data => this.subjects = data });
    }
    if (!this.showTeacherForm) {
      this.resetNewTeacher();
    }
  }

  createTeacher(): void {
    if (!this.newTeacher.idPassportNo || !this.newTeacher.firstName || !this.newTeacher.lastName
        || !this.newTeacher.email || !this.newTeacher.phone || this.newTeacher.subjectId === 0) {
      this.error = 'Please fill in all fields for the new teacher.';
      return;
    }
    this.loading = true; this.error = '';
    this.adminApi.createTeacher(this.newTeacher).subscribe({
      next: teacher => {
        this.teachers = [...this.teachers, teacher];
        this.showTeacherForm = false;
        this.resetNewTeacher();
        this.loading = false;
        this.showSuccess(`Teacher account created. Share email "${teacher.email}" so they can activate their account.`);
      },
      error: err => {
        this.error = err?.error?.message || err?.error?.errors
          ? JSON.stringify(err.error.errors) : 'Failed to create teacher.';
        this.loading = false;
      }
    });
  }

  private resetNewTeacher(): void {
    this.newTeacher = { idPassportNo: '', firstName: '', lastName: '', email: '', phone: '', subjectId: 0 };
  }

  // ─── Create Student ───────────────────────────────────────────────────────

  // ─── Create Student ───────────────────────────────────────────────────────

  loadStudents(): void {
    this.loading = true; this.error = '';
    this.adminApi.getAllStudents().subscribe({
      next: data => { this.students = data; this.loading = false; },
      error: () => { this.error = 'Failed to load students.'; this.loading = false; }
    });
  }

  toggleStudentForm(): void {
    this.showStudentForm = !this.showStudentForm;
    if (this.showStudentForm && this.grades.length === 0) {
      this.adminApi.getGrades().subscribe({ next: data => this.grades = data });
    }
    if (!this.showStudentForm) {
      this.resetNewStudent();
    }
  }

  createStudent(): void {
    if (!this.newStudent.idPassportNo || !this.newStudent.firstName || !this.newStudent.lastName
        || !this.newStudent.email || !this.newStudent.phone || this.newStudent.gradeId === 0) {
      this.error = 'Please fill in all fields for the new student.';
      return;
    }
    this.loading = true; this.error = '';
    this.adminApi.createStudent(this.newStudent).subscribe({
      next: student => {
        this.students = [...this.students, student];
        this.showStudentForm = false;
        this.resetNewStudent();
        this.loading = false;
        this.showSuccess(`Student account created (ID: ${student.studentUniqueId}). Share their Unique ID and email so they can activate.`);
      },
      error: err => {
        this.error = err?.error?.message || 'Failed to create student.';
        this.loading = false;
      }
    });
  }

  private resetNewStudent(): void {
    this.newStudent = { idPassportNo: '', firstName: '', lastName: '', email: '', phone: '', gradeId: 0 };
  }

  // ─── Teacher assignment ───────────────────────────────────────────────────

  toggleAssignPanel(studentId: number): void {
    this.assigningStudentId = this.assigningStudentId === studentId ? null : studentId;
    this.teacherToAssignId = 0;
    // Ensure teachers are loaded for the dropdown
    if (this.teachers.length === 0) {
      this.adminApi.getAllTeachers().subscribe({ next: data => this.teachers = data });
    }
  }

  assignTeacher(studentId: number): void {
    if (!this.teacherToAssignId) return;
    this.adminApi.assignStudentToTeacher(studentId, this.teacherToAssignId).subscribe({
      next: () => {
        // Refresh student list to get updated teacher assignments
        this.adminApi.getAllStudents().subscribe({ next: data => this.students = data });
        this.assigningStudentId = null;
        this.teacherToAssignId = 0;
        this.showSuccess('Teacher assigned successfully.');
      },
      error: err => this.error = err?.error?.message || 'Failed to assign teacher.'
    });
  }

  unassignTeacher(studentId: number, teacherId: number): void {
    this.adminApi.unassignStudentFromTeacher(studentId, teacherId).subscribe({
      next: () => {
        this.adminApi.getAllStudents().subscribe({ next: data => this.students = data });
        this.showSuccess('Teacher unassigned successfully.');
      },
      error: err => this.error = err?.error?.message || 'Failed to unassign teacher.'
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
