import { common, createLowlight } from 'lowlight'

/** 共用的語法上色（部落格編輯器的程式碼區塊、前台文章與作品的程式碼）。只載入常見語言。 */
export const lowlight = createLowlight(common)

/** 使用者習慣的寫法對應到 highlight.js 的語言名稱。 */
const aliases: Record<string, string> = {
  'c#': 'csharp',
  cs: 'csharp',
  '.net': 'csharp',
  'c++': 'cpp',
  js: 'javascript',
  ts: 'typescript',
  vue: 'xml',
  html: 'xml',
  sh: 'bash',
  shell: 'bash',
  yml: 'yaml',
  golang: 'go',
  py: 'python',
}

export function resolveLanguage(language: string): string | null {
  const key = language.trim().toLowerCase()
  const name = aliases[key] ?? key
  return name && lowlight.registered(name) ? name : null
}

interface HastNode {
  type: string
  value?: string
  properties?: { className?: string[] }
  children?: HastNode[]
}

const escape = (text: string) =>
  text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')

/** lowlight 回傳語法樹；轉成只有 <span class> 的 HTML，文字一律跳脫。 */
function toHtml(node: HastNode): string {
  if (node.type === 'text') return escape(node.value ?? '')
  const inner = (node.children ?? []).map(toHtml).join('')
  const className = node.properties?.className?.join(' ')
  return node.type === 'element' && className
    ? `<span class="${escape(className)}">${inner}</span>`
    : inner
}

/** 回傳上色後的 HTML；不認得的語言只做跳脫。 */
export function highlightCode(code: string, language: string): string {
  const name = resolveLanguage(language)
  if (!name) return escape(code)
  return toHtml(lowlight.highlight(name, code) as HastNode)
}

/** 為已渲染的內容中 <pre><code class="language-xxx"> 上色（文章頁）。 */
export function highlightIn(root: HTMLElement) {
  root.querySelectorAll<HTMLElement>('pre code[class*="language-"]').forEach((code) => {
    const language = [...code.classList]
      .find((c) => c.startsWith('language-'))
      ?.slice('language-'.length)
    // highlightCode 會跳脫所有文字，只輸出 lowlight 的 <span class>，不會帶入原始 HTML
    if (language) code.innerHTML = highlightCode(code.textContent ?? '', language)
  })
}
