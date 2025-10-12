<template>
  <div class="cyborg-container">
    <div class="cyborg-card">
      <div class="card-header">
        <h1 class="cyborg-title">
          <Icon icon="mdi:pencil" />
          Edit Todo Item
        </h1>
        <router-link 
          :to="{ name: Routes.TodoItems.name }"
          class="cyborg-btn cyborg-btn-secondary"
        >
          <Icon icon="mdi:arrow-left" />
          Back to List
        </router-link>
      </div>
      
      <div v-if="loading" class="loading-state">
        <Icon icon="mdi:loading" class="spinner" />
        Loading todo item...
      </div>
      
      <form v-else-if="todoItem" @submit.prevent="updateTodoItem" class="todo-form">
        <div class="form-group">
          <label for="title" class="form-label">Title *</label>
          <input
            id="title"
            v-model="form.title"
            type="text"
            class="cyborg-input"
            placeholder="Enter todo item title"
            required
          />
        </div>
        
        <div class="form-group">
          <label for="note" class="form-label">Note</label>
          <textarea
            id="note"
            v-model="form.note"
            class="cyborg-input"
            rows="4"
            placeholder="Add optional details or notes"
          ></textarea>
        </div>
        
        <div class="form-row">
          <div class="form-group">
            <label for="listId" class="form-label">Todo List *</label>
            <select
              id="listId"
              v-model="form.listId"
              class="cyborg-input"
              required
            >
              <option value="">Select a list</option>
              <option 
                v-for="list in todoLists" 
                :key="list.id" 
                :value="list.id"
              >
                {{ list.title }}
              </option>
            </select>
          </div>
          
          <div class="form-group">
            <label for="priority" class="form-label">Priority</label>
            <select
              id="priority"
              v-model="form.priority"
              class="cyborg-input"
            >
              <option value="0">None</option>
              <option value="1">Low</option>
              <option value="2">Normal</option>
              <option value="3">High</option>
              <option value="4">Critical</option>
            </select>
          </div>
        </div>
        
        <div class="form-row">
          <div class="form-group">
            <label for="reminder" class="form-label">Reminder</label>
            <input
              id="reminder"
              v-model="form.reminder"
              type="datetime-local"
              class="cyborg-input"
            />
          </div>
          
          <div class="form-group">
            <label class="form-label">Status</label>
            <div class="checkbox-group">
              <label class="checkbox-label">
                <input
                  v-model="form.done"
                  type="checkbox"
                  class="cyborg-checkbox"
                />
                <span class="checkmark"></span>
                Mark as completed
              </label>
            </div>
          </div>
        </div>
        
        <div class="form-actions">
          <button
            type="button"
            @click="goBack"
            class="cyborg-btn cyborg-btn-secondary"
            :disabled="updating"
          >
            Cancel
          </button>
          <button
            type="submit"
            class="cyborg-btn cyborg-btn-gradient"
            :disabled="updating || !form.title || !form.listId"
          >
            <Icon v-if="updating" icon="mdi:loading" class="spinner" />
            <Icon v-else icon="mdi:check" />
            {{ updating ? 'Updating...' : 'Update Todo Item' }}
          </button>
        </div>
      </form>
      
      <div v-else class="error-state">
        <Icon icon="mdi:alert-circle" size="48" />
        <p>Todo item not found</p>
        <router-link 
          :to="{ name: Routes.TodoItems.name }"
          class="cyborg-btn cyborg-btn-gradient"
        >
          Back to List
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { Icon } from '@iconify/vue'
import { Routes } from '@/router/constants'
import { TodoItemsService, TodoListsService } from '@/services/api'
import type { TodoItemDto, TodoListBriefDto, PriorityLevel } from '@/types/api'

const router = useRouter()
const route = useRoute()
const loading = ref(true)
const updating = ref(false)

const todoItem = ref<TodoItemDto | null>(null)
const todoLists = ref<TodoListBriefDto[]>([])

const form = reactive({
  title: '',
  note: '',
  listId: '',
  priority: 2,
  reminder: '',
  done: false
})

