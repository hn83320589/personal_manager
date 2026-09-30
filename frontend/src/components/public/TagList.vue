<template>
  <ul v-if="tags.length" class="flex flex-wrap gap-1.5" aria-label="標籤">
    <li
      v-for="tag in tags"
      :key="tag"
      class="whitespace-nowrap rounded-full bg-soft px-2.5 py-0.5 text-[0.78rem] text-muted"
    >
      {{ tag }}
    </li>
  </ul>
</template>

<script setup lang="ts">
defineProps<{ tags: readonly string[] }>()
</script>
