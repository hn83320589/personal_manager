<template>
  <div :class="gridClass[variant]">
    <WorkCard
      v-for="work in works"
      :key="work.slug"
      :work="work"
      :username="username"
      :variant="variant"
      :ratio="ratio"
    />
  </div>
</template>

<script setup lang="ts">
import type { Schemas } from '@/api/types'
import WorkCard from './WorkCard.vue'
import { gridClass, type CardRatio, type CardStyle } from './workCards'

defineProps<{
  works: Schemas['PortfolioCardDto'][]
  username: string
  variant: CardStyle
  ratio: CardRatio
}>()
</script>
