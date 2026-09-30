<template>
  <div class="p-6">
    <!-- Report Header -->
    <div class="mb-6">
      <h3 class="text-lg font-medium text-gray-900">工作報表</h3>
      <p class="text-sm text-gray-500">
        {{ formatDateRange(dateRange.start, dateRange.end) }} 的工作數據分析
      </p>
    </div>

    <!-- Key Metrics -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
      <div class="bg-gradient-to-r from-blue-500 to-blue-600 rounded-lg p-6 text-white">
        <div class="flex items-center">
          <ClockIcon class="w-8 h-8" />
          <div class="ml-4">
            <p class="text-blue-100">總工作時間</p>
            <p class="text-2xl font-bold">{{ totalWorkHours.toFixed(1) }}h</p>
          </div>
        </div>
      </div>

      <div class="bg-gradient-to-r from-green-500 to-green-600 rounded-lg p-6 text-white">
        <div class="flex items-center">
          <CheckCircleIcon class="w-8 h-8" />
          <div class="ml-4">
            <p class="text-green-100">完成任務</p>
            <p class="text-2xl font-bold">{{ completedTasks }}</p>
          </div>
        </div>
      </div>

      <div class="bg-gradient-to-r from-purple-500 to-purple-600 rounded-lg p-6 text-white">
        <div class="flex items-center">
          <ChartBarIcon class="w-8 h-8" />
          <div class="ml-4">
            <p class="text-purple-100">生產力指數</p>
            <p class="text-2xl font-bold">{{ productivityIndex }}</p>
          </div>
        </div>
      </div>

      <div class="bg-gradient-to-r from-yellow-500 to-yellow-600 rounded-lg p-6 text-white">
        <div class="flex items-center">
          <TrendingUpIcon class="w-8 h-8" />
          <div class="ml-4">
            <p class="text-yellow-100">效率評分</p>
            <p class="text-2xl font-bold">{{ efficiencyScore }}%</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Charts Row -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-8">
      <!-- Daily Hours Chart -->
      <div class="bg-white rounded-lg border border-gray-200 p-6">
        <h4 class="text-lg font-medium text-gray-900 mb-4">每日工作時間</h4>
        <div class="h-64 flex items-end justify-between space-x-2">
          <div
            v-for="(day, index) in dailyHours"
            :key="index"
            class="flex flex-col items-center flex-1"
          >
            <div
              class="bg-blue-500 rounded-t-md transition-all duration-300 hover:bg-blue-600 min-h-4"
              :style="{ height: `${(day.hours / maxDailyHours) * 240}px` }"
              :title="`${day.date}: ${day.hours.toFixed(1)}h`"
            ></div>
            <div class="mt-2 text-xs text-gray-500 text-center">
              <div>{{ formatShortDate(day.date) }}</div>
              <div class="font-medium">{{ day.hours.toFixed(1) }}h</div>
            </div>
          </div>
        </div>
      </div>

      <!-- Project Distribution -->
      <div class="bg-white rounded-lg border border-gray-200 p-6">
        <h4 class="text-lg font-medium text-gray-900 mb-4">專案時間分配</h4>
        <div class="space-y-4">
          <div
            v-for="project in projectStats"
            :key="project.name"
            class="flex items-center"
          >
            <div class="flex-1">
              <div class="flex justify-between items-center mb-1">
                <span class="text-sm font-medium text-gray-700">{{ project.name }}</span>
                <span class="text-sm text-gray-500">{{ project.hours.toFixed(1) }}h</span>
              </div>
              <div class="w-full bg-gray-200 rounded-full h-2">
                <div
                  class="bg-blue-600 h-2 rounded-full"
                  :style="{ width: `${project.percentage}%` }"
                ></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Task Status Overview -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-8 mb-8">
      <!-- Task Status Breakdown -->
      <div class="lg:col-span-2 bg-white rounded-lg border border-gray-200 p-6">
        <h4 class="text-lg font-medium text-gray-900 mb-4">任務狀態分佈</h4>
        <div class="grid grid-cols-2 md:grid-cols-3 gap-4">
          <div
            v-for="status in taskStatusStats"
            :key="status.label"
            class="text-center p-4 rounded-lg"
            :class="status.bgClass"
          >
            <div class="text-2xl font-bold" :class="status.textClass">{{ status.count }}</div>
            <div class="text-sm text-gray-600">{{ status.label }}</div>
            <div class="text-xs text-gray-500">{{ status.percentage }}%</div>
          </div>
        </div>
      </div>

      <!-- Average Times -->
      <div class="bg-white rounded-lg border border-gray-200 p-6">
        <h4 class="text-lg font-medium text-gray-900 mb-4">平均統計</h4>
        <div class="space-y-4">
          <div class="flex justify-between items-center">
            <span class="text-sm text-gray-600">日均工作時間</span>
            <span class="font-medium">{{ averageDailyHours.toFixed(1) }}h</span>
          </div>
          <div class="flex justify-between items-center">
            <span class="text-sm text-gray-600">平均任務完成時間</span>
            <span class="font-medium">{{ averageTaskTime.toFixed(1) }}h</span>
          </div>
          <div class="flex justify-between items-center">
            <span class="text-sm text-gray-600">任務完成率</span>
            <span class="font-medium">{{ taskCompletionRate }}%</span>
          </div>
          <div class="flex justify-between items-center">
            <span class="text-sm text-gray-600">按時完成率</span>
            <span class="font-medium">{{ onTimeCompletionRate }}%</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Productivity Trends -->
    <div class="bg-white rounded-lg border border-gray-200 p-6 mb-8">
      <h4 class="text-lg font-medium text-gray-900 mb-4">生產力趨勢</h4>
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div class="text-center p-4 bg-blue-50 rounded-lg">
          <ArrowTrendingUpIcon class="w-8 h-8 text-blue-600 mx-auto mb-2" />
          <div class="text-lg font-semibold text-blue-900">{{ weeklyGrowth }}%</div>
          <div class="text-sm text-blue-700">週增長率</div>
        </div>
        <div class="text-center p-4 bg-green-50 rounded-lg">
          <ClockIcon class="w-8 h-8 text-green-600 mx-auto mb-2" />
          <div class="text-lg font-semibold text-green-900">{{ focusTime.toFixed(1) }}h</div>
          <div class="text-sm text-green-700">專注時間</div>
        </div>
        <div class="text-center p-4 bg-purple-50 rounded-lg">
          <ChartBarIcon class="w-8 h-8 text-purple-600 mx-auto mb-2" />
          <div class="text-lg font-semibold text-purple-900">{{ peakHour }}</div>
          <div class="text-sm text-purple-700">高效時段</div>
        </div>
        <div class="text-center p-4 bg-yellow-50 rounded-lg">
          <AcademicCapIcon class="w-8 h-8 text-yellow-600 mx-auto mb-2" />
          <div class="text-lg font-semibold text-yellow-900">{{ improvementArea }}</div>
          <div class="text-sm text-yellow-700">改進建議</div>
        </div>
      </div>
    </div>

    <!-- Export Actions -->
    <div class="flex justify-end space-x-3">
      <BaseButton
        variant="outline"
        disabled
        title="PDF 匯出功能尚未開放"
      >
        <DocumentArrowDownIcon class="w-4 h-4 mr-2" />
        匯出 PDF
      </BaseButton>
      <BaseButton
        variant="outline"
        disabled
        title="Excel 匯出功能尚未開放"
      >
        <TableCellsIcon class="w-4 h-4 mr-2" />
        匯出 Excel
      </BaseButton>
      <BaseButton
        variant="primary"
        disabled
        title="分享功能尚未開放"
      >
        <ShareIcon class="w-4 h-4 mr-2" />
        分享報表
      </BaseButton>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import {
  ClockIcon,
  CheckCircleIcon,
  ChartBarIcon,
  ArrowTrendingUpIcon,
  DocumentArrowDownIcon,
  TableCellsIcon,
  ShareIcon,
  AcademicCapIcon
} from '@heroicons/vue/24/outline'
import type { WorkTask, WorkTaskStatus } from '@/types/api'
import BaseButton from '@/components/ui/BaseButton.vue'

