<template>
  <AdminLayout>
    <!-- Page Header -->
    <div class="mb-8">
      <div class="flex justify-between items-center">
        <div>
          <h2 class="text-2xl font-bold text-gray-900">聯絡方式管理</h2>
          <p class="mt-1 text-sm text-gray-600">管理顯示在個人頁面的聯絡資訊與社群連結</p>
        </div>
        <BaseButton variant="primary" @click="openCreate">
          <PlusIcon class="w-4 h-4 mr-2" />
          新增聯絡方式
        </BaseButton>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="isLoading" class="flex justify-center py-12">
      <LoadingSpinner />
    </div>

    <!-- Empty -->
    <div v-else-if="contacts.length === 0" class="text-center py-12">
      <PhoneIcon class="mx-auto h-12 w-12 text-gray-400" />
      <h3 class="mt-2 text-sm font-medium text-gray-900">尚無聯絡方式</h3>
      <p class="mt-1 text-sm text-gray-500">新增 Email、電話或社群帳號</p>
      <BaseButton variant="primary" class="mt-4" @click="openCreate">
        <PlusIcon class="w-4 h-4 mr-2" />
        新增聯絡方式
      </BaseButton>
    </div>

    <!-- List -->
    <div v-else class="space-y-3">
      <div
        v-for="contact in sortedContacts"
        :key="contact.id"
        class="bg-white border border-gray-200 rounded-lg p-4 flex items-center justify-between shadow-sm hover:shadow transition-shadow"
      >
        <div class="flex items-center space-x-4">
          <div
            class="w-10 h-10 rounded-full flex items-center justify-center text-white text-sm font-bold flex-shrink-0"
            :style="{ backgroundColor: typeColor(contact.type) }"
          >
            {{ (contact.label || contact.type).charAt(0).toUpperCase() }}
          </div>
          <div>
            <div class="flex items-center space-x-2">
              <span class="font-medium text-gray-900">{{ contact.label || contact.type }}</span>
              <span class="text-xs px-2 py-0.5 rounded-full bg-gray-100 text-gray-600">{{ contact.type }}</span>
              <span
                v-if="!contact.isPublic"
                class="text-xs px-2 py-0.5 rounded-full bg-yellow-100 text-yellow-700"
              >
                私人
              </span>
            </div>
            <p class="text-sm text-gray-500 mt-0.5">{{ contact.value }}</p>
          </div>
        </div>
        <div class="flex items-center space-x-2">
          <BaseButton variant="outline" size="small" @click="openEdit(contact)">
            <PencilIcon class="w-4 h-4" />
          </BaseButton>
          <BaseButton variant="outline" size="small" class="text-red-600 hover:text-red-700" @click="confirmDelete(contact)">
            <TrashIcon class="w-4 h-4" />
          </BaseButton>
        </div>
      </div>
    </div>

    <!-- Create / Edit Modal -->
    <BaseModal
      :show="showModal"
      @close="closeModal"
      :title="editingContact ? '編輯聯絡方式' : '新增聯絡方式'"
      max-width="lg"
    >
      <div class="space-y-4">
        <div>
          <label class="form-label">類型 <span class="text-red-500">*</span></label>
          <select v-model="form.type" class="form-select mt-1">
            <option v-for="t in contactTypes" :key="t" :value="t">{{ t }}</option>
          </select>
        </div>
        <div>
          <label class="form-label">顯示名稱</label>
          <input v-model="form.label" type="text" class="form-input mt-1" placeholder="例：個人 Email、LinkedIn 主頁" />
        </div>
        <div>
          <label class="form-label">值 <span class="text-red-500">*</span></label>
          <input v-model="form.value" type="text" class="form-input mt-1" :placeholder="valuePlaceholder" />
          <p class="mt-1 text-xs text-gray-500">Email 請填完整地址，電話請含國碼，社群請填完整 URL</p>
        </div>
        <div>
          <label class="form-label">排序</label>
          <input v-model.number="form.sortOrder" type="number" class="form-input mt-1" min="0" />
        </div>
        <div class="flex items-center">
          <input id="isPublic" v-model="form.isPublic" type="checkbox" class="h-4 w-4 text-blue-600 border-gray-300 rounded" />
          <label for="isPublic" class="ml-2 text-sm text-gray-700">公開顯示</label>
        </div>
      </div>
      <div class="mt-6 flex justify-end space-x-3">
        <BaseButton variant="outline" @click="closeModal">取消</BaseButton>
        <BaseButton variant="primary" :disabled="!isFormValid" @click="submit">
          {{ editingContact ? '儲存' : '新增' }}
        </BaseButton>
      </div>
    </BaseModal>

    <!-- Delete Confirm Modal -->
    <BaseModal :show="showDeleteModal" @close="showDeleteModal = false" title="確認刪除" max-width="sm">
      <p class="text-sm text-gray-600">
        確定要刪除「{{ deletingContact?.label || deletingContact?.type }}」嗎？此操作無法復原。
      </p>
      <div class="mt-6 flex justify-end space-x-3">
        <BaseButton variant="outline" @click="showDeleteModal = false">取消</BaseButton>
        <BaseButton variant="primary" class="bg-red-600 hover:bg-red-700" @click="deleteContact">刪除</BaseButton>
      </div>
    </BaseModal>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { PlusIcon, PencilIcon, TrashIcon, PhoneIcon } from '@heroicons/vue/24/outline'
