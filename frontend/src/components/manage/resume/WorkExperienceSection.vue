<template>
  <section class="flex flex-col gap-3" aria-labelledby="work-title">
    <div class="flex items-center justify-between gap-3">
      <h2 id="work-title" class="font-bold">工作經歷</h2>
      <button type="button" class="btn btn-small" @click="openNew">新增經歷</button>
    </div>
    <PageState :loading="loading" :error="error" @retry="reload">
      <p
        v-if="!items.length"
        class="rounded-xl border border-rule bg-surface p-6 text-center text-muted"
      >
        還沒有工作經歷。
      </p>
      <ManageList
        v-else
        :items="items"
        :label-of="(w) => w.position"
        @edit="openEdit"
        @move="move"
        @remove="remove"
      >
        <template #default="{ item }">
          <p class="font-medium">
            {{ item.position }} <span class="font-normal text-muted">· {{ item.company }}</span>
          </p>
          <p class="font-mono text-xs text-muted">
            {{
              formatPeriod({ start: item.startDate, end: item.endDate, isCurrent: item.isCurrent })
            }}
            <span v-if="!item.isPublic" class="ml-2 rounded bg-soft px-1.5 font-sans">不公開</span>
          </p>
        </template>
      </ManageList>
    </PageState>

    <SidePanel
      :open="editing !== null"
      :title="editingId ? '編輯工作經歷' : '新增工作經歷'"
      :saving="saving"
      @close="editing = null"
      @submit="submit"
    >
      <template v-if="editing">
        <FormField v-slot="{ id }" label="職稱" required>
          <input :id="id" v-model.trim="editing.position" class="input" maxlength="100" required />
        </FormField>
        <FormField v-slot="{ id }" label="公司" required>
          <input :id="id" v-model.trim="editing.company" class="input" maxlength="100" required />
        </FormField>
        <div class="grid gap-4 sm:grid-cols-2">
          <FormField v-slot="{ id }" label="開始">
            <input :id="id" v-model="editing.startMonth" type="month" class="input" />
          </FormField>
          <FormField v-slot="{ id }" label="結束">
            <input
              :id="id"
              v-model="editing.endMonth"
              type="month"
              class="input"
              :disabled="editing.isCurrent"
            />
          </FormField>
        </div>
        <label class="flex items-center gap-2 text-sm">
          <input v-model="editing.isCurrent" type="checkbox" class="h-4 w-4 accent-accent" />
          目前在職
        </label>
        <FormField v-slot="{ id }" label="工作內容" hint="負責的事情與成果，可以分行。">
          <textarea
            :id="id"
            v-model="editing.description"
            class="input min-h-32"
            maxlength="2000"
          />
        </FormField>
        <label class="flex items-center gap-2 text-sm">
          <input v-model="editing.isPublic" type="checkbox" class="h-4 w-4 accent-accent" />
          顯示在公開頁面
        </label>
      </template>
    </SidePanel>
  </section>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { workExperiencesApi } from '@/api/collections'
import type { Schemas } from '@/api/types'
import { useOwnedList } from '@/composables/useOwnedList'
import { formatPeriod } from '@/lib/format'
import PageState from '@/components/public/PageState.vue'
import FormField from '../FormField.vue'
import ManageList from '../ManageList.vue'
import SidePanel from '../SidePanel.vue'

type Work = Schemas['WorkExperienceDto']
/** 日期欄位以 <input type="month"> 編輯（YYYY-MM），送出時轉成當月第一天。 */
interface WorkForm {
  position: string
  company: string
  startMonth: string
  endMonth: string
  isCurrent: boolean
  description: string
  isPublic: boolean
}

const { items, loading, error, saving, reload, save, remove, move } = useOwnedList(
  workExperiencesApi,
  {
    created: '已新增工作經歷',
    updated: '已更新工作經歷',
    removed: '已刪除工作經歷',
  },
)

const editing = ref<WorkForm | null>(null)
const editingId = ref<number | null>(null)

const toMonth = (date?: string | null) => (date ? date.slice(0, 7) : '')
const toDate = (month: string) => (month ? `${month}-01` : null)

function openNew() {
  editingId.value = null
  editing.value = {
    position: '',
    company: '',
    startMonth: '',
    endMonth: '',
    isCurrent: false,
    description: '',
    isPublic: true,
  }
}

function openEdit(work: Work) {
  editingId.value = work.id
  editing.value = {
    position: work.position,
    company: work.company,
    startMonth: toMonth(work.startDate),
    endMonth: toMonth(work.endDate),
    isCurrent: work.isCurrent,
    description: work.description,
    isPublic: work.isPublic,
  }
}

async function submit() {
  const form = editing.value
  if (!form) return
  const body: Schemas['SaveWorkExperienceRequest'] = {
    position: form.position,
    company: form.company,
    startDate: toDate(form.startMonth),
    endDate: form.isCurrent ? null : toDate(form.endMonth),
    isCurrent: form.isCurrent,
    description: form.description,
    isPublic: form.isPublic,
  }
  if (await save(editingId.value, body)) editing.value = null
}
</script>
