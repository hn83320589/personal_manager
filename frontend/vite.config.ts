import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// 把 API 與上傳的檔案轉送到後端，瀏覽器看到的是同一個網站（與正式環境的反向代理相同）
const backendProxy = {
  '/api': 'http://localhost:5037',
  '/files': 'http://localhost:5037',
}

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue({
      script: {
        defineModel: true,
        propsDestructure: true,
      },
    }),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  build: {
    target: 'esnext',
    rollupOptions: {
      output: {
        manualChunks: {
          'vendor-vue': ['vue', 'vue-router', 'pinia'],
          'vendor-ui': ['@heroicons/vue'],
          'vendor-http': ['axios'],
        },
      },
    },
    sourcemap: false,
    cssCodeSplit: true,
    chunkSizeWarningLimit: 1000,
  },
  server: {
    port: 5173,
    strictPort: true,
    hmr: false,
    proxy: backendProxy,
  },
  // 預覽建置結果（E2E 在 CI 使用）時同樣轉送
  preview: {
    port: 4173,
    proxy: backendProxy,
  },
  optimizeDeps: {
    include: [
      'vue',
      'vue-router',
      'pinia',
      'axios',
      '@heroicons/vue/24/outline',
      '@heroicons/vue/24/solid',
    ],
  },
})
