import httpService from './http'
import type { ApiResponse, AuthResponse, User } from '@/types/api'

class AuthService {
  async login(credentials: { username: string; password: string }): Promise<ApiResponse<AuthResponse>> {
    const response = await httpService.post<AuthResponse>('/auth/login', credentials)

    if (response.success && response.data) {
      this.saveAuthData(response.data)
    }

    return response
  }

  async register(data: { username: string; email: string; password: string; fullName?: string }): Promise<ApiResponse<AuthResponse>> {
    const response = await httpService.post<AuthResponse>('/auth/register', data)

    if (response.success && response.data) {
      this.saveAuthData(response.data)
    }

    return response
  }

  async getCurrentUser(): Promise<ApiResponse<User>> {
    return httpService.get<User>('/auth/me')
  }

  async forgotPassword(email: string): Promise<ApiResponse<null>> {
    return httpService.post<null>('/auth/forgot-password', { email })
  }

  async resetPassword(token: string, newPassword: string): Promise<ApiResponse<null>> {
    return httpService.post<null>('/auth/reset-password', { token, newPassword })
  }

  async logout(): Promise<void> {
    const refreshToken = localStorage.getItem('refresh_token')
    if (refreshToken) {
      try {
        await httpService.post('/auth/logout', { refreshToken })
      } catch { /* ignore — best effort */ }
    }
    this.clearAuthData()
  }

  isAuthenticated(): boolean {
    const token = localStorage.getItem('auth_token')
    if (!token) return false

    const expiryTime = localStorage.getItem('token_expiry')
    if (expiryTime && Date.now() > parseInt(expiryTime)) {
      // Access token expired, but refresh token might still be valid
      // Let the interceptor handle the refresh
      const refreshToken = localStorage.getItem('refresh_token')
      if (!refreshToken) {
        this.clearAuthData()
        return false
      }
    }

    return true
  }

  getCurrentUserData(): Partial<User> | null {
    const userData = localStorage.getItem('user_data')
    if (userData) {
      try {
        return JSON.parse(userData)
      } catch {
        return null
      }
    }
    return null
  }

  getAuthToken(): string | null {
    return localStorage.getItem('auth_token')
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refresh_token')
  }

  private saveAuthData(data: AuthResponse): void {
    localStorage.setItem('auth_token', data.token)
    localStorage.setItem('refresh_token', data.refreshToken)
    localStorage.setItem('user_data', JSON.stringify({
      id: data.userId,
      username: data.username,
      email: data.email,
      fullName: data.fullName,
      role: data.role,
    }))
    if (data.expiresAt) {
      localStorage.setItem('token_expiry', new Date(data.expiresAt).getTime().toString())
    }
  }

  clearAuthData(): void {
    localStorage.removeItem('auth_token')
    localStorage.removeItem('refresh_token')
    localStorage.removeItem('user_data')
    localStorage.removeItem('token_expiry')
  }
}

export const authService = new AuthService()
export default authService
