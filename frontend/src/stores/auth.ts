import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '@/api/auth'
import { ApiError, http } from '@/api/http'
import { errorMessage } from '@/composables/useAsyncAction'
import type { Schemas } from '@/api/types'

type AuthUser = Schemas['AuthUserDto']

/**
 * 登入狀態只存在這個 store（記憶體），不寫入 localStorage；重新整理頁面後以 refresh cookie 還原（ADR-010）。
 * 長效的 refresh token 是 httpOnly cookie，頁面上的腳本讀不到；access token 只有 15 分鐘效期。
 */
export const useAuthStore = defineStore('auth', () => {
  const user = ref<AuthUser | null>(null)
  const accessToken = ref<string | null>(null)
  const error = ref<string | null>(null)
  const isLoading = ref(false)
  let restoring: Promise<void> | null = null
  let expiredHandler: (() => void) | null = null

  const isAuthenticated = computed(() => user.value !== null && accessToken.value !== null)
  const isAdmin = computed(() => user.value?.role === 'Admin')
  const userDisplayName = computed(() => user.value?.fullName || user.value?.username || '')

  function startSession(session: Schemas['AccessTokenDto']) {
    accessToken.value = session.accessToken
    user.value = session.user
  }

  function clearSession() {
    accessToken.value = null
    user.value = null
  }

  async function refreshAccessToken(): Promise<string> {
    const session = await authApi.refresh()
    if (!session) throw new ApiError('請先登入', 401)
    startSession(session)
    return session.accessToken
  }

  http.setAuthHooks({
    getAccessToken: () => accessToken.value,
    refreshAccessToken,
    onSessionExpired: () => {
      clearSession()
      expiredHandler?.()
    },
  })

  async function login(credentials: Schemas['LoginRequest']): Promise<boolean> {
    isLoading.value = true
    error.value = null
    try {
      startSession(await authApi.login(credentials))
      return true
    } catch (e) {
      error.value = e instanceof ApiError ? errorMessage(e) : '登入失敗，請稍後再試'
      return false
    } finally {
      isLoading.value = false
    }
  }

  /** 註冊成功後直接登入。 */
  async function register(request: Schemas['RegisterRequest']): Promise<boolean> {
    isLoading.value = true
    error.value = null
    try {
      startSession(await authApi.register(request))
      return true
    } catch (e) {
      error.value = errorMessage(e)
      return false
    } finally {
      isLoading.value = false
    }
  }

  /** 伺服器端撤銷 refresh token；即使連不上伺服器，本機也一律登出。 */
  async function logout() {
    try {
      await authApi.logout()
    } catch {
      // 本機狀態照樣清除；伺服器端的 token 會在到期後失效
    } finally {
      clearSession()
    }
  }

  /** App 啟動時呼叫；沒有 refresh cookie（訪客，伺服器回 204）或 cookie 已失效都維持登出，不視為錯誤。多次呼叫只會 refresh 一次。 */
  function restoreSession(): Promise<void> {
    restoring ??= refreshAccessToken().then(
      () => undefined,
      () => clearSession(),
    )
    return restoring
  }

  function onSessionExpired(handler: () => void) {
    expiredHandler = handler
  }

  function clearError() {
    error.value = null
  }

  return {
    user,
    accessToken,
    error,
    isLoading,
    isAuthenticated,
    isAdmin,
    userDisplayName,
    login,
    register,
    logout,
    restoreSession,
    refreshAccessToken,
    onSessionExpired,
    clearSession,
    clearError,
  }
})
