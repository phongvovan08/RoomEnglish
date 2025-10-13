<template>
  <div class="todo-lists-container">
    <!-- Header Section -->
    <div class="header-section">
      <div class="flex justify-between items-center mb-8">
        <div>
          <h1 class="text-4xl font-bold text-white gaming-glow mb-2">{{ $t('menu.todoLists') }}</h1>
          <p class="text-gray-400">Organize your tasks with gaming style</p>
        </div>
        <router-link
          :to="Routes.TodoLists.children.Create.path"
          class="gaming-btn-primary flex items-center px-6 py-3"
        >
          <Icon icon="mdi:plus" class="w-5 h-5 mr-2" />
          {{ $t('menu.create') }}
        </router-link>
      </div>
    </div>
    
    <!-- Content Section -->
    <div class="content-section">
      <!-- Loading State -->
      <div v-if="loading" class="loading-container">
        <div class="gaming-card p-8 text-center">
          <div class="loading-spinner"></div>
          <p class="text-gray-400 mt-4">{{ $t('common.loading') }}</p>
        </div>
      </div>
      
      <!-- Error State -->
      <div v-else-if="error" class="error-container">
        <div class="gaming-card p-8 text-center border-red-500/30">
          <Icon icon="mdi:alert-circle" class="w-12 h-12 text-red-400 mx-auto mb-4 gaming-pulse" />
          <p class="text-red-400 mb-6 text-lg">{{ error }}</p>
          <button
            @click="loadTodoLists"
            class="gaming-btn-secondary px-6 py-3 border-red-500/50 hover:border-red-400"
          >
            <Icon icon="mdi:refresh" class="w-5 h-5 mr-2" />
            {{ $t('common.retry') }}
          </button>
        </div>
      </div>
      
      <!-- Todo Lists Grid -->
      <div v-else-if="todoLists.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div
          v-for="todoList in todoLists"
          :key="todoList.id"
          class="todo-list-card gaming-card group"
        >
          <!-- Header with actions -->
          <div class="flex justify-between items-start mb-4">
            <h3 class="text-xl font-bold text-white truncate group-hover:text-pink-300 transition-colors">
              {{ todoList.title }}
            </h3>
            <div class="flex space-x-2 opacity-0 group-hover:opacity-100 transition-opacity">
              <router-link
                :to="`${Routes.TodoLists.children.Edit.path.replace(':id', todoList.id?.toString() || '')}`"
                class="action-btn text-blue-400 hover:text-blue-300"
                title="Edit List"
              >
                <Icon icon="mdi:pencil" class="w-4 h-4" />
              </router-link>
              <button
                @click="handleDeleteTodoList(todoList.id!)"
                class="action-btn text-red-400 hover:text-red-300"
                title="Delete List"
              >
                <Icon icon="mdi:delete" class="w-4 h-4" />
              </button>
            </div>
          </div>
          
          <!-- Stats -->
          <div class="flex items-center justify-between mb-4">
            <div class="flex items-center space-x-2 text-gray-400">
              <Icon icon="mdi:format-list-checks" class="w-4 h-4" />
              <span class="text-sm">{{ todoList.items?.length || 0 }} items</span>
            </div>
            <div class="color-badge" :class="getColorClass(todoList.colour)">
              {{ todoList.colour }}
            </div>
          </div>
          
          <!-- Progress Bar -->
          <div class="progress-section mb-4">
            <div class="flex justify-between text-xs text-gray-400 mb-1">
              <span>Progress</span>
              <span>{{ Math.round((completedItems(todoList) / Math.max(todoList.items?.length || 1, 1)) * 100) }}%</span>
            </div>
            <div class="progress-bar">
              <div 
                class="progress-fill"
                :style="`width: ${Math.round((completedItems(todoList) / Math.max(todoList.items?.length || 1, 1)) * 100)}%`"
              ></div>
            </div>
          </div>
          
          <!-- View Details Button -->
          <router-link
            :to="{
              name: Routes.TodoItems.children.ByList.name,
              params: { listId: todoList.id?.toString() || '' },
              query: { title: todoList.title }
            }"
            class="gaming-btn-primary w-full text-center py-2 block"
          >
            <Icon icon="mdi:eye" class="w-4 h-4 mr-2 inline" />
            {{ $t('common.viewDetails') }}
          </router-link>
        </div>
      </div>
      
      <!-- Empty State -->
      <div v-else class="empty-state">
        <div class="gaming-card p-12 text-center">
          <Icon icon="mdi:format-list-bulleted-square" class="w-20 h-20 text-pink-500/30 mx-auto mb-6 gaming-pulse" />
          <h3 class="text-xl font-medium text-gray-300 mb-3">{{ $t('todoLists.empty.title') }}</h3>
          <p class="text-gray-500 mb-6">{{ $t('todoLists.empty.description') }}</p>
          <router-link
            :to="Routes.TodoLists.children.Create.path"
            class="gaming-btn-primary inline-flex items-center px-6 py-3"
          >
            <Icon icon="mdi:plus" class="w-5 h-5 mr-2" />
            {{ $t('todoLists.createFirst') }}
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Routes } from '@/router/constants'
import { useTodoLists } from '@/composables/useTodoLists'

