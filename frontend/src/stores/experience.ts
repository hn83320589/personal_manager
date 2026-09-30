import { defineStore } from "pinia";
import { ref, computed } from "vue";
import type { Education, WorkExperience } from "@/types/api";
import experienceService from "@/services/experienceService";

export const useExperienceStore = defineStore("experience", () => {
  // State
  const educations = ref<Education[]>([]);
  const workExperiences = ref<WorkExperience[]>([]);
  const currentEducation = ref<Education | null>(null);
  const currentWorkExperience = ref<WorkExperience | null>(null);
  const isLoading = ref(false);
  const error = ref<string | null>(null);

  // Getters
  const publicEducations = computed(() =>
    educations.value
      .filter((edu) => edu.isPublic)
      .sort((a, b) => a.sortOrder - b.sortOrder),
  );

  const publicWorkExperiences = computed(() =>
    workExperiences.value
      .filter((exp) => exp.isPublic)
      .sort((a, b) => a.sortOrder - b.sortOrder),
  );

  const currentPosition = computed(() =>
    workExperiences.value.find((exp) => exp.isCurrent),
  );

  // Education Actions
  async function fetchEducations() {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await experienceService.getEducations();
      educations.value = response.data;
    } catch (err) {
      error.value = "Failed to fetch educations";
      console.error(err);
    } finally {
      isLoading.value = false;
    }
  }

  async function fetchEducationById(id: number) {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await experienceService.getEducationById(id);
      currentEducation.value = response.data;
    } catch (err) {
      error.value = "Failed to fetch education";
      console.error(err);
    } finally {
      isLoading.value = false;
    }
  }

  async function createEducation(educationData: Partial<Education>) {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await experienceService.createEducation(educationData);
      educations.value.push(response.data);
      return response.data;
    } catch (err) {
      error.value = "Failed to create education";
      console.error(err);
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  async function updateEducation(
    id: number,
    educationData: Partial<Education>,
  ) {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await experienceService.updateEducation(
        id,
        educationData,
      );
      const index = educations.value.findIndex((edu) => edu.id === id);
      if (index !== -1) {
        educations.value[index] = response.data;
      }
      return response.data;
    } catch (err) {
      error.value = "Failed to update education";
      console.error(err);
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  async function deleteEducation(id: number) {
    isLoading.value = true;
    error.value = null;

    try {
      await experienceService.deleteEducation(id);
      educations.value = educations.value.filter((edu) => edu.id !== id);
      return true;
    } catch (err) {
      error.value = "Failed to delete education";
      console.error(err);
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  // Work Experience Actions
  async function fetchWorkExperiences() {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await experienceService.getWorkExperiences();
      workExperiences.value = response.data;
    } catch (err) {
      error.value = "Failed to fetch work experiences";
      console.error(err);
    } finally {
      isLoading.value = false;
    }
  }

  async function fetchWorkExperienceById(id: number) {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await experienceService.getWorkExperienceById(id);
      currentWorkExperience.value = response.data;
    } catch (err) {
      error.value = "Failed to fetch work experience";
      console.error(err);
    } finally {
      isLoading.value = false;
    }
  }

  async function createWorkExperience(experienceData: Partial<WorkExperience>) {
    isLoading.value = true;
    error.value = null;

    try {
      const response =
        await experienceService.createWorkExperience(experienceData);
      workExperiences.value.push(response.data);
      return response.data;
    } catch (err) {
      error.value = "Failed to create work experience";
      console.error(err);
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  async function updateWorkExperience(
    id: number,
    experienceData: Partial<WorkExperience>,
  ) {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await experienceService.updateWorkExperience(
        id,
        experienceData,
      );
      const index = workExperiences.value.findIndex((exp) => exp.id === id);
      if (index !== -1) {
        workExperiences.value[index] = response.data;
      }
      return response.data;
    } catch (err) {
      error.value = "Failed to update work experience";
      console.error(err);
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  async function deleteWorkExperience(id: number) {
    isLoading.value = true;
    error.value = null;

    try {
      await experienceService.deleteWorkExperience(id);
      workExperiences.value = workExperiences.value.filter(
        (exp) => exp.id !== id,
      );
      return true;
    } catch (err) {
      error.value = "Failed to delete work experience";
      console.error(err);
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  function clearError() {
    error.value = null;
  }

  function clearCurrentEducation() {
    currentEducation.value = null;
  }

  function clearCurrentWorkExperience() {
    currentWorkExperience.value = null;
  }

  return {
    // State
    educations,
    workExperiences,
    currentEducation,
    currentWorkExperience,
    isLoading,
    error,
    // Getters
    publicEducations,
    publicWorkExperiences,
    currentPosition,
    // Actions
    fetchEducations,
    fetchEducationById,
    createEducation,
    updateEducation,
    deleteEducation,
    fetchWorkExperiences,
    fetchWorkExperienceById,
    createWorkExperience,
    updateWorkExperience,
    deleteWorkExperience,
    clearError,
    clearCurrentEducation,
    clearCurrentWorkExperience,
  };
});
