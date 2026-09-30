<template>
  <AdminPage
    title="經歷"
    description="公開頁面「經歷與技能」區塊會依這裡的順序顯示，工作經歷在前、學歷在後。"
  >
    <WorkExperienceSection />
    <EducationSection class="mt-4" />
  </AdminPage>
</template>

<script setup lang="ts">
import AdminPage from '@/components/manage/AdminPage.vue'
import EducationSection from '@/components/manage/resume/EducationSection.vue'
import WorkExperienceSection from '@/components/manage/resume/WorkExperienceSection.vue'
</script>
