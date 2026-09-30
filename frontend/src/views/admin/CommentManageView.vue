<template>
  <AdminLayout>
    <!-- Page Header -->
    <div class="mb-8">
      <div class="flex justify-between items-center">
        <div>
          <h2 class="text-2xl font-bold text-gray-900">留言管理</h2>
          <p class="mt-1 text-sm text-gray-600">
            管理訪客留言、審核內容和回覆互動
          </p>
        </div>
        <div class="flex space-x-3">
          <BaseButton
            variant="outline"
            @click="showBatchModal = true"
            :disabled="selectedComments.length === 0"
          >
            <Squares2X2Icon class="w-4 h-4 mr-2" />
            批量操作 ({{ selectedComments.length }})
          </BaseButton>
          <BaseButton
            variant="outline"
            @click="exportComments"
          >
            <ArrowDownTrayIcon class="w-4 h-4 mr-2" />
            匯出留言
          </BaseButton>
          <BaseButton
            variant="primary"
            @click="showSettingsModal = true"
          >
            <Cog6ToothIcon class="w-4 h-4 mr-2" />
            留言設定
          </BaseButton>
        </div>
      </div>
    </div>

    <!-- Stats Cards -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-6">
      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-blue-100">
            <ChatBubbleLeftEllipsisIcon class="w-6 h-6 text-blue-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">總留言數</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ totalComments }}</p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-green-100">
            <CheckCircleIcon class="w-6 h-6 text-green-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">已審核</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ approvedComments }}</p>
            <p class="text-xs text-gray-500">{{ approvedRate.toFixed(1) }}%</p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-yellow-100">
            <ClockIcon class="w-6 h-6 text-yellow-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">待審核</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ pendingComments }}</p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-red-100">
            <ExclamationTriangleIcon class="w-6 h-6 text-red-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">已檢舉</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ reportedComments }}</p>
          </div>
        </div>
      </BaseCard>
    </div>

    <!-- Filters and Controls -->
    <div class="mb-6">
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <!-- Search and Filters -->
        <div class="flex flex-col sm:flex-row gap-4 flex-1">
          <div class="relative flex-1 max-w-md">
            <MagnifyingGlassIcon class="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
            <input
              v-model="searchQuery"
              type="text"
              placeholder="搜尋留言內容、作者..."
              class="pl-10 pr-4 py-2 w-full border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
          
          <div class="flex gap-2">
            <select
              v-model="selectedStatus"
              class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">所有狀態</option>
              <option value="pending">待審核</option>
              <option value="approved">已審核</option>
            </select>

            <select
              v-model="selectedTimeRange"
              class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">所有時間</option>
              <option value="today">今天</option>
              <option value="week">本週</option>
              <option value="month">本月</option>
              <option value="quarter">本季</option>
            </select>
          </div>
        </div>

        <!-- View Options -->
        <div class="flex items-center space-x-4">
          <div class="flex items-center space-x-2">
            <label class="text-sm text-gray-700">排序:</label>
            <select
              v-model="sortBy"
              class="px-2 py-1 text-sm border border-gray-300 rounded focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="createdAt">留言時間</option>
              <option value="name">作者姓名</option>
              <option value="status">審核狀態</option>
            </select>
          </div>

          <div class="flex space-x-1 bg-gray-100 p-1 rounded-lg">
            <button
              @click="viewMode = 'list'"
              :class="[
                'px-3 py-1 text-sm font-medium rounded transition-colors',
                viewMode === 'list'
                  ? 'bg-white text-gray-900 shadow'
                  : 'text-gray-600 hover:text-gray-900'
              ]"
            >
              <ListBulletIcon class="w-4 h-4" />
            </button>
            <button
              @click="viewMode = 'card'"
              :class="[
                'px-3 py-1 text-sm font-medium rounded transition-colors',
                viewMode === 'card'
                  ? 'bg-white text-gray-900 shadow'
                  : 'text-gray-600 hover:text-gray-900'
              ]"
            >
              <Squares2X2Icon class="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Comments Content -->
    <div class="bg-white shadow rounded-lg overflow-hidden">
      <!-- List View -->
      <div v-if="viewMode === 'list'">
        <!-- Table Header -->
        <div class="bg-gray-50 px-6 py-3 border-b border-gray-200">
          <div class="flex items-center">
            <input
              type="checkbox"
              :checked="allSelected"
              :indeterminate="someSelected"
              @change="toggleSelectAll"
              class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded mr-4"
            />
            <div class="flex-1 grid grid-cols-12 gap-4 text-xs font-medium text-gray-500 uppercase tracking-wider">
              <div class="col-span-3">作者資訊</div>
              <div class="col-span-4">留言內容</div>
              <div class="col-span-2">狀態</div>
              <div class="col-span-2">時間</div>
              <div class="col-span-1">操作</div>
            </div>
          </div>
        </div>

        <!-- Comment Rows -->
        <div class="divide-y divide-gray-200">
          <div
            v-for="comment in filteredAndSortedComments"
            :key="comment.id"
            class="px-6 py-4 hover:bg-gray-50"
            :class="{
              'bg-yellow-50': !comment.isApproved,
            }"
          >
            <div class="flex items-center">
              <input
                type="checkbox"
                :checked="selectedComments.includes(comment.id)"
                @change="toggleCommentSelection(comment.id)"
                class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded mr-4"
              />
              <div class="flex-1 grid grid-cols-12 gap-4">
                <!-- Author Info -->
                <div class="col-span-3">
                  <div class="flex items-center">
                    <div class="flex-shrink-0">
                      <div class="h-10 w-10 rounded-full bg-gray-300 flex items-center justify-center">
                        <UserIcon class="h-5 w-5 text-gray-600" />
                      </div>
                    </div>
                    <div class="ml-3">
                      <p class="text-sm font-medium text-gray-900">{{ comment.name }}</p>
                      <p class="text-sm text-gray-500">{{ comment.email }}</p>
                    </div>
                  </div>
                </div>

                <!-- Comment Content -->
                <div class="col-span-4">
                  <p class="text-sm text-gray-900 line-clamp-3">{{ comment.message }}</p>
                  <div v-if="comment.adminReply" class="mt-2 pl-4 border-l-2 border-blue-500 bg-blue-50 p-2 rounded">
                    <p class="text-xs text-blue-800 font-medium">管理員回覆:</p>
                    <p class="text-sm text-blue-900">{{ comment.adminReply }}</p>
                  </div>
                </div>

                <!-- Status -->
                <div class="col-span-2">
                  <span :class="getStatusBadgeClass(comment.isApproved ? 'approved' : 'pending')">
                    {{ comment.isApproved ? '已審核' : '待審核' }}
                  </span>
                </div>

                <!-- Time -->
                <div class="col-span-2">
                  <p class="text-sm text-gray-900">{{ formatDate(comment.createdAt) }}</p>
                  <p class="text-xs text-gray-500">{{ formatTimeAgo(comment.createdAt) }}</p>
                </div>

                <!-- Actions -->
                <div class="col-span-1">
                  <div class="flex items-center space-x-1">
                    <button
                      v-if="!comment.isApproved"
                      @click="approveComment(comment.id)"
                      class="p-1 text-green-600 hover:text-green-700 transition-colors"
                      title="審核通過"
                    >
                      <CheckIcon class="w-4 h-4" />
                    </button>
                    <button
                      v-if="comment.isApproved"
                      @click="rejectComment(comment.id)"
                      class="p-1 text-red-600 hover:text-red-700 transition-colors"
                      title="拒絕留言"
                    >
                      <XMarkIcon class="w-4 h-4" />
                    </button>
                    <button
                      @click="openReplyModal(comment)"
                      class="p-1 text-blue-600 hover:text-blue-700 transition-colors"
                      title="回覆留言"
                    >
                      <ChatBubbleLeftIcon class="w-4 h-4" />
                    </button>
                    <button
                      @click="deleteComment(comment.id)"
                      class="p-1 text-gray-600 hover:text-red-600 transition-colors"
                      title="刪除留言"
                    >
                      <TrashIcon class="w-4 h-4" />
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Card View -->
      <div v-else-if="viewMode === 'card'" class="p-6">
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <div
            v-for="comment in filteredAndSortedComments"
            :key="comment.id"
            class="bg-white border border-gray-200 rounded-lg p-6 hover:shadow-md transition-shadow"
            :class="{
              'border-yellow-300 bg-yellow-50': !comment.isApproved,
            }"
          >
            <!-- Card Header -->
            <div class="flex items-center justify-between mb-4">
              <div class="flex items-center">
                <input
                  type="checkbox"
                  :checked="selectedComments.includes(comment.id)"
                  @change="toggleCommentSelection(comment.id)"
                  class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded mr-3"
                />
                <span :class="getStatusBadgeClass(comment.isApproved ? 'approved' : 'pending')">
                  {{ comment.isApproved ? '已審核' : '待審核' }}
                </span>
              </div>
              <span class="text-xs text-gray-500">{{ formatTimeAgo(comment.createdAt) }}</span>
            </div>

            <!-- Author -->
            <div class="flex items-center mb-3">
              <div class="h-8 w-8 rounded-full bg-gray-300 flex items-center justify-center mr-3">
                <UserIcon class="h-4 w-4 text-gray-600" />
              </div>
              <div>
                <p class="text-sm font-medium text-gray-900">{{ comment.name }}</p>
                <p class="text-xs text-gray-500">{{ comment.email }}</p>
              </div>
            </div>

            <!-- Message -->
            <p class="text-sm text-gray-700 mb-4 line-clamp-4">{{ comment.message }}</p>

            <!-- Admin Reply -->
            <div v-if="comment.adminReply" class="mb-4 pl-3 border-l-2 border-blue-500 bg-blue-50 p-2 rounded">
              <p class="text-xs text-blue-800 font-medium">管理員回覆:</p>
              <p class="text-sm text-blue-900">{{ comment.adminReply }}</p>
            </div>

            <!-- Actions -->
            <div class="flex justify-end space-x-2">
              <BaseButton
                v-if="!comment.isApproved"
                variant="outline"
                size="small"
                @click="approveComment(comment.id)"
                class="text-green-600 border-green-300 hover:bg-green-50"
              >
                <CheckIcon class="w-4 h-4 mr-1" />
                通過
              </BaseButton>
              <BaseButton
                v-if="comment.isApproved"
                variant="outline"
                size="small"
                @click="rejectComment(comment.id)"
                class="text-red-600 border-red-300 hover:bg-red-50"
              >
                <XMarkIcon class="w-4 h-4 mr-1" />
                拒絕
              </BaseButton>
              <BaseButton
                variant="outline"
                size="small"
                @click="openReplyModal(comment)"
              >
                <ChatBubbleLeftIcon class="w-4 h-4 mr-1" />
                回覆
              </BaseButton>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-if="filteredAndSortedComments.length === 0" class="text-center py-12">
        <ChatBubbleLeftEllipsisIcon class="mx-auto h-12 w-12 text-gray-400" />
        <h3 class="mt-2 text-sm font-medium text-gray-900">沒有找到留言</h3>
        <p class="mt-1 text-sm text-gray-500">
          {{ searchQuery ? '嘗試調整搜尋條件' : '還沒有任何訪客留言' }}
        </p>
      </div>
    </div>

    <!-- Reply Modal -->
    <BaseModal
      :show="showReplyModal"
      @close="showReplyModal = false"
      title="回覆留言"
      max-width="md"
    >
      <div v-if="replyingComment" class="space-y-4">
        <!-- Original Comment -->
        <div class="bg-gray-50 rounded-lg p-4">
          <div class="flex items-center mb-2">
            <UserIcon class="h-5 w-5 text-gray-600 mr-2" />
            <span class="font-medium text-gray-900">{{ replyingComment.name }}</span>
            <span class="text-sm text-gray-500 ml-2">{{ formatDate(replyingComment.createdAt) }}</span>
          </div>
          <p class="text-sm text-gray-700">{{ replyingComment.message }}</p>
        </div>

        <!-- Reply Form -->
        <div>
          <label for="adminReply" class="block text-sm font-medium text-gray-700 mb-2">
            管理員回覆
          </label>
          <BaseTextarea
            id="adminReply"
            v-model="replyText"
            :rows="4"
            placeholder="輸入您的回覆..."
            class="mt-1"
          />
        </div>
      </div>

      <div class="mt-6 flex justify-end space-x-3">
        <BaseButton
          variant="outline"
          @click="showReplyModal = false"
        >
          取消
        </BaseButton>
        <BaseButton
          variant="primary"
          @click="submitReply"
          :disabled="!replyText.trim()"
        >
          發送回覆
        </BaseButton>
      </div>
    </BaseModal>

    <!-- Batch Operations Modal -->
    <BaseModal
      :show="showBatchModal"
      @close="showBatchModal = false"
      title="批量操作"
      max-width="md"
    >
      <div class="space-y-4">
        <p class="text-sm text-gray-600">
          已選擇 {{ selectedComments.length }} 則留言
        </p>
        
        <div class="space-y-3">
          <BaseButton
            variant="outline"
            @click="batchApprove"
            class="w-full justify-start text-green-600"
          >
            <CheckIcon class="w-4 h-4 mr-2" />
            批量審核通過
          </BaseButton>
          
          <BaseButton
            variant="outline"
            @click="batchReject"
            class="w-full justify-start text-red-600"
          >
            <XMarkIcon class="w-4 h-4 mr-2" />
            批量拒絕
          </BaseButton>
          
          <BaseButton
            variant="outline"
            @click="batchMarkSpam"
            class="w-full justify-start text-orange-600"
          >
            <ExclamationTriangleIcon class="w-4 h-4 mr-2" />
            標記為垃圾留言
          </BaseButton>
          
          <BaseButton
            variant="outline"
            @click="batchDelete"
            class="w-full justify-start text-red-600"
          >
            <TrashIcon class="w-4 h-4 mr-2" />
            批量刪除
          </BaseButton>
        </div>
      </div>
    </BaseModal>

    <!-- Settings Modal -->
    <BaseModal
      :show="showSettingsModal"
      @close="showSettingsModal = false"
      title="留言系統設定"
      max-width="lg"
    >
      <div class="space-y-6">
        <!-- Moderation Settings -->
        <div>
          <h3 class="text-lg font-medium text-gray-900 mb-4">審核設定</h3>
          <div class="space-y-4">
            <div class="flex items-center justify-between">
              <div>
                <label class="text-sm font-medium text-gray-700">自動審核</label>
                <p class="text-xs text-gray-500">新留言自動通過審核</p>
              </div>
              <input
                v-model="settings.autoApprove"
                type="checkbox"
                class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
              />
            </div>

            <div class="flex items-center justify-between">
              <div>
                <label class="text-sm font-medium text-gray-700">需要註冊</label>
                <p class="text-xs text-gray-500">只允許註冊用戶留言</p>
              </div>
              <input
                v-model="settings.requireRegistration"
                type="checkbox"
                class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
              />
            </div>

            <div class="flex items-center justify-between">
              <div>
                <label class="text-sm font-medium text-gray-700">垃圾留言過濾</label>
                <p class="text-xs text-gray-500">自動檢測並過濾垃圾留言</p>
              </div>
              <input
                v-model="settings.spamFilter"
                type="checkbox"
                class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
              />
            </div>
          </div>
        </div>

        <!-- Notification Settings -->
        <div class="border-t pt-6">
          <h3 class="text-lg font-medium text-gray-900 mb-4">通知設定</h3>
          <div class="space-y-4">
            <div class="flex items-center justify-between">
              <div>
                <label class="text-sm font-medium text-gray-700">新留言通知</label>
                <p class="text-xs text-gray-500">有新留言時發送郵件通知</p>
              </div>
              <input
                v-model="settings.emailNotification"
                type="checkbox"
                class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
              />
            </div>

            <div v-if="settings.emailNotification">
              <label class="block text-sm font-medium text-gray-700 mb-1">通知郵箱</label>
              <BaseInput
                v-model="settings.notificationEmail"
                type="email"
                placeholder="admin@example.com"
              />
            </div>
          </div>
        </div>
      </div>

      <div class="mt-6 flex justify-end space-x-3">
        <BaseButton
          variant="outline"
          @click="showSettingsModal = false"
        >
          取消
        </BaseButton>
        <BaseButton
          variant="primary"
          @click="saveSettings"
        >
          儲存設定
        </BaseButton>
      </div>
    </BaseModal>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import {
  ChatBubbleLeftEllipsisIcon,
  ChatBubbleLeftIcon,
  CheckCircleIcon,
  ClockIcon,
  ExclamationTriangleIcon,
  MagnifyingGlassIcon,
  ListBulletIcon,
  Squares2X2Icon,
  ArrowDownTrayIcon,
  Cog6ToothIcon,
  UserIcon,
  CheckIcon,
  XMarkIcon,
  TrashIcon
} from '@heroicons/vue/24/outline'
import { useCommentStore } from '@/stores/comment'
import type { GuestBookEntry } from '@/types/api'
import AdminLayout from '@/components/layout/AdminLayout.vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseCard from '@/components/ui/BaseCard.vue'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseTextarea from '@/components/ui/BaseTextarea.vue'
import BaseModal from '@/components/ui/BaseModal.vue'

