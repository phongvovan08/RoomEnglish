// API Response Types matching backend DTOs

export interface PaginatedList<T> {
  items: T[]
  pageNumber: number
  totalPages: number
  totalCount: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export interface TodoItemDto {
  id: number
  listId: number
  title: string
  note?: string
  priority: PriorityLevel
  reminder?: string
  done: boolean
  created: string
  createdBy?: string
  lastModified?: string
  lastModifiedBy?: string
}

export interface TodoListDto {
  id: number
  title: string
  colour: string
  items: TodoItemDto[]
}

export interface TodoListBriefDto {
  id: number
  title: string
}

export interface CreateTodoItemCommand {
  listId: number
  title: string
  note?: string
  priority?: PriorityLevel
  reminder?: string
}

export interface UpdateTodoItemCommand {
  id: number
  title: string
  note?: string
  priority?: PriorityLevel
  reminder?: string
  done: boolean
}

export interface CreateTodoListCommand {
  title: string
}

export interface UpdateTodoListCommand {
  id: number
  title: string
}

export enum PriorityLevel {
  None = 0,
  Low = 1,
  Normal = 2,
  High = 3,
  Critical = 4
}

export interface WeatherForecastDto {
  date: string
  temperatureC: number
  temperatureF: number
  summary: string
}

// API Error Response
export interface ApiErrorResponse {
  type: string
  title: string
  status: number
  detail?: string
  instance?: string
  errors?: Record<string, string[]>
}

// Pagination Request
export interface PaginationRequest {
  pageNumber?: number
  pageSize?: number
}

// Todo Items Query Parameters
export interface TodoItemsQuery extends PaginationRequest {
  listId?: number
  searchTerm?: string
  priority?: PriorityLevel
  done?: boolean
}