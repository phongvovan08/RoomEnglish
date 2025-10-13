<template>
  <div class="edit-todo-list-container">
    <!-- Header Section -->
    <div class="header-section">
      <div class="flex items-center space-x-4 mb-8">
        <router-link
          :to="Routes.TodoLists.path"
          class="gaming-btn-secondary flex items-center p-2"
        >
          <Icon icon="mdi:arrow-left" class="w-5 h-5" />
        </router-link>
        <div>
          <h1 class="text-4xl font-bold text-white gaming-glow">Edit Todo List</h1>
          <p class="text-gray-400 mt-2">Modify your gaming-style todo list</p>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="gaming-card">
      <div class="flex items-center justify-center py-12">
        <div class="loading-spinner"></div>
        <p class="text-gray-400 ml-4">Loading todo list...</p>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="gaming-card">
      <div class="text-center py-12">
        <Icon icon="mdi:alert-circle" class="w-16 h-16 text-red-500 mx-auto mb-4" />
        <h3 class="text-xl font-medium text-white mb-2">Error Loading Todo List</h3>
        <p class="text-gray-400 mb-6">{{ error }}</p>
        <router-link
          :to="Routes.TodoLists.path"
          class="gaming-btn-secondary inline-flex items-center"
        >
          <Icon icon="mdi:arrow-left" class="w-4 h-4 mr-2" />
          Back to Todo Lists
        </router-link>
      </div>
    </div>

    <!-- Edit Form -->
    <div v-else-if="todoList" class="gaming-card">
      <form @submit.prevent="handleUpdateTodoList" class="space-y-8">
        <!-- Title Field -->
        <div class="form-group">
          <label for="title" class="gaming-label">
            Title
          </label>
          <div class="input-wrapper">
            <input
              id="title"
              v-model="form.title"
              type="text"
              required
              class="gaming-input"
              placeholder="Enter todo list title"
            />
            <div class="input-glow"></div>
          </div>
        </div>

        <!-- Color Field -->
        <div class="form-group">
          <label for="colour" class="gaming-label">
            Color
          </label>
          <div class="select-wrapper">
            <select
              id="colour"
              v-model="form.colour"
              class="gaming-select"
            >
              <option value="Blue">Blue</option>
              <option value="Green">Green</option>
              <option value="Red">Red</option>
              <option value="Yellow">Yellow</option>
              <option value="Purple">Purple</option>
              <option value="Orange">Orange</option>
              <option value="Pink">Pink</option>
            </select>
            <div class="select-glow"></div>
          </div>
        </div>

        <!-- Todo List Info -->
        <div class="info-section">
          <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div class="stat-card">
              <div class="stat-icon">
                <Icon icon="mdi:format-list-checks" class="w-6 h-6" />
              </div>
              <div>
                <p class="stat-label">Total Items</p>
                <p class="stat-value">{{ todoList.items?.length || 0 }}</p>
              </div>
            </div>
            <div class="stat-card">
              <div class="stat-icon">
                <Icon icon="mdi:check-circle" class="w-6 h-6" />
              </div>
              <div>
                <p class="stat-label">Completed</p>
                <p class="stat-value">{{ completedItems }}</p>
              </div>
            </div>
            <div class="stat-card">
              <div class="stat-icon">
                <Icon icon="mdi:clock-outline" class="w-6 h-6" />
              </div>
              <div>
                <p class="stat-label">Remaining</p>
                <p class="stat-value">{{ remainingItems }}</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Action Buttons -->
        <div class="flex flex-col sm:flex-row justify-between space-y-4 sm:space-y-0 sm:space-x-4 pt-6">
          <div class="flex space-x-4">
            <router-link
              :to="Routes.TodoLists.path"
              class="gaming-btn-secondary"
            >
              <Icon icon="mdi:arrow-left" class="w-4 h-4 mr-2" />
              Cancel
            </router-link>
            <router-link
              :to="{ name: Routes.TodoItems.children.ByList.name, params: { listId: todoList.id } }"
              class="gaming-btn-secondary"
            >
              <Icon icon="mdi:eye" class="w-4 h-4 mr-2" />
              View Items
            </router-link>
          </div>
          
          <div class="flex space-x-4">
            <button
              type="button"
              @click="handleDeleteTodoList"
              class="gaming-btn-danger"
              :disabled="updating"
            >
              <Icon v-if="deleting" icon="mdi:loading" class="w-4 h-4 mr-2 animate-spin" />
              <Icon v-else icon="mdi:delete" class="w-4 h-4 mr-2" />
              Delete
            </button>
            <button
              type="submit"
              :disabled="updating || !hasChanges"
              class="gaming-btn-primary"
            >
              <Icon v-if="updating" icon="mdi:loading" class="w-4 h-4 mr-2 animate-spin" />
              <Icon v-else icon="mdi:content-save" class="w-4 h-4 mr-2" />
              Update List
            </button>
          </div>
        </div>
      </form>
    </div>

    <!-- Delete Confirmation Dialog -->
    <div v-if="showDeleteDialog" class="dialog-overlay" @click.self="showDeleteDialog = false">
      <div class="dialog-content gaming-card">
        <div class="text-center">
          <Icon icon="mdi:alert-circle" class="w-16 h-16 text-red-500 mx-auto mb-4" />
          <h3 class="text-xl font-bold text-white mb-2">Delete Todo List</h3>
          <p class="text-gray-400 mb-6">
            Are you sure you want to delete "{{ todoList?.title }}"? 
            This action cannot be undone and will delete all items in this list.
          </p>
          <div class="flex justify-center space-x-4">
            <button
              @click="showDeleteDialog = false"
              class="gaming-btn-secondary"
              :disabled="deleting"
            >
              Cancel
            </button>
            <button
              @click="confirmDelete"
              class="gaming-btn-danger"
              :disabled="deleting"
            >
              <Icon v-if="deleting" icon="mdi:loading" class="w-4 h-4 mr-2 animate-spin" />
              Delete List
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Routes } from '@/router/constants'
import { useTodoLists } from '@/composables/useTodoLists'
import { useNotifications } from '@/composables/useNotifications'
import type { TodoListDto, TodoItemDto } from '@/types/api'

