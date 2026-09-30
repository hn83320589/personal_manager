// API Response Types
export interface ApiResponse<T = any> {
  success: boolean
  message: string
  data: T
  errors: string[]
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

// ===== Auth =====
export interface LoginRequest {
  username: string
  password: string
}

export interface RegisterRequest {
  username: string
  email: string
  password: string
  fullName?: string
}

export interface AuthResponse {
  userId: number
  token: string
  username: string
  email: string
  fullName: string
  role: string
  expiresAt: string
  refreshToken: string
  refreshTokenExpiresAt: string
}

// ===== User =====
export interface User {
  id: number
  username: string
  email: string
  fullName: string
  role: string
  isActive: boolean
  createdAt: string
}

export interface PublicUser {
  id: number
  username: string
  fullName: string
}

export interface ProfileDirectory {
  userId: number
  username: string
  fullName: string
  title: string
  summary: string
  profileImageUrl: string
  location: string
  themeColor: string
}

// ===== PersonalProfile =====
export interface PersonalProfile {
  id: number
  userId: number
  title: string
  summary: string
  description: string
  profileImageUrl: string
  website: string
  location: string
  themeColor: string
  createdAt: string
  updatedAt: string
}

// ===== Education =====
export interface Education {
  id: number
  userId: number
  school: string
  degree: string
  fieldOfStudy: string
  startYear?: number
  endYear?: number
  description: string
  isPublic: boolean
  sortOrder: number
}

// ===== WorkExperience =====
export interface WorkExperience {
  id: number
  userId: number
  company: string
  position: string
  startDate?: string
  endDate?: string
  isCurrent: boolean
  description: string
  isPublic: boolean
  sortOrder: number
}

// ===== Skill =====
export type SkillLevel = 'Beginner' | 'Intermediate' | 'Advanced' | 'Expert'

export interface Skill {
  id: number
  userId: number
  name: string
  category: string
  level: SkillLevel
  yearsOfExperience: number
  isPublic: boolean
  sortOrder: number
}

// ===== Portfolio =====
export interface Portfolio {
  id: number
  userId: number
  title: string
  description: string
  imageUrl: string
  projectUrl: string
  repositoryUrl: string
  technologies: string
  isFeatured: boolean
  isPublic: boolean
  sortOrder: number
  createdAt: string
}

// ===== CalendarEvent =====
export interface CalendarEvent {
  id: number
  userId: number
  title: string
  description: string
  startTime: string
  endTime: string
  isAllDay: boolean
  isPublic: boolean
  color: string
  recurrenceRule: string
  createdAt: string
}

// ===== Project =====
export interface Project {
  id: number
  userId: number
  name: string
  description: string
  color: string
  sortOrder: number
  createdAt: string
  updatedAt: string
}

// ===== TodoItem =====
export type TodoPriority = 'Low' | 'Medium' | 'High'
export type TodoStatus = 'Pending' | 'InProgress' | 'Completed'

export interface TodoItem {
  id: number
  userId: number
  title: string
  description: string
  priority: TodoPriority
  status: TodoStatus
  dueDate?: string
  completedAt?: string
  createdAt: string
  updatedAt: string
}

// ===== WorkTask =====
export type WorkTaskStatus = 'Pending' | 'Planning' | 'InProgress' | 'Testing' | 'Completed' | 'OnHold' | 'Cancelled'
export type WorkTaskPriority = 'Low' | 'Medium' | 'High' | 'Urgent'

export interface WorkTask {
  id: number
  userId: number
  title: string
  description: string
  projectId?: number | null
  projectName?: string | null
  priority: WorkTaskPriority
  status: WorkTaskStatus
  estimatedHours: number
  actualHours: number
  dueDate?: string
  completedAt?: string
  tags: string
  createdAt: string
  updatedAt: string
}

// ===== BlogPost =====
export type BlogPostStatus = 'Draft' | 'Published' | 'Archived'

export interface BlogPost {
  id: number
  userId: number
  title: string
  slug: string
  content: string
  summary: string
  category: string
  tags: string[]
  status: BlogPostStatus
  isPublic: boolean
  viewCount: number
  publishedAt?: string
  createdAt: string
  updatedAt: string
}

// ===== GuestBookEntry =====
export interface GuestBookEntry {
  id: number
  targetUserId: number
  name: string
  email: string
  message: string
  isApproved: boolean
  adminReply: string
  createdAt: string
}

// ===== FileUpload =====
export type FileUploadType = 'image' | 'pdf' | 'document' | 'presentation' | 'other'

export interface FileUpload {
  id: number
  userId: number
  fileName: string
  storedName: string
  fileUrl: string
  fileType: FileUploadType
  fileSize: number
  mimeType: string
  createdAt: string
}

// ===== PortfolioAttachment =====
export interface PortfolioAttachment {
  id: number
  portfolioId: number
  fileUploadId: number | null
  fileName: string
  fileUrl: string
  fileType: string
  fileSize: number
  sortOrder: number
  createdAt: string
}

export interface CreatePortfolioAttachmentDto {
  portfolioId: number
  fileUploadId?: number | null
  fileName: string
  fileUrl: string
  fileType: string
  fileSize: number
  sortOrder: number
}

// ===== ContactMethod =====
export type ContactType = 'Email' | 'Phone' | 'LinkedIn' | 'GitHub' | 'Facebook' | 'Twitter' | 'Instagram' | 'Discord' | 'Other'

export interface ContactMethod {
  id: number
  userId: number
  type: ContactType
  label: string
  value: string
  icon: string
  isPublic: boolean
  sortOrder: number
}

// ===== TimeEntry =====
export interface TimeEntry {
  id: number
  userId: number
  workTaskId?: number
  task: string
  project: string
  date: string
  startTime?: string
  endTime?: string
  duration: number
  description: string
  createdAt: string
  updatedAt: string
}

export interface CreateTimeEntryDto {
  workTaskId?: number
  task: string
  project?: string
  date: string
  startTime?: string
  endTime?: string
  duration: number
  description?: string
}

export interface UpdateTimeEntryDto {
  workTaskId?: number
  task?: string
  project?: string
  date?: string
  startTime?: string
  endTime?: string
  duration?: number
  description?: string
}
