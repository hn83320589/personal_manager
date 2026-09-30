<template>
  <div class="flex flex-col gap-1.5">
    <label :for="id" class="text-sm font-medium">
      {{ label }}
      <span v-if="required" class="text-danger" aria-hidden="true">*</span>
    </label>
    <slot :id="id" :described-by="hint ? hintId : undefined" />
    <p v-if="hint" :id="hintId" class="text-xs text-muted">{{ hint }}</p>
  </div>
</template>

<script setup lang="ts">
import { useId } from 'vue'

defineProps<{ label: string; hint?: string; required?: boolean }>()

const id = useId()
const hintId = `${id}-hint`
</script>
