<template>
  <AdminPage title="聯絡方式" description="顯示在公開頁面最下方的「聯絡」區塊，訪客可以一鍵複製。">
    <template #actions>
      <button type="button" class="btn btn-primary btn-small" @click="openNew">新增聯絡方式</button>
    </template>

    <PageState :loading="loading" :error="error" @retry="reload">
      <p
        v-if="!items.length"
        class="rounded-xl border border-rule bg-surface p-8 text-center text-muted"
      >
        還沒有聯絡方式。
      </p>
      <ManageList
        v-else
        :items="items"
        :label-of="(c) => c.label || typeLabel(c.type)"
        @edit="openEdit"
        @move="move"
        @remove="remove"
      >
        <template #default="{ item }">
          <p class="font-mono text-xs uppercase tracking-[0.06em] text-muted">
            {{ item.label || typeLabel(item.type) }}
          </p>
          <p class="truncate">
            {{ item.value }}
            <span v-if="!item.isPublic" class="ml-2 rounded bg-soft px-1.5 text-xs text-muted"
              >不公開</span
            >
          </p>
        </template>
      </ManageList>
    </PageState>

    <SidePanel
      :open="editing !== null"
      :title="editingId ? '編輯聯絡方式' : '新增聯絡方式'"
      :saving="saving"
      @close="editing = null"
      @submit="submit"
    >
      <template v-if="editing">
        <FormField v-slot="{ id }" label="類型" required>
          <select :id="id" v-model="editing.type" class="input">
            <option v-for="type in contactTypes" :key="type.value" :value="type.value">
              {{ type.label }}
            </option>
          </select>
        </FormField>
        <FormField v-slot="{ id }" :label="valueLabel" required :hint="valueHint">
          <input :id="id" v-model.trim="editing.value" class="input" maxlength="200" required />
        </FormField>
        <FormField
          v-slot="{ id }"
          label="顯示名稱"
          hint="選填，例如「工作信箱」；留空則顯示類型名稱。"
        >
          <input :id="id" v-model.trim="editing.label" class="input" maxlength="50" />
        </FormField>
        <label class="flex items-center gap-2 text-sm">
          <input v-model="editing.isPublic" type="checkbox" class="h-4 w-4 accent-accent" />
          顯示在公開頁面
        </label>
      </template>
    </SidePanel>
  </AdminPage>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { contactMethodsApi } from '@/api/collections'
import type { Schemas } from '@/api/types'
import { useOwnedList } from '@/composables/useOwnedList'
import AdminPage from '@/components/manage/AdminPage.vue'
import FormField from '@/components/manage/FormField.vue'
import ManageList from '@/components/manage/ManageList.vue'
import SidePanel from '@/components/manage/SidePanel.vue'
import PageState from '@/components/public/PageState.vue'

type Contact = Schemas['ContactMethodDto']
type ContactType = Schemas['ContactType']
type ContactForm = { type: ContactType; value: string; label: string; isPublic: boolean }

const contactTypes: { value: ContactType; label: string }[] = [
  { value: 'Email', label: 'Email' },
  { value: 'Phone', label: '電話' },
  { value: 'Website', label: '個人網站' },
  { value: 'LinkedIn', label: 'LinkedIn' },
  { value: 'GitHub', label: 'GitHub' },
  { value: 'Behance', label: 'Behance' },
  { value: 'Dribbble', label: 'Dribbble' },
  { value: 'Instagram', label: 'Instagram' },
  { value: 'Threads', label: 'Threads' },
  { value: 'Facebook', label: 'Facebook' },
  { value: 'Twitter', label: 'X（Twitter）' },
  { value: 'YouTube', label: 'YouTube' },
  { value: 'Line', label: 'LINE' },
  { value: 'Discord', label: 'Discord' },
  { value: 'Other', label: '其他' },
]

const typeLabel = (type: ContactType) => contactTypes.find((t) => t.value === type)?.label ?? type

const { items, loading, error, saving, reload, save, remove, move } = useOwnedList(
  contactMethodsApi,
  {
    created: '已新增聯絡方式',
    updated: '已更新聯絡方式',
    removed: '已刪除聯絡方式',
  },
)

const editing = ref<ContactForm | null>(null)
const editingId = ref<number | null>(null)

const valueLabel = computed(() =>
  editing.value?.type === 'Email'
    ? 'Email 地址'
    : editing.value?.type === 'Phone'
      ? '電話號碼'
      : '網址或帳號',
)
const valueHint = computed(() =>
  ['Email', 'Phone', 'Line', 'Discord'].includes(editing.value?.type ?? '')
    ? undefined
    : '貼上完整網址（https://…），訪客可以直接點開。',
)

function openNew() {
  editingId.value = null
  editing.value = { type: 'Email', value: '', label: '', isPublic: true }
}

function openEdit(c: Contact) {
  editingId.value = c.id
  editing.value = { type: c.type, value: c.value, label: c.label, isPublic: c.isPublic }
}

async function submit() {
  if (editing.value && (await save(editingId.value, editing.value))) editing.value = null
}
</script>
