<template>
  <div class="space-y-6">
    <!-- Event Header -->
    <div class="flex items-start justify-between">
      <div class="flex items-start space-x-3">
        <div
          class="flex-shrink-0 w-4 h-4 rounded-full mt-1"
          :style="{ backgroundColor: event.color || '#6B7280' }"
        ></div>
        <div>
          <h3 class="text-lg font-semibold text-gray-900">{{ event.title }}</h3>
          <div class="flex items-center space-x-2 mt-1">
            <span :class="[
              'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium',
              'bg-gray-100 text-gray-800'
            ]">
              一般
            </span>
            <span :class="[
              'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
              event.isPublic
                ? 'bg-green-100 text-green-800'
                : 'bg-gray-100 text-gray-800'
            ]">
              {{ event.isPublic ? '公開' : '私人' }}
            </span>
            <span :class="[
              'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
              getEventStatusStyle(event)
            ]">
              {{ getEventStatus(event) }}
            </span>
          </div>
        </div>
      </div>
      <div class="flex space-x-2">
        <BaseButton
          variant="outline"
          size="small"
          @click="$emit('edit', event)"
        >
          <PencilIcon class="w-4 h-4 mr-2" />
          編輯
        </BaseButton>
        <BaseButton
          variant="outline"
          size="small"
          @click="$emit('delete', event.id)"
          class="text-red-600 hover:text-red-900"
        >
          <TrashIcon class="w-4 h-4 mr-2" />
          刪除
        </BaseButton>
      </div>
    </div>

    <!-- Event Details -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
      <!-- Left Column -->
      <div class="space-y-4">
        <!-- Date and Time -->
        <div>
          <div class="flex items-center space-x-2 mb-2">
            <CalendarIcon class="w-5 h-5 text-gray-400" />
            <h4 class="text-sm font-medium text-gray-700">日期時間</h4>
          </div>
          <div class="ml-7 space-y-1">
            <div class="text-sm text-gray-900">
              {{ formatEventDate(event) }}
            </div>
            <div class="text-sm text-gray-500">
              {{ formatEventTime(event) }}
            </div>
            <div v-if="getEventDuration(event)" class="text-xs text-gray-400">
              持續時間: {{ getEventDuration(event) }}
            </div>
          </div>
        </div>

        <!-- Description -->
        <div v-if="event.description">
          <div class="flex items-center space-x-2 mb-2">
            <DocumentTextIcon class="w-5 h-5 text-gray-400" />
            <h4 class="text-sm font-medium text-gray-700">描述</h4>
          </div>
          <div class="ml-7">
            <p class="text-sm text-gray-900 whitespace-pre-wrap">{{ event.description }}</p>
          </div>
        </div>
      </div>

      <!-- Right Column -->
      <div class="space-y-4">
        <!-- Event Metadata -->
        <div>
          <div class="flex items-center space-x-2 mb-2">
            <InformationCircleIcon class="w-5 h-5 text-gray-400" />
            <h4 class="text-sm font-medium text-gray-700">事件資訊</h4>
          </div>
          <div class="ml-7 space-y-2">
            <div class="flex justify-between text-sm">
              <span class="text-gray-500">建立時間:</span>
              <span class="text-gray-900">{{ formatDateTime(event.createdAt) }}</span>
            </div>
          </div>
        </div>

        <!-- Quick Actions -->
        <div>
          <div class="flex items-center space-x-2 mb-2">
            <CommandLineIcon class="w-5 h-5 text-gray-400" />
            <h4 class="text-sm font-medium text-gray-700">快速操作</h4>
          </div>
          <div class="ml-7 space-y-2">
            <BaseButton
              variant="outline"
              size="small"
              @click="copyEventDetails"
              class="w-full justify-start"
            >
              <ClipboardIcon class="w-4 h-4 mr-2" />
              複製事件詳細
            </BaseButton>
            <BaseButton
              variant="outline"
              size="small"
              @click="exportToCalendar"
              class="w-full justify-start"
            >
              <CalendarDaysIcon class="w-4 h-4 mr-2" />
              匯出到行事曆
            </BaseButton>
            <BaseButton
              variant="outline"
              size="small"
              disabled
              class="w-full justify-start"
              title="Google Calendar 同步功能尚未開放"
            >
              <CloudArrowUpIcon class="w-4 h-4 mr-2" />
              同步到 Google
            </BaseButton>
          </div>
        </div>

        <!-- Reminders (if any) -->
        <div v-if="hasReminders">
          <div class="flex items-center space-x-2 mb-2">
            <BellIcon class="w-5 h-5 text-gray-400" />
            <h4 class="text-sm font-medium text-gray-700">提醒設定</h4>
          </div>
          <div class="ml-7 space-y-1">
            <div
              v-for="reminder in eventReminders"
              :key="reminder.id"
              class="text-sm text-gray-900"
            >
              {{ reminder.description }}
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Related Events (Future feature) -->
    <div v-if="relatedEvents.length > 0" class="pt-4 border-t border-gray-200">
      <h4 class="text-sm font-medium text-gray-700 mb-3">相關事件</h4>
      <div class="space-y-2">
        <div
          v-for="relatedEvent in relatedEvents"
          :key="relatedEvent.id"
          class="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
        >
          <div class="flex items-center space-x-3">
            <div
              class="w-3 h-3 rounded-full"
              :style="{ backgroundColor: relatedEvent.color }"
            ></div>
            <div>
              <div class="text-sm font-medium text-gray-900">{{ relatedEvent.title }}</div>
              <div class="text-xs text-gray-500">{{ formatEventDate(relatedEvent) }}</div>
            </div>
          </div>
          <BaseButton
            variant="outline"
            size="small"
            @click="viewRelatedEvent(relatedEvent)"
          >
            查看
          </BaseButton>
        </div>
      </div>
    </div>

    <!-- Action Buttons -->
    <div class="flex justify-end space-x-3 pt-6 border-t border-gray-200">
      <BaseButton
        variant="outline"
        @click="$emit('close')"
      >
        關閉
      </BaseButton>
      <BaseButton
        variant="primary"
        @click="$emit('edit', event)"
      >
        編輯事件
      </BaseButton>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import {
  PencilIcon,
  TrashIcon,
  CalendarIcon,
  DocumentTextIcon,
  InformationCircleIcon,
  CommandLineIcon,
  ClipboardIcon,
  CalendarDaysIcon,
  CloudArrowUpIcon,
  BellIcon
} from '@heroicons/vue/24/outline'
import type { CalendarEvent } from '@/types/api'
import BaseButton from '@/components/ui/BaseButton.vue'

