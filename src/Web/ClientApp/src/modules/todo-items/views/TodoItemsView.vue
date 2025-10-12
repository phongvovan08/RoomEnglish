<template>
  <div class="cyborg-container">
    <div class="cyborg-card">
      <div class="card-header">
        <h1 class="cyborg-title">
          <Icon icon="mdi:format-list-checkbox" />
          {{ $t('menu.todoItems') }}
        </h1>
        <router-link 
          :to="{ name: Routes.TodoItems.children.Create.name }"
          class="cyborg-btn cyborg-btn-gradient"
        >
          <Icon icon="mdi:plus" />
          {{ $t('menu.create') }}
        </router-link>
      </div>
      
      <div class="todo-items-list">
        <div v-if="loading" class="loading-state">
          <Icon icon="mdi:loading" class="spinner" />
          {{ $t('common.loading') }}
        </div>
        
        <div v-else-if="todoItems.length === 0" class="empty-state">
          <Icon icon="mdi:clipboard-text-off-outline" size="64" />
          <p>No todo items found</p>
          <router-link 
            :to="{ name: Routes.TodoItems.children.Create.name }"
            class="cyborg-btn cyborg-btn-gradient"
          >
            Create your first todo item
          </router-link>
        </div>
        
        <div v-else class="items-grid">
          <div 
            v-for="item in todoItems" 
            :key="item.id"
            class="cyborg-card item-card"
          >
            <div class="item-header">
              <h3>{{ item.title }}</h3>
              <div class="item-actions">
                <button 
                  @click="toggleComplete(item)"
                  class="cyborg-btn cyborg-btn-sm"
                  :class="{ 'completed': item.done }"
                  :disabled="updating"
                >
                  <Icon :icon="item.done ? 'mdi:check-circle' : 'mdi:circle-outline'" />
                </button>
                <router-link 
                  :to="{ name: Routes.TodoItems.children.Edit.name, params: { id: item.id } }"
                  class="cyborg-btn cyborg-btn-sm cyborg-btn-secondary"
                >
                  <Icon icon="mdi:pencil" />
                </router-link>
                <button 
                  @click="deleteItem(item)"
                  class="cyborg-btn cyborg-btn-sm cyborg-btn-danger"
                  :disabled="updating"
                >
                  <Icon icon="mdi:delete" />
                </button>
              </div>
            </div>
            
            <p v-if="item.note" class="item-note">{{ item.note }}</p>
            
            <div class="item-meta">
              <span class="priority" :class="`priority-${item.priority}`">
                {{ getPriorityLabel(item.priority) }}
              </span>
              <span v-if="item.listId" class="list-name">
                List: {{ getListName(item.listId) }}
              </span>
            </div>
            
            <div class="item-dates">
              <small class="text-secondary">
                Created: {{ formatDate(item.created) }}
              </small>
              <small v-if="item.lastModified" class="text-secondary">
                Modified: {{ formatDate(item.lastModified) }}
              </small>
            </div>
          </div>
        </div>
        
        <!-- Pagination -->
        <div v-if="pagination.totalPages > 1" class="pagination">
          <button
            @click="goToPage(pagination.pageNumber - 1)"
            :disabled="!pagination.hasPreviousPage || loading"
            class="cyborg-btn cyborg-btn-secondary"
          >
            <Icon icon="mdi:chevron-left" />
            Previous
          </button>
          
          <span class="page-info">
            Page {{ pagination.pageNumber }} of {{ pagination.totalPages }}
            ({{ pagination.totalCount }} items total)
          </span>
          
          <button
            @click="goToPage(pagination.pageNumber + 1)"
            :disabled="!pagination.hasNextPage || loading"
            class="cyborg-btn cyborg-btn-secondary"
          >
            Next
            <Icon icon="mdi:chevron-right" />
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Icon } from '@iconify/vue'
import { Routes } from '@/router/constants'
import { TodoItemsService, TodoListsService } from '@/services/api'
import { useNotifications } from '@/composables/useNotifications'
import type { TodoItemDto, PaginatedList, TodoListBriefDto } from '@/types/api'

const { success, error: showError, warning } = useNotifications()

