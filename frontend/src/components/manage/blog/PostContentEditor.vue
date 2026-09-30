<template>
  <div class="flex min-w-0 flex-col gap-2.5">
    <div
      v-if="editor"
      class="sticky top-[calc(env(safe-area-inset-top,0px)+8px)] z-10 flex flex-wrap gap-0.5 rounded-card border border-rule bg-surface p-1.5"
      role="toolbar"
      aria-label="格式"
    >
      <template v-for="(group, g) in toolGroups" :key="g">
        <span v-if="g > 0" class="mx-1 my-1 w-px bg-rule" aria-hidden="true" />
        <button
          v-for="tool in group"
          :key="tool.label"
          type="button"
          :title="tool.label"
          :aria-label="tool.label"
          :aria-pressed="tool.active?.() ?? undefined"
          :disabled="tool.disabled?.()"
          :class="[
            'h-8 min-w-8 rounded-md px-2 text-[0.86rem]',
            tool.active?.() ? 'bg-ink text-paper' : 'text-ink hover:bg-soft',
            'disabled:opacity-35 disabled:hover:bg-transparent',
          ]"
          @mousedown.prevent
          @click="tool.run()"
        >
          {{ tool.text }}
        </button>
      </template>
      <input
        ref="imagePicker"
        type="file"
        class="sr-only"
        :accept="imageAccept"
        multiple
        @change="onPickImages"
      />
    </div>

    <EditorContent
      :editor="editor"
      class="post-editor prose max-w-none rounded-xl border border-rule bg-surface px-[clamp(16px,4vw,40px)] py-6 focus-within:border-accent focus-within:ring-2 focus-within:ring-accent/20"
    />

    <div class="flex flex-wrap gap-x-4 gap-y-1 text-[0.8rem] text-muted">
      <span>{{ words }} 字</span>
      <span>約 {{ minutes }} 分鐘</span>
      <span v-if="uploading">圖片上傳中…</span>
      <span>可直接貼上或拖曳圖片；在空白行輸入 / 開啟插入選單。</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue'
import type { Editor } from '@tiptap/core'
import { EditorContent, useEditor } from '@tiptap/vue-3'
import StarterKit from '@tiptap/starter-kit'
import Link from '@tiptap/extension-link'
import Underline from '@tiptap/extension-underline'
import Image from '@tiptap/extension-image'
import Placeholder from '@tiptap/extension-placeholder'
import CharacterCount from '@tiptap/extension-character-count'
import CodeBlockLowlight from '@tiptap/extension-code-block-lowlight'
import { filesApi } from '@/api/files'
import { errorMessage } from '@/composables/useAsyncAction'
import { toEmbed } from '@/lib/embeds'
import { imageAccept } from '@/lib/fileTypes'
import { lowlight } from '@/lib/highlight'
import { countWords, readingMinutes } from '@/lib/readingTime'
import { useToastStore } from '@/stores/toast'
import { Embed } from './extensions/Embed'
import { Figure } from './extensions/Figure'
import { SlashCommand } from './extensions/SlashCommand'

const props = defineProps<{ modelValue: string }>()
const emit = defineEmits<{ 'update:modelValue': [html: string] }>()

const toasts = useToastStore()
const imagePicker = ref<HTMLInputElement>()
const uploading = ref(false)
const text = ref('')

const editor = useEditor({
  content: props.modelValue,
  extensions: [
    StarterKit.configure({ heading: { levels: [2, 3] }, codeBlock: false }),
    Underline,
    Link.configure({
      openOnClick: false,
      autolink: true,
      HTMLAttributes: { rel: 'noopener', target: '_blank' },
    }),
    Image, // 舊文章中沒有說明的圖片
    Figure,
    Embed,
    CodeBlockLowlight.configure({ lowlight }),
    CharacterCount,
    Placeholder.configure({ placeholder: '開始寫作…輸入 / 可插入圖片、影片、程式碼等' }),
    SlashCommand.configure({
      actions: { insertImage: pickImages, insertEmbed: askEmbed, insertCode: insertCode },
    }),
  ],
  editorProps: {
    attributes: { 'aria-label': '文章內容', class: 'min-h-96 outline-none' },
    handlePaste: (_view, event) => uploadFrom(event.clipboardData?.files),
    handleDrop: (_view, event) => uploadFrom((event as DragEvent).dataTransfer?.files),
  },
  onCreate: ({ editor }) => (text.value = editor.getText()),
  onUpdate: ({ editor }) => {
    text.value = editor.getText()
    emit('update:modelValue', editor.isEmpty ? '' : editor.getHTML())
  },
})

// 載入另一篇文章時同步內容；自己打字觸發的更新不重設，避免游標跳動
watch(
  () => props.modelValue,
  (value) => {
    const current = editor.value
    if (current && value !== (current.isEmpty ? '' : current.getHTML()))
      current.commands.setContent(value, false)
  },
)
onBeforeUnmount(() => editor.value?.destroy())

const words = computed(() => countWords(text.value))
const minutes = computed(() => readingMinutes(text.value))

