import axios, {
  AxiosError,
  type AxiosAdapter,
  type AxiosInstance,
  type AxiosRequestConfig,
  type InternalAxiosRequestConfig,
} from 'axios'

/** API 回傳的錯誤。status 為 0 表示連不到伺服器。 */
export class ApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly errors: string[] = [],
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

/**
 * 由 auth store 提供。http 層只負責在適當時機呼叫，不保存任何登入狀態：
 * access token 只在記憶體，refresh token 是瀏覽器自動送出的 httpOnly cookie（ADR-010）。
 */
export interface AuthHooks {
  getAccessToken(): string | null
  /** 以 refresh cookie 換新的 access token；無法續期時丟出錯誤。 */
  refreshAccessToken(): Promise<string>
  onSessionExpired(): void
}

interface HttpClientOptions {
  baseURL: string
  adapter?: AxiosAdapter
  maxRetries?: number
  retryDelayMs?: number
}

interface RequestState {
  retryCount?: number
  refreshed?: boolean
}

type TrackedConfig = InternalAxiosRequestConfig & { _state?: RequestState }

const unreachableMessage = '無法連線到伺服器，請稍後再試'

export function createHttpClient({
  baseURL,
  adapter,
  maxRetries = 2,
  retryDelayMs = 400,
}: HttpClientOptions) {
  const instance: AxiosInstance = axios.create({
    baseURL,
    adapter,
    timeout: 15_000,
    withCredentials: true,
  })
  let auth: AuthHooks | null = null
  let pendingRefresh: Promise<string> | null = null

  instance.interceptors.request.use((config) => {
    const token = auth?.getAccessToken()
    if (token) config.headers.set('Authorization', `Bearer ${token}`)
    return config
  })

  instance.interceptors.response.use(undefined, async (error: unknown) => {
    if (!(error instanceof AxiosError) || !error.config) throw toApiError(error)
    const config = error.config as TrackedConfig
    const state = (config._state ??= {})
    const status = error.response?.status ?? 0

    if (status === 401 && auth && !state.refreshed && !isAuthRequest(config)) {
      state.refreshed = true
      await refreshOnce(auth)
      return instance.request(config)
    }

    if (isRetryable(config, status) && (state.retryCount ?? 0) < maxRetries) {
      state.retryCount = (state.retryCount ?? 0) + 1
      await delay(retryDelayMs * state.retryCount)
      return instance.request(config)
    }

    throw toApiError(error)
  })

  /** 同時失敗的多個請求共用同一次 refresh，refresh token 輪換後舊的就不能再用。 */
  async function refreshOnce(hooks: AuthHooks): Promise<string> {
    pendingRefresh ??= hooks.refreshAccessToken().finally(() => (pendingRefresh = null))
    try {
      return await pendingRefresh
    } catch {
      hooks.onSessionExpired()
      throw new ApiError('登入已過期，請重新登入', 401)
    }
  }

  async function send<T>(config: AxiosRequestConfig): Promise<T> {
    const response = await instance.request<{ data: T }>(config)
    return response.data.data
  }

  return {
    setAuthHooks(hooks: AuthHooks) {
      auth = hooks
    },
    get: <T>(url: string, params?: object) => send<T>({ method: 'GET', url, params }),
    post: <T = void>(url: string, body?: unknown) => send<T>({ method: 'POST', url, data: body }),
    put: <T = void>(url: string, body?: unknown) => send<T>({ method: 'PUT', url, data: body }),
    delete: <T = void>(url: string) => send<T>({ method: 'DELETE', url }),
    /** 上傳檔案：不設逾時（大檔案可能要數分鐘），可回報進度 0–1。 */
    upload: <T>(url: string, form: FormData, onProgress?: (ratio: number) => void) =>
      send<T>({
        method: 'POST',
        url,
        data: form,
        timeout: 0,
        onUploadProgress: (e) => onProgress?.(e.total ? e.loaded / e.total : 0),
      }),
  }
}

export type HttpClient = ReturnType<typeof createHttpClient>

/** 登入、refresh 等請求本身回 401 代表帳密或 cookie 無效，不能再用 refresh 處理。 */
function isAuthRequest(config: InternalAxiosRequestConfig) {
  return config.url?.startsWith('/auth/') ?? false
}

/** 只重試 GET：寫入請求在伺服器可能已處理，重送會造成重複資料。 */
function isRetryable(config: InternalAxiosRequestConfig, status: number) {
  const isGet = (config.method ?? 'get').toLowerCase() === 'get'
  return isGet && (status === 0 || status >= 500)
}

function toApiError(error: unknown): ApiError {
  if (error instanceof ApiError) return error
  if (error instanceof AxiosError) {
    const response = error.response
    if (!response) return new ApiError(unreachableMessage, 0)
    if (response.status === 429) return new ApiError('操作太頻繁了，請稍候一分鐘再試', 429)
    const body = response.data as { message?: string; errors?: string[] } | undefined
    return new ApiError(
      body?.message || `請求失敗（${response.status}）`,
      response.status,
      body?.errors ?? [],
    )
  }
  return new ApiError(error instanceof Error ? error.message : '發生未預期的錯誤', 0)
}

function delay(ms: number) {
  return ms > 0 ? new Promise((resolve) => setTimeout(resolve, ms)) : Promise.resolve()
}

export const http = createHttpClient({ baseURL: import.meta.env.VITE_API_BASE_URL || '/api' })
