<template>
  <aside :class="sidebarClasses">
    <div class="flex flex-col h-full">
      <!-- Sidebar Header -->
      <div
        class="flex items-center justify-between p-4 border-b border-gray-200"
      >
        <h2 class="text-lg font-semibold text-gray-800">{{ title }}</h2>
        <button
          v-if="collapsible"
          @click="toggleSidebar"
          class="p-1 rounded-lg hover:bg-gray-100 transition-colors"
        >
          <ChevronLeftIcon v-if="!collapsed" class="h-5 w-5 text-gray-500" />
          <ChevronRightIcon v-else class="h-5 w-5 text-gray-500" />
        </button>
      </div>

      <!-- Navigation Menu -->
      <nav class="flex-1 p-4 space-y-2 overflow-y-auto">
        <div v-for="item in menuItems" :key="item.name">
          <!-- Group Header -->
          <div v-if="item.type === 'group'" class="mb-2">
            <h3
              v-if="!collapsed"
              class="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-2"
            >
              {{ item.name }}
            </h3>
            <div class="space-y-1">
              <SidebarItem
                v-for="subItem in item.children"
                :key="subItem.name"
                :item="subItem"
                :collapsed="collapsed"
                @click="handleItemClick"
              />
            </div>
          </div>

          <!-- Regular Item -->
          <SidebarItem
            v-else
            :item="item"
            :collapsed="collapsed"
            @click="handleItemClick"
          />
        </div>
      </nav>

      <!-- Sidebar Footer -->
      <div v-if="$slots.footer" class="p-4 border-t border-gray-200">
        <slot name="footer" :collapsed="collapsed" />
      </div>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { ChevronLeftIcon, ChevronRightIcon } from "@heroicons/vue/24/outline";
import SidebarItem from "./SidebarItem.vue";

export interface MenuItem {
  name: string;
  type?: "item" | "group";
  icon?: any;
  route?: string;
  badge?: string | number;
  active?: boolean;
  children?: MenuItem[];
  onClick?: () => void;
}

interface Props {
  title?: string;
  menuItems: MenuItem[];
  collapsible?: boolean;
  defaultCollapsed?: boolean;
  width?: string;
  position?: "left" | "right";
}

const props = withDefaults(defineProps<Props>(), {
  title: "選單",
  collapsible: true,
  defaultCollapsed: false,
  width: "256px", // w-64
  position: "left",
});

const emit = defineEmits<{
  itemClick: [item: MenuItem];
  toggleCollapse: [collapsed: boolean];
}>();

const collapsed = ref(props.defaultCollapsed);

const sidebarClasses = computed(() => {
  const baseClasses = "bg-white border-gray-200 transition-all duration-300";
  const positionClasses = props.position === "left" ? "border-r" : "border-l";
  const widthClasses = collapsed.value ? "w-16" : `w-64`;

  return [baseClasses, positionClasses, widthClasses].join(" ");
});

function toggleSidebar() {
  collapsed.value = !collapsed.value;
  emit("toggleCollapse", collapsed.value);
}

function handleItemClick(item: MenuItem) {
  if (item.onClick) {
    item.onClick();
  }
  emit("itemClick", item);
}
</script>
