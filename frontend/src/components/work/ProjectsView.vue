<template>
  <div class="p-6">
    <div v-if="loading" class="text-center py-8">
      <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      <p class="mt-2 text-sm text-gray-500">載入中...</p>
    </div>
    
    <div v-else-if="projects.length === 0" class="text-center py-8">
      <FolderIcon class="mx-auto h-12 w-12 text-gray-400" />
      <h3 class="mt-2 text-sm font-medium text-gray-900">沒有專案資料</h3>
      <p class="mt-1 text-sm text-gray-500">開始追蹤您的工作專案。</p>
    </div>

    <div v-else>
      <!-- Projects Grid -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div
          v-for="project in projects"
          :key="project.name"
          class="bg-white border border-gray-200 rounded-lg p-6 hover:shadow-md transition-shadow cursor-pointer"
          @click="$emit('view-project', project)"
        >
          <!-- Project Header -->
          <div class="flex items-center justify-between mb-4">
            <div class="flex items-center space-x-3">
              <div class="p-2 bg-blue-100 rounded-lg">
                <FolderIcon class="w-6 h-6 text-blue-600" />
              </div>
              <div>
                <h3 class="text-lg font-semibold text-gray-900">{{ project.name }}</h3>
                <p class="text-sm text-gray-500">{{ project.totalTasks }} 個任務</p>
              </div>
            </div>
            <BaseButton
              variant="outline"
              size="small"
              @click.stop="$emit('edit-project', project)"
            >
              <PencilIcon class="w-4 h-4" />
            </BaseButton>
          </div>

          <!-- Progress Bar -->
          <div class="mb-4">
            <div class="flex justify-between items-center mb-2">
              <span class="text-sm font-medium text-gray-700">完成進度</span>
              <span class="text-sm text-gray-500">{{ Math.round(project.completionRate) }}%</span>
            </div>
            <div class="w-full bg-gray-200 rounded-full h-2">
              <div 
                class="bg-green-600 h-2 rounded-full transition-all duration-300"
                :style="{ width: `${project.completionRate}%` }"
              ></div>
            </div>
            <div class="flex justify-between text-xs text-gray-500 mt-1">
              <span>{{ project.completedTasks }}/{{ project.totalTasks }} 已完成</span>
            </div>
          </div>

          <!-- Time Statistics -->
          <div class="grid grid-cols-2 gap-4 mb-4">
            <div class="text-center">
              <div class="text-2xl font-bold text-blue-600">{{ project.totalHours.toFixed(1) }}</div>
              <div class="text-xs text-gray-500">實際時間 (小時)</div>
            </div>
            <div class="text-center">
              <div class="text-2xl font-bold text-gray-600">{{ project.estimatedHours.toFixed(1) }}</div>
              <div class="text-xs text-gray-500">預估時間 (小時)</div>
            </div>
          </div>

          <!-- Efficiency Indicator -->
          <div class="flex items-center justify-between mb-4">
            <span class="text-sm text-gray-700">時間效率</span>
            <div class="flex items-center space-x-2">
              <div 
                :class="getEfficiencyStyle(project.efficiency)"
                class="px-2 py-1 rounded-full text-xs font-medium"
              >
                {{ project.efficiency.toFixed(0) }}%
              </div>
              <component 
                :is="getEfficiencyIcon(project.efficiency)"
                :class="getEfficiencyIconColor(project.efficiency)"
                class="w-4 h-4"
              />
            </div>
          </div>

          <!-- Recent Activity -->
          <div class="pt-4 border-t border-gray-100">
            <div class="flex items-center justify-between">
              <span class="text-sm text-gray-500">最近活動</span>
              <span class="text-xs text-gray-400">{{ getRecentActivity(project) }}</span>
            </div>
          </div>

          <!-- Quick Actions -->
          <div class="flex space-x-2 mt-4">
            <BaseButton
              variant="outline"
              size="small"
              @click.stop="$emit('view-project', project)"
              class="flex-1"
            >
              <EyeIcon class="w-4 h-4 mr-1" />
              查看詳情
            </BaseButton>
            <BaseButton
              variant="outline"
              size="small"
              @click.stop="addTaskToProject(project)"
              title="新增任務到此專案"
            >
              <PlusIcon class="w-4 h-4" />
            </BaseButton>
          </div>
        </div>
      </div>

      <!-- Summary Statistics -->
      <div v-if="projects.length > 0" class="mt-8 bg-gray-50 rounded-lg p-6">
        <h3 class="text-lg font-medium text-gray-900 mb-4">專案總覽統計</h3>
        <div class="grid grid-cols-1 md:grid-cols-4 gap-6">
          <div class="text-center">
            <div class="text-3xl font-bold text-blue-600">{{ projects.length }}</div>
            <div class="text-sm text-gray-500">活躍專案</div>
          </div>
          <div class="text-center">
            <div class="text-3xl font-bold text-green-600">{{ totalCompletedTasks }}</div>
            <div class="text-sm text-gray-500">完成任務</div>
          </div>
          <div class="text-center">
            <div class="text-3xl font-bold text-yellow-600">{{ totalWorkedHours.toFixed(1) }}</div>
            <div class="text-sm text-gray-500">總工作時間</div>
          </div>
          <div class="text-center">
            <div class="text-3xl font-bold text-purple-600">{{ overallEfficiency.toFixed(0) }}%</div>
            <div class="text-sm text-gray-500">整體效率</div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import {
  FolderIcon,
  PencilIcon,
  EyeIcon,
  PlusIcon,
  ArrowTrendingUpIcon,
  ArrowTrendingDownIcon,
  MinusIcon
} from '@heroicons/vue/24/outline'
import BaseButton from '@/components/ui/BaseButton.vue'

