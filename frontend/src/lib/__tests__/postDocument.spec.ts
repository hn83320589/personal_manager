import { describe, it, expect } from 'vitest'
import type { Schemas } from '@/api/types'
import { toEditablePost, toPostRequest } from '../postDocument'

const now = new Date('2026-09-30T08:00:00Z')

const dto = (overrides: Partial<Schemas['MyPostDto']> = {}): Schemas['MyPostDto'] => ({
  id: 1,
  title: '資料庫架構',
  slug: 'db',
  content: '<p>內容</p>',
  summary: '',
  category: '技術',
  tags: ['SQL'],
  coverImageUrl: '',
  status: 'Draft',
  publishedAt: null,
  updatedAt: '2026-09-30T00:00:00Z',
  viewCount: 0,
  readingMinutes: 1,
  ...overrides,
})

describe('postDocument', () => {
  it('treats a published post with a future date as scheduled', () => {
    const post = toEditablePost(
      dto({ status: 'Published', publishedAt: '2026-10-15T01:00:00Z' }),
      now,
    )

    expect(post.mode).toBe('scheduled')
  })

  it('keeps the original publish date when a published post is saved again', () => {
    const post = toEditablePost(
      dto({ status: 'Published', publishedAt: '2026-09-01T00:00:00Z' }),
      now,
    )

    expect(toPostRequest(post)).toMatchObject({
      status: 'Published',
      publishedAt: '2026-09-01T00:00:00Z',
    })
  })

  it('lets the server stamp the time when a draft is published', () => {
    const post = toEditablePost(dto(), now)
    post.mode = 'published'

    expect(toPostRequest(post)).toMatchObject({ status: 'Published', publishedAt: null })
  })

  it('sends the chosen local time for a scheduled post', () => {
    const post = toEditablePost(dto(), now)
    post.mode = 'scheduled'
    post.scheduledAt = '2026-10-15T09:00'

    expect(toPostRequest(post).publishedAt).toBe(new Date('2026-10-15T09:00').toISOString())
  })

  it('saves drafts as drafts', () => {
    expect(toPostRequest(toEditablePost(dto(), now))).toMatchObject({
      status: 'Draft',
      title: '資料庫架構',
    })
  })
})