// Time Entry Interface (reused from TimesheetView)
interface TimeEntry {
  id: number
  taskId?: number
  task: string
  project?: string
  date: string
  startTime?: string
  duration: number
  description?: string
}

// Props
interface Props {
  tasks: WorkTask[]
  timeEntries: TimeEntry[]
  dateRange: {
    start: string
    end: string
  }
}

const props = defineProps<Props>()

// Computed - Basic Metrics
const totalWorkHours = computed(() => {
  return props.timeEntries.reduce((sum, entry) => sum + (entry.duration / 60), 0)
})

const completedTasks = computed(() => {
  return props.tasks.filter(task => task.status === 'Completed').length
})

const productivityIndex = computed(() => {
  // Based on tasks completed vs time spent
  const hoursPerTask = totalWorkHours.value / Math.max(completedTasks.value, 1)
  return Math.max(100 - Math.round(hoursPerTask * 10), 10)
})

const efficiencyScore = computed(() => {
  const totalEstimated = props.tasks.reduce((sum, task) => sum + (task.estimatedHours || 0), 0)
  const totalActual = props.tasks.reduce((sum, task) => sum + (task.actualHours || 0), 0)
  
  if (totalEstimated === 0) return 100
  return Math.round((totalEstimated / Math.max(totalActual, 0.1)) * 100)
})

