<template>
  <form @submit.prevent="handleSubmit" class="space-y-6">
    <!-- Entry Information -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
      <!-- Date -->
      <div>
        <label for="date" class="block text-sm font-medium text-gray-700">
          日期 <span class="text-red-500">*</span>
        </label>
        <BaseInput
          id="date"
          v-model="formData.date"
          type="date"
          required
          class="mt-1"
        />
      </div>

      <!-- Task/Activity -->
      <div>
        <label for="task" class="block text-sm font-medium text-gray-700">
          任務/活動 <span class="text-red-500">*</span>
        </label>
        <div class="mt-1 flex space-x-2">
          <BaseInput
            id="task"
            v-model="formData.task"
            type="text"
            placeholder="輸入任務名稱"
            required
            class="flex-1"
          />
          <BaseButton
            type="button"
            variant="outline"
            size="small"
            @click="showTaskSelector = true"
            title="選擇現有任務"
          >
            <ListBulletIcon class="w-4 h-4" />
          </BaseButton>
        </div>
      </div>

      <!-- Project -->
      <div>
        <label for="project" class="block text-sm font-medium text-gray-700">
          專案
        </label>
        <BaseInput
          id="project"
          v-model="formData.project"
          type="text"
          placeholder="輸入專案名稱"
          class="mt-1"
        />
      </div>

      <!-- Task ID (if linked to specific task) -->
      <div v-if="formData.taskId">
        <label class="block text-sm font-medium text-gray-700">
          關聯任務
        </label>
        <div class="mt-1 bg-gray-50 border border-gray-200 rounded-md p-2">
          <span class="text-sm text-gray-900">{{ linkedTaskTitle }}</span>
          <BaseButton
            type="button"
            variant="outline"
            size="small"
            @click="unlinkTask"
            class="ml-2"
          >
            取消關聯
          </BaseButton>
        </div>
      </div>
    </div>

    <!-- Time Information -->
    <div class="space-y-4">
      <h3 class="text-lg font-medium text-gray-900">時間資訊</h3>
      
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <!-- Start Time -->
        <div>
          <label for="startTime" class="block text-sm font-medium text-gray-700">
            開始時間
          </label>
          <BaseInput
            id="startTime"
            v-model="formData.startTime"
            type="time"
            class="mt-1"
          />
        </div>

        <!-- End Time -->
        <div>
          <label for="endTime" class="block text-sm font-medium text-gray-700">
            結束時間
          </label>
          <BaseInput
            id="endTime"
            v-model="formData.endTime"
            type="time"
            class="mt-1"
          />
        </div>

        <!-- Duration -->
        <div>
          <label for="duration" class="block text-sm font-medium text-gray-700">
            時長 (分鐘) <span class="text-red-500">*</span>
          </label>
          <BaseInput
            id="duration"
            v-model="formData.duration"
            type="number"
            min="1"
            step="1"
            placeholder="60"
            required
            class="mt-1"
          />
          <p class="mt-1 text-xs text-gray-500">{{ formatDurationDisplay(formData.duration) }}</p>
        </div>
      </div>

      <!-- Auto-calculate helper -->
      <div v-if="formData.startTime && formData.endTime" class="bg-blue-50 border border-blue-200 rounded-lg p-3">
        <div class="flex items-center justify-between">
          <span class="text-sm text-blue-800">
            計算時長: {{ calculateDurationFromTimes() }} 分鐘 ({{ formatDurationDisplay(calculateDurationFromTimes()) }})
          </span>
          <BaseButton
            type="button"
            variant="outline"
            size="small"
            @click="useDurationFromTimes"
          >
            使用此時長
          </BaseButton>
        </div>
      </div>
    </div>

    <!-- Description -->
    <div>
      <label for="description" class="block text-sm font-medium text-gray-700">
        工作描述
      </label>
      <BaseTextarea
        id="description"
        v-model="formData.description"
        :rows="4"
        placeholder="描述此次工作的具體內容、成果或備註..."
        class="mt-1"
      />
    </div>

    <!-- Entry Summary -->
    <div v-if="isFormValid" class="bg-gray-50 border border-gray-200 rounded-lg p-4">
      <h3 class="text-sm font-medium text-gray-900 mb-2">時間記錄摘要</h3>
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
        <div>
          <p><strong>日期:</strong> {{ formatDisplayDate(formData.date) }}</p>
          <p><strong>任務:</strong> {{ formData.task }}</p>
          <p v-if="formData.project"><strong>專案:</strong> {{ formData.project }}</p>
        </div>
        <div>
          <p v-if="formData.startTime || formData.endTime">
            <strong>時間:</strong> 
            {{ formData.startTime || '--:--' }} ~ {{ formData.endTime || '--:--' }}
          </p>
          <p><strong>時長:</strong> {{ formatDurationDisplay(formData.duration) }}</p>
          <p class="text-blue-600"><strong>日薪 (估算):</strong> {{ estimatedEarnings }}</p>
        </div>
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
        {{ entry ? '更新記錄' : '建立記錄' }}
      </BaseButton>
    </div>

    <!-- Task Selector Modal -->
    <BaseModal
      :show="showTaskSelector"
      @close="showTaskSelector = false"
      title="選擇任務"
      max-width="lg"
    >
      <div class="space-y-4">
        <!-- Search -->
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
            v-for="task in filteredAvailableTasks"
            :key="task.id"
            @click="selectTaskFromModal(task)"
            class="p-3 border border-gray-200 rounded-lg cursor-pointer hover:bg-gray-50 transition-colors"
          >
            <div class="flex items-center justify-between">
              <div class="flex-1">
                <h4 class="text-sm font-medium text-gray-900">{{ task.title }}</h4>
                <p class="text-xs text-gray-500 mt-1">{{ task.description }}</p>
                <div class="flex items-center space-x-2 mt-2">
                  <span class="text-xs text-gray-500">{{ task.projectName || '無專案' }}</span>
                  <span :class="getStatusStyle(task.status)">
                    {{ getStatusLabel(task.status) }}
                  </span>
                </div>
              </div>
              <ChevronRightIcon class="w-4 h-4 text-gray-400" />
            </div>
          </div>
        </div>

        <div v-if="filteredAvailableTasks.length === 0" class="text-center py-8 text-gray-500">
          <ListBulletIcon class="mx-auto h-8 w-8 text-gray-400 mb-2" />
          <p class="text-sm">{{ taskSearchQuery ? '未找到符合的任務' : '沒有可用的任務' }}</p>
        </div>
      </div>
    </BaseModal>
  </form>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import {
  ListBulletIcon,
  MagnifyingGlassIcon,
  ChevronRightIcon
} from '@heroicons/vue/24/outline'
import type { WorkTask, WorkTaskStatus } from '@/types/api'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseTextarea from '@/components/ui/BaseTextarea.vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseModal from '@/components/ui/BaseModal.vue'

