<template>
  <div class="min-h-screen px-4 sm:px-6 lg:px-10">
    <SiteNav :username="username" :full-name="profile?.fullName" />
    <main id="main" class="mx-auto max-w-wrap" tabindex="-1">
      <PageState
        :loading="loading && !profile"
        :error="error"
        :not-found="notFound"
        not-found-text="找不到這位使用者"
        @retry="reload"
      >
        <RouterView />
      </PageState>
    </main>
    <SiteFooter :username="username" :full-name="profile?.fullName" />
  </div>
</template>

<script setup lang="ts">
import { computed, provide } from 'vue'
import { useRoute } from 'vue-router'
import { publicApi } from '@/api/public'
import { useAsyncData } from '@/composables/useAsyncData'
import { useAccent } from '@/composables/useAccent'
import PageState from '@/components/public/PageState.vue'
import SiteFooter from '@/components/public/SiteFooter.vue'
import SiteNav from '@/components/public/SiteNav.vue'
import { publicContextKey } from '@/components/public/publicContext'

const route = useRoute()
const username = computed(() => String(route.params.username))

const {
  data: profile,
  loading,
  error,
  notFound,
  reload,
} = useAsyncData(() => publicApi.profile(username.value), { watch: [username] })

useAccent(computed(() => profile.value?.themeColor))
provide(publicContextKey, { username, profile })
</script>
