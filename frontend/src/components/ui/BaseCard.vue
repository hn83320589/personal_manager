<template>
  <div :class="cardClasses">
    <!-- Card Header -->
    <div v-if="$slots.header || title || subtitle" :class="headerClasses">
      <slot name="header">
        <div class="flex items-center justify-between">
          <div>
            <h3 v-if="title" :class="titleClasses">{{ title }}</h3>
            <p v-if="subtitle" :class="subtitleClasses">{{ subtitle }}</p>
          </div>
          <div v-if="$slots.actions" class="flex items-center space-x-2">
            <slot name="actions" />
          </div>
        </div>
      </slot>
    </div>

    <!-- Card Body -->
    <div :class="bodyClasses">
      <slot />
    </div>

    <!-- Card Footer -->
    <div v-if="$slots.footer" :class="footerClasses">
      <slot name="footer" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  title?: string
  subtitle?: string
  variant?: 'default' | 'outlined' | 'elevated' | 'filled'
  size?: 'small' | 'medium' | 'large'
  padding?: 'none' | 'small' | 'medium' | 'large'
  hoverable?: boolean
  clickable?: boolean
  loading?: boolean
  borderRadius?: 'none' | 'small' | 'medium' | 'large' | 'full'
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'default',
  size: 'medium',
  padding: 'medium',
  hoverable: false,
  clickable: false,
  loading: false,
  borderRadius: 'medium'
})

const emit = defineEmits<{
  click: [event: MouseEvent]
}>()

const cardClasses = computed(() => {
  const baseClasses = 'bg-white overflow-hidden transition-all duration-200'
  
  const variantClasses = {
    default: 'border border-gray-200',
    outlined: 'border-2 border-gray-300',
    elevated: 'shadow-lg border border-gray-100',
    filled: 'bg-gray-50 border-0'
  }
  
  const borderRadiusClasses = {
    none: 'rounded-none',
    small: 'rounded-sm',
    medium: 'rounded-lg',
    large: 'rounded-xl',
    full: 'rounded-2xl'
  }
  
  const interactionClasses = []
  if (props.hoverable) {
    interactionClasses.push('hover:shadow-md hover:border-gray-300')
  }
  if (props.clickable) {
    interactionClasses.push('cursor-pointer hover:shadow-lg transform hover:-translate-y-1')
  }
  
  const loadingClasses = props.loading ? 'opacity-50 pointer-events-none' : ''
  
  return [
    baseClasses,
    variantClasses[props.variant],
    borderRadiusClasses[props.borderRadius],
    ...interactionClasses,
    loadingClasses
  ].join(' ')
})

const headerClasses = computed(() => {
  const baseClasses = 'border-b border-gray-200'
  
  const paddingClasses = {
    none: 'p-0',
    small: 'p-3',
    medium: 'p-4',
    large: 'p-6'
  }
  
  return [baseClasses, paddingClasses[props.padding]].join(' ')
})

const bodyClasses = computed(() => {
  const paddingClasses = {
    none: 'p-0',
    small: 'p-3',
    medium: 'p-4',
    large: 'p-6'
  }
  
  return paddingClasses[props.padding]
})

const footerClasses = computed(() => {
  const baseClasses = 'border-t border-gray-100 bg-gray-50'
  
  const paddingClasses = {
    none: 'p-0',
    small: 'p-3',
    medium: 'p-4',
    large: 'p-6'
  }
  
  return [baseClasses, paddingClasses[props.padding]].join(' ')
})

const titleClasses = computed(() => {
  const sizeClasses = {
    small: 'text-base',
    medium: 'text-lg',
    large: 'text-xl'
  }
  
  return ['font-semibold text-gray-900', sizeClasses[props.size]].join(' ')
})

const subtitleClasses = computed(() => {
  const sizeClasses = {
    small: 'text-xs',
    medium: 'text-sm',
    large: 'text-base'
  }
  
  return ['text-gray-500 mt-1', sizeClasses[props.size]].join(' ')
})

function handleClick(event: MouseEvent) {
  if (props.clickable) {
    emit('click', event)
  }
}
</script>