<template>
  <form :class="formClasses" @submit="handleSubmit">
    <slot />
    
    <div v-if="showActions" :class="actionsClasses">
      <slot name="actions" :submit="handleSubmit" :reset="handleReset">
        <div class="flex justify-end space-x-3">
          <BaseButton
            v-if="showResetButton"
            type="button"
            variant="secondary"
            @click="handleReset"
          >
            {{ resetText }}
          </BaseButton>
          
          <BaseButton
            type="submit"
            :variant="submitVariant"
            :loading="loading"
            :disabled="disabled"
          >
            {{ submitText }}
          </BaseButton>
        </div>
      </slot>
    </div>
  </form>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import BaseButton from './BaseButton.vue'

interface Props {
  layout?: 'vertical' | 'horizontal' | 'inline'
  showActions?: boolean
  showResetButton?: boolean
  submitText?: string
  resetText?: string
  submitVariant?: 'primary' | 'secondary' | 'success' | 'danger'
  loading?: boolean
  disabled?: boolean
  spacing?: 'compact' | 'normal' | 'relaxed'
}

const props = withDefaults(defineProps<Props>(), {
  layout: 'vertical',
  showActions: true,
  showResetButton: false,
  submitText: 'Submit',
  resetText: 'Reset',
  submitVariant: 'primary',
  loading: false,
  disabled: false,
  spacing: 'normal'
})

const emit = defineEmits<{
  submit: [event: Event]
  reset: [event: Event]
}>()

const formClasses = computed(() => {
  const baseClasses = 'w-full'
  
  const layoutClasses = {
    vertical: 'space-y-4',
    horizontal: 'grid grid-cols-1 md:grid-cols-2 gap-4',
    inline: 'flex flex-wrap items-end gap-4'
  }
  
  const spacingClasses = {
    compact: props.layout === 'vertical' ? 'space-y-2' : 'gap-2',
    normal: props.layout === 'vertical' ? 'space-y-4' : 'gap-4',
    relaxed: props.layout === 'vertical' ? 'space-y-6' : 'gap-6'
  }
  
  return [
    baseClasses,
    layoutClasses[props.layout],
    spacingClasses[props.spacing]
  ].join(' ')
})

const actionsClasses = computed(() => {
  const baseClasses = 'pt-6 border-t border-gray-200'
  return baseClasses
})

function handleSubmit(event: Event) {
  event.preventDefault()
  if (!props.disabled && !props.loading) {
    emit('submit', event)
  }
}

function handleReset(event: Event) {
  emit('reset', event)
}
</script>