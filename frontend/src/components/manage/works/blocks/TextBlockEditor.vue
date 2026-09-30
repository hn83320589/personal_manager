<template>
  <FormField v-slot="{ id }" label="小標題（可留空）">
    <input :id="id" v-model="block.title" class="input" maxlength="200" />
  </FormField>
  <div class="flex flex-col gap-1.5">
    <span class="text-sm font-medium">內文</span>
    <RichTextEditor v-model="block.html" label="內文" placeholder="說明背景、做法或成果…" />
  </div>
</template>

<script setup lang="ts">
import type { EditableBlock } from '@/lib/workDocument'
import FormField from '../../FormField.vue'
import RichTextEditor from '../../RichTextEditor.vue'

const block = defineModel<Extract<EditableBlock, { type: 'text' }>>({ required: true })
</script>
