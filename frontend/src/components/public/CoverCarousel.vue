<template>
  <div
    ref="root"
    class="absolute inset-0"
    @mouseenter="paused = true"
    @mouseleave="paused = false"
    @focusin="paused = true"
    @focusout="paused = false"
    @touchstart.passive="onTouchStart"
    @touchend="onTouchEnd"
  >
    <img
      v-for="(image, i) in images"
      :key="image.url + i"
      :src="image.url"
      :alt="i === 0 ? image.alt || title : ''"
      :data-active="i === current"
      :loading="i === 0 ? 'eager' : 'lazy'"
      :style="{ objectPosition: focus }"
      class="absolute inset-0 h-full w-full object-cover opacity-0 transition-[opacity,transform] duration-700 data-[active=true]:opacity-100 group-hover:data-[active=true]:scale-[1.02]"
    />
    <template v-if="images.length > 1">
      <span class="absolute right-2.5 top-2.5 z-[2] rounded bg-black/55 px-1.5 font-mono text-[0.7rem] text-white">
        {{ current + 1 }}/{{ images.length }}
      </span>
      <div class="absolute inset-x-0 bottom-2.5 z-[2] flex justify-center gap-1.5">
        <button
          v-for="(_, i) in images"
          :key="i"
          type="button"
          class="grid h-[22px] w-[22px] place-items-center"
          :aria-label="`第 ${i + 1} 張封面`"
          :aria-current="i === current"
          @click.prevent="current = i"
        >
          <i
            :class="[
              'block h-[7px] rounded-full shadow-[0_0_0_1px_rgba(0,0,0,.18)] transition-all',
              i === current ? 'w-[18px] bg-white' : 'w-[7px] bg-white/55',
            ]"
          />
        </button>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import type { Schemas } from '@/api/types'

const props = withDefaults(
  defineProps<{ images: Schemas['PortfolioImage'][]; title: string; focus?: string; interval?: number }>(),
  { focus: '50% 50%', interval: 4500 },
)

const root = ref<HTMLElement>()
const current = ref(0)
const paused = ref(false)
const visible = ref(true)
let timer: ReturnType<typeof setInterval> | undefined
let observer: IntersectionObserver | undefined
let touchX: number | null = null

function go(step: number) {
  current.value = (current.value + step + props.images.length) % props.images.length
}

onMounted(() => {
  const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches
  if (props.images.length < 2 || reduceMotion) return
  // 只在卡片出現在畫面上、且訪客沒有停在上面時輪播
  if ('IntersectionObserver' in window) {
    observer = new IntersectionObserver(([entry]) => (visible.value = entry?.isIntersecting ?? true))
    if (root.value) observer.observe(root.value)
  }
  timer = setInterval(() => {
    if (!paused.value && visible.value) go(1)
  }, props.interval)
})

onUnmounted(() => {
  clearInterval(timer)
  observer?.disconnect()
})

function onTouchStart(e: TouchEvent) {
  touchX = e.touches[0]?.clientX ?? null
}

function onTouchEnd(e: TouchEvent) {
  const endX = e.changedTouches[0]?.clientX
  if (touchX === null || endX === undefined) return
  const dx = endX - touchX
  touchX = null
  if (Math.abs(dx) > 40) {
    e.preventDefault() // 滑動換圖時不要觸發卡片連結
    go(dx < 0 ? 1 : -1)
  }
}
</script>
