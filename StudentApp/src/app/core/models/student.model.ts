/**
 * DOMAIN LAYER - Student Models
 * Pure TypeScript interfaces representing domain entities
 * No dependencies on services or components
 */

/**
 * Core Student entity - matches backend domain model
 */
export interface Student {
  id: number;
  studentUniqueId: string;
  idPassportNo: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  grade: string;
  assessment1: number;
  assessment2: number;
  assessment3: number;
  createdAt: string;
  updatedAt?: string;
}

/**
 * Student List DTO - Minimal data for list views
 */
export interface StudentListDto {
  id: number;
  firstName: string;
  lastName: string;
}

/**
 * Student Detail DTO - Complete data with calculated fields
 */
export interface StudentDetailDto {
  id: number;
  studentUniqueId: string;
  idPassportNo: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  grade: string;
  createdAt: string;
  assessment1: number;
  assessment2: number;
  assessment3: number;
  totalScore: number;
  averageScore: number;
  percentage: number;
  performanceLevel: string;
}

/**
 * Create Student DTO - Data required for creating a new student
 */
export interface CreateStudentDto {
  idPassportNo: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  grade: string;
  assessment1: number;
  assessment2: number;
  assessment3: number;
}

/**
 * Update Student DTO - Data required for updating a student
 */
export interface UpdateStudentDto {
  idPassportNo: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  grade: string;
  assessment1: number;
  assessment2: number;
  assessment3: number;
}
