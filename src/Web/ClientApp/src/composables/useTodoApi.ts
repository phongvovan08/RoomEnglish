import { ref, type Ref } from 'vue'
import { TodoItemsService, TodoListsService } from '@/services/api'
import type { TodoItemDto, PaginatedList, TodoListBriefDto, PriorityLevel } from '@/types/api'

export function useTodoItems() {
  const loading = ref(false)
  const error = ref<string | null>(null)

  const getTodoItems = async (
    listId?: number, 
    pageNumber = 1, 
    pageSize = 10
  ): Promise<PaginatedList<TodoItemDto> | null> => {
    try {
      loading.value = true
      error.value = null
      
      const response = await TodoItemsService.getWithPagination(listId, pageNumber, pageSize)
      return response as PaginatedList<TodoItemDto>
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to load todo items'
      console.error('Failed to load todo items:', err)
      
      // Return mock data for now (until authentication is set up)
      if (err instanceof Error && err.message.includes('Authentication required')) {
        console.warn('Using mock data - authentication not configured')
        return {
          items: [
            {
              id: 1,
              listId: 1,
              title: "Complete Cyborg Gaming Integration",
              note: "Integrate templatemo_579_cyborg_gaming with Vue 3 application",
              priority: 3,
              done: true,
              created: "2024-01-15T10:30:00Z",
              createdBy: "Gaming Developer",
              lastModified: "2024-01-16T14:22:00Z",
              reminder: "2024-01-20T09:00:00Z"
            },
            {
              id: 2,
              listId: 1,
              title: "Implement Real API Integration", 
              note: "Connect frontend to C# backend with proper authentication",
              priority: 2,
              done: false,
              created: "2024-01-16T11:00:00Z",
              createdBy: "Gaming Developer"
            },
            {
              id: 3,
              listId: 2,
              title: "Add Gaming Notifications",
              note: "Create toast notifications with gaming style animations",
              priority: 1,
              done: false,
              created: "2024-01-17T09:15:00Z",
              createdBy: "Gaming Developer"
            }
          ],
          pageNumber: pageNumber,
          totalPages: 1,
          totalCount: 3,
          hasPreviousPage: false,
          hasNextPage: false
        }
      }
      
      return null
    } finally {
      loading.value = false
    }
  }

  const getTodoItem = async (id: number): Promise<TodoItemDto | null> => {
    try {
      loading.value = true
      error.value = null
      
      const response = await TodoItemsService.getById(id)
      return response as TodoItemDto
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to load todo item'
      console.error('Failed to load todo item:', err)
      return null
    } finally {
      loading.value = false
    }
  }

  const createTodoItem = async (command: {
    listId: number
    title: string
    note?: string
    priority?: PriorityLevel
    reminder?: string
  }): Promise<boolean> => {
    try {
      loading.value = true
      error.value = null
      
      await TodoItemsService.create(command)
      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to create todo item'
      console.error('Failed to create todo item:', err)
      
      // Mock success for now (until authentication is set up)
      if (err instanceof Error && err.message.includes('Authentication required')) {
        console.warn('Simulating create success - authentication not configured')
        // Simulate API delay
        await new Promise(resolve => setTimeout(resolve, 500))
        return true
      }
      
      return false
    } finally {
      loading.value = false
    }
  }

  const updateTodoItem = async (id: number, command: {
    id: number
    title: string
    note?: string
    priority?: PriorityLevel
    reminder?: string
    done: boolean
  }): Promise<boolean> => {
    try {
      loading.value = true
      error.value = null
      
      await TodoItemsService.update(id, command)
      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to update todo item'
      console.error('Failed to update todo item:', err)
      return false
    } finally {
      loading.value = false
    }
  }

  const deleteTodoItem = async (id: number): Promise<boolean> => {
    try {
      loading.value = true
      error.value = null
      
      await TodoItemsService.delete(id)
      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to delete todo item'
      console.error('Failed to delete todo item:', err)
      return false
    } finally {
      loading.value = false
    }
  }

  return {
    loading: loading as Readonly<Ref<boolean>>,
    error: error as Readonly<Ref<string | null>>,
    getTodoItems,
    getTodoItem,
    createTodoItem,
    updateTodoItem,
    deleteTodoItem
  }
}

export function useTodoLists() {
  const loading = ref(false)
  const error = ref<string | null>(null)

  const getTodoLists = async (): Promise<TodoListBriefDto[]> => {
    try {
      loading.value = true
      error.value = null
      
      const response = await TodoListsService.getAll()
      return response as TodoListBriefDto[]
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to load todo lists'
      console.error('Failed to load todo lists:', err)
      
      // Return mock data for now (until authentication is set up)
      if (err instanceof Error && err.message.includes('Authentication required')) {
        console.warn('Using mock data - authentication not configured')
        return [
          { id: 1, title: "Personal Tasks" },
          { id: 2, title: "Work Projects" }, 
          { id: 3, title: "Shopping List" },
          { id: 4, title: "Learning Goals" }
        ]
      }
      
      return []
    } finally {
      loading.value = false
    }
  }

  return {
    loading: loading as Readonly<Ref<boolean>>,
    error: error as Readonly<Ref<string | null>>,
    getTodoLists
  }
}

// Utility functions
export function getPriorityLabel(priority: PriorityLevel): string {
  const labels: Record<PriorityLevel, string> = {
    [0]: 'None',
    [1]: 'Low', 
    [2]: 'Normal',
    [3]: 'High',
    [4]: 'Critical'
  }
  return labels[priority] || 'Unknown'
}

export function getPriorityColor(priority: PriorityLevel): string {
  const colors: Record<PriorityLevel, string> = {
    [0]: 'priority-0',
    [1]: 'priority-1',
    [2]: 'priority-2', 
    [3]: 'priority-3',
    [4]: 'priority-4'
  }
  return colors[priority] || 'priority-0'
}

export function formatApiDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}