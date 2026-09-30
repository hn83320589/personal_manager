<template>
  <AdminLayout>
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
      <!-- Profile Form -->
      <div class="lg:col-span-2">
        <BaseCard title="基本資料">
          <BaseForm @submit="handleSaveProfile" :show-actions="false">
            <BaseInput
              v-model="profileForm.title"
              label="職稱 *"
              placeholder="例如：前端工程師、全端開發者"
              required
            />

            <BaseTextarea
              v-model="profileForm.summary"
              label="個人簡介"
              placeholder="簡短介紹您的專業背景和技能..."
              :rows="3"
            />

            <BaseTextarea
              v-model="profileForm.description"
              label="詳細描述"
              placeholder="更詳細的個人介紹，包含工作經驗、專業技能等..."
              :rows="5"
            />

            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <BaseInput
                v-model="profileForm.location"
                label="所在地點"
                placeholder="例如：台北市, 台灣"
              />
              <BaseInput
                v-model="profileForm.website"
                type="url"
                label="個人網站"
                placeholder="https://your-website.com"
              />
            </div>

            <BaseInput
              v-model="profileForm.profileImageUrl"
              label="頭像圖片 URL"
              placeholder="https://example.com/avatar.jpg"
            />

            <!-- Theme selector -->
            <div class="space-y-2">
              <label class="block text-sm font-medium text-gray-700">頁面主題色</label>
              <div class="flex gap-3 flex-wrap">
                <button
                  v-for="opt in themeOptions"
                  :key="opt.key"
                  type="button"
                  @click="profileForm.themeColor = opt.key"
                  :title="opt.label"
                  class="w-9 h-9 rounded-full border-4 transition-all"
                  :style="{ backgroundColor: opt.color, borderColor: profileForm.themeColor === opt.key ? opt.color : 'transparent', outline: profileForm.themeColor === opt.key ? '2px solid ' + opt.color : 'none', outlineOffset: '2px' }"
                ></button>
              </div>
              <p class="text-xs text-gray-500">目前：{{ themeOptions.find(t => t.key === profileForm.themeColor)?.label }}</p>
            </div>

            <div class="flex justify-end space-x-4 pt-6">
              <BaseButton variant="outline" type="button" @click="resetForm">
                重置
              </BaseButton>
              <BaseButton
                type="submit"
                :loading="isSaving"
                :disabled="!isFormValid"
              >
                儲存變更
              </BaseButton>
            </div>
          </BaseForm>
        </BaseCard>
      </div>

      <!-- Profile Preview -->
      <div class="lg:col-span-1">
        <BaseCard title="預覽">
          <div class="space-y-4">
            <div class="text-center">
              <div
                class="mx-auto h-24 w-24 rounded-full flex items-center justify-center text-white text-3xl font-semibold"
                :style="{ backgroundColor: themeOptions.find(t => t.key === profileForm.themeColor)?.color ?? '#0ea5e9' }"
              >
                {{ getInitials(authStore.user?.fullName ?? '', '') }}
              </div>
              <h3 class="mt-3 text-lg font-semibold text-gray-900">{{ authStore.user?.fullName || authStore.user?.username }}</h3>
              <p v-if="profileForm.title" class="text-gray-600 text-sm">{{ profileForm.title }}</p>
              <p v-if="profileForm.location" class="text-xs text-gray-500 mt-1 flex items-center justify-center gap-1">
                <MapPinIcon class="w-3.5 h-3.5" />
                {{ profileForm.location }}
              </p>
            </div>

            <div v-if="profileForm.summary" class="text-sm text-gray-600 bg-gray-50 rounded-lg p-3">
              {{ profileForm.summary }}
            </div>

            <div v-if="profileForm.website" class="flex items-center gap-1 text-sm">
              <GlobeAltIcon class="w-4 h-4 text-gray-400" />
              <a :href="profileForm.website" target="_blank" class="text-primary-600 hover:underline truncate">
                {{ profileForm.website }}
              </a>
            </div>
          </div>
        </BaseCard>
      </div>
    </div>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, computed, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  MapPinIcon,
  EnvelopeIcon,
  PhoneIcon,
  BriefcaseIcon,
  CodeBracketIcon,
  ChatBubbleLeftEllipsisIcon,
  GlobeAltIcon
} from '@heroicons/vue/24/outline'
import { useProfileStore } from '@/stores/profile'
import { useAuthStore } from '@/stores/auth'
import AdminLayout from '@/components/layout/AdminLayout.vue'
import BaseCard from '@/components/ui/BaseCard.vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseTextarea from '@/components/ui/BaseTextarea.vue'
import BaseForm from '@/components/ui/BaseForm.vue'
import { themes } from '@/composables/useTheme'
import type { ThemeName } from '@/composables/useTheme'

