<template>
  <div class="p-6">
    <!-- Date Range Header -->
    <div class="flex justify-between items-center mb-6">
      <div>
        <h3 class="text-lg font-medium text-gray-900">時間表</h3>
        <p class="text-sm text-gray-500">
          {{ formatDateRange(dateRange.start, dateRange.end) }}
        </p>
      </div>
      <div class="flex items-center space-x-2">
        <BaseButton
          variant="outline"
          size="small"
          @click="exportTimesheet"
        >
          <DocumentArrowDownIcon class="w-4 h-4 mr-2" />
          匯出
        </BaseButton>
        <BaseButton
          variant="primary"
          size="small"
          @click="addTimeEntry"
        >
          <PlusIcon class="w-4 h-4 mr-2" />
          新增記錄
        </BaseButton>
      </div>
    </div>

    <div v-if="loading" class="text-center py-8">
      <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      <p class="mt-2 text-sm text-gray-500">載入中...</p>
    </div>
    
    <div v-else-if="timeEntries.length === 0" class="text-center py-8">
      <ClockIcon class="mx-auto h-12 w-12 text-gray-400" />
      <h3 class="mt-2 text-sm font-medium text-gray-900">沒有時間記錄</h3>
      <p class="mt-1 text-sm text-gray-500">在此期間內沒有工作記錄。</p>
    </div>

    <div v-else>
      <!-- Summary Cards -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
        <div class="bg-white border border-gray-200 rounded-lg p-4">
          <div class="flex items-center">
            <ClockIcon class="w-8 h-8 text-blue-600" />
            <div class="ml-3">
              <p class="text-sm text-gray-500">總工作時間</p>
              <p class="text-xl font-semibold text-gray-900">{{ totalHours.toFixed(1) }}h</p>
            </div>
          </div>
        </div>
        
        <div class="bg-white border border-gray-200 rounded-lg p-4">
          <div class="flex items-center">
            <CalendarDaysIcon class="w-8 h-8 text-green-600" />
            <div class="ml-3">
              <p class="text-sm text-gray-500">工作天數</p>
              <p class="text-xl font-semibold text-gray-900">{{ workingDays }}</p>
            </div>
          </div>
        </div>
        
        <div class="bg-white border border-gray-200 rounded-lg p-4">
          <div class="flex items-center">
            <ChartBarIcon class="w-8 h-8 text-purple-600" />
            <div class="ml-3">
              <p class="text-sm text-gray-500">日均時數</p>
              <p class="text-xl font-semibold text-gray-900">{{ averageHoursPerDay.toFixed(1) }}h</p>
            </div>
          </div>
        </div>
        
        <div class="bg-white border border-gray-200 rounded-lg p-4">
          <div class="flex items-center">
            <CurrencyDollarIcon class="w-8 h-8 text-yellow-600" />
            <div class="ml-3">
              <p class="text-sm text-gray-500">預估收入</p>
              <p class="text-xl font-semibold text-gray-900">${{ estimatedEarnings }}</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Timesheet Table -->
      <div class="bg-white shadow rounded-lg overflow-hidden">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                日期
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                任務/專案
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                時間
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                時長
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                描述
              </th>
              <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                操作
              </th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <template v-for="(entries, date) in groupedEntries" :key="date">
              <!-- Date Header Row -->
              <tr class="bg-gray-50">
                <td colspan="6" class="px-6 py-3">
                  <div class="flex items-center justify-between">
                    <div class="flex items-center space-x-3">
                      <span class="text-sm font-medium text-gray-900">
                        {{ formatDisplayDate(date) }}
                      </span>
                      <span class="text-xs text-gray-500">
                        ({{ getWeekday(date) }})
                      </span>
                    </div>
                    <div class="text-sm font-medium text-blue-600">
                      總計: {{ getDailyTotal(entries).toFixed(1) }}h
                    </div>
                  </div>
                </td>
              </tr>
              
              <!-- Time Entries for this date -->
              <tr v-for="entry in entries" :key="entry.id" class="hover:bg-gray-50">
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  <!-- Empty for grouped display -->
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <div>
                    <div class="text-sm font-medium text-gray-900">{{ entry.task }}</div>
                    <div v-if="entry.project" class="text-sm text-gray-500">{{ entry.project }}</div>
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  <div v-if="entry.startTime && entry.endTime">
                    {{ entry.startTime }} - {{ entry.endTime }}
                  </div>
                  <div v-else class="text-gray-400">--:-- - --:--</div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span class="inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-blue-100 text-blue-800">
                    {{ formatDuration(entry.duration) }}
                  </span>
                </td>
                <td class="px-6 py-4">
                  <div class="text-sm text-gray-900 max-w-xs truncate" :title="entry.description">
                    {{ entry.description || '無描述' }}
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                  <div class="flex justify-end space-x-2">
                    <BaseButton
                      variant="outline"
                      size="small"
                      @click="$emit('edit-entry', entry)"
                    >
                      <PencilIcon class="w-4 h-4" />
                    </BaseButton>
                    <BaseButton
                      variant="outline"
                      size="small"
                      @click="$emit('delete-entry', entry.id)"
                      class="text-red-600 hover:text-red-900"
                    >
                      <TrashIcon class="w-4 h-4" />
                    </BaseButton>
                  </div>
                </td>
              </tr>
            </template>
          </tbody>
        </table>
      </div>

      <!-- Weekly View Toggle -->
      <div class="mt-6 flex justify-center">
        <div class="bg-gray-100 p-1 rounded-lg">
          <button
            @click="viewMode = 'daily'"
            :class="[
              'px-3 py-2 text-sm font-medium rounded-md transition-colors',
              viewMode === 'daily'
                ? 'bg-white text-gray-900 shadow'
                : 'text-gray-600 hover:text-gray-900'
            ]"
          >
            每日檢視
          </button>
          <button
            @click="viewMode = 'weekly'"
            :class="[
              'px-3 py-2 text-sm font-medium rounded-md transition-colors',
              viewMode === 'weekly'
                ? 'bg-white text-gray-900 shadow'
                : 'text-gray-600 hover:text-gray-900'
            ]"
          >
            週檢視
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import {
  PlusIcon,
  ClockIcon,
  CalendarDaysIcon,
  ChartBarIcon,
  CurrencyDollarIcon,
  DocumentArrowDownIcon,
  PencilIcon,
  TrashIcon
} from '@heroicons/vue/24/outline'
import BaseButton from '@/components/ui/BaseButton.vue'

