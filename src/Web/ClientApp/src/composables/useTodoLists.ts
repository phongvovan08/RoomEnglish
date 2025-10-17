import { ref, computed, readonly } from 'vue'
import { TodoListsService } from '@/services/todoService'
import type { TodoList, CreateTodoListRequest, UpdateTodoListRequest, PriorityLevel } from '@/services/todoService'
import { useNotifications } from '@/utils/notifications'

export function useTodoLists() {
  const { addNotification } = useNotifications()
  
  // Reactive state
  const todoLists = ref<TodoList[]>([])
  const priorityLevels = ref<PriorityLevel[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  
  // Enhanced error handling 
  const handleApiError = (err: unknown, operation: string): string => {
    let errorMessage = `Failed to ${operation}`
    
    if (err instanceof Error) {
      if (err.message.includes('Failed to fetch')) {
        errorMessage = 'Cannot connect to API server. Please check if backend is running on https://localhost:5001'
      } else if (err.message.includes('401')) {
        errorMessage = 'Authentication failed. Please login again.'
      } else if (err.message.includes('403')) {
        errorMessage = 'Access denied. You do not have permission for this action.'
      } else if (err.message.includes('404')) {
        errorMessage = 'API endpoint not found. The backend may be misconfigured.'
      } else if (err.message.includes('500')) {
        errorMessage = 'Internal server error. Please check the backend logs.'
      } else {
        errorMessage = err.message
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
      const errorMessage = handleApiError(err, 'load todo lists')
      
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
      // First check if we already have the data in cache
      const cachedList = todoLists.value.find(list => list.id === id)
      if (cachedList) {
        loading.value = false
        return cachedList
      }
      
      // If not cached, load all todo lists and find the one we need
      await loadTodoLists()
      const foundList = todoLists.value.find(list => list.id === id)
      
      if (!foundList) {
        throw new Error('Todo list not found')
      }
      
      return foundList
    } catch (err) {
      let errorMessage = 'Failed to load todo list'
      
      if (err instanceof Error) {
        if (err.message.includes('Failed to fetch')) {
          errorMessage = 'Cannot connect to server. Please check backend connection.'
        } else if (err.message.includes('401')) {
          errorMessage = 'Authentication required. Please login.'
        } else if (err.message.includes('404') || err.message.includes('not found')) {
          errorMessage = 'Todo list not found.'
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