<template>
  <AdminPage title="技能" description="名稱與分類可以自由輸入；上方的建議依你的作品集模式提供。">
    <template #actions>
      <button type="button" class="btn btn-primary btn-small" @click="openNew()">新增技能</button>
    </template>

    <section
      v-if="suggestions.length"
      class="flex flex-col gap-2 rounded-xl border border-dashed border-rule p-4"
      aria-label="建議的技能"
    >
      <p class="text-sm text-muted">點一下就能加入：</p>
      <div
        v-for="group in suggestions"
        :key="group.category"
        class="flex flex-wrap items-center gap-1.5"
      >
        <span class="mr-1 font-mono text-xs text-muted">{{ group.category }}</span>
        <button
          v-for="name in group.names"
          :key="name"
          type="button"
          class="rounded-full border border-dashed border-rule px-2.5 py-0.5 text-xs text-muted hover:border-accent hover:text-accent"
          @click="openNew({ name, category: group.category })"
        >
          + {{ name }}
        </button>
      </div>
    </section>

    <PageState :loading="loading" :error="error" @retry="reload">
      <p
        v-if="!items.length"
        class="rounded-xl border border-rule bg-surface p-8 text-center text-muted"
      >
        還沒有技能。
      </p>
      <ManageList
        v-else
        :items="items"
        :label-of="(s) => s.name"
        @edit="openEdit"
        @move="move"
        @remove="remove"
      >
        <template #default="{ item }">
          <span class="font-medium">{{ item.name }}</span>
          <span class="ml-2 text-sm text-muted">{{
            [
              item.category,
              levelLabel(item),
              item.yearsOfExperience ? `${item.yearsOfExperience} 年` : '',
            ]
              .filter(Boolean)
              .join(' · ')
          }}</span>
          <span v-if="!item.isPublic" class="ml-2 rounded bg-soft px-1.5 text-xs text-muted"
            >不公開</span
          >
        </template>
      </ManageList>
    </PageState>

    <SidePanel
      :open="editing !== null"
      :title="editingId ? '編輯技能' : '新增技能'"
      :saving="saving"
      @close="editing = null"
      @submit="submit"
    >
      <template v-if="editing">
        <FormField v-slot="{ id }" label="名稱" required>
          <input :id="id" v-model.trim="editing.name" class="input" maxlength="100" required />
        </FormField>
        <FormField v-slot="{ id }" label="分類" hint="相同分類的技能會放在一起顯示。">
          <input
            :id="id"
            v-model.trim="editing.category"
            class="input"
            maxlength="50"
            list="skill-categories"
          />
          <datalist id="skill-categories">
            <option v-for="category in categories" :key="category" :value="category" />
          </datalist>
        </FormField>
        <div class="grid gap-4 sm:grid-cols-2">
          <FormField v-slot="{ id }" label="熟練度" hint="選填">
            <select :id="id" v-model="editing.level" class="input">
              <option :value="null">不填</option>
              <option v-for="level in skillLevels" :key="level.value" :value="level.value">
                {{ level.label }}
              </option>
            </select>
          </FormField>
          <FormField v-slot="{ id }" label="年資" hint="選填">
            <input
              :id="id"
              v-model.number="editing.yearsOfExperience"
              type="number"
              min="0"
              max="60"
              class="input"
            />
          </FormField>
        </div>
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
import { skillsApi } from '@/api/collections'
import { profileApi } from '@/api/profile'
import type { Schemas } from '@/api/types'
import { useAsyncData } from '@/composables/useAsyncData'
import { useOwnedList } from '@/composables/useOwnedList'
import AdminPage from '@/components/manage/AdminPage.vue'
import FormField from '@/components/manage/FormField.vue'
import ManageList from '@/components/manage/ManageList.vue'
import SidePanel from '@/components/manage/SidePanel.vue'
import { skillLevels, skillSuggestions } from '@/components/manage/skills/skillSuggestions'
import PageState from '@/components/public/PageState.vue'

type Skill = Schemas['SkillDto']
type SkillForm = Omit<Schemas['SaveSkillRequest'], 'level'> & {
  level: Schemas['SkillLevel'] | null
}

const { items, loading, error, saving, reload, save, remove, move } = useOwnedList(skillsApi, {
  created: '已新增技能',
  updated: '已更新技能',
  removed: '已刪除技能',
})
const { data: profile } = useAsyncData(profileApi.get)

const categories = computed(() => [...new Set(items.value.map((s) => s.category).filter(Boolean))])

/** 只列出清單裡還沒有的建議。 */
const suggestions = computed(() => {
  if (!profile.value) return []
  const existing = new Set(items.value.map((s) => s.name.toLowerCase()))
  return skillSuggestions[profile.value.portfolioMode]
    .map((g) => ({ ...g, names: g.names.filter((n) => !existing.has(n.toLowerCase())) }))
    .filter((g) => g.names.length)
})

const editing = ref<SkillForm | null>(null)
const editingId = ref<number | null>(null)

function openNew(preset: Partial<SkillForm> = {}) {
  editingId.value = null
  editing.value = {
    name: '',
    category: '',
    level: null,
    yearsOfExperience: null,
    isPublic: true,
    ...preset,
  }
}

function openEdit(skill: Skill) {
  editingId.value = skill.id
  editing.value = {
    name: skill.name,
    category: skill.category,
    level: skill.level ?? null,
    yearsOfExperience: skill.yearsOfExperience ?? null,
    isPublic: skill.isPublic,
  }
}

async function submit() {
  if (!editing.value) return
  const { level, yearsOfExperience, ...rest } = editing.value
  const body: Schemas['SaveSkillRequest'] = {
    ...rest,
    level: level ?? undefined,
    yearsOfExperience: typeof yearsOfExperience === 'number' ? yearsOfExperience : null,
  }
  if (await save(editingId.value, body)) editing.value = null
}

function levelLabel(skill: Skill) {
  return skillLevels.find((l) => l.value === skill.level)?.label ?? ''
}
</script>