// Time Entry Interface
interface TimeEntry {
  id?: number
  taskId?: number
  task: string
  project?: string
  date: string
  startTime?: string
  endTime?: string
  duration: number
  description?: string
  createdAt?: string
  updatedAt?: string
}

// Props
interface Props {
  entry?: TimeEntry | null
  availableTasks?: WorkTask[]
}

const props = withDefaults(defineProps<Props>(), {
  availableTasks: () => []
})

// Emits
const emit = defineEmits<{
  save: [data: Partial<TimeEntry>]
  cancel: []
}>()

// State
const showTaskSelector = ref(false)
const taskSearchQuery = ref('')

const formData = ref({
  taskId: undefined as number | undefined,
  task: '',
  project: '',
  date: new Date().toISOString().split('T')[0],
  startTime: '',
  endTime: '',
  duration: 0,
  description: ''
})

// Constants
const hourlyRate = 50 // Example hourly rate for estimation

// Computed
const isFormValid = computed(() => {
  return formData.value.task.trim().length > 0 &&
         formData.value.date &&
         formData.value.duration > 0
})

const linkedTaskTitle = computed(() => {
  if (formData.value.taskId) {
    const task = props.availableTasks.find(t => t.id === formData.value.taskId)
    return task?.title || 'Unknown Task'
  }
  return ''
})

