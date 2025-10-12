import { ref } from 'vue'
import { TodoListsService } from '@/services/todoService'
import type { TodoList, CreateTodoListRequest, UpdateTodoListRequest, PriorityLevel } from '@/services/todoService'
import { useNotifications } from './useNotifications'
import { ApiHealthService } from '@/services/apiHealthService'

export function useTodoLists() {
  const { addNotification } = useNotifications()
  
  // Reactive state
  const todoLists = ref<TodoList[]>([])
  const priorityLevels = ref<PriorityLevel[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  
  // Enhanced error handling with API health check
  const handleApiError = async (err: unknown, operation: string): Promise<string> => {
    let errorMessage = `Failed to ${operation}`
    
    // Use ApiHealthService for better error messages
    if (err instanceof Error) {
      errorMessage = ApiHealthService.getErrorMessage(err)
      
      // If it's a network error, also check API health
      if (err.message.includes('Failed to fetch')) {
        const healthCheck = await ApiHealthService.checkApiHealth()
        if (!healthCheck.isHealthy) {
          errorMessage = healthCheck.message
        }
      }
    }
    
    return errorMessage
  }

  // Load all TodoLists
  const loadTodoLists = async (): Promise<void> => {
    loading.value = true
    error.value = null
    
    try {
      const data = await TodoListsService.getAllTodoListsWithData()
      todoLists.value = data.lists || []
      priorityLevels.value = data.priorityLevels || []
      
      addNotification({
        type: 'success',
        title: 'Success',
        message: `Loaded ${todoLists.value.length} todo lists`,
      })
    } catch (err) {
      const errorMessage = await handleApiError(err, 'load todo lists')
      
      error.value = errorMessage
      addNotification({
        type: 'error',
        title: 'API Connection Error',
        message: errorMessage,
        duration: 10000 // Show longer for errors
      })
      console.error('Load TodoLists error:', err)
    } finally {
      loading.value = false
    }
  }

  // Get TodoList by ID
  const getTodoListById = async (id: number): Promise<TodoList | null> => {
    loading.value = true
    error.value = null
    
    try {
      const todoList = await TodoListsService.getTodoListById(id)
      return todoList
    } catch (err) {
      let errorMessage = 'Failed to load todo list'
      
      if (err instanceof Error) {
        if (err.message.includes('Failed to fetch')) {
          errorMessage = 'Cannot connect to server. Please check backend connection.'
        } else if (err.message.includes('401')) {
          errorMessage = 'Authentication required. Please login.'
        } else if (err.message.includes('404')) {
          errorMessage = 'Todo list not found or API endpoint unavailable.'
        } else {
          errorMessage = err.message
        }
      }
      
      error.value = errorMessage
      addNotification({
        type: 'error',
        title: 'API Error',
        message: errorMessage,
        duration: 8000
      })
      console.error('Get TodoList error:', err)
      return null
    } finally {
      loading.value = false
    }
  }

  // Create new TodoList
  const createTodoList = async (todoListData: CreateTodoListRequest): Promise<boolean> => {
    loading.value = true
    error.value = null
    
    try {
      const newTodoList = await TodoListsService.createTodoList(todoListData)
      
      // Add to local state
      todoLists.value.push(newTodoList)
      
      addNotification({
        type: 'success',
        title: 'Success',
        message: `Todo list "${todoListData.title}" created successfully`,
      })
      
      return true
    } catch (err) {
      let errorMessage = 'Failed to create todo list'
      
      if (err instanceof Error) {
        if (err.message.includes('Failed to fetch')) {
          errorMessage = 'Cannot connect to server. Backend may be offline.'
        } else if (err.message.includes('401')) {
          errorMessage = 'Authentication expired. Please login again.'
        } else if (err.message.includes('400')) {
          errorMessage = 'Invalid todo list data. Please check your input.'
        } else if (err.message.includes('409')) {
          errorMessage = 'Todo list with this name already exists.'
        } else {
          errorMessage = err.message
        }
      }
      
      error.value = errorMessage
      addNotification({
        type: 'error',
        title: 'Create Failed',
        message: errorMessage,
        duration: 8000
      })
      console.error('Create TodoList error:', err)
      return false
    } finally {
      loading.value = false
    }
  }

  // Update TodoList
  const updateTodoList = async (todoListData: UpdateTodoListRequest): Promise<boolean> => {
    loading.value = true
    error.value = null
    
    try {
      await TodoListsService.updateTodoList(todoListData)
      
      // Update local state
      const index = todoLists.value.findIndex(list => list.id === todoListData.id)
      if (index !== -1) {
        todoLists.value[index] = { ...todoLists.value[index], ...todoListData }
      }
      
      addNotification({
        type: 'success',
        title: 'Success',
        message: `Todo list "${todoListData.title}" updated successfully`,
      })
      
      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to update todo list'
      addNotification({
        type: 'error',
        title: 'Error',
        message: error.value,
      })
      console.error('Update TodoList error:', err)
      return false
    } finally {
      loading.value = false
    }
  }

  // Delete TodoList
  const deleteTodoList = async (id: number): Promise<boolean> => {
    loading.value = true
    error.value = null
    
    try {
      await TodoListsService.deleteTodoList(id)
      
      // Remove from local state
      const index = todoLists.value.findIndex(list => list.id === id)
      if (index !== -1) {
        const deletedList = todoLists.value.splice(index, 1)[0]
        addNotification({
          type: 'success',
          title: 'Success',
          message: `Todo list "${deletedList.title}" deleted successfully`,
        })
      }
      
      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to delete todo list'
      addNotification({
        type: 'error',
        title: 'Error',
        message: error.value,
      })
      console.error('Delete TodoList error:', err)
      return false
    } finally {
      loading.value = false
    }
  }

  // Computed properties
  const todoListsCount = computed(() => todoLists.value.length)
  const hasError = computed(() => !!error.value)
  const isLoading = computed(() => loading.value)

  return {
    // State
    todoLists: readonly(todoLists),
    priorityLevels: readonly(priorityLevels),
    loading: readonly(loading),
    error: readonly(error),
    
    // Computed
    todoListsCount,
    hasError,
    isLoading,
    
    // Actions
    loadTodoLists,
    getTodoListById,
    createTodoList,
    updateTodoList,
    deleteTodoList,
  }
}