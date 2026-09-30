import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

// Import views
import HomeView from '../views/HomeView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    // 首頁：使用者目錄
    {
      path: '/',
      name: 'home',
      component: HomeView,
      meta: { title: '首頁' }
    },

    // 登入
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/LoginView.vue'),
      meta: { title: '登入', requiresGuest: true }
    },

    // 忘記密碼 / 重設密碼
    {
      path: '/forgot-password',
      name: 'forgot-password',
      component: () => import('../views/ForgotPasswordView.vue'),
      meta: { title: '忘記密碼', requiresGuest: true }
    },
    {
      path: '/reset-password',
      name: 'reset-password',
      component: () => import('../views/ResetPasswordView.vue'),
      meta: { title: '重設密碼' }
    },

    // /@:username — 個人公開頁面（UserLayout 包覆）
    {
      path: '/@:username',
      component: () => import('../components/layout/UserLayout.vue'),
      children: [
        {
          path: '',
          name: 'user-about',
          component: () => import('../views/user/UserAboutView.vue'),
          meta: { title: '關於我' }
        },
        {
          path: 'experience',
          name: 'user-experience',
          component: () => import('../views/user/UserExperienceView.vue'),
          meta: { title: '學經歷' }
        },
        {
          path: 'skills',
          name: 'user-skills',
          component: () => import('../views/user/UserSkillView.vue'),
          meta: { title: '技能專長' }
        },
        {
          path: 'portfolio',
          name: 'user-portfolio',
          component: () => import('../views/user/UserPortfolioView.vue'),
          meta: { title: '作品集' }
        },
        {
          path: 'portfolio/:id',
          name: 'user-project-detail',
          component: () => import('../views/user/UserProjectDetailView.vue'),
          meta: { title: '作品詳情' }
        },
        {
          path: 'blog',
          name: 'user-blog',
          component: () => import('../views/user/UserBlogListView.vue'),
          meta: { title: '部落格' }
        },
        {
          path: 'blog/:slug',
          name: 'user-blog-detail',
          component: () => import('../views/user/UserBlogDetailView.vue'),
          meta: { title: '文章內容' }
        },
        {
          path: 'calendar',
          name: 'user-calendar',
          component: () => import('../views/user/UserCalendarView.vue'),
          meta: { title: '公開行事曆' }
        },
        {
          path: 'guestbook',
          name: 'user-guestbook',
          component: () => import('../views/user/UserGuestbookView.vue'),
          meta: { title: '留言板' }
        },
        {
          path: 'contact',
          name: 'user-contact',
          component: () => import('../views/user/UserContactView.vue'),
          meta: { title: '聯絡我' }
        },
      ]
    },

    // 管理後台
    {
      path: '/admin',
      redirect: '/admin/dashboard'
    },
    {
      path: '/admin/dashboard',
      name: 'dashboard',
      component: () => import('../views/admin/DashboardView.vue'),
      meta: { title: '管理儀表板', requiresAuth: true }
    },
    {
      path: '/admin/profile',
      name: 'profile-manage',
      component: () => import('../views/admin/ProfileManageView.vue'),
      meta: { title: '個人資料管理', requiresAuth: true }
    },
    {
      path: '/admin/experience',
      name: 'experience-manage',
      component: () => import('../views/admin/ExperienceManageView.vue'),
      meta: { title: '學經歷管理', requiresAuth: true }
    },
    {
      path: '/admin/skills',
      name: 'skill-manage',
      component: () => import('../views/admin/SkillManageView.vue'),
      meta: { title: '專長管理', requiresAuth: true }
    },
    {
      path: '/admin/projects',
      name: 'project-manage',
      component: () => import('../views/admin/ProjectManageView.vue'),
      meta: { title: '作品管理', requiresAuth: true }
    },
    {
      path: '/admin/calendar',
      name: 'calendar-manage',
      component: () => import('../views/admin/CalendarManageView.vue'),
      meta: { title: '行事曆管理', requiresAuth: true }
    },
    {
      path: '/admin/work-tracking',
      name: 'work-tracking',
      component: () => import('../views/admin/WorkTrackingView.vue'),
      meta: { title: '工作追蹤', requiresAuth: true }
    },
    {
      path: '/admin/tasks',
      name: 'task-manage',
      component: () => import('../views/admin/TaskManageView.vue'),
      meta: { title: '待辦事項管理', requiresAuth: true }
    },
    {
      path: '/admin/blog',
      name: 'blog-manage',
      component: () => import('../views/admin/BlogManageView.vue'),
      meta: { title: '文章管理', requiresAuth: true }
    },
    {
      path: '/admin/blog/editor',
      name: 'blog-editor',
      component: () => import('../views/admin/BlogEditorView.vue'),
      meta: { title: '文章編輯器', requiresAuth: true }
    },
    {
      path: '/admin/blog/editor/:id',
      name: 'blog-editor-edit',
      component: () => import('../views/admin/BlogEditorView.vue'),
      meta: { title: '編輯文章', requiresAuth: true }
    },
    {
      path: '/admin/comments',
      name: 'comment-manage',
      component: () => import('../views/admin/CommentManageView.vue'),
      meta: { title: '留言管理', requiresAuth: true }
    },
    {
      path: '/admin/contacts',
      name: 'contact-manage',
      component: () => import('../views/admin/ContactManageView.vue'),
      meta: { title: '聯絡方式管理', requiresAuth: true }
    },
    {
      path: '/admin/files',
      name: 'file-manager',
      component: () => import('../views/admin/FileManagerView.vue'),
      meta: { title: '檔案管理', requiresAuth: true }
    },
    // Catch all 404
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('../views/NotFoundView.vue'),
      meta: { title: '頁面不存在' }
    }
  ],
})

// Navigation guards
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()

  // Initialize auth state
  authStore.initializeAuth()

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
