<template>
  <div class="p-6">
    <!-- Loading State -->
    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div v-for="i in 6" :key="i" class="animate-pulse">
        <div class="bg-white border border-gray-200 rounded-lg overflow-hidden">
          <div class="h-48 bg-gray-300"></div>
          <div class="p-6">
            <div class="space-y-3">
              <div class="h-4 bg-gray-300 rounded w-3/4"></div>
              <div class="h-3 bg-gray-300 rounded w-full"></div>
              <div class="h-3 bg-gray-300 rounded w-2/3"></div>
              <div class="flex space-x-2 mt-4">
                <div class="h-6 w-16 bg-gray-300 rounded-full"></div>
                <div class="h-6 w-20 bg-gray-300 rounded-full"></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Posts Grid -->
    <div v-else-if="posts.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div
        v-for="post in posts"
        :key="post.id"
        class="bg-white border border-gray-200 rounded-lg overflow-hidden hover:shadow-lg transition-shadow"
        :class="{
          'ring-2 ring-blue-500 ring-opacity-50': selectedPosts.includes(post.id)
        }"
      >
        <!-- Card Header with Selection -->
        <div class="relative">
          <!-- Featured Image -->
          <div class="h-48 bg-gray-200 overflow-hidden">
            <div
              class="w-full h-full flex items-center justify-center bg-gradient-to-br from-blue-50 to-purple-50"
            >
              <DocumentTextIcon class="h-16 w-16 text-gray-400" />
            </div>
          </div>

          <!-- Selection Checkbox -->
          <div class="absolute top-3 left-3">
            <input
              type="checkbox"
              :checked="selectedPosts.includes(post.id)"
              @change="$emit('toggle-select', post.id)"
              class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded bg-white"
            />
          </div>

          <!-- Status Badge -->
          <div class="absolute top-3 right-3">
            <span :class="getStatusBadgeClass(post.status === 'Published' ? 'published' : 'draft')">
              <div class="flex items-center">
                <div :class="getStatusDotClass(post.status === 'Published' ? 'published' : 'draft')" class="w-2 h-2 rounded-full mr-1"></div>
                {{ getStatusLabel(post.status === 'Published' ? 'published' : 'draft') }}
              </div>
            </span>
          </div>

          <!-- Public Badge -->
          <div v-if="post.isPublic" class="absolute bottom-3 right-3">
            <span class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
              <EyeIcon class="w-3 h-3 mr-1" />
              公開
            </span>
          </div>
        </div>

        <!-- Card Content -->
        <div class="p-6">
          <!-- Post Title -->
          <div class="mb-3">
            <h3 class="text-lg font-semibold text-gray-900 line-clamp-2 hover:text-blue-600 cursor-pointer"
                @click="$emit('edit-post', post)">
              {{ post.title }}
            </h3>
          </div>

          <!-- Post Excerpt -->
          <p v-if="post.summary" class="text-sm text-gray-600 mb-4 line-clamp-3">
            {{ post.summary }}
          </p>

          <!-- Post Meta -->
          <div class="space-y-3">
            <!-- Category and Tags -->
            <div class="flex flex-wrap gap-2">
              <span
                v-if="post.category"
                class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-purple-100 text-purple-800"
              >
                <TagIcon class="w-3 h-3 mr-1" />
                {{ post.category }}
              </span>
              <span
                v-for="tag in getTagsList(post.tags)"
                :key="tag"
                class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-700"
              >
                {{ tag }}
              </span>
            </div>

            <!-- Author and Views -->
            <div class="flex items-center justify-between text-sm text-gray-500">
              <span>系統管理員</span>
              <div class="flex items-center">
                <EyeIcon class="w-4 h-4 mr-1" />
                {{ (post.viewCount || 0).toLocaleString() }}
              </div>
            </div>

            <!-- Date Information -->
            <div class="text-xs text-gray-500">
              <div>更新: {{ formatDate(post.updatedAt) }}</div>
              <div v-if="post.publishedAt" class="text-green-600">
                發布: {{ formatDate(post.publishedAt) }}
              </div>
            </div>
          </div>
        </div>

        <!-- Card Actions -->
        <div class="px-6 py-4 bg-gray-50 border-t border-gray-200">
          <div class="flex justify-between items-center">
            <div class="flex space-x-2">
              <!-- Quick Actions -->
              <button
                @click="$emit('toggle-publish', post)"
                :class="[
                  'p-2 rounded-full transition-colors',
                  post.status === 'Published'
                    ? 'text-yellow-600 hover:bg-yellow-100'
                    : 'text-green-600 hover:bg-green-100'
                ]"
                :title="post.status === 'Published' ? '取消發布' : '發布文章'"
              >
                <EyeSlashIcon v-if="post.status === 'Published'" class="w-4 h-4" />
                <EyeIcon v-else class="w-4 h-4" />
              </button>

              <button
                @click="$emit('preview-post', post)"
                class="p-2 text-blue-600 hover:bg-blue-100 rounded-full transition-colors"
                title="預覽文章"
              >
                <MagnifyingGlassIcon class="w-4 h-4" />
              </button>

              <button
                @click="$emit('duplicate-post', post)"
                class="p-2 text-purple-600 hover:bg-purple-100 rounded-full transition-colors"
                title="複製文章"
              >
                <DocumentDuplicateIcon class="w-4 h-4" />
              </button>
            </div>

            <div class="flex space-x-2">
              <button
                @click="$emit('edit-post', post)"
                class="p-2 text-gray-600 hover:bg-gray-100 rounded-full transition-colors"
                title="編輯文章"
              >
                <PencilIcon class="w-4 h-4" />
              </button>

              <button
                @click="$emit('delete-post', post.id)"
                class="p-2 text-red-600 hover:bg-red-100 rounded-full transition-colors"
                title="刪除文章"
              >
                <TrashIcon class="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="text-center py-12">
      <DocumentTextIcon class="mx-auto h-12 w-12 text-gray-400" />
      <h3 class="mt-2 text-sm font-medium text-gray-900">沒有找到文章</h3>
      <p class="mt-1 text-sm text-gray-500">
        開始撰寫您的第一篇文章吧
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import {
  DocumentTextIcon,
  EyeIcon,
  EyeSlashIcon,
  MagnifyingGlassIcon,
  PencilIcon,
  TrashIcon,
  DocumentDuplicateIcon,
  TagIcon
} from '@heroicons/vue/24/outline'
import type { BlogPost } from '@/types/api'

