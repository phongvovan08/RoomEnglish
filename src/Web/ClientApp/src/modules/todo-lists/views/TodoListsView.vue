<template>
  <div class="space-y-6">
    <div class="flex justify-between items-center">
      <h1 class="text-3xl font-bold text-gray-900">{{ $t('menu.todoLists') }}</h1>
      <router-link
        :to="Routes.TodoLists.children.Create.path"
        class="inline-flex items-center px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors duration-200"
      >
        <Icon icon="mdi:plus" class="w-5 h-5 mr-2" />
        {{ $t('menu.create') }}
      </router-link>
    </div>
    
    <div class="bg-white rounded-lg shadow-sm border border-gray-200">
      <!-- Loading State -->
      <div v-if="loading" class="p-8 text-center">
        <Icon icon="mdi:loading" class="w-8 h-8 animate-spin text-blue-600 mx-auto mb-4" />
        <p class="text-gray-600">{{ $t('common.loading') }}</p>
      </div>
      
      <!-- Error State -->
      <div v-else-if="error" class="p-8 text-center">
        <Icon icon="mdi:alert-circle" class="w-8 h-8 text-red-600 mx-auto mb-4" />
        <p class="text-red-600 mb-4">{{ error }}</p>
        <button
          @click="loadTodoLists"
          class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
        >
          {{ $t('common.retry') }}
        </button>
      </div>
      
      <!-- Todo Lists Grid -->
      <div v-else-if="todoLists.length > 0" class="p-6">
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <div
            v-for="todoList in todoLists"
            :key="todoList.id"
            class="bg-white border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow duration-200"
          >
            <div class="flex justify-between items-start mb-3">
              <h3 class="text-lg font-semibold text-gray-900 truncate">{{ todoList.title }}</h3>
              <div class="flex space-x-2">
                <router-link
                  :to="`${Routes.TodoLists.children.Edit.path.replace(':id', todoList.id.toString())}`"
                  class="text-blue-600 hover:text-blue-800"
                >
                  <Icon icon="mdi:pencil" class="w-5 h-5" />
                </router-link>
                <button
                  @click="deleteTodoList(todoList.id)"
                  class="text-red-600 hover:text-red-800"
                >
                  <Icon icon="mdi:delete" class="w-5 h-5" />
                </button>
              </div>
            </div>
            
            <div class="flex items-center justify-between text-sm text-gray-600">
              <span class="flex items-center">
                <Icon icon="mdi:format-list-checks" class="w-4 h-4 mr-1" />
                {{ todoList.items?.length || 0 }} {{ $t('todoLists.items') }}
              </span>
              <span class="px-2 py-1 rounded-full text-xs" :class="getColorClass(todoList.colour)">
                {{ todoList.colour }}
              </span>
            </div>
            
            <router-link
              :to="`${Routes.TodoLists.children.View.path.replace(':id', todoList.id.toString())}`"
              class="block mt-3 text-blue-600 hover:text-blue-800 text-sm font-medium"
            >
              {{ $t('common.viewDetails') }}
            </router-link>
          </div>
        </div>
      </div>
      
      <!-- Empty State -->
      <div v-else class="p-8 text-center">
        <Icon icon="mdi:format-list-bulleted-square" class="w-16 h-16 text-gray-400 mx-auto mb-4" />
        <h3 class="text-lg font-medium text-gray-900 mb-2">{{ $t('todoLists.empty.title') }}</h3>
        <p class="text-gray-600 mb-4">{{ $t('todoLists.empty.description') }}</p>
        <router-link
          :to="Routes.TodoLists.children.Create.path"
          class="inline-flex items-center px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
        >
          <Icon icon="mdi:plus" class="w-5 h-5 mr-2" />
          {{ $t('todoLists.createFirst') }}
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Routes } from '@/router/constants'

// Types
interface TodoList {
  id: number
  title: string
  colour?: string
  items?: TodoItem[]
}

interface TodoItem {
  id: number
  title: string
  done: boolean
}

// Reactive data
const todoLists = ref<TodoList[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

// Methods
const loadTodoLists = async () => {
  try {
    loading.value = true
    error.value = null
    
    // TODO: Replace with actual API call
    // const response = await fetch('/api/TodoLists')
    // if (!response.ok) throw new Error('Failed to fetch todo lists')
    // const data = await response.json()
    // todoLists.value = data.lists || []
    
    // Mock data for now
    await new Promise(resolve => setTimeout(resolve, 1000))
    todoLists.value = [
      {
        id: 1,
        title: 'Personal Tasks',
        colour: 'Blue',
        items: [{ id: 1, title: 'Sample task', done: false }]
      },
      {
        id: 2,
        title: 'Work Projects',
        colour: 'Green',
        items: []
      }
    ]
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'An error occurred'
  } finally {
    loading.value = false
  }
}

const deleteTodoList = async (id: number) => {
  if (!confirm('Are you sure you want to delete this todo list?')) return
  
  try {
    // TODO: Replace with actual API call
    // await fetch(`/api/TodoLists/${id}`, { method: 'DELETE' })
    todoLists.value = todoLists.value.filter(list => list.id !== id)
  } catch (err) {
    console.error('Error deleting todo list:', err)
  }
}

const getColorClass = (colour?: string) => {
  const colorMap: Record<string, string> = {
    'Blue': 'bg-blue-100 text-blue-800',
    'Green': 'bg-green-100 text-green-800',
    'Red': 'bg-red-100 text-red-800',
    'Yellow': 'bg-yellow-100 text-yellow-800',
    'Purple': 'bg-purple-100 text-purple-800',
  }
  return colorMap[colour || ''] || 'bg-gray-100 text-gray-800'
}

// Load data on mount
onMounted(() => {
  loadTodoLists()
})
</script>