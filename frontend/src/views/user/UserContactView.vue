<template>
  <div class="space-y-6">
    <LoadingSpinner v-if="isLoading" size="medium" text="載入中..." />

    <template v-else-if="contacts.length > 0">
      <!-- Direct contacts -->
      <div v-if="directContacts.length > 0" class="bg-white rounded-xl border border-gray-200 p-6 shadow-sm">
        <h2 class="text-lg font-semibold text-gray-900 mb-4">直接聯絡</h2>
        <div class="space-y-3">
          <a
            v-for="contact in directContacts"
            :key="contact.id"
            :href="contactHref(contact)"
            target="_blank"
            rel="noopener noreferrer"
            class="flex items-center gap-4 p-3 rounded-xl border border-gray-100 hover:border-[var(--color-primary,#0ea5e9)] hover:bg-[var(--color-primary-light,#e0f2fe)] transition-colors group"
          >
            <div
              class="w-10 h-10 rounded-full flex items-center justify-center text-white text-sm font-bold flex-shrink-0"
              :style="{ backgroundColor: typeColor(contact.type) }"
            >{{ (contact.label || contact.type).charAt(0).toUpperCase() }}</div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-semibold text-gray-900 group-hover:text-[var(--color-primary,#0ea5e9)] transition-colors">
                {{ contact.label || contact.type }}
              </p>
              <p class="text-xs text-gray-500 truncate">{{ contact.value }}</p>
            </div>
            <ArrowTopRightOnSquareIcon class="h-4 w-4 text-gray-300 group-hover:text-[var(--color-primary,#0ea5e9)] transition-colors flex-shrink-0" />
          </a>
        </div>
      </div>

      <!-- Social links -->
      <div v-if="socialContacts.length > 0" class="bg-white rounded-xl border border-gray-200 p-6 shadow-sm">
        <h2 class="text-lg font-semibold text-gray-900 mb-4">社群媒體</h2>
        <div class="grid grid-cols-2 gap-3">
          <a
            v-for="contact in socialContacts"
            :key="contact.id"
            :href="contactHref(contact)"
            target="_blank"
            rel="noopener noreferrer"
            class="flex items-center gap-2.5 px-3 py-2.5 rounded-xl border-2 border-gray-200 hover:border-[var(--color-primary,#0ea5e9)] transition-colors group"
          >
            <div
              class="w-8 h-8 rounded-full flex items-center justify-center text-white text-sm font-bold flex-shrink-0"
              :style="{ backgroundColor: typeColor(contact.type) }"
            >{{ (contact.label || contact.type).charAt(0).toUpperCase() }}</div>
            <span class="text-sm font-medium text-gray-700 group-hover:text-[var(--color-primary,#0ea5e9)] transition-colors truncate">
              {{ contact.label || contact.type }}
            </span>
          </a>
        </div>
      </div>
    </template>

    <div v-else-if="!isLoading" class="text-center text-gray-400 py-12">尚未設定聯絡方式</div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, inject, onMounted, watch } from 'vue'
import type { ComputedRef } from 'vue'
import { ArrowTopRightOnSquareIcon } from '@heroicons/vue/24/outline'
import httpService from '@/services/http'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import type { ContactMethod } from '@/types/api'

const userId = inject<ComputedRef<number | null>>('userId')

const isLoading = ref(true)
const contacts = ref<ContactMethod[]>([])

const socialTypes = new Set(['LinkedIn', 'GitHub', 'Facebook', 'Twitter', 'Instagram', 'Discord'])
const directContacts = computed(() => contacts.value.filter(c => !socialTypes.has(c.type)))
const socialContacts = computed(() => contacts.value.filter(c => socialTypes.has(c.type)))

const typeColorMap: Record<string, string> = {
  Email: '#0ea5e9',
  Phone: '#22c55e',
  LinkedIn: '#0077b5',
  GitHub: '#333',
  Facebook: '#1877f2',
  Twitter: '#1da1f2',
  Instagram: '#e1306c',
  Discord: '#5865f2',
  Other: '#64748b',
}
function typeColor(type: string): string {
  return typeColorMap[type] ?? '#64748b'
}

function contactHref(c: ContactMethod): string {
  if (c.type === 'Email') return `mailto:${c.value}`
  if (c.type === 'Phone') return `tel:${c.value}`
  return c.value
}

async function load(uid: number) {
  isLoading.value = true
  try {
    const res = await httpService.get<ContactMethod[]>(`/contactmethods/user/${uid}/public`)
    if (res.success) contacts.value = res.data || []
  } catch {
    // ignore errors on public pages
  } finally {
    isLoading.value = false
  }
}

onMounted(() => { if (userId?.value) load(userId.value) })
watch(() => userId?.value, (uid) => { if (uid) load(uid) })
</script>
