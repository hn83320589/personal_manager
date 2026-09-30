import { Node } from '@tiptap/core'
import { VueNodeViewRenderer } from '@tiptap/vue-3'
import EmbedView from './EmbedView.vue'

declare module '@tiptap/core' {
  interface Commands<ReturnType> {
    embed: {
      insertEmbed: (src: string) => ReturnType
    }
  }
}

/**
 * 嵌入內容（影片、Figma 等）：只保存網址 <div data-embed="…">（ADR-012），
 * 前台依白名單轉成播放器，後端也只保留白名單網址。
 */
export const Embed = Node.create({
  name: 'embed',
  group: 'block',
  atom: true,
  draggable: true,

  addAttributes() {
    return { src: { default: null } }
  },

  parseHTML() {
    return [
      {
        tag: 'div[data-embed]',
        getAttrs: (el) => ({ src: (el as HTMLElement).getAttribute('data-embed') }),
      },
    ]
  },

  renderHTML({ HTMLAttributes }) {
    return ['div', { 'data-embed': HTMLAttributes.src }]
  },

  addNodeView() {
    return VueNodeViewRenderer(EmbedView)
  },

  addCommands() {
    return {
      insertEmbed:
        (src) =>
        ({ commands }) =>
          commands.insertContent({ type: this.name, attrs: { src } }),
    }
  },
})
