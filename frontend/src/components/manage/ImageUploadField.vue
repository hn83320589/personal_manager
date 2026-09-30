<template>
  <div :class="['flex gap-4', shape === 'wide' ? 'flex-col items-start' : 'items-center']">
    <div
      :class="[
        'grid shrink-0 place-items-center overflow-hidden border border-rule bg-soft text-muted',
        shapeClass,
      ]"
    >
      <img v-if="modelValue" :src="modelValue" alt="" class="h-full w-full object-cover" />
      <PhotoIcon v-else class="h-6 w-6" aria-hidden="true" />
    </div>
    <div class="flex flex-col gap-1.5">
      <div class="flex flex-wrap gap-2">
        <label class="btn btn-small cursor-pointer">
          {{
            uploading
              ? `上傳中 ${Math.round(progress * 100)}%`
              : modelValue
                ? '更換圖片'
                : '上傳圖片'
          }}
          <input
            :id="id"
            type="file"
            accept="image/png,image/jpeg,image/webp,image/gif"
            class="sr-only"
            :disabled="uploading"
            @change="onPick"
          />
        </label>
        <button
          v-if="modelValue"
          type="button"
          class="btn btn-small"
          @click="$emit('update:modelValue', '')"
        >
          移除
        </button>
      </div>
      <p class="text-xs text-muted">JPG、PNG、WebP、GIF</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { PhotoIcon } from '@heroicons/vue/24/outline'
import { filesApi } from '@/api/files'
import { errorMessage } from '@/composables/useAsyncAction'
import { useToastStore } from '@/stores/toast'

const props = withDefaults(
  defineProps<{ modelValue: string; id?: string; shape?: 'square' | 'wide' }>(),
  {
    id: undefined,
    shape: 'square',
  },
)
const emit = defineEmits<{ 'update:modelValue': [url: string] }>()

const shapeClass = computed(() =>
  props.shape === 'wide' ? 'aspect-[16/9] w-full rounded-lg' : 'h-20 w-20 rounded-2xl',
)
const uploading = ref(false)
const progress = ref(0)
const toasts = useToastStore()

async function onPick(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  input.value = ''
  if (!file) return
  uploading.value = true
  progress.value = 0
  try {
    const uploaded = await filesApi.upload(file, (ratio) => (progress.value = ratio))
    emit('update:modelValue', uploaded.url)
  } catch (e) {
    toasts.error(errorMessage(e))
  } finally {
    uploading.value = false
  }
}
</script>
