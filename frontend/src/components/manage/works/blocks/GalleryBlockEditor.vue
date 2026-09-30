<template>
  <div class="flex flex-col gap-1.5">
    <span class="text-sm font-medium">排列</span>
    <SegmentedControl v-model="block.layout" :options="layouts" label="圖庫排列" />
    <p class="text-xs text-muted">「原比例」不裁切圖片，適合直式海報、插畫、攝影與長截圖。</p>
  </div>

  <div class="grid grid-cols-[repeat(auto-fill,minmax(11rem,1fr))] gap-3">
    <div
      v-for="(item, i) in block.items"
      :key="`${item.fileId ?? item.url}-${i}`"
      class="flex flex-col overflow-hidden rounded-card border border-rule bg-paper"
    >
      <div class="relative aspect-[4/3] bg-soft">
        <img :src="item.url" alt="" class="h-full w-full object-cover" />
        <span
          class="absolute left-1.5 top-1.5 rounded bg-black/70 px-1.5 font-mono text-[0.72rem] text-white"
        >
          {{ i + 1 }}
        </span>
        <div class="absolute right-1.5 top-1.5 flex gap-1">
          <button
            v-for="action in itemActions(i)"
            :key="action.label"
            type="button"
            class="grid h-[26px] w-[26px] place-items-center rounded-full bg-black/70 text-sm leading-none text-white disabled:opacity-30"
            :aria-label="`${action.label}第 ${i + 1} 張`"
            :disabled="action.disabled"
            @click="action.run()"
          >
            {{ action.icon }}
          </button>
        </div>
      </div>
      <label class="sr-only" :for="`${uid}-cap-${i}`">第 {{ i + 1 }} 張的說明</label>
      <input
        :id="`${uid}-cap-${i}`"
        v-model="item.caption"
        class="border-t border-rule bg-transparent px-2.5 py-2 text-[0.83rem] focus:outline-none focus:ring-2 focus:ring-inset focus:ring-accent"
        maxlength="300"
        placeholder="為這張圖寫一句說明"
      />
      <label class="sr-only" :for="`${uid}-alt-${i}`">第 {{ i + 1 }} 張的替代文字</label>
      <input
        :id="`${uid}-alt-${i}`"
        v-model="item.alt"
        class="border-t border-rule bg-transparent px-2.5 py-1.5 text-[0.78rem] text-muted focus:outline-none focus:ring-2 focus:ring-inset focus:ring-accent"
        maxlength="300"
        placeholder="替代文字（描述圖片內容）"
      />
    </div>
    <UploadDropzone
      :accept="imageAccept"
      label="＋ 加入圖片"
      :hint="imageHint"
      @uploaded="(files) => block.items.push(...files.map(imageFromFile))"
    />
  </div>
</template>

<script setup lang="ts">
import { useId } from 'vue'
import { imageAccept, imageHint } from '@/lib/fileTypes'
import { imageFromFile, type EditableBlock } from '@/lib/workDocument'
import SegmentedControl from '../../SegmentedControl.vue'
import UploadDropzone from '../../UploadDropzone.vue'

const block = defineModel<Extract<EditableBlock, { type: 'gallery' }>>({ required: true })
const uid = useId()

const layouts = [
  { value: 'masonry', label: '原比例' },
  { value: 'cols-2', label: '兩欄' },
  { value: 'cols-3', label: '三欄' },
  { value: 'stack', label: '上下堆疊' },
]

function moveItem(from: number, to: number) {
  const items = block.value.items
  items.splice(to, 0, items.splice(from, 1)[0]!)
}

const itemActions = (i: number) => [
  { label: '往前移', icon: '‹', disabled: i === 0, run: () => moveItem(i, i - 1) },
  {
    label: '往後移',
    icon: '›',
    disabled: i === block.value.items.length - 1,
    run: () => moveItem(i, i + 1),
  },
  { label: '移除', icon: '×', disabled: false, run: () => block.value.items.splice(i, 1) },
]
</script>