const router = useRouter()
const route = useRoute()
const { success: showSuccess, error: showError } = useNotifications()
const { getTodoListById, updateTodoList, deleteTodoList, loading } = useTodoLists()

// Reactive state
const todoList = ref<TodoListDto | null>(null)
const error = ref<string | null>(null)
const updating = ref(false)
const deleting = ref(false)
const showDeleteDialog = ref(false)

// Form data
const form = reactive({
  title: '',
  colour: 'Blue'
})

// Original form data for comparison
const originalForm = ref({
  title: '',
  colour: 'Blue'
})

// Computed properties
const listId = computed(() => parseInt(route.params.id as string))

const hasChanges = computed(() => {
  return form.title !== originalForm.value.title || 
         form.colour !== originalForm.value.colour
})

const completedItems = computed(() => {
  return todoList.value?.items?.filter((item: TodoItemDto) => item.done).length || 0
})

const remainingItems = computed(() => {
  return todoList.value?.items?.filter((item: TodoItemDto) => !item.done).length || 0
})

// Methods
const loadTodoList = async () => {
  try {
    error.value = null
    const data = await getTodoListById(listId.value)
    
    if (data) {
      todoList.value = data as TodoListDto
      form.title = data.title
      form.colour = data.colour
      originalForm.value = {
        title: data.title,
        colour: data.colour
      }
    } else {
      error.value = 'Todo list not found'
    }
  } catch (err) {
    console.error('Error loading todo list:', err)
    error.value = 'Failed to load todo list'
  }
}

const handleUpdateTodoList = async () => {
  if (!hasChanges.value) return

  try {
    updating.value = true
    
    const result = await updateTodoList({
      id: listId.value,
      title: form.title,
      colour: form.colour
    })

    if (result) {
      originalForm.value = {
        title: form.title,
        colour: form.colour
      }
      showSuccess('Todo list updated successfully!')
      router.push(Routes.TodoLists.path)
    }
  } catch (err) {
    console.error('Error updating todo list:', err)
    showError('Failed to update todo list')
  } finally {
    updating.value = false
  }
}

const handleDeleteTodoList = () => {
  showDeleteDialog.value = true
}

const confirmDelete = async () => {
  try {
    deleting.value = true
    
    const result = await deleteTodoList(listId.value)
    
    if (result) {
      showSuccess('Todo list deleted successfully!')
      router.push(Routes.TodoLists.path)
    }
  } catch (err) {
    console.error('Error deleting todo list:', err)
    showError('Failed to delete todo list')
  } finally {
    deleting.value = false
    showDeleteDialog.value = false
  }
}

// Initialize
onMounted(async () => {
  if (listId.value && !isNaN(listId.value)) {
    await loadTodoList()
  } else {
    error.value = 'Invalid todo list ID'
  }
})
</script>

<style scoped>
/* Gaming Container */
.edit-todo-list-container {
  min-height: 100vh;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
  padding: 2rem;
}

/* Header Section */
.header-section {
  margin-bottom: 2rem;
}

.gaming-glow {
  text-shadow: 0 0 10px #e75e8d, 0 0 20px #e75e8d, 0 0 30px #e75e8d;
}

/* Gaming Card */
.gaming-card {
  background: rgba(255, 255, 255, 0.05);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(231, 94, 141, 0.3);
  border-radius: 20px;
  padding: 2.5rem;
  box-shadow: 
    0 8px 32px rgba(0, 0, 0, 0.3),
    inset 0 1px 0 rgba(255, 255, 255, 0.1);
  position: relative;
  overflow: hidden;
  max-width: 800px;
  margin: 0 auto;
}

.gaming-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background: linear-gradient(90deg, transparent, rgba(231, 94, 141, 0.1), transparent);
  transition: left 0.5s;
}