// Stores
const commentStore = useCommentStore()

// State
const searchQuery = ref('')
const selectedStatus = ref('')
const selectedTimeRange = ref('')
const sortBy = ref<'createdAt' | 'name' | 'status'>('createdAt')
const viewMode = ref<'list' | 'card'>('list')
const selectedComments = ref<number[]>([])
const showReplyModal = ref(false)
const showBatchModal = ref(false)
const showSettingsModal = ref(false)
const replyingComment = ref<GuestBookEntry | null>(null)
const replyText = ref('')

const settings = ref({
  autoApprove: false,
  requireRegistration: false,
  spamFilter: true,
  emailNotification: true,
  notificationEmail: 'admin@example.com'
})

// Computed
const comments = computed(() => commentStore.entries)

const totalComments = computed(() => comments.value.length)

const approvedComments = computed(() => {
  return comments.value.filter(comment => comment.isApproved).length
})

const approvedRate = computed(() => {
  return totalComments.value > 0 ? (approvedComments.value / totalComments.value) * 100 : 0
})

const pendingComments = computed(() => {
  return comments.value.filter(comment => !comment.isApproved).length
})

const reportedComments = computed(() => {
  return 0
})

const allSelected = computed(() => {
  return filteredAndSortedComments.value.length > 0 && 
         filteredAndSortedComments.value.every(comment => selectedComments.value.includes(comment.id))
})

