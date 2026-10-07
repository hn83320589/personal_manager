import { watch } from 'vue'
import type { Editor } from '@tiptap/core'

/** 編輯器的 HTML；空白文件回傳空字串，與「沒有內容」一致。 */
export const editorHtml = (editor: Editor) => (editor.isEmpty ? '' : editor.getHTML())

/** 外部換成另一份內容時（例如切換作品）才同步；自己打字觸發的更新不重設，避免游標跳動。 */
export function syncEditorContent(editor: () => Editor | undefined, value: () => string) {
  watch(value, (html) => {
    const current = editor()
    if (current && html !== editorHtml(current)) current.commands.setContent(html, false)
  })
}

/** 與後端清洗規則相同：連結只接受 http、https、mailto。 */
const allowedLink = /^(https?:|mailto:)/i

/** 詢問連結網址：留空移除連結，不允許的網址交給 onInvalid 說明原因。 */
export function editLink(editor: Editor, onInvalid: (message: string) => void) {
  const previous = editor.getAttributes('link').href as string | undefined
  const url = window.prompt('連結網址（留空則移除連結）', previous ?? 'https://')?.trim()
  if (url === undefined) return
  if (!url) editor.chain().focus().unsetLink().run()
  else if (allowedLink.test(url)) editor.chain().focus().setLink({ href: url }).run()
  else onInvalid('連結需為 http、https 或 mailto 開頭')
}