// Use TodoLists composable
const { 
  todoLists, 
  loading, 
  error, 
  loadTodoLists, 
  deleteTodoList: deleteTodoListAPI 
} = useTodoLists()

// Methods
const handleDeleteTodoList = async (id: number) => {
  if (!confirm('Are you sure you want to delete this todo list?')) return
  await deleteTodoListAPI(id)
}

const completedItems = (todoList: any) => {
  if (!todoList.items) return 0
  return todoList.items.filter((item: any) => item.done).length
}

const getColorClass = (colour?: string) => {
  const colorMap: Record<string, string> = {
    'Blue': 'bg-blue-500/20 text-blue-300 border border-blue-500/30',
    'Green': 'bg-green-500/20 text-green-300 border border-green-500/30', 
    'Red': 'bg-red-500/20 text-red-300 border border-red-500/30',
    'Yellow': 'bg-yellow-500/20 text-yellow-300 border border-yellow-500/30',
    'Purple': 'bg-purple-500/20 text-purple-300 border border-purple-500/30',
  }
  return colorMap[colour || ''] || 'bg-gray-500/20 text-gray-300 border border-gray-500/30'
}

// Load data on mount
onMounted(() => {
  loadTodoLists()
})
</script>

<style scoped>
.todo-lists-container {
  min-height: 100vh;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
  padding: 2rem;
  position: relative;
  overflow: hidden;
}

.todo-lists-container::before {
  content: '';
  position: fixed;
  inset: 0;
  background: radial-gradient(circle at 20% 80%, rgba(231, 94, 141, 0.1) 0%, transparent 50%),
              radial-gradient(circle at 80% 20%, rgba(75, 85, 195, 0.1) 0%, transparent 50%);
  pointer-events: none;
  z-index: -1;
}

.header-section {
  position: relative;
  z-index: 2;
}

.gaming-card {
  background: rgba(255, 255, 255, 0.05);
  backdrop-filter: blur(15px);
  border: 1px solid rgba(231, 94, 141, 0.2);
  border-radius: 1rem;
  padding: 1.5rem;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
}

.gaming-card:hover {
  transform: translateY(-4px);
  border-color: rgba(231, 94, 141, 0.4);
  box-shadow: 0 12px 40px rgba(231, 94, 141, 0.2);
}

.gaming-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background: linear-gradient(90deg, transparent, rgba(231, 94, 141, 0.1), transparent);
  transition: left 0.6s ease;
}

.gaming-card:hover::before {
  left: 100%;
}

.gaming-btn-primary {
  background: linear-gradient(135deg, #e75e8d 0%, #4b55c3 100%);
  color: white;
  border: none;
  border-radius: 0.5rem;
  font-weight: 600;
  text-decoration: none;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(231, 94, 141, 0.3);
}

.gaming-btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(231, 94, 141, 0.5);
  color: white;
  text-decoration: none;
}

.gaming-btn-secondary {
  background: rgba(255, 255, 255, 0.1);
  color: white;
  border: 1px solid rgba(231, 94, 141, 0.3);
  border-radius: 0.5rem;
  font-weight: 500;
  text-decoration: none;
  transition: all 0.3s ease;
}

.gaming-btn-secondary:hover {
  background: rgba(231, 94, 141, 0.2);
  border-color: rgba(231, 94, 141, 0.5);
  transform: translateY(-1px);
  color: white;
  text-decoration: none;
}

.gaming-glow {
  text-shadow: 0 0 20px rgba(231, 94, 141, 0.5);
}

.gaming-pulse {
  animation: gaming-pulse 2s infinite;
}

@keyframes gaming-pulse {
  0%, 100% { opacity: 0.3; }
  50% { opacity: 0.6; }
}

.todo-list-card {
  transition: all 0.3s ease;
}

.todo-list-card:hover {
  transform: translateY(-4px) scale(1.02);
}

.action-btn {
  padding: 0.5rem;
  border-radius: 0.375rem;
  transition: all 0.3s ease;
}

.action-btn:hover {
  background: rgba(255, 255, 255, 0.1);
  transform: scale(1.1);
}

.color-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
}

.progress-section {
  margin: 1rem 0;
}

.progress-bar {
  width: 100%;
  height: 6px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 3px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, #e75e8d 0%, #4b55c3 100%);
  border-radius: 3px;
  transition: width 0.5s ease;
  box-shadow: 0 0 10px rgba(231, 94, 141, 0.5);
}

.loading-spinner {
  width: 3rem;
  height: 3rem;
  border: 4px solid rgba(231, 94, 141, 0.2);
  border-top: 4px solid #e75e8d;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.content-section {
  position: relative;
  z-index: 1;
}

.loading-container,
.error-container,
.empty-state {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 400px;
}

/* Responsive design */
@media (max-width: 768px) {
  .todo-lists-container {
    padding: 1rem;
  }
  
  .header-section .flex {
    flex-direction: column;
    align-items: stretch;
    gap: 1rem;
  }
  
  .content-section .grid {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
}
</style>