import { Extension } from '@tiptap/core'
import Suggestion, { type SuggestionProps } from '@tiptap/suggestion'
import { VueRenderer } from '@tiptap/vue-3'
import SlashMenu from './SlashMenu.vue'
import { filterSlashItems, slashItems, type SlashActions, type SlashItem } from './slashItems'

/** 在空白行輸入「/」開啟插入選單（標題、圖片、嵌入、程式碼…）。 */
export const SlashCommand = Extension.create<{ actions: SlashActions | null }>({
  name: 'slashCommand',

  addOptions() {
    return { actions: null }
  },

  addProseMirrorPlugins() {
    const actions = this.options.actions
    if (!actions) return []
    const all = slashItems(actions)

    return [
      Suggestion<SlashItem, SlashItem>({
        editor: this.editor,
        char: '/',
        startOfLine: true,
        items: ({ query }) => filterSlashItems(all, query),
        command: ({ editor, range, props }) => props.run(editor, range),
        render: () => {
          let renderer: VueRenderer | null = null

          const place = (props: SuggestionProps<SlashItem, SlashItem>) => {
            const rect = props.clientRect?.()
            const el = renderer?.element as HTMLElement | undefined
            if (!rect || !el) return
            el.style.position = 'fixed'
            el.style.left = `${rect.left}px`
            el.style.top = `${rect.bottom + 6}px`
            el.style.zIndex = '60'
          }

          return {
            onStart: (props) => {
              renderer = new VueRenderer(SlashMenu, { props, editor: props.editor })
              document.body.appendChild(renderer.element as HTMLElement)
              place(props)
            },
            onUpdate: (props) => {
              renderer?.updateProps(props)
              place(props)
            },
            onKeyDown: ({ event }) => {
              if (event.key === 'Escape') {
                renderer?.destroy()
                ;(renderer?.element as HTMLElement | undefined)?.remove()
                renderer = null
                return true
              }
              return (
                (
                  renderer?.ref as { onKeyDown?: (e: KeyboardEvent) => boolean } | null
                )?.onKeyDown?.(event) ?? false
              )
            },
            onExit: () => {
              ;(renderer?.element as HTMLElement | undefined)?.remove()
              renderer?.destroy()
              renderer = null
            },
          }
        },
      }),
    ]
  },
})
