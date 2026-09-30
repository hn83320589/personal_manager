const APP_TITLE = 'Personal Manager'

export interface SeoMeta {
  title?: string
  description?: string
  ogImage?: string
  ogType?: 'website' | 'article' | 'profile'
}

function upsertMeta(attr: string, key: string, value: string) {
  let el = document.querySelector(`meta[${attr}="${key}"]`) as HTMLMetaElement | null
  if (!el) {
    el = document.createElement('meta')
    el.setAttribute(attr, key)
    document.head.appendChild(el)
  }
  el.content = value
}

export function setPageSeo(meta: SeoMeta) {
  document.title = meta.title ? `${meta.title} | ${APP_TITLE}` : APP_TITLE

  upsertMeta('property', 'og:title', meta.title || APP_TITLE)
  upsertMeta('property', 'og:type', meta.ogType ?? 'website')
  upsertMeta('property', 'og:url', window.location.href)

  if (meta.description) {
    upsertMeta('name', 'description', meta.description)
    upsertMeta('property', 'og:description', meta.description)
    upsertMeta('name', 'twitter:description', meta.description)
  }

  if (meta.ogImage) {
    upsertMeta('property', 'og:image', meta.ogImage)
    upsertMeta('name', 'twitter:card', 'summary_large_image')
    upsertMeta('name', 'twitter:image', meta.ogImage)
  }
}

export function stripHtml(html: string): string {
  return html.replace(/<[^>]+>/g, '').replace(/\s+/g, ' ').trim()
}
