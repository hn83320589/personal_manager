<template>
  <div class="border border-gray-300 rounded-lg overflow-hidden">
    <!-- Toolbar -->
    <div class="flex flex-wrap items-center gap-1 p-2 bg-gray-50 border-b border-gray-200">
      <!-- Text Formatting -->
      <button
        type="button"
        @click="editor?.chain().focus().toggleBold().run()"
        :class="['toolbar-btn', editor?.isActive('bold') ? 'toolbar-btn-active' : '']"
        title="粗體 (Ctrl+B)"
      ><strong class="text-sm">B</strong></button>

      <button
        type="button"
        @click="editor?.chain().focus().toggleItalic().run()"
        :class="['toolbar-btn', editor?.isActive('italic') ? 'toolbar-btn-active' : '']"
        title="斜體 (Ctrl+I)"
      ><em class="text-sm">I</em></button>

      <button
        type="button"
        @click="editor?.chain().focus().toggleUnderline().run()"
        :class="['toolbar-btn', editor?.isActive('underline') ? 'toolbar-btn-active' : '']"
        title="底線 (Ctrl+U)"
      ><span class="text-sm underline">U</span></button>

      <div class="w-px h-5 bg-gray-300 mx-1"></div>

      <!-- Headings -->
      <button
        type="button"
        @click="editor?.chain().focus().toggleHeading({ level: 2 }).run()"
        :class="['toolbar-btn', editor?.isActive('heading', { level: 2 }) ? 'toolbar-btn-active' : '']"
        title="標題 2"
      ><span class="text-xs font-bold">H2</span></button>

      <button
        type="button"
        @click="editor?.chain().focus().toggleHeading({ level: 3 }).run()"
        :class="['toolbar-btn', editor?.isActive('heading', { level: 3 }) ? 'toolbar-btn-active' : '']"
        title="標題 3"
      ><span class="text-xs font-bold">H3</span></button>

      <div class="w-px h-5 bg-gray-300 mx-1"></div>

      <!-- Lists -->
      <button
        type="button"
        @click="editor?.chain().focus().toggleBulletList().run()"
        :class="['toolbar-btn', editor?.isActive('bulletList') ? 'toolbar-btn-active' : '']"
        title="無序清單"
      >•</button>

      <button
        type="button"
        @click="editor?.chain().focus().toggleOrderedList().run()"
        :class="['toolbar-btn', editor?.isActive('orderedList') ? 'toolbar-btn-active' : '']"
        title="有序清單"
      >1.</button>

      <button
        type="button"
        @click="editor?.chain().focus().toggleBlockquote().run()"
        :class="['toolbar-btn', editor?.isActive('blockquote') ? 'toolbar-btn-active' : '']"
        title="引用區塊"
      >"</button>

      <div class="w-px h-5 bg-gray-300 mx-1"></div>

      <!-- Alignment -->
      <button
        type="button"
        @click="editor?.chain().focus().setTextAlign('left').run()"
        :class="['toolbar-btn', editor?.isActive({ textAlign: 'left' }) ? 'toolbar-btn-active' : '']"
        title="靠左對齊"
      >≡</button>

      <button
        type="button"
        @click="editor?.chain().focus().setTextAlign('center').run()"
        :class="['toolbar-btn', editor?.isActive({ textAlign: 'center' }) ? 'toolbar-btn-active' : '']"
        title="置中對齊"
      >≡</button>

      <button
        type="button"
        @click="editor?.chain().focus().setTextAlign('right').run()"
        :class="['toolbar-btn', editor?.isActive({ textAlign: 'right' }) ? 'toolbar-btn-active' : '']"
        title="靠右對齊"
      >≡</button>

      <div class="w-px h-5 bg-gray-300 mx-1"></div>

      <!-- Link -->
      <button
        type="button"
        @click="insertLink"
        :class="['toolbar-btn', editor?.isActive('link') ? 'toolbar-btn-active' : '']"
        title="插入連結"
      >🔗</button>

      <!-- Image -->
      <button
        type="button"
        @click="showImagePicker = true"
        class="toolbar-btn"
        title="插入圖片"
      >🖼</button>

      <div class="w-px h-5 bg-gray-300 mx-1"></div>

      <!-- Clear Formatting -->
      <button
        type="button"
        @click="editor?.chain().focus().unsetAllMarks().clearNodes().run()"
        class="toolbar-btn text-xs text-gray-600"
        title="清除格式"
      >清除</button>
    </div>

    <!-- Editor Content -->
    <EditorContent :editor="editor" class="tiptap-content" />

    <!-- Image Insert Modal -->
    <div v-if="showImagePicker" class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50" @click.self="showImagePicker = false">
      <div class="bg-white rounded-lg p-6 w-96 shadow-xl">
        <h3 class="text-lg font-semibold mb-4">插入圖片</h3>
        <div class="space-y-3">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">圖片 URL</label>
            <input
              v-model="imageUrl"
              type="url"
              placeholder="https://example.com/image.jpg"
              class="w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div class="text-center text-sm text-gray-400">— 或 —</div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">從檔案庫選取</label>
            <FilePicker
              v-model="showImagePicker"
              fileType="image"
              @select="onImageSelected"
            />
          </div>
        </div>
        <div class="flex justify-end gap-2 mt-4">
          <button type="button" @click="showImagePicker = false" class="px-3 py-1.5 text-sm text-gray-600 border border-gray-300 rounded-md hover:bg-gray-50">取消</button>
          <button type="button" @click="confirmInsertImage" class="px-3 py-1.5 text-sm text-white bg-blue-600 rounded-md hover:bg-blue-700">插入</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onBeforeUnmount } from 'vue'
