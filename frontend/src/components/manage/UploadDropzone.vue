<template>
  <label
    :class="[
      'flex cursor-pointer flex-col items-center justify-center gap-1.5 rounded-card border-[1.5px] border-dashed p-3 text-center text-[0.85rem] transition-colors',
      compact ? 'min-h-[4.5rem]' : 'min-h-36',
      over
        ? 'border-accent bg-accent/10 text-accent'
        : 'border-rule text-muted hover:border-accent hover:bg-accent/10 hover:text-accent',
    ]"
    @dragover.prevent="over = true"
    @dragleave="over = false"
    @drop.prevent="onDrop"
  >
    <input
      type="file"
      class="sr-only"
      :accept="accept"
      :multiple="multiple"
      :disabled="uploads.length > 0"
      @change="onPick"
    />
    <template v-if="uploads.length">
      <b>上傳中…</b>
      <span v-for="upload in uploads" :key="upload.name" class="w-full max-w-60 truncate">
        {{ upload.name }} · {{ Math.round(upload.progress * 100) }}%
      </span>
    </template>
    <template v-else>
      <b>{{ label }}</b>
      <span>{{ hint }}</span>
    </template>
  </label>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { filesApi } from '@/api/files'
import type { Schemas } from '@/api/types'
import { errorMessage } from '@/composables/useAsyncAction'
import { useToastStore } from '@/stores/toast'

withDefaults(
  defineProps<{
    accept: string
    label: string
    hint: string
    multiple?: boolean
    compact?: boolean
  }>(),
  { multiple: true, compact: false },
)
const emit = defineEmits<{ uploaded: [files: Schemas['FileDto'][]] }>()

const over = ref(false)
const uploads = ref<{ name: string; progress: number }[]>([])
const toasts = useToastStore()

function onPick(event: Event) {
  const input = event.target as HTMLInputElement
  void upload([...(input.files ?? [])])
  input.value = ''
}

function onDrop(event: DragEvent) {
  over.value = false
  void upload([...(event.dataTransfer?.files ?? [])])
}

/** 依序上傳（避免同時送出多個大檔），失敗的檔案顯示原因、其餘照常加入。 */
async function upload(files: File[]) {
  if (!files.length) return
  uploads.value = files.map((f) => ({ name: f.name, progress: 0 }))
  const done: Schemas['FileDto'][] = []
  for (const [i, file] of files.entries()) {
    try {
      done.push(await filesApi.upload(file, (ratio) => (uploads.value[i]!.progress = ratio)))
    } catch (e) {
      toasts.error(`「${file.name}」上傳失敗：${errorMessage(e)}`)
    }
  }
  uploads.value = []
  if (done.length) emit('uploaded', done)
}
</script>
