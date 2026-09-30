import { computed } from 'vue'

type ThemeName = 'blue' | 'green' | 'purple' | 'rose' | 'slate'

const themes: Record<ThemeName, Record<string, string>> = {
  blue: {
    '--color-primary': '#0ea5e9',
    '--color-primary-hover': '#0284c7',
    '--color-primary-light': '#e0f2fe',
    '--color-primary-dark': '#0c4a6e',
    '--color-primary-rgb': '14, 165, 233',
  },
  green: {
    '--color-primary': '#22c55e',
    '--color-primary-hover': '#16a34a',
    '--color-primary-light': '#dcfce7',
    '--color-primary-dark': '#14532d',
    '--color-primary-rgb': '34, 197, 94',
  },
  purple: {
    '--color-primary': '#a855f7',
    '--color-primary-hover': '#9333ea',
    '--color-primary-light': '#f3e8ff',
    '--color-primary-dark': '#581c87',
    '--color-primary-rgb': '168, 85, 247',
  },
  rose: {
    '--color-primary': '#f43f5e',
    '--color-primary-hover': '#e11d48',
    '--color-primary-light': '#ffe4e6',
    '--color-primary-dark': '#881337',
    '--color-primary-rgb': '244, 63, 94',
  },
  slate: {
    '--color-primary': '#64748b',
    '--color-primary-hover': '#475569',
    '--color-primary-light': '#f1f5f9',
    '--color-primary-dark': '#0f172a',
    '--color-primary-rgb': '100, 116, 139',
  },
}

export function useTheme(themeColor: string) {
  const themeVars = computed(() => {
    const key = themeColor as ThemeName
    return themes[key] ?? themes.blue
  })

  return { themeVars }
}

export { themes }
export type { ThemeName }
