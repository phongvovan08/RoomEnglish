export interface ApiListResponse<T> {
  items: T[]
  total: number
}

export interface PaginationParams {
  page: number
  pageSize: number
}

export interface SortParams {
  sortField: string
  sortOrder: number
}

export interface FilterParams {
  [key: string]: any
}

export type RequestTypeParams = {
  requestType?: number
}

export type QueryParams = {
  filters?: FilterParams
  sort?: SortParams
}

export type RequestParams = PaginationParams & RequestTypeParams & QueryParams
