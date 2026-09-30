<template>
  <div>
    <LoadingSpinner v-if="isLoading" size="medium" text="載入中..." />

    <div v-else class="grid grid-cols-1 lg:grid-cols-2 gap-8">
      <!-- Work Experience -->
      <section>
        <h2 class="text-xl font-bold text-gray-900 mb-6">工作經歷</h2>

        <div v-if="workExperiences.length > 0" class="relative">
          <!-- Vertical line -->
          <div class="absolute left-3.5 top-0 bottom-0 w-px bg-gray-200"></div>

          <div class="space-y-6">
            <div
              v-for="(work, index) in workExperiences"
              :key="work.id"
              class="relative pl-10"
            >
              <!-- Dot -->
              <div
                class="absolute left-0 top-1 w-7 h-7 rounded-full flex items-center justify-center z-10"
                :style="work.isCurrent
                  ? { backgroundColor: 'var(--color-primary,#0ea5e9)' }
                  : { backgroundColor: '#e5e7eb' }"
              >
                <div
                  class="w-2.5 h-2.5 rounded-full"
                  :class="work.isCurrent ? 'bg-white' : 'bg-gray-400'"
                ></div>
              </div>

              <!-- Card -->
              <div class="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
                <div class="flex items-start justify-between flex-wrap gap-2 mb-1">
                  <div>
                    <h3 class="font-semibold text-gray-900">{{ work.position }}</h3>
                    <p class="text-sm text-gray-600">{{ work.company }}</p>
                  </div>
                  <span
                    class="text-xs px-2 py-0.5 rounded-full flex-shrink-0 font-medium"
                    :class="work.isCurrent
                      ? 'text-white'
                      : 'bg-gray-100 text-gray-500'"
                    :style="work.isCurrent ? { backgroundColor: 'var(--color-primary,#0ea5e9)' } : {}"
                  >
                    {{ work.isCurrent ? '在職中' : formatDate(work.endDate) }}
                  </span>
                </div>
                <p class="text-xs text-gray-400 mb-2">
                  {{ formatDate(work.startDate) }} – {{ work.isCurrent ? '至今' : formatDate(work.endDate) }}
                </p>
                <p v-if="work.description" class="text-sm text-gray-600 leading-relaxed">{{ work.description }}</p>
              </div>
            </div>
          </div>
        </div>

        <p v-else class="text-gray-400">尚無工作經歷</p>
      </section>

      <!-- Education -->
      <section>
        <h2 class="text-xl font-bold text-gray-900 mb-6">學歷</h2>

        <div v-if="educations.length > 0" class="relative">
          <!-- Vertical line -->
          <div class="absolute left-3.5 top-0 bottom-0 w-px bg-gray-200"></div>

          <div class="space-y-6">
            <div
              v-for="edu in educations"
              :key="edu.id"
              class="relative pl-10"
            >
              <!-- Dot -->
              <div
                class="absolute left-0 top-1 w-7 h-7 rounded-full flex items-center justify-center z-10"
                :style="{ backgroundColor: 'var(--color-primary-light,#e0f2fe)' }"
              >
                <div
                  class="w-2.5 h-2.5 rounded-full"
                  :style="{ backgroundColor: 'var(--color-primary,#0ea5e9)' }"
                ></div>
              </div>

              <!-- Card -->
              <div class="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
                <div class="flex items-start justify-between flex-wrap gap-2 mb-1">
                  <div>
                    <h3 class="font-semibold text-gray-900">{{ edu.school }}</h3>
                    <p v-if="edu.degree || edu.fieldOfStudy" class="text-sm text-gray-600">
                      {{ [edu.degree, edu.fieldOfStudy].filter(Boolean).join(' · ') }}
                    </p>
                  </div>
                  <span v-if="edu.startYear || edu.endYear" class="text-xs text-gray-400 flex-shrink-0">
                    {{ edu.startYear }} – {{ edu.endYear ?? '至今' }}
                  </span>
                </div>
                <p v-if="edu.description" class="text-sm text-gray-600 leading-relaxed">{{ edu.description }}</p>
              </div>
            </div>
          </div>
        </div>

        <p v-else class="text-gray-400">尚無學歷</p>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, inject, onMounted, watch } from 'vue'
import type { ComputedRef } from 'vue'
import httpService from '@/services/http'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import type { Education, WorkExperience } from '@/types/api'

const userId = inject<ComputedRef<number | null>>('userId')

const isLoading = ref(true)
const workExperiences = ref<WorkExperience[]>([])
const educations = ref<Education[]>([])

function formatDate(dateStr?: string | null): string {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return `${d.getFullYear()}/${String(d.getMonth() + 1).padStart(2, '0')}`
}

async function load(uid: number) {
  isLoading.value = true
  try {
    const [workRes, eduRes] = await Promise.all([
      httpService.get<WorkExperience[]>(`/workexperiences/user/${uid}/public`),
      httpService.get<Education[]>(`/educations/user/${uid}/public`),
    ])
    if (workRes.success) workExperiences.value = workRes.data || []
    if (eduRes.success) educations.value = eduRes.data || []
  } finally {
    isLoading.value = false
  }
}

onMounted(() => { if (userId?.value) load(userId.value) })
watch(() => userId?.value, (uid) => { if (uid) load(uid) })
</script>
