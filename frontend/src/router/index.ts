import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

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

    // 管理後台（新版）：AdminShell 提供側欄；尚未重寫的頁面仍是下方各自獨立的舊路由
    {
      path: '/admin',
      component: () => import('../views/manage/AdminShell.vue'),
      meta: { requiresAuth: true },
      children: [
        { path: '', redirect: '/admin/dashboard' },
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
    {
      path: '/admin/dashboard',
      name: 'dashboard',
      component: () => import('../views/admin/DashboardView.vue'),
      meta: { title: '管理儀表板', requiresAuth: true },
    },
    {
      path: '/admin/calendar',
      name: 'calendar-manage',
      component: () => import('../views/admin/CalendarManageView.vue'),
      meta: { title: '行事曆管理', requiresAuth: true },
    },
    {
      path: '/admin/work-tracking',
      name: 'work-tracking',
      component: () => import('../views/admin/WorkTrackingView.vue'),
      meta: { title: '工作追蹤', requiresAuth: true },
    },
    {
      path: '/admin/tasks',
      name: 'task-manage',
      component: () => import('../views/admin/TaskManageView.vue'),
      meta: { title: '待辦事項管理', requiresAuth: true },
    },
    {
      path: '/admin/comments',
      name: 'comment-manage',
      component: () => import('../views/admin/CommentManageView.vue'),
      meta: { title: '留言管理', requiresAuth: true },
    },
    {
      path: '/admin/files',
      name: 'file-manager',
      component: () => import('../views/admin/FileManagerView.vue'),
      meta: { title: '檔案管理', requiresAuth: true },
    },
    // Catch all 404
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('../views/NotFoundView.vue'),
      meta: { title: '頁面不存在' },
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

  // Set page title
  if (to.meta.title) {
    document.title = `${to.meta.title} - ${import.meta.env.VITE_APP_TITLE || 'Personal Manager'}`
  }

  // Check auth requirements
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next({ name: 'login', query: { redirect: to.fullPath } })
    return
  }

  if (to.meta.requiresGuest && authStore.isAuthenticated) {
    next({ name: 'dashboard' })
    return
  }

  next()
})

export default router
