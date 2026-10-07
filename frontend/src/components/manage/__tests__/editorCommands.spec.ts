import { describe, it, expect, vi, afterEach } from 'vitest'
import { Editor } from '@tiptap/core'
import StarterKit from '@tiptap/starter-kit'
import Link from '@tiptap/extension-link'
import { editLink, editorHtml } from '../editorCommands'

const makeEditor = (content = '<p>聯絡我</p>') => {
  const editor = new Editor({ extensions: [StarterKit, Link], content })
  editor.commands.selectAll()
  return editor
}

describe('editLink', () => {
  afterEach(() => vi.restoreAllMocks())

  it('links the selection to an http, https or mailto address', () => {
    vi.spyOn(window, 'prompt').mockReturnValue('mailto:me@example.com')
    const editor = makeEditor()
    editLink(editor, vi.fn())
    expect(editor.getHTML()).toContain('href="mailto:me@example.com"')
  })

  it('refuses other schemes and says why', () => {
    vi.spyOn(window, 'prompt').mockReturnValue('javascript:alert(1)')
    const onInvalid = vi.fn()
    const editor = makeEditor()
    editLink(editor, onInvalid)
    expect([editor.getHTML().includes('href'), onInvalid.mock.calls.length]).toEqual([false, 1])
  })

  it('removes the link when left empty', () => {
    vi.spyOn(window, 'prompt').mockReturnValue('')
    const editor = makeEditor('<p><a href="https://example.com">聯絡我</a></p>')
    editLink(editor, vi.fn())
    expect(editor.getHTML()).not.toContain('href')
  })
})

describe('editorHtml', () => {
  it('is an empty string for an empty document', () => {
    expect(editorHtml(makeEditor(''))).toBe('')
  })
})
