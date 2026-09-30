<template>
  <div
    class="min-h-screen bg-gray-50 flex flex-col justify-center py-12 sm:px-6 lg:px-8"
  >
    <div class="sm:mx-auto sm:w-full sm:max-w-md">
      <!-- Logo/Header -->
      <div class="text-center">
        <h1 class="text-3xl font-bold text-gray-900">Personal Manager</h1>
        <h2 class="mt-6 text-2xl font-semibold text-gray-900">登入您的帳號</h2>
        <p class="mt-2 text-sm text-gray-600">
          存取您的個人儀表板並管理您的內容
        </p>
      </div>
    </div>

    <div class="mt-8 sm:mx-auto sm:w-full sm:max-w-md">
      <div class="bg-white py-8 px-4 shadow sm:rounded-lg sm:px-10">
        <!-- Error Message -->
        <div
          v-if="error"
          class="mb-6 bg-red-50 border border-red-200 rounded-lg p-4"
        >
          <div class="flex">
            <ExclamationTriangleIcon class="h-5 w-5 text-red-400" />
            <div class="ml-3">
              <h3 class="text-sm font-medium text-red-800">登入失敗</h3>
              <p class="mt-1 text-sm text-red-700">{{ error }}</p>
            </div>
          </div>
        </div>

        <!-- Login Form -->
        <BaseForm @submit="handleLogin">
          <BaseInput
            v-model="form.username"
            type="text"
            label="帳號或電子信箱"
            placeholder="請輸入帳號或電子信箱"
            required
            :disabled="isLoading"
          />

          <BaseInput
            v-model="form.password"
            type="password"
            label="密碼"
            placeholder="請輸入密碼"
            required
            :disabled="isLoading"
          />

          <!-- Remember Me -->
          <div class="flex items-center justify-between">
            <div class="flex items-center">
              <input
                id="remember-me"
                v-model="form.rememberMe"
                type="checkbox"
                class="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
                :disabled="isLoading"
              />
              <label for="remember-me" class="ml-2 block text-sm text-gray-900">
                記住我
              </label>
            </div>

            <div class="text-sm">
              <router-link
                to="/forgot-password"
                class="font-medium text-primary-600 hover:text-primary-500"
              >
                忘記密碼？
              </router-link>
            </div>
          </div>

          <!-- Submit Button -->
          <BaseButton
            type="submit"
            class="w-full"
            :loading="isLoading"
            :disabled="!isFormValid || isLoading"
          >
            <span v-if="!isLoading">登入</span>
            <span v-else>登入中...</span>
          </BaseButton>
        </BaseForm>

        <!-- Divider -->
        <div class="mt-6">
          <div class="relative">
            <div class="absolute inset-0 flex items-center">
              <div class="w-full border-t border-gray-300" />
            </div>
            <div class="relative flex justify-center text-sm">
              <span class="px-2 bg-white text-gray-500">或以以下方式登入</span>
            </div>
          </div>

          <!-- Social Login Buttons -->
          <div class="mt-6 grid grid-cols-2 gap-3">
            <BaseButton
              variant="outline"
              size="small"
              class="w-full"
              @click="handleSocialLogin('google')"
              :disabled="isLoading"
            >
              <svg class="w-4 h-4 mr-2" viewBox="0 0 24 24">
                <path
                  fill="#4285F4"
                  d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
                />
                <path
                  fill="#34A853"
                  d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
                />
                <path
                  fill="#FBBC05"
                  d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
                />
                <path
                  fill="#EA4335"
                  d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
                />
              </svg>
              Google
            </BaseButton>

            <BaseButton
              variant="outline"
              size="small"
              class="w-full"
              @click="handleSocialLogin('github')"
              :disabled="isLoading"
            >
              <svg class="w-4 h-4 mr-2" fill="currentColor" viewBox="0 0 24 24">
                <path
                  d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z"
                />
              </svg>
              GitHub
            </BaseButton>
          </div>
        </div>

        <!-- Demo Account Info -->
        <div class="mt-6 bg-blue-50 border border-blue-200 rounded-lg p-4">
          <div class="text-center">
            <h3 class="text-sm font-medium text-blue-800">展示帳號</h3>
            <p class="mt-1 text-sm text-blue-700">
              Username:
              <code class="bg-blue-100 px-1 rounded">admin</code> Password:
              <code class="bg-blue-100 px-1 rounded">demo123</code>
            </p>
            <BaseButton
              variant="outline"
              size="small"
              class="mt-3"
              @click="fillDemoCredentials"
              :disabled="isLoading"
            >
              使用展示帳號
            </BaseButton>
          </div>
        </div>
      </div>

      <!-- Footer Links -->
      <div class="mt-6 text-center text-sm text-gray-600">
        <p>
          還沒有帳號？
          <router-link
            to="/register"
            class="font-medium text-primary-600 hover:text-primary-500"
          >
            註冊
          </router-link>
        </p>
        <p class="mt-2">
          <router-link
            to="/"
            class="font-medium text-primary-600 hover:text-primary-500"
          >
            ← 返回首頁
          </router-link>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive, onMounted } from "vue";
import { useRouter, useRoute } from "vue-router";
import { ExclamationTriangleIcon } from "@heroicons/vue/24/outline";
import { useAuthStore } from "@/stores/auth";
import BaseButton from "@/components/ui/BaseButton.vue";
import BaseInput from "@/components/ui/BaseInput.vue";
import BaseForm from "@/components/ui/BaseForm.vue";

// Router
const router = useRouter();
const route = useRoute();

// Stores
const authStore = useAuthStore();

// State
const form = reactive({
  username: "",
  password: "",
  rememberMe: false,
});

const isLoading = ref(false);

// Computed
const error = computed(() => authStore.error);
const isFormValid = computed(() => {
  return form.username.trim() && form.password.trim();
});

// Methods
async function handleLogin() {
  if (!isFormValid.value || isLoading.value) return;

  isLoading.value = true;
  authStore.clearError();

  try {
    const success = await authStore.login({
      username: form.username.trim(),
      password: form.password.trim(),
    });

    if (success) {
      // Redirect to intended page or dashboard
      const redirectTo = (route.query.redirect as string) || "/admin/dashboard";
      router.push(redirectTo);
    }
  } catch (error) {
    console.error("Login error:", error);
  } finally {
    isLoading.value = false;
  }
}

async function handleSocialLogin(provider: "google" | "github") {
  try {
    // In a real app, this would redirect to OAuth provider
    console.log(`Social login with ${provider}`);

    // For demo purposes, we'll simulate a successful login
    if (provider === "google") {
      // Simulate Google OAuth success
      alert("Google OAuth would be implemented here");
    } else {
      // Simulate GitHub OAuth success
      alert("GitHub OAuth would be implemented here");
    }
  } catch (error) {
    console.error(`${provider} login error:`, error);
  }
}

function fillDemoCredentials() {
  form.username = "admin";
  form.password = "demo123";
}

// Lifecycle
onMounted(() => {
  // Clear any existing errors when component mounts
  authStore.clearError();

  // Check if already authenticated
  if (authStore.isAuthenticated) {
    const redirectTo = (route.query.redirect as string) || "/admin/dashboard";
    router.push(redirectTo);
  }
});
</script>

<style scoped>
/* Custom styles for the login page */
code {
  font-family:
    ui-monospace, SFMono-Regular, "SF Mono", Menlo, Monaco, "Cascadia Code",
    "Roboto Mono", Consolas, "Courier New", monospace;
}
</style>
