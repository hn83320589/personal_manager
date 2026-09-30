<template>
  <form @submit.prevent="handleSubmit" class="space-y-6">
    <!-- Timer Selection -->
    <div class="space-y-4">
      <h3 class="text-lg font-medium text-gray-900">選擇要追蹤的任務</h3>
      
      <!-- Quick Start Option -->
      <div class="bg-blue-50 border border-blue-200 rounded-lg p-4">
        <div class="flex items-center space-x-3">
          <input
            id="quick-start"
            v-model="timerMode"
            type="radio"
            value="quick"
            class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
          />
          <label for="quick-start" class="flex-1">
            <span class="text-sm font-medium text-gray-900">快速開始</span>
            <p class="text-sm text-gray-500">立即開始計時，稍後可以指定任務</p>
          </label>
        </div>
        
        <div v-if="timerMode === 'quick'" class="mt-4 space-y-3">
          <BaseInput
            v-model="quickData.title"
            type="text"
            placeholder="輸入任務描述..."
            required
          />
          <BaseInput
            v-model="quickData.project"
            type="text"
            placeholder="專案名稱 (可選)"
          />
        </div>
      </div>

      <!-- Select Existing Task -->
      <div class="bg-gray-50 border border-gray-200 rounded-lg p-4">
        <div class="flex items-center space-x-3">
          <input
            id="select-task"
            v-model="timerMode"
            type="radio"
            value="existing"
            class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
          />
          <label for="select-task" class="flex-1">
            <span class="text-sm font-medium text-gray-900">選擇現有任務</span>
            <p class="text-sm text-gray-500">從已建立的任務中選擇</p>
          </label>
        </div>
        
        <div v-if="timerMode === 'existing'" class="mt-4 space-y-4">
          <!-- Task Search -->
          <div class="relative">
            <MagnifyingGlassIcon class="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
            <input
              v-model="taskSearchQuery"
              type="text"
              placeholder="搜尋任務..."
              class="pl-10 pr-4 py-2 w-full border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            />
          </div>

          <!-- Task List -->
          <div class="max-h-60 overflow-y-auto space-y-2">
            <div
              v-for="task in filteredTasks"
              :key="task.id"
              @click="selectTask(task)"
              :class="[
                'p-3 border rounded-lg cursor-pointer transition-colors',
                selectedTaskId === task.id
                  ? 'border-blue-500 bg-blue-50'
                  : 'border-gray-200 hover:bg-gray-50'
              ]"
            >
              <div class="flex items-center justify-between">
                <div class="flex-1">
                  <h4 class="text-sm font-medium text-gray-900">{{ task.title }}</h4>
                  <p class="text-xs text-gray-500 mt-1">{{ task.description }}</p>
                  <div class="flex items-center space-x-4 mt-2">
                    <span class="text-xs text-gray-500">
                      專案: {{ task.projectName || '無' }}
                    </span>
                    <span :class="getStatusStyle(task.status)">
                      {{ getStatusLabel(task.status) }}
                    </span>
                    <span :class="getPriorityStyle(task.priority)">
                      {{ getPriorityLabel(task.priority) }}
                    </span>
                  </div>
                  <div class="flex items-center space-x-4 mt-1 text-xs text-gray-500">
                    <span>預估: {{ task.estimatedHours || 0 }}h</span>
                    <span>實際: {{ task.actualHours || 0 }}h</span>
                    <span v-if="task.dueDate">到期: {{ formatDate(task.dueDate) }}</span>
                  </div>
                </div>
                <div class="ml-4">
                  <CheckIcon 
                    v-if="selectedTaskId === task.id"
                    class="w-5 h-5 text-blue-600"
                  />
                </div>
              </div>
            </div>
          </div>

          <div v-if="filteredTasks.length === 0" class="text-center py-8 text-gray-500">
            <ListBulletIcon class="mx-auto h-8 w-8 text-gray-400 mb-2" />
            <p class="text-sm">{{ taskSearchQuery ? '未找到符合的任務' : '沒有可用的任務' }}</p>
          </div>
        </div>
      </div>

      <!-- Manual Entry -->
      <div class="bg-gray-50 border border-gray-200 rounded-lg p-4">
        <div class="flex items-center space-x-3">
          <input
            id="manual-entry"
            v-model="timerMode"
            type="radio"
            value="manual"
            class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
          />
          <label for="manual-entry" class="flex-1">
            <span class="text-sm font-medium text-gray-900">手動輸入時間</span>
            <p class="text-sm text-gray-500">記錄已完成的工作時間</p>
          </label>
        </div>
        
        <div v-if="timerMode === 'manual'" class="mt-4 space-y-4">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">
                任務標題 <span class="text-red-500">*</span>
              </label>
              <BaseInput
                v-model="manualData.title"
                type="text"
                placeholder="輸入任務標題"
                required
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">
                專案名稱
              </label>
              <BaseInput
                v-model="manualData.project"
                type="text"
                placeholder="輸入專案名稱"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">
                日期 <span class="text-red-500">*</span>
              </label>
              <BaseInput
                v-model="manualData.date"
                type="date"
                required
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">
                工作時間 (小時) <span class="text-red-500">*</span>
              </label>
              <BaseInput
                v-model="manualData.duration"
                type="number"
                min="0.25"
                step="0.25"
                placeholder="1.5"
                required
              />
            </div>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              工作描述
            </label>
            <BaseTextarea
              v-model="manualData.description"
              :rows="3"
              placeholder="描述完成的工作內容..."
            />
          </div>
        </div>
      </div>
    </div>

    <!-- Timer Settings (for quick and existing modes) -->
    <div v-if="timerMode !== 'manual'" class="space-y-4">
      <h3 class="text-lg font-medium text-gray-900">計時器設定</h3>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">
            提醒間隔 (分鐘)
          </label>
          <BaseSelect 
            v-model="timerSettings.reminderInterval"
            :options="[
              { label: '不提醒', value: 0 },
              { label: '15 分鐘', value: 15 },
              { label: '30 分鐘', value: 30 },
              { label: '1 小時', value: 60 },
              { label: '2 小時', value: 120 }
            ]"
          />
        </div>
        <div class="flex items-center">
          <input
            id="auto-pause"
            v-model="timerSettings.autoPause"
            type="checkbox"
            class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
          />
          <label for="auto-pause" class="ml-2 text-sm text-gray-700">
            自動暫停 (當電腦閒置時)
          </label>
        </div>
      </div>
    </div>

    <!-- Summary (for selected task) -->
    <div v-if="selectedTask" class="bg-blue-50 border border-blue-200 rounded-lg p-4">
      <h3 class="text-sm font-medium text-blue-900 mb-2">即將開始追蹤</h3>
      <div class="text-sm text-blue-800">
        <p><strong>任務:</strong> {{ selectedTask.title }}</p>
        <p v-if="selectedTask.projectName"><strong>專案:</strong> {{ selectedTask.projectName }}</p>
        <p><strong>狀態:</strong> {{ getStatusLabel(selectedTask.status) }}</p>
        <p><strong>預估時間:</strong> {{ selectedTask.estimatedHours || 0 }} 小時</p>
        <p><strong>已用時間:</strong> {{ selectedTask.actualHours || 0 }} 小時</p>
      </div>
    </div>

    <!-- Form Actions -->
    <div class="flex justify-end space-x-3 pt-6 border-t border-gray-200">
      <BaseButton
        type="button"
        variant="outline"
        @click="$emit('cancel')"
      >
        取消
      </BaseButton>
      <BaseButton
        type="submit"
        variant="primary"
        :disabled="!isFormValid"
      >
        {{ getSubmitButtonText() }}
      </BaseButton>
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import {
  MagnifyingGlassIcon,
  CheckIcon,
  ListBulletIcon
} from '@heroicons/vue/24/outline'
import type { WorkTask, WorkTaskStatus, WorkTaskPriority } from '@/types/api'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseTextarea from '@/components/ui/BaseTextarea.vue'
import BaseSelect from '@/components/ui/BaseSelect.vue'
import BaseButton from '@/components/ui/BaseButton.vue'

