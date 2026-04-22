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
          <button [class.active]="activeTab === 'teachers'" (click)="activeTab = 'teachers'; loadTeachersIfNeeded()">
            👩‍🏫 Teachers
          </button>
          <button [class.active]="activeTab === 'students'" (click)="activeTab = 'students'; loadStudentsIfNeeded()">
            🎓 Students
          </button>
          <button [class.active]="activeTab === 'audit'" (click)="activeTab = 'audit'; loadAuditLogsIfNeeded()">
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
            <div class="tab-header-actions">
              <button class="btn-primary-sm" (click)="toggleTeacherForm()">{{ showTeacherForm ? '✕ Cancel' : '+ New Teacher' }}</button>
              <button class="btn-secondary-sm" (click)="toggleBulkTeacher()">{{ showBulkTeacher ? '✕ Close Bulk Import' : '⬆ Bulk Import' }}</button>
            </div>
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
              <button class="btn-primary" (click)="createTeacher()" [disabled]="loading || submittingTeacher">{{ submittingTeacher ? 'Creating…' : 'Create Account' }}</button>
            </div>
          </div>

          <!-- Bulk Import Teachers Panel -->
          <div *ngIf="showBulkTeacher" class="panel-form panel-bulk">
            <h3>Bulk Import Teachers</h3>
            <p class="panel-hint">
              CSV format: <code>IdPassportNo,FirstName,LastName,Email,Phone,SubjectName</code><br>
              Use exact subject names (e.g. <em>Mathematics</em>, <em>English</em>). Max 500 rows.
            </p>
            <div class="bulk-mode-tabs">
              <button [class.active]="bulkTeacherMode === 'paste'" (click)="bulkTeacherMode = 'paste'">Paste CSV</button>
              <button [class.active]="bulkTeacherMode === 'file'" (click)="bulkTeacherMode = 'file'">Upload File</button>
            </div>

            <!-- Paste mode -->
            <div *ngIf="bulkTeacherMode === 'paste'" class="bulk-paste-area">
              <textarea [(ngModel)]="bulkTeacherCsvText" placeholder="IdPassportNo,FirstName,LastName,Email,Phone,SubjectName&#10;ABC123456,Jane,Doe,jane@school.edu,12345678,Mathematics" rows="8"></textarea>
              <div class="panel-actions">
                <button class="btn-secondary" (click)="previewBulkTeachers()">Preview Rows</button>
                <button class="btn-link" (click)="downloadTeacherTemplate()">⬇ Download Template</button>
              </div>
            </div>

            <!-- File upload mode -->
            <div *ngIf="bulkTeacherMode === 'file'" class="bulk-file-area">
              <input #teacherFileInput type="file" accept=".csv,text/csv" (change)="onTeacherFileSelected($event)" style="display:none" />
              <button class="btn-secondary" (click)="teacherFileInput.click()">Choose CSV File</button>
              <span class="file-name" *ngIf="bulkTeacherFile">{{ bulkTeacherFile.name }}</span>
              <div class="panel-actions" style="margin-top:8px">
                <button class="btn-link" (click)="downloadTeacherTemplate()">⬇ Download Template</button>
              </div>
            </div>

            <!-- Preview table -->
            <div *ngIf="bulkTeacherPreview.length > 0" class="bulk-preview">
              <p class="bulk-preview-label">Preview — {{ bulkTeacherPreview.length }} row(s)</p>
              <div class="bulk-table-wrap">
                <table class="data-table bulk-table">
                  <thead><tr><th>#</th><th>ID/Passport</th><th>First Name</th><th>Last Name</th><th>Email</th><th>Phone</th><th>Subject</th></tr></thead>
                  <tbody>
                    <tr *ngFor="let r of bulkTeacherPreview; let i = index">
                      <td>{{ i + 1 }}</td><td>{{ r.idPassportNo }}</td><td>{{ r.firstName }}</td>
                      <td>{{ r.lastName }}</td><td>{{ r.email }}</td><td>{{ r.phone }}</td><td>{{ r.subjectName }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
              <div class="panel-actions" style="margin-top:12px">
                <button class="btn-primary" (click)="executeBulkTeachers()" [disabled]="importingTeachers">{{ importingTeachers ? 'Importing…' : 'Import ' + bulkTeacherPreview.length + ' Teachers' }}</button>
                <button class="btn-secondary" (click)="clearBulkTeachers()">Clear</button>
              </div>
            </div>

            <!-- Import results -->
            <div *ngIf="bulkTeacherResult" class="bulk-result">
              <div class="bulk-result-summary">
                <span class="result-ok">✓ {{ bulkTeacherResult.successCount }} imported</span>
                <span *ngIf="bulkTeacherResult.failureCount > 0" class="result-fail">✗ {{ bulkTeacherResult.failureCount }} failed</span>
              </div>
              <div *ngIf="bulkTeacherResult.failureCount > 0" class="bulk-table-wrap">
                <table class="data-table bulk-table">
                  <thead><tr><th>Row</th><th>Identifier</th><th>Error</th></tr></thead>
                  <tbody>
                    <tr *ngFor="let r of bulkTeacherResult.results" [class.row-fail]="!r.success">
                      <td *ngIf="!r.success">{{ r.row }}</td>
                      <td *ngIf="!r.success">{{ r.identifier }}</td>
                      <td *ngIf="!r.success">{{ r.error }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
              <button class="btn-link" (click)="bulkTeacherResult = null">Dismiss</button>
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
                <td>
                  <button class="btn-edit-sm" (click)="openEditTeacher(t)">Edit</button>
                  <button class="btn-warning-sm" title="Clear password — teacher must re-activate" (click)="resetTeacherPassword(t)" [disabled]="resettingTeacherId === t.teacherId">{{ resettingTeacherId === t.teacherId ? 'Resetting…' : 'Reset Password' }}</button>
                  <button class="btn-danger-sm" (click)="confirmDeleteTeacher(t)" [disabled]="deletingTeacherId === t.teacherId">{{ deletingTeacherId === t.teacherId ? 'Deleting…' : 'Delete' }}</button>
                </td>
              </tr>
            </tbody>
          </table>
        </section>

        <!-- Students Tab -->
        <section *ngIf="activeTab === 'students'">
          <div class="tab-header">
            <h2>All Students <span class="count-badge">{{ students.length }}</span></h2>
            <div class="tab-header-actions">
              <button class="btn-primary-sm" (click)="toggleStudentForm()">{{ showStudentForm ? '✕ Cancel' : '+ New Student' }}</button>
              <button class="btn-secondary-sm" (click)="toggleBulkStudent()">{{ showBulkStudent ? '✕ Close Bulk Import' : '⬆ Bulk Import' }}</button>
            </div>
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
              <button class="btn-primary" (click)="createStudent()" [disabled]="loading || submittingStudent">{{ submittingStudent ? 'Creating…' : 'Create Account' }}</button>
            </div>
          </div>

          <!-- Bulk Import Students Panel -->
          <div *ngIf="showBulkStudent" class="panel-form panel-bulk">
            <h3>Bulk Import Students</h3>
            <p class="panel-hint">
              CSV format: <code>IdPassportNo,FirstName,LastName,Email,Phone,GradeName</code><br>
              Use exact grade names (e.g. <em>Grade 10</em>) or just the number (e.g. <em>10</em>). Max 500 rows.
            </p>
            <div class="bulk-mode-tabs">
              <button [class.active]="bulkStudentMode === 'paste'" (click)="bulkStudentMode = 'paste'">Paste CSV</button>
              <button [class.active]="bulkStudentMode === 'file'" (click)="bulkStudentMode = 'file'">Upload File</button>
            </div>

            <!-- Paste mode -->
            <div *ngIf="bulkStudentMode === 'paste'" class="bulk-paste-area">
              <textarea [(ngModel)]="bulkStudentCsvText" placeholder="IdPassportNo,FirstName,LastName,Email,Phone,GradeName&#10;ABC123456,John,Smith,john@school.edu,87654321,Grade 10" rows="8"></textarea>
              <div class="panel-actions">
                <button class="btn-secondary" (click)="previewBulkStudents()">Preview Rows</button>
                <button class="btn-link" (click)="downloadStudentTemplate()">⬇ Download Template</button>
              </div>
            </div>

            <!-- File upload mode -->
            <div *ngIf="bulkStudentMode === 'file'" class="bulk-file-area">
              <input #studentFileInput type="file" accept=".csv,text/csv" (change)="onStudentFileSelected($event)" style="display:none" />
              <button class="btn-secondary" (click)="studentFileInput.click()">Choose CSV File</button>
              <span class="file-name" *ngIf="bulkStudentFile">{{ bulkStudentFile.name }}</span>
              <div class="panel-actions" style="margin-top:8px">
                <button class="btn-link" (click)="downloadStudentTemplate()">⬇ Download Template</button>
              </div>
            </div>

            <!-- Preview table -->
            <div *ngIf="bulkStudentPreview.length > 0" class="bulk-preview">
              <p class="bulk-preview-label">Preview — {{ bulkStudentPreview.length }} row(s)</p>
              <div class="bulk-table-wrap">
                <table class="data-table bulk-table">
                  <thead><tr><th>#</th><th>ID/Passport</th><th>First Name</th><th>Last Name</th><th>Email</th><th>Phone</th><th>Grade</th></tr></thead>
                  <tbody>
                    <tr *ngFor="let r of bulkStudentPreview; let i = index">
                      <td>{{ i + 1 }}</td><td>{{ r.idPassportNo }}</td><td>{{ r.firstName }}</td>
                      <td>{{ r.lastName }}</td><td>{{ r.email }}</td><td>{{ r.phone }}</td><td>{{ r.gradeName }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
              <div class="panel-actions" style="margin-top:12px">
                <button class="btn-primary" (click)="executeBulkStudents()" [disabled]="importingStudents">{{ importingStudents ? 'Importing…' : 'Import ' + bulkStudentPreview.length + ' Students' }}</button>
                <button class="btn-secondary" (click)="clearBulkStudents()">Clear</button>
              </div>
            </div>

            <!-- Import results -->
            <div *ngIf="bulkStudentResult" class="bulk-result">
              <div class="bulk-result-summary">
                <span class="result-ok">✓ {{ bulkStudentResult.successCount }} imported</span>
                <span *ngIf="bulkStudentResult.failureCount > 0" class="result-fail">✗ {{ bulkStudentResult.failureCount }} failed</span>
              </div>
              <div *ngIf="bulkStudentResult.failureCount > 0" class="bulk-table-wrap">
                <table class="data-table bulk-table">
                  <thead><tr><th>Row</th><th>Identifier</th><th>Error</th></tr></thead>
                  <tbody>
                    <tr *ngFor="let r of bulkStudentResult.results" [class.row-fail]="!r.success">
                      <td *ngIf="!r.success">{{ r.row }}</td>
                      <td *ngIf="!r.success">{{ r.identifier }}</td>
                      <td *ngIf="!r.success">{{ r.error }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
              <button class="btn-link" (click)="bulkStudentResult = null">Dismiss</button>
            </div>
          </div>

          <div class="search-bar">
            <input placeholder="Search by name or ID…" [(ngModel)]="studentSearch" />
          </div>
          <div *ngIf="!loading && filteredStudents.length === 0" class="empty-state">No students found.</div>
          <table *ngIf="filteredStudents.length > 0" class="data-table">
            <thead>
              <tr><th>Unique ID</th><th>Name</th><th>Email</th><th>Grade</th><th>Status</th><th>Assigned Teachers</th><th>Performance</th><th></th></tr>
            </thead>
            <tbody>
              <ng-container *ngFor="let s of filteredStudents">
                <tr>
                  <td><code>{{ s.studentUniqueId }}</code></td>
                  <td>{{ s.firstName }} {{ s.lastName }}</td>
                  <td>{{ s.email }}</td>
                  <td>{{ s.gradeName }}</td>
                  <td><span class="status-badge" [class.pending]="!s.isActive">{{ s.isActive ? 'Active' : 'Pending Activation' }}</span></td>
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
                  <td>
                    <button class="btn-edit-sm" (click)="openEditStudent(s)">Edit</button>
                    <button class="btn-warning-sm" title="Clear password — student must re-activate" (click)="resetStudentPassword(s)" [disabled]="resettingStudentId === s.id">{{ resettingStudentId === s.id ? 'Resetting…' : 'Reset Password' }}</button>
                    <button class="btn-danger-sm" (click)="confirmDeleteStudent(s)" [disabled]="deletingStudentId === s.id">{{ deletingStudentId === s.id ? 'Deleting…' : 'Delete' }}</button>
                  </td>
                </tr>
                <!-- Inline assign row -->
                <tr *ngIf="assigningStudentId === s.id" class="assign-row">
                  <td colspan="8">
                    <div class="assign-panel">
                      <label>Assign teacher to {{ s.firstName }} {{ s.lastName }}:</label>
                      <select [(ngModel)]="teacherToAssignId">
                        <option [ngValue]="0" disabled>-- Select teacher --</option>
                        <option *ngFor="let t of teachers" [ngValue]="t.teacherId">{{ t.firstName }} {{ t.lastName }} ({{ t.subjectName }})</option>
                      </select>
                      <button class="btn-primary" (click)="assignTeacher(s.id)" [disabled]="teacherToAssignId === 0 || assigningInProgress">{{ assigningInProgress ? 'Assigning…' : 'Assign' }}</button>
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
            <button class="btn-danger" (click)="executeDelete()" [disabled]="deleteInProgress">{{ deleteInProgress ? 'Deleting…' : 'Delete' }}</button>
            <button class="btn-secondary" (click)="cancelDelete()">Cancel</button>
          </div>
        </div>
      </div>

      <!-- Edit Teacher Modal -->
      <div *ngIf="editTeacherTarget" class="modal-overlay" (click)="cancelEditTeacher()">
        <div class="modal modal-wide" (click)="$event.stopPropagation()">
          <h3>Edit Teacher</h3>
          <div class="form-row">
            <div class="form-field"><label>ID / Passport No.</label><input [(ngModel)]="editTeacher.idPassportNo" maxlength="9" /></div>
            <div class="form-field"><label>First Name</label><input [(ngModel)]="editTeacher.firstName" /></div>
            <div class="form-field"><label>Last Name</label><input [(ngModel)]="editTeacher.lastName" /></div>
          </div>
          <div class="form-row">
            <div class="form-field"><label>Email</label><input type="email" [(ngModel)]="editTeacher.email" /></div>
            <div class="form-field"><label>Phone (8 digits)</label><input [(ngModel)]="editTeacher.phone" maxlength="8" /></div>
            <div class="form-field">
              <label>Subject</label>
              <select [(ngModel)]="editTeacher.subjectId">
                <option [ngValue]="0" disabled>-- Select --</option>
                <option *ngFor="let s of subjects" [ngValue]="s.id">{{ s.name }}</option>
              </select>
            </div>
          </div>
          <div *ngIf="editError" class="alert-error" style="margin-top:8px">{{ editError }}</div>
          <div class="modal-actions" style="margin-top:16px">
            <button class="btn-primary" (click)="saveEditTeacher()" [disabled]="savingEdit">{{ savingEdit ? 'Saving…' : 'Save Changes' }}</button>
            <button class="btn-secondary" (click)="cancelEditTeacher()">Cancel</button>
          </div>
        </div>
      </div>

      <!-- Edit Student Modal -->
      <div *ngIf="editStudentTarget" class="modal-overlay" (click)="cancelEditStudent()">
        <div class="modal modal-wide" (click)="$event.stopPropagation()">
          <h3>Edit Student</h3>
          <div class="form-row">
            <div class="form-field"><label>ID / Passport No.</label><input [(ngModel)]="editStudent.idPassportNo" maxlength="9" /></div>
            <div class="form-field"><label>First Name</label><input [(ngModel)]="editStudent.firstName" /></div>
            <div class="form-field"><label>Last Name</label><input [(ngModel)]="editStudent.lastName" /></div>
          </div>
          <div class="form-row">
            <div class="form-field"><label>Email</label><input type="email" [(ngModel)]="editStudent.email" /></div>
            <div class="form-field"><label>Phone (8 digits)</label><input [(ngModel)]="editStudent.phone" maxlength="8" /></div>
            <div class="form-field">
              <label>Grade</label>
              <select [(ngModel)]="editStudent.gradeId">
                <option [ngValue]="0" disabled>-- Select --</option>
                <option *ngFor="let g of grades" [ngValue]="g.id">{{ g.name }}</option>
              </select>
            </div>
          </div>
          <div *ngIf="editError" class="alert-error" style="margin-top:8px">{{ editError }}</div>
          <div class="modal-actions" style="margin-top:16px">
            <button class="btn-primary" (click)="saveEditStudent()" [disabled]="savingEdit">{{ savingEdit ? 'Saving…' : 'Save Changes' }}</button>
            <button class="btn-secondary" (click)="cancelEditStudent()">Cancel</button>
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
    .btn-warning-sm { padding: 4px 12px; background: #d97706; color: #fff; border: none; border-radius: 4px; cursor: pointer; font-size: 0.8rem; margin-right: 4px; }
    .btn-warning-sm:hover { background: #b45309; }
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
    .btn-edit-sm { padding: 4px 12px; background: #1e40af; color: #fff; border: none; border-radius: 4px; cursor: pointer; font-size: 0.8rem; margin-right: 4px; }
    .btn-edit-sm:hover { background: #1e3a8a; }
    .modal-wide { max-width: 620px; }
    button:disabled { opacity: 0.6; cursor: not-allowed; }

    /* Inline assign row */
    .assign-row td { background: #eef2ff; padding: 10px 14px !important; }
    .assign-panel { display: flex; align-items: center; gap: 10px; flex-wrap: wrap; }
    .assign-panel label { font-size: 0.85rem; color: #1a1a2e; font-weight: 500; }
    .assign-panel select { padding: 6px 10px; border: 1.5px solid #ddd; border-radius: 5px; font-size: 0.85rem; min-width: 220px; }

    /* Tab header actions */
    .tab-header-actions { display: flex; gap: 8px; }
    .btn-secondary-sm { padding: 6px 16px; background: #6b7280; color: #fff; border: none; border-radius: 6px; cursor: pointer; font-size: 0.85rem; font-weight: 600; }
    .btn-secondary-sm:hover { background: #4b5563; }

    /* Bulk import panel */
    .panel-bulk { border-color: #c6d4e8; }
    .bulk-mode-tabs { display: flex; gap: 4px; margin-bottom: 12px; }
    .bulk-mode-tabs button { padding: 5px 16px; background: #eef0f3; border: 1.5px solid #dde1e7; border-radius: 5px; cursor: pointer; font-size: 0.85rem; color: #555; }
    .bulk-mode-tabs button.active { background: #0f3460; color: #fff; border-color: #0f3460; }
    .bulk-paste-area textarea { width: 100%; padding: 10px; border: 1.5px solid #ddd; border-radius: 5px; font-family: monospace; font-size: 0.8rem; resize: vertical; box-sizing: border-box; }
    .bulk-file-area { display: flex; align-items: center; gap: 10px; flex-wrap: wrap; }
    .file-name { font-size: 0.82rem; color: #1a1a2e; background: #f0f2f5; padding: 4px 10px; border-radius: 4px; }
    .bulk-preview { margin-top: 16px; }
    .bulk-preview-label { font-size: 0.82rem; font-weight: 600; color: #555; margin: 0 0 8px; }
    .bulk-table-wrap { overflow-x: auto; max-height: 300px; overflow-y: auto; }
    .bulk-table { font-size: 0.8rem; }
    .bulk-table th, .bulk-table td { padding: 8px 10px; }
    .row-fail td { background: #fff5f5; color: #c53030; }
    .bulk-result { margin-top: 16px; padding: 14px; background: #f7f8fa; border-radius: 6px; border: 1.5px solid #dde1e7; }
    .bulk-result-summary { display: flex; gap: 16px; margin-bottom: 12px; font-weight: 600; font-size: 0.9rem; }
    .result-ok { color: #065f46; }
    .result-fail { color: #b91c1c; }
    .btn-link { background: none; border: none; color: #0f3460; cursor: pointer; font-size: 0.85rem; text-decoration: underline; padding: 4px 0; }
    .btn-link:hover { color: #1a5276; }
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

  // Per-operation in-progress guards (prevents double-click race conditions)
  submittingTeacher = false;
  submittingStudent = false;
  assigningInProgress = false;
  private unassigningInProgress = false;
  deletingTeacherId: number | null = null;
  deletingStudentId: number | null = null;
  resettingTeacherId: number | null = null;
  resettingStudentId: number | null = null;
  deleteInProgress = false;

  // Edit teacher state
  editTeacherTarget: any = null;
  editTeacher = { idPassportNo: '', firstName: '', lastName: '', email: '', phone: '', subjectId: 0 };

  // Edit student state
  editStudentTarget: any = null;
  editStudent = { idPassportNo: '', firstName: '', lastName: '', email: '', phone: '', gradeId: 0 };

  savingEdit = false;
  editError = '';

  // Bulk import — teachers
  showBulkTeacher = false;
  bulkTeacherMode: 'paste' | 'file' = 'paste';
  bulkTeacherCsvText = '';
  bulkTeacherFile: File | null = null;
  bulkTeacherPreview: any[] = [];
  bulkTeacherResult: any = null;
  importingTeachers = false;

  // Bulk import — students
  showBulkStudent = false;
  bulkStudentMode: 'paste' | 'file' = 'paste';
  bulkStudentCsvText = '';
  bulkStudentFile: File | null = null;
  bulkStudentPreview: any[] = [];
  bulkStudentResult: any = null;
  importingStudents = false;

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

  loadTeachersIfNeeded(): void {
    if (this.teachers.length > 0) return;
    this.loadTeachers();
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
    if (this.submittingTeacher) return;
    if (!this.newTeacher.idPassportNo || !this.newTeacher.firstName || !this.newTeacher.lastName
        || !this.newTeacher.email || !this.newTeacher.phone || this.newTeacher.subjectId === 0) {
      this.error = 'Please fill in all fields for the new teacher.';
      return;
    }
    if (!/^[a-zA-Z0-9\-]{9}$/.test(this.newTeacher.idPassportNo)) {
      this.error = 'ID/Passport No. must be exactly 9 characters (letters, numbers, hyphens only).';
      return;
    }
    if (!/^[a-zA-Z\s\-]{2,50}$/.test(this.newTeacher.firstName.trim())) {
      this.error = 'First name must be 2–50 characters (letters, spaces, hyphens only).';
      return;
    }
    if (!/^[a-zA-Z\s\-]{2,50}$/.test(this.newTeacher.lastName.trim())) {
      this.error = 'Last name must be 2–50 characters (letters, spaces, hyphens only).';
      return;
    }
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.newTeacher.email) || this.newTeacher.email.length > 255) {
      this.error = 'Please enter a valid email address (max 255 characters).';
      return;
    }
    if (!/^\d{8}$/.test(this.newTeacher.phone)) {
      this.error = 'Phone must be exactly 8 digits.';
      return;
    }
    this.submittingTeacher = true;
    this.loading = true; this.error = '';
    this.adminApi.createTeacher(this.newTeacher).subscribe({
      next: teacher => {
        this.teachers = [...this.teachers, teacher];
        this.showTeacherForm = false;
        this.resetNewTeacher();
        this.loading = false;
        this.submittingTeacher = false;
        this.showSuccess(`Teacher account created. Share email "${teacher.email}" so they can activate their account.`);
      },
      error: err => {
        this.error = err?.error?.message
          || (err?.error?.errors ? JSON.stringify(err.error.errors) : null)
          || 'Failed to create teacher.';
        this.loading = false;
        this.submittingTeacher = false;
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

  loadStudentsIfNeeded(): void {
    if (this.students.length > 0) return;
    this.loadStudents();
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
    if (this.submittingStudent) return;
    if (!this.newStudent.idPassportNo || !this.newStudent.firstName || !this.newStudent.lastName
        || !this.newStudent.email || !this.newStudent.phone || this.newStudent.gradeId === 0) {
      this.error = 'Please fill in all fields for the new student.';
      return;
    }
    if (!/^[a-zA-Z0-9\-]{9}$/.test(this.newStudent.idPassportNo)) {
      this.error = 'ID/Passport No. must be exactly 9 characters (letters, numbers, hyphens only).';
      return;
    }
    if (!/^[a-zA-Z\s\-]{2,50}$/.test(this.newStudent.firstName.trim())) {
      this.error = 'First name must be 2–50 characters (letters, spaces, hyphens only).';
      return;
    }
    if (!/^[a-zA-Z\s\-]{2,50}$/.test(this.newStudent.lastName.trim())) {
      this.error = 'Last name must be 2–50 characters (letters, spaces, hyphens only).';
      return;
    }
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.newStudent.email) || this.newStudent.email.length > 255) {
      this.error = 'Please enter a valid email address (max 255 characters).';
      return;
    }
    if (!/^\d{8}$/.test(this.newStudent.phone)) {
      this.error = 'Phone must be exactly 8 digits.';
      return;
    }
    this.submittingStudent = true;
    this.loading = true; this.error = '';
    this.adminApi.createStudent(this.newStudent).subscribe({
      next: student => {
        this.students = [...this.students, student];
        this.showStudentForm = false;
        this.resetNewStudent();
        this.loading = false;
        this.submittingStudent = false;
        this.showSuccess(`Student account created (ID: ${student.studentUniqueId}). Share their Unique ID and email so they can activate.`);
      },
      error: err => {
        this.error = err?.error?.message || 'Failed to create student.';
        this.loading = false;
        this.submittingStudent = false;
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
    if (!this.teacherToAssignId || this.assigningInProgress) return;
    this.assigningInProgress = true;
    const teacherId = this.teacherToAssignId;
    const teacher = this.teachers.find(t => t.teacherId === teacherId);
    this.adminApi.assignStudentToTeacher(studentId, teacherId).subscribe({
      next: () => {
        // Update local array without a full reload
        this.students = this.students.map(s => {
          if (s.id !== studentId) return s;
          const teachers = [...(s.teachers || []), {
            teacherId: teacher?.teacherId,
            fullName: `${teacher?.firstName ?? ''} ${teacher?.lastName ?? ''}`.trim(),
            firstName: teacher?.firstName,
            lastName: teacher?.lastName,
            subjectName: teacher?.subjectName
          }];
          return { ...s, teachers };
        });
        this.assigningStudentId = null;
        this.teacherToAssignId = 0;
        this.assigningInProgress = false;
        this.showSuccess('Teacher assigned successfully.');
      },
      error: err => {
        this.error = err?.error?.message || 'Failed to assign teacher.';
        this.assigningInProgress = false;
      }
    });
  }

  unassignTeacher(studentId: number, teacherId: number): void {
    if (this.unassigningInProgress) return;
    this.unassigningInProgress = true;
    this.adminApi.unassignStudentFromTeacher(studentId, teacherId).subscribe({
      next: () => {
        // Update local array without a full reload
        this.students = this.students.map(s => {
          if (s.id !== studentId) return s;
          return { ...s, teachers: (s.teachers || []).filter((t: any) => t.teacherId !== teacherId) };
        });
        this.unassigningInProgress = false;
        this.showSuccess('Teacher unassigned successfully.');
      },
      error: err => {
        this.error = err?.error?.message || 'Failed to unassign teacher.';
        this.unassigningInProgress = false;
      }
    });
  }


  loadAuditLogs(): void {
    this.loading = true; this.error = '';
    this.adminApi.getAuditLogs(1, 50).subscribe({
      next: data => {
        this.auditLogs = data;
        this.applyAuditFilter();
        this.loading = false;
      },
      error: () => { this.error = 'Failed to load audit logs.'; this.loading = false; }
    });
  }

  loadAuditLogsIfNeeded(): void {
    if (this.auditLogs.length > 0) return;
    this.loadAuditLogs();
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

  resetTeacherPassword(teacher: any): void {
    if (this.resettingTeacherId === teacher.teacherId) return;
    if (!confirm(`Reset password for ${teacher.firstName} ${teacher.lastName}? They will need to re-activate their account.`)) return;
    this.resettingTeacherId = teacher.teacherId;
    this.adminApi.resetTeacherPassword(teacher.teacherId).subscribe({
      next: () => {
        this.teachers = this.teachers.map(t =>
          t.teacherId === teacher.teacherId ? { ...t, isActive: false } : t
        );
        this.resettingTeacherId = null;
        this.showSuccess(`Password reset for ${teacher.firstName} ${teacher.lastName}. They must re-activate their account.`);
      },
      error: err => {
        this.error = err?.error?.message || 'Failed to reset password.';
        this.resettingTeacherId = null;
      }
    });
  }

  resetStudentPassword(student: any): void {
    if (this.resettingStudentId === student.id) return;
    if (!confirm(`Reset password for ${student.firstName} ${student.lastName}? They will need to re-activate their account.`)) return;
    this.resettingStudentId = student.id;
    this.adminApi.resetStudentPassword(student.id).subscribe({
      next: () => {
        this.resettingStudentId = null;
        this.showSuccess(`Password reset for ${student.firstName} ${student.lastName}. They must re-activate their account.`);
      },
      error: err => {
        this.error = err?.error?.message || 'Failed to reset password.';
        this.resettingStudentId = null;
      }
    });
  }

  // ─── Bulk Import — Teachers ───────────────────────────────────────────────

  toggleBulkTeacher(): void {
    this.showBulkTeacher = !this.showBulkTeacher;
    if (!this.showBulkTeacher) this.clearBulkTeachers();
  }

  clearBulkTeachers(): void {
    this.bulkTeacherCsvText = '';
    this.bulkTeacherFile = null;
    this.bulkTeacherPreview = [];
    this.bulkTeacherResult = null;
  }

  onTeacherFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    this.bulkTeacherFile = input.files[0];
    this.readFileAsCsv(this.bulkTeacherFile, parsed => {
      this.bulkTeacherPreview = this.parseCsvToTeacherRows(parsed);
    });
  }

  previewBulkTeachers(): void {
    if (!this.bulkTeacherCsvText.trim()) { this.error = 'Paste CSV content first.'; return; }
    this.bulkTeacherPreview = this.parseCsvToTeacherRows(this.bulkTeacherCsvText);
    if (this.bulkTeacherPreview.length === 0) this.error = 'No valid rows found in pasted CSV.';
  }

  executeBulkTeachers(): void {
    if (this.importingTeachers || !this.bulkTeacherPreview.length) return;
    this.importingTeachers = true;
    const payload = this.bulkTeacherPreview;
    this.adminApi.bulkImportTeachers(payload).subscribe({
      next: result => {
        this.bulkTeacherResult = result;
        this.importingTeachers = false;
        if (result.successCount > 0) {
          this.showSuccess(`Bulk import: ${result.successCount} teacher(s) created.`);
          // Reload teacher list
          this.adminApi.getAllTeachers().subscribe({ next: data => this.teachers = data });
        }
        this.bulkTeacherPreview = [];
      },
      error: err => {
        this.error = err?.error?.message || 'Bulk import failed.';
        this.importingTeachers = false;
      }
    });
  }

  downloadTeacherTemplate(): void {
    const csv = 'IdPassportNo,FirstName,LastName,Email,Phone,SubjectName\n';
    this.triggerCsvDownload(csv, 'teacher_import_template.csv');
  }

  // ─── Bulk Import — Students ───────────────────────────────────────────────

  toggleBulkStudent(): void {
    this.showBulkStudent = !this.showBulkStudent;
    if (!this.showBulkStudent) this.clearBulkStudents();
  }

  clearBulkStudents(): void {
    this.bulkStudentCsvText = '';
    this.bulkStudentFile = null;
    this.bulkStudentPreview = [];
    this.bulkStudentResult = null;
  }

  onStudentFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    this.bulkStudentFile = input.files[0];
    this.readFileAsCsv(this.bulkStudentFile, parsed => {
      this.bulkStudentPreview = this.parseCsvToStudentRows(parsed);
    });
  }

  previewBulkStudents(): void {
    if (!this.bulkStudentCsvText.trim()) { this.error = 'Paste CSV content first.'; return; }
    this.bulkStudentPreview = this.parseCsvToStudentRows(this.bulkStudentCsvText);
    if (this.bulkStudentPreview.length === 0) this.error = 'No valid rows found in pasted CSV.';
  }

  executeBulkStudents(): void {
    if (this.importingStudents || !this.bulkStudentPreview.length) return;
    this.importingStudents = true;
    const payload = this.bulkStudentPreview;
    this.adminApi.bulkImportStudents(payload).subscribe({
      next: result => {
        this.bulkStudentResult = result;
        this.importingStudents = false;
        if (result.successCount > 0) {
          this.showSuccess(`Bulk import: ${result.successCount} student(s) created.`);
          // Reload student list
          this.adminApi.getAllStudents().subscribe({ next: data => this.students = data });
        }
        this.bulkStudentPreview = [];
      },
      error: err => {
        this.error = err?.error?.message || 'Bulk import failed.';
        this.importingStudents = false;
      }
    });
  }

  downloadStudentTemplate(): void {
    const csv = 'IdPassportNo,FirstName,LastName,Email,Phone,GradeName\n';
    this.triggerCsvDownload(csv, 'student_import_template.csv');
  }

  // ─── CSV helpers ──────────────────────────────────────────────────────────

  private readFileAsCsv(file: File, onLoaded: (text: string) => void): void {
    const reader = new FileReader();
    reader.onload = e => onLoaded(e.target?.result as string || '');
    reader.readAsText(file);
  }

  private parseCsvToTeacherRows(csvText: string): any[] {
    const lines = csvText.split(/\r?\n/).map(l => l.trim()).filter(l => l.length > 0);
    if (lines.length < 2) return [];
    const headers = this.splitCsvRow(lines[0]).map(h => h.toLowerCase());
    const idx = (name: string) => headers.indexOf(name);
    return lines.slice(1).map(line => {
      const cols = this.splitCsvRow(line);
      return {
        idPassportNo: cols[idx('idpassportno')] ?? '',
        firstName:    cols[idx('firstname')]    ?? '',
        lastName:     cols[idx('lastname')]     ?? '',
        email:        cols[idx('email')]        ?? '',
        phone:        cols[idx('phone')]        ?? '',
        subjectName:  cols[idx('subjectname')]  ?? ''
      };
    }).filter(r => r.idPassportNo || r.email);
  }

  private parseCsvToStudentRows(csvText: string): any[] {
    const lines = csvText.split(/\r?\n/).map(l => l.trim()).filter(l => l.length > 0);
    if (lines.length < 2) return [];
    const headers = this.splitCsvRow(lines[0]).map(h => h.toLowerCase());
    const idx = (name: string) => headers.indexOf(name);
    return lines.slice(1).map(line => {
      const cols = this.splitCsvRow(line);
      return {
        idPassportNo: cols[idx('idpassportno')] ?? '',
        firstName:    cols[idx('firstname')]    ?? '',
        lastName:     cols[idx('lastname')]     ?? '',
        email:        cols[idx('email')]        ?? '',
        phone:        cols[idx('phone')]        ?? '',
        gradeName:    cols[idx('gradename')]    ?? ''
      };
    }).filter(r => r.idPassportNo || r.email);
  }

  private splitCsvRow(row: string): string[] {
    // Simple CSV split — handles quoted fields with commas inside
    const result: string[] = [];
    let current = '';
    let inQuotes = false;
    for (let i = 0; i < row.length; i++) {
      const ch = row[i];
      if (ch === '"') {
        if (inQuotes && row[i + 1] === '"') { current += '"'; i++; }
        else inQuotes = !inQuotes;
      } else if (ch === ',' && !inQuotes) {
        result.push(current.trim());
        current = '';
      } else {
        current += ch;
      }
    }
    result.push(current.trim());
    return result;
  }

  private triggerCsvDownload(csvContent: string, filename: string): void {
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = filename; a.click();
    URL.revokeObjectURL(url);
  }

  // ─── Edit Teacher ─────────────────────────────────────────────────────────
  openEditTeacher(teacher: any): void {
    if (this.subjects.length === 0) {
      this.adminApi.getSubjects().subscribe({ next: data => this.subjects = data });
    }
    this.editTeacherTarget = teacher;
    this.editTeacher = {
      idPassportNo: teacher.idPassportNo,
      firstName: teacher.firstName,
      lastName: teacher.lastName,
      email: teacher.email,
      phone: teacher.phone,
      subjectId: teacher.subjectId
    };
    this.editError = '';
  }

  cancelEditTeacher(): void {
    this.editTeacherTarget = null;
    this.editError = '';
  }

  saveEditTeacher(): void {
    if (this.savingEdit) return;
    if (!this.editTeacher.idPassportNo || !this.editTeacher.firstName || !this.editTeacher.lastName
        || !this.editTeacher.email || !this.editTeacher.phone || this.editTeacher.subjectId === 0) {
      this.editError = 'Please fill in all fields.';
      return;
    }
    this.savingEdit = true; this.editError = '';
    this.adminApi.updateTeacher(this.editTeacherTarget.teacherId, this.editTeacher).subscribe({
      next: updated => {
        this.teachers = this.teachers.map(t =>
          t.teacherId === this.editTeacherTarget.teacherId ? { ...t, ...updated } : t
        );
        this.savingEdit = false;
        this.editTeacherTarget = null;
        this.showSuccess('Teacher updated successfully.');
      },
      error: err => {
        this.editError = err?.error?.message || 'Failed to update teacher.';
        this.savingEdit = false;
      }
    });
  }

  // ─── Edit Student ─────────────────────────────────────────────────────────

  openEditStudent(student: any): void {
    if (this.grades.length === 0) {
      this.adminApi.getGrades().subscribe({ next: data => this.grades = data });
    }
    this.editStudentTarget = student;
    this.editStudent = {
      idPassportNo: student.idPassportNo,
      firstName: student.firstName,
      lastName: student.lastName,
      email: student.email,
      phone: student.phone,
      gradeId: student.gradeId
    };
    this.editError = '';
  }

  cancelEditStudent(): void {
    this.editStudentTarget = null;
    this.editError = '';
  }

  saveEditStudent(): void {
    if (this.savingEdit) return;
    if (!this.editStudent.idPassportNo || !this.editStudent.firstName || !this.editStudent.lastName
        || !this.editStudent.email || !this.editStudent.phone || this.editStudent.gradeId === 0) {
      this.editError = 'Please fill in all fields.';
      return;
    }
    this.savingEdit = true; this.editError = '';
    this.adminApi.updateStudent(this.editStudentTarget.id, this.editStudent).subscribe({
      next: updated => {
        this.students = this.students.map(s =>
          s.id === this.editStudentTarget.id ? { ...s, ...updated } : s
        );
        this.savingEdit = false;
        this.editStudentTarget = null;
        this.showSuccess('Student updated successfully.');
      },
      error: err => {
        this.editError = err?.error?.message || 'Failed to update student.';
        this.savingEdit = false;
      }
    });
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
    if (!this.deleteTarget || this.deleteInProgress) return;
    const { type, id } = this.deleteTarget;
    const obs = type === 'teacher'
      ? this.adminApi.deleteTeacher(id)
      : this.adminApi.deleteStudent(id);

    this.deleteInProgress = true;
    if (type === 'teacher') this.deletingTeacherId = id;
    else this.deletingStudentId = id;

    obs.subscribe({
      next: () => {
        this.showSuccess(`${type === 'teacher' ? 'Teacher' : 'Student'} deleted successfully.`);
        this.deleteTarget = null;
        this.deleteInProgress = false;
        this.deletingTeacherId = null;
        this.deletingStudentId = null;
        if (type === 'teacher') {
          this.teachers = this.teachers.filter(t => t.teacherId !== id);
        } else {
          this.students = this.students.filter(s => s.id !== id);
        }
      },
      error: err => {
        this.error = err?.error?.message || 'Delete failed.';
        this.deleteTarget = null;
        this.deleteInProgress = false;
        this.deletingTeacherId = null;
        this.deletingStudentId = null;
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
