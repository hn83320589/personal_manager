<template>
  <ul class="flex flex-wrap gap-x-6 gap-y-2" aria-label="重點數字">
    <li v-for="(metric, i) in metrics" :key="i" class="text-[0.85rem] text-muted">
      <b class="mr-1.5 font-latin text-lg tabular-nums text-ink">{{ metric.value }}</b>{{ metric.label }}
    </li>
  </ul>
</template>

<script setup lang="ts">
import type { Schemas } from '@/api/types'

defineProps<{ metrics: Schemas['PortfolioMetric'][] }>()
</script>
