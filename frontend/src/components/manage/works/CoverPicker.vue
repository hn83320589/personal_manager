<template>
  <div class="flex flex-col gap-2.5">
    <p class="text-xs text-muted">第 1 張為主圖；多張時作品卡片會自動輪播。</p>
    <div v-if="covers.length" class="grid grid-cols-3 gap-2">
      <div
        v-for="(cover, i) in covers"
        :key="`${cover.url}-${i}`"
        class="relative aspect-square overflow-hidden rounded-lg border border-rule bg-soft"
      >
        <img
          :src="cover.url"
          alt=""
          class="h-full w-full object-cover"
          :style="{ objectPosition: focus }"
        />
        <span
          class="absolute left-1 top-1 grid h-5 min-w-5 place-items-center rounded-full bg-accent px-1 font-mono text-[0.7rem] text-accent-ink"
        >
          {{ i + 1 }}
        </span>
        <div class="absolute inset-x-1 bottom-1 flex justify-between">
          <button
            type="button"
            :class="miniButton"
            :disabled="i === 0"
            :aria-label="`第 ${i + 1} 張往前`"
            @click="move(i, -1)"
          >
            ‹
          </button>
          <button
            type="button"
            :class="miniButton"
            :aria-label="`移除第 ${i + 1} 張封面`"
            @click="covers.splice(i, 1)"
          >
            ×
          </button>
          <button
            type="button"
            :class="miniButton"
            :disabled="i === covers.length - 1"
            :aria-label="`第 ${i + 1} 張往後`"
            @click="move(i, 1)"
          >
            ›
          </button>
        </div>
      </div>
    </div>

    <UploadDropzone
      compact
      :accept="imageAccept"
      label="＋ 上傳封面"
      :hint="imageHint"
      @uploaded="(files) => add(files.map(imageFromFile))"
    />

    <div v-if="candidates.length" class="flex flex-col gap-1.5">
      <span class="text-xs text-muted">或從內容中的圖片挑選：</span>
      <div class="grid grid-cols-5 gap-1.5">
        <button
          v-for="candidate in candidates"
          :key="candidate.url"
          type="button"
          class="aspect-square overflow-hidden rounded-md border border-rule hover:ring-2 hover:ring-accent"
          :aria-label="`設為封面：${candidate.alt || candidate.caption || '內容圖片'}`"
          @click="add([{ ...candidate }])"
        >
          <img :src="candidate.url" alt="" class="h-full w-full object-cover" />
        </button>
      </div>
    </div>

    <div v-if="covers.length" class="flex items-center gap-3">
      <div
        class="grid w-[5.5rem] grid-cols-3 gap-1"
        role="radiogroup"
        aria-label="裁切時保留的位置"
      >
        <button
          v-for="point in focusPoints"
          :key="point.value"
          type="button"
          role="radio"
          :aria-checked="focus === point.value"
          :aria-label="point.label"
          class="aspect-square rounded border border-rule bg-paper aria-checked:border-accent aria-checked:bg-accent"
          @click="focus = point.value"
        />
      </div>
      <p class="text-xs text-muted">卡片比例與圖片不同時，保留圖片的哪個位置。</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { imageAccept, imageHint } from '@/lib/fileTypes'
import { imageFromFile, type EditableBlock, type EditableImage } from '@/lib/workDocument'
import UploadDropzone from '../UploadDropzone.vue'

const props = defineProps<{ blocks: EditableBlock[] }>()
const covers = defineModel<EditableImage[]>('covers', { required: true })
const focus = defineModel<string>('focus', { required: true })

const maxCovers = 10
const miniButton =
  'grid h-6 w-6 place-items-center rounded-full bg-black/65 text-sm leading-none text-white disabled:opacity-0'

const rows = ['上', '中', '下']
const cols = ['左', '中', '右']
const focusPoints = rows.flatMap((row, r) =>
  cols.map((col, c) => ({
    value: `${c * 50}% ${r * 50}%`,
    label: row === '中' && col === '中' ? '正中央' : `${row}${col}`,
  })),
)

/** 內容區塊中已經有、但還不是封面的圖片。 */
const candidates = computed(() => {
  const used = new Set(covers.value.map((c) => c.url))
  const images = props.blocks.flatMap((b) =>
    b.type === 'image' && b.image ? [b.image] : b.type === 'gallery' ? b.items : [],
  )
  return images.filter(
    (image, i) => !used.has(image.url) && images.findIndex((x) => x.url === image.url) === i,
  )
})

function add(images: EditableImage[]) {
  covers.value.push(...images.slice(0, maxCovers - covers.value.length))
}

function move(i: number, delta: -1 | 1) {
  covers.value.splice(i + delta, 0, covers.value.splice(i, 1)[0]!)
}
</script>
