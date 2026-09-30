import httpService from './http'
import type { Project } from '@/types/api'

export interface CreateProjectDto {
  name: string
  description?: string
  color?: string
  sortOrder?: number
}

export interface UpdateProjectDto {
  name?: string
  description?: string
  color?: string
  sortOrder?: number
}

export const projectService = {
  getAll: () => httpService.get<Project[]>('/projects'),
  getByUserId: (userId: number) => httpService.get<Project[]>(`/projects/user/${userId}`),
  getById: (id: number) => httpService.get<Project>(`/projects/${id}`),
  create: (dto: CreateProjectDto) => httpService.post<Project>('/projects', dto),
  update: (id: number, dto: UpdateProjectDto) => httpService.put<Project>(`/projects/${id}`, dto),
  delete: (id: number) => httpService.delete<void>(`/projects/${id}`),
}
