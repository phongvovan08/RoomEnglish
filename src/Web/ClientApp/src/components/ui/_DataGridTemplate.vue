<!--
  Template for creating new DataGrid wrapper components
  
  Instructions:
  1. Copy this file to create a new wrapper (e.g., UserDataGrid.vue, ProductDataGrid.vue)
  2. Replace all occurrences of "Entity" with your entity name (e.g., "User", "Product")
  3. Replace all occurrences of "entity" with lowercase entity name (e.g., "user", "product")
  4. Update the interface properties to match your entity
  5. Customize columns, actions, and slots as needed
  6. Implement proper API calls in the parent component
-->

<template>
  <DataGrid
    :data="entities"
    :columns="columns"
    :actions="actions"
    :pagination="true"
    :searchable="true"
    :clickable="true"
    :page-size="pageSize"
    :search-placeholder="'Tìm kiếm {entity display name}...'"
    :empty-state-title="'Chưa có {entity display name} nào'"
    :empty-state-message="'Tạo {entity display name} đầu tiên để bắt đầu'"
    :current-page="currentPage"
    :total-items="totalItems"
    :total-pages="totalPages"
    @row-click="handleRowClick"
    @action-click="handleActionClick"
    @search="handleSearch"
    @page-change="handlePageChange"
    @page-size-change="handlePageSizeChange"
  >
    <!-- Custom grid item for entity -->
    <template #grid-item="{ item }">
      <div class="entity-card" @click="$emit('entity-click', item)">
        <div class="entity-header">
          <div class="entity-info">
            <h3>{{ item.name }}</h3>
            <!-- Add more entity-specific info here -->
          </div>
          <div class="entity-actions">
            <button @click.stop="$emit('edit-entity', item)" class="action-btn edit">
              <Icon icon="mdi:pencil" class="w-4 h-4" />
            </button>
            <button @click.stop="$emit('delete-entity', item)" class="action-btn delete">
              <Icon icon="mdi:delete" class="w-4 h-4" />
            </button>
          </div>
        </div>
        
        <!-- Add more entity-specific content here -->
        <div class="entity-stats">
          <div class="stat">
            <Icon icon="mdi:calendar" class="w-4 h-4" />
            <span>{{ formatDate(item.createdAt) }}</span>
          </div>
        </div>
      </div>
    </template>

    <!-- Custom cells for table view -->
    <template #cell-createdAt="{ value }">
      <span class="date-cell">{{ formatDate(value) }}</span>
    </template>

    <!-- Add more custom cells as needed -->

    <!-- Custom empty state -->
    <template #empty-state>
      <div class="entity-empty-state">
        <Icon icon="mdi:database-outline" class="w-16 h-16 text-gray-400 mb-4" />
        <h3>Chưa có {entity display name} nào</h3>
        <p>Tạo {entity display name} đầu tiên để bắt đầu</p>
        <div class="empty-actions">
          <button @click="$emit('create-entity')" class="btn-primary">
            <Icon icon="mdi:plus" class="w-5 h-5 mr-2" />
            Tạo {entity display name} đầu tiên
          </button>
        </div>
      </div>
    </template>
  </DataGrid>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Icon } from '@iconify/vue'
import DataGrid, { type GridColumn, type GridAction } from './DataGrid.vue'

// TODO: Replace with your entity interface
interface Entity {
  id: number
  name: string
  // Add more properties as needed
  createdAt: string
}

interface Props {
  entities: Entity[]
  pageSize?: number
  // Server-side pagination props (required)
  currentPage: number
  totalItems: number
  totalPages: number
}

const props = withDefaults(defineProps<Props>(), {
  pageSize: 12
})

const emit = defineEmits<{
  'entity-click': [entity: Entity]
  'edit-entity': [entity: Entity]
  'delete-entity': [entity: Entity]
  'create-entity': []
  'search': [query: string]
  'page-change': [page: number]
  'page-size-change': [pageSize: number]
}>()

// TODO: Define columns for your entity
const columns = computed<GridColumn[]>(() => [
  {
    key: 'name',
    label: 'Tên',
    sortable: true,
    type: 'text'
  },
  // Add more columns as needed
  {
    key: 'createdAt',
    label: 'Ngày tạo',
    sortable: true,
    type: 'date'
  }
])

// TODO: Define actions for your entity
const actions = computed<GridAction[]>(() => [
  {
    key: 'edit',
    icon: 'mdi:pencil',
    tooltip: 'Sửa {entity display name}',
    variant: 'primary'
  },
  {
    key: 'delete',
    icon: 'mdi:delete',
    tooltip: 'Xóa {entity display name}',
    variant: 'danger'
  }
])

// Event handlers
const handleRowClick = (entity: Entity) => {
  emit('entity-click', entity)
}

const handleActionClick = (action: string, entity: Entity) => {
  if (action === 'edit') {
    emit('edit-entity', entity)
  } else if (action === 'delete') {
    emit('delete-entity', entity)
  }
}

const handleSearch = (query: string) => {
  emit('search', query)
}

const handlePageChange = (page: number) => {
  emit('page-change', page)
}

const handlePageSizeChange = (pageSize: number) => {
  emit('page-size-change', pageSize)
}

// Utility functions
const formatDate = (date: string | Date): string => {
  if (!date) return ''
  const d = new Date(date)
  return d.toLocaleDateString('vi-VN')
}
</script>

<style scoped>
/* Entity card styles for grid view */
.entity-card {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 0.5rem;
  padding: 1rem;
  cursor: pointer;
  transition: all 0.2s;
}

.entity-card:hover {
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  border-color: #93c5fd;
}

.entity-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 0.75rem;
}

.entity-info h3 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #111827;
  margin: 0 0 0.25rem 0;
}

.entity-actions {
  display: flex;
  gap: 0.25rem;
}

.action-btn {
  padding: 0.25rem;
  border-radius: 0.25rem;
  border: none;
  background: none;
  cursor: pointer;
  transition: all 0.2s;
  color: #6b7280;
}

.action-btn.edit:hover {
  background-color: #eff6ff;
  color: #2563eb;
}

.action-btn.delete:hover {
  background-color: #fef2f2;
  color: #dc2626;
}

.entity-stats {
  display: flex;
  gap: 1rem;
}

.stat {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  font-size: 0.75rem;
  color: #6b7280;
}

/* Table cell styles */
.date-cell {
  color: #6b7280;
  font-size: 0.875rem;
}

/* Empty state styles */
.entity-empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 0;
  color: #6b7280;
}

.entity-empty-state h3 {
  font-size: 1.125rem;
  font-weight: 500;
  color: #111827;
  margin: 0 0 0.25rem 0;
}

.entity-empty-state p {
  margin: 0 0 1.5rem 0;
  text-align: center;
}

.empty-actions {
  display: flex;
  gap: 0.5rem;
}

.btn-primary {
  display: flex;
  align-items: center;
  padding: 0.5rem 1rem;
  background-color: #2563eb;
  color: white;
  border: none;
  border-radius: 0.375rem;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-primary:hover {
  background-color: #1d4ed8;
}
</style>