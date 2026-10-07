<template>
  <div class="overflow-hidden rounded-xl border border-rule bg-surface">
    <div class="grid grid-cols-7 border-b border-rule bg-paper text-center text-xs text-muted">
      <span v-for="name in WEEKDAYS" :key="name" class="py-2">{{ name }}</span>
    </div>
    <div class="grid grid-cols-7">
      <button
        v-for="day in days"
        :key="day.key"
        type="button"
        :aria-label="`${day.label}${day.events.length ? `，${day.events.length} 個行程` : ''}`"
        :aria-pressed="day.key === selected"
        :class="[
          'flex min-h-[4.5rem] flex-col items-stretch gap-0.5 border-b border-r border-rule p-1 text-left align-top [&:nth-child(7n)]:border-r-0 sm:min-h-24',
          day.inMonth ? '' : 'bg-paper/60 text-muted',
          day.key === selected ? 'ring-2 ring-inset ring-accent' : 'hover:bg-soft',
        ]"
        @click="$emit('select', day.key)"
      >
        <span
          :class="[
            'grid h-6 w-6 place-items-center rounded-full text-xs tabular-nums',
            day.key === today ? 'bg-accent font-bold text-accent-ink' : '',
          ]"
        >
          {{ day.date.getDate() }}
        </span>
        <!-- 手機只顯示色點，較寬的畫面顯示標題 -->
        <span class="flex flex-wrap gap-0.5 sm:hidden">
          <i
            v-for="(e, i) in day.events.slice(0, 4)"
            :key="i"
            class="h-1.5 w-1.5 rounded-full"
            :style="{ background: colorOf(e) }"
          />
        </span>
        <span
          v-for="(e, i) in day.events.slice(0, 3)"
          :key="i"
          class="hidden truncate rounded px-1 text-[0.72rem] sm:block"
          :style="{ background: `${colorOf(e)}22`, color: 'rgb(var(--ink))' }"
        >
          {{ e.title }}
        </span>
        <span v-if="day.events.length > 3" class="hidden text-[0.7rem] text-muted sm:block">
          還有 {{ day.events.length - 3 }} 個
        </span>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Schemas } from '@/api/types'
import { groupByDay, monthGrid, WEEKDAYS } from '@/lib/calendar'
import { localDate } from '@/lib/format'

type Occurrence = Schemas['OccurrenceDto']

const props = defineProps<{ year: number; month: number; events: Occurrence[]; selected: string }>()
defineEmits<{ select: [day: string] }>()

const today = localDate()

interface DayCell {
  key: string
  date: Date
  label: string
  inMonth: boolean
  events: Occurrence[]
}

const days = computed<DayCell[]>(() => {
  const byDay = groupByDay(props.events)
  return monthGrid(props.year, props.month).map((date) => {
    const key = localDate(date)
    return {
      key,
      date,
      label: `${date.getMonth() + 1} 月 ${date.getDate()} 日`,
      inMonth: date.getMonth() === props.month,
      events: byDay.get(key) ?? [],
    }
  })
})

const colorOf = (event: Occurrence) => event.color || '#2754c5'
</script>
