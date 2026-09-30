import { beforeEach, vi } from 'vitest'
import { config } from '@vue/test-utils'
import { createPinia } from 'pinia'

// 全域測試設定

// Mock 瀏覽器 APIs
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation((query) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(), // Deprecated
    removeListener: vi.fn(), // Deprecated
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
})

// 可實際讀寫的記憶體版 Storage（每個測試前清空），讓依賴 localStorage 的程式能被真實地測試
class MemoryStorage implements Storage {
  private items = new Map<string, string>()
  get length() {
    return this.items.size
  }
  clear() {
    this.items.clear()
  }
  getItem(key: string) {
    return this.items.get(key) ?? null
  }
  key(index: number) {
    return [...this.items.keys()][index] ?? null
  }
  removeItem(key: string) {
    this.items.delete(key)
  }
  setItem(key: string, value: string) {
    this.items.set(key, String(value))
  }
}
const memoryLocalStorage = new MemoryStorage()
const memorySessionStorage = new MemoryStorage()
Object.defineProperty(window, 'localStorage', { value: memoryLocalStorage })
Object.defineProperty(window, 'sessionStorage', { value: memorySessionStorage })
Object.defineProperty(globalThis, 'localStorage', { value: memoryLocalStorage })
Object.defineProperty(globalThis, 'sessionStorage', { value: memorySessionStorage })

// Mock window.location
Object.defineProperty(window, 'location', {
  value: {
    href: 'http://localhost:3000',
    origin: 'http://localhost:3000',
    protocol: 'http:',
    host: 'localhost:3000',
    hostname: 'localhost',
    port: '3000',
    pathname: '/',
    search: '',
    hash: '',
    reload: vi.fn(),
    assign: vi.fn(),
    replace: vi.fn(),
  },
  writable: true,
})

// Vue Test Utils 全域配置
config.global.plugins = [createPinia()]

// 每個測試前重置 mocks 與瀏覽器儲存
beforeEach(() => {
  vi.clearAllMocks()
  memoryLocalStorage.clear()
  memorySessionStorage.clear()
})
