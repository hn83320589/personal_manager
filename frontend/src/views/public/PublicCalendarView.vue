<template>
  <PageHead eyebrow="Calendar" title="行事曆">公開的行程與活動。</PageHead>

  <div class="mt-8 flex items-center justify-between gap-4">
    <button type="button" class="btn btn-small" @click="shiftMonth(-1)">← 上個月</button>
    <h2 class="font-latin text-lg font-bold tabular-nums">{{ monthLabel }}</h2>
    <button type="button" class="btn btn-small" @click="shiftMonth(1)">下個月 →</button>
  </div>

  <PageState :loading="loading" :error="error" @retry="reload">
    <p v-if="!days.length" class="mt-8 text-muted">這個月沒有公開的行程。</p>
    <ol v-else class="mt-6 flex flex-col border-t border-rule">
      <li
        v-for="day in days"
        :key="day.key"
        class="grid gap-2 border-b border-rule py-4 min-[561px]:grid-cols-[7rem_1fr] min-[561px]:gap-6"
      >
        <span class="font-mono text-sm tabular-nums text-muted">{{ day.label }}</span>
        <ul class="flex flex-col gap-3">
          <li v-for="event in day.events" :key="`${event.eventId}-${event.start}`" class="flex gap-3">
            <i class="mt-2 h-2 w-2 shrink-0 rounded-full" :style="{ background: event.color || 'rgb(var(--accent))' }" />
            <div>
              <b class="font-medium">{{ event.title }}</b>
              <span class="ml-2 text-sm text-muted">{{ timeRange(event, ' – ') }}</span>
              <p v-if="event.description" class="text-sm text-muted">{{ event.description }}</p>
            </div>
          </li>
        </ul>
      </li>
    </ol>
  </PageState>
</template>

<script setup lang="ts">
import { computed, ref, watchEffect } from 'vue'
import { publicApi } from '@/api/public'
import type { Schemas } from '@/api/types'
import { useAsyncData } from '@/composables/useAsyncData'
import { setPageSeo } from '@/composables/useSeo'
import { timeRange, WEEKDAYS } from '@/lib/calendar'
import { pad } from '@/lib/format'
import PageHead from '@/components/public/PageHead.vue'
import PageState from '@/components/public/PageState.vue'
import { usePublicContext } from '@/components/public/publicContext'

type Occurrence = Schemas['OccurrenceDto']

const { username, profile } = usePublicContext()
const today = new Date()
const month = ref(new Date(today.getFullYear(), today.getMonth(), 1))

const monthLabel = computed(() => `${month.value.getFullYear()} 年 ${month.value.getMonth() + 1} 月`)

function shiftMonth(delta: number) {
  month.value = new Date(month.value.getFullYear(), month.value.getMonth() + delta, 1)
}

const {
  data: events,
  loading,
  error,
  reload,
} = useAsyncData(
  () => {
    const from = month.value
    const to = new Date(from.getFullYear(), from.getMonth() + 1, 1)
    return publicApi.calendar(username.value, { from: from.toISOString(), to: to.toISOString() })
  },
  { watch: [username, month] },
)


/** 依訪客當地日期分組；後端已展開重複行程並排序。 */
const days = computed(() => {
  const groups = new Map<string, { key: string; label: string; events: Occurrence[] }>()
  for (const event of events.value ?? []) {
    const start = new Date(event.start)
    const key = `${start.getFullYear()}-${start.getMonth()}-${start.getDate()}`
    const label = `${pad(start.getMonth() + 1)}/${pad(start.getDate())}（${WEEKDAYS[start.getDay()]}）`
    const group = groups.get(key) ?? { key, label, events: [] }
    group.events.push(event)
    groups.set(key, group)
  }
  return [...groups.values()]
})


watchEffect(() => setPageSeo({ title: `行事曆 · ${profile.value?.fullName || username.value}` }))
</script>
