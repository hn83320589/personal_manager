<template>
  <FormField v-slot="{ id }" label="區塊標題">
    <input :id="id" v-model="block.title" class="input" maxlength="200" placeholder="例如：成果" />
  </FormField>
  <div class="grid grid-cols-[repeat(auto-fill,minmax(12rem,1fr))] gap-2.5">
    <div
      v-for="(metric, i) in block.items"
      :key="i"
      class="flex flex-col gap-1.5 rounded-card border border-rule bg-paper p-2.5"
    >
      <label class="sr-only" :for="`${uid}-v-${i}`">第 {{ i + 1 }} 個數字</label>
      <input
        :id="`${uid}-v-${i}`"
        v-model="metric.value"
        class="input font-latin"
        maxlength="30"
        placeholder="例如 −62%"
      />
      <label class="sr-only" :for="`${uid}-l-${i}`">第 {{ i + 1 }} 個數字的說明</label>
      <input
        :id="`${uid}-l-${i}`"
        v-model="metric.label"
        class="input"
        maxlength="100"
        placeholder="例如 P95 延遲"
      />
      <button
        type="button"
        class="self-start text-xs text-muted hover:text-danger"
        @click="block.items.splice(i, 1)"
      >
        移除
      </button>
    </div>
    <button
      v-if="block.items.length < maxMetrics"
      type="button"
      class="flex min-h-[4.5rem] items-center justify-center rounded-card border-[1.5px] border-dashed border-rule text-[0.85rem] font-bold text-muted hover:border-accent hover:text-accent"
      @click="block.items.push({ value: '', label: '' })"
    >
      ＋ 新增一個數字
    </button>
  </div>
  <p class="text-xs text-muted">
    最多 {{ maxMetrics }} 個。第一個數據區塊的數字也會顯示在作品卡片上。
  </p>
</template>

<script setup lang="ts">
import { useId } from 'vue'
import type { EditableBlock } from '@/lib/workDocument'
import FormField from '../../FormField.vue'

const block = defineModel<Extract<EditableBlock, { type: 'metrics' }>>({ required: true })
const uid = useId()
const maxMetrics = 8
</script>
