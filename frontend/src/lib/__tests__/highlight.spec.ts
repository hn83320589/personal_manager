import { describe, it, expect } from 'vitest'
import { highlightCode, highlightIn } from '../highlight'

describe('highlightCode', () => {
  it('marks keywords and strings for the given language', () => {
    const html = highlightCode('const name = "山茶"', 'javascript')

    expect([
      html.includes('<span class="hljs-keyword">const</span>'),
      html.includes('hljs-string'),
    ]).toEqual([true, true])
  })

  it('escapes markup inside the code', () => {
    const html = highlightCode('<script>alert(1)</script>', 'plaintext')

    expect(html).toBe('&lt;script&gt;alert(1)&lt;/script&gt;')
  })

  it('accepts common aliases such as C#', () => {
    expect(highlightCode('public class A {}', 'C#')).toContain('hljs-keyword')
  })

  it('leaves unknown languages as escaped plain text', () => {
    expect(highlightCode('a < b', 'brainfuck-9000')).toBe('a &lt; b')
  })
})

describe('highlightIn', () => {
  it('highlights code blocks inside rendered article content', () => {
    const root = document.createElement('div')
    root.innerHTML = '<pre><code class="language-sql">SELECT 1</code></pre>'

    highlightIn(root)

    expect(root.innerHTML).toContain('<span class="hljs-keyword">SELECT</span>')
  })
})
