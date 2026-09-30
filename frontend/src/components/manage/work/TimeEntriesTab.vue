<template>
  <div class="flex flex-col gap-3">
    <div class="flex flex-wrap items-center justify-between gap-2">
      <div class="flex items-center gap-2">
        <button type="button" class="btn btn-small" aria-label="上一週" @click="shiftWeek(-1)">
          ←
        </button>
        <span class="font-mono text-sm tabular-nums">{{ weekStart }} – {{ weekEnd }}</span>
        <button type="button" class="btn btn-small" aria-label="下一週" @click="shiftWeek(1)">
          →
        </button>
      </div>
      <button type="button" class="btn btn-small" @click="openNew">手動新增紀錄</button>
    </div>

    <PageState :loading="loading && !entries" :error="error" @retry="reload">
      <p
        v-if="!days.length"
        class="rounded-xl border border-rule bg-surface p-8 text-center text-muted"
      >
        這週沒有時間紀錄。
      </p>
      <section
        v-for="day in days"
        :key="day.date"
        class="flex flex-col gap-1.5"
        :aria-label="day.date"
      >
        <h3 class="flex justify-between text-sm text-muted">
          <span>{{ day.date }}</span>
          <span class="font-mono tabular-nums">{{ formatMinutes(day.total) }}</span>
        </h3>
        <ul class="flex flex-col overflow-hidden rounded-xl border border-rule bg-surface">
          <li v-for="entry in day.entries" :key="entry.id" class="border-rule [&+&]:border-t">
            <button
              type="button"
              class="flex w-full items-center gap-3 px-4 py-2 text-left hover:bg-soft"
              @click="openEdit(entry)"
            >
              <span class="min-w-0 flex-1">
                <span class="block truncate">{{ entry.workTaskTitle ?? entry.title }}</span>
                <span
                  v-if="entry.projectName || entry.description"
                  class="block truncate text-xs text-muted"
                >
                  {{ [entry.projectName, entry.description].filter(Boolean).join(' · ') }}
                </span>
              </span>
              <span class="font-mono text-xs tabular-nums text-muted">
                {{
                  entry.startTime
                    ? `${entry.startTime.slice(0, 5)}–${entry.endTime?.slice(0, 5)} · `
                    : ''
                }}{{ formatMinutes(entry.durationMinutes) }}
              </span>
            </button>
          </li>
        </ul>
      </section>
    </PageState>

    <SidePanel
      :open="form !== null"
      :title="editingId ? '編輯時間紀錄' : '新增時間紀錄'"
      :saving="running"
      @close="form = null"
      @submit="submit"
    >
      <template v-if="form">
        <FormField v-slot="{ id }" label="任務">
          <select :id="id" v-model="form.workTaskId" class="input">
            <option :value="null">不連結任務</option>
            <option v-for="task in tasks" :key="task.id" :value="task.id">{{ task.title }}</option>
          </select>
        </FormField>
        <FormField v-if="form.workTaskId === null" v-slot="{ id }" label="做了什麼" required>
          <input :id="id" v-model.trim="form.title" class="input" maxlength="200" required />
        </FormField>
        <FormField v-slot="{ id }" label="日期" required>
          <input :id="id" v-model="form.date" type="date" class="input" required />
        </FormField>
        <div class="grid grid-cols-3 gap-2">
          <FormField v-slot="{ id }" label="開始">
            <input :id="id" v-model="form.startTime" type="time" class="input" />
          </FormField>
          <FormField v-slot="{ id }" label="結束">
            <input :id="id" v-model="form.endTime" type="time" class="input" />
          </FormField>
          <FormField v-slot="{ id }" label="或時長（分）">
            <input
              :id="id"
              v-model.number="form.durationMinutes"
              type="number"
              min="1"
              max="1440"
              class="input"
              :disabled="!!(form.startTime && form.endTime)"
            />
          </FormField>
        </div>
        <FormField v-slot="{ id }" label="備註">
          <textarea :id="id" v-model="form.description" class="input min-h-20" maxlength="2000" />
        </FormField>
      </template>
      <template v-if="editingId" #footer-start>
        <DeleteButton item-name="這筆紀錄" @confirm="remove" />
      </template>
    </SidePanel>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { timeEntriesApi } from '@/api/tools'
