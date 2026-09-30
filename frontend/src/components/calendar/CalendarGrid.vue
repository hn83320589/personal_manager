<template>
  <div class="calendar-grid select-none">
    <!-- Day of week headers -->
    <div class="grid grid-cols-7 mb-1">
      <div
        v-for="day in weekDays"
        :key="day"
        class="py-2 text-center text-xs font-semibold text-gray-500 uppercase tracking-wide"
      >
        {{ day }}
      </div>
    </div>

    <!-- Calendar days -->
    <div class="grid grid-cols-7 border-l border-t border-gray-200">
      <div
        v-for="cell in calendarCells"
        :key="cell.key"
        class="min-h-[80px] border-r border-b border-gray-200 p-1 cursor-pointer hover:bg-gray-50 transition-colors"
        :class="{ 'bg-gray-50': !cell.isCurrentMonth }"
        @click="emit('dateClick', cell.date)"
      >
        <!-- Day number -->
        <div class="flex items-center justify-center w-6 h-6 mb-1 rounded-full text-sm"
          :class="[
            cell.isToday
              ? 'bg-[var(--color-primary,#0ea5e9)] text-white font-semibold'
              : cell.isCurrentMonth
                ? 'text-gray-900'
                : 'text-gray-400'
          ]"
        >
          {{ cell.day }}
        </div>

        <!-- Events -->
        <div class="space-y-0.5">
          <div
            v-for="event in cell.events.slice(0, 3)"
            :key="event.id"
            class="truncate text-xs px-1 py-0.5 rounded cursor-pointer hover:opacity-80 transition-opacity"
            :style="{ backgroundColor: event.color || 'var(--color-primary,#0ea5e9)', color: '#fff' }"
            :title="event.title"
            @click.stop="emit('eventClick', event)"
          >
            {{ event.isAllDay ? '' : formatTime(event.startTime) + ' ' }}{{ event.title }}
          </div>
          <div
            v-if="cell.events.length > 3"
            class="text-xs text-gray-500 px-1"
          >
            +{{ cell.events.length - 3 }} 更多
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { CalendarEvent } from '@/types/api'

interface Props {
  events: CalendarEvent[]
  currentDate: Date
}

const props = defineProps<Props>()
const emit = defineEmits<{
  eventClick: [event: CalendarEvent]
  dateClick: [date: Date]
}>()

const weekDays = ['日', '一', '二', '三', '四', '五', '六']

const today = new Date()
today.setHours(0, 0, 0, 0)

const calendarCells = computed(() => {
  const year = props.currentDate.getFullYear()
  const month = props.currentDate.getMonth()

  const firstDay = new Date(year, month, 1)
  const lastDay = new Date(year, month + 1, 0)
  const startOffset = firstDay.getDay() // 0=Sun
  const totalCells = Math.ceil((startOffset + lastDay.getDate()) / 7) * 7

  return Array.from({ length: totalCells }, (_, i) => {
    const date = new Date(year, month, 1 - startOffset + i)
    date.setHours(0, 0, 0, 0)
    const isCurrentMonth = date.getMonth() === month
    const isToday = date.getTime() === today.getTime()

    const dayEvents = props.events.filter(e => {
      const start = new Date(e.startTime)
      start.setHours(0, 0, 0, 0)
      return start.getTime() === date.getTime()
    })

    return {
      key: date.toISOString(),
      date,
      day: date.getDate(),
      isCurrentMonth,
      isToday,
      events: dayEvents,
    }
  })
})

function formatTime(iso: string): string {
  const d = new Date(iso)
  return d.toLocaleTimeString('zh-TW', { hour: '2-digit', minute: '2-digit', hour12: false })
}
</script>
