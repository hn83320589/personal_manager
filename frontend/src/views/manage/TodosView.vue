<template>
  <AdminPage title="待辦" description="只有你看得到。按 Enter 快速新增，點項目編輯細節。">
    <form class="flex gap-2" @submit.prevent="quickAdd">
      <label for="quick-todo" class="sr-only">新增待辦</label>
      <input
        id="quick-todo"
        v-model.trim="quickTitle"
        class="input"
        placeholder="新增待辦，按 Enter"
        maxlength="200"
      />
      <button
        type="submit"
        class="btn btn-primary btn-small shrink-0"
        :disabled="!quickTitle || saving"
      >
        新增
      </button>
    </form>

    <PageState :loading="loading" :error="error" @retry="reload">
      <section
        v-for="group in groups"
        :key="group.status"
        class="flex flex-col gap-2"
        :aria-label="group.label"
      >
        <h2 class="flex items-center gap-2 text-sm font-medium text-muted">
          {{ group.label }} <span class="font-mono text-xs">{{ group.items.length }}</span>
          <button
            v-if="group.status === 'Completed' && group.items.length"
            type="button"
            class="ml-auto text-xs font-normal hover:text-ink"
            @click="showCompleted = !showCompleted"
          >
            {{ showCompleted ? '收起' : '顯示' }}
          </button>
        </h2>
        <ul
          v-if="group.status !== 'Completed' || showCompleted"
          class="flex flex-col overflow-hidden rounded-xl border border-rule bg-surface"
        >
          <li v-if="!group.items.length" class="px-4 py-3 text-sm text-muted">沒有項目</li>
          <li
            v-for="todo in group.items"
            :key="todo.id"
            class="flex items-center gap-3 border-rule px-4 py-2.5 [&+&]:border-t"
          >
            <input
              type="checkbox"
              class="h-4 w-4 shrink-0 accent-accent"
              :checked="todo.status === 'Completed'"
              :aria-label="`完成「${todo.title}」`"
              @change="toggle(todo)"
            />
            <button type="button" class="min-w-0 flex-1 text-left" @click="openEdit(todo)">
              <span
                :class="[
                  'block truncate',
                  todo.status === 'Completed' ? 'text-muted line-through' : '',
                ]"
                >{{ todo.title }}</span
              >
              <span class="flex gap-2 text-xs text-muted">
                <span
                  v-if="todo.priority !== 'Medium'"
                  :class="todo.priority === 'High' ? 'text-danger' : ''"
                >
                  {{ priorityLabel[todo.priority] }}優先
                </span>
                <span v-if="todo.dueDate" :class="isOverdue(todo) ? 'text-danger' : ''"
                  >到期 {{ todo.dueDate }}</span
                >
              </span>
            </button>
            <DeleteButton :item-name="todo.title" @confirm="remove(todo.id)" />
          </li>
        </ul>
      </section>
    </PageState>

    <SidePanel
      :open="editing !== null"
      title="編輯待辦"
      :saving="saving"
      @close="editing = null"
      @submit="submit"
    >
      <template v-if="editing">
        <FormField v-slot="{ id }" label="標題" required>
          <input :id="id" v-model.trim="editing.title" class="input" maxlength="200" required />
        </FormField>
        <FormField v-slot="{ id }" label="說明">
          <textarea
            :id="id"
            v-model="editing.description"
            class="input min-h-24"
            maxlength="2000"
          />
        </FormField>
        <div class="flex flex-col gap-1.5">
          <span class="text-sm font-medium">狀態</span>
          <SegmentedControl v-model="editing.status" :options="statusOptions" label="狀態" />
        </div>
        <div class="flex flex-col gap-1.5">
          <span class="text-sm font-medium">優先度</span>
          <SegmentedControl v-model="editing.priority" :options="priorityOptions" label="優先度" />
        </div>
        <FormField v-slot="{ id }" label="到期日">
          <input :id="id" v-model="editing.dueDate" type="date" class="input" />
        </FormField>
      </template>
    </SidePanel>
  </AdminPage>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { todosApi } from '@/api/tools'
