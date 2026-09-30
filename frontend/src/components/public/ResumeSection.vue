<template>
  <div class="grid gap-14 min-[861px]:grid-cols-[minmax(0,1.3fr)_minmax(0,1fr)]">
    <ol v-if="entries.length" class="flex flex-col" aria-label="經歷與學歷">
      <li
        v-for="entry in entries"
        :key="entry.key"
        class="grid gap-0.5 border-t border-rule py-[18px] last:border-b min-[521px]:grid-cols-[8.5rem_1fr] min-[521px]:gap-4"
      >
        <span class="pt-[3px] font-mono text-[0.8rem] tabular-nums text-muted">{{ entry.period }}</span>
        <div>
          <h3 class="font-bold">{{ entry.title }}</h3>
          <div class="text-[0.92rem] text-muted">{{ entry.org }}</div>
          <p v-if="entry.description" class="mt-1.5 whitespace-pre-line text-[0.92rem] text-muted">
            {{ entry.description }}
          </p>
        </div>
      </li>
    </ol>
    <p v-else class="text-muted">尚未填寫經歷。</p>

    <div v-if="skillGroups.length" class="flex flex-col gap-[22px]">
      <div v-for="group in skillGroups" :key="group.name">
        <h3 class="mb-2.5 font-mono text-[0.8rem] font-medium uppercase tracking-[0.06em] text-muted">
          {{ group.name }}
        </h3>
        <ul class="flex flex-col">
          <li
            v-for="skill in group.skills"
            :key="skill.id"
            class="flex justify-between gap-3 border-b border-dashed border-rule py-[7px] text-[0.95rem]"
          >
            {{ skill.name }}
            <span v-if="skillDetail(skill)" class="text-[0.82rem] text-muted">{{ skillDetail(skill) }}</span>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Schemas } from '@/api/types'
import { formatPeriod } from '@/lib/format'

type Skill = Schemas['PublicSkillDto']

const props = defineProps<{
  works: Schemas['PublicWorkExperienceDto'][]
  educations: Schemas['PublicEducationDto'][]
  skills: Skill[]
  skillDisplay: Schemas['SkillDisplay']
}>()

/** 工作經歷在前，學歷接在後面，都已由後端依使用者設定的順序排列。 */
const entries = computed(() => [
  ...props.works.map((w) => ({
    key: `w${w.id}`,
    period: formatPeriod({ start: w.startDate, end: w.endDate, isCurrent: w.isCurrent }),
    title: w.position,
    org: w.company,
    description: w.description,
  })),
  ...props.educations.map((e) => ({
    key: `e${e.id}`,
    period: formatPeriod({ start: e.startYear, end: e.endYear }),
    title: [e.degree, e.fieldOfStudy].filter(Boolean).join(' · ') || e.school,
    org: e.school,
    description: e.description,
  })),
])

const skillGroups = computed(() => {
  const groups = new Map<string, Skill[]>()
  for (const skill of props.skills) {
    const name = skill.category || '技能'
    groups.set(name, [...(groups.get(name) ?? []), skill])
  }
  return [...groups].map(([name, skills]) => ({ name, skills }))
})

const levelLabel: Record<Schemas['SkillLevel'], string> = {
  Beginner: '入門',
  Intermediate: '中等',
  Advanced: '進階',
  Expert: '專家',
}

function skillDetail(skill: Skill): string {
  if (props.skillDisplay === 'Level' && skill.level) return levelLabel[skill.level]
  if (props.skillDisplay === 'Years' && skill.yearsOfExperience) return `${skill.yearsOfExperience} 年`
  return ''
}
</script>
