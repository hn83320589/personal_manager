<template>
  <div v-if="options.length > 1" class="mt-8 flex flex-wrap gap-2" role="group" :aria-label="label">
    <button
      v-for="option in [allLabel, ...options]"
      :key="option"
      type="button"
      :aria-pressed="option === (modelValue ?? allLabel)"
      class="rounded-full border border-rule bg-surface px-3.5 py-1 text-[0.88rem] text-muted hover:text-ink aria-pressed:border-ink aria-pressed:bg-ink aria-pressed:text-paper"
      @click="$emit('update:modelValue', option === allLabel ? null : option)"
    >
      {{ option }}
    </button>
  </div>
</template>

<script setup lang="ts">
withDefaults(defineProps<{ options: string[]; modelValue: string | null; label: string; allLabel?: string }>(), {
  allLabel: '全部',
})
defineEmits<{ 'update:modelValue': [value: string | null] }>()
</script>
