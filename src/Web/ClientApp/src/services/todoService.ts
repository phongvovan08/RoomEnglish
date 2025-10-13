import appConfig from '@/config/app.config'

// TodoList interfaces
export interface TodoList {
  id?: number
  title: string
  colour: string
  items?: TodoItem[]
}

export interface TodoItem {
  id?: number
  listId: number
  title: string
  note?: string
  priority: number
  reminder?: Date
  done: boolean
}

export interface PriorityLevel {
  id: number
  title: string
}

export interface TodoListsResponse {
  lists: TodoList[]
  priorityLevels: PriorityLevel[]
}

export interface PaginatedTodoItems {
  items: TodoItem[]
  pageNumber: number
  totalPages: number
  totalCount: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export interface CreateTodoListRequest {
  title: string
  colour?: string
}

export interface UpdateTodoListRequest {
  id: number
  title: string
  colour?: string
}

export class TodoListsService {
  private static readonly BASE_URL = `${appConfig.apiBaseUrl}/api/TodoLists`

  // Get all TodoLists with priority levels
  static async getAllTodoListsWithData(): Promise<TodoListsResponse> {
    try {
      const response = await fetch(this.BASE_URL, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${this.getAuthToken()}`,
          'Content-Type': 'application/json',
        },
      })

      if (!response.ok) {
        const errorText = await response.text()
        throw new Error(`HTTP ${response.status}: ${errorText || response.statusText}`)
      }

      return response.json()
    } catch (error) {
      if (error instanceof TypeError && error.message.includes('Failed to fetch')) {
        throw new Error('Network error: Cannot connect to API server. Please check if backend is running.')
      }
      throw error
    }
  }

  // Get all TodoLists (backward compatibility)
  static async getAllTodoLists(): Promise<TodoList[]> {
    const data = await this.getAllTodoListsWithData()
    return data.lists || []
  }

  // Get priority levels
  static async getPriorityLevels(): Promise<PriorityLevel[]> {
    const data = await this.getAllTodoListsWithData()
    return data.priorityLevels || []
  }



  // Create new TodoList
  static async createTodoList(todoList: CreateTodoListRequest): Promise<TodoList> {
    try {
      const response = await fetch(this.BASE_URL, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${this.getAuthToken()}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(todoList),
      })

      if (!response.ok) {
        const errorText = await response.text()
        throw new Error(`HTTP ${response.status}: ${errorText || 'Failed to create TodoList'}`)
      }

      return response.json()
    } catch (error) {
      if (error instanceof TypeError && error.message.includes('Failed to fetch')) {
        throw new Error('Network error: Cannot connect to API server.')
      }
      throw error
    }
  }

  // Update TodoList
  static async updateTodoList(todoList: UpdateTodoListRequest): Promise<void> {
    const response = await fetch(`${this.BASE_URL}/${todoList.id}`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${this.getAuthToken()}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(todoList),
    })

    if (!response.ok) {
      const error = await response.text()
      throw new Error(error || 'Failed to update TodoList')
    }
  }

  // Delete TodoList
  static async deleteTodoList(id: number): Promise<void> {
    const response = await fetch(`${this.BASE_URL}/${id}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${this.getAuthToken()}`,
        'Content-Type': 'application/json',
      },
    })

    if (!response.ok) {
      const error = await response.text()
      throw new Error(error || 'Failed to delete TodoList')
    }
  }

  // Helper method to get auth token
  private static getAuthToken(): string | null {
    return localStorage.getItem(appConfig.auth.tokenKey)
  }
}

// TodoItems service
export class TodoItemsService {
  private static readonly BASE_URL = `${appConfig.apiBaseUrl}/api/TodoItems`

  // Get TodoItems with pagination
  static async getTodoItems(pageNumber = 1, pageSize = 10): Promise<PaginatedTodoItems> {
    const params = new URLSearchParams({
      PageNumber: pageNumber.toString(),
      PageSize: pageSize.toString(),
    })

    const response = await fetch(`${this.BASE_URL}?${params}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${this.getAuthToken()}`,
        'Content-Type': 'application/json',
      },
    })

    if (!response.ok) {
      throw new Error(`Failed to fetch TodoItems: ${response.statusText}`)
    }

    return response.json()
  }

  // Get TodoItem by ID
  static async getTodoItemById(id: number): Promise<TodoItem> {
    const response = await fetch(`${this.BASE_URL}/${id}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${this.getAuthToken()}`,
        'Content-Type': 'application/json',
      },
    })

    if (!response.ok) {
      throw new Error(`Failed to fetch TodoItem: ${response.statusText}`)
    }

    return response.json()
  }

  // Create new TodoItem
  static async createTodoItem(todoItem: Partial<TodoItem>): Promise<TodoItem> {
    const response = await fetch(this.BASE_URL, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.getAuthToken()}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(todoItem),
    })

    if (!response.ok) {
      const error = await response.text()
      throw new Error(error || 'Failed to create TodoItem')
    }

    return response.json()
  }

  // Update TodoItem
  static async updateTodoItem(todoItem: TodoItem): Promise<void> {
    const response = await fetch(`${this.BASE_URL}/${todoItem.id}`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${this.getAuthToken()}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(todoItem),
    })

    if (!response.ok) {
      const error = await response.text()
      throw new Error(error || 'Failed to update TodoItem')
    }
  }

  // Delete TodoItem
  static async deleteTodoItem(id: number): Promise<void> {
    const response = await fetch(`${this.BASE_URL}/${id}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${this.getAuthToken()}`,
        'Content-Type': 'application/json',
      },
    })

    if (!response.ok) {
      const error = await response.text()
      throw new Error(error || 'Failed to delete TodoItem')
    }
  }

  // Helper method to get auth token
  private static getAuthToken(): string | null {
    return localStorage.getItem(appConfig.auth.tokenKey)
  }
}