<template>
  <AdminPage title="工作追蹤" description="任務、計時與時間紀錄，只有你看得到。">
    <TimerBar :tasks="tasks" @saved="onTimeChanged" />

    <SegmentedControl v-model="tab" :options="tabs" label="檢視" class="max-w-sm" />

    <PageState :loading="loading" :error="error" @retry="reload">
      <TasksTab
        v-if="tab === 'tasks'"
        v-model="tasks"
        :projects="projects"
        @manage-projects="projectsOpen = true"
      />
      <TimeEntriesTab
        v-else-if="tab === 'time'"
        ref="timeTab"
        :tasks="tasks"
        @changed="refreshTasks"
      />
      <SummaryTab v-else ref="summaryTab" />
    </PageState>

    <ProjectsPanel
      :open="projectsOpen"
      @close="projectsOpen = false"
      @changed="onProjectsChanged"
    />
  </AdminPage>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { projectsApi, workTasksApi } from '@/api/tools'
import type { Schemas } from '@/api/types'
import { useAsyncData } from '@/composables/useAsyncData'
import AdminPage from '@/components/manage/AdminPage.vue'
import SegmentedControl from '@/components/manage/SegmentedControl.vue'
import ProjectsPanel from '@/components/manage/work/ProjectsPanel.vue'
import SummaryTab from '@/components/manage/work/SummaryTab.vue'
import TasksTab from '@/components/manage/work/TasksTab.vue'
import TimeEntriesTab from '@/components/manage/work/TimeEntriesTab.vue'
import TimerBar from '@/components/manage/work/TimerBar.vue'
import PageState from '@/components/public/PageState.vue'

type Tab = 'tasks' | 'time' | 'summary'
const tabs: { value: Tab; label: string }[] = [
  { value: 'tasks', label: '任務' },
  { value: 'time', label: '時間紀錄' },
  { value: 'summary', label: '統計' },
]
const tab = ref<Tab>('tasks')
const projectsOpen = ref(false)

const { data, loading, error, reload } = useAsyncData(async () => {
  const [projects, tasks] = await Promise.all([projectsApi.list(), workTasksApi.list()])
  return { projects, tasks }
})

const projects = ref<Schemas['ProjectDto'][]>([])
const tasks = ref<Schemas['WorkTaskDto'][]>([])
watch(data, (loaded) => {
  projects.value = loaded?.projects ?? []
  tasks.value = loaded?.tasks ?? []
})

const timeTab = ref<InstanceType<typeof TimeEntriesTab>>()
const summaryTab = ref<InstanceType<typeof SummaryTab>>()

/** 任務的實際時數由時間紀錄加總，紀錄變動後重新載入任務。 */
async function refreshTasks() {
  tasks.value = await workTasksApi.list()
}

async function onTimeChanged() {
  await Promise.all([refreshTasks(), timeTab.value?.reload(), summaryTab.value?.reload()])
}

async function onProjectsChanged(list: Schemas['ProjectDto'][]) {
  projects.value = list
  await refreshTasks() // 專案改名或刪除後，任務上的專案名稱也要更新
}
</script>
