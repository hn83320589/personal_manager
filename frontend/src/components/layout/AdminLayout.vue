<template>
  <div class="min-h-screen bg-gray-50 flex">
    <!-- Sidebar -->
    <div class="hidden md:flex md:w-64 md:flex-col">
      <div
        class="flex flex-col flex-grow pt-5 pb-4 overflow-y-auto bg-white border-r border-gray-200"
      >
        <!-- Logo/Brand -->
        <div class="flex items-center flex-shrink-0 px-4">
          <router-link to="/" class="flex items-center">
            <div
              class="w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center"
            >
              <span class="text-white font-bold text-sm">PM</span>
            </div>
            <span class="ml-2 text-lg font-semibold text-gray-900"
              >Personal Manager</span
            >
          </router-link>
        </div>

        <!-- Navigation -->
        <nav class="mt-8 flex-1 px-2 space-y-1">
          <!-- Dashboard -->
          <router-link
            to="/admin/dashboard"
            class="nav-link"
            active-class="nav-link-active"
          >
            <HomeIcon class="nav-icon" />
            儀表板
          </router-link>

          <!-- Content Management -->
          <div class="mt-6">
            <h3
              class="px-3 text-xs font-semibold text-gray-500 uppercase tracking-wider"
            >
              內容管理
            </h3>
            <div class="mt-2 space-y-1">
              <router-link
                to="/admin/profile"
                class="nav-link"
                active-class="nav-link-active"
              >
                <UserIcon class="nav-icon" />
                個人資料
              </router-link>

              <router-link
                to="/admin/experience"
                class="nav-link"
                active-class="nav-link-active"
              >
                <AcademicCapIcon class="nav-icon" />
                學經歷
              </router-link>

              <router-link
                to="/admin/skills"
                class="nav-link"
                active-class="nav-link-active"
              >
                <CpuChipIcon class="nav-icon" />
                專長技能
              </router-link>

              <router-link
                to="/admin/projects"
                class="nav-link"
                active-class="nav-link-active"
              >
                <BriefcaseIcon class="nav-icon" />
                作品專案
              </router-link>

              <router-link
                to="/admin/contacts"
                class="nav-link"
                active-class="nav-link-active"
              >
                <PhoneIcon class="nav-icon" />
                聯絡方式
              </router-link>

              <router-link
                to="/admin/files"
                class="nav-link"
                active-class="nav-link-active"
              >
                <FolderIcon class="nav-icon" />
                檔案管理
              </router-link>
            </div>
          </div>

          <!-- Interactive Features -->
          <div class="mt-6">
            <h3
              class="px-3 text-xs font-semibold text-gray-500 uppercase tracking-wider"
            >
              互動功能
            </h3>
            <div class="mt-2 space-y-1">
              <router-link
                to="/admin/blog"
                class="nav-link"
                active-class="nav-link-active"
              >
                <DocumentTextIcon class="nav-icon" />
                部落格文章
              </router-link>

              <router-link
                to="/admin/comments"
                class="nav-link"
                active-class="nav-link-active"
              >
                <ChatBubbleLeftEllipsisIcon class="nav-icon" />
                留言管理
              </router-link>

              <router-link
                to="/admin/calendar"
                class="nav-link"
                active-class="nav-link-active"
              >
                <CalendarDaysIcon class="nav-icon" />
                行事曆
              </router-link>
            </div>
          </div>

          <!-- Task Management -->
          <div class="mt-6">
            <h3
              class="px-3 text-xs font-semibold text-gray-500 uppercase tracking-wider"
            >
              任務管理
            </h3>
            <div class="mt-2 space-y-1">
              <router-link
                to="/admin/tasks"
                class="nav-link"
                active-class="nav-link-active"
              >
                <CheckCircleIcon class="nav-icon" />
                待辦事項
              </router-link>

              <router-link
                to="/admin/work-tracking"
                class="nav-link"
                active-class="nav-link-active"
              >
                <ClockIcon class="nav-icon" />
                工作追蹤
              </router-link>
            </div>
          </div>
        </nav>

        <!-- User Info & Logout -->
        <div class="flex-shrink-0 flex border-t border-gray-200 p-4">
          <div class="flex items-center w-full">
            <div class="flex-shrink-0">
              <div
                class="w-8 h-8 bg-gray-300 rounded-full flex items-center justify-center"
              >
                <UserIcon class="w-5 h-5 text-gray-600" />
              </div>
            </div>
            <div class="ml-3 flex-1">
              <p class="text-sm font-medium text-gray-700">
                {{ authStore.userDisplayName }}
              </p>
              <p class="text-xs text-gray-500">{{ authStore.userRole }}</p>
            </div>
            <BaseButton
              variant="outline"
              size="small"
              @click="handleLogout"
              class="ml-3"
            >
              <ArrowRightOnRectangleIcon class="w-4 h-4" />
            </BaseButton>
          </div>
        </div>
      </div>
    </div>

    <!-- Mobile sidebar (overlay) -->
    <div
      v-if="mobileMenuOpen"
      class="fixed inset-0 z-40 flex md:hidden"
      @click="mobileMenuOpen = false"
    >
      <div class="fixed inset-0 bg-gray-600 bg-opacity-75"></div>
      <div class="relative flex-1 flex flex-col max-w-xs w-full bg-white">
        <!-- Mobile navigation content (similar to desktop) -->
        <div class="absolute top-0 right-0 -mr-12 pt-2">
          <button
            @click="mobileMenuOpen = false"
            class="ml-1 flex items-center justify-center h-10 w-10 rounded-full focus:outline-none focus:ring-2 focus:ring-inset focus:ring-white"
          >
            <XMarkIcon class="h-6 w-6 text-white" />
          </button>
        </div>
        <!-- Same nav content as desktop -->
        <div class="flex-1 h-0 pt-5 pb-4 overflow-y-auto">
          <!-- Mobile nav content here if needed -->
        </div>
      </div>
    </div>

    <!-- Main content -->
    <div class="flex flex-col w-0 flex-1 overflow-hidden">
      <!-- Top bar -->
      <div class="relative z-10 flex-shrink-0 flex h-16 bg-white shadow">
        <!-- Mobile menu button -->
        <button
          @click="mobileMenuOpen = true"
          class="px-4 border-r border-gray-200 text-gray-500 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-indigo-500 md:hidden"
        >
          <Bars3Icon class="h-6 w-6" />
        </button>

        <!-- Page title -->
        <div class="flex-1 px-4 flex justify-between items-center">
          <div>
            <h1 class="text-lg font-semibold text-gray-900">
              {{ pageTitle }}
            </h1>
          </div>

          <!-- Quick actions -->
          <div class="flex items-center space-x-2">
            <BaseButton variant="outline" size="small" @click="openPreview">
              <EyeIcon class="w-4 h-4 mr-2" />
              預覽網站
            </BaseButton>
          </div>
        </div>
      </div>

      <!-- Page content -->
      <main
        class="flex-1 relative overflow-y-auto focus:outline-none bg-gray-50"
      >
        <div class="py-6">
          <div class="max-w-7xl mx-auto px-4 sm:px-6 md:px-8">
            <slot />
          </div>
        </div>
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from "vue";
import { useRouter, useRoute } from "vue-router";
import {
  HomeIcon,
  UserIcon,
  AcademicCapIcon,
  CpuChipIcon,
  BriefcaseIcon,
  PhoneIcon,
  FolderIcon,
  DocumentTextIcon,
  ChatBubbleLeftEllipsisIcon,
  CalendarDaysIcon,
  CheckCircleIcon,
  ClockIcon,
  ArrowRightOnRectangleIcon,
  Bars3Icon,
  XMarkIcon,
  EyeIcon,
} from "@heroicons/vue/24/outline";
import { useAuthStore } from "@/stores/auth";
import BaseButton from "@/components/ui/BaseButton.vue";

// Router & Route
const router = useRouter();
const route = useRoute();

// Stores
const authStore = useAuthStore();

// State
const mobileMenuOpen = ref(false);

// Computed
const pageTitle = computed(() => {
  return (route.meta.title as string) || "管理後台";
});

// Methods
function openPreview() {
  window.open("/", "_blank");
}

async function handleLogout() {
  try {
    await authStore.logout();
    router.push("/");
  } catch (error) {
    console.error("Logout error:", error);
  }
}
</script>

<style scoped>
.nav-link {
  @apply w-full flex items-center px-2 py-2 text-sm font-medium rounded-md text-gray-600 hover:bg-gray-50 hover:text-gray-900 transition-colors duration-150;
}

.nav-link.group {
  /* 如果需要 group 功能，在模板中直接添加 class="group" */
}

.nav-link-active {
  @apply bg-blue-50 text-blue-600 border-r-2 border-blue-600;
}

.nav-icon {
  @apply mr-3 flex-shrink-0 h-5 w-5;
}
</style>
