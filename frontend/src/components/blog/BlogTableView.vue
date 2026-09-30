<template>
  <div class="overflow-x-auto">
    <!-- Loading State -->
    <div v-if="loading" class="p-6">
      <div class="animate-pulse space-y-4">
        <div v-for="i in 5" :key="i" class="flex items-center space-x-4 p-4">
          <div class="w-4 h-4 bg-gray-300 rounded"></div>
          <div class="flex-1 space-y-2">
            <div class="h-4 bg-gray-300 rounded w-3/4"></div>
            <div class="h-3 bg-gray-300 rounded w-1/2"></div>
          </div>
          <div class="w-16 h-6 bg-gray-300 rounded"></div>
        </div>
      </div>
    </div>

    <!-- Table -->
    <table v-else-if="posts.length > 0" class="min-w-full divide-y divide-gray-200">
      <thead class="bg-gray-50">
        <tr>
          <th scope="col" class="relative w-12 px-6 sm:w-16 sm:px-8">
            <input
              type="checkbox"
              :checked="allSelected"
              :indeterminate="someSelected"
              @change="toggleSelectAll"
              class="absolute left-4 top-1/2 -mt-2 h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500 sm:left-6"
            />
          </th>
          <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
            文章
          </th>
          <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
            狀態
          </th>
          <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
            分類
          </th>
          <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
            作者
          </th>
          <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
            觀看數
          </th>
          <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
            更新時間
          </th>
          <th scope="col" class="relative px-6 py-3">
            <span class="sr-only">操作</span>
          </th>
        </tr>
      </thead>
      <tbody class="bg-white divide-y divide-gray-200">
        <tr
          v-for="post in posts"
          :key="post.id"
          :class="{
            'bg-blue-50': selectedPosts.includes(post.id),
            'hover:bg-gray-50': !selectedPosts.includes(post.id)
          }"
        >
          <!-- Selection Checkbox -->
          <td class="relative w-12 px-6 sm:w-16 sm:px-8">
            <input
              type="checkbox"
              :checked="selectedPosts.includes(post.id)"
              @change="$emit('toggle-select', post.id)"
              class="absolute left-4 top-1/2 -mt-2 h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500 sm:left-6"
            />
          </td>

          <!-- Post Title and Content -->
          <td class="px-6 py-4">
            <div class="flex items-start space-x-3">
              <!-- Featured Image Thumbnail -->
              <div class="flex-shrink-0">
                <div
                  class="h-10 w-10 rounded bg-gray-200 flex items-center justify-center"
                >
                  <DocumentTextIcon class="h-5 w-5 text-gray-400" />
                </div>
              </div>
              
              <div class="flex-1 min-w-0">
                <div class="flex items-center space-x-2">
                  <h3 class="text-sm font-medium text-gray-900 truncate">
                    {{ post.title }}
                  </h3>
                  <span
                    v-if="post.isPublic"
                    class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-green-100 text-green-800"
                    title="公開文章"
                  >
                    <EyeIcon class="w-3 h-3 mr-1" />
                    公開
                  </span>
                </div>
                <p v-if="post.summary" class="mt-1 text-xs text-gray-500 line-clamp-2">
                  {{ post.summary }}
                </p>
                <div v-if="post.tags" class="mt-2 flex flex-wrap gap-1">
                  <span
                    v-for="tag in getTagsList(post.tags)"
                    :key="tag"
                    class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-gray-100 text-gray-800"
                  >
                    {{ tag }}
                  </span>
                </div>
              </div>
            </div>
          </td>

          <!-- Status -->
          <td class="px-6 py-4 whitespace-nowrap">
            <span :class="getStatusBadgeClass(post.status === 'Published' ? 'published' : 'draft')">
              <div class="flex items-center">
                <div :class="getStatusDotClass(post.status === 'Published' ? 'published' : 'draft')" class="w-2 h-2 rounded-full mr-2"></div>
                {{ getStatusLabel(post.status === 'Published' ? 'published' : 'draft') }}
              </div>
            </span>
          </td>

          <!-- Category -->
          <td class="px-6 py-4 whitespace-nowrap">
            <span
              v-if="post.category"
              class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-purple-100 text-purple-800"
            >
              {{ post.category }}
            </span>
            <span v-else class="text-gray-400 text-sm">無分類</span>
          </td>

          <!-- Author -->
          <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
            系統管理員
          </td>

          <!-- Views -->
          <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
            <div class="flex items-center">
              <EyeIcon class="w-4 h-4 mr-1 text-gray-400" />
              {{ (post.viewCount || 0).toLocaleString() }}
            </div>
          </td>

          <!-- Updated At -->
          <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
            {{ formatDate(post.updatedAt) }}
            <div v-if="post.publishedAt" class="text-xs text-gray-400">
              發布: {{ formatDate(post.publishedAt) }}
            </div>
          </td>

          <!-- Actions -->
          <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
            <div class="flex items-center space-x-2">
              <!-- Quick Actions -->
              <button
                @click="$emit('toggle-publish', post)"
                :class="[
                  'p-1 rounded transition-colors',
                  (post.status === 'Published' ? 'published' : 'draft') === 'published'
                    ? 'text-yellow-600 hover:text-yellow-700 hover:bg-yellow-50'
                    : 'text-green-600 hover:text-green-700 hover:bg-green-50'
                ]"
                :title="(post.status === 'Published' ? 'published' : 'draft') === 'published' ? '取消發布' : '發布文章'"
              >
                <EyeSlashIcon v-if="(post.status === 'Published' ? 'published' : 'draft') === 'published'" class="w-4 h-4" />
                <EyeIcon v-else class="w-4 h-4" />
              </button>

              <button
                @click="$emit('preview-post', post)"
                class="p-1 text-blue-600 hover:text-blue-700 hover:bg-blue-50 rounded transition-colors"
                title="預覽文章"
              >
                <MagnifyingGlassIcon class="w-4 h-4" />
              </button>

              <button
                @click="$emit('duplicate-post', post)"
                class="p-1 text-purple-600 hover:text-purple-700 hover:bg-purple-50 rounded transition-colors"
                title="複製文章"
              >
                <DocumentDuplicateIcon class="w-4 h-4" />
              </button>

              <button
                @click="$emit('edit-post', post)"
                class="p-1 text-gray-600 hover:text-gray-700 hover:bg-gray-50 rounded transition-colors"
                title="編輯文章"
              >
                <PencilIcon class="w-4 h-4" />
              </button>

              <button
                @click="$emit('delete-post', post.id)"
                class="p-1 text-red-600 hover:text-red-700 hover:bg-red-50 rounded transition-colors"
                title="刪除文章"
              >
                <TrashIcon class="w-4 h-4" />
              </button>
            </div>
          </td>
        </tr>
      </tbody>
    </table>

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
import { computed } from 'vue'
import {
  DocumentTextIcon,
  EyeIcon,
  EyeSlashIcon,
  MagnifyingGlassIcon,
  PencilIcon,
  TrashIcon,
  DocumentDuplicateIcon
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
  'toggle-select-all': []
  'edit-post': [post: BlogPost]
  'delete-post': [id: number]
  'duplicate-post': [post: BlogPost]
  'toggle-publish': [post: BlogPost]
  'preview-post': [post: BlogPost]
}>()

// Computed
const allSelected = computed(() => {
  return props.posts.length > 0 && props.posts.every(post => props.selectedPosts.includes(post.id))
})

const someSelected = computed(() => {
  return props.selectedPosts.length > 0 && props.selectedPosts.length < props.posts.length
})

// Methods
function toggleSelectAll() {
  emit('toggle-select-all')
}

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
    draft: 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800',
    published: 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800',
    scheduled: 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800',
    archived: 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800'
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
  if (Array.isArray(tags)) return tags
  return tags.split(',').map(tag => tag.trim()).filter(tag => tag.length > 0)
}
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>