import { useEditor, EditorContent } from '@tiptap/vue-3'
import StarterKit from '@tiptap/starter-kit'
import Image from '@tiptap/extension-image'
import Link from '@tiptap/extension-link'
import Placeholder from '@tiptap/extension-placeholder'
import TextAlign from '@tiptap/extension-text-align'
import Underline from '@tiptap/extension-underline'
import FilePicker from '@/components/admin/FilePicker.vue'
import type { FileUpload } from '@/types/api'

const props = withDefaults(defineProps<{
  modelValue: string
  placeholder?: string
}>(), {
  placeholder: '開始輸入文章內容...'
})

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void
}>()

const showImagePicker = ref(false)
const imageUrl = ref('')

const editor = useEditor({
  content: props.modelValue,
  extensions: [
    StarterKit,
    Underline,
    Image.configure({ inline: false, allowBase64: false }),
    Link.configure({ openOnClick: false }),
    Placeholder.configure({ placeholder: props.placeholder }),
    TextAlign.configure({ types: ['heading', 'paragraph'] }),
  ],
  onUpdate({ editor }) {
    emit('update:modelValue', editor.getHTML())
  },
})

watch(() => props.modelValue, (val) => {
  if (editor.value && editor.value.getHTML() !== val) {
    editor.value.commands.setContent(val, false)
  }
})

function insertLink() {
  const url = window.prompt('輸入連結 URL:')
  if (url) {
    editor.value?.chain().focus().setLink({ href: url }).run()
  }
}

function onImageSelected(file: FileUpload) {
  const backendBase = import.meta.env.VITE_API_BASE_URL?.replace('/api', '') || 'http://localhost:5037'
  const fullUrl = file.fileUrl.startsWith('http') ? file.fileUrl : `${backendBase}${file.fileUrl}`
  editor.value?.chain().focus().setImage({ src: fullUrl }).run()
  showImagePicker.value = false
  imageUrl.value = ''
}

function confirmInsertImage() {
  if (imageUrl.value) {
    editor.value?.chain().focus().setImage({ src: imageUrl.value }).run()
    showImagePicker.value = false
    imageUrl.value = ''
  }
}

onBeforeUnmount(() => {
  editor.value?.destroy()
})
</script>

<style>
.tiptap-content .ProseMirror {
  min-height: 400px;
  padding: 1rem;
  outline: none;
}

.tiptap-content .ProseMirror p.is-editor-empty:first-child::before {
  content: attr(data-placeholder);
  color: #9ca3af;
  pointer-events: none;
  float: left;
  height: 0;
}

.tiptap-content .ProseMirror h2 {
  font-size: 1.5rem;
  font-weight: 700;
  margin: 1rem 0 0.5rem;
}

.tiptap-content .ProseMirror h3 {
  font-size: 1.25rem;
  font-weight: 600;
  margin: 0.75rem 0 0.5rem;
}

.tiptap-content .ProseMirror ul {
  list-style-type: disc;
  padding-left: 1.5rem;
  margin: 0.5rem 0;
}

.tiptap-content .ProseMirror ol {
  list-style-type: decimal;
  padding-left: 1.5rem;
  margin: 0.5rem 0;
}

.tiptap-content .ProseMirror blockquote {
  border-left: 4px solid #e5e7eb;
  padding-left: 1rem;
  color: #6b7280;
  margin: 0.5rem 0;
}

.tiptap-content .ProseMirror img {
  max-width: 100%;
  height: auto;
  border-radius: 0.375rem;
  margin: 0.5rem 0;
}

.tiptap-content .ProseMirror a {
  color: #2563eb;
  text-decoration: underline;
}
</style>

<style scoped>
.toolbar-btn {
  @apply p-1.5 rounded text-gray-600 hover:text-gray-900 hover:bg-gray-200 transition-colors text-sm min-w-[28px] flex items-center justify-center;
}

.toolbar-btn-active {
  @apply bg-blue-100 text-blue-700;
}
</style>
