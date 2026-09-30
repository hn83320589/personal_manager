import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import authService from '@/services/authService'
import type { User } from '@/types/api'

export const useAuthStore = defineStore('auth', () => {
  // State
  const user = ref<User | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  const token = ref<string | null>(null)

  // Getters
  const isAuthenticated = computed(() => !!(user.value && token.value))
  const userDisplayName = computed(() => user.value?.username || 'Guest')
  const userRole = computed(() => user.value?.role || 'guest')
  const hasPermission = computed(() => (_permission: string) => {
    return userRole.value === 'admin' || userRole.value === 'owner'
  })

  // Actions
  async function login(credentials: { username: string; password: string }) {
    isLoading.value = true
    error.value = null

    try {
      const response = await authService.login(credentials)

      if (response.success && response.data) {
        token.value = response.data.token
        // Fetch full user profile after login
        await fetchCurrentUser()
        return true
      } else {
        error.value = response.message || 'Login failed'
        return false
      }
    } catch (err) {
      console.error('Login error:', err)
      error.value = 'Network error occurred'
      return false
    } finally {
      isLoading.value = false
    }
  }

  async function logout() {
    isLoading.value = true

    try {
      await authService.logout()
    } catch (err) {
      console.error('Logout error:', err)
    } finally {
      user.value = null
      token.value = null
      error.value = null
      isLoading.value = false
    }
  }

  async function fetchCurrentUser() {
    try {
      const response = await authService.getCurrentUser()
      if (response.success && response.data) {
        user.value = response.data
      }
    } catch (err) {
      console.error('Failed to fetch current user:', err)
    }
  }

  function initializeAuth() {
    const currentToken = authService.getAuthToken()

    if (currentToken && authService.isAuthenticated()) {
      token.value = currentToken
      // Restore user data from localStorage
      const userData = authService.getCurrentUserData()
      if (userData) {
        user.value = userData as User
      }
    }
  }

  function clearError() {
    error.value = null
  }

  return {
    // State
    user,
    isLoading,
    error,
    token,
    // Getters
    isAuthenticated,
    userDisplayName,
    userRole,
    hasPermission,
    // Actions
    login,
    logout,
    fetchCurrentUser,
    initializeAuth,
    clearError,
  }
})
