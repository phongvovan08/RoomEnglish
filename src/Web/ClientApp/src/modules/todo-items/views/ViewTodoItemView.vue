<template>
  <div class="cyborg-container">
    <div class="cyborg-card">
      <div class="card-header">
        <h1 class="cyborg-title">
          <Icon icon="mdi:eye" />
          Todo Item Details
        </h1>
        <div class="header-actions">
          <router-link 
            :to="{ name: Routes.TodoItems.children.Edit.name, params: { id: $route.params.id } }"
            class="cyborg-btn cyborg-btn-gradient"
            v-if="todoItem"
          >
            <Icon icon="mdi:pencil" />
            Edit
          </router-link>
          <router-link 
            :to="{ name: Routes.TodoItems.name }"
            class="cyborg-btn cyborg-btn-secondary"
          >
            <Icon icon="mdi:arrow-left" />
            Back to List
          </router-link>
        </div>
      </div>
      
      <div v-if="loading" class="loading-state">
        <Icon icon="mdi:loading" class="spinner" />
        Loading todo item...
      </div>
      
      <div v-else-if="todoItem" class="todo-details">
        <div class="detail-section">
          <div class="detail-header">
            <h2>{{ todoItem.title }}</h2>
            <div class="status-badge" :class="{ 'completed': todoItem.done }">
              <Icon :icon="todoItem.done ? 'mdi:check-circle' : 'mdi:circle-outline'" />
              {{ todoItem.done ? 'Completed' : 'Pending' }}
            </div>
          </div>
          
          <p v-if="todoItem.note" class="todo-note">{{ todoItem.note }}</p>
        </div>
        
        <div class="detail-grid">
          <div class="detail-card">
            <Icon icon="mdi:format-list-bulleted" />
            <div class="detail-content">
              <label>Todo List</label>
              <span>{{ getListName(todoItem.listId) }}</span>
            </div>
          </div>
          
          <div class="detail-card">
            <Icon icon="mdi:flag" />
            <div class="detail-content">
              <label>Priority</label>
              <span class="priority" :class="`priority-${todoItem.priority}`">
                {{ getPriorityLabel(todoItem.priority) }}
              </span>
            </div>
          </div>
          
          <div v-if="todoItem.reminder" class="detail-card">
            <Icon icon="mdi:clock-outline" />
            <div class="detail-content">
              <label>Reminder</label>
              <span>{{ formatDate(todoItem.reminder) }}</span>
            </div>
          </div>
          
          <div class="detail-card">
            <Icon icon="mdi:calendar-plus" />
            <div class="detail-content">
              <label>Created</label>
              <span>{{ formatDate(todoItem.created) }}</span>
            </div>
          </div>
          
          <div v-if="todoItem.createdBy" class="detail-card">
            <Icon icon="mdi:account" />
            <div class="detail-content">
              <label>Created By</label>
              <span>{{ todoItem.createdBy }}</span>
            </div>
          </div>
          
          <div v-if="todoItem.lastModified" class="detail-card">
            <Icon icon="mdi:calendar-edit" />
            <div class="detail-content">
              <label>Last Modified</label>
              <span>{{ formatDate(todoItem.lastModified) }}</span>
            </div>
          </div>
        </div>
        
        <div class="action-section">
          <button
            @click="toggleComplete"
            class="cyborg-btn"
            :class="todoItem.done ? 'cyborg-btn-secondary' : 'cyborg-btn-gradient'"
          >
            <Icon :icon="todoItem.done ? 'mdi:undo' : 'mdi:check'" />
            {{ todoItem.done ? 'Mark as Pending' : 'Mark as Completed' }}
          </button>
          
          <button
            @click="deleteTodoItem"
            class="cyborg-btn cyborg-btn-danger"
          >
            <Icon icon="mdi:delete" />
            Delete Todo Item
          </button>
        </div>
      </div>
      
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
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { Icon } from '@iconify/vue'
import { Routes } from '@/router/constants'
import { TodoItemsService, TodoListsService } from '@/services/api'
import { useNotifications } from '@/utils/notifications'
import type { TodoItemDto, TodoListBriefDto, PriorityLevel } from '@/types/api'

const router = useRouter()
const route = useRoute()
const { showError } = useNotifications()

const loading = ref(true)

const todoItem = ref<TodoItemDto | null>(null)
const todoLists = ref<TodoListBriefDto[]>([])