const loading = ref(true)
const updating = ref(false)
const todoItems = ref<TodoItemDto[]>([])
const todoLists = ref<TodoListBriefDto[]>([])
const pagination = ref({
  pageNumber: 1,
  totalPages: 0,
  totalCount: 0,
  hasNextPage: false,
  hasPreviousPage: false
})

const loadTodoItems = async () => {
  try {
    loading.value = true
    
    const response = await TodoItemsService.getWithPagination(
      undefined, // listId - get all items
      pagination.value.pageNumber,
      10 // pageSize
    ) as PaginatedList<TodoItemDto>
    
    todoItems.value = response.items
    pagination.value = {
      pageNumber: response.pageNumber,
      totalPages: response.totalPages,
      totalCount: response.totalCount,
      hasNextPage: response.hasNextPage,
      hasPreviousPage: response.hasPreviousPage
    }
  } catch (error) {
    console.error('Failed to load todo items:', error)
    showError('Load Failed 🚫', 'Failed to load todo items. Please try again.')
    todoItems.value = []
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

const toggleComplete = async (item: TodoItemDto) => {
  try {
    updating.value = true
    const originalDone = item.done
    item.done = !item.done
    
    await TodoItemsService.update(item.id, {
      id: item.id,
      title: item.title,
      note: item.note,
      priority: item.priority,
      reminder: item.reminder,
      done: item.done
    })
    
    if (item.done) {
      success('Task Completed! ✅', `"${item.title}" has been marked as completed`)
    } else {
      warning('Task Reopened 🔄', `"${item.title}" has been marked as incomplete`)
    }
  } catch (error) {
    console.error('Failed to toggle todo item completion:', error)
    showError('Update Failed ❌', 'Failed to update todo item status. Please try again.')
    // Revert on error
    item.done = !item.done
  } finally {
    updating.value = false
  }
}

const deleteItem = async (item: TodoItemDto) => {
  if (confirm('Are you sure you want to delete this todo item?')) {
    try {
      updating.value = true
      await TodoItemsService.delete(item.id)
      todoItems.value = todoItems.value.filter(i => i.id !== item.id)
      success('Item Deleted! 🗑️', `"${item.title}" has been successfully deleted`)
    } catch (error) {
      console.error('Failed to delete todo item:', error)
      showError('Delete Failed ❌', 'Failed to delete todo item. Please try again.')
    } finally {
      updating.value = false
    }
  }
}

const goToPage = async (pageNumber: number) => {
  pagination.value.pageNumber = pageNumber
  await loadTodoItems()
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
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

onMounted(async () => {
  await Promise.all([
    loadTodoItems(),
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
.empty-state {
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

.items-grid {
  display: grid;
  gap: 1.5rem;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  margin-bottom: 2rem;
}

.item-card {
  padding: 1.5rem;
  transition: all 0.3s ease;
}

.item-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(231, 94, 141, 0.15);
}

.item-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.item-header h3 {
  margin: 0;
  color: var(--text-primary);
  flex-grow: 1;
  margin-right: 1rem;
}

.item-actions {
  display: flex;
  gap: 0.5rem;
}

.item-note {
  color: var(--text-secondary);
  margin-bottom: 1rem;
  font-style: italic;
}

.item-meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.875rem;
  margin-bottom: 0.5rem;
}

.item-dates {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.text-secondary {
  color: var(--text-secondary);
}

.priority {
  padding: 0.25rem 0.75rem;
  border-radius: 1rem;
  font-weight: 500;
  text-transform: uppercase;
  font-size: 0.75rem;
}

.priority-0 { background: var(--bg-secondary); color: var(--text-secondary); }
.priority-1 { background: rgba(34, 197, 94, 0.2); color: #22c55e; }
.priority-2 { background: rgba(251, 191, 36, 0.2); color: #fbbf24; }
.priority-3 { background: rgba(239, 68, 68, 0.2); color: #ef4444; }
.priority-4 { background: rgba(239, 68, 68, 0.3); color: #dc2626; }

.list-name {
  color: var(--text-secondary);
}

.cyborg-btn.completed {
  background: var(--success-color);
  color: white;
}

.pagination {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
  border-top: 1px solid var(--border-color);
}

.page-info {
  color: var(--text-secondary);
  font-size: 0.875rem;
}

@media (max-width: 768px) {
  .pagination {
    flex-direction: column;
    gap: 1rem;
    text-align: center;
  }
}
</style>