// Props
interface Props {
  tasks: WorkTask[]
}

const props = defineProps<Props>()

// Emits
const emit = defineEmits<{
  startTimer: [data: any]
  cancel: []
}>()

// State
const timerMode = ref<'quick' | 'existing' | 'manual'>('quick')
const taskSearchQuery = ref('')
const selectedTaskId = ref<number | null>(null)

const quickData = ref({
  title: '',
  project: ''
})

const manualData = ref({
  title: '',
  project: '',
  date: new Date().toISOString().split('T')[0],
  duration: 0,
  description: ''
})

const timerSettings = ref({
  reminderInterval: 30,
  autoPause: true
})

// Computed
const filteredTasks = computed(() => {
  if (!taskSearchQuery.value) {
    return props.tasks.filter(task => task.status !== 'Completed') // Exclude completed tasks
  }

  const query = taskSearchQuery.value.toLowerCase()
  return props.tasks.filter(task =>
    task.status !== 'Completed' && (
      task.title.toLowerCase().includes(query) ||
      task.description?.toLowerCase().includes(query) ||
      task.projectName?.toLowerCase().includes(query)
    )
  )
})

const selectedTask = computed(() => {
  if (selectedTaskId.value) {
    return props.tasks.find(task => task.id === selectedTaskId.value)
  }
  return null
})

