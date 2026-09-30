<template>
  <aside
    class="flex flex-col gap-3.5 rounded-xl border border-rule bg-surface p-[18px]"
    aria-label="發佈設定"
  >
    <h2 :class="sectionTitle">發佈</h2>
    <div class="flex flex-col gap-1.5">
      <span class="text-sm font-medium">狀態</span>
      <SegmentedControl v-model="post.mode" :options="modes" label="發佈狀態" />
      <p class="text-xs text-muted">{{ modeHint }}</p>
    </div>
    <FormField v-if="post.mode === 'scheduled'" v-slot="{ id }" label="發佈時間">
      <input :id="id" v-model="post.scheduledAt" type="datetime-local" class="input" />
    </FormField>

    <hr class="border-rule" />
    <h2 :class="sectionTitle">封面圖</h2>
    <ImageUploadField v-model="post.coverImageUrl" shape="wide" />
    <p class="text-xs text-muted">文章列表與分享預覽會使用這張圖。</p>

    <hr class="border-rule" />
    <h2 :class="sectionTitle">文章資訊</h2>
    <FormField v-slot="{ id }" label="摘要" hint="顯示在文章列表與搜尋結果，留空則不顯示。">
      <textarea :id="id" v-model="post.summary" class="input min-h-20" maxlength="500" />
    </FormField>
    <FormField v-slot="{ id }" label="分類">
      <input
        :id="id"
        v-model.trim="post.category"
        class="input"
        maxlength="50"
        :list="`${id}-list`"
      />
      <datalist :id="`${id}-list`">
        <option v-for="c in categories" :key="c" :value="c" />
      </datalist>
    </FormField>
    <FormField v-slot="{ id }" label="標籤" hint="會提示你用過的標籤，也可以輸入新的。">
      <TagInput :id="id" v-model="post.tags" :suggestions="tagSuggestions" />
    </FormField>
    <FormField v-slot="{ id }" label="網址" :hint="`/@${username}/blog/${post.slug || '…'}`">
      <input
        :id="id"
        v-model.trim="post.slug"
        class="input font-mono text-sm"
        maxlength="80"
        pattern="[a-z0-9]+(-[a-z0-9]+)*"
      />
    </FormField>
  </aside>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { EditablePost, PublishMode } from '@/lib/postDocument'
import FormField from '../FormField.vue'
import ImageUploadField from '../ImageUploadField.vue'
import SegmentedControl from '../SegmentedControl.vue'
import TagInput from '../TagInput.vue'

defineProps<{ username: string; categories: string[]; tagSuggestions: string[] }>()
const post = defineModel<EditablePost>({ required: true })

const sectionTitle = 'font-mono text-xs uppercase tracking-[0.06em] text-muted'

const modes: { value: PublishMode; label: string }[] = [
  { value: 'draft', label: '草稿' },
  { value: 'published', label: '已發佈' },
  { value: 'scheduled', label: '排程' },
]

const modeHint = computed(
  () =>
    ({
      draft: '只有你看得到。',
      published: '已公開在你的文章列表。修改會自動儲存並立即更新。',
      scheduled: '到了發佈時間會自動公開。',
    })[post.value.mode],
)
</script>
