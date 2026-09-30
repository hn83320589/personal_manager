import httpService from './http'
import type { ApiResponse, Education, WorkExperience } from '@/types/api'

class ExperienceService {
  // Education API methods
  async getEducations(): Promise<ApiResponse<Education[]>> {
    return httpService.get<Education[]>('/educations')
  }

  async getEducationById(id: number): Promise<ApiResponse<Education>> {
    return httpService.get<Education>(`/educations/${id}`)
  }

  async getEducationsByUserId(userId: number): Promise<ApiResponse<Education[]>> {
    return httpService.get<Education[]>(`/educations/user/${userId}`)
  }

  async getPublicEducationsByUserId(userId: number): Promise<ApiResponse<Education[]>> {
    return httpService.get<Education[]>(`/educations/user/${userId}/public`)
  }

  async createEducation(education: Partial<Education>): Promise<ApiResponse<Education>> {
    return httpService.post<Education>('/educations', education)
  }

  async updateEducation(id: number, education: Partial<Education>): Promise<ApiResponse<Education>> {
    return httpService.put<Education>(`/educations/${id}`, education)
  }

  async deleteEducation(id: number): Promise<ApiResponse<void>> {
    return httpService.delete<void>(`/educations/${id}`)
  }

  // Work Experience API methods
  async getWorkExperiences(): Promise<ApiResponse<WorkExperience[]>> {
    return httpService.get<WorkExperience[]>('/workexperiences')
  }

  async getWorkExperienceById(id: number): Promise<ApiResponse<WorkExperience>> {
    return httpService.get<WorkExperience>(`/workexperiences/${id}`)
  }

  async getWorkExperiencesByUserId(userId: number): Promise<ApiResponse<WorkExperience[]>> {
    return httpService.get<WorkExperience[]>(`/workexperiences/user/${userId}`)
  }

  async getPublicWorkExperiencesByUserId(userId: number): Promise<ApiResponse<WorkExperience[]>> {
    return httpService.get<WorkExperience[]>(`/workexperiences/user/${userId}/public`)
  }

  async createWorkExperience(experience: Partial<WorkExperience>): Promise<ApiResponse<WorkExperience>> {
    return httpService.post<WorkExperience>('/workexperiences', experience)
  }

  async updateWorkExperience(id: number, experience: Partial<WorkExperience>): Promise<ApiResponse<WorkExperience>> {
    return httpService.put<WorkExperience>(`/workexperiences/${id}`, experience)
  }

  async deleteWorkExperience(id: number): Promise<ApiResponse<void>> {
    return httpService.delete<void>(`/workexperiences/${id}`)
  }
}

export const experienceService = new ExperienceService()
export default experienceService
