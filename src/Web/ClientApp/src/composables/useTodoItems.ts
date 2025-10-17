import { ref, readonly } from 'vue'
import { TodoItemsService, type TodoItemBriefDto, type PaginatedList, type CreateTodoItemRequest, type UpdateTodoItemRequest } from '@/services/todoItemsService'
import { useNotifications } from '@/utils/notifications'

export function useTodoItems() {
  const { showError, showSuccess, showInfo } = useNotifications()
  
  const todoItems = ref<TodoItemBriefDto[]>([])
  const isLoading = ref(false)
  const totalPages = ref(0)
  const totalCount = ref(0)
  const currentPage = ref(1)
  const hasNextPage = ref(false)
  const hasPreviousPage = ref(false)

  // Load todo items for a specific list
  const loadTodoItems = async (listId: number, pageNumber: number = 1, pageSize: number = 10): Promise<void> => {
    isLoading.value = true
    try {
      const response: PaginatedList<TodoItemBriefDto> = await TodoItemsService.getTodoItems({
        listId,
        pageNumber,
        pageSize
      })
      
      todoItems.value = response.items
      totalPages.value = response.totalPages
      totalCount.value = response.totalCount
      currentPage.value = response.pageNumber
      hasNextPage.value = response.hasNextPage
      hasPreviousPage.value = response.hasPreviousPage
      
    } catch (error) {
      handleApiError(error)
    } finally {
      isLoading.value = false
    }
  }

  // Create new todo item
  const createTodoItem = async (request: CreateTodoItemRequest): Promise<boolean> => {
    try {
      await TodoItemsService.createTodoItem(request)
      showSuccess('Todo item created successfully!')
      
      // Reload current page
      await loadTodoItems(request.listId, currentPage.value)
      return true
    } catch (error) {
      handleApiError(error)
      return false
    }
  }

  // Update todo item
  const updateTodoItem = async (request: UpdateTodoItemRequest): Promise<boolean> => {
    try {
      await TodoItemsService.updateTodoItem(request)
      showSuccess('Todo item updated successfully!')
      
      // Update local state
      const index = todoItems.value.findIndex(item => item.id === request.id)
      if (index !== -1) {
        todoItems.value[index] = {
          ...todoItems.value[index],
          title: request.title,
          done: request.done
        }
      }
      return true
    } catch (error) {
      handleApiError(error)
      return false
    }
  }

  // Toggle todo item done status
  const toggleTodoItem = async (item: TodoItemBriefDto): Promise<boolean> => {
    try {
      await TodoItemsService.toggleTodoItem(item)
      
      // Update local state
      const index = todoItems.value.findIndex(i => i.id === item.id)
      if (index !== -1) {
        todoItems.value[index].done = !todoItems.value[index].done
      }
      
      const status = !item.done ? 'completed' : 'reopened'
      showInfo(`Todo item ${status}!`)
      return true
    } catch (error) {
      handleApiError(error)
      return false
    }
  }

  // Delete todo item
  const deleteTodoItem = async (id: number, listId: number): Promise<boolean> => {
    try {
      await TodoItemsService.deleteTodoItem(id)
      showSuccess('Todo item deleted successfully!')
      
      // Reload current page
      await loadTodoItems(listId, currentPage.value)
      return true
    } catch (error) {
      handleApiError(error)
      return false
    }
  }

  // Load next page
  const loadNextPage = async (listId: number): Promise<void> => {
    if (hasNextPage.value) {
      await loadTodoItems(listId, currentPage.value + 1)
    }
  }

  // Load previous page
  const loadPreviousPage = async (listId: number): Promise<void> => {
    if (hasPreviousPage.value) {
      await loadTodoItems(listId, currentPage.value - 1)
    }
  }

  // Error handling
  const handleApiError = (error: unknown): void => {
    if (error instanceof Error) {
      if (error.message.includes('401')) {
        showError('Please login to access todo items.')
      } else if (error.message.includes('403')) {
        showError('You do not have permission to access this todo list.')
      } else if (error.message.includes('404')) {
        showError('Todo list not found.')
      } else if (error.message.includes('500')) {
        showError('Server error occurred. Please try again later.')
      } else if (error.message.includes('Failed to fetch')) {
        showError('Cannot connect to server. Please check your internet connection.')
      } else {
        showError(`Error: ${error.message}`)
      }
    } else {
      showError('An unexpected error occurred.')
    }
  }

  // Clear data
  const clearTodoItems = (): void => {
    todoItems.value = []
    totalPages.value = 0
    totalCount.value = 0
    currentPage.value = 1
    hasNextPage.value = false
    hasPreviousPage.value = false
  }

  return {
    // State
    todoItems: readonly(todoItems),
    isLoading: readonly(isLoading),
    totalPages: readonly(totalPages),
    totalCount: readonly(totalCount),
    currentPage: readonly(currentPage),
    hasNextPage: readonly(hasNextPage),
    hasPreviousPage: readonly(hasPreviousPage),
    
    // Actions
    loadTodoItems,
    createTodoItem,
    updateTodoItem,
    toggleTodoItem,
    deleteTodoItem,
    loadNextPage,
    loadPreviousPage,
    clearTodoItems
  }
}