// Props
interface Props {
  event: CalendarEvent
}

const props = defineProps<Props>()

// Emits
const emit = defineEmits<{
  edit: [event: CalendarEvent]
  delete: [id: number]
  close: []
  'view-related': [event: CalendarEvent]
}>()

// State
const relatedEvents = ref<CalendarEvent[]>([]) // Future feature
const eventReminders = ref<any[]>([]) // Future feature

// Computed
const hasReminders = computed(() => eventReminders.value.length > 0)

// Methods
function getEventStatus(event: CalendarEvent): string {
  const now = new Date()
  const start = new Date(event.startTime)
  const end = event.endTime ? new Date(event.endTime) : start

  if (start > now) return '即將到來'
  if (start <= now && end >= now) return '進行中'
  return '已結束'
}

function getEventStatusStyle(event: CalendarEvent): string {
  const status = getEventStatus(event)
  switch (status) {
    case '即將到來': return 'bg-blue-100 text-blue-800'
    case '進行中': return 'bg-green-100 text-green-800'
    case '已結束': return 'bg-gray-100 text-gray-800'
    default: return 'bg-gray-100 text-gray-800'
  }
}

function formatEventDate(event: CalendarEvent): string {
  const start = new Date(event.startTime)
  const end = event.endTime ? new Date(event.endTime) : null

  const startDateStr = start.toLocaleDateString('zh-TW', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    weekday: 'long'
  })

  if (end && end.toDateString() !== start.toDateString()) {
    const endDateStr = end.toLocaleDateString('zh-TW', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      weekday: 'long'
    })
    return `${startDateStr} 至 ${endDateStr}`
  }

  return startDateStr
}

