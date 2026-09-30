import { fileURLToPath } from 'node:url'
import { mergeConfig, defineConfig, configDefaults } from 'vitest/config'
import viteConfig from './vite.config'

// 固定時區，日期相關的測試在任何機器（包含 CI）結果都相同
process.env.TZ = 'Asia/Taipei'

export default mergeConfig(
  viteConfig,
  defineConfig({
    test: {
      environment: 'jsdom',
      environmentOptions: {
        jsdom: {
          url: 'http://localhost:3000',
        },
      },
      exclude: [...configDefaults.exclude, 'e2e/**'],
      root: fileURLToPath(new URL('./', import.meta.url)),
      setupFiles: ['./src/test-utils/setup.ts'],
      coverage: {
        provider: 'v8',
        reporter: ['text', 'json', 'html'],
        exclude: [
          ...(configDefaults.coverage?.exclude ?? []),
          'src/test-utils/**',
          '**/*.spec.ts',
          '**/*.test.ts',
        ],
      },
      globals: true,
    },
  }),
)
