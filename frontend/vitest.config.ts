import { fileURLToPath } from 'node:url'
import { mergeConfig, defineConfig, configDefaults } from 'vitest/config'
import viteConfig from './vite.config'

export default mergeConfig(
  viteConfig as any,
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
      globalSetup: ['./src/test-utils/globalSetup.ts'],
      setupFiles: ['./src/test-utils/setup.ts'],
      pool: 'threads',
      poolOptions: {
        threads: {
          execArgv: ['--localstorage-file', '/tmp/vitest-localstorage.json'],
        },
      },
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
