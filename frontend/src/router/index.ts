import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { setPageSeo } from '@/composables/useSeo'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  // 返回上一頁時回到原本位置；#about、#contact 等錨點捲動到對應區塊（扣掉固定導覽列）
  scrollBehavior(to, _from, savedPosition) {
    if (savedPosition) return savedPosition
    if (to.hash) return { el: to.hash, top: 80, behavior: 'smooth' }
    return { top: 0 }
  },
  routes: [
    // 首頁：使用者目錄
    {
      path: '/',
      name: 'home',
      component: () => import('../views/public/DirectoryView.vue'),
      meta: { title: '探索個人頁面' },
    },

    // 登入
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/LoginView.vue'),
      meta: { title: '登入', requiresGuest: true },
    },

    {
      path: '/register',
      name: 'register',
      component: () => import('../views/RegisterView.vue'),
      meta: { title: '建立帳號', requiresGuest: true },
    },

    // 忘記密碼 / 重設密碼
    {
      path: '/forgot-password',
      name: 'forgot-password',
      component: () => import('../views/ForgotPasswordView.vue'),
      meta: { title: '忘記密碼', requiresGuest: true },
    },
    {
      path: '/reset-password',
      name: 'reset-password',
      component: () => import('../views/ResetPasswordView.vue'),
      meta: { title: '重設密碼' },
    },

    // /@:username — 個人公開頁面
    {
      path: '/@:username',
      component: () => import('../views/public/PublicLayout.vue'),
      children: [
        {
          path: '',
          name: 'public-home',
          component: () => import('../views/public/PublicHomeView.vue'),
        },
        {
          path: 'works',
          name: 'public-works',
          component: () => import('../views/public/PublicWorksView.vue'),
        },
        {
          path: 'works/:slug',
          name: 'public-work',
          component: () => import('../views/public/PublicWorkView.vue'),
        },
        {
          path: 'blog',
          name: 'public-blog',
          component: () => import('../views/public/PublicBlogView.vue'),
        },
        {
          path: 'blog/:slug',
          name: 'public-post',
          component: () => import('../views/public/PublicPostView.vue'),
        },
        {
          path: 'guestbook',
          name: 'public-guestbook',
          component: () => import('../views/public/PublicGuestbookView.vue'),
        },
        {
          path: 'calendar',
          name: 'public-calendar',
          component: () => import('../views/public/PublicCalendarView.vue'),
        },
        // 舊網址：經歷、技能、聯絡已併入首頁；作品集改為 works（舊的以 id 為網址，無法對應到 slug）
        { path: 'portfolio/:id?', redirect: (to) => ({ name: 'public-works', params: to.params }) },
        {
          path: 'experience',
          redirect: (to) => ({ name: 'public-home', params: to.params, hash: '#about' }),
        },
        {
          path: 'skills',
          redirect: (to) => ({ name: 'public-home', params: to.params, hash: '#about' }),
        },
        { path: 'about', redirect: (to) => ({ name: 'public-home', params: to.params }) },
        {
          path: 'contact',
          redirect: (to) => ({ name: 'public-home', params: to.params, hash: '#contact' }),
        },
      ],
    },

    // 管理後台：所有頁面都是 AdminShell 的子路由，AdminShell 提供側欄
    {
      path: '/admin',
      component: () => import('../views/manage/AdminShell.vue'),
      meta: { requiresAuth: true },
      children: [
        { path: '', redirect: '/admin/dashboard' },
        {
          path: 'dashboard',
          name: 'dashboard',
          component: () => import('../views/manage/DashboardView.vue'),
          meta: { title: '儀表板' },
        },
        {
          path: 'account',
          name: 'manage-account',
          component: () => import('../views/manage/AccountView.vue'),
          meta: { title: '修改密碼' },
        },
        {
          path: 'users',
          name: 'manage-users',
          component: () => import('../views/manage/UsersView.vue'),
          meta: { title: '使用者管理', requiresAdmin: true },
        },
        {
          path: 'profile',
          name: 'manage-profile',
          component: () => import('../views/manage/ProfileView.vue'),
          meta: { title: '個人資料' },
        },
        {
          path: 'works',
          name: 'manage-works',
          component: () => import('../views/manage/WorksView.vue'),
          meta: { title: '作品' },
        },
        {
          path: 'works/:id(\\d+)',
          name: 'manage-work',
          component: () => import('../views/manage/WorkEditorView.vue'),
          meta: { title: '編輯作品' },
        },
        { path: 'projects', redirect: '/admin/works' },
        {
          path: 'blog',
          name: 'manage-posts',
          component: () => import('../views/manage/PostsView.vue'),
          meta: { title: '文章' },
        },
        {
          path: 'blog/:id(\\d+)',
          name: 'manage-post',
          component: () => import('../views/manage/PostEditorView.vue'),
          meta: { title: '編輯文章' },
        },
        { path: 'blog/editor/:id(\\d+)', redirect: (to) => `/admin/blog/${to.params.id}` },
        { path: 'blog/editor', redirect: '/admin/blog' },
        {
          path: 'comments',
          name: 'manage-guestbook',
          component: () => import('../views/manage/GuestbookView.vue'),
          meta: { title: '留言' },
        },
        {
          path: 'files',
          name: 'manage-files',
          component: () => import('../views/manage/FilesView.vue'),
          meta: { title: '檔案' },
        },
        {
          path: 'tasks',
          name: 'manage-todos',
          component: () => import('../views/manage/TodosView.vue'),
          meta: { title: '待辦' },
        },
        {
          path: 'calendar',
          name: 'manage-calendar',
          component: () => import('../views/manage/CalendarView.vue'),
          meta: { title: '行事曆' },
        },
        {
          path: 'work-tracking',
          name: 'manage-work-tracking',
          component: () => import('../views/manage/WorkTrackingView.vue'),
          meta: { title: '工作追蹤' },
        },
        {
          path: 'experience',
          name: 'manage-resume',
          component: () => import('../views/manage/ResumeView.vue'),
          meta: { title: '經歷' },
        },
        {
          path: 'skills',
          name: 'manage-skills',
          component: () => import('../views/manage/SkillsView.vue'),
          meta: { title: '技能' },
        },
        {
          path: 'contacts',
          name: 'manage-contacts',
          component: () => import('../views/manage/ContactsView.vue'),
          meta: { title: '聯絡方式' },
        },
      ],
    },
    // Catch all 404
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('../views/NotFoundView.vue'),
      meta: { title: '找不到頁面' },
    },
  ],
})

// Navigation guards
router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore()

  // 需要判斷登入狀態的路由，先等待 refresh cookie 還原完成，重新整理頁面時才不會被誤導到登入頁
  if (to.meta.requiresAuth || to.meta.requiresGuest) {
    await authStore.restoreSession()
  }

  // Check auth requirements
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next({ name: 'login', query: { redirect: to.fullPath } })
    return
  }

  // 管理員頁面：一般使用者導回儀表板（後端同樣會拒絕）
  if (to.meta.requiresAdmin && !authStore.isAdmin) {
    next({ name: 'dashboard' })
    return
  }

  if (to.meta.requiresGuest && authStore.isAuthenticated) {
    next({ name: 'dashboard' })
    return
  }

  next()
})

// 固定標題的頁面由路由設定；個人頁面、文章、作品等依內容在頁面內呼叫 setPageSeo（掛載後執行，會覆蓋這裡）
router.afterEach((to) => {
  if (typeof to.meta.title === 'string') setPageSeo({ title: to.meta.title })
})

export default router
