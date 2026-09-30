<template>
  <section class="flex flex-col gap-3" aria-labelledby="education-title">
    <div class="flex items-center justify-between gap-3">
      <h2 id="education-title" class="font-bold">學歷</h2>
      <button type="button" class="btn btn-small" @click="openNew">新增學歷</button>
    </div>
    <PageState :loading="loading" :error="error" @retry="reload">
      <p
        v-if="!items.length"
        class="rounded-xl border border-rule bg-surface p-6 text-center text-muted"
      >
        還沒有學歷。
      </p>
      <ManageList
        v-else
        :items="items"
        :label-of="(e) => e.school"
        @edit="openEdit"
        @move="move"
        @remove="remove"
      >
        <template #default="{ item }">
          <p class="font-medium">
            {{ item.school }}
            <span class="font-normal text-muted">{{
              [item.degree, item.fieldOfStudy].filter(Boolean).join(' · ')
            }}</span>
          </p>
          <p class="font-mono text-xs text-muted">
            {{ formatPeriod({ start: item.startYear, end: item.endYear }) }}
            <span v-if="!item.isPublic" class="ml-2 rounded bg-soft px-1.5 font-sans">不公開</span>
          </p>
        </template>
      </ManageList>
    </PageState>

    <SidePanel
      :open="editing !== null"
      :title="editingId ? '編輯學歷' : '新增學歷'"
      :saving="saving"
      @close="editing = null"
      @submit="submit"
    >
      <template v-if="editing">
        <FormField v-slot="{ id }" label="學校" required>
          <input :id="id" v-model.trim="editing.school" class="input" maxlength="100" required />
        </FormField>
        <div class="grid gap-4 sm:grid-cols-2">
          <FormField v-slot="{ id }" label="學位">
            <input
              :id="id"
              v-model.trim="editing.degree"
              class="input"
              maxlength="50"
              placeholder="例如：學士"
            />
          </FormField>
          <FormField v-slot="{ id }" label="科系">
            <input :id="id" v-model.trim="editing.fieldOfStudy" class="input" maxlength="100" />
          </FormField>
        </div>
        <div class="grid gap-4 sm:grid-cols-2">
          <FormField v-slot="{ id }" label="入學年份">
            <input
              :id="id"
              v-model.number="editing.startYear"
              type="number"
              min="1900"
              max="2100"
              class="input"
            />
          </FormField>
          <FormField v-slot="{ id }" label="畢業年份">
            <input
              :id="id"
              v-model.number="editing.endYear"
              type="number"
              min="1900"
              max="2100"
              class="input"
            />
          </FormField>
        </div>
        <FormField v-slot="{ id }" label="說明">
          <textarea
            :id="id"
            v-model="editing.description"
            class="input min-h-24"
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
import { educationsApi } from '@/api/collections'
import type { Schemas } from '@/api/types'
import { useOwnedList } from '@/composables/useOwnedList'
import { formatPeriod } from '@/lib/format'
import PageState from '@/components/public/PageState.vue'
import FormField from '../FormField.vue'
import ManageList from '../ManageList.vue'
import SidePanel from '../SidePanel.vue'

type Education = Schemas['EducationDto']
type EducationForm = {
  school: string
  degree: string
  fieldOfStudy: string
  startYear: number | ''
  endYear: number | ''
  description: string
  isPublic: boolean
}

const { items, loading, error, saving, reload, save, remove, move } = useOwnedList(educationsApi, {
  created: '已新增學歷',
  updated: '已更新學歷',
  removed: '已刪除學歷',
})

const editing = ref<EducationForm | null>(null)
const editingId = ref<number | null>(null)

function openNew() {
  editingId.value = null
  editing.value = {
    school: '',
    degree: '',
    fieldOfStudy: '',
    startYear: '',
    endYear: '',
    description: '',
    isPublic: true,
  }
}

function openEdit(e: Education) {
  editingId.value = e.id
  editing.value = {
    school: e.school,
    degree: e.degree,
    fieldOfStudy: e.fieldOfStudy,
    startYear: e.startYear ?? '',
    endYear: e.endYear ?? '',
    description: e.description,
    isPublic: e.isPublic,
  }
}

const year = (value: number | '') => (typeof value === 'number' ? value : null)

async function submit() {
  const form = editing.value
  if (!form) return
  const body: Schemas['SaveEducationRequest'] = {
    ...form,
    startYear: year(form.startYear),
    endYear: year(form.endYear),
  }
  if (await save(editingId.value, body)) editing.value = null
}
</script>
