<template>
  <Teleport to="body">
    <Transition
      enter-active-class="transition-opacity duration-300"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-opacity duration-300"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="show"
        class="fixed inset-0 z-50 flex items-center justify-center"
        @click="handleOverlayClick"
      >
        <!-- Backdrop -->
        <div class="absolute inset-0 bg-black bg-opacity-50"></div>

        <!-- Modal -->
        <Transition
          enter-active-class="transition-all duration-300"
          enter-from-class="opacity-0 scale-95 translate-y-4"
          enter-to-class="opacity-100 scale-100 translate-y-0"
          leave-active-class="transition-all duration-300"
          leave-from-class="opacity-100 scale-100 translate-y-0"
          leave-to-class="opacity-0 scale-95 translate-y-4"
        >
          <div
            v-if="show"
            :class="modalClasses"
            @click.stop
          >
            <!-- Header -->
            <div v-if="$slots.header || title || closable" class="flex items-center justify-between p-6 border-b border-gray-200">
              <div>
                <slot name="header">
                  <h3 class="text-lg font-semibold text-gray-900">{{ title }}</h3>
                </slot>
              </div>
              <button
                v-if="closable"
                @click="close"
                class="text-gray-400 hover:text-gray-600 transition-colors"
              >
                <XMarkIcon class="h-6 w-6" />
              </button>
            </div>

            <!-- Body -->
            <div :class="bodyClasses">
              <slot />
            </div>

            <!-- Footer -->
            <div v-if="$slots.footer" class="flex items-center justify-end space-x-3 p-6 border-t border-gray-200">
              <slot name="footer" :close="close" />
            </div>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, watch } from 'vue'
import { XMarkIcon } from '@heroicons/vue/24/outline'

interface Props {
  show: boolean
  title?: string
  size?: 'small' | 'medium' | 'large' | 'extra-large' | 'full'
  closable?: boolean
  closeOnOverlay?: boolean
  persistent?: boolean
  scrollable?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  size: 'medium',
  closable: true,
  closeOnOverlay: true,
  persistent: false,
  scrollable: true
})

const emit = defineEmits<{
  close: []
  'update:show': [value: boolean]
}>()

const modalClasses = computed(() => {
  const baseClasses = 'relative bg-white rounded-lg shadow-xl max-h-full flex flex-col'
  
  const sizeClasses = {
    small: 'w-full max-w-md mx-4',
    medium: 'w-full max-w-lg mx-4',
    large: 'w-full max-w-2xl mx-4',
    'extra-large': 'w-full max-w-4xl mx-4',
    full: 'w-full h-full m-4'
  }
  
  return [baseClasses, sizeClasses[props.size]].join(' ')
})

const bodyClasses = computed(() => {
  const baseClasses = 'p-6'
  const scrollClasses = props.scrollable ? 'overflow-y-auto flex-1' : ''
  
  return [baseClasses, scrollClasses].join(' ')
})

function close() {
  if (!props.persistent) {
    emit('close')
    emit('update:show', false)
  }
}

function handleOverlayClick() {
  if (props.closeOnOverlay) {
    close()
  }
}

// Handle ESC key
watch(() => props.show, (newShow) => {
  if (newShow) {
    document.addEventListener('keydown', handleEscapeKey)
  } else {
    document.removeEventListener('keydown', handleEscapeKey)
  }
})

function handleEscapeKey(event: KeyboardEvent) {
  if (event.key === 'Escape' && props.closable) {
    close()
  }
}

// Prevent body scroll when modal is open
watch(() => props.show, (newShow) => {
  if (newShow) {
    document.body.style.overflow = 'hidden'
  } else {
    document.body.style.overflow = ''
  }
})
</script>