<template>
  <form @submit.prevent="handleSubmit" class="space-y-6">
    <!-- Basic Information -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
      <!-- Task Title -->
      <div class="md:col-span-2">
        <label for="title" class="block text-sm font-medium text-gray-700">
          任務標題 <span class="text-red-500">*</span>
        </label>
        <BaseInput
          id="title"
          v-model="formData.title"
          type="text"
          placeholder="輸入任務標題"
          required
          class="mt-1"
        />
      </div>

      <!-- Description -->
      <div class="md:col-span-2">
        <label for="description" class="block text-sm font-medium text-gray-700">
          任務描述
        </label>
        <BaseTextarea
          id="description"
          v-model="formData.description"
          :rows="4"
          placeholder="詳細描述任務內容和要求..."
          class="mt-1"
        />
      </div>
    </div>

    <!-- Task Details -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
      <!-- Status -->
      <div>
        <label for="status" class="block text-sm font-medium text-gray-700">
          狀態 <span class="text-red-500">*</span>
        </label>
        <BaseSelect
          id="status"
          v-model="formData.status"
          :options="taskStatuses"
          placeholder="選擇狀態"
          required
          class="mt-1"
        />
      </div>

      <!-- Priority -->
      <div>
        <label for="priority" class="block text-sm font-medium text-gray-700">
          優先級 <span class="text-red-500">*</span>
        </label>
        <BaseSelect
          id="priority"
          v-model="formData.priority"
          :options="taskPriorities"
          placeholder="選擇優先級"
          required
          class="mt-1"
        />
      </div>

      <!-- Project -->
      <div>
        <label for="project" class="block text-sm font-medium text-gray-700">
          關聯專案
        </label>
        <select
          id="project"
          v-model="formData.projectId"
          class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
        >
          <option :value="null">無專案</option>
          <option v-for="p in projects" :key="p.id" :value="p.id">{{ p.name }}</option>
        </select>
      </div>

      <!-- Tags -->
      <div>
        <label for="tags" class="block text-sm font-medium text-gray-700">
          標籤
        </label>
        <BaseInput
          id="tags"
          v-model="formData.tags"
          type="text"
          placeholder="用逗號分隔多個標籤"
          class="mt-1"
        />
        <p class="mt-1 text-xs text-gray-500">例如: 緊急, 前端, 修復</p>
      </div>
    </div>

    <!-- Dates and Time -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
      <!-- Due Date -->
      <div>
        <label for="dueDate" class="block text-sm font-medium text-gray-700">
          到期日期
        </label>
        <BaseInput
          id="dueDate"
          v-model="formData.dueDate"
          type="date"
          class="mt-1"
        />
      </div>

      <!-- Estimated Hours -->
      <div>
        <label for="estimatedHours" class="block text-sm font-medium text-gray-700">
          預估時間 (小時)
        </label>
        <BaseInput
          id="estimatedHours"
          v-model="formData.estimatedHours"
          type="number"
          min="0"
          step="0.5"
          placeholder="0"
          class="mt-1"
        />
      </div>

      <!-- Actual Hours -->
      <div>
        <label for="actualHours" class="block text-sm font-medium text-gray-700">
          實際時間 (小時)
        </label>
        <BaseInput
          id="actualHours"
          v-model="formData.actualHours"
          type="number"
          min="0"
          step="0.5"
          placeholder="0"
          class="mt-1"
          :disabled="!task" 
        />
        <p v-if="!task" class="mt-1 text-xs text-gray-500">新任務建立後可編輯</p>
      </div>
    </div>

    <!-- Task Progress (for existing tasks) -->
    <div v-if="task && formData.estimatedHours && formData.estimatedHours > 0" class="space-y-3">
      <label class="block text-sm font-medium text-gray-700">任務進度</label>
      <div class="bg-gray-50 rounded-lg p-4">
        <div class="flex justify-between items-center mb-2">
          <span class="text-sm text-gray-600">完成度</span>
          <span class="text-sm font-medium">{{ progressPercentage }}%</span>
        </div>
        <div class="w-full bg-gray-200 rounded-full h-3">
          <div 
            class="bg-blue-600 h-3 rounded-full transition-all duration-300"
            :style="{ width: `${progressPercentage}%` }"
          ></div>
        </div>
        <div class="mt-2 grid grid-cols-2 gap-4 text-sm">
          <div class="text-gray-600">
            預估: {{ formData.estimatedHours || 0 }} 小時
          </div>
          <div class="text-gray-600">
            實際: {{ formData.actualHours || 0 }} 小時
          </div>
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
        {{ task ? '更新任務' : '建立任務' }}
      </BaseButton>
    </div>

  </form>
