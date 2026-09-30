<template>
  <div class="space-y-6">
    <LoadingSpinner v-if="isLoading" size="medium" text="載入中..." />

    <template v-else-if="profile">
      <!-- Summary / Bio -->
      <div v-if="profile.summary || profile.description" class="bg-white rounded-xl border border-gray-200 p-6 shadow-sm">
        <h2 class="text-lg font-semibold text-gray-900 mb-3">個人簡介</h2>
        <p v-if="profile.summary" class="text-gray-700 mb-3 leading-relaxed">{{ profile.summary }}</p>
        <p v-if="profile.description" class="text-gray-600 text-sm whitespace-pre-wrap leading-relaxed">{{ profile.description }}</p>
        <a
          v-if="profile.website"
          :href="profile.website"
          target="_blank"
          rel="noopener noreferrer"
          class="inline-flex items-center gap-1.5 mt-3 text-sm text-sky-600 hover:text-sky-800 transition-colors"
        >
          <GlobeAltIcon class="h-4 w-4" />
          {{ profile.website }}
        </a>
      </div>

      <!-- Expert Skills -->
      <div v-if="expertSkills.length > 0" class="bg-white rounded-xl border border-gray-200 p-6 shadow-sm">
        <h2 class="text-lg font-semibold text-gray-900 mb-4">核心專長</h2>
        <div class="grid grid-cols-2 md:grid-cols-3 gap-3">
          <div
            v-for="skill in expertSkills"
            :key="skill.id"
            class="flex items-center gap-2"
          >
            <div class="w-2.5 h-2.5 rounded-full flex-shrink-0" :style="{ backgroundColor: 'var(--color-primary, #0ea5e9)' }"></div>
            <span class="text-sm text-gray-700 font-medium">{{ skill.name }}</span>
          </div>
        </div>
      </div>

      <!-- Current Position -->
      <div v-if="currentPosition" class="bg-white rounded-xl border border-gray-200 p-6 shadow-sm">
        <h2 class="text-lg font-semibold text-gray-900 mb-4">目前職位</h2>
        <div class="space-y-1">
          <h3 class="font-semibold text-gray-900">{{ currentPosition.position }}</h3>
          <p class="text-sm font-medium" :style="{ color: 'var(--color-primary, #0ea5e9)' }">{{ currentPosition.company }}</p>
          <p v-if="currentPosition.description" class="text-sm text-gray-600 mt-2 leading-relaxed">{{ currentPosition.description }}</p>
        </div>
      </div>

      <!-- Latest Education -->
      <div v-if="latestEducation" class="bg-white rounded-xl border border-gray-200 p-6 shadow-sm">
        <h2 class="text-lg font-semibold text-gray-900 mb-4">學歷</h2>
        <div class="space-y-1">
          <h3 class="font-semibold text-gray-900">
            {{ latestEducation.degree }}<span v-if="latestEducation.fieldOfStudy"> - {{ latestEducation.fieldOfStudy }}</span>
          </h3>
          <p class="text-sm font-medium" :style="{ color: 'var(--color-primary, #0ea5e9)' }">{{ latestEducation.school }}</p>
          <p class="text-sm text-gray-500">
            {{ latestEducation.startYear }}<span v-if="latestEducation.endYear"> - {{ latestEducation.endYear }}</span><span v-else> - 至今</span>
          </p>
        </div>
      </div>

      <!-- Contact Methods -->
      <div v-if="contacts.length > 0" class="bg-white rounded-xl border border-gray-200 p-6 shadow-sm">
        <h2 class="text-lg font-semibold text-gray-900 mb-4">聯絡我</h2>

        <!-- Direct contacts (Email / Phone / all non-social) -->
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

        <!-- Social links grid -->
        <div v-if="socialContacts.length > 0" class="mt-4 grid grid-cols-2 gap-2">
          <a
            v-for="contact in socialContacts"
            :key="contact.id"
            :href="contactHref(contact)"
            target="_blank"
            rel="noopener noreferrer"
            class="flex items-center gap-2.5 px-3 py-2.5 rounded-xl border-2 border-gray-200 hover:border-[var(--color-primary,#0ea5e9)] transition-colors group"
          >
            <div
              class="w-7 h-7 rounded-full flex items-center justify-center text-white text-xs font-bold flex-shrink-0"
              :style="{ backgroundColor: typeColor(contact.type) }"
            >{{ (contact.label || contact.type).charAt(0).toUpperCase() }}</div>
            <span class="text-sm font-medium text-gray-700 group-hover:text-[var(--color-primary,#0ea5e9)] transition-colors truncate">
              {{ contact.label || contact.type }}
            </span>
          </a>
        </div>
      </div>
    </template>

    <div v-else-if="!isLoading" class="text-center text-gray-400 py-12">尚無個人資料</div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, inject, onMounted, watch } from 'vue'
import type { ComputedRef } from 'vue'
import { GlobeAltIcon, ArrowTopRightOnSquareIcon } from '@heroicons/vue/24/outline'
import profileService from '@/services/profileService'
import httpService from '@/services/http'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import type { PersonalProfile, ContactMethod, Skill, WorkExperience, Education } from '@/types/api'

const userId = inject<ComputedRef<number | null>>('userId')

const isLoading = ref(true)
const profile = ref<PersonalProfile | null>(null)
const contacts = ref<ContactMethod[]>([])
const skills = ref<Skill[]>([])
const workExperiences = ref<WorkExperience[]>([])
const educations = ref<Education[]>([])

const expertSkills = computed(() =>
  skills.value.filter(s => s.level === 'Expert' || s.level === 'Advanced').slice(0, 6)
)
const currentPosition = computed(() =>
  workExperiences.value.find(w => w.isCurrent) ?? workExperiences.value[0] ?? null
)
const latestEducation = computed(() => educations.value[0] ?? null)

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
    const [profileRes, contactRes, skillRes, workRes, eduRes] = await Promise.all([
      profileService.getProfileByUserId(uid),
      httpService.get<ContactMethod[]>(`/contactmethods/user/${uid}/public`),
      httpService.get<Skill[]>(`/skills/user/${uid}/public`),
      httpService.get<WorkExperience[]>(`/workexperiences/user/${uid}/public`),
      httpService.get<Education[]>(`/educations/user/${uid}/public`),
    ])
    if (profileRes.success) profile.value = profileRes.data
    if (contactRes.success) contacts.value = contactRes.data || []
    if (skillRes.success) skills.value = skillRes.data || []
    if (workRes.success) workExperiences.value = workRes.data || []
    if (eduRes.success) educations.value = eduRes.data || []
  } catch {
    // ignore errors on public pages
  } finally {
    isLoading.value = false
  }
}

onMounted(() => { if (userId?.value) load(userId.value) })
watch(() => userId?.value, (uid) => { if (uid) load(uid) })
</script>
