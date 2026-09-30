<template>
  <div class="p-6">
    <!-- Loading State -->
    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div v-for="i in 6" :key="i" class="animate-pulse">
        <div class="bg-white border border-gray-200 rounded-lg p-6">
          <div class="flex items-center justify-between mb-4">
            <div class="w-4 h-4 bg-gray-300 rounded"></div>
            <div class="w-16 h-6 bg-gray-300 rounded-full"></div>
          </div>
          <div class="space-y-3">
            <div class="h-4 bg-gray-300 rounded w-3/4"></div>
            <div class="h-3 bg-gray-300 rounded w-1/2"></div>
            <div class="h-2 bg-gray-300 rounded w-full"></div>
          </div>
        </div>
      </div>
    </div>

    <!-- Tasks Grid -->
    <div v-else-if="tasks.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div
        v-for="task in tasks"
        :key="task.id"
        class="bg-white border border-gray-200 rounded-lg p-6 hover:shadow-md transition-shadow"
        :class="{
          'border-l-4 border-l-red-500': isOverdue(task),
          'border-l-4 border-l-yellow-500': isDueSoon(task) && !isOverdue(task),
          'opacity-70': task.status === 'Completed'
        }"
      >
        <!-- Card Header -->
        <div class="flex items-center justify-between mb-4">
          <div class="flex items-center space-x-3">
            <!-- Selection Checkbox -->
            <input
              type="checkbox"
              :checked="selectedTasks.includes(task.id)"
              @change="$emit('toggleSelect', task.id)"
              class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
            />

            <!-- Priority Badge -->
            <span :class="getPriorityStyle(String(task.priority))">
              {{ getPriorityLabel(String(task.priority)) }}
            </span>
          </div>

          <!-- Completion Toggle -->
          <button
            @click="$emit('toggleComplete', task)"
            :class="[
              'flex items-center justify-center w-6 h-6 rounded border-2 transition-colors',
              task.status === 'Completed'
                ? 'bg-green-500 border-green-500 text-white'
                : 'border-gray-300 hover:border-green-500'
            ]"
          >
            <CheckIcon v-if="task.status === 'Completed'" class="w-4 h-4" />
          </button>
        </div>

        <!-- Task Content -->
        <div class="space-y-3">
          <h3 :class="[
            'text-lg font-semibold line-clamp-2',
            task.status === 'Completed' ? 'line-through text-gray-500' : 'text-gray-900'
          ]">
            {{ task.title }}
          </h3>

          <p v-if="task.description" class="text-sm text-gray-600 line-clamp-3">
            {{ task.description }}
          </p>

          <!-- Status Badge (inline) -->

          <!-- Date Information -->
          <div v-if="task.dueDate" class="flex items-center text-sm text-gray-500">
            <CalendarDaysIcon class="w-4 h-4 mr-1" />
            {{ formatDate(task.dueDate) }}
            <span v-if="isOverdue(task)" class="ml-2 px-2 py-1 text-xs bg-red-100 text-red-700 rounded-full">
              逾期
            </span>
            <span v-else-if="isDueSoon(task)" class="ml-2 px-2 py-1 text-xs bg-yellow-100 text-yellow-700 rounded-full">
              即將到期
            </span>
          </div>

          <!-- Status Badge -->
          <div class="flex justify-between items-center">
            <span :class="getStatusStyle(String(task.status))">
              {{ getStatusLabel(String(task.status)) }}
            </span>

            <div class="flex items-center space-x-1">
              <BaseButton
                variant="outline"
                size="small"
                @click="$emit('duplicateTask', task)"
                title="複製"
              >
                <DocumentDuplicateIcon class="w-4 h-4" />
              </BaseButton>
              <BaseButton
                variant="outline"
                size="small"
                @click="$emit('editTask', task)"
                title="編輯"
              >
                <PencilIcon class="w-4 h-4" />
              </BaseButton>
              <BaseButton
                variant="outline"
                size="small"
                @click="$emit('deleteTask', task.id)"
                title="刪除"
                class="text-red-600 hover:text-red-700"
              >
                <TrashIcon class="w-4 h-4" />
              </BaseButton>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="text-center py-12">
      <Squares2X2Icon class="mx-auto h-12 w-12 text-gray-400" />
      <h3 class="mt-2 text-sm font-medium text-gray-900">沒有找到待辦事項</h3>
      <p class="mt-1 text-sm text-gray-500">
        開始建立您的第一個待辦事項吧
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import {
  CheckIcon,
  CalendarDaysIcon,
  PencilIcon,
  TrashIcon,
  DocumentDuplicateIcon,
  Squares2X2Icon
} from '@heroicons/vue/24/outline'
import type { TodoItem } from '@/types/api'
import BaseButton from '@/components/ui/BaseButton.vue'

// Props
interface Props {
  tasks: TodoItem[]
  loading?: boolean
  selectedTasks: number[]
}

const props = withDefaults(defineProps<Props>(), {
  loading: false
})

// Emits
const emit = defineEmits<{
  toggleSelect: [id: number]
  toggleComplete: [task: TodoItem]
  editTask: [task: TodoItem]
  deleteTask: [id: number]
  duplicateTask: [task: TodoItem]
}>()

// Methods - Reuse the same logic from TaskListView
function getPriorityLabel(priority: string): string {
  const priorityMap: Record<string, string> = {
    Low: '低',
    Medium: '中',
    High: '高',
  }
  return priorityMap[priority] || '中'
}

function getPriorityStyle(priority: string): string {
  const styleMap: Record<string, string> = {
    Low: 'px-2 py-1 text-xs font-medium bg-green-100 text-green-800 rounded-full',
    Medium: 'px-2 py-1 text-xs font-medium bg-blue-100 text-blue-800 rounded-full',
    High: 'px-2 py-1 text-xs font-medium bg-orange-100 text-orange-800 rounded-full',
  }
  return styleMap[priority] || styleMap.Medium
}

function getStatusLabel(status: string): string {
  const statusMap: Record<string, string> = {
    Pending: '待處理',
    InProgress: '進行中',
    Completed: '已完成',
  }
  return statusMap[status] || '待處理'
}

function getStatusStyle(status: string): string {
  const styleMap: Record<string, string> = {
    Pending: 'px-2 py-1 text-xs font-medium bg-gray-100 text-gray-800 rounded-full',
    InProgress: 'px-2 py-1 text-xs font-medium bg-yellow-100 text-yellow-800 rounded-full',
    Completed: 'px-2 py-1 text-xs font-medium bg-green-100 text-green-800 rounded-full',
  }
  return styleMap[status] || styleMap.Pending
}

function formatDate(dateString: string | undefined): string {
  if (!dateString) return ''
  
  const date = new Date(dateString)
  const now = new Date()
  const diffDays = Math.ceil((date.getTime() - now.getTime()) / (1000 * 60 * 60 * 24))
  
  if (diffDays === 0) return '今天'
  if (diffDays === 1) return '明天'
  if (diffDays === -1) return '昨天'
  
  return date.toLocaleDateString('zh-TW', {
    month: 'short',
    day: 'numeric'
  })
}

function isOverdue(task: TodoItem): boolean {
  if (!task.dueDate || task.status === 'Completed') return false
  
  const dueDate = new Date(task.dueDate)
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  
  return dueDate < today
}

function isDueSoon(task: TodoItem): boolean {
  if (!task.dueDate || task.status === 'Completed') return false
  
  const dueDate = new Date(task.dueDate)
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  
  const diffDays = Math.ceil((dueDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24))
  
  return diffDays >= 0 && diffDays <= 2 // Due within 2 days
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