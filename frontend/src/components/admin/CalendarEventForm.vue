<template>
  <form @submit.prevent="handleSubmit" class="space-y-6">
    <!-- Event Title -->
    <div>
      <label for="title" class="block text-sm font-medium text-gray-700">
        事件標題 <span class="text-red-500">*</span>
      </label>
      <BaseInput
        id="title"
        v-model="formData.title"
        type="text"
        required
        placeholder="請輸入事件標題"
        class="mt-1"
      />
    </div>

    <!-- Description -->
    <div>
      <label for="description" class="block text-sm font-medium text-gray-700">
        事件描述
      </label>
      <BaseTextarea
        id="description"
        v-model="formData.description"
        :rows="3"
        placeholder="詳細描述此事件的內容、目的或相關資訊..."
        class="mt-1"
      />
    </div>

    <!-- Date and Time -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <!-- Start Date and Time -->
      <div class="space-y-4">
        <h3 class="text-sm font-medium text-gray-700">開始時間</h3>
        
        <div>
          <label for="startDate" class="block text-sm font-medium text-gray-700">
            開始日期 <span class="text-red-500">*</span>
          </label>
          <input
            id="startDate"
            v-model="formData.startDate"
            type="date"
            required
            class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          />
        </div>

        <div v-if="!formData.isAllDay">
          <label for="startTime" class="block text-sm font-medium text-gray-700">
            開始時間 <span class="text-red-500">*</span>
          </label>
          <input
            id="startTime"
            v-model="formData.startTime"
            type="time"
            :required="!formData.isAllDay"
            class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          />
        </div>
      </div>

      <!-- End Date and Time -->
      <div class="space-y-4">
        <h3 class="text-sm font-medium text-gray-700">結束時間</h3>
        
        <div>
          <label for="endDate" class="block text-sm font-medium text-gray-700">
            結束日期
          </label>
          <input
            id="endDate"
            v-model="formData.endDate"
            type="date"
            :min="formData.startDate"
            class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          />
          <p class="mt-1 text-xs text-gray-500">
            留空表示單日事件
          </p>
        </div>

        <div v-if="!formData.isAllDay && formData.endDate">
          <label for="endTime" class="block text-sm font-medium text-gray-700">
            結束時間
          </label>
          <input
            id="endTime"
            v-model="formData.endTime"
            type="time"
            class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          />
        </div>
      </div>
    </div>

    <!-- All Day Event -->
    <div class="flex items-center">
      <input
        id="isAllDay"
        v-model="formData.isAllDay"
        type="checkbox"
        class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
      />
      <label for="isAllDay" class="ml-2 block text-sm text-gray-700">
        全天事件
      </label>
    </div>

    <!-- Event Color -->
    <div>
      <label class="block text-sm font-medium text-gray-700">
        事件顏色
      </label>
      <div class="mt-2 flex flex-wrap gap-3">
        <div
          v-for="color in eventColors"
          :key="color.value"
          @click="formData.color = color.value"
          :class="[
            'w-8 h-8 rounded-full cursor-pointer border-2 transition-all',
            formData.color === color.value
              ? 'border-gray-900 scale-110'
              : 'border-gray-300 hover:border-gray-500'
          ]"
          :style="{ backgroundColor: color.value }"
          :title="color.name"
        ></div>
        
        <!-- Custom Color Input -->
        <div class="relative">
          <input
            v-model="formData.color"
            type="color"
            class="w-8 h-8 rounded-full border-2 border-gray-300 cursor-pointer"
            title="自訂顏色"
          />
        </div>
      </div>
      <p class="mt-1 text-xs text-gray-500">
        選擇事件在行事曆中的顯示顏色
      </p>
    </div>

    <!-- Recurrence (Future feature) -->
    <div>
      <label class="block text-sm font-medium text-gray-700">
        重複設定
      </label>
      <select
        v-model="formData.recurrenceRule"
        class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
      >
        <option value="">不重複</option>
        <option value="daily">每日</option>
        <option value="weekly">每週</option>
        <option value="monthly">每月</option>
        <option value="yearly">每年</option>
      </select>
    </div>

    <!-- Reminders -->
    <div>
      <label class="block text-sm font-medium text-gray-700">
        提醒設定
      </label>
      <div class="mt-2 space-y-2">
        <div
          v-for="(reminder, index) in formData.reminders"
          :key="index"
          class="flex items-center space-x-2"
        >
          <select
            v-model="reminder.value"
            class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="0">事件開始時</option>
            <option value="5">5分鐘前</option>
            <option value="15">15分鐘前</option>
            <option value="30">30分鐘前</option>
            <option value="60">1小時前</option>
            <option value="1440">1天前</option>
          </select>
          <select
            v-model="reminder.type"
            class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="popup">彈跳提醒</option>
            <option value="email">郵件提醒</option>
          </select>
          <BaseButton
            type="button"
            variant="outline"
            size="small"
            @click="removeReminder(index)"
            class="text-red-600 hover:text-red-900"
          >
            <TrashIcon class="w-4 h-4" />
          </BaseButton>
        </div>
        <BaseButton
          type="button"
          variant="outline"
          size="small"
          @click="addReminder"
          class="mt-2"
        >
          <PlusIcon class="w-4 h-4 mr-2" />
          新增提醒
        </BaseButton>
      </div>
    </div>

    <!-- Visibility Settings -->
    <div class="space-y-4 pt-4 border-t border-gray-200">
      <h3 class="text-sm font-medium text-gray-700">顯示設定</h3>
      
      <div class="flex items-center justify-between">
        <div>
          <label class="text-sm font-medium text-gray-700">公開顯示</label>
          <p class="text-sm text-gray-500">勾選後將在公開行事曆中顯示</p>
        </div>
        <input
          id="isPublic"
          v-model="formData.isPublic"
          type="checkbox"
          class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
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
        :loading="loading"
      >
        {{ event ? '更新事件' : '建立事件' }}
      </BaseButton>
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { PlusIcon, TrashIcon } from '@heroicons/vue/24/outline'
import type { CalendarEvent } from '@/types/api'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseTextarea from '@/components/ui/BaseTextarea.vue'
import BaseButton from '@/components/ui/BaseButton.vue'

