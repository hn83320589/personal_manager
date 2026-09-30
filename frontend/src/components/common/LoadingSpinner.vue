<template>
  <div class="flex items-center justify-center" :class="containerClass">
    <div class="animate-spin rounded-full border-4 border-gray-200" :class="spinnerClass">
      <div class="h-full w-full rounded-full border-4 border-primary-500 border-r-transparent"></div>
    </div>
    <span v-if="text" class="ml-3 text-gray-600">{{ text }}</span>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  size?: 'small' | 'medium' | 'large'
  text?: string
  fullScreen?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  size: 'medium',
  text: '',
  fullScreen: false,
})

const containerClass = computed(() => {
  return props.fullScreen ? 'fixed inset-0 z-50 bg-white bg-opacity-75' : ''
})

const spinnerClass = computed(() => {
  switch (props.size) {
    case 'small':
      return 'h-4 w-4'
    case 'large':
      return 'h-12 w-12'
    default:
      return 'h-8 w-8'
  }
})
</script>