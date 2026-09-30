import { http } from './http'
import type { Schemas } from './types'

/** /api/me 底下「自己的清單」共用的操作：列表、新增、更新、刪除、排序。 */
export interface OwnedCollectionApi<Item, Save> {
  list(): Promise<Item[]>
  create(body: Save): Promise<Item>
  update(id: number, body: Save): Promise<Item>
  remove(id: number): Promise<void>
  reorder(ids: number[]): Promise<void>
}

function ownedCollection<Item, Save>(base: string): OwnedCollectionApi<Item, Save> {
  return {
    list: () => http.get<Item[]>(base),
    create: (body) => http.post<Item>(base, body),
    update: (id, body) => http.put<Item>(`${base}/${id}`, body),
    remove: (id) => http.delete(`${base}/${id}`),
    reorder: (ids) => http.put(`${base}/order`, { ids }),
  }
}

export const skillsApi = ownedCollection<Schemas['SkillDto'], Schemas['SaveSkillRequest']>(
  '/me/skills',
)

export const educationsApi = ownedCollection<
  Schemas['EducationDto'],
  Schemas['SaveEducationRequest']
>('/me/educations')

export const workExperiencesApi = ownedCollection<
  Schemas['WorkExperienceDto'],
  Schemas['SaveWorkExperienceRequest']
>('/me/work-experiences')

export const contactMethodsApi = ownedCollection<
  Schemas['ContactMethodDto'],
  Schemas['SaveContactMethodRequest']
>('/me/contact-methods')
