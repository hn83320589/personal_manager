import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useBlogStore } from '@/stores/blog'
import type { BlogPost } from '@/types/api'

// Mock blogService
vi.mock('@/services/blogService', () => ({
  default: {
    getBlogPosts: vi.fn(),
    getPublishedBlogPosts: vi.fn(),
    getBlogPostById: vi.fn(),
    getBlogPostBySlug: vi.fn(),
    createBlogPost: vi.fn(),
    updateBlogPost: vi.fn(),
    deleteBlogPost: vi.fn(),
  },
}))

import blogService from '@/services/blogService'

function makePost(overrides: Partial<BlogPost> = {}): BlogPost {
  return {
    id: 1,
    userId: 1,
    title: 'Test Post',
    slug: 'test-post',
    content: 'Hello world',
    summary: '',
    category: 'Tech',
    tags: [],
    status: 'Published',
    isPublic: true,
    viewCount: 0,
    publishedAt: '2026-01-01T00:00:00Z',
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: '2026-01-01T00:00:00Z',
    ...overrides,
  }
}

describe('useBlogStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('initial state', () => {
    it('starts with empty posts array', () => {
      const store = useBlogStore()
      expect(store.posts).toEqual([])
    })

    it('starts with null currentPost', () => {
      const store = useBlogStore()
      expect(store.currentPost).toBeNull()
    })

    it('starts with isLoading false', () => {
      const store = useBlogStore()
      expect(store.isLoading).toBe(false)
    })

    it('starts with null error', () => {
      const store = useBlogStore()
      expect(store.error).toBeNull()
    })
  })

  describe('publishedPosts getter', () => {
    it('returns only Published posts sorted by publishedAt descending', () => {
      const store = useBlogStore()
      store.posts = [
        makePost({ id: 1, status: 'Draft', publishedAt: '2026-01-03T00:00:00Z' }),
        makePost({ id: 2, status: 'Published', publishedAt: '2026-01-01T00:00:00Z' }),
        makePost({ id: 3, status: 'Published', publishedAt: '2026-01-02T00:00:00Z' }),
      ]
      expect(store.publishedPosts.map(p => p.id)).toEqual([3, 2])
    })

    it('excludes draft posts', () => {
      const store = useBlogStore()
      store.posts = [makePost({ id: 1, status: 'Draft' })]
      expect(store.publishedPosts).toHaveLength(0)
    })
  })

  describe('publicPosts getter', () => {
    it('returns only published posts that are public', () => {
      const store = useBlogStore()
      store.posts = [
        makePost({ id: 1, status: 'Published', isPublic: true }),
        makePost({ id: 2, status: 'Published', isPublic: false }),
      ]
      expect(store.publicPosts.map(p => p.id)).toEqual([1])
    })
  })

  describe('draftPosts getter', () => {
    it('returns only Draft posts', () => {
      const store = useBlogStore()
      store.posts = [
        makePost({ id: 1, status: 'Draft' }),
        makePost({ id: 2, status: 'Published' }),
      ]
      expect(store.draftPosts.map(p => p.id)).toEqual([1])
    })
  })

  describe('searchPosts', () => {
    it('matches by title (case-insensitive)', async () => {
      const store = useBlogStore()
      store.posts = [
        makePost({ id: 1, title: 'Vue Tips', status: 'Published' }),
        makePost({ id: 2, title: 'React Guide', status: 'Published' }),
      ]
      const results = await store.searchPosts('vue')
      expect(results.map(p => p.id)).toEqual([1])
    })

    it('matches by content', async () => {
      const store = useBlogStore()
      store.posts = [
        makePost({ id: 1, content: 'TypeScript is great', status: 'Published' }),
        makePost({ id: 2, content: 'Python is also great', status: 'Published' }),
      ]
      const results = await store.searchPosts('typescript')
      expect(results.map(p => p.id)).toEqual([1])
    })

    it('returns empty array when no match', async () => {
      const store = useBlogStore()
      store.posts = [makePost({ id: 1, title: 'Hello', content: 'World', status: 'Published' })]
      const results = await store.searchPosts('nonexistent')
      expect(results).toHaveLength(0)
    })

    it('only searches published posts', async () => {
      const store = useBlogStore()
      store.posts = [
        makePost({ id: 1, title: 'Draft Post', status: 'Draft' }),
        makePost({ id: 2, title: 'Draft Post Published', status: 'Published' }),
      ]
      const results = await store.searchPosts('draft post')
      expect(results.map(p => p.id)).toEqual([2])
    })
  })

  describe('createPost', () => {
    it('adds new post to store on success', async () => {
      const store = useBlogStore()
      const newPost = makePost({ id: 99, title: 'New Post' })
      vi.mocked(blogService.createBlogPost).mockResolvedValue({ data: newPost, success: true, message: '', errors: null })

      await store.createPost({ title: 'New Post' })

      expect(store.posts).toContainEqual(newPost)
    })

    it('sets error and returns null on failure', async () => {
      const store = useBlogStore()
      vi.mocked(blogService.createBlogPost).mockRejectedValue(new Error('Network error'))

      const result = await store.createPost({ title: 'Fail' })

      expect(result).toBeNull()
      expect(store.error).toBe('Failed to create post')
    })
  })

  describe('deletePost', () => {
    it('removes post from store on success', async () => {
      const store = useBlogStore()
      store.posts = [makePost({ id: 1 }), makePost({ id: 2 })]
      vi.mocked(blogService.deleteBlogPost).mockResolvedValue({ data: undefined, success: true, message: '', errors: null })

      await store.deletePost(1)

      expect(store.posts.map(p => p.id)).toEqual([2])
    })

    it('returns false and sets error on failure', async () => {
      const store = useBlogStore()
      store.posts = [makePost({ id: 1 })]
      vi.mocked(blogService.deleteBlogPost).mockRejectedValue(new Error('Network error'))

      const result = await store.deletePost(1)

      expect(result).toBe(false)
      expect(store.error).toBe('Failed to delete post')
      expect(store.posts).toHaveLength(1)
    })
  })
})
