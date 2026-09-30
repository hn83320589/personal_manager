<template>
  <div>
    <LoadingSpinner v-if="isLoading" size="medium" text="載入中..." />

    <div v-else-if="project" class="space-y-6">
      <div>
        <RouterLink :to="`/@${username}/portfolio`" class="inline-flex items-center gap-1 text-sm text-sky-600 hover:text-sky-800">
          <ArrowLeftIcon class="h-4 w-4" />返回作品集
        </RouterLink>
      </div>

      <!-- Main card -->
      <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
        <!-- Image -->
        <div v-if="project.imageUrl" class="relative">
          <img :src="project.imageUrl" :alt="project.title" class="w-full max-h-72 object-cover" />
        </div>

        <div class="p-6 sm:p-8">
          <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
            <!-- Main info -->
            <div class="lg:col-span-2 space-y-5">
              <div>
                <div class="flex items-start gap-3 flex-wrap">
                  <h1 class="text-2xl font-bold text-gray-900">{{ project.title }}</h1>
                  <span
                    v-if="project.isFeatured"
                    class="text-xs bg-amber-100 text-amber-700 px-2.5 py-1 rounded-full font-medium flex-shrink-0"
                  >⭐ 精選</span>
                </div>
                <span v-if="project.createdAt" class="text-sm text-gray-400 mt-1 block">
                  {{ formatDate(project.createdAt) }}
                </span>
              </div>

              <p v-if="project.description" class="text-gray-700 whitespace-pre-wrap leading-relaxed">{{ project.description }}</p>

              <!-- Technologies -->
              <div v-if="technologies.length > 0">
                <h3 class="text-sm font-semibold text-gray-700 mb-2">使用技術</h3>
                <div class="flex flex-wrap gap-2">
                  <span
                    v-for="tech in technologies"
                    :key="tech"
                    class="text-sm px-3 py-1 rounded-full font-medium"
                    :style="{ backgroundColor: 'var(--color-primary-light,#e0f2fe)', color: 'var(--color-primary-dark,#0c4a6e)' }"
                  >{{ tech }}</span>
                </div>
              </div>

              <!-- Action buttons -->
              <div class="flex flex-wrap gap-3">
                <a
                  v-if="project.projectUrl"
                  :href="project.projectUrl"
                  target="_blank"
                  rel="noopener noreferrer"
                  class="inline-flex items-center gap-2 px-4 py-2 rounded-lg text-white text-sm font-medium transition-opacity hover:opacity-90"
                  :style="{ backgroundColor: 'var(--color-primary,#0ea5e9)' }"
                >
                  <GlobeAltIcon class="h-4 w-4" />線上展示
                </a>
                <a
                  v-if="project.repositoryUrl"
                  :href="project.repositoryUrl"
                  target="_blank"
                  rel="noopener noreferrer"
                  class="inline-flex items-center gap-2 px-4 py-2 rounded-lg border border-gray-300 text-gray-700 bg-white text-sm font-medium hover:bg-gray-50 transition-colors"
                >
                  <CodeBracketIcon class="h-4 w-4" />查看程式碼
                </a>
              </div>
            </div>

            <!-- Sidebar details -->
            <div class="lg:col-span-1">
              <div class="bg-gray-50 rounded-xl border border-gray-200 p-4 space-y-4">
                <h3 class="text-sm font-semibold text-gray-900">專案詳情</h3>
                <div v-if="project.createdAt" class="text-sm">
                  <span class="text-gray-500">建立日期</span>
                  <p class="text-gray-800 mt-0.5">{{ formatDate(project.createdAt) }}</p>
                </div>
                <div class="text-sm">
                  <span class="text-gray-500">可見性</span>
                  <p class="mt-0.5">
                    <span :class="project.isPublic ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'"
                      class="inline-block text-xs px-2 py-0.5 rounded-full font-medium">
                      {{ project.isPublic ? '公開' : '私人' }}
                    </span>
                  </p>
                </div>
                <div v-if="project.projectUrl || project.repositoryUrl" class="text-sm space-y-2">
                  <span class="text-gray-500">連結</span>
                  <div class="space-y-1">
                    <a
                      v-if="project.projectUrl"
                      :href="project.projectUrl"
                      target="_blank"
                      rel="noopener noreferrer"
                      class="flex items-center gap-1.5 text-sky-600 hover:text-sky-800 transition-colors"
                    >
                      <GlobeAltIcon class="h-4 w-4" />線上專案
                    </a>
                    <a
                      v-if="project.repositoryUrl"
                      :href="project.repositoryUrl"
                      target="_blank"
                      rel="noopener noreferrer"
                      class="flex items-center gap-1.5 text-gray-600 hover:text-gray-800 transition-colors"
                    >
                      <CodeBracketIcon class="h-4 w-4" />原始碼
                    </a>
                  </div>
                </div>

                <!-- Attachments -->
                <div v-if="attachments.length > 0" class="text-sm space-y-2">
                  <span class="text-gray-500">附件下載</span>
                  <div class="space-y-1">
                    <a
                      v-for="att in attachments"
                      :key="att.id"
                      :href="att.fileUrl"
                      target="_blank"
                      rel="noopener noreferrer"
                      class="flex items-center gap-1.5 text-sky-600 hover:text-sky-800 transition-colors"
                    >
                      <ArrowDownTrayIcon class="h-4 w-4 flex-shrink-0" />
                      <span class="truncate">{{ att.fileName }}</span>
                      <span class="text-gray-400 text-xs flex-shrink-0">{{ formatFileSize(att.fileSize) }}</span>
                    </a>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Related Projects -->
      <section v-if="relatedProjects.length > 0">
        <h2 class="text-lg font-semibold text-gray-900 mb-4">更多作品</h2>
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          <RouterLink
            v-for="rel in relatedProjects"
            :key="rel.id"
            :to="`/@${username}/portfolio/${rel.id}`"
            class="bg-white rounded-xl border border-gray-200 shadow-sm hover:shadow-md transition-shadow p-4 group"
          >
            <img v-if="rel.imageUrl" :src="rel.imageUrl" :alt="rel.title" class="w-full h-28 object-cover rounded-lg mb-3" />
            <h3 class="font-medium text-gray-900 group-hover:text-sky-600 transition-colors text-sm">{{ rel.title }}</h3>
            <p v-if="rel.description" class="text-xs text-gray-500 mt-1 line-clamp-2">{{ rel.description }}</p>
          </RouterLink>
        </div>
      </section>
    </div>

    <div v-else-if="!isLoading" class="text-center text-gray-400 py-12">找不到此作品</div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, inject, onMounted } from 'vue'
