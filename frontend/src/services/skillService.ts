import httpService from './http'
import type { ApiResponse, Skill } from '@/types/api'

class SkillService {
  async getSkills(): Promise<ApiResponse<Skill[]>> {
    return httpService.get<Skill[]>('/skills')
  }

  async getSkillById(id: number): Promise<ApiResponse<Skill>> {
    return httpService.get<Skill>(`/skills/${id}`)
  }

  async getSkillsByUserId(userId: number): Promise<ApiResponse<Skill[]>> {
    return httpService.get<Skill[]>(`/skills/user/${userId}`)
  }

  async getPublicSkillsByUserId(userId: number): Promise<ApiResponse<Skill[]>> {
    return httpService.get<Skill[]>(`/skills/user/${userId}/public`)
  }

  async getSkillsByCategory(userId: number, category: string): Promise<ApiResponse<Skill[]>> {
    return httpService.get<Skill[]>(`/skills/user/${userId}/category/${encodeURIComponent(category)}`)
  }

  async createSkill(skill: Partial<Skill>): Promise<ApiResponse<Skill>> {
    return httpService.post<Skill>('/skills', skill)
  }

  async updateSkill(id: number, skill: Partial<Skill>): Promise<ApiResponse<Skill>> {
    return httpService.put<Skill>(`/skills/${id}`, skill)
  }

  async deleteSkill(id: number): Promise<ApiResponse<void>> {
    return httpService.delete<void>(`/skills/${id}`)
  }
}

export const skillService = new SkillService()
export default skillService
