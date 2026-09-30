<template>
  <FormField v-slot="{ id }" label="區塊標題">
    <input
      :id="id"
      v-model="block.title"
      class="input"
      maxlength="200"
      placeholder="例如：相關文件、試閱"
    />
  </FormField>
  <div class="flex flex-col gap-2">
    <div
      v-for="(file, i) in block.items"
      :key="file.fileId"
      class="grid grid-cols-[minmax(0,1fr)_auto] items-center gap-3 rounded-card border border-rule bg-paper px-3 py-2.5"
    >
      <div class="flex min-w-0 flex-col gap-1">
        <b class="break-words text-[0.88rem] font-medium">
          {{ file.fileName }}
          <span class="font-normal text-muted"
            >· {{ file.kind.toUpperCase() }} · {{ formatFileSize(file.size) }}</span
          >
        </b>
        <label class="sr-only" :for="`${uid}-${i}`">{{ file.fileName }} 的說明</label>
        <input
          :id="`${uid}-${i}`"
          v-model="file.description"
          class="input py-1 text-[0.83rem]"
          maxlength="300"
          placeholder="一句話說明這份文件"
        />
      </div>
      <button
        type="button"
        class="text-sm text-muted hover:text-danger"
        @click="block.items.splice(i, 1)"
      >
        移除
      </button>
    </div>
    <UploadDropzone
      compact
      :accept="documentAccept"
      label="＋ 上傳文件"
      :hint="documentHint"
      @uploaded="addFiles"
    />
  </div>
</template>

<script setup lang="ts">
import { useId } from 'vue'
import type { Schemas } from '@/api/types'
import { documentAccept, documentHint } from '@/lib/fileTypes'
import { formatFileSize } from '@/lib/format'
import type { EditableBlock } from '@/lib/workDocument'
import FormField from '../../FormField.vue'
import UploadDropzone from '../../UploadDropzone.vue'

const block = defineModel<Extract<EditableBlock, { type: 'files' }>>({ required: true })
const uid = useId()

function addFiles(files: Schemas['FileDto'][]) {
  block.value.items.push(
    ...files.map((f) => ({
      fileId: f.id,
      url: f.url,
      fileName: f.fileName,
      kind: f.kind,
      size: f.size,
      description: '',
    })),
  )
}
</script>
