import type { Schemas } from '@/api/types'

/**
 * 依作品集模式提供的技能建議。技能名稱與分類都可以自由輸入，
 * 這些只是在清單裡還沒有時的備援選項。
 */
export const skillSuggestions: Record<
  Schemas['PortfolioMode'],
  { category: string; names: string[] }[]
> = {
  Designer: [
    {
      category: '設計工具',
      names: [
        'Figma',
        'Illustrator',
        'Photoshop',
        'InDesign',
        'After Effects',
        'Premiere Pro',
        'Blender',
        'Cinema 4D',
        'Procreate',
      ],
    },
    {
      category: '專業領域',
      names: [
        '品牌識別',
        '版面設計',
        '字體排印',
        '包裝設計',
        '插畫',
        'UI 設計',
        'UX 研究',
        '動態設計',
        '3D 建模',
        '攝影',
      ],
    },
  ],
  Frontend: [
    { category: '語言', names: ['TypeScript', 'JavaScript', 'HTML', 'CSS'] },
    {
      category: '框架與工具',
      names: ['Vue', 'React', 'Next.js', 'Nuxt', 'Vite', 'Tailwind CSS', 'Vitest', 'Playwright'],
    },
    { category: '專業領域', names: ['無障礙設計', '效能優化', '響應式設計', '設計系統'] },
  ],
  Backend: [
    { category: '語言', names: ['C#', 'Go', 'Java', 'Python', 'Node.js', 'SQL'] },
    {
      category: '框架與資料庫',
      names: ['.NET', 'Spring Boot', 'PostgreSQL', 'MySQL', 'Redis', 'Kafka'],
    },
    { category: '基礎設施', names: ['Docker', 'Kubernetes', 'AWS', 'Azure', 'CI/CD', 'Linux'] },
    { category: '專業領域', names: ['系統設計', 'API 設計', '效能調校', '資料建模'] },
  ],
}

export const skillLevels: { value: Schemas['SkillLevel']; label: string }[] = [
  { value: 'Beginner', label: '入門' },
  { value: 'Intermediate', label: '中等' },
  { value: 'Advanced', label: '進階' },
  { value: 'Expert', label: '專家' },
]
