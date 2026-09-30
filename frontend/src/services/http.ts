import axios from 'axios'
import type { AxiosInstance, AxiosResponse, AxiosError } from 'axios'
import type { ApiResponse } from '@/types/api'
import { useAuthStore } from '@/stores/auth'

class HttpService {
  private client: AxiosInstance

  constructor() {
    this.client = axios.create({
      baseURL: import.meta.env.VITE_API_BASE_URL,
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json',
        Accept: 'application/json',
      },
      // Performance optimizations
      maxContentLength: 10 * 1024 * 1024, // 10MB
      maxBodyLength: 10 * 1024 * 1024, // 10MB
      // Enable compression
      decompress: true,
    })

    this.setupInterceptors()
  }

  private setupInterceptors() {
    // Request interceptor
    this.client.interceptors.request.use(
      (config) => {
        // Add auth token if available
        const token = this.getAuthToken()
        if (token) {
          config.headers.Authorization = `Bearer ${token}`
        }

        if (import.meta.env.VITE_DEBUG === 'true') {
          console.log('[HTTP Request]', config)
        }

        return config
      },
      (error) => {
        console.error('[HTTP Request Error]', error)
        return Promise.reject(error)
      },
    )

    // Response interceptor
    this.client.interceptors.response.use(
      (response: AxiosResponse<ApiResponse>) => {
        if (import.meta.env.VITE_DEBUG === 'true') {
          console.log('[HTTP Response]', response)
        }

        return response
      },
      async (error: AxiosError<ApiResponse>) => {
        const originalRequest = error.config as any

        if (error.response?.status === 401) {
          // Don't retry if already retried, or if this is the refresh/logout endpoint
          const isAuthEndpoint =
            originalRequest?.url?.includes('/auth/refresh') ||
            originalRequest?.url?.includes('/auth/logout')

          if (!originalRequest?._isRetry && !isAuthEndpoint) {
            originalRequest._isRetry = true
            try {
              const newToken = await this.tryRefreshToken()
              originalRequest.headers = originalRequest.headers || {}
              originalRequest.headers.Authorization = `Bearer ${newToken}`
              return this.client.request(originalRequest)
            } catch {
              this.handleUnauthorized()
            }
          } else {
            this.handleUnauthorized()
          }
        } else if (error.response?.status === 403) {
          console.error('Access forbidden')
        } else if (error.response && error.response.status >= 500) {
          console.error('Server error occurred')
        }

        return Promise.reject(error)
      },
    )
  }

  // 舊頁面的過渡：登入狀態一律由 auth store 管理（記憶體 + refresh cookie），
  // 不再讀寫 localStorage。Phase 5 各頁改用 src/api 後刪除本檔。
  private getAuthToken(): string | null {
    return useAuthStore().accessToken
  }

  private tryRefreshToken(): Promise<string> {
    return useAuthStore().refreshAccessToken()
  }

  private handleUnauthorized() {
    useAuthStore().clearSession()
  }

  // Request retry mechanism
  private async retryRequest<T>(
    requestFn: () => Promise<AxiosResponse<ApiResponse<T>>>,
    maxRetries = 3,
    delay = 1000,
  ): Promise<ApiResponse<T>> {
    let lastError: any

    for (let attempt = 1; attempt <= maxRetries; attempt++) {
      try {
        const response = await requestFn()
        return response.data
      } catch (error) {
        lastError = error

        // Don't retry on client errors (4xx)
        if (error instanceof Error && 'response' in error) {
          const axiosError = error as AxiosError
          if (axiosError.response?.status && axiosError.response.status < 500) {
            throw error
          }
        }

        if (attempt < maxRetries) {
          await new Promise((resolve) => setTimeout(resolve, delay * attempt))
        }
      }
    }

    throw lastError
  }

  // Generic HTTP methods with retry
  async get<T>(url: string, params?: any): Promise<ApiResponse<T>> {
    return this.retryRequest(() => this.client.get<ApiResponse<T>>(url, { params }))
  }

  async post<T>(url: string, data?: any): Promise<ApiResponse<T>> {
    return this.retryRequest(() => this.client.post<ApiResponse<T>>(url, data))
  }

  async put<T>(url: string, data?: any): Promise<ApiResponse<T>> {
    return this.retryRequest(() => this.client.put<ApiResponse<T>>(url, data))
  }

  async delete<T>(url: string): Promise<ApiResponse<T>> {
    return this.retryRequest(() => this.client.delete<ApiResponse<T>>(url))
  }

  async patch<T>(url: string, data?: any): Promise<ApiResponse<T>> {
    return this.retryRequest(() => this.client.patch<ApiResponse<T>>(url, data))
  }

  // File upload method
  async uploadFile<T>(url: string, formData: FormData): Promise<ApiResponse<T>> {
    const response = await this.client.post<ApiResponse<T>>(url, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    })
    return response.data
  }
}

// Export singleton instance
export const httpService = new HttpService()
export default httpService
