// API Base URL mapping from backend
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:5001/api'

// API Endpoints mapping to backend controllers
export const API_ENDPOINTS = {
  // Authentication endpoints (maps to /api/Authentication)
  AUTHENTICATION: {
    GET_TOKEN: '/Authentication/GetToken',
    GET_DEFAULT_TOKEN: '/Authentication/GetDefaultToken',
  },
  
  // Users endpoints (maps to /api/Users)
  USERS: {
    LOGIN: '/Users/login',
    REGISTER: '/Users/register', 
    REFRESH: '/Users/refresh',
  },
  
  // TodoLists endpoints (maps to /api/TodoLists)
  TODO_LISTS: {
    GET_ALL: '/TodoLists',
    CREATE: '/TodoLists',
    GET_BY_ID: (id: number) => `/TodoLists/${id}`,
    UPDATE: (id: number) => `/TodoLists/${id}`,
    DELETE: (id: number) => `/TodoLists/${id}`,
  },
  
  // TodoItems endpoints (maps to /api/TodoItems)
  TODO_ITEMS: {
    GET_WITH_PAGINATION: '/TodoItems',
    CREATE: '/TodoItems',
    GET_BY_ID: (id: number) => `/TodoItems/${id}`,
    UPDATE: (id: number) => `/TodoItems/${id}`,
    UPDATE_DETAILS: (id: number) => `/TodoItems/UpdateItemDetails?Id=${id}`,
    DELETE: (id: number) => `/TodoItems/${id}`,
  },
  
  // WeatherForecasts endpoints (maps to /api/WeatherForecasts)
  WEATHER: {
    GET_FORECASTS: '/WeatherForecasts',
  },
}

// Base API class
class ApiService {
  private baseURL: string

  constructor() {
    this.baseURL = API_BASE_URL
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const url = `${this.baseURL}${endpoint}`
    
    // Get token from localStorage or other auth mechanism
    const token = localStorage.getItem('access_token')
    
    const config: RequestInit = {
      headers: {
        'Content-Type': 'application/json',
        ...(token && { Authorization: `Bearer ${token}` }),
        ...options.headers,
      },
      ...options,
    }

    try {
      const response = await fetch(url, config)
      
      if (!response.ok) {
        // Handle specific HTTP status codes
        let errorMessage = `HTTP ${response.status}: ${response.statusText}`
        
        if (response.status === 401) {
          errorMessage = 'Authentication required. Please login first.'
        } else if (response.status === 403) {
          errorMessage = 'Access denied. You don\'t have permission to perform this action.'
        } else if (response.status === 404) {
          errorMessage = 'The requested resource was not found.'
        } else if (response.status === 500) {
          errorMessage = 'Internal server error. Please try again later.'
        }
        
        // Try to get detailed error from response body
        try {
          const errorBody = await response.text()
          if (errorBody) {
            const parsedError = JSON.parse(errorBody)
            if (parsedError.title || parsedError.detail) {
              errorMessage = `${parsedError.title || 'Error'}: ${parsedError.detail || errorMessage}`
            }
          }
        } catch {
          // Ignore JSON parse errors, use default message
        }
        
        throw new Error(errorMessage)
      }
      
      // Handle empty responses
      const contentType = response.headers.get('content-type')
      if (contentType && contentType.includes('application/json')) {
        return await response.json()
      }
      
      return response.text() as T
    } catch (error) {
      console.error(`API request failed: ${url}`, error)
      throw error
    }
  }

  // Generic HTTP methods
  async get<T>(endpoint: string): Promise<T> {
    return this.request<T>(endpoint, { method: 'GET' })
  }

  async post<T>(endpoint: string, data?: any): Promise<T> {
    return this.request<T>(endpoint, {
      method: 'POST',
      body: data ? JSON.stringify(data) : undefined,
    })
  }

  async put<T>(endpoint: string, data?: any): Promise<T> {
    return this.request<T>(endpoint, {
      method: 'PUT',
      body: data ? JSON.stringify(data) : undefined,
    })
  }

  async delete<T>(endpoint: string): Promise<T> {
    return this.request<T>(endpoint, { method: 'DELETE' })
  }
}

// Create singleton instance
export const apiService = new ApiService()

// Specific service classes for each domain
export class AuthenticationService {
  static async getToken(email: string, password: string) {
    return apiService.post(API_ENDPOINTS.AUTHENTICATION.GET_TOKEN, {
      email,
      password,
    })
  }

  static async getDefaultToken() {
    return apiService.post(API_ENDPOINTS.AUTHENTICATION.GET_DEFAULT_TOKEN)
  }
}

export class TodoListsService {
  static async getAll() {
    return apiService.get(API_ENDPOINTS.TODO_LISTS.GET_ALL)
  }

  static async create(title: string) {
    return apiService.post(API_ENDPOINTS.TODO_LISTS.CREATE, { title })
  }

  static async getById(id: number) {
    return apiService.get(API_ENDPOINTS.TODO_LISTS.GET_BY_ID(id))
  }

  static async update(id: number, title: string) {
    return apiService.put(API_ENDPOINTS.TODO_LISTS.UPDATE(id), { id, title })
  }

  static async delete(id: number) {
    return apiService.delete(API_ENDPOINTS.TODO_LISTS.DELETE(id))
  }
}

export class TodoItemsService {
  static async getWithPagination(listId?: number, pageNumber = 1, pageSize = 10) {
    const params = new URLSearchParams({
      PageNumber: pageNumber.toString(),
      PageSize: pageSize.toString(),
      ...(listId && { ListId: listId.toString() }),
    })
    
    return apiService.get(`${API_ENDPOINTS.TODO_ITEMS.GET_WITH_PAGINATION}?${params}`)
  }

  static async getById(id: number) {
    return apiService.get(API_ENDPOINTS.TODO_ITEMS.GET_BY_ID(id))
  }

  static async create(command: {
    listId: number
    title: string
    note?: string
    priority?: number
    reminder?: string
  }) {
    return apiService.post(API_ENDPOINTS.TODO_ITEMS.CREATE, command)
  }

  static async update(id: number, command: {
    id: number
    title: string
    note?: string
    priority?: number
    reminder?: string
    done: boolean
  }) {
    return apiService.put(API_ENDPOINTS.TODO_ITEMS.UPDATE(id), command)
  }

  static async updateDetails(id: number, title: string, note?: string, priority?: number) {
    return apiService.put(API_ENDPOINTS.TODO_ITEMS.UPDATE_DETAILS(id), {
      title,
      note,
      priority
    })
  }

  static async delete(id: number) {
    return apiService.delete(API_ENDPOINTS.TODO_ITEMS.DELETE(id))
  }
}

export class WeatherService {
  static async getForecasts() {
    return apiService.get(API_ENDPOINTS.WEATHER.GET_FORECASTS)
  }
}

export default apiService