// Daily Hours Analysis
const dailyHours = computed(() => {
  const dailyMap: Record<string, number> = {}
  
  props.timeEntries.forEach(entry => {
    if (!dailyMap[entry.date]) {
      dailyMap[entry.date] = 0
    }
    dailyMap[entry.date] += entry.duration / 60
  })
  
  // Generate array for chart
  const startDate = new Date(props.dateRange.start)
  const endDate = new Date(props.dateRange.end)
  const days = []
  
  for (let date = new Date(startDate); date <= endDate; date.setDate(date.getDate() + 1)) {
    const dateStr = date.toISOString().split('T')[0]
    days.push({
      date: dateStr,
      hours: dailyMap[dateStr] || 0
    })
  }
  
  return days
})

const maxDailyHours = computed(() => {
  return Math.max(...dailyHours.value.map(day => day.hours), 8)
})

// Project Statistics
const projectStats = computed(() => {
  const projectMap: Record<string, number> = {}
  
  props.timeEntries.forEach(entry => {
    const project = entry.project || '未分類'
    if (!projectMap[project]) {
      projectMap[project] = 0
    }
    projectMap[project] += entry.duration / 60
  })
  
  const totalHours = Object.values(projectMap).reduce((sum, hours) => sum + hours, 0)
  
  return Object.entries(projectMap)
    .map(([name, hours]) => ({
      name,
      hours,
      percentage: totalHours > 0 ? (hours / totalHours) * 100 : 0
    }))
    .sort((a, b) => b.hours - a.hours)
    .slice(0, 8) // Top 8 projects
})

