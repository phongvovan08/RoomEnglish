<template>
  <div class="cyborg-container">
    <div class="cyborg-card">
      <div class="card-header">
        <h1 class="cyborg-title">
          <Icon icon="mdi:plus-circle" />
          Create Todo Item
        </h1>
        <router-link 
          :to="{ name: Routes.TodoItems.name }"
          class="cyborg-btn cyborg-btn-secondary"
        >
          <Icon icon="mdi:arrow-left" />
          Back to List
        </router-link>
      </div>
      
      <form @submit.prevent="createTodoItem" class="todo-form">
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
        
        <div class="form-group">
          <label for="reminder" class="form-label">Reminder</label>
          <input
            id="reminder"
            v-model="form.reminder"
            type="datetime-local"
            class="cyborg-input"
          />
        </div>
        
        <div class="form-actions">
          <button
            type="button"
            @click="goBack"
            class="cyborg-btn cyborg-btn-secondary"
            :disabled="creating || loadingLists"
          >
            Cancel
          </button>
          <button
            type="submit"
            class="cyborg-btn cyborg-btn-gradient"
            :disabled="creating || loadingLists || !form.title || !form.listId"
          >
            <Icon v-if="creating" icon="mdi:loading" class="spinner" />
            <Icon v-else icon="mdi:check" />
            {{ creating ? 'Creating...' : 'Create Todo Item' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Icon } from '@iconify/vue'
import { Routes } from '@/router/constants'
import { useTodoItems, useTodoLists } from '@/composables/useTodoApi'
import { useNotifications } from '@/utils/notifications'
import type { TodoListBriefDto, PriorityLevel } from '@/types/api'

const router = useRouter()
const { createTodoItem: createItem, loading: creating } = useTodoItems()
const { getTodoLists, loading: loadingLists } = useTodoLists()
const { showSuccess, showError } = useNotifications()

const loading = ref(false)
const todoLists = ref<TodoListBriefDto[]>([])

const form = reactive({
  title: '',
  note: '',
  listId: '',
  priority: 2,
  reminder: ''
})

const loadTodoLists = async () => {
  const lists = await getTodoLists()
  todoLists.value = lists
}

const createTodoItem = async () => {
  const command = {
    listId: Number(form.listId),
    title: form.title,
    note: form.note || undefined,
    priority: Number(form.priority) as PriorityLevel,
    reminder: form.reminder || undefined
  }
  
  const success_result = await createItem(command)
  
  if (success_result) {
    showSuccess('Todo Item Created', `"${form.title}" has been created successfully.`)
    router.push({ name: Routes.TodoItems.name })
  } else {
    showError('Creation Failed', 'Failed to create todo item. Please try again.')
  }
}

const goBack = () => {
  router.push({ name: Routes.TodoItems.name })
}

onMounted(() => {
  loadTodoLists()
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

.todo-form {
  max-width: 600px;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 200px;
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

.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 2rem;
  padding-top: 1rem;
  border-top: 1px solid var(--border-color);
}

.spinner {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
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