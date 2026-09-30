<template>
  <aside
    class="flex flex-col gap-3.5 rounded-xl border border-rule bg-surface p-[18px]"
    aria-label="作品資訊"
  >
    <h2 :class="sectionTitle">基本資訊</h2>
    <FormField v-slot="{ id }" label="標題" required>
      <input :id="id" v-model="work.title" class="input" maxlength="200" />
    </FormField>
    <FormField v-slot="{ id }" label="一句話簡介" hint="顯示在作品卡片與作品頁開頭。">
      <textarea :id="id" v-model="work.summary" class="input min-h-20" maxlength="500" />
    </FormField>
    <div class="grid grid-cols-[minmax(0,1fr)_6rem] gap-2">
      <FormField v-slot="{ id }" label="分類">
        <input
          :id="id"
          v-model.trim="work.category"
          class="input"
          maxlength="50"
          :list="`${id}-list`"
        />
        <datalist :id="`${id}-list`">
          <option v-for="c in categories" :key="c" :value="c" />
        </datalist>
      </FormField>
      <FormField v-slot="{ id }" label="年份">
        <input :id="id" v-model.number="year" type="number" min="1900" max="2100" class="input" />
      </FormField>
    </div>

    <hr class="border-rule" />
    <h2 :class="sectionTitle">封面</h2>
    <CoverPicker
      v-model:covers="work.covers"
      v-model:focus="work.coverFocus"
      :blocks="work.blocks"
    />

    <hr class="border-rule" />
    <h2 :class="sectionTitle">作品資訊欄位</h2>
    <div class="grid grid-cols-2 gap-2">
      <FormField v-slot="{ id }" label="角色">
        <input
          :id="id"
          v-model="work.role"
          class="input"
          maxlength="100"
          placeholder="例如：品牌設計"
        />
      </FormField>
      <FormField v-slot="{ id }" label="期間">
        <input
          :id="id"
          v-model="work.period"
          class="input"
          maxlength="100"
          placeholder="2024.10 – 2025.04"
        />
      </FormField>
    </div>
    <PairRows
      v-model="work.fields"
      key-field="label"
      value-field="value"
      key-label="欄位名稱"
      value-label="內容"
      :suggestions="fieldSuggestions[mode]"
      :max="20"
    />

    <hr class="border-rule" />
    <h2 :class="sectionTitle">連結</h2>
    <PairRows
      v-model="work.links"
      key-field="label"
      value-field="url"
      key-label="名稱"
      value-label="網址"
      value-placeholder="https://"
      value-type="url"
      :suggestions="['網站', 'Behance', 'GitHub', '線上試玩']"
      :max="10"
    />

    <hr class="border-rule" />
    <h2 :class="sectionTitle">標籤與網址</h2>
    <FormField v-slot="{ id }" label="標籤" hint="訪客可以依標籤篩選作品。">
      <TagInput :id="id" v-model="work.tags" :suggestions="tagSuggestions" />
    </FormField>
    <FormField
      v-slot="{ id }"
      label="網址代稱"
      :hint="`作品網址：/@${username}/works/${work.slug || '…'}`"
    >
      <input
        :id="id"
        v-model.trim="work.slug"
        class="input font-mono text-sm"
        maxlength="80"
        pattern="[a-z0-9]+(-[a-z0-9]+)*"
      />
    </FormField>
    <label class="flex items-center gap-2 text-sm">
      <input v-model="work.isFeatured" type="checkbox" class="h-4 w-4 accent-accent" />
      精選作品（資訊型卡片會以寬版顯示）
    </label>
  </aside>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Schemas } from '@/api/types'
import type { EditableWork } from '@/lib/workDocument'
import FormField from '../FormField.vue'
import TagInput from '../TagInput.vue'
import CoverPicker from './CoverPicker.vue'
import PairRows from './PairRows.vue'

defineProps<{
  username: string
  mode: Schemas['PortfolioMode']
  categories: string[]
  tagSuggestions: string[]
}>()
const work = defineModel<EditableWork>({ required: true })

const sectionTitle = 'font-mono text-xs uppercase tracking-[0.06em] text-muted'

/** 依作品集模式建議的自訂欄位名稱。 */
const fieldSuggestions: Record<Schemas['PortfolioMode'], string[]> = {
  Designer: ['客戶', '媒材', '尺寸', '製作'],
  Frontend: ['技術', '平台', '團隊'],
  Backend: ['技術', '規模', '架構', '團隊'],
}

/** 空白的年份欄位代表不填。 */
const year = computed({
  get: () => work.value.year ?? '',
  set: (value: number | string) => (work.value.year = typeof value === 'number' ? value : null),
})
</script>
