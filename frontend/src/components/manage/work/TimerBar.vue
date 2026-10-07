<template>
  <section
    class="flex flex-wrap items-center gap-3 rounded-xl border border-rule bg-surface p-3"
    aria-label="計時器"
  >
    <template v-if="timer">
      <span class="h-2.5 w-2.5 animate-pulse rounded-full bg-danger" aria-hidden="true" />
      <span class="min-w-0 flex-1 truncate">
        <b class="font-medium">{{ timerLabel }}</b>
        <span class="ml-2 font-mono tabular-nums text-muted">{{ elapsed }}</span>
      </span>
      <button type="button" class="btn btn-primary btn-small" :disabled="running" @click="stop">
        停止並記錄
      </button>
      <button type="button" class="btn btn-small" @click="discard">捨棄</button>
    </template>
    <form v-else class="flex w-full flex-wrap items-center gap-2" @submit.prevent="start">
      <label for="timer-task" class="sr-only">要計時的任務</label>
      <select id="timer-task" v-model="taskId" class="input w-auto max-w-72 py-1.5 text-sm">
        <option :value="null">不連結任務</option>
        <option v-for="task in openTasks" :key="task.id" :value="task.id">{{ task.title }}</option>
      </select>
      <label v-if="taskId === null" for="timer-title" class="sr-only">正在做什麼</label>
      <input
        v-if="taskId === null"
        id="timer-title"
        v-model.trim="title"
        class="input min-w-40 flex-1 py-1.5 text-sm"
        placeholder="正在做什麼？"
        maxlength="200"
      />
      <button type="submit" class="btn btn-primary btn-small" :disabled="taskId === null && !title">
        開始計時
      </button>
    </form>
  </section>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { timeEntriesApi } from '@/api/tools'
import type { Schemas } from '@/api/types'
import { useAsyncAction } from '@/composables/useAsyncAction'
import { pad } from '@/lib/format'
import { timerToEntry, type RunningTimer } from '@/lib/timer'

const props = defineProps<{ tasks: Schemas['WorkTaskDto'][] }>()
const emit = defineEmits<{ saved: [] }>()

/** 計時中的狀態存在這個瀏覽器，重新整理或關閉分頁後仍會繼續計時。 */
const storageKey = 'pm-running-timer'

function readTimer(): RunningTimer | null {
  try {
    const raw = localStorage.getItem(storageKey)
    return raw ? (JSON.parse(raw) as RunningTimer) : null
  } catch {
    return null
  }
}

function writeTimer(value: RunningTimer | null) {
  timer.value = value
  try {
    if (value) localStorage.setItem(storageKey, JSON.stringify(value))
    else localStorage.removeItem(storageKey)
  } catch {
    // 無法使用 localStorage 時只在這個頁面計時
  }
}

const timer = ref<RunningTimer | null>(readTimer())
const taskId = ref<number | null>(null)
const title = ref('')
const now = ref(Date.now())
let tick: ReturnType<typeof setInterval> | undefined
onMounted(() => (tick = setInterval(() => (now.value = Date.now()), 1000)))
onUnmounted(() => clearInterval(tick))

const openTasks = computed(() =>
  props.tasks.filter((t) => t.status !== 'Completed' && t.status !== 'Cancelled'),
)

const timerLabel = computed(() => {
  if (!timer.value) return ''
  return props.tasks.find((t) => t.id === timer.value!.workTaskId)?.title ?? timer.value.title
})

const elapsed = computed(() => {
  if (!timer.value) return ''
  const seconds = Math.max(
    0,
    Math.floor((now.value - new Date(timer.value.startedAt).getTime()) / 1000),
  )
  return `${pad(Math.floor(seconds / 3600))}:${pad(Math.floor(seconds / 60) % 60)}:${pad(seconds % 60)}`
})

function start() {
  writeTimer({ workTaskId: taskId.value, title: title.value, startedAt: new Date().toISOString() })
  title.value = ''
}

const { run, running } = useAsyncAction()

async function stop() {
  if (!timer.value) return
  const entry = timerToEntry(timer.value, new Date())
  const saved = await run(() => timeEntriesApi.create(entry), { success: '已記錄時間' })
  if (!saved) return
  writeTimer(null)
  emit('saved')
}

function discard() {
  writeTimer(null)
}
</script>