// Router
const router = useRouter()

// Stores
const profileStore = useProfileStore()
const authStore = useAuthStore()

// State
const isSaving = ref(false)
const profileId = ref<number | null>(null)

const profileForm = reactive({
  title: '',
  summary: '',
  description: '',
  profileImageUrl: '',
  website: '',
  location: '',
  themeColor: 'blue' as ThemeName,
})

const themeOptions: { key: ThemeName; label: string; color: string }[] = [
  { key: 'blue', label: '天空藍', color: '#0ea5e9' },
  { key: 'green', label: '翠綠', color: '#22c55e' },
  { key: 'purple', label: '紫羅蘭', color: '#a855f7' },
  { key: 'rose', label: '玫瑰紅', color: '#f43f5e' },
  { key: 'slate', label: '石板灰', color: '#64748b' },
]

// Computed
const isFormValid = computed(() => profileForm.title.trim() !== '')

const hasSocialLinks = computed(() => profileForm.website)

// Methods
function getInitials(firstName: string, lastName: string): string {
  const first = firstName.charAt(0).toUpperCase()
  const last = lastName.charAt(0).toUpperCase()
  return first + last || 'U'
}

async function handleSaveProfile() {
  if (!isFormValid.value || isSaving.value) return

  isSaving.value = true

  try {
    let result
    if (profileId.value) {
      result = await profileStore.updateProfile(profileId.value, {
        title: profileForm.title,
        summary: profileForm.summary,
        description: profileForm.description,
        profileImageUrl: profileForm.profileImageUrl,
        website: profileForm.website,
        location: profileForm.location,
        themeColor: profileForm.themeColor,
      })
    } else {
      result = await profileStore.createProfile({
        userId: authStore.user!.id,
        title: profileForm.title,
        summary: profileForm.summary,
        description: profileForm.description,
        profileImageUrl: profileForm.profileImageUrl,
        website: profileForm.website,
        location: profileForm.location,
        themeColor: profileForm.themeColor,
      })
      if (result) profileId.value = result.id
    }
    if (result) {
      alert('個人資料已儲存！')
    } else {
      alert('儲存失敗，請稍後再試。')
    }
  } catch (error) {
    console.error('Save profile error:', error)
    alert('儲存失敗，請稍後再試。')
  } finally {
    isSaving.value = false
  }
}

function resetForm() {
  const p = profileStore.currentProfile
  if (p) {
    profileId.value = p.id
    profileForm.title = p.title
    profileForm.summary = p.summary
    profileForm.description = p.description
    profileForm.profileImageUrl = p.profileImageUrl
    profileForm.website = p.website
    profileForm.location = p.location
    profileForm.themeColor = (p.themeColor as ThemeName) || 'blue'
  }
}

// Lifecycle
onMounted(async () => {
  try {
    const uid = authStore.user?.id
    if (uid) {
      await profileStore.fetchCurrentUserProfile(uid)
      resetForm()
    }
  } catch (error) {
    console.error('Failed to load profile:', error)
  }
})
</script>

<style scoped>
/* Custom styles if needed */
</style>