import type { Schemas } from '@/api/types'
import { useAsyncAction } from '@/composables/useAsyncAction'
import { useAsyncData } from '@/composables/useAsyncData'
import { localDate } from '@/lib/format'
import { formatMinutes } from '@/lib/timer'
import PageState from '@/components/public/PageState.vue'
import DeleteButton from '../DeleteButton.vue'
import FormField from '../FormField.vue'
import SidePanel from '../SidePanel.vue'

type Entry = Schemas['TimeEntryDto']
interface EntryForm {
  workTaskId: number | null
  title: string
  date: string
  startTime: string
  endTime: string
  durationMinutes: number | ''
  description: string
}

defineProps<{ tasks: Schemas['WorkTaskDto'][] }>()
const emit = defineEmits<{ changed: [] }>()

/** 一週從週一開始。 */
function mondayOf(date: Date) {
  const monday = new Date(
    date.getFullYear(),
    date.getMonth(),
    date.getDate() - ((date.getDay() + 6) % 7),
  )
  return monday
}
const monday = ref(mondayOf(new Date()))
const weekStart = computed(() => localDate(monday.value))
const weekEnd = computed(() => {
  const sunday = new Date(monday.value)
  sunday.setDate(sunday.getDate() + 6)
  return localDate(sunday)
})

function shiftWeek(delta: number) {
  const next = new Date(monday.value)
  next.setDate(next.getDate() + delta * 7)
  monday.value = next
}

const {
  data: entries,
  loading,
  error,
  reload,
} = useAsyncData(() => timeEntriesApi.list({ from: weekStart.value, to: weekEnd.value }), {
  watch: [monday],
})

const days = computed(() => {
  const groups = new Map<string, Entry[]>()
  for (const entry of entries.value ?? [])
    groups.set(entry.date, [...(groups.get(entry.date) ?? []), entry])
  return [...groups]
    .sort(([a], [b]) => b.localeCompare(a))
    .map(([date, list]) => ({
      date,
      entries: list,
      total: list.reduce((sum, e) => sum + e.durationMinutes, 0),
    }))
})

const form = ref<EntryForm | null>(null)
const editingId = ref<number | null>(null)
const { run, running } = useAsyncAction()

function openNew() {
  editingId.value = null
  form.value = {
    workTaskId: null,
    title: '',
    date: localDate(),
    startTime: '',
    endTime: '',
    durationMinutes: 60,
    description: '',
  }
}

function openEdit(entry: Entry) {
  editingId.value = entry.id
  form.value = {
    workTaskId: entry.workTaskId ?? null,
    title: entry.title,
    date: entry.date,
    startTime: entry.startTime?.slice(0, 5) ?? '',
    endTime: entry.endTime?.slice(0, 5) ?? '',
    durationMinutes: entry.durationMinutes,
    description: entry.description,
  }
}

async function submit() {
  const f = form.value
  if (!f) return
  const timed = !!(f.startTime && f.endTime)
  const body: Schemas['SaveTimeEntryRequest'] = {
    workTaskId: f.workTaskId,
    title: f.workTaskId ? null : f.title,
    date: f.date,
    startTime: timed ? `${f.startTime}:00` : null,
    endTime: timed ? `${f.endTime}:00` : null,
    durationMinutes: timed || f.durationMinutes === '' ? null : f.durationMinutes,
    description: f.description,
  }
  const saved = await run(
    () =>
      editingId.value ? timeEntriesApi.update(editingId.value, body) : timeEntriesApi.create(body),
    { success: '已儲存時間紀錄' },
  )
  if (!saved) return
  form.value = null
  await reload()
  emit('changed')
}

async function remove() {
  const id = editingId.value
  if (id === null) return
  if (await run(async () => (await timeEntriesApi.remove(id), true), { success: '已刪除紀錄' })) {
    form.value = null
    await reload()
    emit('changed')
  }
}

defineExpose({ reload })
</script>
