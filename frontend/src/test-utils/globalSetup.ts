// Global setup: runs in Node.js context before any test environment
// Needed because Node.js 22+ has a built-in global `localStorage` stub,
// but without --localstorage-file it lacks proper methods like getItem/setItem.
// @vue/devtools-kit calls localStorage.getItem at module init time, so we
// must polyfill before any test modules are loaded.

export function setup() {
  if (typeof globalThis.localStorage === 'undefined' || typeof (globalThis.localStorage as Storage).getItem !== 'function') {
    const store = new Map<string, string>()
    const mockStorage = {
      getItem: (key: string) => store.get(key) ?? null,
      setItem: (key: string, value: string) => { store.set(key, String(value)) },
      removeItem: (key: string) => { store.delete(key) },
      clear: () => { store.clear() },
      key: (index: number) => [...store.keys()][index] ?? null,
      get length() { return store.size },
    }
    Object.defineProperty(globalThis, 'localStorage', { value: mockStorage, writable: true, configurable: true })
    Object.defineProperty(globalThis, 'sessionStorage', { value: mockStorage, writable: true, configurable: true })
  }
}
