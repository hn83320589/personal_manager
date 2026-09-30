import type { Editor, Range } from '@tiptap/core'

export interface SlashItem {
  label: string
  hint: string
  /** 搜尋用的關鍵字（中英文都可以） */
  keywords: string[]
  run: (editor: Editor, range: Range) => void
}

/** 需要與使用者互動的動作（上傳圖片、輸入網址）由編輯器元件提供。 */
export interface SlashActions {
  insertImage: (editor: Editor) => void
  insertEmbed: (editor: Editor) => void
  insertCode: (editor: Editor) => void
}

export function slashItems(actions: SlashActions): SlashItem[] {
  const chain = (editor: Editor, range: Range) => editor.chain().focus().deleteRange(range)
  return [
    {
      label: '大標題',
      hint: 'H2',
      keywords: ['h2', 'heading', '標題'],
      run: (e, r) => chain(e, r).setHeading({ level: 2 }).run(),
    },
    {
      label: '小標題',
      hint: 'H3',
      keywords: ['h3', 'heading', '標題'],
      run: (e, r) => chain(e, r).setHeading({ level: 3 }).run(),
    },
    {
      label: '圖片',
      hint: '上傳',
      keywords: ['image', 'img', '圖'],
      run: (e, r) => (chain(e, r).run(), actions.insertImage(e)),
    },
    {
      label: '嵌入',
      hint: '影片等',
      keywords: ['embed', 'video', 'youtube', '影片'],
      run: (e, r) => (chain(e, r).run(), actions.insertEmbed(e)),
    },
    {
      label: '程式碼',
      hint: '{ }',
      keywords: ['code', '程式'],
      run: (e, r) => (chain(e, r).run(), actions.insertCode(e)),
    },
    {
      label: '引言',
      hint: '❝',
      keywords: ['quote', '引用'],
      run: (e, r) => chain(e, r).setBlockquote().run(),
    },
    {
      label: '項目清單',
      hint: '•',
      keywords: ['list', 'ul', '清單'],
      run: (e, r) => chain(e, r).toggleBulletList().run(),
    },
    {
      label: '編號清單',
      hint: '1.',
      keywords: ['ol', 'number', '清單'],
      run: (e, r) => chain(e, r).toggleOrderedList().run(),
    },
    {
      label: '分隔線',
      hint: '―',
      keywords: ['hr', 'divider', '分隔'],
      run: (e, r) => chain(e, r).setHorizontalRule().run(),
    },
  ]
}

export function filterSlashItems(items: SlashItem[], query: string): SlashItem[] {
  const q = query.trim().toLowerCase()
  if (!q) return items
  return items.filter((item) => item.label.includes(q) || item.keywords.some((k) => k.includes(q)))
}