const loadTodoItem = async () => {
  try {
    const itemId = Number(route.params.id)
    
    const response = await TodoItemsService.getById(itemId) as TodoItemDto
    todoItem.value = response
    
    // Populate form
    if (todoItem.value) {
      form.title = todoItem.value.title
      form.note = todoItem.value.note || ''
      form.listId = todoItem.value.listId.toString()
      form.priority = todoItem.value.priority
      form.done = todoItem.value.done
      
      if (todoItem.value.reminder) {
        const reminder = new Date(todoItem.value.reminder)
        form.reminder = reminder.toISOString().slice(0, 16)
      }
    }
  } catch (error) {
    console.error('Failed to load todo item:', error)
    todoItem.value = null
  } finally {
    loading.value = false
  }
}

const loadTodoLists = async () => {
  try {
    const response = await TodoListsService.getAll() as TodoListBriefDto[]
    todoLists.value = response
  } catch (error) {
    console.error('Failed to load todo lists:', error)
    todoLists.value = []
  }
}

const updateTodoItem = async () => {
  try {
    updating.value = true
    
    const command = {
      id: todoItem.value!.id,
      title: form.title,
      note: form.note || undefined,
      priority: Number(form.priority) as PriorityLevel,
      reminder: form.reminder || undefined,
      done: form.done
    }
    
    await TodoItemsService.update(todoItem.value!.id, command)
    
    // Navigate back to todo items list
    router.push({ name: Routes.TodoItems.name })
  } catch (error) {
    console.error('Failed to update todo item:', error)
    alert('Failed to update todo item. Please try again.')
  } finally {
    updating.value = false
  }
}

const goBack = () => {
  router.push({ name: Routes.TodoItems.name })
}

onMounted(async () => {
  await Promise.all([
    loadTodoItem(),
    loadTodoLists()
  ])
})
</script>

<style scoped>
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid var(--border-color);
}

.loading-state,
.error-state {
  text-align: center;
  padding: 4rem 2rem;
  color: var(--text-secondary);
}

.spinner {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.todo-form {
  max-width: 600px;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
}

.form-label {
  display: block;
  margin-bottom: 0.5rem;
  color: var(--text-primary);
  font-weight: 500;
}

.cyborg-input {
  width: 100%;
  padding: 0.875rem 1rem;
  background: var(--bg-secondary);
  border: 1px solid var(--border-color);
  border-radius: 0.5rem;
  color: var(--text-primary);
  transition: all 0.3s ease;
}

.cyborg-input:focus {
  outline: none;
  border-color: var(--accent-pink);
  box-shadow: 0 0 0 3px rgba(231, 94, 141, 0.1);
}

.cyborg-input::placeholder {
  color: var(--text-secondary);
}

textarea.cyborg-input {
  resize: vertical;
  min-height: 100px;
}

select.cyborg-input {
  cursor: pointer;
}

.checkbox-group {
  padding-top: 0.5rem;
}

.checkbox-label {
  display: flex;
  align-items: center;
  cursor: pointer;
  color: var(--text-primary);
}

.cyborg-checkbox {
  display: none;
}

.checkmark {
  width: 20px;
  height: 20px;
  border: 2px solid var(--border-color);
  border-radius: 4px;
  margin-right: 0.75rem;
  position: relative;
  transition: all 0.3s ease;
}

.cyborg-checkbox:checked + .checkmark {
  background: var(--accent-pink);
  border-color: var(--accent-pink);
}

.cyborg-checkbox:checked + .checkmark::after {
  content: '';
  position: absolute;
  left: 5px;
  top: 1px;
  width: 6px;
  height: 10px;
  border: solid white;
  border-width: 0 2px 2px 0;
  transform: rotate(45deg);
}

.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 2rem;
  padding-top: 1rem;
  border-top: 1px solid var(--border-color);
}

@media (max-width: 768px) {
  .form-row {
    grid-template-columns: 1fr;
  }
  
  .form-actions {
    flex-direction: column-reverse;
  }
  
  .card-header {
    flex-direction: column;
    gap: 1rem;
    text-align: center;
  }
}
</style>