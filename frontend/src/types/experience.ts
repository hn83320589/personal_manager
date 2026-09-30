// Re-export types from api.ts with aliases for better naming
export type { Education, WorkExperience, User } from "./api";

// Extended interfaces for UI forms
export interface EducationForm {
  id?: number;
  userId?: number;
  institution: string; // maps to school
  degree: string;
  fieldOfStudy?: string;
  gpa?: string;
  location?: string;
  startDate: string; // maps to startYear
  endDate?: string; // maps to endYear
  description?: string;
  websiteUrl?: string;
  sortOrder: number;
  isPublic: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface WorkExperienceForm {
  id?: number;
  userId?: number;
  company: string;
  position: string;
  department?: string;
  employmentType?: string;
  location?: string;
  startDate: string;
  endDate?: string;
  isCurrent: boolean;
  description?: string;
  skills?: string | string[];
  websiteUrl?: string;
  sortOrder: number;
  isPublic: boolean;
  createdAt?: string;
  updatedAt?: string;
}
