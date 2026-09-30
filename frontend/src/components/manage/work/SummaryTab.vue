<template>
  <div class="flex flex-col gap-4">
    <SegmentedControl v-model="range" :options="ranges" label="統計期間" class="max-w-xs" />

    <PageState :loading="loading && !summary" :error="error" @retry="reload">
      <template v-if="summary">
        <p class="text-sm text-muted">
          合計
          <b class="font-latin text-2xl tabular-nums text-ink">{{
            formatMinutes(summary.totalMinutes)
          }}</b>
        </p>

        <section class="flex flex-col gap-2" aria-labelledby="by-project">
          <h3 id="by-project" class="text-sm font-medium">依專案</h3>
          <p v-if="!summary.byProject.length" class="text-sm text-muted">沒有紀錄。</p>
          <div
            v-for="row in summary.byProject"
            :key="row.projectId ?? 'none'"
            class="grid grid-cols-[8rem_1fr_auto] items-center gap-3 text-sm"
          >
            <span class="truncate">{{ row.projectName ?? '未分類' }}</span>
            <span class="h-2.5 overflow-hidden rounded-full bg-soft">
              <span
                class="block h-full rounded-full bg-accent"
                :style="{ width: `${percent(row.minutes)}%` }"
              />
            </span>
            <span class="font-mono text-xs tabular-nums text-muted">{{
              formatMinutes(row.minutes)
            }}</span>
          </div>
        </section>

        <section class="flex flex-col gap-2" aria-labelledby="by-day">
          <h3 id="by-day" class="text-sm font-medium">每日</h3>
          <div class="flex h-32 items-end gap-1" role="list">
            <div
              v-for="day in summary.byDay"
              :key="day.date"
              role="listitem"
              class="flex flex-1 flex-col items-center justify-end gap-1"
              :title="`${day.date}：${formatMinutes(day.minutes)}`"
            >
              <span
                class="w-full rounded-t bg-accent/70"
                :style="{ height: `${dayHeight(day.minutes)}%` }"
              />
              <span class="font-mono text-[0.6rem] text-muted">{{ day.date.slice(8) }}</span>
            </div>
          </div>
        </section>
      </template>
    </PageState>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { timeEntriesApi } from '@/api/tools'
import { useAsyncData } from '@/composables/useAsyncData'
import { localDate } from '@/lib/format'
import { formatMinutes } from '@/lib/timer'
import PageState from '@/components/public/PageState.vue'
import SegmentedControl from '../SegmentedControl.vue'

type Range = 'week' | 'month'
const ranges: { value: Range; label: string }[] = [
  { value: 'week', label: '最近 7 天' },
  { value: 'month', label: '最近 30 天' },
]
const range = ref<Range>('week')

const period = computed(() => {
  const to = new Date()
  const from = new Date(
    to.getFullYear(),
    to.getMonth(),
    to.getDate() - (range.value === 'week' ? 6 : 29),
  )
  return { from: localDate(from), to: localDate(to) }
})

const {
  data: summary,
  loading,
  error,
  reload,
} = useAsyncData(() => timeEntriesApi.summary(period.value), { watch: [range] })

const percent = (minutes: number) =>
  summary.value?.totalMinutes ? Math.round((minutes / summary.value.totalMinutes) * 100) : 0

const dayMax = computed(() => Math.max(1, ...(summary.value?.byDay ?? []).map((d) => d.minutes)))
const dayHeight = (minutes: number) => Math.max(2, Math.round((minutes / dayMax.value) * 100))

defineExpose({ reload })
</script>
