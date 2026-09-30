<template>
  <UploadDropzone
    v-if="!block.image"
    :accept="imageAccept"
    label="＋ 上傳一張圖片"
    hint="JPG、PNG、WebP、GIF"
    :multiple="false"
    @uploaded="(files) => (block.image = imageFromFile(files[0]!))"
  />
  <div v-else class="grid items-start gap-3.5 min-[521px]:grid-cols-[9rem_1fr]">
    <div class="flex flex-col gap-2">
      <div class="aspect-[4/3] overflow-hidden rounded-lg border border-rule bg-soft">
        <img :src="block.image.url" alt="" class="h-full w-full object-cover" />
      </div>
      <button
        type="button"
        class="text-left text-xs text-muted hover:text-danger"
        @click="block.image = null"
      >
        換一張圖片
      </button>
    </div>
    <div class="flex flex-col gap-2.5">
      <FormField v-slot="{ id }" label="圖片說明">
        <input :id="id" v-model="block.image.caption" class="input" maxlength="300" />
      </FormField>
      <FormField
        v-slot="{ id }"
        label="替代文字"
        hint="描述圖片內容，給看不到圖片的訪客（例如使用螢幕閱讀器）。"
      >
        <input :id="id" v-model="block.image.alt" class="input" maxlength="300" />
      </FormField>
      <div class="flex flex-col gap-1.5">
        <span class="text-sm font-medium">版面</span>
        <SegmentedControl v-model="block.layout" :options="layouts" label="圖片版面" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { imageAccept } from '@/lib/fileTypes'
import { imageFromFile, type EditableBlock } from '@/lib/workDocument'
import FormField from '../../FormField.vue'
import SegmentedControl from '../../SegmentedControl.vue'
import UploadDropzone from '../../UploadDropzone.vue'

const block = defineModel<Extract<EditableBlock, { type: 'image' }>>({ required: true })

const layouts = [
  { value: 'narrow', label: '與文字同寬' },
  { value: 'wide', label: '寬版' },
  { value: 'bleed', label: '滿版出血' },
]
</script>
