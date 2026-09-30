import DOMPurify from 'dompurify'

/**
 * 所有 v-html 輸出前都要經過這裡。後端存入前已用 HtmlSanitizer 清洗（ADR-012），
 * 前端再清一次作為第二道防線，避免舊資料或後端疏漏直接變成 XSS。
 */
export function sanitizeHtml(html: string | null | undefined): string {
  if (!html) return ''
  return DOMPurify.sanitize(html, { USE_PROFILES: { html: true } })
}