const isFormValid = computed(() => {
  switch (timerMode.value) {
    case 'quick':
      return quickData.value.title.trim().length > 0
    case 'existing':
      return selectedTaskId.value !== null
    case 'manual':
      return manualData.value.title.trim().length > 0 &&
             manualData.value.date &&
             manualData.value.duration > 0
    default:
      return false
  }
})

// Methods
function selectTask(task: WorkTask) {
  selectedTaskId.value = task.id
}

function getSubmitButtonText(): string {
  switch (timerMode.value) {
    case 'quick':
      return '開始計時'
    case 'existing':
      return '開始追蹤'
    case 'manual':
      return '記錄時間'
    default:
      return '確定'
  }
}

function handleSubmit() {
  if (!isFormValid.value) return

  let submitData: any

  switch (timerMode.value) {
    case 'quick':
      submitData = {
        mode: 'quick',
        title: quickData.value.title.trim(),
        project: quickData.value.project?.trim() || null,
        settings: timerSettings.value
      }
      break
    
    case 'existing':
      submitData = {
        mode: 'existing',
        taskId: selectedTaskId.value,
        settings: timerSettings.value
      }
      break
    
    case 'manual':
      submitData = {
        mode: 'manual',
        title: manualData.value.title.trim(),
        project: manualData.value.project?.trim() || null,
        date: manualData.value.date,
        duration: manualData.value.duration,
        description: manualData.value.description?.trim() || null
      }
      break
  }

  emit('startTimer', submitData)
}

function getStatusLabel(status: WorkTaskStatus): string {
  const labelMap: Record<WorkTaskStatus, string> = {
    'Pending': '待處理',
    'Planning': '規劃中',
    'InProgress': '進行中',
    'Testing': '測試中',
    'Completed': '已完成',
    'OnHold': '暫停',
    'Cancelled': '已取消'
  }
  return labelMap[status] || '未知'
}

function getStatusStyle(status: WorkTaskStatus): string {
  const styleMap: Record<WorkTaskStatus, string> = {
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

function getPriorityLabel(priority: WorkTaskPriority): string {
  const priorityMap: Record<WorkTaskPriority, string> = {
    'Low': '低',
    'Medium': '中',
    'High': '高',
    'Urgent': '緊急'
  }
  return priorityMap[priority] || '未知'
}

function getPriorityStyle(priority: WorkTaskPriority): string {
  const styleMap: Record<WorkTaskPriority, string> = {
    'Low': 'inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-800',
    'Medium': 'inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-blue-100 text-blue-800',
    'High': 'inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-orange-100 text-orange-800',
    'Urgent': 'inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-red-100 text-red-800'
  }
  return styleMap[priority] || 'inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-gray-100 text-gray-800'
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString('zh-TW', {
    month: 'short',
    day: 'numeric'
  })
}
</script>