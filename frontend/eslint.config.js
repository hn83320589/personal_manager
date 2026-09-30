import pluginVue from 'eslint-plugin-vue'
import { defineConfigWithVueTs, vueTsConfigs } from '@vue/eslint-config-typescript'
import skipFormatting from '@vue/eslint-config-prettier/skip-formatting'

// 格式交給 Prettier（npm run format），ESLint 只檢查程式碼品質
export default defineConfigWithVueTs(
  {
    name: 'app/files-to-lint',
    files: ['**/*.{ts,mts,tsx,vue}'],
  },
  {
    name: 'app/files-to-ignore',
    ignores: ['**/dist/**', '**/coverage/**', 'src/api/schema.ts'],
  },
  pluginVue.configs['flat/essential'],
  vueTsConfigs.recommended,
  {
    name: 'app/rules',
    rules: {
      '@typescript-eslint/no-unused-vars': [
        'error',
        { argsIgnorePattern: '^_', varsIgnorePattern: '^_', ignoreRestSiblings: true },
      ],
    },
  },
  {
    // Phase 5 依新設計重寫的舊畫面：暫時容許 any，重寫時從清單移除（docs/TASKS.md Phase 5）
    name: 'app/legacy-screens',
    files: [
      'src/views/admin/**',
      'src/views/*.vue',
      'src/components/{admin,blog,calendar,layout,task,work,ui,common}/**',
      'src/stores/blog.ts',
      'src/stores/calendar.ts',
      'src/stores/comment.ts',
      'src/stores/portfolio.ts',
      'src/stores/profile.ts',
      'src/stores/task.ts',
      'src/services/**',
      'src/types/api.ts',
    ],
    rules: { '@typescript-eslint/no-explicit-any': 'warn' },
  },
  skipFormatting,
)
