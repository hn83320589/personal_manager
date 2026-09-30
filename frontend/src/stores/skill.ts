import { defineStore } from "pinia";
import { ref, computed } from "vue";
import type { Skill, SkillLevel } from "@/types/api";
import skillService from "@/services/skillService";

export const useSkillStore = defineStore("skill", () => {
  // State
  const skills = ref<Skill[]>([]);
  const currentSkill = ref<Skill | null>(null);
  const isLoading = ref(false);
  const error = ref<string | null>(null);

  // Getters
  const publicSkills = computed(() =>
    skills.value
      .filter((skill) => skill.isPublic)
      .sort((a, b) => a.sortOrder - b.sortOrder),
  );

  const skillsByCategory = computed(() => {
    const categories: Record<string, Skill[]> = {};
    publicSkills.value.forEach((skill) => {
      const category = skill.category || "Other";
      if (!categories[category]) {
        categories[category] = [];
      }
      categories[category].push(skill);
    });
    return categories;
  });

  const skillsByLevel = computed(() => {
    const levels: Record<SkillLevel, Skill[]> = {
      Beginner: [],
      Intermediate: [],
      Advanced: [],
      Expert: [],
    };
    publicSkills.value.forEach((skill) => {
      if (levels[skill.level]) {
        levels[skill.level].push(skill);
      }
    });
    return levels;
  });

  const expertSkills = computed(() =>
    publicSkills.value.filter((skill) => skill.level === "Expert"),
  );

  // Actions
  async function fetchSkills() {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await skillService.getSkills();
      skills.value = response.data;
    } catch (err) {
      error.value = "Failed to fetch skills";
      console.error(err);
    } finally {
      isLoading.value = false;
    }
  }

  async function fetchSkillById(id: number) {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await skillService.getSkillById(id);
      currentSkill.value = response.data;
    } catch (err) {
      error.value = "Failed to fetch skill";
      console.error(err);
    } finally {
      isLoading.value = false;
    }
  }

  async function fetchSkillsByCategory(category: string) {
    // Note: Use computed skillsByCategory property instead
    // This method can be removed if not needed
    return skillsByCategory.value[category] || [];
  }

  async function fetchSkillsByLevel(level: SkillLevel) {
    // Note: Use computed skillsByLevel property instead
    // This method can be removed if not needed
    return skillsByLevel.value[level] || [];
  }

  async function createSkill(skillData: Partial<Skill>) {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await skillService.createSkill(skillData);
      skills.value.push(response.data);
      return response.data;
    } catch (err) {
      error.value = "Failed to create skill";
      console.error(err);
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  async function updateSkill(id: number, skillData: Partial<Skill>) {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await skillService.updateSkill(id, skillData);
      const index = skills.value.findIndex((skill) => skill.id === id);
      if (index !== -1) {
        skills.value[index] = response.data;
      }
      return response.data;
    } catch (err) {
      error.value = "Failed to update skill";
      console.error(err);
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  async function deleteSkill(id: number) {
    isLoading.value = true;
    error.value = null;

    try {
      await skillService.deleteSkill(id);
      skills.value = skills.value.filter((skill) => skill.id !== id);
      return true;
    } catch (err) {
      error.value = "Failed to delete skill";
      console.error(err);
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function updateSkillOrder(skillId: number, newOrder: number) {
    isLoading.value = true;
    error.value = null;

    try {
      // Update order locally (API endpoint for order update can be added later)
      const skill = skills.value.find((s) => s.id === skillId);
      if (skill) {
        await updateSkill(skillId, { sortOrder: newOrder });
        return true;
      }
      return false;
    } catch (err) {
      error.value = "Failed to update skill order";
      console.error(err);
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  function clearError() {
    error.value = null;
  }

  function clearCurrentSkill() {
    currentSkill.value = null;
  }

  return {
    // State
    skills,
    currentSkill,
    isLoading,
    error,
    // Getters
    publicSkills,
    skillsByCategory,
    skillsByLevel,
    expertSkills,
    // Actions
    fetchSkills,
    fetchSkillById,
    fetchSkillsByCategory,
    fetchSkillsByLevel,
    createSkill,
    updateSkill,
    deleteSkill,
    updateSkillOrder,
    clearError,
    clearCurrentSkill,
  };
});