// Props
interface Props {
  event?: CalendarEvent | null
}

const props = withDefaults(defineProps<Props>(), {
  event: null
})

// Emits
const emit = defineEmits<{
  save: [data: any]
  cancel: []
}>()

// State
const loading = ref(false)

const formData = ref({
  title: '',
  description: '',
  startDate: '',
  startTime: '',
  endDate: '',
  endTime: '',
  isAllDay: false,
  color: '#3B82F6',
  recurrenceRule: '',
  reminders: [] as Array<{ value: number, type: string }>,
  isPublic: false
})

// Constants
const eventColors = [
  { name: '藍色', value: '#3B82F6' },
  { name: '綠色', value: '#10B981' },
  { name: '黃色', value: '#F59E0B' },
  { name: '紅色', value: '#EF4444' },
  { name: '紫色', value: '#8B5CF6' },
  { name: '粉色', value: '#EC4899' },
  { name: '橙色', value: '#F97316' },
  { name: '青色', value: '#06B6D4' },
  { name: '灰色', value: '#6B7280' }
]

// Watchers
watch(() => formData.value.isAllDay, (newValue) => {
  if (newValue) {
    formData.value.startTime = ''
    formData.value.endTime = ''
  } else if (!formData.value.startTime) {
    formData.value.startTime = '09:00'
  }
})

watch(() => formData.value.startDate, (newValue) => {
  if (newValue && !formData.value.endDate) {
    formData.value.endDate = newValue
  }
})

watch(() => formData.value.endDate, (newValue) => {
  if (!newValue) {
    formData.value.endTime = ''
  } else if (!formData.value.endTime && !formData.value.isAllDay && formData.value.startTime) {
    // Set default end time 1 hour after start time
    const [hours, minutes] = formData.value.startTime.split(':')
    const endHour = (parseInt(hours) + 1).toString().padStart(2, '0')
    formData.value.endTime = `${endHour}:${minutes}`
  }
})

// Methods
function addReminder() {
  formData.value.reminders.push({ value: 15, type: 'popup' })
}

function removeReminder(index: number) {
  formData.value.reminders.splice(index, 1)
}

async function handleSubmit() {
  loading.value = true
  
  try {
    const submitData = { ...formData.value }
    
    // Handle all-day events
    if (submitData.isAllDay) {
      submitData.startTime = '00:00'
      submitData.endTime = '23:59'
    }
    
    // Ensure end date is set for single-day events
    if (!submitData.endDate) {
      submitData.endDate = submitData.startDate
    }
    
    // Combine date and time into ISO strings for API
    const startTimeISO = submitData.isAllDay
      ? `${submitData.startDate}T00:00:00`
      : `${submitData.startDate}T${submitData.startTime}:00`
    const endTimeISO = submitData.endDate
      ? (submitData.isAllDay
          ? `${submitData.endDate}T23:59:00`
          : `${submitData.endDate}T${submitData.endTime || '23:59'}:00`)
      : undefined

    // Prepare final data for API
    const finalData = {
      title: submitData.title,
      description: submitData.description || undefined,
      startTime: startTimeISO,
      endTime: endTimeISO,
      isAllDay: submitData.isAllDay,
      color: submitData.color,
      recurrenceRule: submitData.recurrenceRule || '',
      isPublic: submitData.isPublic
    }
    
    emit('save', finalData)
  } catch (error) {
    console.error('Submit error:', error)
  } finally {
    loading.value = false
  }
}

function initializeForm() {
  if (props.event) {
    const event = props.event

    // Parse startTime/endTime ISO strings into date and time parts
    const startDate = event.startTime ? event.startTime.split('T')[0] : ''
    const startTimePart = event.startTime ? event.startTime.split('T')[1]?.slice(0, 5) : ''
    const endDate = event.endTime ? event.endTime.split('T')[0] : ''
    const endTimePart = event.endTime ? event.endTime.split('T')[1]?.slice(0, 5) : ''

    formData.value.title = event.title
    formData.value.description = event.description || ''
    formData.value.startDate = startDate
    formData.value.startTime = startTimePart || ''
    formData.value.endDate = endDate
    formData.value.endTime = endTimePart || ''
    formData.value.isAllDay = event.isAllDay
    formData.value.color = event.color || '#3B82F6'
    formData.value.recurrenceRule = event.recurrenceRule || ''
    formData.value.isPublic = event.isPublic
  } else {
    // Reset form for new event
    const now = new Date()
    const today = now.toISOString().split('T')[0]
    const currentTime = now.toTimeString().slice(0, 5)

    formData.value = {
      title: '',
      description: '',
      startDate: today,
      startTime: currentTime,
      endDate: '',
      endTime: '',
      isAllDay: false,
      color: '#3B82F6',
      recurrenceRule: '',
      reminders: [],
      isPublic: false
    }
  }
}

// Lifecycle
onMounted(() => {
  initializeForm()
})

// Watch for event changes
watch(() => props.event, () => {
  initializeForm()
}, { immediate: true })
</script>