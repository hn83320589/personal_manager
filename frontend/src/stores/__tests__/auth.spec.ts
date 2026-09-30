import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '../auth'

// Mock authService
vi.mock('@/services/authService', () => ({
  default: {
    login: vi.fn(),
    logout: vi.fn(),
    getCurrentUser: vi.fn(),
    getCurrentUserData: vi.fn(),
    getAuthToken: vi.fn(),
    isAuthenticated: vi.fn(),
  }
}))

describe('AuthStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('初始狀態', () => {
    it('應該有正確的初始狀態', () => {
      const store = useAuthStore()
      
      expect(store.user).toBeNull()
      expect(store.token).toBeNull()
      expect(store.isAuthenticated).toBe(false)
      expect(store.userRole).toBe('guest')
      expect(store.isLoading).toBe(false)
      expect(store.error).toBeNull()
    })
  })

  describe('getters', () => {
    it('isAuthenticated 應該基於 token 和 user 狀態', () => {
      const store = useAuthStore()
      
      // 初始狀態
      expect(store.isAuthenticated).toBe(false)
      
      // 設定 token 和 user
      store.token = 'test-token'
      store.user = { id: 1, username: 'testuser', email: 'test@example.com', role: 'user', isActive: true, createdAt: '2024-01-01', fullName: 'Test User' }
      expect(store.isAuthenticated).toBe(true)
      
      // 只有 token
      store.user = null
      expect(store.isAuthenticated).toBe(false)
    })

    it('userRole 應該返回使用者角色', () => {
      const store = useAuthStore()
      
      expect(store.userRole).toBe('guest')
      
      store.user = { id: 1, username: 'admin', email: 'admin@example.com', role: 'admin', isActive: true, createdAt: '2024-01-01', fullName: 'Test User' }
      expect(store.userRole).toBe('admin')
    })

    it('userDisplayName 應該返回使用者名稱', () => {
      const store = useAuthStore()
      
      expect(store.userDisplayName).toBe('Guest')
      
      store.user = { id: 1, username: 'testuser', email: 'test@example.com', role: 'user', isActive: true, createdAt: '2024-01-01', fullName: 'Test User' }
      expect(store.userDisplayName).toBe('testuser')
    })

    it('hasPermission 應該檢查權限', () => {
      const store = useAuthStore()
      
      // 無使用者
      expect(store.hasPermission('test')).toBe(false)
      
      // 一般使用者
      store.user = { id: 1, username: 'user', email: 'user@example.com', role: 'user', isActive: true, createdAt: '2024-01-01', fullName: 'Test User' }
      expect(store.hasPermission('test')).toBe(false)
      
      // 管理員
      store.user = { id: 1, username: 'admin', email: 'admin@example.com', role: 'admin', isActive: true, createdAt: '2024-01-01', fullName: 'Test User' }
      expect(store.hasPermission('test')).toBe(true)
    })
  })

  describe('actions', () => {
    it('clearError 應該清除錯誤訊息', () => {
      const store = useAuthStore()
      
      store.error = 'some error'
      store.clearError()
      
      expect(store.error).toBeNull()
    })

    it('logout 應該清除認證資料', async () => {
      const store = useAuthStore()
      
      // 設定一些資料
      store.user = { id: 1, username: 'testuser', email: 'test@example.com', role: 'user', isActive: true, createdAt: '2024-01-01', fullName: 'Test User' }
      store.token = 'test-token'
      store.error = 'some error'
      
      await store.logout()
      
      expect(store.user).toBeNull()
      expect(store.token).toBeNull()
      expect(store.error).toBeNull()
      expect(store.isLoading).toBe(false)
    })
  })
})