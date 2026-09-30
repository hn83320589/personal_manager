import { describe, it, expect } from 'vitest'
import { sanitizeHtml } from '../sanitizeHtml'

describe('sanitizeHtml', () => {
  it('removes scripts', () => {
    expect(sanitizeHtml('<p>hi</p><script>alert(1)</script>')).toBe('<p>hi</p>')
  })

  it('removes event handler attributes', () => {
    expect(sanitizeHtml('<img src="/files/a.png" onerror="alert(1)">')).toBe(
      '<img src="/files/a.png">',
    )
  })

  it('removes javascript: links', () => {
    expect(sanitizeHtml('<a href="javascript:alert(1)">x</a>')).toBe('<a>x</a>')
  })

  it('keeps the formatting the editor produces', () => {
    const html =
      '<h2>標題</h2><p><strong>粗體</strong> <a href="https://example.com">連結</a></p><pre><code class="language-ts">const a = 1</code></pre>'

    expect(sanitizeHtml(html)).toBe(html)
  })

  it('keeps embed placeholders so the page can render the player', () => {
    expect(sanitizeHtml('<div data-embed="https://youtu.be/abc"></div>')).toBe(
      '<div data-embed="https://youtu.be/abc"></div>',
    )
  })

  it('returns an empty string for empty content', () => {
    expect(sanitizeHtml(null)).toBe('')
  })
})
