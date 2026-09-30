<template>
  <FormField v-slot="{ id }" label="語言">
    <input
      :id="id"
      v-model.trim="block.language"
      class="input max-w-60"
      maxlength="30"
      placeholder="例如 C#、Go、SQL"
    />
  </FormField>
  <FormField v-slot="{ id }" label="程式碼">
    <textarea
      :id="id"
      v-model="block.code"
      class="min-h-32 w-full resize-y rounded-lg border border-rule bg-[#12151b] px-3 py-2 font-mono text-[0.82rem] leading-relaxed text-[#dfe3ea] focus:outline-none focus:ring-2 focus:ring-accent/40"
      maxlength="20000"
      spellcheck="false"
    />
  </FormField>
  <FormField v-slot="{ id }" label="說明">
    <input :id="id" v-model="block.caption" class="input" maxlength="300" />
  </FormField>
</template>

<script setup lang="ts">
import type { EditableBlock } from '@/lib/workDocument'
import FormField from '../../FormField.vue'

const block = defineModel<Extract<EditableBlock, { type: 'code' }>>({ required: true })
</script>
