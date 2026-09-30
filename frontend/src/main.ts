import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import { useAuthStore } from '@/stores/auth'

const app = createApp(App)

app.use(createPinia())
app.use(router)

// 以 refresh cookie 還原登入狀態；需要登入的路由會在守衛中等待這一步完成
const authStore = useAuthStore()
authStore.onSessionExpired(() => {
  router.push({ name: 'login', query: { redirect: router.currentRoute.value.fullPath } })
})
void authStore.restoreSession()

app.mount('#app')