// Props
interface Props {
  posts: BlogPost[]
  loading?: boolean
  selectedPosts: number[]
}

const props = withDefaults(defineProps<Props>(), {
  loading: false
})

// Emits
const emit = defineEmits<{
  'toggle-select': [id: number]
  'edit-post': [post: BlogPost]
  'delete-post': [id: number]
  'duplicate-post': [post: BlogPost]
  'toggle-publish': [post: BlogPost]
  'preview-post': [post: BlogPost]
}>()

// Methods
function getStatusLabel(status: string): string {
  const statusMap = {
    draft: '草稿',
    published: '已發布',
    scheduled: '排程發布',
    archived: '已封存'
  }
  return statusMap[status as keyof typeof statusMap] || '草稿'
}

function getStatusBadgeClass(status: string): string {
  const classMap = {
    draft: 'inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-800',
    published: 'inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800',
    scheduled: 'inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800',
    archived: 'inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-red-100 text-red-800'
  }
  return classMap[status as keyof typeof classMap] || classMap.draft
}

function getStatusDotClass(status: string): string {
  const classMap = {
    draft: 'bg-gray-400',
    published: 'bg-green-400',
    scheduled: 'bg-blue-400',
    archived: 'bg-red-400'
  }
  return classMap[status as keyof typeof classMap] || classMap.draft
}

function formatDate(dateString: string | undefined): string {
  if (!dateString) return ''
  
  const date = new Date(dateString)
  const now = new Date()
  const diffDays = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60 * 24))
  
  if (diffDays === 0) return '今天'
  if (diffDays === 1) return '昨天'
  if (diffDays < 7) return `${diffDays} 天前`
  
  return date.toLocaleDateString('zh-TW', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}

function getTagsList(tags: string[] | string | undefined): string[] {
  if (!tags) return []
  if (Array.isArray(tags)) return tags.slice(0, 3)
  return tags.split(',').map(tag => tag.trim()).filter(tag => tag.length > 0).slice(0, 3)
}
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.line-clamp-3 {
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>