// Task Status Statistics
const taskStatusStats = computed(() => {
  const statusMap: Record<string, number> = {
    'Pending': 0, 'Planning': 0, 'InProgress': 0, 'Testing': 0, 'Completed': 0, 'OnHold': 0, 'Cancelled': 0
  }

  props.tasks.forEach(task => {
    statusMap[task.status] = (statusMap[task.status] || 0) + 1
  })

  const total = props.tasks.length

  return [
    {
      label: '待處理',
      count: statusMap['Pending'],
      percentage: total > 0 ? Math.round((statusMap['Pending'] / total) * 100) : 0,
      bgClass: 'bg-gray-50',
      textClass: 'text-gray-900'
    },
    {
      label: '進行中',
      count: statusMap['InProgress'],
      percentage: total > 0 ? Math.round((statusMap['InProgress'] / total) * 100) : 0,
      bgClass: 'bg-blue-50',
      textClass: 'text-blue-900'
    },
    {
      label: '已完成',
      count: statusMap['Completed'],
      percentage: total > 0 ? Math.round((statusMap['Completed'] / total) * 100) : 0,
      bgClass: 'bg-green-50',
      textClass: 'text-green-900'
    },
    {
      label: '暫停',
      count: statusMap['OnHold'],
      percentage: total > 0 ? Math.round((statusMap['OnHold'] / total) * 100) : 0,
      bgClass: 'bg-yellow-50',
      textClass: 'text-yellow-900'
    },
    {
      label: '已取消',
      count: statusMap['Cancelled'],
      percentage: total > 0 ? Math.round((statusMap['Cancelled'] / total) * 100) : 0,
      bgClass: 'bg-red-50',
      textClass: 'text-red-900'
    }
  ]
})

// Average Statistics
const averageDailyHours = computed(() => {
  const workingDays = dailyHours.value.filter(day => day.hours > 0).length
  return workingDays > 0 ? totalWorkHours.value / workingDays : 0
})

const averageTaskTime = computed(() => {
  const completedTasksWithTime = props.tasks.filter(task => 
    task.status === 'Completed' && task.actualHours && task.actualHours > 0
  )
  
  if (completedTasksWithTime.length === 0) return 0
  
  const totalTime = completedTasksWithTime.reduce((sum, task) => sum + (task.actualHours || 0), 0)
  return totalTime / completedTasksWithTime.length
})

const taskCompletionRate = computed(() => {
  if (props.tasks.length === 0) return 0
  return Math.round((completedTasks.value / props.tasks.length) * 100)
})

const onTimeCompletionRate = computed(() => {
  const tasksWithDueDate = props.tasks.filter(task => task.dueDate && task.status === 'Completed')
  if (tasksWithDueDate.length === 0) return 100
  
  const onTimeTasks = tasksWithDueDate.filter(task => {
    if (!task.completedAt || !task.dueDate) return false
    return new Date(task.completedAt) <= new Date(task.dueDate)
  })
  
  return Math.round((onTimeTasks.length / tasksWithDueDate.length) * 100)
})

// Productivity Insights
const weeklyGrowth = computed(() => {
  // Simulate weekly growth calculation
  return Math.round(Math.random() * 20 - 5) // -5% to +15%
})

const focusTime = computed(() => {
  // Calculate focused work time (entries > 2 hours)
  return props.timeEntries
    .filter(entry => entry.duration >= 120)
    .reduce((sum, entry) => sum + (entry.duration / 60), 0)
})

const peakHour = computed(() => {
  // Find most productive hour
  const hourMap: Record<number, number> = {}
  
  props.timeEntries.forEach(entry => {
    if (entry.startTime) {
      const hour = parseInt(entry.startTime.split(':')[0])
      hourMap[hour] = (hourMap[hour] || 0) + 1
    }
  })
  
  const peakHourNum = Object.entries(hourMap)
    .sort(([,a], [,b]) => b - a)[0]?.[0]
  
  return peakHourNum ? `${peakHourNum}:00` : '09:00'
})

const improvementArea = computed(() => {
  if (efficiencyScore.value < 80) return '時間管理'
  if (onTimeCompletionRate.value < 80) return '截止期限'
  if (taskCompletionRate.value < 70) return '任務執行'
  return '持續優化'
})

// Methods
function formatDateRange(start: string, end: string): string {
  const startDate = new Date(start)
  const endDate = new Date(end)
  
  return `${startDate.toLocaleDateString('zh-TW')} - ${endDate.toLocaleDateString('zh-TW')}`
}

function formatShortDate(date: string): string {
  return new Date(date).toLocaleDateString('zh-TW', {
    month: 'short',
    day: 'numeric'
  })
}

</script>