const loadTodoItem = async () => {
  try {
    const itemId = Number(route.params.id)
    
    const response = await TodoItemsService.getById(itemId) as TodoItemDto
    todoItem.value = response
  } catch (error) {
    console.error('Failed to load todo item:', error)
    showError('Load Failed 🚫', 'Failed to load todo item details. Please try again.')
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

const toggleComplete = async () => {
  if (!todoItem.value) return
  
  try {
    const originalDone = todoItem.value.done
    todoItem.value.done = !todoItem.value.done
    
    await TodoItemsService.update(todoItem.value.id, {
      id: todoItem.value.id,
      title: todoItem.value.title,
      note: todoItem.value.note,
      priority: todoItem.value.priority,
      reminder: todoItem.value.reminder,
      done: todoItem.value.done
    })
    
    // Update timestamp locally
    todoItem.value.lastModified = new Date().toISOString()
  } catch (error) {
    console.error('Failed to update todo item:', error)
    // Revert the change on error
    if (todoItem.value) {
      todoItem.value.done = !todoItem.value.done
    }
  }
}

const deleteTodoItem = async () => {
  if (!todoItem.value) return
  
  if (confirm('Are you sure you want to delete this todo item? This action cannot be undone.')) {
    try {
      await TodoItemsService.delete(todoItem.value.id)
      router.push({ name: Routes.TodoItems.name })
    } catch (error) {
      console.error('Failed to delete todo item:', error)
      alert('Failed to delete todo item. Please try again.')
    }
  }
}

const getPriorityLabel = (priority: number) => {
  const labels = ['None', 'Low', 'Normal', 'High', 'Critical']
  return labels[priority] || 'Unknown'
}

const getListName = (listId: number) => {
  const list = todoLists.value.find(l => l.id === listId)
  return list?.title || `List ${listId}`
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
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

.header-actions {
  display: flex;
  gap: 1rem;
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

.detail-section {
  margin-bottom: 2rem;
}

.detail-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
  gap: 1rem;
}

.detail-header h2 {
  margin: 0;
  color: var(--text-primary);
  flex-grow: 1;
}

.status-badge {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  border-radius: 1rem;
  font-size: 0.875rem;
  font-weight: 500;
  background: rgba(107, 114, 128, 0.2);
  color: var(--text-secondary);
  white-space: nowrap;
}

.status-badge.completed {
  background: rgba(34, 197, 94, 0.2);
  color: #22c55e;
}

.todo-note {
  color: var(--text-secondary);
  line-height: 1.6;
  font-style: italic;
  padding: 1rem;
  background: var(--bg-secondary);
  border-radius: 0.5rem;
  border-left: 3px solid var(--accent-pink);
}

.detail-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1rem;
  margin-bottom: 2rem;
}

.detail-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  background: var(--bg-secondary);
  border: 1px solid var(--border-color);
  border-radius: 0.5rem;
  transition: all 0.3s ease;
}

.detail-card:hover {
  border-color: var(--accent-pink);
}

.detail-card .iconify {
  color: var(--accent-pink);
  font-size: 1.25rem;
  flex-shrink: 0;
}

.detail-content {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  min-width: 0;
}

.detail-content label {
  font-size: 0.75rem;
  color: var(--text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  font-weight: 500;
}

.detail-content span {
  color: var(--text-primary);
  font-weight: 500;
  word-break: break-word;
}

.priority {
  padding: 0.25rem 0.75rem;
  border-radius: 1rem;
  font-weight: 500;
  text-transform: uppercase;
  font-size: 0.75rem;
  text-align: center;
}

.priority-0 { background: rgba(107, 114, 128, 0.2); color: #6b7280; }
.priority-1 { background: rgba(34, 197, 94, 0.2); color: #22c55e; }
.priority-2 { background: rgba(251, 191, 36, 0.2); color: #fbbf24; }
.priority-3 { background: rgba(239, 68, 68, 0.2); color: #ef4444; }
.priority-4 { background: rgba(239, 68, 68, 0.3); color: #dc2626; }

.action-section {
  display: flex;
  gap: 1rem;
  padding-top: 1rem;
  border-top: 1px solid var(--border-color);
}

@media (max-width: 768px) {
  .card-header,
  .detail-header {
    flex-direction: column;
    gap: 1rem;
    text-align: center;
  }
  
  .header-actions,
  .action-section {
    flex-direction: column;
  }
  
  .detail-grid {
    grid-template-columns: 1fr;
  }
}
</style>