function formatEventTime(event: CalendarEvent): string {
  if (event.isAllDay) return '全天事件'

  const startDate = new Date(event.startTime)
  const startTimeStr = startDate.toLocaleTimeString('zh-TW', { hour: '2-digit', minute: '2-digit' })

  if (event.endTime && event.endTime !== event.startTime) {
    const endDate = new Date(event.endTime)
    const endTimeStr = endDate.toLocaleTimeString('zh-TW', { hour: '2-digit', minute: '2-digit' })
    return `${startTimeStr} - ${endTimeStr}`
  }

  return startTimeStr
}

function formatDateTime(dateString: string): string {
  return new Date(dateString).toLocaleString('zh-TW', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

function getEventDuration(event: CalendarEvent): string {
  if (event.isAllDay) return ''

  if (!event.endTime || event.endTime === event.startTime) return ''

  const start = new Date(event.startTime)
  const end = new Date(event.endTime)

  const diffMs = end.getTime() - start.getTime()
  if (diffMs <= 0) return ''

  const diffHours = diffMs / (1000 * 60 * 60)

  if (diffHours < 1) {
    const diffMinutes = diffMs / (1000 * 60)
    return `${Math.round(diffMinutes)} 分鐘`
  } else if (diffHours === 1) {
    return '1 小時'
  } else {
    return `${diffHours} 小時`
  }
}

async function copyEventDetails() {
  const details = `
事件: ${props.event.title}
日期: ${formatEventDate(props.event)}
時間: ${formatEventTime(props.event)}
${props.event.description ? `描述: ${props.event.description}` : ''}
`.trim()

  try {
    await navigator.clipboard.writeText(details)
    alert('事件詳細已複製到剪貼板')
  } catch (err) {
    console.error('複製失敗:', err)
    alert('無法複製到剪貼板，請稍後再試')
  }
}

function exportToCalendar() {
  // Generate ICS file for calendar export
  const event = props.event
  const startDate = new Date(event.startTime)
  const endDate = event.endTime
    ? new Date(event.endTime)
    : new Date(startDate.getTime() + 60 * 60 * 1000) // Default 1 hour

  const formatICSDate = (date: Date) => {
    return date.toISOString().replace(/[-:]/g, '').replace(/\.\d{3}/, '')
  }

  const icsContent = `BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//Personal Manager//Event//EN
BEGIN:VEVENT
UID:${event.id}@personalmanager.com
DTSTAMP:${formatICSDate(new Date())}
DTSTART:${formatICSDate(startDate)}
DTEND:${formatICSDate(endDate)}
SUMMARY:${event.title}
${event.description ? `DESCRIPTION:${event.description.replace(/\n/g, '\\n')}` : ''}
END:VEVENT
END:VCALENDAR`

  const blob = new Blob([icsContent], { type: 'text/calendar' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = `${event.title}.ics`
  link.click()
  URL.revokeObjectURL(url)
}

function syncToGoogle() {
  /**
   * Google Calendar 同步功能
   *
   * 注意：此功能需要實作 Google Calendar API 整合 (Phase 2.3 已規劃)
   *
   * 實作步驟：
   * 1. 引入 googleCalendarService (已存在於 src/services/googleCalendarService.ts)
   * 2. 使用 googleCalendarService.exportEvent(props.event) 匯出事件
   * 3. 處理 OAuth 認證流程（如未授權）
   * 4. 顯示同步進度與結果通知
   * 5. 處理錯誤情況（網路、授權、衝突等）
   *
   * 相關檔案：
   * - src/services/googleCalendarService.ts
   * - src/components/admin/GoogleCalendarSync.vue
   */
}

function viewRelatedEvent(event: CalendarEvent) {
  /**
   * 查看相關事件功能
   *
   * 功能說明：顯示與當前事件相關的其他事件
   *
   * 實作建議：
   * 1. 根據以下條件尋找相關事件：
   *    - 相同的標籤 (tags)
   *    - 時間接近（前後7天內）
   * 2. 使用模態視窗或側邊欄顯示相關事件列表
   * 3. 支援快速切換到相關事件的詳細資訊
   * 4. 提供「新增為相關事件」的功能
   *
   * 資料結構需求：
   * - CalendarEvent 可能需要新增 relatedEventIds?: number[] 屬性
   * - 後端 API 需要支援 /api/calendarevents/{id}/related 端點
   */
  emit('view-related', event)
}
</script>