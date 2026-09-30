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

    <!-- Task Settings -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
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

    </div>

    <!-- Due Date -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
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
import { ref, computed, watch } from 'vue'
import type { TodoItem, TodoStatus, TodoPriority } from '@/types/api'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseTextarea from '@/components/ui/BaseTextarea.vue'
import BaseSelect from '@/components/ui/BaseSelect.vue'
import BaseButton from '@/components/ui/BaseButton.vue'

// Props
interface Props {
  task?: TodoItem | null
}

const props = defineProps<Props>()

// Emits
const emit = defineEmits<{
  save: [data: Partial<TodoItem>]
  cancel: []
}>()

// State
const formData = ref({
  title: '',
  description: '',
  status: 'Pending' as TodoStatus,
  priority: 'Medium' as TodoPriority,
  dueDate: ''
})

// Constants
const taskStatuses = [
  { value: 'Pending', label: '待處理' },
  { value: 'InProgress', label: '進行中' },
  { value: 'Completed', label: '已完成' }
]

const taskPriorities = [
  { value: 'Low', label: '低' },
  { value: 'Medium', label: '中' },
  { value: 'High', label: '高' }
]

// Computed
const isFormValid = computed(() => {
  return formData.value.title.trim().length > 0 &&
         formData.value.status &&
         formData.value.priority
})

// Methods
function handleSubmit() {
  if (!isFormValid.value) return

  const submitData: Partial<TodoItem> = {
    title: formData.value.title.trim(),
    description: formData.value.description?.trim() || undefined,
    status: formData.value.status as TodoStatus,
    priority: formData.value.priority as TodoPriority,
    dueDate: formData.value.dueDate || undefined,
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
    title: '',
    description: '',
    status: 'Pending' as TodoStatus,
    priority: 'Medium' as TodoPriority,
    dueDate: ''
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
      dueDate: newTask.dueDate?.split('T')[0] || ''
    }
  } else {
    resetForm()
  }
}, { immediate: true })
</script>