const filteredAvailableTasks = computed(() => {
  if (!taskSearchQuery.value) {
    return props.availableTasks
  }
  
  const query = taskSearchQuery.value.toLowerCase()
  return props.availableTasks.filter(task =>
    task.title.toLowerCase().includes(query) ||
    task.description?.toLowerCase().includes(query) ||
    task.projectName?.toLowerCase().includes(query)
  )
})

const estimatedEarnings = computed(() => {
  const hours = formData.value.duration / 60
  const earnings = hours * hourlyRate
  return `$${earnings.toFixed(2)}`
})

// Methods
function formatDurationDisplay(minutes: number): string {
  if (!minutes || minutes === 0) return '0分鐘'
  
  const hours = Math.floor(minutes / 60)
  const mins = minutes % 60
  
  if (hours === 0) {
    return `${mins}分鐘`
  } else if (mins === 0) {
    return `${hours}小時`
  } else {
    return `${hours}小時${mins}分鐘`
  }
}

function formatDisplayDate(dateString: string): string {
  if (!dateString) return ''
  return new Date(dateString).toLocaleDateString('zh-TW', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    weekday: 'long'
  })
}

function calculateDurationFromTimes(): number {
  if (!formData.value.startTime || !formData.value.endTime) return 0
  
  const [startHour, startMin] = formData.value.startTime.split(':').map(Number)
  const [endHour, endMin] = formData.value.endTime.split(':').map(Number)
  
  const startMinutes = startHour * 60 + startMin
  let endMinutes = endHour * 60 + endMin
  
  // Handle overnight work
  if (endMinutes < startMinutes) {
    endMinutes += 24 * 60
  }
  
  return endMinutes - startMinutes
}

function useDurationFromTimes() {
  const calculatedDuration = calculateDurationFromTimes()
  if (calculatedDuration > 0) {
    formData.value.duration = calculatedDuration
  }
}

function selectTaskFromModal(task: WorkTask) {
  formData.value.taskId = task.id
  formData.value.task = task.title
  formData.value.project = task.projectName || ''
  showTaskSelector.value = false
}

function unlinkTask() {
  formData.value.taskId = undefined
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

function handleSubmit() {
  if (!isFormValid.value) return

  const submitData: Partial<TimeEntry> = {
    taskId: formData.value.taskId,
    task: formData.value.task.trim(),
    project: formData.value.project?.trim() || undefined,
    date: formData.value.date,
    startTime: formData.value.startTime || undefined,
    endTime: formData.value.endTime || undefined,
    duration: formData.value.duration,
    description: formData.value.description?.trim() || undefined
  }

  // Remove undefined values
  Object.keys(submitData).forEach(key => {
    if (submitData[key as keyof typeof submitData] === undefined) {
      delete submitData[key as keyof typeof submitData]
    }
  })

  emit('save', submitData)
}

function resetForm() {
  formData.value = {
    taskId: undefined,
    task: '',
    project: '',
    date: new Date().toISOString().split('T')[0],
    startTime: '',
    endTime: '',
    duration: 0,
    description: ''
  }
}

// Watch for entry changes
watch(() => props.entry, (newEntry) => {
  if (newEntry) {
    formData.value = {
      taskId: newEntry.taskId,
      task: newEntry.task || '',
      project: newEntry.project || '',
      date: newEntry.date || new Date().toISOString().split('T')[0],
      startTime: newEntry.startTime || '',
      endTime: newEntry.endTime || '',
      duration: newEntry.duration || 0,
      description: newEntry.description || ''
    }
  } else {
    resetForm()
  }
}, { immediate: true })

// Auto-calculate duration when times change
watch([() => formData.value.startTime, () => formData.value.endTime], () => {
  if (formData.value.startTime && formData.value.endTime) {
    const calculated = calculateDurationFromTimes()
    if (calculated > 0 && formData.value.duration === 0) {
      formData.value.duration = calculated
    }
  }
})
</script>