import type { Schemas } from '@/api/types'
import { useAsyncAction } from '@/composables/useAsyncAction'
import { useAsyncData } from '@/composables/useAsyncData'
import { localDate } from '@/lib/format'
import AdminPage from '@/components/manage/AdminPage.vue'
import DeleteButton from '@/components/manage/DeleteButton.vue'
import FormField from '@/components/manage/FormField.vue'
import SegmentedControl from '@/components/manage/SegmentedControl.vue'
import SidePanel from '@/components/manage/SidePanel.vue'
import PageState from '@/components/public/PageState.vue'

type Todo = Schemas['TodoDto']
type TodoForm = Omit<Schemas['SaveTodoRequest'], 'description' | 'dueDate'> & {
  description: string
  dueDate: string
}

const priorityLabel: Record<Schemas['TodoPriority'], string> = {
  Low: '低',
  Medium: '中',
  High: '高',
}
const priorityOptions = (['High', 'Medium', 'Low'] as const).map((value) => ({
  value,
  label: priorityLabel[value],
}))
const statusOptions: { value: Schemas['TodoStatus']; label: string }[] = [
  { value: 'Pending', label: '待辦' },
  { value: 'InProgress', label: '進行中' },
  { value: 'Completed', label: '已完成' },
]

const { data, loading, error, reload } = useAsyncData(() => todosApi.list())
const todos = ref<Todo[]>([])
watch(data, (loaded) => (todos.value = loaded ?? []))
const { run, running: saving } = useAsyncAction()

const showCompleted = ref(false)
const priorityRank: Record<Schemas['TodoPriority'], number> = { High: 0, Medium: 1, Low: 2 }

/** 依狀態分組；同組內高優先度在前，再依到期日。 */
const groups = computed(() =>
  statusOptions.map((option) => ({
    status: option.value,
    label: option.label,
    items: todos.value
      .filter((t) => t.status === option.value)
      .sort(
        (a, b) =>
          priorityRank[a.priority] - priorityRank[b.priority] ||
          (a.dueDate ?? '9999').localeCompare(b.dueDate ?? '9999'),
      ),
  })),
)

const today = localDate()
const isOverdue = (todo: Todo) =>
  todo.status !== 'Completed' && !!todo.dueDate && todo.dueDate < today

function toRequest(todo: Todo): Schemas['SaveTodoRequest'] {
  return {
    title: todo.title,
    description: todo.description,
    priority: todo.priority,
    status: todo.status,
    dueDate: todo.dueDate ?? null,
  }
}

function replace(saved: Todo) {
  todos.value = todos.value.map((t) => (t.id === saved.id ? saved : t))
}

const quickTitle = ref('')
async function quickAdd() {
  const created = await run(() =>
    todosApi.create({ title: quickTitle.value, priority: 'Medium', status: 'Pending' }),
  )
  if (!created) return
  todos.value = [...todos.value, created]
  quickTitle.value = ''
}

async function toggle(todo: Todo) {
  const status: Schemas['TodoStatus'] = todo.status === 'Completed' ? 'Pending' : 'Completed'
  const saved = await run(() => todosApi.update(todo.id, { ...toRequest(todo), status }))
  if (saved) replace(saved)
}

async function remove(id: number) {
  if (await run(async () => (await todosApi.remove(id), true)))
    todos.value = todos.value.filter((t) => t.id !== id)
}

const editing = ref<TodoForm | null>(null)
const editingId = ref<number | null>(null)

function openEdit(todo: Todo) {
  editingId.value = todo.id
  editing.value = { ...toRequest(todo), description: todo.description, dueDate: todo.dueDate ?? '' }
}

async function submit() {
  if (!editing.value || editingId.value === null) return
  const saved = await run(
    () =>
      todosApi.update(editingId.value!, {
        ...editing.value!,
        dueDate: editing.value!.dueDate || null,
      }),
    {
      success: '已更新待辦',
    },
  )
  if (saved) {
    replace(saved)
    editing.value = null
  }
}
</script>
