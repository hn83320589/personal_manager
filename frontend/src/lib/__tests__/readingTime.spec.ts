import { describe, it, expect } from 'vitest'
import { countWords, readingMinutes } from '../readingTime'

describe('readingTime', () => {
  it('counts each Chinese character and each English word', () => {
    expect(countWords('我用 Vue 3 寫前端')).toBe(7)
  })

  it('reads 400 Chinese characters per minute', () => {
    expect(readingMinutes('字'.repeat(401))).toBe(2)
  })

  it('reads 200 English words per minute', () => {
    expect(readingMinutes('word '.repeat(200))).toBe(1)
  })

  it('never estimates less than one minute', () => {
    expect(readingMinutes('')).toBe(1)
  })
})
