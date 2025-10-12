import { ref } from 'vue'
import { TodoListsService } from '@/services/todoService'
import type { TodoList, CreateTodoListRequest, UpdateTodoListRequest } from '@/services/todoService'
import { useNotifications } from './useNotifications'

export function useTodoLists() {
  const { addNotification } = useNotifications()
  
  // Reactive state
  const todoLists = ref<TodoList[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Load all TodoLists
  const loadTodoLists = async (): Promise<void> => {
    loading.value = true
    error.value = null
    
    try {
      todoLists.value = await TodoListsService.getAllTodoLists()
      addNotification({
        type: 'success',
        title: 'Success',
        message: `Loaded ${todoLists.value.length} todo lists`,
      })
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to load todo lists'
      addNotification({
        type: 'error',
        title: 'Error',
        message: error.value,
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
      error.value = err instanceof Error ? err.message : 'Failed to load todo list'
      addNotification({
        type: 'error',
        title: 'Error',
        message: error.value,
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
      error.value = err instanceof Error ? err.message : 'Failed to create todo list'
      addNotification({
        type: 'error',
        title: 'Error',
        message: error.value,
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