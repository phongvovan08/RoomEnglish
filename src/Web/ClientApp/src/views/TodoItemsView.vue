<template>
  <div class="todo-items-container">
    <!-- Header with back navigation -->
    <div class="header-section">
      <div class="flex items-center justify-between mb-6">
        <div class="flex items-center space-x-4">
          <router-link
            :to="Routes.TodoLists.path"
            class="gaming-btn-secondary flex items-center px-4 py-2"
          >
            <Icon icon="mdi:arrow-left" class="w-5 h-5 mr-2" />
            Back to Lists
          </router-link>
          <div>
            <h1 class="text-3xl font-bold text-white gaming-glow">{{ todoListTitle }}</h1>
            <p class="text-gray-400 mt-1">Manage your todo items</p>
          </div>
        </div>
        <button
          @click="showCreateDialog = true"
          class="gaming-btn-primary flex items-center px-6 py-3"
        >
          <Icon icon="mdi:plus" class="w-5 h-5 mr-2" />
          Add Item
        </button>
      </div>
    </div>

    <!-- Stats Section -->
    <div class="stats-section mb-6" v-if="!isLoading">
      <div class="grid grid-cols-3 gap-4">
        <div class="stat-card">
          <div class="stat-value">{{ totalCount }}</div>
          <div class="stat-label">Total Items</div>
        </div>
        <div class="stat-card">
          <div class="stat-value">{{ completedCount }}</div>
          <div class="stat-label">Completed</div>
        </div>
        <div class="stat-card">
          <div class="stat-value">{{ remainingCount }}</div>
          <div class="stat-label">Remaining</div>
        </div>
      </div>
    </div>

    <!-- Loading state -->
    <div v-if="isLoading" class="loading-container">
      <div class="gaming-card p-8 text-center">
        <div class="loading-spinner"></div>
        <p class="text-gray-400 mt-4">Loading todo items...</p>
      </div>
    </div>

    <!-- Todo Items List -->
    <div v-else-if="todoItems.length > 0" class="items-section">
      <div class="space-y-3">
        <div
          v-for="item in todoItems"
          :key="item.id"
          class="todo-item gaming-card"
          :class="{ 'completed': item.done }"
        >
          <div class="flex items-center space-x-4">
            <!-- Checkbox -->
            <button
              @click="handleToggleItem(item)"
              class="todo-checkbox"
              :class="{ 'checked': item.done }"
            >
              <Icon v-if="item.done" icon="mdi:check" class="w-4 h-4 text-white" />
            </button>

            <!-- Content -->
            <div class="flex-1 min-w-0">
              <h3
                class="text-lg font-semibold transition-all duration-300"
                :class="item.done ? 'text-gray-500 line-through' : 'text-white'"
              >
                {{ item.title }}
              </h3>
              <div class="flex items-center justify-between mt-2">
                <span class="text-sm text-gray-400">
                  ID: {{ item.id }}
                </span>
                <div class="flex space-x-2">
                  <button
                    @click="editItem(item)"
                    class="action-btn text-blue-400 hover:text-blue-300"
                  >
                    <Icon icon="mdi:pencil" class="w-4 h-4" />
                  </button>
                  <button
                    @click="confirmDeleteItem(item)"
                    class="action-btn text-red-400 hover:text-red-300"
                  >
                    <Icon icon="mdi:delete" class="w-4 h-4" />
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="empty-state">
      <div class="gaming-card p-12 text-center">
        <Icon icon="mdi:format-list-checks" class="w-20 h-20 text-pink-500/30 mx-auto mb-6 gaming-pulse" />
        <h3 class="text-xl font-medium text-gray-300 mb-3">No Todo Items Yet</h3>
        <p class="text-gray-500 mb-6">Start adding items to organize your tasks</p>
        <button
          @click="showCreateDialog = true"
          class="gaming-btn-primary inline-flex items-center px-6 py-3"
        >
          <Icon icon="mdi:plus" class="w-5 h-5 mr-2" />
          Add First Item
        </button>
      </div>
    </div>

    <!-- Pagination -->
    <div v-if="totalPages > 1" class="pagination-section mt-8">
      <div class="gaming-card p-4">
        <div class="flex items-center justify-between">
          <button
            @click="loadPreviousPage(listId)"
            :disabled="!hasPreviousPage"
            class="gaming-btn-secondary px-4 py-2 disabled:opacity-30"
          >
            <Icon icon="mdi:chevron-left" class="w-5 h-5 mr-1" />
            Previous
          </button>
          
          <div class="flex items-center space-x-4 text-gray-300">
            <span>Page {{ currentPage }} of {{ totalPages }}</span>
            <span class="text-pink-400">•</span>
            <span>{{ totalCount }} total items</span>
          </div>
          
          <button
            @click="loadNextPage(listId)"
            :disabled="!hasNextPage"
            class="gaming-btn-secondary px-4 py-2 disabled:opacity-30"
          >
            Next
            <Icon icon="mdi:chevron-right" class="w-5 h-5 ml-1" />
          </button>
        </div>
      </div>
    </div>

    <!-- Create/Edit Dialog -->
    <div v-if="showCreateDialog || showEditDialog" class="dialog-overlay" @click.self="closeDialogs">
      <div class="dialog-content gaming-card">
        <div class="flex items-center justify-between mb-6">
          <h2 class="text-xl font-bold text-white gaming-glow">
            {{ showEditDialog ? 'Edit Todo Item' : 'Create New Todo Item' }}
          </h2>
          <button @click="closeDialogs" class="text-gray-400 hover:text-white transition-colors">
            <Icon icon="mdi:close" class="w-6 h-6" />
          </button>
        </div>

        <form @submit.prevent="handleSaveItem" class="space-y-6">
          <div>
            <label class="block text-sm font-medium text-gray-300 mb-2">
              Title <span class="text-pink-400">*</span>
            </label>
            <input
              v-model="itemForm.title"
              type="text"
              placeholder="Enter todo item title..."
              class="gaming-input w-full"
              required
              :disabled="isSaving"
            />
          </div>

          <div class="flex justify-end space-x-3 pt-4">
            <button
              type="button"
              @click="closeDialogs"
              class="gaming-btn-secondary px-6 py-2"
              :disabled="isSaving"
            >
              Cancel
            </button>
            <button
              type="submit"
              class="gaming-btn-primary px-6 py-2"
              :disabled="!itemForm.title.trim() || isSaving"
            >
              <Icon v-if="isSaving" icon="mdi:loading" class="w-4 h-4 mr-2 animate-spin" />
              {{ showEditDialog ? 'Update Item' : 'Create Item' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Delete Confirmation Dialog -->
    <div v-if="showDeleteDialog" class="dialog-overlay" @click.self="showDeleteDialog = false">
      <div class="dialog-content gaming-card">
        <div class="text-center">
          <Icon icon="mdi:alert-circle" class="w-16 h-16 text-red-500 mx-auto mb-4" />
          <h3 class="text-xl font-bold text-white mb-2">Delete Todo Item</h3>
          <p class="text-gray-400 mb-6">
            Are you sure you want to delete "{{ deletingItem?.title }}"? 
            <br>
            <span class="text-red-400 font-medium">This action cannot be undone.</span>
          </p>
          <div class="flex justify-center space-x-4">
            <button
              @click="showDeleteDialog = false"
              class="gaming-btn-secondary"
              :disabled="isDeleting"
            >
              Cancel
            </button>
            <button
              @click="handleDeleteItem"
              class="gaming-btn-danger"
              :disabled="isDeleting"
            >
              <Icon v-if="isDeleting" icon="mdi:loading" class="w-4 h-4 mr-2 animate-spin" />
              <Icon v-else icon="mdi:delete" class="w-4 h-4 mr-2" />
              Delete Item
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onUnmounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { Icon } from '@iconify/vue'
import { useTodoItems } from '@/composables/useTodoItems'
import { useNotifications } from '@/composables/useNotifications'
import { Routes } from '@/router/constants'
import type { TodoItemBriefDto } from '@/services/todoItemsService'

const route = useRoute()
const { error } = useNotifications()
const {
  todoItems,
  isLoading,
  totalPages,
  totalCount,
  currentPage,
  hasNextPage,
  hasPreviousPage,
  loadTodoItems,
  createTodoItem,
  updateTodoItem,
  toggleTodoItem,
  deleteTodoItem,
  loadNextPage,
  loadPreviousPage
} = useTodoItems()

// Route params
const listId = computed(() => parseInt(route.params.listId as string))
const todoListTitle = computed(() => route.query.title as string || 'Todo Items')

// Dialog states
const showCreateDialog = ref(false)
const showEditDialog = ref(false)
const showDeleteDialog = ref(false)
const editingItem = ref<TodoItemBriefDto | null>(null)
const deletingItem = ref<TodoItemBriefDto | null>(null)
const isSaving = ref(false)
const isDeleting = ref(false)

// Dropdown menu state
const activeItemMenu = ref<number | null>(null)

// Form data
const itemForm = reactive({
  title: ''
})

// Computed stats
const completedCount = computed(() => todoItems.value.filter((item: TodoItemBriefDto) => item.done).length)
const remainingCount = computed(() => todoItems.value.filter((item: TodoItemBriefDto) => !item.done).length)

// Initialize data
onMounted(async () => {
  if (listId.value && !isNaN(listId.value)) {
    await loadTodoItems(listId.value)
  } else {
    error('Invalid todo list ID')
  }
  
  // Close dropdown menu when clicking outside
  const handleClickOutside = (event: Event) => {
    const target = event.target as HTMLElement
    if (!target.closest('.relative')) {
      closeItemMenu()
    }
  }
  
  document.addEventListener('click', handleClickOutside)
  
  // Clean up event listener
  onUnmounted(() => {
    document.removeEventListener('click', handleClickOutside)
  })
})

// Item actions
const handleToggleItem = async (item: TodoItemBriefDto) => {
  await toggleTodoItem(item)
}

const editItem = (item: TodoItemBriefDto) => {
  closeItemMenu()
  editingItem.value = item
  itemForm.title = item.title
  showEditDialog.value = true
}

const handleSaveItem = async () => {
  if (!itemForm.title.trim()) return

  try {
    isSaving.value = true

    if (showEditDialog.value && editingItem.value) {
      // Update existing item
      await updateTodoItem({
        id: editingItem.value.id,
        title: itemForm.title.trim(),
        done: editingItem.value.done
      })
    } else {
      // Create new item
      await createTodoItem({
        listId: listId.value,
        title: itemForm.title.trim()
      })
    }
    
    closeDialogs()
  } catch (error) {
    console.error('Save item failed:', error)
  } finally {
    isSaving.value = false
  }
}

// Dropdown menu functions
const toggleItemMenu = (itemId: number) => {
  activeItemMenu.value = activeItemMenu.value === itemId ? null : itemId
}

const closeItemMenu = () => {
  activeItemMenu.value = null
}

const confirmDeleteItem = (item: TodoItemBriefDto) => {
  closeItemMenu()
  deletingItem.value = item
  showDeleteDialog.value = true
}

const handleDeleteItem = async () => {
  if (!deletingItem.value) return

  try {
    isDeleting.value = true
    await deleteTodoItem(deletingItem.value.id, listId.value)
    showDeleteDialog.value = false
    deletingItem.value = null
  } catch (error) {
    console.error('Failed to delete item:', error)
  } finally {
    isDeleting.value = false
  }
}

const closeDialogs = () => {
  showCreateDialog.value = false
  showEditDialog.value = false
  showDeleteDialog.value = false
  editingItem.value = null
  deletingItem.value = null
  itemForm.title = ''
}
</script>

<style scoped>
.todo-items-container {
  min-height: 100vh;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
  padding: 2rem;
  position: relative;
  overflow: hidden;
}

.todo-items-container::before {
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
  transform: translateY(-2px);
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
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(231, 94, 141, 0.3);
}

.gaming-btn-primary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(231, 94, 141, 0.5);
}

.gaming-btn-primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.gaming-btn-secondary {
  background: rgba(255, 255, 255, 0.1);
  color: white;
  border: 1px solid rgba(231, 94, 141, 0.3);
  border-radius: 0.5rem;
  font-weight: 500;
  transition: all 0.3s ease;
}

.gaming-btn-secondary:hover:not(:disabled) {
  background: rgba(231, 94, 141, 0.2);
  border-color: rgba(231, 94, 141, 0.5);
  transform: translateY(-1px);
}

.gaming-input {
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(231, 94, 141, 0.3);
  border-radius: 0.5rem;
  padding: 0.75rem 1rem;
  color: white;
  transition: all 0.3s ease;
}

.gaming-input:focus {
  outline: none;
  border-color: #e75e8d;
  box-shadow: 0 0 0 3px rgba(231, 94, 141, 0.2);
  background: rgba(255, 255, 255, 0.15);
}

.gaming-input::placeholder {
  color: rgba(255, 255, 255, 0.5);
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

.stats-section .stat-card {
  background: rgba(255, 255, 255, 0.05);
  backdrop-filter: blur(15px);
  border: 1px solid rgba(231, 94, 141, 0.2);
  border-radius: 1rem;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
  text-align: center;
  padding: 1rem;
}

.stat-value {
  font-size: 2rem;
  font-weight: bold;
  color: #e75e8d;
  text-shadow: 0 0 10px rgba(231, 94, 141, 0.5);
}

.stat-label {
  color: rgba(255, 255, 255, 0.7);
  font-size: 0.875rem;
  margin-top: 0.25rem;
}

.todo-item {
  transition: all 0.3s ease;
}

.todo-item.completed {
  opacity: 0.6;
}

.todo-checkbox {
  width: 1.5rem;
  height: 1.5rem;
  border: 2px solid #e75e8d;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.3s ease;
  flex-shrink: 0;
}

.todo-checkbox.checked {
  background: #e75e8d;
  box-shadow: 0 0 10px rgba(231, 94, 141, 0.5);
}

.todo-checkbox:hover {
  background: rgba(231, 94, 141, 0.2);
  transform: scale(1.1);
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

.dialog-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.8);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  animation: fadeIn 0.3s ease;
}

.dialog-content {
  width: 100%;
  max-width: 500px;
  margin: 2rem;
  max-height: 90vh;
  overflow-y: auto;
  animation: slideIn 0.3s ease;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes slideIn {
  from { 
    opacity: 0;
    transform: translateY(-20px) scale(0.95);
  }
  to { 
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

/* Gaming Buttons */
.gaming-btn-danger {
  background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
  color: white;
  padding: 0.875rem 2rem;
  border: none;
  border-radius: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 1px;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(239, 68, 68, 0.3);
  display: flex;
  align-items: center;
  justify-content: center;
}

.gaming-btn-danger:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(239, 68, 68, 0.4);
}

.gaming-btn-danger:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Responsive design */
@media (max-width: 768px) {
  .todo-items-container {
    padding: 1rem;
  }
  
  .header-section .flex {
    flex-direction: column;
    align-items: stretch;
    gap: 1rem;
  }
  
  .stats-section .grid {
    grid-template-columns: 1fr;
    gap: 0.5rem;
  }
}
</style>
