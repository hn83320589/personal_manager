<template>
  <div class="week-view overflow-x-auto">
    <!-- Day headers -->
    <div class="grid border-b border-gray-200" :style="gridCols">
      <div class="w-12 flex-shrink-0"></div>
      <div
        v-for="day in weekDays"
        :key="day.key"
        class="py-2 text-center border-l border-gray-200"
      >
        <p class="text-xs text-gray-500 uppercase">{{ day.label }}</p>
        <p
          class="mt-1 mx-auto w-8 h-8 flex items-center justify-center rounded-full text-sm font-medium"
          :class="day.isToday
            ? 'bg-[var(--color-primary,#0ea5e9)] text-white'
            : 'text-gray-900'"
        >
          {{ day.date }}
        </p>
      </div>
    </div>

    <!-- Time grid -->
    <div class="relative">
      <div
        v-for="hour in hours"
        :key="hour"
        class="grid items-start"
        :style="gridCols"
      >
        <!-- Time label -->
        <div class="w-12 pr-2 text-right text-xs text-gray-400 -mt-2">{{ formatHour(hour) }}</div>

        <!-- Day columns -->
        <div
          v-for="day in weekDays"
          :key="day.key"
          class="border-l border-t border-gray-100 h-14 relative cursor-pointer hover:bg-blue-50/30 transition-colors"
          @click="emit('timeSlotClick', day.dateObj, formatHour(hour))"
        >
          <div
            v-for="event in getEventsForSlot(day.dateObj, hour)"
            :key="event.id"
            class="absolute inset-x-0.5 top-0.5 rounded text-xs px-1 py-0.5 truncate cursor-pointer hover:opacity-80 z-10"
            :style="{ backgroundColor: event.color || 'var(--color-primary,#0ea5e9)', color: '#fff' }"
            :title="event.title"
            @click.stop="emit('eventClick', event)"
          >
            {{ event.title }}
          </div>
        </div>
      </div>
    </div>

    <!-- All-day events row -->
    <div v-if="allDayEventsByDay.some(d => d.length > 0)" class="grid border-b border-gray-200 bg-gray-50" :style="gridCols">
      <div class="w-12 text-xs text-gray-400 flex items-center justify-end pr-2">全天</div>
      <div
        v-for="(dayEvents, idx) in allDayEventsByDay"
        :key="idx"
        class="border-l border-gray-200 p-1 min-h-[28px]"
      >
        <div
          v-for="event in dayEvents"
          :key="event.id"
          class="truncate text-xs px-1 py-0.5 rounded mb-0.5 cursor-pointer hover:opacity-80"
          :style="{ backgroundColor: event.color || 'var(--color-primary,#0ea5e9)', color: '#fff' }"
          @click.stop="emit('eventClick', event)"
        >
          {{ event.title }}
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
  timeSlotClick: [date: Date, time: string]
}>()

const gridCols = { display: 'grid', gridTemplateColumns: '3rem repeat(7, 1fr)' }
const hours = Array.from({ length: 24 }, (_, i) => i)
const todayStr = new Date().toDateString()

const weekDays = computed(() => {
  const d = new Date(props.currentDate)
  const day = d.getDay()
  const monday = new Date(d)
  monday.setDate(d.getDate() - day) // start from Sunday

  return Array.from({ length: 7 }, (_, i) => {
    const date = new Date(monday)
    date.setDate(monday.getDate() + i)
    return {
      key: date.toISOString(),
      label: ['日', '一', '二', '三', '四', '五', '六'][i],
      date: date.getDate(),
      dateObj: date,
      isToday: date.toDateString() === todayStr,
    }
  })
})

const allDayEventsByDay = computed(() =>
  weekDays.value.map(d =>
    props.events.filter(e => {
      if (!e.isAllDay) return false
      const start = new Date(e.startTime)
      start.setHours(0, 0, 0, 0)
      const target = new Date(d.dateObj)
      target.setHours(0, 0, 0, 0)
      return start.getTime() === target.getTime()
    })
  )
)

function getEventsForSlot(date: Date, hour: number): CalendarEvent[] {
  return props.events.filter(e => {
    if (e.isAllDay) return false
    const start = new Date(e.startTime)
    const sameDay =
      start.getFullYear() === date.getFullYear() &&
      start.getMonth() === date.getMonth() &&
      start.getDate() === date.getDate()
    return sameDay && start.getHours() === hour
  })
}

function formatHour(h: number): string {
  return `${String(h).padStart(2, '0')}:00`
}
</script>