import type { ComputedRef } from 'vue'
import { useRoute, RouterLink } from 'vue-router'
import { ArrowLeftIcon, GlobeAltIcon, CodeBracketIcon, ArrowDownTrayIcon } from '@heroicons/vue/24/outline'
import httpService from '@/services/http'
import portfolioAttachmentService from '@/services/portfolioAttachmentService'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import type { Portfolio, PortfolioAttachment } from '@/types/api'

const route = useRoute()
const username = inject<ComputedRef<string>>('username')
const userId = inject<ComputedRef<number | null>>('userId')

const isLoading = ref(true)
const project = ref<Portfolio | null>(null)
const allPortfolios = ref<Portfolio[]>([])
const attachments = ref<PortfolioAttachment[]>([])

function formatFileSize(bytes: number): string {
  if (!bytes) return ''
  if (bytes < 1024) return `${bytes}B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)}KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)}MB`
}

const technologies = computed(() =>
  project.value?.technologies?.split(',').map(t => t.trim()).filter(Boolean) ?? []
)

const relatedProjects = computed(() =>
  allPortfolios.value.filter(p => p.id !== project.value?.id).slice(0, 3)
)

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString('zh-TW', { year: 'numeric', month: 'long' })
}

onMounted(async () => {
  const id = route.params.id as string
  try {
    const [projectRes, portfoliosRes, attachmentsRes] = await Promise.all([
      httpService.get<Portfolio>(`/portfolios/${id}`),
      userId?.value ? httpService.get<Portfolio[]>(`/portfolios/user/${userId.value}/public`) : Promise.resolve({ success: false, data: [], message: '', errors: [] }),
      portfolioAttachmentService.getByPortfolio(Number(id)),
    ])
    if (projectRes.success) project.value = projectRes.data
    if (portfoliosRes.success) allPortfolios.value = portfoliosRes.data || []
    if (attachmentsRes.success) attachments.value = attachmentsRes.data || []
  } catch {
    // ignore
  } finally {
    isLoading.value = false
  }
})
</script>
