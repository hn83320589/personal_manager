<template>
  <template v-if="profile">
    <IntroSection :profile="profile" />

    <PageState :loading="loading" :error="error" @retry="reload">
      <template v-if="data">
        <section v-if="data.works.length" class="mt-24" aria-labelledby="works-title">
          <SectionHeading
            id="works-title"
            title="作品"
            :link="{ to: { name: 'public-works', params }, label: `全部 ${data.works.length} 件` }"
          />
          <WorkGrid
            :works="data.works.slice(0, homeWorkLimit)"
            :username="username"
            :variant="profile.cardStyle"
            :ratio="profile.cardRatio"
          />
        </section>

        <section
          v-if="hasResume"
          id="about"
          class="mt-24 scroll-mt-24"
          aria-labelledby="about-title"
        >
          <SectionHeading id="about-title" title="經歷與技能" />
          <ResumeSection
            :works="data.experiences"
            :educations="data.educations"
            :skills="data.skills"
            :skill-display="profile.skillDisplay"
          />
        </section>

        <section v-if="data.posts.length" class="mt-24" aria-labelledby="writing-title">
          <SectionHeading
            id="writing-title"
            title="最近的文章"
            :link="{ to: { name: 'public-blog', params }, label: '所有文章' }"
          />
          <PostList :posts="data.posts" :username="username" />
        </section>

        <ContactSection :contacts="data.contacts" :username="username" />
      </template>
    </PageState>
  </template>
</template>

<script setup lang="ts">
import { computed, watchEffect } from 'vue'
import { publicApi } from '@/api/public'
import { useAsyncData } from '@/composables/useAsyncData'
import { setPageSeo } from '@/composables/useSeo'
import ContactSection from '@/components/public/ContactSection.vue'
import IntroSection from '@/components/public/IntroSection.vue'
import PageState from '@/components/public/PageState.vue'
import PostList from '@/components/public/PostList.vue'
import ResumeSection from '@/components/public/ResumeSection.vue'
import SectionHeading from '@/components/public/SectionHeading.vue'
import WorkGrid from '@/components/public/WorkGrid.vue'
import { usePublicContext } from '@/components/public/publicContext'

/** 首頁最多顯示的作品數，其餘到作品頁查看。 */
const homeWorkLimit = 6

const { username, profile } = usePublicContext()
const params = computed(() => ({ username: username.value }))

const { data, loading, error, reload } = useAsyncData(
  async () => {
    const u = username.value
    const [works, experiences, educations, skills, posts, contacts] = await Promise.all([
      publicApi.portfolios(u),
      publicApi.workExperiences(u),
      publicApi.educations(u),
      publicApi.skills(u),
      publicApi.posts(u, { pageSize: 3 }),
      publicApi.contactMethods(u),
    ])
    return { works, experiences, educations, skills, posts: posts.items, contacts }
  },
  { watch: [username] },
)

const hasResume = computed(
  () => !!data.value && (data.value.experiences.length + data.value.educations.length + data.value.skills.length > 0),
)

watchEffect(() => {
  if (!profile.value) return
  setPageSeo({
    title: profile.value.fullName || profile.value.username,
    description: profile.value.summary || profile.value.title,
    ogImage: profile.value.profileImageUrl || undefined,
    ogType: 'profile',
  })
})
</script>
