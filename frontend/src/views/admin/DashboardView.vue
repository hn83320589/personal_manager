<template>
  <AdminLayout>
    <!-- Quick Stats -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-blue-100">
            <DocumentTextIcon class="w-6 h-6 text-blue-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">總文章數</h3>
            <p class="text-2xl font-semibold text-gray-900">
              <span v-if="isLoading" class="animate-pulse">—</span>
              <span v-else>{{ stats.totalPosts }}</span>
            </p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-green-100">
            <BriefcaseIcon class="w-6 h-6 text-green-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">專案作品</h3>
            <p class="text-2xl font-semibold text-gray-900">
              <span v-if="isLoading" class="animate-pulse">—</span>
              <span v-else>{{ stats.totalPortfolios }}</span>
            </p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-purple-100">
            <ChatBubbleLeftEllipsisIcon class="w-6 h-6 text-purple-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">待審核留言</h3>
            <p class="text-2xl font-semibold text-gray-900">
              <span v-if="isLoading" class="animate-pulse">—</span>
              <span v-else>{{ stats.pendingComments }}</span>
            </p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-yellow-100">
            <CheckCircleIcon class="w-6 h-6 text-yellow-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">待辦事項</h3>
            <p class="text-2xl font-semibold text-gray-900">
              <span v-if="isLoading" class="animate-pulse">—</span>
              <span v-else>{{ stats.pendingTodos }}</span>
            </p>
          </div>
        </div>
      </BaseCard>
    </div>

    <!-- Recent Activity -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
      <!-- Recent Posts -->
      <BaseCard title="最近文章">
        <div v-if="isLoading" class="space-y-4">
          <div v-for="i in 3" :key="i" class="flex items-center space-x-4 animate-pulse">
            <div class="w-10 h-10 bg-gray-200 rounded-lg flex-shrink-0"></div>
            <div class="flex-1 space-y-1">
              <div class="h-4 bg-gray-200 rounded w-3/4"></div>
              <div class="h-3 bg-gray-100 rounded w-1/3"></div>
            </div>
          </div>
        </div>

        <div v-else-if="recentPosts.length > 0" class="space-y-3">
          <div
            v-for="post in recentPosts"
            :key="post.id"
            class="flex items-center gap-3 p-2 rounded-lg hover:bg-gray-50 transition-colors cursor-pointer"
            @click="router.push(`/admin/blog`)"
          >
            <div class="flex-shrink-0 w-10 h-10 bg-blue-50 rounded-lg flex items-center justify-center">
              <DocumentTextIcon class="w-5 h-5 text-blue-500" />
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-medium text-gray-900 truncate">{{ post.title }}</p>
              <p class="text-xs text-gray-400">{{ relativeTime(post.updatedAt) }}</p>
            </div>
            <span
              class="text-xs px-2 py-0.5 rounded-full font-medium flex-shrink-0"
              :class="statusClass(post.status)"
            >{{ statusLabel(post.status) }}</span>
          </div>
        </div>

        <p v-else class="text-sm text-gray-400 py-4 text-center">尚無文章</p>

        <template #footer>
          <BaseButton variant="outline" size="small" class="w-full" @click="router.push('/admin/blog')">
            查看所有文章
          </BaseButton>
        </template>
      </BaseCard>

      <!-- Quick Actions -->
      <BaseCard title="快速操作">
        <div class="grid grid-cols-2 gap-4">
          <BaseButton variant="primary" class="h-16" @click="router.push('/admin/blog/editor')">
            <div class="text-center">
              <PlusIcon class="w-5 h-5 mx-auto mb-1" />
              <span class="text-sm">新增文章</span>
            </div>
          </BaseButton>

          <BaseButton variant="outline" class="h-16" @click="router.push('/admin/projects')">
            <div class="text-center">
              <FolderPlusIcon class="w-5 h-5 mx-auto mb-1" />
              <span class="text-sm">新增專案</span>
            </div>
          </BaseButton>

          <BaseButton variant="outline" class="h-16" @click="router.push('/admin/calendar')">
            <div class="text-center">
              <CalendarDaysIcon class="w-5 h-5 mx-auto mb-1" />
              <span class="text-sm">管理行程</span>
            </div>
          </BaseButton>

          <BaseButton variant="outline" class="h-16" @click="router.push('/admin/profile')">
            <div class="text-center">
              <CogIcon class="w-5 h-5 mx-auto mb-1" />
              <span class="text-sm">個人設定</span>
            </div>
          </BaseButton>
        </div>
      </BaseCard>
    </div>

    <!-- System Info -->
    <div class="mt-8">
      <BaseCard title="系統資訊">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div>
            <h4 class="text-sm font-medium text-gray-500">使用者角色</h4>
            <p class="text-lg font-semibold text-gray-900">{{ authStore.userRole }}</p>
          </div>
          <div>
            <h4 class="text-sm font-medium text-gray-500">帳號建立日期</h4>
            <p class="text-lg font-semibold text-gray-900">{{ formatDate(authStore.user?.createdAt) }}</p>
          </div>
          <div>
            <h4 class="text-sm font-medium text-gray-500">帳號狀態</h4>
            <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
              已啟用
            </span>
          </div>
        </div>
      </BaseCard>
    </div>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  DocumentTextIcon,
  BriefcaseIcon,
  ChatBubbleLeftEllipsisIcon,
  CheckCircleIcon,
  PlusIcon,
  FolderPlusIcon,
  CalendarDaysIcon,
  CogIcon
} from '@heroicons/vue/24/outline'
import { useAuthStore } from '@/stores/auth'
import httpService from '@/services/http'
import AdminLayout from '@/components/layout/AdminLayout.vue'
import BaseCard from '@/components/ui/BaseCard.vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import type { BlogPost, Portfolio, GuestBookEntry, TodoItem } from '@/types/api'