import { useAuthStore } from '@/stores/auth'
import contactMethodService from '@/services/contactMethodService'
import type { ContactMethod, ContactType } from '@/types/api'
import AdminLayout from '@/components/layout/AdminLayout.vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseCard from '@/components/ui/BaseCard.vue'
import BaseModal from '@/components/ui/BaseModal.vue'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'

const authStore = useAuthStore()

const contacts = ref<ContactMethod[]>([])
const isLoading = ref(false)
const showModal = ref(false)
const showDeleteModal = ref(false)
const editingContact = ref<ContactMethod | null>(null)
const deletingContact = ref<ContactMethod | null>(null)

const contactTypes: ContactType[] = ['Email', 'Phone', 'LinkedIn', 'GitHub', 'Facebook', 'Twitter', 'Instagram', 'Discord', 'Other']

const form = ref({
  type: 'Email' as ContactType,
  label: '',
  value: '',
  icon: '',
  isPublic: true,
  sortOrder: 0,
})

const typeColorMap: Record<string, string> = {
  Email: '#ea4335', Phone: '#34a853', LinkedIn: '#0077b5', GitHub: '#333',
  Facebook: '#1877f2', Twitter: '#1da1f2', Instagram: '#e1306c',
  Discord: '#5865f2', Other: '#6b7280',
}

const sortedContacts = computed(() =>
  [...contacts.value].sort((a, b) => a.sortOrder - b.sortOrder)
)

const isFormValid = computed(() => form.value.value.trim().length > 0)

const valuePlaceholder = computed(() => {
  const map: Partial<Record<ContactType, string>> = {
    Email: 'example@email.com',
    Phone: '+886-9xx-xxx-xxx',
    LinkedIn: 'https://linkedin.com/in/yourname',
    GitHub: 'https://github.com/yourname',
    Facebook: 'https://facebook.com/yourname',
    Twitter: 'https://twitter.com/yourname',
    Instagram: 'https://instagram.com/yourname',
    Discord: 'username#0000',
  }
  return map[form.value.type] || '輸入值'
})

function typeColor(type: string): string {
  return typeColorMap[type] || '#6b7280'
}

async function fetchContacts() {
  const uid = authStore.user?.id
  if (!uid) return
  isLoading.value = true
  try {
    const res = await contactMethodService.getByUserId(uid)
    if (res.success) contacts.value = res.data || []
  } finally {
    isLoading.value = false
  }
}

function openCreate() {
  editingContact.value = null
  form.value = { type: 'Email', label: '', value: '', icon: '', isPublic: true, sortOrder: contacts.value.length }
  showModal.value = true
}

function openEdit(contact: ContactMethod) {
  editingContact.value = contact
  form.value = {
    type: contact.type,
    label: contact.label,
    value: contact.value,
    icon: contact.icon,
    isPublic: contact.isPublic,
    sortOrder: contact.sortOrder,
  }
  showModal.value = true
}

function closeModal() {
  showModal.value = false
  editingContact.value = null
}

async function submit() {
  const uid = authStore.user?.id
  if (!uid) return
  if (editingContact.value) {
    const res = await contactMethodService.update(editingContact.value.id, form.value)
    if (res.success) {
      const idx = contacts.value.findIndex(c => c.id === editingContact.value!.id)
      if (idx !== -1) contacts.value[idx] = res.data
    }
  } else {
    const res = await contactMethodService.create({ ...form.value, userId: uid })
    if (res.success) contacts.value.push(res.data)
  }
  closeModal()
}

function confirmDelete(contact: ContactMethod) {
  deletingContact.value = contact
  showDeleteModal.value = true
}

async function deleteContact() {
  if (!deletingContact.value) return
  const res = await contactMethodService.delete(deletingContact.value.id)
  if (res.success) {
    contacts.value = contacts.value.filter(c => c.id !== deletingContact.value!.id)
  }
  showDeleteModal.value = false
  deletingContact.value = null
}

onMounted(fetchContacts)
</script>
