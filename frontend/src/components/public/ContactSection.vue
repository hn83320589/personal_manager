<template>
  <section
    id="contact"
    class="mt-24 grid scroll-mt-24 gap-8 rounded-2xl border border-rule bg-surface p-[clamp(28px,5vw,56px)] min-[761px]:grid-cols-[1fr_auto] min-[761px]:items-center"
    aria-labelledby="contact-title"
  >
    <div class="max-w-[30rem]">
      <h2 id="contact-title" class="font-hand text-[clamp(1.6rem,3vw,2.2rem)] font-bold">想合作或聊聊？</h2>
      <p class="mt-3 text-muted">委託、職缺或只是想交流，都歡迎聯絡。也可以到留言板打聲招呼。</p>
      <RouterLink :to="{ name: 'public-guestbook', params: { username } }" class="btn mt-6">留言板</RouterLink>
    </div>

    <ul v-if="contacts.length" class="flex min-w-0 flex-col gap-2 min-[761px]:min-w-[20rem]">
      <li
        v-for="contact in contacts"
        :key="contact.id"
        class="flex items-center justify-between gap-4 rounded-lg border border-rule bg-paper px-4 py-3"
      >
        <div class="min-w-0">
          <div class="font-mono text-[0.72rem] uppercase tracking-[0.06em] text-muted">
            {{ contact.label || typeLabel[contact.type] }}
          </div>
          <a
            v-if="linkOf(contact)"
            :href="linkOf(contact)"
            :target="isWebLink(contact) ? '_blank' : undefined"
            :rel="isWebLink(contact) ? 'noopener' : undefined"
            class="block truncate font-medium hover:text-accent"
          >
            {{ displayValue(contact) }}
          </a>
          <span v-else class="block truncate font-medium">{{ contact.value }}</span>
        </div>
        <button
          type="button"
          class="shrink-0 rounded-md px-2 py-1 text-sm text-muted hover:bg-soft hover:text-ink"
          @click="copy(contact)"
        >
          {{ copiedId === contact.id ? '已複製' : '複製' }}
        </button>
      </li>
    </ul>
  </section>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import type { Schemas } from '@/api/types'

type Contact = Schemas['PublicContactMethodDto']

defineProps<{ contacts: Contact[]; username: string }>()

const typeLabel: Record<Schemas['ContactType'], string> = {
  Email: 'Email',
  Phone: '電話',
  LinkedIn: 'LinkedIn',
  GitHub: 'GitHub',
  Facebook: 'Facebook',
  Twitter: 'X',
  Instagram: 'Instagram',
  Discord: 'Discord',
  Other: '其他',
  Behance: 'Behance',
  Dribbble: 'Dribbble',
  YouTube: 'YouTube',
  Threads: 'Threads',
  Line: 'LINE',
  Website: '網站',
}

const isWebLink = (c: Contact) => /^https?:\/\//i.test(c.value)

function linkOf(c: Contact): string | undefined {
  if (c.type === 'Email') return `mailto:${c.value}`
  if (c.type === 'Phone') return `tel:${c.value.replace(/[^\d+]/g, '')}`
  return isWebLink(c) ? c.value : undefined
}

const displayValue = (c: Contact) => c.value.replace(/^https?:\/\/(www\.)?/i, '').replace(/\/$/, '')

const copiedId = ref<number | null>(null)
let resetTimer: ReturnType<typeof setTimeout> | undefined

async function copy(c: Contact) {
  try {
    await navigator.clipboard.writeText(c.value)
    copiedId.value = c.id
    clearTimeout(resetTimer)
    resetTimer = setTimeout(() => (copiedId.value = null), 2000)
  } catch {
    window.prompt('請手動複製', c.value)
  }
}
</script>
