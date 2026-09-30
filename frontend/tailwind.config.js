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
