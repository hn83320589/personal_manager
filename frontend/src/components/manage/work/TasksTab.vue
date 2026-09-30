<template>
  <div class="flex flex-col gap-3">
    <div class="flex flex-wrap items-center gap-2">
      <form class="flex min-w-60 flex-1 gap-2" @submit.prevent="quickAdd">
        <label for="quick-task" class="sr-only">新增任務</label>
        <input
          id="quick-task"
          v-model.trim="quickTitle"
          class="input py-1.5 text-sm"
          placeholder="新增任務，按 Enter"
          maxlength="200"
        />
      </form>
      <label for="task-project" class="sr-only">依專案篩選</label>
      <select id="task-project" v-model="projectFilter" class="input w-auto py-1.5 text-sm">
        <option :value="null">所有專案</option>
        <option v-for="project in projects" :key="project.id" :value="project.id">
          {{ project.name }}
        </option>
      </select>
      <label class="flex items-center gap-1.5 text-sm text-muted">
        <input v-model="showDone" type="checkbox" class="h-4 w-4 accent-accent" />
        顯示已完成
      </label>
      <button type="button" class="btn btn-small" @click="$emit('manage-projects')">
        管理專案
      </button>
    </div>

    <p
      v-if="!visible.length"
      class="rounded-xl border border-rule bg-surface p-8 text-center text-muted"
    >
      沒有任務。
    </p>
    <ul v-else class="flex flex-col overflow-hidden rounded-xl border border-rule bg-surface">
      <li v-for="task in visible" :key="task.id" class="border-rule [&+&]:border-t">
        <button
          type="button"
          class="flex w-full flex-wrap items-center gap-x-3 gap-y-1 px-4 py-2.5 text-left hover:bg-soft"
          @click="openEdit(task)"
        >
          <span class="min-w-0 flex-1">
            <span
              :class="[
                'block truncate',
                task.status === 'Completed' ? 'text-muted line-through' : '',
              ]"
              >{{ task.title }}</span
            >
            <span class="flex flex-wrap gap-x-2 text-xs text-muted">
              <span v-if="task.projectName" class="flex items-center gap-1">
                <i
                  class="h-2 w-2 rounded-full"
                  :style="{ background: projectColor(task.projectId) }"
                />{{ task.projectName }}
              </span>
              <span>{{ statusLabel(task.status) }}</span>
              <span
                v-if="task.priority === 'Urgent' || task.priority === 'High'"
                class="text-danger"
                >{{ priorityLabel(task.priority) }}優先</span
              >
              <span v-if="task.dueDate">到期 {{ task.dueDate.slice(0, 10) }}</span>
            </span>
          </span>
          <span class="font-mono text-xs tabular-nums text-muted">
            {{ formatMinutes(task.actualMinutes)
            }}{{ task.estimatedHours ? ` / 預估 ${task.estimatedHours} 小時` : '' }}
          </span>
        </button>
      </li>
    </ul>

    <SidePanel
      :open="editing !== null"
      title="編輯任務"
      :saving="running"
      @close="editing = null"
      @submit="submit"
    >
      <template v-if="editing">
        <FormField v-slot="{ id }" label="任務" required>
          <input :id="id" v-model.trim="editing.title" class="input" maxlength="200" required />
        </FormField>
        <FormField v-slot="{ id }" label="專案">
          <select :id="id" v-model="editing.projectId" class="input">
            <option :value="null">不屬於專案</option>
            <option v-for="project in projects" :key="project.id" :value="project.id">
              {{ project.name }}
            </option>
          </select>
        </FormField>
        <div class="grid grid-cols-2 gap-3">
          <FormField v-slot="{ id }" label="狀態">
            <select :id="id" v-model="editing.status" class="input">
              <option v-for="o in taskStatusOptions" :key="o.value" :value="o.value">
                {{ o.label }}
              </option>
            </select>
          </FormField>
          <FormField v-slot="{ id }" label="優先度">
            <select :id="id" v-model="editing.priority" class="input">
              <option v-for="o in taskPriorityOptions" :key="o.value" :value="o.value">
                {{ o.label }}
              </option>
            </select>
          </FormField>
          <FormField v-slot="{ id }" label="預估時數">
            <input
              :id="id"
              v-model.number="editing.estimatedHours"
              type="number"
              min="0"
              step="0.5"
              class="input"
            />
          </FormField>
          <FormField v-slot="{ id }" label="到期日">
            <input :id="id" v-model="editing.dueDate" type="date" class="input" />
          </FormField>
        </div>
        <FormField v-slot="{ id }" label="說明">
          <textarea
            :id="id"
            v-model="editing.description"
            class="input min-h-24"
            maxlength="5000"
          />
        </FormField>
      </template>
      <template v-if="editingId" #footer-start>
        <DeleteButton item-name="這個任務" @confirm="remove" />
      </template>
    </SidePanel>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { workTasksApi } from '@/api/tools'