const someSelected = computed(() => {
  return selectedComments.value.length > 0 && selectedComments.value.length < filteredAndSortedComments.value.length
})

const filteredAndSortedComments = computed(() => {
  let filtered = comments.value

  // Search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(comment =>
      comment.message.toLowerCase().includes(query) ||
      comment.name.toLowerCase().includes(query) ||
      comment.email.toLowerCase().includes(query)
    )
  }

  // Status filter
  if (selectedStatus.value) {
    if (selectedStatus.value === 'approved') {
      filtered = filtered.filter(comment => comment.isApproved)
    } else if (selectedStatus.value === 'pending') {
      filtered = filtered.filter(comment => !comment.isApproved)
    }
  }

  // Time range filter
  if (selectedTimeRange.value) {
    const now = new Date()
    const startDate = new Date()
    
    switch (selectedTimeRange.value) {
      case 'today':
        startDate.setHours(0, 0, 0, 0)
        break
      case 'week':
        startDate.setDate(now.getDate() - 7)
        break
      case 'month':
        startDate.setMonth(now.getMonth() - 1)
        break
      case 'quarter':
        startDate.setMonth(now.getMonth() - 3)
        break
    }
    
    filtered = filtered.filter(comment => new Date(comment.createdAt) >= startDate)
  }

  // Sort
  return filtered.sort((a, b) => {
    switch (sortBy.value) {
      case 'createdAt':
        return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
      case 'name':
        return a.name.localeCompare(b.name)
      case 'status':
        return (a.isApproved ? 1 : 0) - (b.isApproved ? 1 : 0)
      default:
        return 0
    }
  })
})

