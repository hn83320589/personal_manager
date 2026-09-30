<template>
  <div>
    <LoadingSpinner v-if="isLoading" size="medium" text="載入中..." />

    <template v-else-if="skills.length > 0">
      <!-- Filter Tabs -->
      <div class="mb-6 border-b border-gray-200">
        <nav class="-mb-px flex space-x-6 overflow-x-auto scrollbar-hide">
          <button
            v-for="tab in filterTabs"
            :key="tab.key"
            :class="[
              'py-3 px-1 border-b-2 text-sm font-medium whitespace-nowrap transition-colors',
              activeFilter === tab.key
                ? 'border-[var(--color-primary,#0ea5e9)] text-[var(--color-primary,#0ea5e9)]'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300',
            ]"
            @click="activeFilter = tab.key"
          >
            {{ tab.label }}
            <span
              :class="[
                'ml-1.5 py-0.5 px-2 rounded-full text-xs',
                activeFilter === tab.key
                  ? 'bg-sky-100 text-sky-700'
                  : 'bg-gray-100 text-gray-500',
              ]"
            >{{ tab.count }}</span>
          </button>
        </nav>
      </div>

      <!-- All: grouped by category -->
      <template v-if="activeFilter === 'all'">
        <div v-for="(group, category) in groupedSkills" :key="category" class="mb-8">
          <div class="flex items-center justify-between mb-4">
            <h2 class="text-base font-semibold text-gray-900">{{ category || '其他' }}</h2>
            <span class="text-xs text-gray-400">{{ group.length }} 項</span>
          </div>
          <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
            <div
              v-for="skill in group"
              :key="skill.id"
              class="bg-white rounded-xl border border-gray-200 p-4 shadow-sm hover:shadow-md transition-shadow duration-200"
            >
              <div class="flex items-center justify-between mb-2">
                <span class="font-medium text-gray-900 text-sm">{{ skill.name }}</span>
                <span :class="levelBadgeClass(skill.level)" class="text-xs px-2 py-0.5 rounded-full font-medium">
                  {{ levelLabel(skill.level) }}
                </span>
              </div>
              <!-- Progress bar + dots -->
              <div class="flex items-center gap-2">
                <div class="flex-1 bg-gray-100 rounded-full h-1.5">
                  <div
                    :class="levelBarClass(skill.level)"
                    :style="{ width: levelPercent(skill.level) + '%' }"
                    class="h-1.5 rounded-full transition-all duration-300"
                  ></div>
                </div>
                <div class="flex gap-1">
                  <div
                    v-for="n in 4"
                    :key="n"
                    :class="[
                      'w-1.5 h-1.5 rounded-full transition-colors',
                      n <= levelNumber(skill.level) ? levelDotClass(skill.level) : 'bg-gray-200',
                    ]"
                  ></div>
                </div>
              </div>
              <p v-if="skill.yearsOfExperience > 0" class="text-xs text-gray-400 mt-1.5">
                {{ skill.yearsOfExperience }} 年經驗
              </p>
            </div>
          </div>
        </div>
      </template>

      <!-- Filtered by level: circle view -->
      <template v-else>
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          <div
            v-for="skill in filteredSkills"
            :key="skill.id"
            class="bg-white rounded-xl border border-gray-200 p-5 shadow-sm hover:shadow-lg hover:-translate-y-0.5 transition-all duration-200 text-center"
          >
            <h3 class="text-base font-semibold text-gray-900 mb-0.5">{{ skill.name }}</h3>
            <p v-if="skill.category" class="text-xs text-gray-400 mb-3">{{ skill.category }}</p>
            <!-- SVG Circle -->
            <div class="flex justify-center mb-3">
              <div class="relative w-20 h-20">
                <svg class="w-20 h-20 -rotate-90" viewBox="0 0 24 24">
                  <circle cx="12" cy="12" r="10" stroke="#e5e7eb" stroke-width="2" fill="none" />
                  <circle
                    cx="12" cy="12" r="10"
                    :class="levelColorClass(skill.level)"
                    stroke-width="2" fill="none"
                    :stroke-dasharray="`${(levelNumber(skill.level) / 4) * 62.83} 62.83`"
                    class="transition-all duration-500"
                  />
                </svg>
                <div class="absolute inset-0 flex items-center justify-center">
                  <span class="text-xs font-semibold text-gray-700">{{ Math.round((levelNumber(skill.level) / 4) * 100) }}%</span>
                </div>
              </div>
            </div>
            <span :class="levelBadgeClass(skill.level)" class="text-xs px-2.5 py-0.5 rounded-full font-medium">{{ levelLabel(skill.level) }}</span>
            <p v-if="skill.yearsOfExperience > 0" class="text-xs text-gray-500 mt-2">{{ skill.yearsOfExperience }} 年經驗</p>
          </div>
        </div>
      </template>

      <!-- Stats -->
      <div class="mt-10 bg-white rounded-xl border border-gray-200 p-6 shadow-sm">
        <h2 class="text-base font-semibold text-gray-900 mb-4">技能總覽</h2>
        <div class="grid grid-cols-2 md:grid-cols-4 gap-4 text-center">
          <div>
            <div class="text-2xl font-bold" :style="{ color: 'var(--color-primary, #0ea5e9)' }">{{ skills.length }}</div>
            <div class="text-xs text-gray-500 mt-1">總技能數</div>
          </div>
          <div>
            <div class="text-2xl font-bold text-purple-600">{{ expertCount }}</div>
            <div class="text-xs text-gray-500 mt-1">專家級</div>
          </div>
          <div>
            <div class="text-2xl font-bold text-green-600">{{ Object.keys(groupedSkills).length }}</div>
            <div class="text-xs text-gray-500 mt-1">分類數</div>
          </div>
          <div>
            <div class="text-2xl font-bold text-amber-500">{{ avgYears }}+</div>
            <div class="text-xs text-gray-500 mt-1">平均年資</div>
          </div>
        </div>
      </div>
    </template>

    <div v-else-if="!isLoading" class="text-center text-gray-400 py-12">尚無技能資料</div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, inject, onMounted, watch } from 'vue'
