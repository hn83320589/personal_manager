<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Hero Section -->
    <section class="bg-white">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
        <div class="text-center">
          <h1 class="text-4xl font-bold text-gray-900 sm:text-5xl lg:text-6xl">
            學經歷
          </h1>
          <p class="mt-6 text-xl text-gray-600 max-w-3xl mx-auto">
            我的專業歷程與學術背景，塑造了我的專業能力。
          </p>
        </div>
      </div>
    </section>

    <!-- Main Content -->
    <section class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
      <div v-if="isLoading" class="flex justify-center">
        <LoadingSpinner />
      </div>

      <div v-else class="grid grid-cols-1 lg:grid-cols-2 gap-12">
        <!-- Work Experience -->
        <div class="space-y-8">
          <div class="text-center lg:text-left">
            <h2 class="text-3xl font-bold text-gray-900 mb-4">工作經歷</h2>
            <p class="text-gray-600">職涯中的專業角色與成就。</p>
          </div>

          <div v-if="workExperiences.length === 0" class="text-center py-12">
            <BriefcaseIcon class="mx-auto h-12 w-12 text-gray-400" />
            <h3 class="mt-2 text-sm font-medium text-gray-900">尚無工作經歷</h3>
            <p class="mt-1 text-sm text-gray-500">工作經歷資訊將顯示於此。</p>
          </div>

          <div v-else class="relative">
            <!-- Timeline line -->
            <div class="absolute left-8 top-6 bottom-6 w-px bg-gray-200"></div>

            <div class="space-y-8">
              <div
                v-for="(experience, index) in workExperiences"
                :key="experience.id"
                class="relative"
              >
                <!-- Timeline dot -->
                <div
                  class="absolute left-6 w-4 h-4 bg-primary-500 border-4 border-white rounded-full"
                ></div>

                <!-- Experience Card -->
                <div class="ml-16">
                  <BaseCard
                    :class="{
                      'border-primary-200 bg-primary-50': experience.isCurrent,
                    }"
                  >
                    <div class="space-y-3">
                      <div class="flex items-start justify-between">
                        <div class="flex-1">
                          <h3 class="text-lg font-semibold text-gray-900">
                            {{ experience.position }}
                            <span
                              v-if="experience.isCurrent"
                              class="ml-2 inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800"
                            >
                              現職
                            </span>
                          </h3>
                          <p class="text-primary-600 font-medium">
                            {{ experience.company }}
                          </p>
                        </div>
                      </div>

                      <div class="text-sm text-gray-600">
                        {{ formatDate(experience.startDate) }}
                        -
                        {{
                          experience.isCurrent
                            ? "至今"
                            : formatDate(experience.endDate)
                        }}
                        <span class="ml-2 text-gray-500">
                          ({{
                            calculateDuration(
                              experience.startDate,
                              experience.endDate,
                              experience.isCurrent,
                            )
                          }})
                        </span>
                      </div>

                      <div
                        v-if="experience.description"
                        class="prose prose-sm max-w-none text-gray-700"
                      >
                        <p class="whitespace-pre-line">
                          {{ experience.description }}
                        </p>
                      </div>
                    </div>
                  </BaseCard>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Education -->
        <div class="space-y-8">
          <div class="text-center lg:text-left">
            <h2 class="text-3xl font-bold text-gray-900 mb-4">學歷</h2>
            <p class="text-gray-600">塑造我基礎的學術背景與資歷。</p>
          </div>

          <div v-if="educations.length === 0" class="text-center py-12">
            <AcademicCapIcon class="mx-auto h-12 w-12 text-gray-400" />
            <h3 class="mt-2 text-sm font-medium text-gray-900">尚無學歷資料</h3>
            <p class="mt-1 text-sm text-gray-500">學歷資訊將顯示於此。</p>
          </div>

          <div v-else class="relative">
            <!-- Timeline line -->
            <div class="absolute left-8 top-6 bottom-6 w-px bg-gray-200"></div>

            <div class="space-y-8">
              <div
                v-for="(education, index) in educations"
                :key="education.id"
                class="relative"
              >
                <!-- Timeline dot -->
                <div
                  class="absolute left-6 w-4 h-4 bg-blue-500 border-4 border-white rounded-full"
                ></div>

                <!-- Education Card -->
                <div class="ml-16">
                  <BaseCard>
                    <div class="space-y-3">
                      <div>
                        <h3 class="text-lg font-semibold text-gray-900">
                          {{ education.degree }}
                          <span
                            v-if="education.fieldOfStudy"
                            class="text-base font-normal text-gray-600"
                          >
                            - {{ education.fieldOfStudy }}
                          </span>
                        </h3>
                        <p class="text-blue-600 font-medium">
                          {{ education.school }}
                        </p>
                      </div>

                      <div class="text-sm text-gray-600">
                        {{ education.startYear }}
                        -
                        {{ education.endYear || "至今" }}
                        <span
                          v-if="education.endYear"
                          class="ml-2 text-gray-500"
                        >
                          ({{
                            Number(education.endYear) -
                            Number(education.startYear)
                          }}
                          年)
                        </span>
                      </div>
                    </div>
                  </BaseCard>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Skills Section -->
      <section v-if="skills.length > 0" class="mt-16">
        <div class="text-center mb-12">
          <h2 class="text-3xl font-bold text-gray-900 mb-4">技能與專長</h2>
          <p class="text-gray-600">在歷程中培養的技術能力與專業技能。</p>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          <BaseCard
            v-for="(categorySkills, category) in skillsByCategory"
            :key="category"
            :title="category"
          >
            <div class="space-y-3">
              <div
                v-for="skill in categorySkills"
                :key="skill.id"
                class="flex items-center justify-between"
              >
                <span class="text-gray-700">{{ skill.name }}</span>
                <div class="flex items-center space-x-2">
                  <div class="flex space-x-1">
                    <div
                      v-for="dot in 4"
                      :key="dot"
                      :class="[
                        'w-2 h-2 rounded-full',
                        dot <= skillLevelToNumber(skill.level)
                          ? 'bg-primary-500'
                          : 'bg-gray-200',
                      ]"
                    ></div>
                  </div>
                  <span class="text-xs text-gray-500">
                    {{ skill.level }}
                  </span>
                </div>
              </div>
            </div>
          </BaseCard>
        </div>
      </section>

      <!-- Call to Action -->
      <section class="mt-16">
        <BaseCard class="bg-primary-50 border-primary-200">
          <div class="text-center space-y-4">
            <h3 class="text-xl font-semibold text-gray-900">想了解更多？</h3>
            <p class="text-gray-600">
              瀏覽我的作品集，看看我如何將這些技能應用在實際專案中。
            </p>
            <div class="flex justify-center space-x-4">
              <router-link to="/portfolio">
                <BaseButton variant="primary"> 查看作品集 </BaseButton>
              </router-link>
              <router-link to="/contact">
                <BaseButton variant="outline"> 與我聯繫 </BaseButton>
              </router-link>
            </div>
          </div>
        </BaseCard>
      </section>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from "vue";