// Methods
function toggleCommentSelection(commentId: number) {
  const index = selectedComments.value.indexOf(commentId)
  if (index > -1) {
    selectedComments.value.splice(index, 1)
  } else {
    selectedComments.value.push(commentId)
  }
}

function toggleSelectAll() {
  if (allSelected.value) {
    selectedComments.value = []
  } else {
    selectedComments.value = filteredAndSortedComments.value.map(comment => comment.id)
  }
}

function getStatusLabel(status: string): string {
  const statusMap = {
    pending: '待審核',
    approved: '已審核',
    rejected: '已拒絕',
    spam: '垃圾留言'
  }
  return statusMap[status as keyof typeof statusMap] || status
}

function getStatusBadgeClass(status: string): string {
  const classMap = {
    pending: 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800',
    approved: 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800',
    rejected: 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800',
    spam: 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800'
  }
  return classMap[status as keyof typeof classMap] || classMap.pending
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString('zh-TW', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

function formatTimeAgo(dateString: string): string {
  const now = new Date()
  const date = new Date(dateString)
  const diffInHours = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60))
  
  if (diffInHours < 1) return '剛剛'
  if (diffInHours < 24) return `${diffInHours} 小時前`
  
  const diffInDays = Math.floor(diffInHours / 24)
  if (diffInDays < 7) return `${diffInDays} 天前`
  
  const diffInWeeks = Math.floor(diffInDays / 7)
  if (diffInWeeks < 4) return `${diffInWeeks} 週前`
  
  return formatDate(dateString)
}