import type { ComputedRef } from 'vue'
import httpService from '@/services/http'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import type { Skill, SkillLevel } from '@/types/api'

const userId = inject<ComputedRef<number | null>>('userId')

const isLoading = ref(true)
const skills = ref<Skill[]>([])
const activeFilter = ref('all')

const groupedSkills = computed(() =>
  skills.value.reduce((acc: Record<string, Skill[]>, s) => {
    const cat = s.category || '其他'
    if (!acc[cat]) acc[cat] = []
    acc[cat].push(s)
    return acc
  }, {})
)

const byLevel = computed(() =>
  skills.value.reduce((acc: Record<string, Skill[]>, s) => {
    if (!acc[s.level]) acc[s.level] = []
    acc[s.level].push(s)
    return acc
  }, {})
)

const filterTabs = computed(() => [
  { key: 'all', label: '所有分類', count: skills.value.length },
  { key: 'Expert', label: '專家', count: byLevel.value['Expert']?.length ?? 0 },
  { key: 'Advanced', label: '進階', count: byLevel.value['Advanced']?.length ?? 0 },
  { key: 'Intermediate', label: '中級', count: byLevel.value['Intermediate']?.length ?? 0 },
  { key: 'Beginner', label: '初學', count: byLevel.value['Beginner']?.length ?? 0 },
])

const filteredSkills = computed(() =>
  activeFilter.value === 'all' ? skills.value : (byLevel.value[activeFilter.value] ?? [])
)

const expertCount = computed(() => (byLevel.value['Expert']?.length ?? 0))
const avgYears = computed(() => {
  const withExp = skills.value.filter(s => s.yearsOfExperience > 0)
  if (!withExp.length) return 0
  return Math.round(withExp.reduce((s, sk) => s + sk.yearsOfExperience, 0) / withExp.length)
})

function levelNumber(level: SkillLevel) { return { Beginner: 1, Intermediate: 2, Advanced: 3, Expert: 4 }[level] ?? 2 }
function levelPercent(level: SkillLevel) { return levelNumber(level) * 25 }
function levelLabel(level: SkillLevel) { return { Beginner: '初學', Intermediate: '中級', Advanced: '進階', Expert: '專家' }[level] ?? level }
function levelBadgeClass(level: SkillLevel) {
  return { Beginner: 'bg-gray-100 text-gray-700', Intermediate: 'bg-blue-100 text-blue-700', Advanced: 'bg-green-100 text-green-700', Expert: 'bg-purple-100 text-purple-700' }[level] ?? 'bg-gray-100 text-gray-700'
}
function levelBarClass(level: SkillLevel) {
  return { Beginner: 'bg-gray-400', Intermediate: 'bg-blue-500', Advanced: 'bg-green-500', Expert: 'bg-purple-500' }[level] ?? 'bg-gray-400'
}
function levelDotClass(level: SkillLevel) {
  return { Beginner: 'bg-gray-400', Intermediate: 'bg-blue-500', Advanced: 'bg-green-500', Expert: 'bg-purple-500' }[level] ?? 'bg-gray-400'
}
function levelColorClass(level: SkillLevel) {
  return { Beginner: 'text-gray-400', Intermediate: 'text-blue-500', Advanced: 'text-green-500', Expert: 'text-purple-500' }[level] ?? 'text-gray-400'
}

async function load(uid: number) {
  isLoading.value = true
  try {
    const res = await httpService.get<Skill[]>(`/skills/user/${uid}/public`)
    if (res.success) skills.value = res.data || []
  } catch {
    // ignore
  } finally {
    isLoading.value = false
  }
}

onMounted(() => { if (userId?.value) load(userId.value) })
watch(() => userId?.value, (uid) => { if (uid) load(uid) })
</script>
