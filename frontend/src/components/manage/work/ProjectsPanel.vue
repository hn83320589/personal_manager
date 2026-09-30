<template>
  <SidePanel
    :open="open"
    title="管理專案"
    submit-label="完成"
    @close="$emit('close')"
    @submit="$emit('close')"
  >
    <!-- 面板本身是 form，這裡不能再包 form（HTML 不允許巢狀 form） -->
    <div class="flex gap-2">
      <label for="new-project" class="sr-only">新專案名稱</label>
      <input
        id="new-project"
        v-model.trim="newName"
        class="input"
        placeholder="新專案名稱"
        maxlength="100"
        @keydown.enter.prevent="add"
      />
      <button
        type="button"
        class="btn btn-small shrink-0"
        :disabled="!newName || saving"
        @click="add"
      >
        新增
      </button>
    </div>
    <p v-if="!items.length" class="text-sm text-muted">還沒有專案。</p>
    <ManageList
      v-else
      :items="items"
      :label-of="(p) => p.name"
      @edit="rename"
      @move="move"
      @remove="remove"
    >
      <template #default="{ item }">
        <span class="flex items-center gap-2">
          <i
            class="h-2.5 w-2.5 rounded-full"
            :style="{ background: item.color || 'rgb(var(--muted))' }"
          />
          {{ item.name }}
        </span>
      </template>
    </ManageList>
    <p class="text-xs text-muted">刪除專案不會刪除任務，任務會變成「不屬於專案」。</p>
  </SidePanel>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { projectsApi } from '@/api/tools'
import type { Schemas } from '@/api/types'
import { useOwnedList } from '@/composables/useOwnedList'
import ManageList from '../ManageList.vue'
import SidePanel from '../SidePanel.vue'
import { projectColors } from './workLabels'

type Project = Schemas['ProjectDto']

defineProps<{ open: boolean }>()
const emit = defineEmits<{ close: []; changed: [projects: Project[]] }>()

const { items, saving, save, remove, move } = useOwnedList(projectsApi, {
  created: '已新增專案',
  updated: '已更新專案',
  removed: '已刪除專案',
})
watch(items, (list) => emit('changed', list))

const newName = ref('')

async function add() {
  if (!newName.value) return
  const color = projectColors[items.value.length % projectColors.length]
  if (await save(null, { name: newName.value, color })) newName.value = ''
}

async function rename(project: Project) {
  const name = window.prompt('專案名稱', project.name)?.trim()
  if (name && name !== project.name)
    await save(project.id, { name, description: project.description, color: project.color })
}
</script>