async function approveComment(id: number) {
  try {
    await commentStore.updateEntry(id, { isApproved: true })
  } catch (error) {
    console.error('Approve comment error:', error)
  }
}

async function rejectComment(id: number) {
  try {
    await commentStore.updateEntry(id, { isApproved: false })
  } catch (error) {
    console.error('Reject comment error:', error)
  }
}

async function deleteComment(id: number) {
  if (confirm('確定要刪除這則留言嗎？此操作無法復原。')) {
    try {
      await commentStore.deleteEntry(id)
    } catch (error) {
      console.error('Delete comment error:', error)
    }
  }
}

function openReplyModal(comment: GuestBookEntry) {
  replyingComment.value = comment
  replyText.value = comment.adminReply || ''
  showReplyModal.value = true
}

async function submitReply() {
  if (!replyingComment.value || !replyText.value.trim()) return

  try {
    await commentStore.updateEntry(replyingComment.value.id, {
      adminReply: replyText.value.trim()
    })
    showReplyModal.value = false
    replyingComment.value = null
    replyText.value = ''
  } catch (error) {
    console.error('Submit reply error:', error)
  }
}

async function batchApprove() {
  try {
    await Promise.all(
      selectedComments.value.map(id =>
        commentStore.updateEntry(id, { isApproved: true })
      )
    )
    selectedComments.value = []
    showBatchModal.value = false
  } catch (error) {
    console.error('Batch approve error:', error)
  }
}