import type { Schemas } from '@/api/types'
import { useAsyncAction } from '@/composables/useAsyncAction'
import { formatMinutes } from '@/lib/timer'
import DeleteButton from '../DeleteButton.vue'
import FormField from '../FormField.vue'
import SidePanel from '../SidePanel.vue'
import { priorityLabel, statusLabel, taskPriorityOptions, taskStatusOptions } from './workLabels'

type Task = Schemas['WorkTaskDto']
type TaskForm = Omit<Schemas['SaveWorkTaskRequest'], 'dueDate' | 'description' | 'projectId'> & {
  description: string
  dueDate: string
  projectId: number | null
}

const props = defineProps<{ projects: Schemas['ProjectDto'][] }>()
defineEmits<{ 'manage-projects': [] }>()
const tasks = defineModel<Task[]>({ required: true })

const projectFilter = ref<number | null>(null)
const showDone = ref(false)
const done = new Set<Schemas['WorkTaskStatus']>(['Completed', 'Cancelled'])

const visible = computed(() =>
  tasks.value.filter(
    (t) =>
      (projectFilter.value === null || t.projectId === projectFilter.value) &&
      (showDone.value || !done.has(t.status)),
  ),
)

const projectColor = (id?: number | null) =>
  props.projects.find((p) => p.id === id)?.color || 'rgb(var(--muted))'

const { run, running } = useAsyncAction()
const quickTitle = ref('')

async function quickAdd() {
  if (!quickTitle.value) return
  const created = await run(() =>
    workTasksApi.create({
      title: quickTitle.value,
      projectId: projectFilter.value,
      priority: 'Medium',
      status: 'Pending',
      estimatedHours: 0,
    }),
  )
  if (!created) return
  tasks.value = [created, ...tasks.value]
  quickTitle.value = ''
}

const editing = ref<TaskForm | null>(null)
const editingId = ref<number | null>(null)

function openEdit(task: Task) {
  editingId.value = task.id
  editing.value = {
    title: task.title,
    description: task.description,
    projectId: task.projectId ?? null,
    priority: task.priority,
    status: task.status,
    estimatedHours: task.estimatedHours,
    dueDate: task.dueDate?.slice(0, 10) ?? '',
  }
}

async function submit() {
  if (!editing.value || editingId.value === null) return
  const form = editing.value
  const body: Schemas['SaveWorkTaskRequest'] = {
    ...form,
    estimatedHours: typeof form.estimatedHours === 'number' ? form.estimatedHours : 0,
    dueDate: form.dueDate ? `${form.dueDate}T00:00:00` : null,
  }
  const saved = await run(() => workTasksApi.update(editingId.value!, body), {
    success: '已更新任務',
  })
  if (!saved) return
  tasks.value = tasks.value.map((t) => (t.id === saved.id ? saved : t))
  editing.value = null
}

async function remove() {
  const id = editingId.value
  if (id === null) return
  if (await run(async () => (await workTasksApi.remove(id), true), { success: '已刪除任務' })) {
    tasks.value = tasks.value.filter((t) => t.id !== id)
    editing.value = null
  }
}
</script>
