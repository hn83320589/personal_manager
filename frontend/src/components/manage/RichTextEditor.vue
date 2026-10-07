<template>
  <div
    class="overflow-hidden rounded-lg border border-rule bg-paper focus-within:border-accent focus-within:ring-2 focus-within:ring-accent/20"
  >
    <div
      v-if="editor"
      class="flex flex-wrap gap-0.5 border-b border-rule px-1.5 py-1"
      role="toolbar"
      :aria-label="`${label}工具列`"
    >
      <button
        v-for="tool in tools"
        :key="tool.label"
        type="button"
        :class="[
          'rounded px-2 py-1 text-[0.8rem]',
          tool.active() ? 'bg-ink text-paper' : 'text-muted hover:bg-soft hover:text-ink',
        ]"
        :aria-pressed="tool.active()"
        :title="tool.label"
        @mousedown.prevent
        @click="tool.run()"
      >
        {{ tool.text }}
      </button>
    </div>
    <EditorContent :editor="editor" class="rich-text prose max-w-none px-3 py-2 text-[0.92rem]" />
  </div>
</template>

<script setup lang="ts">
import { onBeforeUnmount } from 'vue'
import { EditorContent, useEditor } from '@tiptap/vue-3'
import StarterKit from '@tiptap/starter-kit'
import Link from '@tiptap/extension-link'
import Placeholder from '@tiptap/extension-placeholder'
import { useToastStore } from '@/stores/toast'
import { editLink, editorHtml, syncEditorContent } from './editorCommands'

const props = withDefaults(
  defineProps<{ modelValue: string; label: string; placeholder?: string }>(),
  {
    placeholder: '',
  },
)
const emit = defineEmits<{ 'update:modelValue': [html: string] }>()
const toasts = useToastStore()

const editor = useEditor({
  content: props.modelValue,
  extensions: [
    StarterKit.configure({ heading: { levels: [3] }, codeBlock: false }),
    Link.configure({ openOnClick: false, autolink: true, HTMLAttributes: { rel: 'noopener' } }),
    Placeholder.configure({ placeholder: props.placeholder }),
  ],
  editorProps: {
    // 讓輔助技術把編輯區視為多行文字欄位
    attributes: {
      role: 'textbox',
      'aria-multiline': 'true',
      'aria-label': props.label,
      class: 'min-h-24 outline-none',
    },
  },
  onUpdate: ({ editor }) => emit('update:modelValue', editorHtml(editor)),
})

syncEditorContent(
  () => editor.value,
  () => props.modelValue,
)

onBeforeUnmount(() => editor.value?.destroy())

function setLink() {
  if (editor.value) editLink(editor.value, toasts.error)
}

const tools = [
  {
    label: '粗體',
    text: 'B',
    run: () => editor.value?.chain().focus().toggleBold().run(),
    active: () => !!editor.value?.isActive('bold'),
  },
  {
    label: '斜體',
    text: 'I',
    run: () => editor.value?.chain().focus().toggleItalic().run(),
    active: () => !!editor.value?.isActive('italic'),
  },
  {
    label: '小標題',
    text: 'H',
    run: () => editor.value?.chain().focus().toggleHeading({ level: 3 }).run(),
    active: () => !!editor.value?.isActive('heading'),
  },
  {
    label: '項目清單',
    text: '•',
    run: () => editor.value?.chain().focus().toggleBulletList().run(),
    active: () => !!editor.value?.isActive('bulletList'),
  },
  {
    label: '編號清單',
    text: '1.',
    run: () => editor.value?.chain().focus().toggleOrderedList().run(),
    active: () => !!editor.value?.isActive('orderedList'),
  },
  {
    label: '引言',
    text: '❝',
    run: () => editor.value?.chain().focus().toggleBlockquote().run(),
    active: () => !!editor.value?.isActive('blockquote'),
  },
  { label: '連結', text: '連結', run: setLink, active: () => !!editor.value?.isActive('link') },
]
</script>

<style scoped>
/* Tiptap 的 placeholder 以 data 屬性標示空段落 */
.rich-text :deep(p.is-editor-empty:first-child::before) {
  content: attr(data-placeholder);
  float: left;
  height: 0;
  pointer-events: none;
  color: rgb(var(--muted) / 0.7);
}
</style>
