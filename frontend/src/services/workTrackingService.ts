import taskService from './taskService'
import type { ApiResponse, WorkTask } from '@/types/api'

// WorkTrackingService delegates to TaskService for work task operations
class WorkTrackingService {
  async getWorkTasks(): Promise<ApiResponse<WorkTask[]>> {
    return taskService.getWorkTasks()
  }

  async getWorkTaskById(id: number): Promise<ApiResponse<WorkTask>> {
    return taskService.getWorkTaskById(id)
  }

  async getWorkTasksByUserId(userId: number): Promise<ApiResponse<WorkTask[]>> {
    return taskService.getWorkTasksByUserId(userId)
  }

  async createWorkTask(task: Partial<WorkTask>): Promise<ApiResponse<WorkTask>> {
    return taskService.createWorkTask(task)
  }

  async updateWorkTask(id: number, task: Partial<WorkTask>): Promise<ApiResponse<WorkTask>> {
    return taskService.updateWorkTask(id, task)
  }

  async deleteWorkTask(id: number): Promise<ApiResponse<void>> {
    return taskService.deleteWorkTask(id)
  }
}

export const workTrackingService = new WorkTrackingService()
export default workTrackingService
