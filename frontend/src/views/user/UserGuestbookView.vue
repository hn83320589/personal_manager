<template>
  <div class="space-y-8">
    <!-- Leave a message form -->
    <div class="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
      <h2 class="text-lg font-semibold text-gray-900 mb-4">留下訊息</h2>
      <form @submit.prevent="handleSubmit" class="space-y-4">
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">姓名 *</label>
            <input
              v-model="form.name"
              type="text"
              required
              placeholder="您的姓名"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 placeholder:text-gray-400 text-sm focus:outline-none focus:ring-2 focus:ring-[var(--color-primary,#0ea5e9)] focus:border-[var(--color-primary,#0ea5e9)]"
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">電子郵件（選填）</label>
            <input
              v-model="form.email"
              type="email"
              placeholder="your@email.com"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 placeholder:text-gray-400 text-sm focus:outline-none focus:ring-2 focus:ring-[var(--color-primary,#0ea5e9)] focus:border-[var(--color-primary,#0ea5e9)]"
            />
          </div>
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">留言內容 *</label>
          <textarea
            v-model="form.message"
            required
            rows="4"
            placeholder="分享您的想法、建議，或是打聲招呼..."
            class="w-full px-3 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 placeholder:text-gray-400 text-sm focus:outline-none focus:ring-2 focus:ring-[var(--color-primary,#0ea5e9)] focus:border-[var(--color-primary,#0ea5e9)] resize-none"
          ></textarea>
        </div>
        <div v-if="submitStatus" :class="submitStatus.success ? 'text-green-600' : 'text-red-500'" class="text-sm">
          {{ submitStatus.message }}
        </div>
        <div class="flex items-center justify-between">
          <p class="text-xs text-gray-400">留言將經審核後顯示</p>
          <button
            type="submit"
            :disabled="isSubmitting"
            class="px-5 py-2 rounded-lg text-white text-sm font-medium transition-opacity hover:opacity-90 disabled:opacity-50"
            :style="{ backgroundColor: 'var(--color-primary, #0ea5e9)' }"
          >{{ isSubmitting ? '送出中...' : '送出留言' }}</button>
        </div>
      </form>
    </div>

    <!-- Comments List -->
    <div>
      <div class="flex items-center justify-between mb-4">
        <h2 class="text-lg font-semibold text-gray-900">留言 <span class="text-gray-400 font-normal text-sm">({{ totalCount }})</span></h2>
      </div>

      <LoadingSpinner v-if="isLoading" size="medium" text="載入中..." />

      <div v-else-if="entries.length > 0" class="space-y-4">
        <article
          v-for="entry in entries"
          :key="entry.id"
          class="bg-white rounded-xl border border-gray-200 shadow-sm p-5"
        >
          <div class="flex items-start gap-4">
            <!-- Avatar -->
            <div
              class="w-11 h-11 rounded-full flex items-center justify-center text-white text-base font-semibold flex-shrink-0"
              :style="{ background: avatarGradient(entry.name) }"
            >{{ getInitials(entry.name) }}</div>

            <!-- Content -->
            <div class="flex-1 min-w-0">
              <div class="flex items-center justify-between mb-1.5 gap-2 flex-wrap">
                <span class="font-semibold text-gray-900 text-sm">{{ entry.name }}</span>
                <time class="text-xs text-gray-400">{{ formatDate(entry.createdAt) }}</time>
              </div>
              <p class="text-gray-700 text-sm whitespace-pre-wrap">{{ entry.message }}</p>

              <!-- Admin Reply -->
              <div v-if="entry.adminReply" class="mt-3 pl-4 border-l-2" :style="{ borderColor: 'var(--color-primary,#0ea5e9)' }">
                <div class="rounded-lg p-3" :style="{ backgroundColor: 'var(--color-primary-light,#e0f2fe)' }">
                  <div class="flex items-center gap-2 mb-1">
                    <div
                      class="w-5 h-5 rounded-full flex items-center justify-center text-white text-xs font-semibold"
                      :style="{ backgroundColor: 'var(--color-primary,#0ea5e9)' }"
                    >A</div>
                    <span class="text-xs font-semibold" :style="{ color: 'var(--color-primary-dark,#0c4a6e)' }">管理員回覆</span>
                  </div>
                  <p class="text-sm whitespace-pre-wrap" :style="{ color: 'var(--color-primary-dark,#0c4a6e)' }">{{ entry.adminReply }}</p>
                </div>
              </div>
            </div>
          </div>
        </article>
      </div>

      <p v-else-if="!isLoading" class="text-gray-400 text-sm">尚無留言，成為第一個留言的人吧！</p>

      <!-- Pagination -->
      <div v-if="totalPages > 1" class="mt-6 flex justify-center items-center gap-2">
        <button
          :disabled="currentPage === 1"
          @click="goToPage(currentPage - 1)"
          class="px-3 py-1.5 text-sm border border-gray-300 rounded-lg text-gray-600 hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
        >上一頁</button>
        <span class="text-sm text-gray-500">{{ currentPage }} / {{ totalPages }}</span>
        <button
          :disabled="currentPage === totalPages"
          @click="goToPage(currentPage + 1)"
          class="px-3 py-1.5 text-sm border border-gray-300 rounded-lg text-gray-600 hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
        >下一頁</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, inject, onMounted, watch } from 'vue'
import type { ComputedRef } from 'vue'
import httpService from '@/services/http'
import commentService from '@/services/commentService'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import type { GuestBookEntry, PagedResult } from '@/types/api'

const userId = inject<ComputedRef<number | null>>('userId')

const isLoading = ref(true)
const isSubmitting = ref(false)
const entries = ref<GuestBookEntry[]>([])
const totalCount = ref(0)
const totalPages = ref(0)
const submitStatus = ref<{ success: boolean; message: string } | null>(null)
const currentPage = ref(1)
const perPage = 10

const form = reactive({ name: '', email: '', message: '' })

// Deterministic gradient based on name
const gradientColors = [
  ['#0ea5e9', '#2563eb'], // sky-blue
  ['#22c55e', '#059669'], // green
  ['#a855f7', '#7c3aed'], // purple
  ['#f43f5e', '#e11d48'], // rose
  ['#f59e0b', '#d97706'], // amber
  ['#64748b', '#475569'], // slate
]

function avatarGradient(name: string): string {
  const idx = name.charCodeAt(0) % gradientColors.length
  const [c1, c2] = gradientColors[idx]
  return `linear-gradient(135deg, ${c1}, ${c2})`
}

function getInitials(name: string): string {
  return name.split(/\s+/).map(w => w[0] ?? '').slice(0, 2).join('').toUpperCase() || '?'
}

function formatDate(dateStr: string): string {
  const d = new Date(dateStr)
  const now = new Date()
  const diff = Math.floor((now.getTime() - d.getTime()) / 86400000)
  if (diff === 0) return '今天'
  if (diff === 1) return '昨天'
  if (diff < 7) return `${diff} 天前`
  if (diff < 30) return `${Math.floor(diff / 7)} 週前`
  return d.toLocaleDateString('zh-TW', { year: 'numeric', month: '2-digit', day: '2-digit' })
}

async function load(uid: number) {
  isLoading.value = true
  try {
    const res = await httpService.get<PagedResult<GuestBookEntry>>(`/guestbookentries/user/${uid}/paged`, {
      page: currentPage.value,
      pageSize: perPage,
    })
    if (res.success && res.data) {
      entries.value = res.data.items
      totalCount.value = res.data.totalCount
      totalPages.value = res.data.totalPages
    }
  } catch {
    // ignore
  } finally {
    isLoading.value = false
  }
}

function goToPage(page: number) {
  currentPage.value = page
  if (userId?.value) load(userId.value)
}

async function handleSubmit() {
  if (!userId?.value) return
  isSubmitting.value = true
  submitStatus.value = null
  try {
    const res = await commentService.createGuestBookEntry({
      targetUserId: userId.value,
      name: form.name,
      email: form.email || undefined,
      message: form.message,
    })
    if (res.success) {
      submitStatus.value = { success: true, message: '留言已送出，審核通過後顯示！' }
      form.name = ''
      form.email = ''
      form.message = ''
    } else {
      submitStatus.value = { success: false, message: res.message || '送出失敗，請稍後再試。' }
    }
  } catch {
    submitStatus.value = { success: false, message: '網路錯誤，請稍後再試。' }
  } finally {
    isSubmitting.value = false
  }
}

onMounted(() => { if (userId?.value) load(userId.value) })
watch(() => userId?.value, (uid) => { if (uid) load(uid) })
</script>
