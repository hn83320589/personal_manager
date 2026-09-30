import { http } from './http'
import type { Schemas } from './types'

export const portfoliosApi = {
  list: () => http.get<Schemas['PortfolioSummaryDto'][]>('/me/portfolios'),
  get: (id: number) => http.get<Schemas['PortfolioDto']>(`/me/portfolios/${id}`),
  create: (body: Schemas['SavePortfolioRequest']) =>
    http.post<Schemas['PortfolioDto']>('/me/portfolios', body),
  update: (id: number, body: Schemas['SavePortfolioRequest']) =>
    http.put<Schemas['PortfolioDto']>(`/me/portfolios/${id}`, body),
  remove: (id: number) => http.delete(`/me/portfolios/${id}`),
  reorder: (ids: number[]) => http.put('/me/portfolios/order', { ids }),
}

export const tagsApi = {
  mine: () => http.get<string[]>('/me/tags'),
}
