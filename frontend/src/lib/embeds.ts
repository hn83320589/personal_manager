/**
 * 作品的嵌入區塊只保存網址（ADR-012），在這裡轉成播放器網址。
 * 白名單必須與後端 Common/EmbedProviders.cs 一致；不在白名單或無法辨識的網址一律不嵌入。
 */
export interface Embed {
  provider: string
  /** video／frame 以 16:9 iframe 顯示；audio 為較矮的播放器；link 只顯示連結卡片 */
  kind: 'video' | 'frame' | 'audio' | 'link'
  src: string
}

type Converter = (url: URL) => Embed | null

const hostIs = (host: string, domain: string, allowSubdomains: boolean) =>
  host === domain || (allowSubdomains && host.endsWith(`.${domain}`))

const youtubeId = (url: URL): string | null => {
  if (url.hostname === 'youtu.be') return url.pathname.slice(1) || null
  const match = url.pathname.match(/^\/(?:embed|shorts|live)\/([\w-]+)/)
  return match?.[1] ?? url.searchParams.get('v')
}

const providers: { domain: string; subdomains: boolean; convert: Converter }[] = [
  {
    domain: 'youtube.com',
    subdomains: true,
    convert: (url) => {
      const id = youtubeId(url)
      return id
        ? {
            provider: 'YouTube',
            kind: 'video',
            src: `https://www.youtube-nocookie.com/embed/${id}`,
          }
        : null
    },
  },
  {
    domain: 'youtu.be',
    subdomains: false,
    convert: (url) => {
      const id = youtubeId(url)
      return id
        ? {
            provider: 'YouTube',
            kind: 'video',
            src: `https://www.youtube-nocookie.com/embed/${id}`,
          }
        : null
    },
  },
  {
    domain: 'vimeo.com',
    subdomains: true,
    convert: (url) => {
      const id = url.pathname.match(/(\d+)(?:\/|$)/)?.[1]
      return id
        ? { provider: 'Vimeo', kind: 'video', src: `https://player.vimeo.com/video/${id}` }
        : null
    },
  },
  {
    domain: 'figma.com',
    subdomains: true,
    convert: (url) => ({
      provider: 'Figma',
      kind: 'frame',
      src: `https://www.figma.com/embed?embed_host=personal-manager&url=${encodeURIComponent(url.href)}`,
    }),
  },
  {
    domain: 'sketchfab.com',
    subdomains: true,
    convert: (url) => {
      const id = url.pathname.match(/([0-9a-f]{32})(?:\/|$)/)?.[1]
      return id
        ? { provider: 'Sketchfab', kind: 'frame', src: `https://sketchfab.com/models/${id}/embed` }
        : null
    },
  },
  {
    domain: 'soundcloud.com',
    subdomains: true,
    convert: (url) => ({
      provider: 'SoundCloud',
      kind: 'audio',
      src: `https://w.soundcloud.com/player/?url=${encodeURIComponent(url.href)}`,
    }),
  },
  {
    domain: 'codepen.io',
    subdomains: true,
    convert: (url) => {
      const match = url.pathname.match(/^\/([\w-]+)\/(?:pen|embed)\/([\w-]+)/)
      return match
        ? {
            provider: 'CodePen',
            kind: 'frame',
            src: `https://codepen.io/${match[1]}/embed/${match[2]}?default-tab=result`,
          }
        : null
    },
  },
  {
    domain: 'docs.google.com',
    subdomains: false,
    convert: (url) => {
      const slides = url.pathname.match(/^\/presentation\/d\/([\w-]+)/)
      if (slides)
        return {
          provider: 'Google 簡報',
          kind: 'frame',
          src: `https://docs.google.com/presentation/d/${slides[1]}/embed`,
        }
      const doc = url.pathname.match(/^\/(document|spreadsheets)\/d\/([\w-]+)/)
      return doc
        ? {
            provider: 'Google 文件',
            kind: 'frame',
            src: `https://docs.google.com/${doc[1]}/d/${doc[2]}/preview`,
          }
        : null
    },
  },
  {
    domain: 'gist.github.com',
    subdomains: false,
    convert: (url) => ({ provider: 'GitHub Gist', kind: 'link', src: url.href }),
  },
]

export function toEmbed(value: string): Embed | null {
  let url: URL
  try {
    url = new URL(value)
  } catch {
    return null
  }
  if (url.protocol !== 'https:') return null
  const host = url.hostname.toLowerCase()
  const provider = providers.find((p) => hostIs(host, p.domain, p.subdomains))
  return provider ? provider.convert(url) : null
}
