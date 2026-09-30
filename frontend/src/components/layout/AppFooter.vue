<template>
  <footer class="bg-white border-t border-gray-200">
    <div class="mx-auto max-w-7xl overflow-hidden px-6 py-20 sm:py-24 lg:px-8">
      <!-- Main footer content -->
      <nav
        class="-mb-6 columns-2 sm:flex sm:justify-center sm:space-x-12"
        aria-label="Footer"
      >
        <div v-for="item in navigation" :key="item.name" class="pb-6">
          <RouterLink
            :to="item.href"
            class="text-sm leading-6 text-gray-600 hover:text-primary-600 transition-colors duration-200"
          >
            {{ item.name }}
          </RouterLink>
        </div>
      </nav>

      <!-- Social media links -->
      <div class="mt-10 flex justify-center space-x-10">
        <a
          v-for="item in socialMedia"
          :key="item.name"
          :href="item.href"
          target="_blank"
          rel="noopener noreferrer"
          class="text-gray-400 hover:text-primary-500 transition-colors duration-200"
        >
          <span class="sr-only">{{ item.name }}</span>
          <component :is="item.icon" class="h-6 w-6" aria-hidden="true" />
        </a>
      </div>

      <!-- Copyright -->
      <p class="mt-10 text-center text-xs leading-5 text-gray-500">
        &copy; {{ currentYear }} Personal Manager. 版權所有。
      </p>

      <!-- Additional info -->
      <div class="mt-6 text-center">
        <p class="text-xs text-gray-400">
          使用 Vue 3 + TypeScript + Tailwind CSS 建構
        </p>
        <p class="mt-1 text-xs text-gray-400">版本 {{ appVersion }}</p>
      </div>
    </div>
  </footer>
</template>

<script setup lang="ts">
import { computed } from "vue";

// Import icons - you may need to install @heroicons/vue if not already installed
// For now, using placeholder components
const GitHubIcon = "div";
const LinkedInIcon = "div";
const TwitterIcon = "div";
const EmailIcon = "div";

const currentYear = computed(() => new Date().getFullYear());
const appVersion = computed(() => "1.0.0"); // You can get this from package.json

const navigation = [
  { name: "關於我", href: "/about" },
  { name: "作品集", href: "/portfolio" },
  { name: "部落格", href: "/blog" },
  { name: "聯絡我", href: "/contact" },
  { name: "隱私政策", href: "/privacy" },
  { name: "服務條款", href: "/terms" },
];

const socialMedia = [
  {
    name: "GitHub",
    href: "#",
    icon: GitHubIcon,
  },
  {
    name: "LinkedIn",
    href: "#",
    icon: LinkedInIcon,
  },
  {
    name: "Twitter",
    href: "#",
    icon: TwitterIcon,
  },
  {
    name: "Email",
    href: "mailto:contact@example.com",
    icon: EmailIcon,
  },
];
</script>
