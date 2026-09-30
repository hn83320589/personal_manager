<template>
  <router-link
    v-if="item.route"
    :to="item.route"
    :class="itemClasses"
    @click="handleClick"
  >
    <component
      :is="item.icon"
      v-if="item.icon"
      class="h-5 w-5 flex-shrink-0"
      :class="iconClasses"
    />
    <span v-if="!collapsed" class="truncate">{{ item.name }}</span>
    <span
      v-if="item.badge && !collapsed"
      class="ml-auto inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-primary-100 text-primary-800"
    >
      {{ item.badge }}
    </span>
  </router-link>

  <button
    v-else
    :class="itemClasses"
    @click="handleClick"
  >
    <component
      :is="item.icon"
      v-if="item.icon"
      class="h-5 w-5 flex-shrink-0"
      :class="iconClasses"
    />
    <span v-if="!collapsed" class="truncate">{{ item.name }}</span>
    <span
      v-if="item.badge && !collapsed"
      class="ml-auto inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-primary-100 text-primary-800"
    >
      {{ item.badge }}
    </span>
  </button>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import type { MenuItem } from './AppSidebar.vue'

interface Props {
  item: MenuItem
  collapsed: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  click: [item: MenuItem]
}>()

const route = useRoute()

const isActive = computed(() => {
  if (props.item.active !== undefined) {
    return props.item.active
  }
  if (props.item.route) {
    return route.path === props.item.route || route.path.startsWith(props.item.route + '/')
  }
  return false
})

const itemClasses = computed(() => {
  const baseClasses = 'flex items-center px-3 py-2 text-sm font-medium rounded-lg transition-colors duration-200'
  const stateClasses = isActive.value
    ? 'bg-primary-100 text-primary-700 border-r-2 border-primary-700'
    : 'text-gray-700 hover:bg-gray-100 hover:text-gray-900'
  const layoutClasses = props.collapsed ? 'justify-center' : 'space-x-3'
  
  return [baseClasses, stateClasses, layoutClasses].join(' ')
})

const iconClasses = computed(() => {
  return isActive.value ? 'text-primary-700' : 'text-gray-400'
})

function handleClick() {
  emit('click', props.item)
}
</script>