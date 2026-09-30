import { Node } from '@tiptap/core'

declare module '@tiptap/core' {
  interface Commands<ReturnType> {
    figure: {
      insertFigure: (attrs: { src: string; alt?: string }) => ReturnType
    }
  }
}

/**
 * 附說明的圖片：<figure><img><figcaption>說明</figcaption></figure>。
 * 說明文字可以直接在編輯器中輸入；後端清洗允許這三個標籤。
 */
export const Figure = Node.create({
  name: 'figure',
  group: 'block',
  content: 'inline*',
  draggable: true,
  isolating: true,

  addAttributes() {
    return {
      src: { default: null },
      alt: { default: '' },
    }
  },

  parseHTML() {
    return [
      {
        tag: 'figure',
        contentElement: 'figcaption',
        getAttrs: (element) => {
          const img = (element as HTMLElement).querySelector('img')
          return img ? { src: img.getAttribute('src'), alt: img.getAttribute('alt') ?? '' } : false
        },
      },
    ]
  },

  renderHTML({ HTMLAttributes }) {
    return [
      'figure',
      ['img', { src: HTMLAttributes.src, alt: HTMLAttributes.alt }],
      ['figcaption', 0],
    ]
  },

  addCommands() {
    return {
      insertFigure:
        (attrs) =>
        ({ commands }) =>
          commands.insertContent({ type: this.name, attrs }),
    }
  },
})