// Project Statistics Interface
interface ProjectStats {
  name: string
  totalTasks: number
  completedTasks: number
  completionRate: number
  totalHours: number
  estimatedHours: number
  efficiency: number
}

// Props
interface Props {
  projects: ProjectStats[]
  loading: boolean
}

const props = defineProps<Props>()

// Emits
// Computed
const totalCompletedTasks = computed(() => {
  return props.projects.reduce((sum, project) => sum + project.completedTasks, 0)
})

const totalWorkedHours = computed(() => {
  return props.projects.reduce((sum, project) => sum + project.totalHours, 0)
})

const overallEfficiency = computed(() => {
  const totalEstimated = props.projects.reduce((sum, project) => sum + project.estimatedHours, 0)
  const totalActual = props.projects.reduce((sum, project) => sum + project.totalHours, 0)
  
  if (totalEstimated === 0) return 0
  return (totalActual / totalEstimated) * 100
})

// Methods
function getEfficiencyStyle(efficiency: number): string {
  if (efficiency <= 80) return 'bg-green-100 text-green-800'
  if (efficiency <= 120) return 'bg-yellow-100 text-yellow-800'
  return 'bg-red-100 text-red-800'
}

function getEfficiencyIcon(efficiency: number) {
  if (efficiency <= 80) return ArrowTrendingUpIcon
  if (efficiency <= 120) return MinusIcon
  return ArrowTrendingDownIcon
}

function getEfficiencyIconColor(efficiency: number): string {
  if (efficiency <= 80) return 'text-green-600'
  if (efficiency <= 120) return 'text-yellow-600'
  return 'text-red-600'
}

function getRecentActivity(project: ProjectStats): string {
  // This would typically come from actual data
  // For now, we'll simulate based on completion rate
  if (project.completionRate === 100) {
    return '已完成'
  } else if (project.completionRate > 50) {
    return '進行中'
  } else {
    return '剛開始'
  }
}

const emit = defineEmits<{
  'view-project': [project: ProjectStats]
  'edit-project': [project: ProjectStats]
}>()

function addTaskToProject(project: ProjectStats) {
  emit('view-project', project)
}
</script>