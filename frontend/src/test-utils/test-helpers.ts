import { mount, type VueWrapper } from "@vue/test-utils";
import { createPinia } from "pinia";
import { createRouter, createWebHistory } from "vue-router";
import type { ComponentPublicInstance } from "vue";

// 測試用路由
export const createTestRouter = () => {
  return createRouter({
    history: createWebHistory(),
    routes: [
      { path: "/", name: "Home", component: { template: "<div>Home</div>" } },
      {
        path: "/about",
        name: "About",
        component: { template: "<div>About</div>" },
      },
      {
        path: "/portfolio",
        name: "Portfolio",
        component: { template: "<div>Portfolio</div>" },
      },
      {
        path: "/login",
        name: "Login",
        component: { template: "<div>Login</div>" },
      },
      {
        path: "/admin/dashboard",
        name: "Dashboard",
        component: { template: "<div>Dashboard</div>" },
      },
    ],
  });
};

// 包裝器工廠函數，提供預設的測試環境
export const createWrapper = <T extends ComponentPublicInstance>(
  component: any,
  options: any = {},
) => {
  const pinia = createPinia();
  const router = createTestRouter();

  return mount(component, {
    global: {
      plugins: [pinia, router],
      stubs: {
        RouterLink: true,
        RouterView: true,
      },
    },
    ...options,
  }) as VueWrapper<T>;
};

// Mock API 回應工廠
export const createMockApiResponse = <T>(
  data: T,
  success = true,
  message = "Success",
) => ({
  success,
  message,
  data,
  errors: success ? null : ["Test error"],
  timestamp: new Date().toISOString(),
});

// Mock 使用者資料
export const createMockUser = (overrides = {}) => ({
  id: 1,
  username: "testuser",
  email: "test@example.com",
  role: "user",
  createdAt: "2024-01-01T00:00:00Z",
  updatedAt: "2024-01-01T00:00:00Z",
  ...overrides,
});

// Mock 作品集資料
export const createMockPortfolio = (overrides = {}) => ({
  id: 1,
  userId: 1,
  title: "Test Project",
  description: "This is a test project",
  technologies: "Vue, TypeScript",
  imageUrl: "https://example.com/image.jpg",
  projectUrl: "https://example.com",
  repositoryUrl: "https://github.com/test/project",
  isFeatured: false,
  isPublic: true,
  sortOrder: 0,
  createdAt: "2024-01-01T00:00:00Z",
  ...overrides,
});

// Mock 技能資料
export const createMockSkill = (overrides = {}) => ({
  id: 1,
  userId: 1,
  name: "JavaScript",
  category: "programming",
  level: "Advanced",
  yearsOfExperience: 5,
  isPublic: true,
  sortOrder: 1,
  ...overrides,
});

// Mock 部落格文章資料
export const createMockBlogPost = (overrides = {}) => ({
  id: 1,
  title: "Test Blog Post",
  content: "This is test content",
  excerpt: "This is a test excerpt",
  slug: "test-blog-post",
  category: "tech",
  tags: ["vue", "testing"],
  featured: false,
  published: true,
  publishedAt: "2024-01-01T00:00:00Z",
  readingTime: 5,
  viewCount: 100,
  createdAt: "2024-01-01T00:00:00Z",
  updatedAt: "2024-01-01T00:00:00Z",
  ...overrides,
});

// 等待 DOM 更新的輔助函數
export const flushPromises = () =>
  new Promise((resolve) => setTimeout(resolve, 0));

// 模擬延遲的輔助函數
export const delay = (ms: number) =>
  new Promise((resolve) => setTimeout(resolve, ms));

// 獲取元件內的文字內容
export const getTextContent = (
  wrapper: VueWrapper<any>,
  selector: string,
): string => {
  const element = wrapper.find(selector);
  return element.exists() ? element.text() : "";
};

// 檢查元件是否包含特定 class
export const hasClass = (
  wrapper: VueWrapper<any>,
  selector: string,
  className: string,
): boolean => {
  const element = wrapper.find(selector);
  return element.exists() && element.classes().includes(className);
};

// 觸發表單提交的輔助函數
export const submitForm = async (
  wrapper: VueWrapper<any>,
  formSelector = "form",
) => {
  const form = wrapper.find(formSelector);
  if (form.exists()) {
    await form.trigger("submit");
    await flushPromises();
  }
};

// 填寫表單欄位的輔助函數
export const fillFormField = async (
  wrapper: VueWrapper<any>,
  selector: string,
  value: string,
) => {
  const field = wrapper.find(selector);
  if (field.exists()) {
    await field.setValue(value);
    await flushPromises();
  }
};

// 點擊按鈕的輔助函數
export const clickButton = async (
  wrapper: VueWrapper<any>,
  selector: string,
) => {
  const button = wrapper.find(selector);
  if (button.exists()) {
    await button.trigger("click");
    await flushPromises();
  }
};

// 檢查載入狀態的輔助函數
export const waitForLoading = async (
  wrapper: VueWrapper<any>,
  loadingSelector = '[data-testid="loading"]',
  timeout = 5000,
) => {
  const startTime = Date.now();

  while (Date.now() - startTime < timeout) {
    await flushPromises();
    const loadingElement = wrapper.find(loadingSelector);
    if (!loadingElement.exists()) {
      break;
    }
    await delay(100);
  }
};
