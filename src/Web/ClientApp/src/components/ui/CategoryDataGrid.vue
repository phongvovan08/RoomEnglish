<template>
  <DataGrid
    :data="categories"
    :columns="columns"
    :actions="actions"
    :pagination="true"
    :searchable="true"
    :clickable="false"
    :server-side="true"
    :loading="loading"
    :page-size="pageSize"
    :search-placeholder="'Tìm kiếm danh mục...'"
    :empty-state-title="'Chưa có danh mục nào'"
    :empty-state-message="'Tạo danh mục đầu tiên để bắt đầu'"
    :current-page="currentPage"
    :total-items="totalItems"
    :total-pages="totalPages"
    @action-click="handleActionClick"
    @search="handleSearch"
    @page-change="handlePageChange"
    @page-size-change="handlePageSizeChange"
    @sort-change="handleSortChange"
  >
    <!-- Custom grid item for category -->
    <template #grid-item="{ item }">
      <div class="category-card">
        <div class="category-header">
          <div class="category-info">
            <h3>{{ item.name }}</h3>
            <span v-if="item.description" class="description">{{ item.description }}</span>
          </div>
          <div class="category-actions">
            <button @click.stop="$emit('detail-category', item)" class="action-btn detail">
              <Icon icon="mdi:eye" class="w-4 h-4" />
            </button>
            <button @click.stop="$emit('edit-category', item)" class="action-btn edit">
              <Icon icon="mdi:pencil" class="w-4 h-4" />
            </button>
            <button @click.stop="$emit('delete-category', item)" class="action-btn delete">
              <Icon icon="mdi:delete" class="w-4 h-4" />
            </button>
          </div>
        </div>
        
        <div class="category-content">
          <div class="category-stats">
            <div class="stat">
              <Icon icon="mdi:book-alphabet" class="w-4 h-4" />
              <span>{{ item.vocabularyCount || 0 }} từ vựng</span>
            </div>
            <div class="stat">
              <Icon icon="mdi:calendar" class="w-4 h-4" />
              <span>{{ formatDate(item.createdAt) }}</span>
            </div>
          </div>
        </div>
      </div>
    </template>

    <!-- Custom cell for vocabulary count -->
    <template #cell-vocabularyCount="{ value }">
      <div class="vocabulary-count">
        <Icon icon="mdi:book-alphabet" class="w-4 h-4" />
        <span>{{ value || 0 }}</span>
      </div>
    </template>

    <!-- Custom cell for created date -->
    <template #cell-createdAt="{ value }">
      <span class="date-cell">{{ formatDate(value) }}</span>
    </template>

    <!-- Custom empty state -->
    <template #empty-state>
      <div class="category-empty-state">
        <Icon icon="mdi:folder-outline" class="w-16 h-16 text-gray-400 mb-4" />
        <h3>Chưa có danh mục nào</h3>
        <p>Tạo danh mục đầu tiên để bắt đầu quản lý từ vựng</p>
        <div class="empty-actions">
          <button @click="$emit('create-category')" class="btn-primary">
            <Icon icon="mdi:plus" class="w-5 h-5 mr-2" />
            Tạo danh mục đầu tiên
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

interface Category {
  id: number
  name: string
  description?: string
  vocabularyCount: number
  createdAt: string
}

interface Props {
  categories: Category[]
  pageSize?: number
  loading?: boolean
  // Server-side pagination props
  currentPage?: number
  totalItems?: number
  totalPages?: number
}

const props = withDefaults(defineProps<Props>(), {
  pageSize: 12,
  loading: false,
  currentPage: 1,
  totalItems: 0,
  totalPages: 1
})

const emit = defineEmits<{
  'category-click': [category: Category]
  'detail-category': [category: Category]
  'edit-category': [category: Category]
  'delete-category': [category: Category]
  'create-category': []
  'search': [query: string]
  'page-change': [page: number]
  'page-size-change': [pageSize: number]
  'sort-change': [sortBy: string, sortOrder: 'asc' | 'desc']
}>()

// Define columns for table view
const columns = computed<GridColumn[]>(() => [
  {
    key: 'name',
    label: 'Tên danh mục',
    sortable: true,
    type: 'text'
  },
  {
    key: 'description',
    label: 'Mô tả',
    sortable: false,
    type: 'text'
  },
  {
    key: 'vocabularyCount',
    label: 'Từ vựng',
    sortable: true,
    type: 'number'
  },
  {
    key: 'createdAt',
    label: 'Ngày tạo',
    sortable: true,
    type: 'date'
  }
])

// Define actions for table view
const actions = computed<GridAction[]>(() => [
  {
    key: 'detail',
    icon: 'mdi:eye',
    tooltip: 'Xem chi tiết',
    variant: 'default'
  },
  {
    key: 'edit',
    icon: 'mdi:pencil',
    tooltip: 'Sửa danh mục',
    variant: 'primary'
  },
  {
    key: 'delete',
    icon: 'mdi:delete',
    tooltip: 'Xóa danh mục',
    variant: 'danger'
  }
])

// Event handlers
const handleActionClick = (action: string, category: Category) => {
  if (action === 'detail') {
    emit('detail-category', category)
  } else if (action === 'edit') {
    emit('edit-category', category)
  } else if (action === 'delete') {
    emit('delete-category', category)
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

const handleSortChange = (sortBy: string, sortOrder: 'asc' | 'desc') => {
  emit('sort-change', sortBy, sortOrder)
}

// Utility functions
const formatDate = (date: string | Date): string => {
  if (!date) return ''
  const d = new Date(date)
  return d.toLocaleDateString('vi-VN')
}
</script>

<style scoped>
/* Category card styles for grid view */
.category-card {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 0.5rem;
  padding: 1rem;
  transition: all 0.2s;
}

.category-card:hover {
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  border-color: #93c5fd;
}

.category-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 0.75rem;
}

.category-info h3 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #111827;
  margin: 0 0 0.25rem 0;
}

.description {
  font-size: 0.875rem;
  color: #6b7280;
  display: -webkit-box;
  -webkit-line-clamp: 1;
  line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.category-actions {
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

.action-btn.detail:hover {
  background-color: #f0f9ff;
  color: #0ea5e9;
}

.action-btn.edit:hover {
  background-color: #eff6ff;
  color: #2563eb;
}

.action-btn.delete:hover {
  background-color: #fef2f2;
  color: #dc2626;
}

.category-content {
  margin-bottom: 0.75rem;
}

.category-stats {
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
.vocabulary-count {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  font-size: 0.875rem;
}

.date-cell {
  color: #6b7280;
  font-size: 0.875rem;
}

/* Empty state styles */
.category-empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 0;
  color: #6b7280;
}

.category-empty-state h3 {
  font-size: 1.125rem;
  font-weight: 500;
  color: #111827;
  margin: 0 0 0.25rem 0;
}

.category-empty-state p {
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