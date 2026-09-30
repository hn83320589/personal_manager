<template>
  <div class="flex overflow-hidden rounded-lg border border-rule" role="group" :aria-label="label">
    <button
      v-for="option in options"
      :key="option.value"
      type="button"
      :aria-pressed="option.value === modelValue"
      class="flex-1 border-rule bg-paper px-2 py-1.5 text-[0.83rem] text-muted [&+&]:border-l hover:text-ink aria-pressed:bg-ink aria-pressed:text-paper"
      @click="$emit('update:modelValue', option.value)"
    >
      {{ option.label }}
    </button>
  </div>
</template>

<script setup lang="ts" generic="T extends string">
defineProps<{ modelValue: T; options: { value: T; label: string }[]; label: string }>()
defineEmits<{ 'update:modelValue': [value: T] }>()
</script>
