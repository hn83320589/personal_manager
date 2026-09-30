<template>
  <BaseModal
    :show="show"
    :title="title"
    :size="size"
    :closable="closable"
    :close-on-overlay="closeOnOverlay"
    :persistent="persistent"
    @close="close"
    @update:show="$emit('update:show', $event)"
  >
    <template #header>
      <div class="flex items-center">
        <component
          :is="typeIcon"
          v-if="type !== 'default'"
          :class="typeIconClasses"
          class="h-6 w-6 mr-3"
        />
        <div>
          <h3 class="text-lg font-semibold text-gray-900">{{ title }}</h3>
          <p v-if="subtitle" class="text-sm text-gray-500">{{ subtitle }}</p>
        </div>
      </div>
    </template>

    <div class="space-y-4">
      <div v-if="message" :class="messageClasses">
        {{ message }}
      </div>
      
      <slot />
    </div>

    <template #footer>
      <div class="flex justify-end space-x-3">
        <BaseButton
          v-if="showCancelButton"
          variant="secondary"
          @click="handleCancel"
        >
          {{ cancelText }}
        </BaseButton>
        
        <BaseButton
          v-if="showConfirmButton"
          :variant="confirmVariant"
          :loading="loading"
          @click="handleConfirm"
        >
          {{ confirmText }}
        </BaseButton>
      </div>
    </template>
  </BaseModal>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import {
  ExclamationTriangleIcon,
  InformationCircleIcon,
  CheckCircleIcon,
  XCircleIcon,
} from '@heroicons/vue/24/outline'
import BaseModal from './BaseModal.vue'
import BaseButton from './BaseButton.vue'

interface Props {
  show: boolean
  type?: 'default' | 'info' | 'success' | 'warning' | 'danger'
  title?: string
  subtitle?: string
  message?: string
  size?: 'small' | 'medium' | 'large' | 'extra-large' | 'full'
  closable?: boolean
  closeOnOverlay?: boolean
  persistent?: boolean
  loading?: boolean
  showCancelButton?: boolean
  showConfirmButton?: boolean
  cancelText?: string
  confirmText?: string
}

const props = withDefaults(defineProps<Props>(), {
  type: 'default',
  size: 'medium',
  closable: true,
  closeOnOverlay: true,
  persistent: false,
  loading: false,
  showCancelButton: true,
  showConfirmButton: true,
  cancelText: 'Cancel',
  confirmText: 'Confirm'
})

const emit = defineEmits<{
  close: []
  cancel: []
  confirm: []
  'update:show': [value: boolean]
}>()

const typeIcon = computed(() => {
  const icons = {
    info: InformationCircleIcon,
    success: CheckCircleIcon,
    warning: ExclamationTriangleIcon,
    danger: XCircleIcon,
  }
  return icons[props.type as keyof typeof icons]
})

const typeIconClasses = computed(() => {
  const classes = {
    info: 'text-blue-500',
    success: 'text-green-500',
    warning: 'text-yellow-500',
    danger: 'text-red-500',
  }
  return classes[props.type as keyof typeof classes] || ''
})

const messageClasses = computed(() => {
  const baseClasses = 'text-sm'
  const typeClasses = {
    default: 'text-gray-700',
    info: 'text-blue-700',
    success: 'text-green-700',
    warning: 'text-yellow-700',
    danger: 'text-red-700',
  }
  
  return [baseClasses, typeClasses[props.type]].join(' ')
})

const confirmVariant = computed(() => {
  const variants = {
    default: 'primary',
    info: 'primary',
    success: 'success',
    warning: 'primary',
    danger: 'danger',
  }
  return variants[props.type] as 'primary' | 'success' | 'danger'
})

function close() {
  emit('close')
}

function handleCancel() {
  emit('cancel')
  close()
}

function handleConfirm() {
  emit('confirm')
}
</script>