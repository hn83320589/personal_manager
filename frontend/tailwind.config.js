import typography from '@tailwindcss/typography'

/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
  theme: {
    extend: {
      // 設計 token（src/assets/tokens.css），會隨深淺色與主題色切換
      colors: {
        paper: 'rgb(var(--paper) / <alpha-value>)',
        surface: 'rgb(var(--surface) / <alpha-value>)',
        ink: 'rgb(var(--ink) / <alpha-value>)',
        muted: 'rgb(var(--muted) / <alpha-value>)',
        rule: 'rgb(var(--rule) / <alpha-value>)',
        soft: 'rgb(var(--soft) / <alpha-value>)',
        accent: {
          DEFAULT: 'rgb(var(--accent) / <alpha-value>)',
          ink: 'rgb(var(--accent-ink) / <alpha-value>)',
        },
        danger: 'rgb(var(--danger) / <alpha-value>)',
        success: 'rgb(var(--success) / <alpha-value>)',
        primary: {
          50: '#f0f9ff',
          100: '#e0f2fe',
          200: '#bae6fd',
          300: '#7dd3fc',
          400: '#38bdf8',
          500: '#0ea5e9',
          600: '#0284c7',
          700: '#0369a1',
          800: '#075985',
          900: '#0c4a6e',
        },
        gray: {
          50: '#f9fafb',
          100: '#f3f4f6',
          200: '#e5e7eb',
          300: '#d1d5db',
          400: '#9ca3af',
          500: '#6b7280',
          600: '#4b5563',
          700: '#374151',
          800: '#1f2937',
          900: '#111827',
        },
      },
      fontFamily: {
        sans: [
          '"Noto Sans TC"',
          'system-ui',
          '-apple-system',
          '"PingFang TC"',
          '"Microsoft JhengHei"',
          'sans-serif',
        ],
        hand: ['"LXGW WenKai TC"', '"Noto Serif TC"', 'serif'],
        latin: ['"Schibsted Grotesk"', '"Noto Sans TC"', 'system-ui', 'sans-serif'],
        mono: ['"JetBrains Mono"', 'ui-monospace', 'Consolas', 'monospace'],
      },
      maxWidth: {
        wrap: '70rem',
        read: '42rem',
      },
      borderRadius: {
        card: '10px',
      },
      // 文章與作品文字區塊（prose）使用設計 token，深淺色自動切換
      typography: {
        DEFAULT: {
          css: {
            '--tw-prose-body': 'rgb(var(--ink))',
            '--tw-prose-headings': 'rgb(var(--ink))',
            '--tw-prose-lead': 'rgb(var(--muted))',
            '--tw-prose-links': 'rgb(var(--accent))',
            '--tw-prose-bold': 'rgb(var(--ink))',
            '--tw-prose-counters': 'rgb(var(--muted))',
            '--tw-prose-bullets': 'rgb(var(--muted))',
            '--tw-prose-hr': 'rgb(var(--rule))',
            '--tw-prose-quotes': 'rgb(var(--ink))',
            '--tw-prose-quote-borders': 'rgb(var(--accent))',
            '--tw-prose-captions': 'rgb(var(--muted))',
            '--tw-prose-code': 'rgb(var(--ink))',
            '--tw-prose-pre-code': '#dfe3ea',
            '--tw-prose-pre-bg': '#12151b',
            '--tw-prose-th-borders': 'rgb(var(--rule))',
            '--tw-prose-td-borders': 'rgb(var(--rule))',
            maxWidth: 'none',
            a: { textDecoration: 'none' },
            'a:hover': { textDecoration: 'underline' },
            'h1, h2, h3': { fontFamily: '"LXGW WenKai TC", "Noto Serif TC", serif' },
            'code::before': { content: 'none' },
            'code::after': { content: 'none' },
          },
        },
      },
    },
  },
  plugins: [typography],
}
