<template>
  <AdminPage title="個人資料" description="顯示在公開頁面的介紹與呈現方式。">
    <template #actions>
      <RouterLink
        v-if="auth.user"
        :to="{ name: 'public-home', params: { username: auth.user.username } }"
        target="_blank"
        class="btn btn-small"
      >
        查看公開頁面 ↗
      </RouterLink>
    </template>

    <PageState :loading="loading" :error="loadError" @retry="reload">
      <form
        v-if="form"
        class="grid gap-6 min-[961px]:grid-cols-[minmax(0,1fr)_22rem] min-[961px]:items-start"
        @submit.prevent="save"
      >
        <section
          class="flex flex-col gap-4 rounded-xl border border-rule bg-surface p-5"
          aria-labelledby="basics-title"
        >
          <h2 id="basics-title" class="font-mono text-xs uppercase tracking-[0.06em] text-muted">
            基本資料
          </h2>
          <FormField v-slot="{ id }" label="頭像">
            <ImageUploadField :id="id" v-model="form.profileImageUrl" />
          </FormField>
          <div class="grid gap-4 sm:grid-cols-2">
            <FormField v-slot="{ id }" label="姓名" required>
              <input :id="id" v-model.trim="form.fullName" class="input" maxlength="100" required />
            </FormField>
            <FormField v-slot="{ id }" label="職稱" hint="例如：品牌設計師、後端工程師">
              <input :id="id" v-model="form.title" class="input" maxlength="100" />
            </FormField>
          </div>
          <FormField v-slot="{ id }" label="一句話介紹" hint="首頁名字下方的簡介，建議 60 字以內。">
            <textarea :id="id" v-model="form.summary" class="input min-h-20" maxlength="500" />
          </FormField>
          <div class="grid gap-4 sm:grid-cols-2">
            <FormField v-slot="{ id }" label="所在地">
              <input :id="id" v-model="form.location" class="input" maxlength="100" />
            </FormField>
            <FormField v-slot="{ id }" label="個人網站">
              <input
                :id="id"
                v-model.trim="form.website"
                type="url"
                class="input"
                maxlength="200"
                placeholder="https://"
              />
            </FormField>
          </div>
        </section>

        <div class="flex flex-col gap-4 min-[961px]:sticky min-[961px]:top-8">
          <PresentationSettings v-model="form" />
          <div class="flex items-center justify-end gap-3">
            <span v-if="!dirty" class="text-sm text-muted">所有變更都已儲存</span>
            <button type="submit" class="btn btn-primary" :disabled="!dirty || saving">
              {{ saving ? '儲存中…' : '儲存' }}
            </button>
          </div>
        </div>
      </form>
    </PageState>
  </AdminPage>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { profileApi } from '@/api/profile'
import type { Schemas } from '@/api/types'
import { useAsyncAction } from '@/composables/useAsyncAction'
import { useAsyncData } from '@/composables/useAsyncData'
import { useAuthStore } from '@/stores/auth'
import AdminPage from '@/components/manage/AdminPage.vue'
import FormField from '@/components/manage/FormField.vue'
import ImageUploadField from '@/components/manage/ImageUploadField.vue'
import PresentationSettings from '@/components/manage/profile/PresentationSettings.vue'
import type { ProfileForm } from '@/components/manage/profile/presentationOptions'
import PageState from '@/components/public/PageState.vue'

const auth = useAuthStore()
const { data: profile, loading, error: loadError, reload } = useAsyncData(profileApi.get)
const { run, running: saving } = useAsyncAction()

const form = ref<ProfileForm | null>(null)
const saved = ref('')

function toForm(p: Schemas['ProfileDto']): ProfileForm {
  return {
    fullName: p.fullName,
    title: p.title,
    summary: p.summary,
    description: p.description,
    profileImageUrl: p.profileImageUrl,
    website: p.website,
    location: p.location,
    themeColor: p.themeColor || 'blue',
    availabilityStatus: p.availabilityStatus,
    portfolioMode: p.portfolioMode,
    cardStyle: p.cardStyle,
    cardRatio: p.cardRatio,
    skillDisplay: p.skillDisplay,
  }
}

watch(profile, (p) => {
  if (!p) return
  form.value = toForm(p)
  saved.value = JSON.stringify(form.value)
})

const dirty = computed(() => !!form.value && JSON.stringify(form.value) !== saved.value)

async function save() {
  if (!form.value) return
  const updated = await run(() => profileApi.save(form.value!), { success: '已儲存個人資料' })
  if (updated) {
    form.value = toForm(updated)
    saved.value = JSON.stringify(form.value)
  }
}
</script>
