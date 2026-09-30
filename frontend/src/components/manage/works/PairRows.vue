<template>
  <div class="flex flex-col gap-1.5">
    <div
      v-for="(row, i) in rows"
      :key="i"
      class="grid grid-cols-[6.5rem_minmax(0,1fr)_auto] items-center gap-1.5"
    >
      <label class="sr-only" :for="`${uid}-k-${i}`">{{ keyLabel }}</label>
      <input
        :id="`${uid}-k-${i}`"
        v-model="row[keyField]"
        class="input px-2 py-1.5 text-[0.88rem]"
        :placeholder="keyLabel"
        maxlength="50"
      />
      <label class="sr-only" :for="`${uid}-v-${i}`">{{ valueLabel }}</label>
      <input
        :id="`${uid}-v-${i}`"
        v-model="row[valueField]"
        class="input px-2 py-1.5 text-[0.88rem]"
        :type="valueType"
        :placeholder="valuePlaceholder"
        maxlength="200"
      />
      <button
        type="button"
        class="rounded-md px-1.5 py-1 text-muted hover:text-danger"
        :aria-label="`移除第 ${i + 1} 列`"
        @click="rows.splice(i, 1)"
      >
        ×
      </button>
    </div>
    <div class="flex flex-wrap items-center gap-1.5">
      <button
        type="button"
        class="text-sm text-accent hover:underline"
        :disabled="rows.length >= max"
        @click="add('')"
      >
        ＋ 新增
      </button>
      <button
        v-for="suggestion in unusedSuggestions"
        :key="suggestion"
        type="button"
        class="rounded-full border border-dashed border-rule px-2.5 py-0.5 text-xs text-muted hover:border-accent hover:text-accent"
        @click="add(suggestion)"
      >
        + {{ suggestion }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts" generic="K extends string, V extends string">
import { computed, useId } from 'vue'

const props = withDefaults(
  defineProps<{
    keyField: K
    valueField: V
    keyLabel: string
    valueLabel: string
    valuePlaceholder?: string
    valueType?: 'text' | 'url'
    suggestions?: string[]
    max: number
  }>(),
  { valuePlaceholder: '', valueType: 'text', suggestions: () => [] },
)

const rows = defineModel<Record<K | V, string>[]>({ required: true })
const uid = useId()

const unusedSuggestions = computed(() =>
  props.suggestions.filter((s) => !rows.value.some((row) => row[props.keyField] === s)),
)

function add(key: string) {
  if (rows.value.length >= props.max) return
  rows.value.push({ [props.keyField]: key, [props.valueField]: '' } as Record<K | V, string>)
}
</script>
