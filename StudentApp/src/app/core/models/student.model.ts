/**
 * DOMAIN LAYER - Student Models
 * Pure TypeScript interfaces representing domain entities
 */

/** Grade lookup entry returned by GET /api/grades */
export interface GradeDto {
  id: number;
  name: string;  // e.g., "Grade 10"
  level: number;
}

/** Single assessment record for a student */
export interface StudentAssessmentDto {
  id: number;
  studentId: number;
  name: string;
  maxScore: number;
  score: number;
  dueDate: string | null;
  isAssigned: boolean;
  instructions: string | null;
  submissionCount: number;
  createdAt: string;
  updatedAt: string;
}

/** Summary of a teacher assignment on a student record */
export interface TeacherSummaryDto {
  teacherId: number;
  fullName: string;
  subjectName: string;
}

/** Core Student entity */
export interface Student {
  id: number;
  studentUniqueId: string;
  idPassportNo: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  gradeId: number;
  gradeName: string;
  /** Issue 5 fix: replaced stale single teacherId with the many-to-many array */
  teachers: TeacherSummaryDto[];
  assessments: StudentAssessmentDto[];
  createdAt: string;
  updatedAt?: string;
}

/** Student List DTO - Data for list table view */
export interface StudentListDto {
  id: number;
  studentUniqueId: string;
  firstName: string;
  lastName: string;
  email: string;
  gradeName: string;
  totalScore: number;
  maxPossible: number;
  percentage: number;
  performanceLevel: string;
  assessmentCount?: number;
  /** Assigned teachers — used when displaying teacher column in the list */
  teachers?: TeacherSummaryDto[];
}

/** Student Detail DTO - Complete data with calculated performance fields */
export interface StudentDetailDto {
  id: number;
  studentUniqueId: string;
  idPassportNo: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  gradeId: number;
  gradeName: string;
  /** Issue 5 fix: replaced stale single teacherId with the many-to-many array */
  teachers: TeacherSummaryDto[];
  assessments: StudentAssessmentDto[];
  totalScore: number;
  maxPossible: number;
  averageScore: number;
  percentage: number;
  performanceLevel: string;
  createdAt: string;
}

/** Create Student DTO - assessments are added separately after creation */
export interface CreateStudentDto {
  idPassportNo: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  gradeId: number;
  /** Issue 5 fix: teacherId removed — backend derives it from the JWT claim */
}

/** Update Student DTO - profile fields only, teacher assignments managed via separate endpoints */
export interface UpdateStudentDto {
  idPassportNo: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  gradeId: number;
}

/** Create assessment DTO */
export interface CreateStudentAssessmentDto {
  name: string;
  maxScore: number;
  score: number;
  dueDate?: string | null;
  isAssigned?: boolean;
  instructions?: string | null;
}

/** Update assessment DTO */
export interface UpdateStudentAssessmentDto {
  name: string;
  maxScore: number;
  score: number;
  dueDate?: string | null;
  isAssigned?: boolean;
  instructions?: string | null;
}

// ── Student Authentication ──────────────────────────────────────────────────

/** Safe student profile returned after login / activation (mirrors StudentProfileDto from API) */
export interface StudentAuthUser {
  id: number;
  studentUniqueId: string;
  idPassportNo: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  gradeId: number;
  gradeName: string;
  teachers: TeacherSummaryDto[];
  assessments: StudentAssessmentDto[];
  totalScore: number;
  maxPossible: number;
  averageScore: number;
  percentage: number;
  performanceLevel: string;
  createdAt: string;
  updatedAt?: string;
}

/** Minimal teacher reference embedded in student responses */
export interface TeacherSummaryDto {
  teacherId: number;
  fullName: string;
  subjectName: string;
}

/** DTO sent to POST /api/students/activate */
export interface StudentActivateDto {
  studentUniqueId: string;
  email: string;
  password: string;
  confirmPassword: string;
}

/** DTO sent to POST /api/students/login */
export interface StudentLoginDto {
  studentUniqueId: string;
  password: string;
}

/** Shape returned by POST /api/students/activate and POST /api/students/login */
export interface StudentLoginResponseDto {
  token: string;
  student: StudentAuthUser;
}

/** Read-only DTO for a file submission uploaded by a student */
export interface AssessmentSubmissionDto {
  id: number;
  studentAssessmentId: number;
  studentId: number;
  fileName: string;
  contentType: string;
  fileSize: number;
  submittedAt: string;
}