// Time Entry Interface
interface TimeEntry {
  id: number
  taskId?: number
  task: string
  project?: string
  date: string
  startTime?: string
  endTime?: string
  duration: number
  description?: string
}

// Props
interface Props {
  timeEntries: TimeEntry[]
  dateRange: {
    start: string
    end: string
  }
  loading: boolean
}

const props = defineProps<Props>()

// Emits
const emit = defineEmits<{
  'edit-entry': [entry: TimeEntry]
  'delete-entry': [id: number]
  'add-entry': []
}>()

// State
const viewMode = ref<'daily' | 'weekly'>('daily')
const hourlyRate = 50 // Example hourly rate

// Computed
const groupedEntries = computed(() => {
  const grouped: Record<string, TimeEntry[]> = {}
  
  props.timeEntries.forEach(entry => {
    if (!grouped[entry.date]) {
      grouped[entry.date] = []
    }
    grouped[entry.date].push(entry)
  })
  
  // Sort entries within each date
  Object.keys(grouped).forEach(date => {
    grouped[date].sort((a, b) => {
      const timeA = a.startTime || '00:00'
      const timeB = b.startTime || '00:00'
      return timeA.localeCompare(timeB)
    })
  })
  
  return grouped
})

const totalHours = computed(() => {
  return props.timeEntries.reduce((sum, entry) => sum + (entry.duration / 60), 0)
})

const workingDays = computed(() => {
  return Object.keys(groupedEntries.value).length
})

const averageHoursPerDay = computed(() => {
  return workingDays.value > 0 ? totalHours.value / workingDays.value : 0
})

const estimatedEarnings = computed(() => {
  return (totalHours.value * hourlyRate).toFixed(0)
})

// Methods
function formatDateRange(start: string, end: string): string {
  const startDate = new Date(start)
  const endDate = new Date(end)
  
  if (start === end) {
    return startDate.toLocaleDateString('zh-TW', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    })
  }
  
  return `${startDate.toLocaleDateString('zh-TW', {
    month: 'short',
    day: 'numeric'
  })} - ${endDate.toLocaleDateString('zh-TW', {
    month: 'short',
    day: 'numeric'
  })}`
}

function formatDisplayDate(date: string): string {
  return new Date(date).toLocaleDateString('zh-TW', {
    month: 'short',
    day: 'numeric'
  })
}

function getWeekday(date: string): string {
  return new Date(date).toLocaleDateString('zh-TW', {
    weekday: 'short'
  })
}

function formatDuration(minutes: number): string {
  const hours = Math.floor(minutes / 60)
  const mins = minutes % 60
  
  if (hours === 0) {
    return `${mins}m`
  } else if (mins === 0) {
    return `${hours}h`
  } else {
    return `${hours}h${mins}m`
  }
}

function getDailyTotal(entries: TimeEntry[]): number {
  return entries.reduce((sum, entry) => sum + (entry.duration / 60), 0)
}

function addTimeEntry() {
  emit('add-entry')
}

function exportTimesheet() {
  // This would implement CSV export functionality
  const csv = generateCSV()
  downloadCSV(csv, `timesheet-${props.dateRange.start}-${props.dateRange.end}.csv`)
}

function generateCSV(): string {
  const headers = ['日期', '任務', '專案', '開始時間', '結束時間', '時長(分鐘)', '描述']
  const rows = [headers.join(',')]
  
  props.timeEntries.forEach(entry => {
    const row = [
      entry.date,
      `"${entry.task}"`,
      `"${entry.project || ''}"`,
      entry.startTime || '',
      entry.endTime || '',
      entry.duration.toString(),
      `"${entry.description || ''}"`
    ]
    rows.push(row.join(','))
  })
  
  return rows.join('\n')
}

function downloadCSV(csv: string, filename: string) {
  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' })
  const link = document.createElement('a')
  
  if (link.download !== undefined) {
    const url = URL.createObjectURL(blob)
    link.setAttribute('href', url)
    link.setAttribute('download', filename)
    link.style.visibility = 'hidden'
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
  }
}
</script>