async function batchReject() {
  try {
    await Promise.all(
      selectedComments.value.map(id =>
        commentStore.updateEntry(id, { isApproved: false })
      )
    )
    selectedComments.value = []
    showBatchModal.value = false
  } catch (error) {
    console.error('Batch reject error:', error)
  }
}

async function batchMarkSpam() {
  try {
    await Promise.all(
      selectedComments.value.map(id =>
        commentStore.updateEntry(id, { isApproved: false })
      )
    )
    selectedComments.value = []
    showBatchModal.value = false
  } catch (error) {
    console.error('Batch mark spam error:', error)
  }
}

async function batchDelete() {
  if (confirm(`確定要刪除選中的 ${selectedComments.value.length} 則留言嗎？此操作無法復原。`)) {
    try {
      await Promise.all(
        selectedComments.value.map(id => commentStore.deleteEntry(id))
      )
      selectedComments.value = []
      showBatchModal.value = false
    } catch (error) {
      console.error('Batch delete error:', error)
    }
  }
}

function exportComments() {
  const exportData = filteredAndSortedComments.value.map(comment => ({
    name: comment.name,
    email: comment.email,
    message: comment.message,
    isApproved: comment.isApproved,
    adminReply: comment.adminReply,
    createdAt: comment.createdAt
  }))

  const blob = new Blob([JSON.stringify(exportData, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `comments-export-${new Date().toISOString().split('T')[0]}.json`
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}

function saveSettings() {
  localStorage.setItem('comment_settings', JSON.stringify(settings.value))
  showSettingsModal.value = false
}

// Lifecycle
onMounted(async () => {
  const saved = localStorage.getItem('comment_settings')
  if (saved) {
    try {
      Object.assign(settings.value, JSON.parse(saved))
    } catch { /* ignore parse errors */ }
  }
  await commentStore.fetchEntries()
})
</script>

<style scoped>
.line-clamp-3 {
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.line-clamp-4 {
  display: -webkit-box;
  -webkit-line-clamp: 4;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>