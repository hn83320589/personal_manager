<template>
  <section
    class="grid items-end gap-10 pt-16 sm:grid-cols-[1fr_auto] sm:pt-[72px]"
    aria-labelledby="intro-name"
  >
    <div class="flex max-w-[44rem] flex-col gap-5">
      <span
        v-if="profile.availabilityStatus"
        class="inline-flex w-fit items-center gap-2 text-[0.85rem] text-muted"
      >
        <i
          class="h-2 w-2 rounded-full bg-[#2fae6b] shadow-[0_0_0_4px_rgba(47,174,107,.2)]"
          aria-hidden="true"
        />
        {{ profile.availabilityStatus }}
      </span>
      <h1 id="intro-name" class="font-hand text-[clamp(2.6rem,7vw,4.6rem)] font-bold leading-[1.1]">
        {{ profile.fullName || profile.username }}
        <small class="mt-2.5 block font-latin text-[0.34em] font-medium tracking-[0.01em] text-muted">
          @{{ profile.username }}
        </small>
      </h1>
      <p v-if="profile.title" class="text-[clamp(1.1rem,2vw,1.3rem)] font-medium text-accent">
        {{ profile.title }}
      </p>
      <p v-if="profile.summary" class="max-w-[36rem] text-muted">{{ profile.summary }}</p>
      <div class="flex flex-wrap gap-2.5">
        <RouterLink :to="{ name: 'public-works', params }" class="btn btn-primary">看作品</RouterLink>
        <RouterLink :to="{ name: 'public-home', params, hash: '#contact' }" class="btn">
          聯絡我
        </RouterLink>
      </div>
    </div>
    <div
      class="order-first grid aspect-square w-[88px] place-items-center overflow-hidden rounded-[18px] border border-rule bg-accent/10 font-hand text-[2rem] text-accent sm:order-none sm:w-[clamp(120px,16vw,180px)] sm:rounded-3xl sm:text-[clamp(2.4rem,5vw,3.4rem)]"
    >
      <img
        v-if="profile.profileImageUrl"
        :src="profile.profileImageUrl"
        alt=""
        class="h-full w-full object-cover"
      />
      <span v-else aria-hidden="true">{{ initial }}</span>
    </div>
    <div
      v-if="facts.length"
      class="flex flex-wrap gap-x-7 gap-y-2 border-t border-rule pt-[18px] text-[0.88rem] text-muted sm:col-span-full"
    >
      <span v-for="fact in facts" :key="fact.label">
        {{ fact.label }}
        <a
          v-if="fact.href"
          :href="fact.href"
          target="_blank"
          rel="noopener"
          class="font-medium text-ink hover:text-accent"
        >
          {{ fact.value }}
        </a>
        <b v-else class="font-medium text-ink">{{ fact.value }}</b>
      </span>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Schemas } from '@/api/types'

const props = defineProps<{ profile: Schemas['ProfileDto'] }>()

const params = computed(() => ({ username: props.profile.username }))
const initial = computed(() =>
  (props.profile.fullName || props.profile.username).trim().charAt(0).toUpperCase(),
)

const facts = computed(() => {
  const list: { label: string; value: string; href?: string }[] = []
  if (props.profile.location) list.push({ label: '所在地', value: props.profile.location })
  if (props.profile.website) {
    list.push({
      label: '網站',
      value: props.profile.website.replace(/^https?:\/\//, ''),
      href: props.profile.website,
    })
  }
  return list
})
</script>
