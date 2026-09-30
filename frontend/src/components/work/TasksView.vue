<template>
  <div class="p-6">
    <div v-if="loading" class="text-center py-8">
      <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      <p class="mt-2 text-sm text-gray-500">載入中...</p>
    </div>
    
    <div v-else-if="tasks.length === 0" class="text-center py-8">
      <ListBulletIcon class="mx-auto h-12 w-12 text-gray-400" />
      <h3 class="mt-2 text-sm font-medium text-gray-900">沒有任務</h3>
      <p class="mt-1 text-sm text-gray-500">開始新增您的工作任務。</p>
    </div>

    <div v-else>
      <!-- Tasks Table -->
      <table class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
          <tr>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              任務
            </th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              專案
            </th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              狀態
            </th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              到期日
            </th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              時間
            </th>
            <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
              操作
            </th>
          </tr>
        </thead>
        <tbody class="bg-white divide-y divide-gray-200">
          <tr v-for="task in tasks" :key="task.id" class="hover:bg-gray-50">
            <td class="px-6 py-4 whitespace-nowrap">
              <div class="flex items-center">
                <input
                  type="checkbox"
                  :checked="task.status === 'Completed'"
                  @change="$emit('toggle-complete', task)"
                  class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                />
                <div class="ml-4">
                  <div class="text-sm font-medium text-gray-900">{{ task.title }}</div>
                  <div class="text-sm text-gray-500">{{ task.description }}</div>
                </div>
              </div>
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
              {{ task.projectName || '-' }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap">
              <span :class="getStatusStyle(task.status)">
                {{ getStatusLabel(task.status) }}
              </span>
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
              {{ task.dueDate ? formatDate(task.dueDate) : '-' }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
              <div>
                <div>估計: {{ task.estimatedHours || 0 }}h</div>
                <div>實際: {{ task.actualHours || 0 }}h</div>
              </div>
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
              <div class="flex justify-end space-x-2">
                <BaseButton
                  variant="outline"
                  size="small"
                  @click="$emit('start-timer', task)"
                  title="開始計時"
                >
                  <ClockIcon class="w-4 h-4" />
                </BaseButton>
                <BaseButton
                  variant="outline"
                  size="small"
                  @click="$emit('edit-task', task)"
                >
                  <PencilIcon class="w-4 h-4" />
                </BaseButton>
                <BaseButton
                  variant="outline"
                  size="small"
                  @click="$emit('delete-task', task.id)"
                  class="text-red-600 hover:text-red-900"
                >
                  <TrashIcon class="w-4 h-4" />
                </BaseButton>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { 
  ListBulletIcon, 
  ClockIcon, 
  PencilIcon, 
  TrashIcon 
} from '@heroicons/vue/24/outline'
import type { WorkTask, WorkTaskStatus } from '@/types/api'
import BaseButton from '@/components/ui/BaseButton.vue'

// Props
interface Props {
  tasks: WorkTask[]
  loading: boolean
}

defineProps<Props>()

// Emits
defineEmits<{
  'start-timer': [task: WorkTask]
  'edit-task': [task: WorkTask]
  'delete-task': [id: number]
  'toggle-complete': [task: WorkTask]
}>()

// Methods
function getStatusLabel(status: WorkTaskStatus | string): string {
  const statusMap: Record<string, string> = {
    'Pending': '待處理',
    'Planning': '規劃中',
    'InProgress': '進行中',
    'Testing': '測試中',
    'Completed': '已完成',
    'OnHold': '暫停',
    'Cancelled': '已取消'
  }
  return statusMap[status] || '未知'
}

function getStatusStyle(status: WorkTaskStatus | string): string {
  const styleMap: Record<string, string> = {
    'Pending': 'inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-gray-100 text-gray-800',
    'Planning': 'inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-blue-100 text-blue-800',
    'InProgress': 'inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-yellow-100 text-yellow-800',
    'Testing': 'inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-purple-100 text-purple-800',
    'Completed': 'inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-800',
    'OnHold': 'inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-orange-100 text-orange-800',
    'Cancelled': 'inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-red-100 text-red-800'
  }
  return styleMap[status] || 'inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-gray-100 text-gray-800'
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString('zh-TW', {
    month: 'short',
    day: 'numeric'
  })
}
</script>