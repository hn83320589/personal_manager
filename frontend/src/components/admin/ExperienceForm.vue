<template>
  <form @submit.prevent="handleSubmit" class="space-y-6">
    <!-- Type Selection (if creating new) -->
    <div v-if="!item" class="mb-6">
      <label class="text-base font-medium text-gray-900">類型</label>
      <div class="mt-4 space-y-4">
        <div class="flex items-center">
          <input
            id="education"
            v-model="formData.type"
            type="radio"
            value="education"
            class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
          />
          <label
            for="education"
            class="ml-3 block text-sm font-medium text-gray-700"
          >
            <AcademicCapIcon class="w-5 h-5 inline mr-2" />
            教育背景
          </label>
        </div>
        <div class="flex items-center">
          <input
            id="work"
            v-model="formData.type"
            type="radio"
            value="work"
            class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
          />
          <label
            for="work"
            class="ml-3 block text-sm font-medium text-gray-700"
          >
            <BriefcaseIcon class="w-5 h-5 inline mr-2" />
            工作經歷
          </label>
        </div>
      </div>
    </div>

    <!-- Education Fields -->
    <div v-if="isEducation" class="space-y-4">
      <!-- Institution -->
      <div>
        <label
          for="institution"
          class="block text-sm font-medium text-gray-700"
        >
          學校/機構 <span class="text-red-500">*</span>
        </label>
        <BaseInput
          id="institution"
          v-model="formData.institution"
          type="text"
          required
          placeholder="請輸入學校或教育機構名稱"
          class="mt-1"
        />
      </div>

      <!-- Degree -->
      <div>
        <label for="degree" class="block text-sm font-medium text-gray-700">
          學位 <span class="text-red-500">*</span>
        </label>
        <BaseInput
          id="degree"
          v-model="formData.degree"
          type="text"
          required
          placeholder="如：學士、碩士、博士"
          class="mt-1"
        />
      </div>

      <!-- Field of Study -->
      <div>
        <label
          for="fieldOfStudy"
          class="block text-sm font-medium text-gray-700"
        >
          科系/專業
        </label>
        <BaseInput
          id="fieldOfStudy"
          v-model="formData.fieldOfStudy"
          type="text"
          placeholder="請輸入科系或專業領域"
          class="mt-1"
        />
      </div>

      <!-- GPA -->
      <div>
        <label for="gpa" class="block text-sm font-medium text-gray-700">
          GPA/成績
        </label>
        <BaseInput
          id="gpa"
          v-model="formData.gpa"
          type="text"
          placeholder="如：3.8/4.0 或 85分"
          class="mt-1"
        />
      </div>
    </div>

    <!-- Work Experience Fields -->
    <div v-if="isWork" class="space-y-4">
      <!-- Company -->
      <div>
        <label for="company" class="block text-sm font-medium text-gray-700">
          公司名稱 <span class="text-red-500">*</span>
        </label>
        <BaseInput
          id="company"
          v-model="formData.company"
          type="text"
          required
          placeholder="請輸入公司名稱"
          class="mt-1"
        />
      </div>

      <!-- Position -->
      <div>
        <label for="position" class="block text-sm font-medium text-gray-700">
          職位 <span class="text-red-500">*</span>
        </label>
        <BaseInput
          id="position"
          v-model="formData.position"
          type="text"
          required
          placeholder="請輸入職位名稱"
          class="mt-1"
        />
      </div>

      <!-- Department -->
      <div>
        <label for="department" class="block text-sm font-medium text-gray-700">
          部門
        </label>
        <BaseInput
          id="department"
          v-model="formData.department"
          type="text"
          placeholder="請輸入部門名稱"
          class="mt-1"
        />
      </div>

      <!-- Employment Type -->
      <div>
        <label
          for="employmentType"
          class="block text-sm font-medium text-gray-700"
        >
          工作類型
        </label>
        <select
          id="employmentType"
          v-model="formData.employmentType"
          class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
        >
          <option value="">請選擇工作類型</option>
          <option value="full-time">全職</option>
          <option value="part-time">兼職</option>
          <option value="contract">合約</option>
          <option value="internship">實習</option>
          <option value="freelance">自由工作者</option>
        </select>
      </div>

      <!-- Is Current -->
      <div class="flex items-center">
        <input
          id="isCurrent"
          v-model="formData.isCurrent"
          type="checkbox"
          class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
        />
        <label for="isCurrent" class="ml-2 block text-sm text-gray-900">
          目前仍在職
        </label>
      </div>
    </div>

    <!-- Common Fields -->
    <div class="space-y-4">
      <!-- Location -->
      <div>
        <label for="location" class="block text-sm font-medium text-gray-700">
          地點
        </label>
        <BaseInput
          id="location"
          v-model="formData.location"
          type="text"
          placeholder="如：台北市、遠端工作"
          class="mt-1"
        />
      </div>

      <!-- Date Range -->
      <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <label
            for="startDate"
            class="block text-sm font-medium text-gray-700"
          >
            開始日期 <span class="text-red-500">*</span>
          </label>
          <input
            id="startDate"
            v-model="formData.startDate"
            type="date"
            required
            class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
          />
        </div>
        <div>
          <label for="endDate" class="block text-sm font-medium text-gray-700">
            結束日期
          </label>
          <input
            id="endDate"
            v-model="formData.endDate"
            type="date"
            :disabled="formData.isCurrent"
            class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 disabled:bg-gray-100"
          />
          <p v-if="formData.isCurrent" class="mt-1 text-xs text-gray-500">
            目前仍在進行中
          </p>
        </div>
      </div>

      <!-- Description -->
      <div>
        <label
          for="description"
          class="block text-sm font-medium text-gray-700"
        >
          描述
        </label>
        <BaseTextarea
          id="description"
          v-model="formData.description"
          :rows="4"
          placeholder="請描述主要內容、成就或學習經驗..."
          class="mt-1"
        />
      </div>

      <!-- Skills (for work experience) -->
      <div v-if="isWork">
        <label for="skills" class="block text-sm font-medium text-gray-700">
          相關技能
        </label>
        <BaseInput
          id="skills"
          v-model="formData.skills"
          type="text"
          placeholder="請用逗號分隔，如：JavaScript, Vue.js, Node.js"
          class="mt-1"
        />
        <p class="mt-1 text-xs text-gray-500">請用逗號分隔多個技能</p>
      </div>

      <!-- Website URL -->
      <div>
        <label for="websiteUrl" class="block text-sm font-medium text-gray-700">
          相關網站
        </label>
        <BaseInput
          id="websiteUrl"
          v-model="formData.websiteUrl"
          type="url"
          placeholder="https://"
          class="mt-1"
        />
      </div>

      <!-- Sort Order -->
      <div>
        <label for="sortOrder" class="block text-sm font-medium text-gray-700">
          排序順序
        </label>
        <input
          id="sortOrder"
          v-model.number="formData.sortOrder"
          type="number"
          min="0"
          placeholder="數字越小排序越前面"
          class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
        />
      </div>

      <!-- Is Public -->
      <div class="flex items-center">
        <input
          id="isPublic"
          v-model="formData.isPublic"
          type="checkbox"
          class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
        />
        <label for="isPublic" class="ml-2 block text-sm text-gray-900">
          公開顯示
        </label>
        <p class="ml-2 text-xs text-gray-500">勾選後將在前台頁面顯示</p>
      </div>
    </div>

    <!-- Form Actions -->
    <div class="flex justify-end space-x-3 pt-6 border-t border-gray-200">
      <BaseButton type="button" variant="outline" @click="$emit('cancel')">
        取消
      </BaseButton>
      <BaseButton type="submit" variant="primary" :loading="loading">
        {{ item ? "更新" : "建立" }}
      </BaseButton>
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from "vue";
import { AcademicCapIcon, BriefcaseIcon } from "@heroicons/vue/24/outline";
import type { Education, WorkExperience } from "@/types/experience";
import BaseInput from "@/components/ui/BaseInput.vue";
import BaseTextarea from "@/components/ui/BaseTextarea.vue";
import BaseButton from "@/components/ui/BaseButton.vue";