function pickImages() {
  imagePicker.value?.click()
}

function onPickImages(event: Event) {
  const input = event.target as HTMLInputElement
  uploadFrom(input.files)
  input.value = ''
}

/** 上傳圖片並以附說明的圖片插入游標位置；不是圖片的檔案交給編輯器預設處理。 */
function uploadFrom(files: FileList | null | undefined): boolean {
  const images = [...(files ?? [])].filter((f) => f.type.startsWith('image/'))
  if (!images.length) return false
  void (async () => {
    uploading.value = true
    for (const file of images) {
      try {
        const uploaded = await filesApi.upload(file)
        editor.value?.chain().focus().insertFigure({ src: uploaded.url, alt: '' }).run()
      } catch (e) {
        toasts.error(`「${file.name}」上傳失敗：${errorMessage(e)}`)
      }
    }
    uploading.value = false
  })()
  return true
}

function askEmbed(target: Editor | null = editor.value ?? null) {
  const url = window.prompt('貼上要嵌入的網址（YouTube、Vimeo、Figma、CodePen 等）')?.trim()
  if (!url || !target) return
  if (!toEmbed(url)) {
    toasts.error('不支援嵌入這個網站的內容')
    return
  }
  target.chain().focus().insertEmbed(url).run()
}

function insertCode(target: Editor | null = editor.value ?? null) {
  if (!target) return
  const language = window.prompt('程式語言（例如 ts、csharp、sql，可留空）')?.trim() ?? ''
  target
    .chain()
    .focus()
    .setCodeBlock(language ? { language } : undefined)
    .run()
}

function setLink() {
  const current = editor.value
  if (!current) return
  const previous = current.getAttributes('link').href as string | undefined
  const url = window.prompt('連結網址（留空則移除連結）', previous ?? 'https://')
  if (url === null) return
  if (!url.trim()) current.chain().focus().unsetLink().run()
  else if (/^(https?:|mailto:)/i.test(url.trim()))
    current.chain().focus().setLink({ href: url.trim() }).run()
  else toasts.error('連結需為 http、https 或 mailto 開頭')
}

interface Tool {
  label: string
  text: string
  run: () => void
  active?: () => boolean
  disabled?: () => boolean
}

const cmd = () => editor.value!.chain().focus()
const is = (name: string, attrs?: Record<string, unknown>) => () =>
  !!editor.value?.isActive(name, attrs)

const toolGroups: Tool[][] = [
  [
    {
      label: '大標題',
      text: 'H2',
      run: () => cmd().toggleHeading({ level: 2 }).run(),
      active: is('heading', { level: 2 }),
    },
    {
      label: '小標題',
      text: 'H3',
      run: () => cmd().toggleHeading({ level: 3 }).run(),
      active: is('heading', { level: 3 }),
    },
    { label: '內文', text: '¶', run: () => cmd().setParagraph().run() },
  ],
  [
    { label: '粗體', text: 'B', run: () => cmd().toggleBold().run(), active: is('bold') },
    { label: '斜體', text: 'I', run: () => cmd().toggleItalic().run(), active: is('italic') },
    { label: '底線', text: 'U', run: () => cmd().toggleUnderline().run(), active: is('underline') },
    { label: '刪除線', text: 'S', run: () => cmd().toggleStrike().run(), active: is('strike') },
    { label: '連結', text: '連結', run: setLink, active: is('link') },
  ],
  [
    {
      label: '項目清單',
      text: '• 清單',
      run: () => cmd().toggleBulletList().run(),
      active: is('bulletList'),
    },
    {
      label: '編號清單',
      text: '1. 清單',
      run: () => cmd().toggleOrderedList().run(),
      active: is('orderedList'),
    },
    {
      label: '引言',
      text: '❝',
      run: () => cmd().toggleBlockquote().run(),
      active: is('blockquote'),
    },
  ],
  [
    { label: '插入圖片', text: '圖片', run: pickImages },
    { label: '嵌入影片或其他內容', text: '嵌入', run: () => askEmbed() },
    { label: '程式碼區塊', text: '{ }', run: () => insertCode(), active: is('codeBlock') },
    { label: '分隔線', text: '―', run: () => cmd().setHorizontalRule().run() },
  ],
  [
    {
      label: '復原',
      text: '↶',
      run: () => cmd().undo().run(),
      disabled: () => !editor.value?.can().undo(),
    },
    {
      label: '重做',
      text: '↷',
      run: () => cmd().redo().run(),
      disabled: () => !editor.value?.can().redo(),
    },
  ],
]
</script>

<style scoped>
.post-editor :deep(p.is-editor-empty:first-child::before) {
  content: attr(data-placeholder);
  float: left;
  height: 0;
  pointer-events: none;
  color: rgb(var(--muted));
}
.post-editor :deep(figure figcaption) {
  border-bottom: 1px dashed rgb(var(--rule));
  text-align: center;
}
.post-editor :deep(figure figcaption:empty::before) {
  content: '為圖片加上說明（選填）';
  color: rgb(var(--muted));
}
</style>
