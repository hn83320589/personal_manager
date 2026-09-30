<template>
  <article class="flex flex-col overflow-hidden rounded-card border border-rule bg-surface">
    <a
      :href="file.url"
      target="_blank"
      rel="noopener"
      class="grid aspect-[4/3] place-items-center overflow-hidden bg-soft"
    >
      <img
        v-if="file.kind === 'Image'"
        :src="file.url"
        :alt="file.fileName"
        class="h-full w-full object-cover"
        loading="lazy"
      />
      <span
        v-else
        class="rounded-md bg-muted px-2.5 py-3 font-mono text-xs font-medium text-paper"
        >{{ kindLabel }}</span
      >
    </a>
    <div class="flex flex-1 flex-col gap-1 p-2.5">
      <b class="break-words text-[0.85rem] font-medium">{{ file.fileName }}</b>
      <span class="text-xs text-muted">
        {{
          [
            formatFileSize(file.size),
            file.width ? `${file.width}×${file.height}` : '',
            formatDate(file.createdAt),
          ]
            .filter(Boolean)
            .join(' · ')
        }}
      </span>

      <div
        v-if="state === 'confirm'"
        class="mt-1 flex flex-col gap-1.5 rounded-md bg-danger/10 p-2 text-xs"
        role="alert"
      >
        <template v-if="usages.length">
          <p class="font-medium text-danger">
            這個檔案仍在使用中，刪除後以下內容的圖片或附件會失效：
          </p>
          <ul class="list-inside list-disc">
            <li v-for="usage in usages" :key="`${usage.kind}-${usage.id}`">
              <RouterLink :to="linkOf(usage)" class="hover:underline"
                >{{ usageLabel[usage.kind] }}「{{ usage.title }}」</RouterLink
              >
            </li>
          </ul>
        </template>
        <p v-else class="text-danger">確定刪除這個檔案？</p>
        <div class="flex gap-1.5">
          <button
            type="button"
            class="rounded bg-danger px-2 py-0.5 font-medium text-white"
            @click="confirmDelete"
          >
            {{ usages.length ? '仍要刪除' : '刪除' }}
          </button>
          <button type="button" class="rounded px-2 py-0.5 hover:bg-soft" @click="state = 'idle'">
            取消
          </button>
        </div>
      </div>

      <div v-else class="mt-auto flex items-center justify-between gap-1 pt-1">
        <button type="button" class="text-xs text-muted hover:text-accent" @click="copyLink">
          {{ copied ? '已複製連結' : '複製連結' }}
        </button>
        <button
          type="button"
          class="text-xs text-muted hover:text-danger"
          :disabled="state === 'checking'"
          :aria-label="`刪除「${file.fileName}」`"
          @click="checkUsages"
        >
          {{ state === 'checking' ? '檢查中…' : '刪除' }}
        </button>
      </div>
    </div>
  </article>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import type { RouteLocationRaw } from 'vue-router'
import { filesApi } from '@/api/files'
import type { Schemas } from '@/api/types'
import { errorMessage } from '@/composables/useAsyncAction'
import { formatDate, formatFileSize } from '@/lib/format'
import { useToastStore } from '@/stores/toast'

type Usage = Schemas['FileUsageDto']

const props = defineProps<{ file: Schemas['FileDto'] }>()
const emit = defineEmits<{ remove: [] }>()

const toasts = useToastStore()
const state = ref<'idle' | 'checking' | 'confirm'>('idle')
const usages = ref<Usage[]>([])
const copied = ref(false)

const kindLabel = computed(
  () =>
    ({ Pdf: 'PDF', Word: 'DOC', PowerPoint: 'PPT', Excel: 'XLS', Archive: 'ZIP', Image: 'IMG' })[
      props.file.kind
    ],
)

const usageLabel: Record<Schemas['FileUsageKind'], string> = {
  Portfolio: '作品',
  Post: '文章',
  Profile: '',
}

function linkOf(usage: Usage): RouteLocationRaw {
  if (usage.kind === 'Portfolio') return `/admin/works/${usage.id}`
  if (usage.kind === 'Post') return `/admin/blog/${usage.id}`
  return '/admin/profile'
}

/** 刪除前先查詢使用中的內容，讓使用者知道會影響哪些地方。 */
async function checkUsages() {
  state.value = 'checking'
  try {
    usages.value = await filesApi.usages(props.file.id)
    state.value = 'confirm'
  } catch (e) {
    toasts.error(errorMessage(e))
    state.value = 'idle'
  }
}

function confirmDelete() {
  state.value = 'idle'
  emit('remove')
}

async function copyLink() {
  try {
    await navigator.clipboard.writeText(new URL(props.file.url, window.location.origin).href)
    copied.value = true
    setTimeout(() => (copied.value = false), 2000)
  } catch {
    toasts.error('無法存取剪貼簿，請直接開啟檔案後複製網址')
  }
}
</script>
