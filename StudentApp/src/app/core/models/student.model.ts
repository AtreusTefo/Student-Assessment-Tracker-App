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
  createdAt: string;
  updatedAt: string;
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
  teacherId: number;
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
  teacherId: number;
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
  teacherId: number;
}

/** Update Student DTO - profile fields only, TeacherId cannot be changed */
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
}

/** Update assessment DTO */
export interface UpdateStudentAssessmentDto {
  name: string;
  maxScore: number;
  score: number;
  dueDate?: string | null;
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
  teacherId: number;
  assessments: StudentAssessmentDto[];
  totalScore: number;
  maxPossible: number;
  averageScore: number;
  percentage: number;
  performanceLevel: string;
  createdAt: string;
  updatedAt?: string;
}

/** DTO sent to POST /api/students/activate */
export interface StudentActivateDto {
  studentUniqueId: string;
  email: string;
  password: string;
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
