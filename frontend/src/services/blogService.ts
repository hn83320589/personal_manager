import httpService from './http'
import type { ApiResponse, BlogPost } from '@/types/api'

class BlogService {
  async getBlogPosts(): Promise<ApiResponse<BlogPost[]>> {
    return httpService.get<BlogPost[]>('/blogposts')
  }

  async getPublishedBlogPosts(): Promise<ApiResponse<BlogPost[]>> {
    return httpService.get<BlogPost[]>('/blogposts/published')
  }

  async getBlogPostById(id: number): Promise<ApiResponse<BlogPost>> {
    return httpService.get<BlogPost>(`/blogposts/${id}`)
  }

  async getBlogPostBySlug(slug: string): Promise<ApiResponse<BlogPost>> {
    return httpService.get<BlogPost>(`/blogposts/slug/${encodeURIComponent(slug)}`)
  }

  async getBlogPostsByUserId(userId: number): Promise<ApiResponse<BlogPost[]>> {
    return httpService.get<BlogPost[]>(`/blogposts/user/${userId}`)
  }

  async createBlogPost(post: Partial<BlogPost>): Promise<ApiResponse<BlogPost>> {
    return httpService.post<BlogPost>('/blogposts', post)
  }

  async updateBlogPost(id: number, post: Partial<BlogPost>): Promise<ApiResponse<BlogPost>> {
    return httpService.put<BlogPost>(`/blogposts/${id}`, post)
  }

  async deleteBlogPost(id: number): Promise<ApiResponse<void>> {
    return httpService.delete<void>(`/blogposts/${id}`)
  }
}

export const blogService = new BlogService()
export default blogService
