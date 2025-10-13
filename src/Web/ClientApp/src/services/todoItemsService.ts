import appConfig from '@/config/app.config'

// DTO Interfaces based on backend models
export interface TodoItemBriefDto {
  id: number
  listId: number
  title: string
  done: boolean
}

export interface PaginatedList<T> {
  items: T[]
  pageNumber: number
  totalPages: number
  totalCount: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export interface CreateTodoItemRequest {
  listId: number
  title: string
}

export interface UpdateTodoItemRequest {
  id: number
  title: string
  done: boolean
}

export interface GetTodoItemsQuery {
  listId: number
  pageNumber?: number
  pageSize?: number
}

export class TodoItemsService {
  private static readonly BASE_URL = `${appConfig.apiBaseUrl}/api/TodoItems`

  private static getAuthHeaders(): HeadersInit {
    const token = localStorage.getItem(appConfig.auth.tokenKey)
    return {
      'Content-Type': 'application/json',
      ...(token && { Authorization: `Bearer ${token}` })
    }
  }

  // Get todo items with pagination
  static async getTodoItems(query: GetTodoItemsQuery): Promise<PaginatedList<TodoItemBriefDto>> {
    const params = new URLSearchParams({
      listId: query.listId.toString(),
      pageNumber: (query.pageNumber || 1).toString(),
      pageSize: (query.pageSize || 10).toString()
    })

    const response = await fetch(`${this.BASE_URL}?${params}`, {
      method: 'GET',
      headers: this.getAuthHeaders()
    })

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`)
    }

    return await response.json()
  }

  // Create new todo item
  static async createTodoItem(request: CreateTodoItemRequest): Promise<number> {
    const response = await fetch(this.BASE_URL, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(request)
    })

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`)
    }

    // Backend returns Created with the ID
    return await response.json()
  }

  // Update todo item (title and done status)
  static async updateTodoItem(request: UpdateTodoItemRequest): Promise<void> {
    const response = await fetch(`${this.BASE_URL}/${request.id}`, {
      method: 'PUT',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(request)
    })

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`)
    }
  }

  // Delete todo item
  static async deleteTodoItem(id: number): Promise<void> {
    const response = await fetch(`${this.BASE_URL}/${id}`, {
      method: 'DELETE',
      headers: this.getAuthHeaders()
    })

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`)
    }
  }

  // Toggle todo item done status (convenience method)
  static async toggleTodoItem(item: TodoItemBriefDto): Promise<void> {
    await this.updateTodoItem({
      id: item.id,
      title: item.title,
      done: !item.done
    })
  }
}