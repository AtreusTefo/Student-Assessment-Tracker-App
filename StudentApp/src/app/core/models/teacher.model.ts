/**
 * DOMAIN LAYER - Teacher Models
 * Pure TypeScript interfaces representing domain entities
 * No dependencies on services or components
 */

/**
 * Core Teacher entity - matches backend domain model
 */
export interface Teacher {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  subject: string;
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
  subject: string;
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
    subject: string;
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
  subject: string;
}
