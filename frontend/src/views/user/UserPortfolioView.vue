<template>
  <div>
    <!-- Search & Filter -->
    <div class="bg-white rounded-xl border border-gray-200 shadow-sm p-4 mb-6">
      <div class="flex flex-col sm:flex-row gap-3">
        <div class="flex-1">
          <input
            v-model="searchTerm"
            type="search"
            placeholder="搜尋作品..."
            class="w-full px-3 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 placeholder:text-gray-400 text-sm focus:outline-none focus:ring-2 focus:ring-[var(--color-primary,#0ea5e9)] focus:border-[var(--color-primary,#0ea5e9)]"
          />
        </div>
        <select
          v-model="selectedTech"
          class="sm:w-44 px-3 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 text-sm focus:outline-none focus:ring-2 focus:ring-[var(--color-primary,#0ea5e9)] focus:border-[var(--color-primary,#0ea5e9)]"
        >
          <option value="">所有技術</option>
          <option v-for="tech in availableTechs" :key="tech" :value="tech">{{ tech }}</option>
        </select>
      </div>
    </div>

    <LoadingSpinner v-if="isLoading" size="medium" text="載入中..." />

    <div v-else-if="filteredPortfolios.length > 0" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
      <RouterLink
        v-for="project in filteredPortfolios"
        :key="project.id"
        :to="`/@${username}/portfolio/${project.id}`"
        class="bg-white rounded-xl border border-gray-200 shadow-sm hover:shadow-md transition-shadow duration-200 overflow-hidden group"
      >
        <!-- Image / placeholder -->
        <div class="relative aspect-video bg-gray-100 overflow-hidden">
          <img
            v-if="project.imageUrl"
            :src="project.imageUrl"
            :alt="project.title"
            class="w-full h-full object-cover"
          />
          <div v-else class="flex items-center justify-center h-full text-gray-300">
            <BriefcaseIcon class="h-12 w-12" />
          </div>
          <!-- Featured badge -->
          <div
            v-if="project.isFeatured"
            class="absolute top-3 left-3 bg-amber-500 text-white px-2.5 py-0.5 rounded-full text-xs font-medium"
          >精選</div>
        </div>
        <div class="p-5">
          <h3 class="font-semibold text-gray-900 group-hover:text-[var(--color-primary,#0ea5e9)] transition-colors mb-1">{{ project.title }}</h3>
          <p v-if="project.description" class="text-sm text-gray-500 line-clamp-2 mb-3">{{ project.description }}</p>
          <!-- Tech tags -->
          <div v-if="project.technologies" class="flex flex-wrap gap-1.5 mb-3">
            <span
              v-for="tech in project.technologies.split(',').slice(0, 4)"
              :key="tech"
              class="text-xs px-2 py-0.5 rounded-full"
              :style="{ backgroundColor: 'var(--color-primary-light,#e0f2fe)', color: 'var(--color-primary-dark,#0c4a6e)' }"
            >{{ tech.trim() }}</span>
          </div>
          <!-- Links + attachment badge -->
          <div class="flex items-center gap-3 flex-wrap">
            <a
              v-if="project.projectUrl"
              :href="project.projectUrl"
              target="_blank"
              rel="noopener noreferrer"
              class="text-xs text-sky-600 hover:text-sky-800 font-medium flex items-center gap-1"
              @click.prevent.stop="openUrl(project.projectUrl)"
            >
              <ArrowTopRightOnSquareIcon class="h-3.5 w-3.5" />查看專案
            </a>
            <a
              v-if="project.repositoryUrl"
              :href="project.repositoryUrl"
              target="_blank"
              rel="noopener noreferrer"
              class="text-xs text-gray-500 hover:text-gray-700 font-medium flex items-center gap-1"
              @click.prevent.stop="openUrl(project.repositoryUrl)"
            >
              <CodeBracketIcon class="h-3.5 w-3.5" />程式碼
            </a>
            <span
              v-if="attachmentCounts[project.id]"
              class="text-xs bg-gray-100 text-gray-500 px-2 py-0.5 rounded-full"
            >📎 {{ attachmentCounts[project.id] }}</span>
          </div>
        </div>
      </RouterLink>
    </div>

    <div v-else-if="!isLoading && portfolios.length > 0" class="text-center text-gray-400 py-12">
      <p>找不到符合的作品</p>
      <button @click="clearFilters" class="mt-3 text-sm text-sky-600 hover:text-sky-800">清除篩選</button>
    </div>

    <div v-else-if="!isLoading" class="text-center text-gray-400 py-12">尚無作品集</div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, inject, onMounted, watch } from 'vue'
import type { ComputedRef } from 'vue'
import { RouterLink } from 'vue-router'
import { BriefcaseIcon, ArrowTopRightOnSquareIcon, CodeBracketIcon } from '@heroicons/vue/24/outline'
import httpService from '@/services/http'
import portfolioAttachmentService from '@/services/portfolioAttachmentService'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import type { Portfolio } from '@/types/api'

const userId = inject<ComputedRef<number | null>>('userId')
const username = inject<ComputedRef<string>>('username')

const isLoading = ref(true)
const portfolios = ref<Portfolio[]>([])
const attachmentCounts = ref<Record<number, number>>({})
const searchTerm = ref('')
const selectedTech = ref('')

const availableTechs = computed(() => {
  const techs = new Set<string>()
  portfolios.value.forEach(p => {
    if (p.technologies) p.technologies.split(',').forEach(t => techs.add(t.trim()))
  })
  return Array.from(techs).sort()
})

const filteredPortfolios = computed(() => {
  let list = portfolios.value
  if (searchTerm.value) {
    const q = searchTerm.value.toLowerCase()
    list = list.filter(p =>
      p.title.toLowerCase().includes(q) ||
      (p.description?.toLowerCase().includes(q)) ||
      (p.technologies?.toLowerCase().includes(q))
    )
  }
  if (selectedTech.value) {
    list = list.filter(p => p.technologies?.toLowerCase().includes(selectedTech.value.toLowerCase()))
  }
  return list
})

function clearFilters() {
  searchTerm.value = ''
  selectedTech.value = ''
}

function openUrl(url: string) {
  window.open(url, '_blank', 'noopener,noreferrer')
}

async function load(uid: number) {
  isLoading.value = true
  try {
    const res = await httpService.get<Portfolio[]>(`/portfolios/user/${uid}/public`)
    if (res.success) {
      portfolios.value = res.data || []
      // Load attachment counts in background
      portfolios.value.forEach(async (p) => {
        try {
          const attRes = await portfolioAttachmentService.getByPortfolio(p.id)
          if (attRes.success && attRes.data?.length) {
            attachmentCounts.value = { ...attachmentCounts.value, [p.id]: attRes.data.length }
          }
        } catch { /* ignore */ }
      })
    }
  } catch {
    // ignore
  } finally {
    isLoading.value = false
  }
}

onMounted(() => { if (userId?.value) load(userId.value) })
watch(() => userId?.value, (uid) => { if (uid) load(uid) })
</script>
