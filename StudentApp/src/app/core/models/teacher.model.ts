/**
 * DOMAIN LAYER - Teacher Models
 * Pure TypeScript interfaces representing domain entities
 * No dependencies on services or components
 */

/**
 * Subject lookup DTO — returned by GET /api/subjects
 */
export interface SubjectDto {
  id: number;
  name: string;
}

/**
 * Core Teacher entity - matches backend domain model
 */
export interface Teacher {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  subjectId: number;
  subjectName: string;
  createdAt: string;
  updatedAt?: string;
}

/**
 * Create Teacher DTO - Data required for teacher registration
 */
export interface CreateTeacherDto {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  subjectId: number;
  password: string;
}

/**
 * Login DTO - Credentials for authentication
 */
export interface LoginDto {
  email: string;
  password: string;
}

/**
 * Login Response - shape returned by POST /api/teachers/login
 */
export interface TeacherLoginResponse {
  token: string;
  teacher: {
    teacherId: number;
    firstName: string;
    lastName: string;
    email: string;
    phone: string;
    subjectId: number;
    subjectName: string;
    enrollmentDate: string;
    createdDate: string;
  };
}

/**
 * Update Teacher DTO - Data for updating teacher profile
 */
export interface UpdateTeacherDto {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  subjectId: number;
}
