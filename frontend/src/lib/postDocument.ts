import type { Schemas } from '@/api/types'
import { formatTime, localDate } from './format'

/** 編輯器中的發佈狀態。「排程」在後端是「已發佈且發佈時間在未來」。 */
export type PublishMode = 'draft' | 'published' | 'scheduled'

export interface EditablePost {
  title: string
  slug: string
  content: string
  summary: string
  category: string
  tags: string[]
  coverImageUrl: string
  mode: PublishMode
  /** <input type="datetime-local"> 的值（當地時間），只在排程時使用 */
  scheduledAt: string
  /** 原本的發佈時間：已發佈的文章再次儲存時沿用，不會變成現在時間 */
  publishedAt: string | null
}

/** Date → datetime-local 的值（當地時間，精確到分鐘）。 */
function toLocalInput(date: Date): string {
  return `${localDate(date)}T${formatTime(date)}`
}

export function toEditablePost(post: Schemas['MyPostDto'], now = new Date()): EditablePost {
  const publishedAt = post.publishedAt ?? null
  const future = publishedAt !== null && new Date(publishedAt) > now
  const mode: PublishMode =
    post.status !== 'Published' ? 'draft' : future ? 'scheduled' : 'published'
  const tomorrowNine = new Date(now.getFullYear(), now.getMonth(), now.getDate() + 1, 9, 0)
  return {
    title: post.title,
    slug: post.slug,
    content: post.content,
    summary: post.summary,
    category: post.category,
    tags: [...post.tags],
    coverImageUrl: post.coverImageUrl,
    mode,
    scheduledAt: toLocalInput(future ? new Date(publishedAt!) : tomorrowNine),
    publishedAt,
  }
}

export function toPostRequest(post: EditablePost): Schemas['SavePostRequest'] {
  const base = {
    title: post.title.trim() || '未命名文章',
    slug: post.slug || null,
    content: post.content,
    summary: post.summary,
    category: post.category,
    tags: post.tags,
    coverImageUrl: post.coverImageUrl,
  }
  switch (post.mode) {
    case 'draft':
      return { ...base, status: 'Draft', publishedAt: null }
    case 'published':
      // 已發佈過的沿用原本時間；第一次發佈由伺服器填入現在時間
      return {
        ...base,
        status: 'Published',
        publishedAt:
          post.publishedAt && new Date(post.publishedAt) <= new Date() ? post.publishedAt : null,
      }
    case 'scheduled':
      return { ...base, status: 'Published', publishedAt: new Date(post.scheduledAt).toISOString() }
  }
}
