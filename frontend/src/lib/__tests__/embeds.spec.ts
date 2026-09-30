import { describe, it, expect } from 'vitest'
import { toEmbed } from '../embeds'

describe('toEmbed', () => {
  it.each([
    [
      'https://www.youtube.com/watch?v=dQw4w9WgXcQ',
      'https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ',
    ],
    ['https://youtu.be/dQw4w9WgXcQ', 'https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ'],
    [
      'https://www.youtube.com/shorts/dQw4w9WgXcQ',
      'https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ',
    ],
    ['https://vimeo.com/123456', 'https://player.vimeo.com/video/123456'],
    [
      'https://codepen.io/dada/pen/abcXYZ',
      'https://codepen.io/dada/embed/abcXYZ?default-tab=result',
    ],
    [
      'https://docs.google.com/presentation/d/1AbC/edit#slide=id.p',
      'https://docs.google.com/presentation/d/1AbC/embed',
    ],
    [
      'https://sketchfab.com/3d-models/tea-cup-0123456789abcdef0123456789abcdef',
      'https://sketchfab.com/models/0123456789abcdef0123456789abcdef/embed',
    ],
  ])('%s → %s', (url, src) => {
    expect(toEmbed(url)?.src).toBe(src)
  })

  it('wraps Figma links in the Figma embed page', () => {
    const embed = toEmbed('https://www.figma.com/design/abc/Poster')

    expect(embed?.src).toBe(
      'https://www.figma.com/embed?embed_host=personal-manager&url=https%3A%2F%2Fwww.figma.com%2Fdesign%2Fabc%2FPoster',
    )
  })

  it('uses the SoundCloud player as an audio embed', () => {
    const embed = toEmbed('https://soundcloud.com/artist/track')

    expect([embed?.kind, embed?.src]).toEqual([
      'audio',
      'https://w.soundcloud.com/player/?url=https%3A%2F%2Fsoundcloud.com%2Fartist%2Ftrack',
    ])
  })

  it('shows GitHub Gist as a link, because gists can only be embedded with a script', () => {
    expect(toEmbed('https://gist.github.com/dada/abc123')).toEqual({
      provider: 'GitHub Gist',
      kind: 'link',
      src: 'https://gist.github.com/dada/abc123',
    })
  })

  it.each([
    'http://www.youtube.com/watch?v=dQw4w9WgXcQ',
    'https://evil.example/player',
    'https://youtube.com.evil.example/watch?v=x',
    'javascript:alert(1)',
    'not a url',
  ])('rejects %s', (url) => {
    expect(toEmbed(url)).toBeNull()
  })

  it('rejects a whitelisted site when the video id cannot be found', () => {
    expect(toEmbed('https://www.youtube.com/feed/trending')).toBeNull()
  })
})
