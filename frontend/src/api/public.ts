import { http } from './http'
import type { Schemas } from './types'

/** 公開頁面的 API：一律以 username 指定是誰的頁面（/api/public/users/{username}/…）。 */
const user = (username: string) => `/public/users/${encodeURIComponent(username)}`

export interface PageQuery {
  page?: number
  pageSize?: number
}

export const publicApi = {
  directory: (query: { q?: string } & PageQuery = {}) =>
    http.get<Schemas['DirectoryCardDtoPagedResult']>('/public/users', query),

  profile: (username: string) => http.get<Schemas['ProfileDto']>(user(username)),

  educations: (username: string) =>
    http.get<Schemas['PublicEducationDto'][]>(`${user(username)}/educations`),

  workExperiences: (username: string) =>
    http.get<Schemas['PublicWorkExperienceDto'][]>(`${user(username)}/work-experiences`),

  skills: (username: string) => http.get<Schemas['PublicSkillDto'][]>(`${user(username)}/skills`),

  contactMethods: (username: string) =>
    http.get<Schemas['PublicContactMethodDto'][]>(`${user(username)}/contact-methods`),

  portfolios: (username: string, filter: { category?: string; tag?: string } = {}) =>
    http.get<Schemas['PortfolioCardDto'][]>(`${user(username)}/portfolios`, filter),

  portfolioFacets: (username: string) =>
    http.get<Schemas['PortfolioFacetsDto']>(`${user(username)}/portfolios/facets`),

  portfolio: (username: string, slug: string) =>
    http.get<Schemas['PublicPortfolioDto']>(
      `${user(username)}/portfolios/${encodeURIComponent(slug)}`,
    ),

  posts: (
    username: string,
    query: { q?: string; tag?: string; category?: string } & PageQuery = {},
  ) => http.get<Schemas['PublicPostSummaryDtoPagedResult']>(`${user(username)}/posts`, query),

  postFacets: (username: string) =>
    http.get<Schemas['PostFacetsDto']>(`${user(username)}/posts/facets`),

  post: (username: string, slug: string) =>
    http.get<Schemas['PublicPostDto']>(`${user(username)}/posts/${encodeURIComponent(slug)}`),

  recordPostView: (username: string, slug: string) =>
    http.post(`${user(username)}/posts/${encodeURIComponent(slug)}/views`),

  guestbook: (username: string, query: PageQuery = {}) =>
    http.get<Schemas['PublicGuestbookEntryDtoPagedResult']>(`${user(username)}/guestbook`, query),

  leaveMessage: (username: string, body: Schemas['LeaveMessageRequest']) =>
    http.post(`${user(username)}/guestbook`, body),

  calendar: (username: string, range: { from: string; to: string }) =>
    http.get<Schemas['OccurrenceDto'][]>(`${user(username)}/calendar`, range),
}