const router = useRouter()
const authStore = useAuthStore()

const isLoading = ref(true)

const stats = ref({
  totalPosts: 0,
  totalPortfolios: 0,
  pendingComments: 0,
  pendingTodos: 0,
})

const recentPosts = ref<BlogPost[]>([])

function statusLabel(status: string): string {
  if (status === 'Published') return '已發布'
  if (status === 'Archived') return '已封存'
  return '草稿'
}

function statusClass(status: string): string {
  if (status === 'Published') return 'bg-green-100 text-green-700'
  if (status === 'Archived') return 'bg-yellow-100 text-yellow-700'
  return 'bg-gray-100 text-gray-500'
}

function relativeTime(dateStr?: string): string {
  if (!dateStr) return ''
  const diff = Math.floor((Date.now() - new Date(dateStr).getTime()) / 86400000)
  if (diff === 0) return '今天'
  if (diff === 1) return '昨天'
  if (diff < 7) return `${diff} 天前`
  if (diff < 30) return `${Math.floor(diff / 7)} 週前`
  return new Date(dateStr).toLocaleDateString('zh-TW', { month: 'numeric', day: 'numeric' })
}

function formatDate(dateStr?: string): string {
  if (!dateStr) return '—'
  return new Date(dateStr).toLocaleDateString('zh-TW', { year: 'numeric', month: 'long', day: 'numeric' })
}

onMounted(async () => {
  const uid = authStore.user?.id
  if (!uid) return

  try {
    const [postsRes, portfoliosRes, commentsRes, todosRes] = await Promise.all([
      httpService.get<BlogPost[]>(`/blogposts/user/${uid}`),
      httpService.get<Portfolio[]>(`/portfolios/user/${uid}`),
      httpService.get<GuestBookEntry[]>('/guestbookentries/all'),
      httpService.get<TodoItem[]>('/todoitems'),
    ])

    if (postsRes.success) {
      const posts = postsRes.data || []
      stats.value.totalPosts = posts.length
      recentPosts.value = [...posts]
        .sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())
        .slice(0, 5)
    }

    if (portfoliosRes.success) {
      stats.value.totalPortfolios = (portfoliosRes.data || []).length
    }

    if (commentsRes.success) {
      stats.value.pendingComments = (commentsRes.data || []).filter(c => !c.isApproved).length
    }

    if (todosRes.success) {
      stats.value.pendingTodos = (todosRes.data || []).filter(t => t.status !== 'Completed').length
    }
  } catch {
    // ignore
  } finally {
    isLoading.value = false
  }
})
</script>