</template>

<script setup lang="ts">
import { ref, computed, watch, withDefaults } from 'vue'
import type { WorkTask, WorkTaskStatus, WorkTaskPriority, Project } from '@/types/api'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseTextarea from '@/components/ui/BaseTextarea.vue'
import BaseSelect from '@/components/ui/BaseSelect.vue'
import BaseButton from '@/components/ui/BaseButton.vue'

// Props
interface Props {
  task?: WorkTask | null
  projects?: Project[]
}

const props = withDefaults(defineProps<Props>(), {
  projects: () => []
})

// Emits
const emit = defineEmits<{
  save: [data: any]
  cancel: []
}>()

const formData = ref({
  title: '',
  description: '',
  status: 'Pending' as WorkTaskStatus,
  priority: 'Medium' as WorkTaskPriority,
  dueDate: '',
  estimatedHours: 0,
  actualHours: 0,
  projectId: null as number | null,
  tags: ''
})

// Constants
const taskStatuses = [
  { value: 'Pending', label: '待處理' },
  { value: 'Planning', label: '規劃中' },
  { value: 'InProgress', label: '進行中' },
  { value: 'Testing', label: '測試中' },
  { value: 'Completed', label: '已完成' },
  { value: 'OnHold', label: '暫停' },
  { value: 'Cancelled', label: '已取消' }
]

const taskPriorities = [
  { value: 'Low', label: '低' },
  { value: 'Medium', label: '中' },
  { value: 'High', label: '高' },
  { value: 'Urgent', label: '緊急' }
]

// Computed
const isFormValid = computed(() => {
  return formData.value.title.trim().length > 0 &&
         formData.value.status !== null &&
         formData.value.priority !== null
})

const progressPercentage = computed(() => {
  const estimated = formData.value.estimatedHours || 0
  const actual = formData.value.actualHours || 0
  
  if (estimated === 0) return 0
  
  return Math.min(Math.round((actual / estimated) * 100), 100)
})

// Methods
function handleSubmit() {
  if (!isFormValid.value) return

  const submitData = {
    title: formData.value.title.trim(),
    description: formData.value.description?.trim() || undefined,
    status: formData.value.status,
    priority: formData.value.priority,
    dueDate: formData.value.dueDate || undefined,
    estimatedHours: formData.value.estimatedHours || undefined,
    actualHours: formData.value.actualHours || undefined,
    projectId: formData.value.projectId ?? undefined,
    tags: formData.value.tags?.trim() || undefined
  }

  emit('save', submitData)
}

function resetForm() {
  formData.value = {
    title: '',
    description: '',
    status: 'Pending' as WorkTaskStatus,
    priority: 'Medium' as WorkTaskPriority,
    dueDate: '',
    estimatedHours: 0,
    actualHours: 0,
    projectId: null,
    tags: ''
  }
}

// Watch for task changes
watch(() => props.task, (newTask) => {
  if (newTask) {
    formData.value = {
      title: newTask.title || '',
      description: newTask.description || '',
      status: newTask.status || 'Pending',
      priority: newTask.priority || 'Medium',
      dueDate: newTask.dueDate || '',
      estimatedHours: newTask.estimatedHours || 0,
      actualHours: newTask.actualHours || 0,
      projectId: newTask.projectId ?? null,
      tags: newTask.tags || ''
    }
  } else {
    resetForm()
  }
}, { immediate: true })
</script>

<style scoped>
.line-clamp-1 {
  display: -webkit-box;
  -webkit-line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>