// Props
interface Props {
  item?: Education | WorkExperience | null;
  type?: "education" | "work";
}

const props = withDefaults(defineProps<Props>(), {
  item: null,
  type: "education",
});

// Emits
const emit = defineEmits<{
  save: [data: any];
  cancel: [];
}>();

// State
const loading = ref(false);
const formData = ref({
  type: props.type,
  // Common fields
  startDate: "",
  endDate: "",
  location: "",
  description: "",
  websiteUrl: "",
  sortOrder: 0,
  isPublic: true,

  // Education fields
  institution: "",
  degree: "",
  fieldOfStudy: "",
  gpa: "",

  // Work fields
  company: "",
  position: "",
  department: "",
  employmentType: "",
  isCurrent: false,
  skills: "",
});

// Computed
const isEducation = computed(() => formData.value.type === "education");
const isWork = computed(() => formData.value.type === "work");

// Watchers
watch(
  () => formData.value.isCurrent,
  (newValue) => {
    if (newValue) {
      formData.value.endDate = "";
    }
  },
);

// Methods
async function handleSubmit() {
  loading.value = true;

  try {
    const submitData = { ...formData.value };

    // Process skills for work experience
    if (
      isWork.value &&
      submitData.skills &&
      typeof submitData.skills === "string"
    ) {
      const skillsArray = submitData.skills
        .split(",")
        .map((s) => s.trim())
        .filter((s) => s);
      submitData.skills = skillsArray.join(",");
    }

    // Remove type field from submit data
    const { type, ...submitDataWithoutType } = submitData;

    emit("save", submitDataWithoutType);
  } catch (error) {
    console.error("Submit error:", error);
  } finally {
    loading.value = false;
  }
}

function initializeForm() {
  if (props.item) {
    const item = props.item;

    // Common fields - check if properties exist
    formData.value.startDate = (item as any).startDate || "";
    formData.value.endDate = (item as any).endDate || "";
    formData.value.location = (item as any).location || "";
    formData.value.description = (item as any).description || "";
    formData.value.websiteUrl = (item as any).websiteUrl || "";
    formData.value.sortOrder = item.sortOrder || 0;
    formData.value.isPublic = item.isPublic ?? true;

    // Check if it's education or work experience
    if ("school" in item) {
      // Education
      formData.value.type = "education";
      formData.value.institution = (item as Education).school;
      formData.value.degree = (item as Education).degree;
      formData.value.fieldOfStudy = (item as Education).fieldOfStudy || "";
      formData.value.gpa = "";
    } else {
      // Work Experience
      formData.value.type = "work";
      formData.value.company = (item as WorkExperience).company;
      formData.value.position = (item as WorkExperience).position;
      formData.value.department = "";
      formData.value.employmentType = "";
      formData.value.isCurrent = (item as WorkExperience).isCurrent || false;
      formData.value.skills = "";
    }
  } else {
    formData.value.type = props.type;
  }
}

// Lifecycle
onMounted(() => {
  initializeForm();
});

// Watch for item changes
watch(
  () => props.item,
  () => {
    initializeForm();
  },
  { immediate: true },
);
</script>
