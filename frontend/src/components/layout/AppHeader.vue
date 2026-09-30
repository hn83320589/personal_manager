<template>
  <header class="bg-white shadow-sm border-b border-gray-200">
    <nav class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8" aria-label="Global">
      <div class="flex h-16 items-center justify-between">
        <!-- Logo / Brand -->
        <div class="flex items-center">
          <RouterLink to="/" class="flex items-center">
            <div class="flex-shrink-0">
              <h1 class="text-xl font-bold text-primary-600">個人管理網站</h1>
            </div>
          </RouterLink>
        </div>

        <!-- Desktop Navigation -->
        <div class="hidden md:flex md:items-center md:space-x-8">
          <RouterLink
            v-for="item in navigation"
            :key="item.name"
            :to="item.href"
            :class="[
              'text-sm font-medium transition-colors duration-200',
              isActiveRoute(item.href)
                ? 'text-primary-600 border-b-2 border-primary-600'
                : 'text-gray-600 hover:text-primary-600'
            ]"
            class="pb-4 pt-4"
          >
            {{ item.name }}
          </RouterLink>
        </div>

        <!-- User Menu -->
        <div class="flex items-center space-x-4">
          <!-- Desktop user menu -->
          <div v-if="isAuthenticated" class="hidden md:flex md:items-center md:space-x-4">
            <Menu as="div" class="relative">
              <MenuButton class="flex items-center space-x-2 text-sm text-gray-600 hover:text-primary-600">
                <UserCircleIcon class="h-8 w-8" />
                <span>{{ userDisplayName }}</span>
                <ChevronDownIcon class="h-4 w-4" />
              </MenuButton>
              
              <transition
                enter-active-class="transition duration-100 ease-out"
                enter-from-class="transform scale-95 opacity-0"
                enter-to-class="transform scale-100 opacity-100"
                leave-active-class="transition duration-75 ease-in"
                leave-from-class="transform scale-100 opacity-100"
                leave-to-class="transform scale-95 opacity-0"
              >
                <MenuItems class="absolute right-0 z-10 mt-2 w-48 origin-top-right rounded-md bg-white py-1 shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none">
                  <MenuItem v-for="item in userNavigation" :key="item.name" v-slot="{ active }">
                    <RouterLink
                      v-if="item.href"
                      :to="item.href"
                      :class="[
                        active ? 'bg-gray-100' : '',
                        'block px-4 py-2 text-sm text-gray-700'
                      ]"
                    >
                      {{ item.name }}
                    </RouterLink>
                    <button
                      v-else
                      @click="handleLogout"
                      :class="[
                        active ? 'bg-gray-100' : '',
                        'block w-full text-left px-4 py-2 text-sm text-gray-700'
                      ]"
                    >
                      {{ item.name }}
                    </button>
                  </MenuItem>
                </MenuItems>
              </transition>
            </Menu>
          </div>
          
          <!-- Login button for unauthenticated users -->
          <div v-else class="hidden md:flex">
            <RouterLink to="/login">
              <BaseButton variant="primary" size="small">
                登入
              </BaseButton>
            </RouterLink>
          </div>

          <!-- Mobile menu button -->
          <div class="flex md:hidden">
            <button
              type="button"
              class="inline-flex items-center justify-center rounded-md p-2 text-gray-600 hover:bg-gray-100 hover:text-primary-600 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-primary-500"
              aria-controls="mobile-menu"
              aria-expanded="false"
              @click="isMobileMenuOpen = !isMobileMenuOpen"
            >
              <span class="sr-only">Open main menu</span>
              <Bars3Icon v-if="!isMobileMenuOpen" class="block h-6 w-6" aria-hidden="true" />
              <XMarkIcon v-else class="block h-6 w-6" aria-hidden="true" />
            </button>
          </div>
        </div>
      </div>

      <!-- Mobile menu -->
      <div v-if="isMobileMenuOpen" class="md:hidden" id="mobile-menu">
        <div class="space-y-1 px-2 pb-3 pt-2 border-t border-gray-200">
          <RouterLink
            v-for="item in navigation"
            :key="item.name"
            :to="item.href"
            :class="[
              'block rounded-md px-3 py-2 text-base font-medium',
              isActiveRoute(item.href)
                ? 'bg-primary-100 text-primary-700'
                : 'text-gray-600 hover:bg-gray-50 hover:text-primary-600'
            ]"
            @click="isMobileMenuOpen = false"
          >
            {{ item.name }}
          </RouterLink>
          
          <!-- Mobile user menu -->
          <div v-if="isAuthenticated" class="border-t border-gray-200 pt-4 pb-3">
            <div class="flex items-center px-3">
              <div class="flex-shrink-0">
                <UserCircleIcon class="h-8 w-8 text-gray-400" />
              </div>
              <div class="ml-3">
                <div class="text-base font-medium text-gray-800">{{ userDisplayName }}</div>
              </div>
            </div>
            <div class="mt-3 space-y-1">
              <RouterLink
                v-for="item in userNavigation"
                :key="item.name"
                :to="item.href || '#'"
                :class="[
                  'block rounded-md px-3 py-2 text-base font-medium text-gray-600 hover:bg-gray-50 hover:text-primary-600'
                ]"
                @click="item.href ? (isMobileMenuOpen = false) : handleLogout()"
              >
                {{ item.name }}
              </RouterLink>
            </div>
          </div>
          
          <div v-else class="border-t border-gray-200 pt-4 pb-3">
            <RouterLink to="/login" @click="isMobileMenuOpen = false">
              <BaseButton variant="primary" size="small" class="w-full">
                登入
              </BaseButton>
            </RouterLink>
          </div>
        </div>
      </div>
    </nav>
  </header>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute, RouterLink } from 'vue-router'
import { Menu, MenuButton, MenuItems, MenuItem } from '@headlessui/vue'
import { 
  Bars3Icon, 
  XMarkIcon, 
  UserCircleIcon, 
  ChevronDownIcon 
} from '@heroicons/vue/24/outline'
import { useAuthStore } from '@/stores/auth'
import BaseButton from '@/components/ui/BaseButton.vue'

const route = useRoute()
const authStore = useAuthStore()

const isMobileMenuOpen = ref(false)

const isAuthenticated = computed(() => authStore.isAuthenticated)
const userDisplayName = computed(() => authStore.userDisplayName)

const navigation = [
  { name: '首頁', href: '/' },
]

const userNavigation = [
  { name: '管理後台', href: '/admin/dashboard' },
  { name: '個人資料', href: '/admin/profile' },
  { name: '登出', href: null },
]

function isActiveRoute(href: string) {
  return route.path === href
}

async function handleLogout() {
  await authStore.logout()
  isMobileMenuOpen.value = false
}
</script>