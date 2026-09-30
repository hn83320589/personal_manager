import type { components } from './schema'

/** 由 backend/openapi.json 產生的 DTO 型別，例如 `Schemas['PortfolioDto']`。 */
export type Schemas = components['schemas']