.gaming-card:hover::before {
  left: 100%;
}

/* Form Groups */
.form-group {
  margin-bottom: 2rem;
}

.gaming-label {
  display: block;
  color: #e75e8d;
  font-weight: 600;
  font-size: 1rem;
  margin-bottom: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 1px;
}

/* Input Styles */
.input-wrapper, .select-wrapper {
  position: relative;
}

.gaming-input, .gaming-select {
  width: 100%;
  padding: 1rem 1.25rem;
  background: rgba(0, 0, 0, 0.3);
  border: 2px solid rgba(231, 94, 141, 0.3);
  border-radius: 12px;
  color: white;
  font-size: 1rem;
  transition: all 0.3s ease;
  backdrop-filter: blur(10px);
}

.gaming-input::placeholder {
  color: rgba(255, 255, 255, 0.5);
}

.gaming-input:focus, .gaming-select:focus {
  outline: none;
  border-color: #e75e8d;
  box-shadow: 0 0 20px rgba(231, 94, 141, 0.3);
  transform: translateY(-2px);
}

.gaming-select {
  cursor: pointer;
}

.gaming-select option {
  background: #1a1a2e;
  color: white;
}

/* Input Glow Effects */
.input-glow, .select-glow {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  border-radius: 12px;
  background: linear-gradient(45deg, #e75e8d, #764ba2);
  opacity: 0;
  z-index: -1;
  transition: opacity 0.3s ease;
  filter: blur(20px);
}

.gaming-input:focus + .input-glow,
.gaming-select:focus + .select-glow {
  opacity: 0.3;
}

/* Info Section */
.info-section {
  padding: 2rem 0;
  border-top: 1px solid rgba(231, 94, 141, 0.2);
  border-bottom: 1px solid rgba(231, 94, 141, 0.2);
  margin: 2rem 0;
}

.stat-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  background: rgba(0, 0, 0, 0.2);
  border: 1px solid rgba(231, 94, 141, 0.2);
  border-radius: 12px;
  padding: 1.5rem;
  backdrop-filter: blur(10px);
  transition: all 0.3s ease;
}

.stat-card:hover {
  border-color: rgba(231, 94, 141, 0.4);
  transform: translateY(-2px);
}

.stat-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 3rem;
  height: 3rem;
  background: linear-gradient(135deg, #e75e8d, #764ba2);
  border-radius: 50%;
  color: white;
}

.stat-label {
  color: #9ca3af;
  font-size: 0.875rem;
  margin-bottom: 0.25rem;
}

.stat-value {
  color: white;
  font-size: 1.5rem;
  font-weight: 700;
}

/* Gaming Buttons */
.gaming-btn-primary {
  background: linear-gradient(135deg, #e75e8d 0%, #764ba2 100%);
  color: white;
  padding: 0.875rem 2rem;
  border: none;
  border-radius: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 1px;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(231, 94, 141, 0.3);
  display: flex;
  align-items: center;
  justify-content: center;
  text-decoration: none;
}

.gaming-btn-primary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(231, 94, 141, 0.4);
}

.gaming-btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.gaming-btn-secondary {
  background: rgba(255, 255, 255, 0.1);
  color: white;
  padding: 0.875rem 2rem;
  border: 2px solid rgba(231, 94, 141, 0.3);
  border-radius: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 1px;
  cursor: pointer;
  transition: all 0.3s ease;
  text-decoration: none;
  display: flex;
  align-items: center;
  justify-content: center;
  backdrop-filter: blur(10px);
}

.gaming-btn-secondary:hover {
  background: rgba(231, 94, 141, 0.1);
  border-color: #e75e8d;
  transform: translateY(-2px);
  text-decoration: none;
  color: white;
}

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

/* Loading Spinner */
.loading-spinner {
  width: 3rem;
  height: 3rem;
  border: 4px solid rgba(231, 94, 141, 0.2);
  border-top: 4px solid #e75e8d;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

/* Dialog Overlay */
.dialog-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  backdrop-filter: blur(5px);
}

.dialog-content {
  max-width: 500px;
  margin: 1rem;
  animation: dialogSlideIn 0.3s ease;
}

@keyframes dialogSlideIn {
  from {
    opacity: 0;
    transform: scale(0.9) translateY(-20px);
  }
  to {
    opacity: 1;
    transform: scale(1) translateY(0);
  }
}

/* Animations */
@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

.animate-spin {
  animation: spin 1s linear infinite;
}

/* Responsive Design */
@media (max-width: 768px) {
  .edit-todo-list-container {
    padding: 1rem;
  }
  
  .gaming-card {
    padding: 1.5rem;
  }
  
  .flex.flex-col.sm\\:flex-row {
    flex-direction: column;
  }
  
  .gaming-btn-primary,
  .gaming-btn-secondary,
  .gaming-btn-danger {
    width: 100%;
    justify-content: center;
  }
}
</style>