import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminApiService, AuditLogDto } from '../../core/services/http/admin-api.service';

type TabType = 'teachers' | 'students' | 'audit';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.css']
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