import { BriefcaseIcon, AcademicCapIcon } from "@heroicons/vue/24/outline";
import { useExperienceStore } from "@/stores/experience";
import { useSkillStore } from "@/stores/skill";
import LoadingSpinner from "@/components/common/LoadingSpinner.vue";
import BaseCard from "@/components/ui/BaseCard.vue";
import BaseButton from "@/components/ui/BaseButton.vue";
import type { SkillLevel } from "@/types/api";

// Stores
const experienceStore = useExperienceStore();
const skillStore = useSkillStore();

// State
const isLoading = ref(true);

// Computed
const workExperiences = computed(() => experienceStore.publicWorkExperiences);
const educations = computed(() => experienceStore.publicEducations);
const skills = computed(() => skillStore.publicSkills);
const skillsByCategory = computed(() => skillStore.skillsByCategory);

// Methods
function skillLevelToNumber(level: SkillLevel): number {
  const map: Record<SkillLevel, number> = {
    Beginner: 1,
    Intermediate: 2,
    Advanced: 3,
    Expert: 4,
  };
  return map[level] || 0;
}

function formatDate(dateString?: string): string {
  if (!dateString) return "";

  const date = new Date(dateString);
  return date.toLocaleDateString("zh-TW", {
    year: "numeric",
    month: "short",
  });
}

function calculateDuration(
  startDate: string,
  endDate?: string,
  isCurrent?: boolean,
): string {
  const start = new Date(startDate);
  const end = isCurrent ? new Date() : endDate ? new Date(endDate) : new Date();

  const diffInMonths =
    (end.getFullYear() - start.getFullYear()) * 12 +
    (end.getMonth() - start.getMonth());

  if (diffInMonths < 12) {
    return `${diffInMonths} 個月`;
  }

  const years = Math.floor(diffInMonths / 12);
  const months = diffInMonths % 12;

  let duration = `${years} 年`;
  if (months > 0) {
    duration += ` ${months} 個月`;
  }

  return duration;
}

// Lifecycle
onMounted(async () => {
  try {
    await Promise.all([
      experienceStore.fetchWorkExperiences(),
      experienceStore.fetchEducations(),
      skillStore.fetchSkills(),
    ]);
  } catch (error) {
    console.error("Failed to load experience data:", error);
  } finally {
    isLoading.value = false;
  }
});
</script>
