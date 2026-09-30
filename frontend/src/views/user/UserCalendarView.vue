<template>
  <div>
    <LoadingSpinner v-if="isLoading" size="medium" text="載入中..." />

    <div v-else-if="events.length > 0" class="space-y-3">
      <div
        v-for="event in events"
        :key="event.id"
        class="bg-white rounded-xl border border-gray-200 p-4 shadow-sm flex items-start gap-4"
      >
        <div
          class="flex-shrink-0 w-12 h-12 rounded-lg flex flex-col items-center justify-center text-white text-xs font-bold"
          :style="{ backgroundColor: event.color || 'var(--color-primary, #0ea5e9)' }"
        >
          <span class="text-base leading-none">{{ new Date(event.startTime).getDate() }}</span>
          <span class="leading-none">{{ monthShort(event.startTime) }}</span>
        </div>
        <div class="flex-1">
          <h3 class="font-medium text-gray-900">{{ event.title }}</h3>
          <p class="text-sm text-gray-500 mt-0.5">
            {{ formatDateTime(event.startTime) }}
            <span v-if="!event.isAllDay"> – {{ formatDateTime(event.endTime) }}</span>
          </p>
          <p v-if="event.description" class="text-sm text-gray-600 mt-1">{{ event.description }}</p>
        </div>
      </div>
    </div>

    <div v-else-if="!isLoading" class="text-center text-gray-400 py-12">尚無公開行程</div>
  </div>
</template>

<script setup lang="ts">
import { ref, inject, onMounted, watch } from 'vue'
import type { ComputedRef } from 'vue'
import httpService from '@/services/http'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import type { CalendarEvent } from '@/types/api'

const userId = inject<ComputedRef<number | null>>('userId')

const isLoading = ref(true)
const events = ref<CalendarEvent[]>([])

const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']

function monthShort(dateStr: string): string {
  return monthNames[new Date(dateStr).getMonth()]
}

function formatDateTime(dateStr: string): string {
  const d = new Date(dateStr)
  return d.toLocaleString('zh-TW', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' })
}

async function load(uid: number) {
  isLoading.value = true
  try {
    const res = await httpService.get<CalendarEvent[]>(`/calendarevents/user/${uid}/public`)
    if (res.success) events.value = res.data || []
  } finally {
    isLoading.value = false
  }
}

onMounted(() => { if (userId?.value) load(userId.value) })
watch(() => userId?.value, (uid) => { if (uid) load(uid) })
</script>
