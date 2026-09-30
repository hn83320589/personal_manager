import httpService from './http'
import type { ApiResponse, TodoItem, WorkTask, TodoStatus, WorkTaskStatus } from '@/types/api'

class TaskService {
  // Todo Items
  async getTodoItems(): Promise<ApiResponse<TodoItem[]>> {
    return httpService.get<TodoItem[]>('/todoitems')
  }

  async getTodoItemById(id: number): Promise<ApiResponse<TodoItem>> {
    return httpService.get<TodoItem>(`/todoitems/${id}`)
  }

  async getTodoItemsByUserId(userId: number): Promise<ApiResponse<TodoItem[]>> {
    return httpService.get<TodoItem[]>(`/todoitems/user/${userId}`)
  }

  async getTodoItemsByStatus(userId: number, status: TodoStatus): Promise<ApiResponse<TodoItem[]>> {
    return httpService.get<TodoItem[]>(`/todoitems/user/${userId}/status/${status}`)
  }

  async createTodoItem(todo: Partial<TodoItem>): Promise<ApiResponse<TodoItem>> {
    return httpService.post<TodoItem>('/todoitems', todo)
  }

  async updateTodoItem(id: number, todo: Partial<TodoItem>): Promise<ApiResponse<TodoItem>> {
    return httpService.put<TodoItem>(`/todoitems/${id}`, todo)
  }

  async deleteTodoItem(id: number): Promise<ApiResponse<void>> {
    return httpService.delete<void>(`/todoitems/${id}`)
  }

  // Work Tasks
  async getWorkTasks(): Promise<ApiResponse<WorkTask[]>> {
    return httpService.get<WorkTask[]>('/worktasks')
  }

  async getWorkTaskById(id: number): Promise<ApiResponse<WorkTask>> {
    return httpService.get<WorkTask>(`/worktasks/${id}`)
  }

  async getWorkTasksByUserId(userId: number): Promise<ApiResponse<WorkTask[]>> {
    return httpService.get<WorkTask[]>(`/worktasks/user/${userId}`)
  }

  async getWorkTasksByProject(userId: number, project: string): Promise<ApiResponse<WorkTask[]>> {
    return httpService.get<WorkTask[]>(`/worktasks/user/${userId}/project/${encodeURIComponent(project)}`)
  }

  async getWorkTasksByStatus(userId: number, status: WorkTaskStatus): Promise<ApiResponse<WorkTask[]>> {
    return httpService.get<WorkTask[]>(`/worktasks/user/${userId}/status/${status}`)
  }

  async createWorkTask(task: Partial<WorkTask>): Promise<ApiResponse<WorkTask>> {
    return httpService.post<WorkTask>('/worktasks', task)
  }

  async updateWorkTask(id: number, task: Partial<WorkTask>): Promise<ApiResponse<WorkTask>> {
    return httpService.put<WorkTask>(`/worktasks/${id}`, task)
  }

  async deleteWorkTask(id: number): Promise<ApiResponse<void>> {
    return httpService.delete<void>(`/worktasks/${id}`)
  }
}

export const taskService = new TaskService()
export default taskService
