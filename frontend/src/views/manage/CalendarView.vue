<template>
  <AdminPage
    title="行事曆"
    description="點日期新增行程；勾選「顯示在公開行事曆」的行程，訪客才看得到。"
  >
    <template #actions>
      <button type="button" class="btn btn-primary btn-small" @click="openNew(selected)">
        新增行程
      </button>
    </template>

    <div class="flex items-center justify-between gap-3">
      <button type="button" class="btn btn-small" aria-label="上個月" @click="shift(-1)">←</button>
      <div class="flex items-center gap-3">
        <h2 class="font-latin text-lg font-bold tabular-nums">{{ year }} 年 {{ month + 1 }} 月</h2>
        <button type="button" class="text-sm text-muted hover:text-accent" @click="goToday">
          今天
        </button>
      </div>
      <button type="button" class="btn btn-small" aria-label="下個月" @click="shift(1)">→</button>
    </div>

    <PageState :loading="loading && !events" :error="error" @retry="reload">
      <MonthGrid
        :year="year"
        :month="month"
        :events="events ?? []"
        :selected="selected"
        @select="selected = $event"
      />

      <section class="flex flex-col gap-2" aria-labelledby="day-title">
        <h2 id="day-title" class="text-sm font-medium text-muted">{{ selectedLabel }}</h2>
        <p v-if="!selectedEvents.length" class="text-sm text-muted">
          沒有行程。<button
            type="button"
            class="text-accent hover:underline"
            @click="openNew(selected)"
          >
            新增
          </button>
        </p>
        <ul v-else class="flex flex-col overflow-hidden rounded-xl border border-rule bg-surface">
          <li
            v-for="event in selectedEvents"
            :key="`${event.eventId}-${event.start}`"
            class="border-rule [&+&]:border-t"
          >
            <button
              type="button"
              class="flex w-full items-center gap-3 px-4 py-2.5 text-left hover:bg-soft"
              @click="openEdit(event.eventId)"
            >
              <i
                class="h-2.5 w-2.5 shrink-0 rounded-full"
                :style="{ background: event.color || 'rgb(var(--accent))' }"
              />
              <span class="min-w-0 flex-1 truncate">{{ event.title }}</span>
              <span class="text-xs text-muted"
                >{{ timeRange(event) }}{{ event.isRecurring ? ' · 重複' : '' }}</span
              >
            </button>
          </li>
        </ul>
      </section>
    </PageState>

    <EventPanel
      v-model="form"
      :event-id="editingId"
      :saving="running"
      @close="form = null"
      @submit="submit"
      @remove="remove"
    />
  </AdminPage>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { calendarApi } from '@/api/tools'
import type { Schemas } from '@/api/types'
import { useAsyncAction } from '@/composables/useAsyncAction'
import { useAsyncData } from '@/composables/useAsyncData'
import { groupByDay } from '@/lib/calendar'
import { newEventForm, toEventForm, toEventRequest, type EventForm } from '@/lib/eventForm'
import { localDate } from '@/lib/format'
import AdminPage from '@/components/manage/AdminPage.vue'
import EventPanel from '@/components/manage/calendar/EventPanel.vue'
import MonthGrid from '@/components/manage/calendar/MonthGrid.vue'
import PageState from '@/components/public/PageState.vue'

type Occurrence = Schemas['OccurrenceDto']

const now = new Date()
const year = ref(now.getFullYear())
const month = ref(now.getMonth())
const selected = ref(localDate(now))

function shift(delta: number) {
  const next = new Date(year.value, month.value + delta, 1)
  year.value = next.getFullYear()
  month.value = next.getMonth()
  selected.value = localDate(next)
}

function goToday() {
  const today = new Date()
  year.value = today.getFullYear()
  month.value = today.getMonth()
  selected.value = localDate(today)
}

/** 載入格子上看得到的範圍（包含前後月份露出的日子）。 */
const {
  data: events,
  loading,
  error,
  reload,
} = useAsyncData(
  () => {
    const from = new Date(year.value, month.value, -6)
    const to = new Date(year.value, month.value + 1, 14)
    return calendarApi.occurrences({ from: from.toISOString(), to: to.toISOString() })
  },
  { watch: [year, month] },
)

const selectedEvents = computed(() => groupByDay(events.value ?? []).get(selected.value) ?? [])
const selectedLabel = computed(() => {
  const [y, m, d] = selected.value.split('-').map(Number)
  const date = new Date(y!, m! - 1, d!)
  return `${m} 月 ${d} 日（${'日一二三四五六'[date.getDay()]}）`
})

const pad = (n: number) => String(n).padStart(2, '0')
function timeRange(event: Occurrence) {
  if (event.isAllDay) return '全天'
  const time = (iso: string) =>
    `${pad(new Date(iso).getHours())}:${pad(new Date(iso).getMinutes())}`
  return `${time(event.start)}–${time(event.end)}`
}

const form = ref<EventForm | null>(null)
const editingId = ref<number | null>(null)
const { run, running } = useAsyncAction()

function openNew(day: string) {
  editingId.value = null
  form.value = newEventForm(day)
}

/** 重複行程的任何一次都編輯整個系列。 */
async function openEdit(eventId: number) {
  const event = await run(() => calendarApi.get(eventId))
  if (!event) return
  editingId.value = eventId
  form.value = toEventForm(event)
}

async function submit() {
  if (!form.value) return
  const body = toEventRequest(form.value)
  const saved = await run(
    () => (editingId.value ? calendarApi.update(editingId.value, body) : calendarApi.create(body)),
    { success: editingId.value ? '已更新行程' : '已新增行程' },
  )
  if (!saved) return
  form.value = null
  await reload()
}

async function remove() {
  if (!editingId.value) return
  const ok = await run(async () => (await calendarApi.remove(editingId.value!), true), {
    success: '已刪除行程',
  })
  if (!ok) return
  form.value = null
  await reload()
}
</script>
