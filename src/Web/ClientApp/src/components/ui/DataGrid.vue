<template>
  <div class="data-grid">
    <!-- Grid Header -->
    <div class="grid-header">
      <div class="grid-controls">
        <div class="view-toggle">
            <button 
            @click="viewMode = 'table'" 
            :class="{ active: viewMode === 'table' }"
            class="view-btn"
          >
            <Icon icon="mdi:view-list" class="w-4 h-4" />
            Table
          </button>
          <button 
            @click="viewMode = 'grid'" 
            :class="{ active: viewMode === 'grid' }"
            class="view-btn"
          >
            <Icon icon="mdi:view-grid" class="w-4 h-4" />
            Grid
          </button>
          
        </div>
        
        <div class="page-size-selector" v-if="pagination">
          <label>Hiển thị:</label>
          <select v-model="currentPageSize" @change="handlePageSizeChange">
            <option v-for="size in pageSizeOptions" :key="size" :value="size">
              {{ size }}
            </option>
          </select>
        </div>
      </div>
      
      <div class="grid-search" v-if="searchable">
        <Icon icon="mdi:magnify" class="w-5 h-5" />
        <input 
          v-model="searchQuery" 
          type="text" 
          :placeholder="searchPlaceholder"
          class="search-input"
          @input="handleSearch"
        />
      </div>
    </div>

    <!-- Table View -->
    <div v-if="viewMode === 'table'" class="table-container">
      <table class="data-table">
        <thead>
          <tr>
            <th 
              v-for="column in columns" 
              :key="column.key"
              :class="{ sortable: column.sortable }"
              @click="column.sortable && handleSort(column.key)"
            >
              <div class="th-content">
                <span>{{ column.label }}</span>
                <Icon 
                  v-if="column.sortable" 
                  :icon="getSortIcon(column.key)" 
                  class="w-4 h-4 sort-icon"
                />
              </div>
            </th>
            <th v-if="actions && actions.length > 0" class="actions-column">Hành động</th>
          </tr>
        </thead>
        <tbody>
          <tr 
            v-for="(item, index) in paginatedData" 
            :key="getItemKey(item, index)"
            :class="{ clickable: clickable }"
            @click="clickable && handleRowClick(item)"
          >
            <td v-for="column in columns" :key="column.key">
              <slot 
                :name="`cell-${column.key}`" 
                :item="item" 
                :value="getNestedValue(item, column.key)"
                :column="column"
              >
                <span v-if="column.type === 'date'">
                  {{ formatDate(getNestedValue(item, column.key)) }}
                </span>
                <span v-else-if="column.type === 'number'">
                  {{ formatNumber(getNestedValue(item, column.key)) }}
                </span>
                <span v-else>
                  {{ getNestedValue(item, column.key) }}
                </span>
              </slot>
            </td>
            <td v-if="actions && actions.length > 0" class="actions-cell">
              <div class="action-buttons">
                <button
                  v-for="action in actions"
                  :key="action.key"
                  @click.stop="handleAction(action.key, item)"
                  :class="['action-btn', action.variant || 'default']"
                  :title="action.tooltip"
                >
                  <Icon :icon="action.icon" class="w-4 h-4" />
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Grid View -->
    <div v-if="viewMode === 'grid'" class="grid-container">
      <div 
        v-for="(item, index) in paginatedData" 
        :key="getItemKey(item, index)"
        :class="['grid-item', { clickable: clickable }]"
        @click="clickable && handleRowClick(item)"
      >
        <slot name="grid-item" :item="item" :index="index">
          <!-- Default grid item layout -->
          <div class="grid-item-header">
            <h3>{{ getNestedValue(item, columns[0]?.key) }}</h3>
            <div v-if="actions && actions.length > 0" class="grid-actions">
              <button
                v-for="action in actions"
                :key="action.key"
                @click.stop="handleAction(action.key, item)"
                :class="['action-btn', action.variant || 'default']"
                :title="action.tooltip"
              >
                <Icon :icon="action.icon" class="w-4 h-4" />
              </button>
            </div>
          </div>
          <div class="grid-item-content">
            <div 
              v-for="column in columns.slice(1)" 
              :key="column.key"
              class="grid-field"
            >
              <label>{{ column.label }}:</label>
              <span>{{ getNestedValue(item, column.key) }}</span>
            </div>
          </div>
        </slot>
      </div>
    </div>

    <!-- Empty State -->
    <div v-if="paginatedData.length === 0" class="empty-state">
      <slot name="empty-state">
        <Icon icon="mdi:database-outline" class="w-16 h-16 text-gray-400 mb-4" />
        <h3>{{ emptyStateTitle }}</h3>
        <p>{{ emptyStateMessage }}</p>
      </slot>
    </div>

    <!-- Pagination -->
    <div v-if="pagination && totalPages > 1" class="pagination">
      <div class="pagination-info">
        Hiển thị {{ startIndex + 1 }}-{{ endIndex }} của {{ totalItems }} mục
      </div>
      
      <div class="pagination-controls">
        <button 
          @click="goToPage(1)" 
          :disabled="currentPage === 1"
          class="pagination-btn"
        >
          <Icon icon="mdi:page-first" class="w-4 h-4" />
        </button>
        
        <button 
          @click="goToPage(currentPage - 1)" 
          :disabled="currentPage === 1"
          class="pagination-btn"
        >
          <Icon icon="mdi:chevron-left" class="w-4 h-4" />
        </button>
        
        <span class="pagination-current">
          {{ currentPage }} / {{ totalPages }}
        </span>
        
        <button 
          @click="goToPage(currentPage + 1)" 
          :disabled="currentPage === totalPages"
          class="pagination-btn"
        >
          <Icon icon="mdi:chevron-right" class="w-4 h-4" />
        </button>
        
        <button 
          @click="goToPage(totalPages)" 
          :disabled="currentPage === totalPages"
          class="pagination-btn"
        >
          <Icon icon="mdi:page-last" class="w-4 h-4" />
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { Icon } from '@iconify/vue'

export interface GridColumn {
  key: string
  label: string
  sortable?: boolean
  type?: 'text' | 'number' | 'date'
  width?: string
}

export interface GridAction {
  key: string
  icon: string
  tooltip: string
  variant?: 'default' | 'primary' | 'danger' | 'success'
}

interface Props {
  data: any[]
  columns: GridColumn[]
  actions?: GridAction[]
  pagination?: boolean
  searchable?: boolean
  clickable?: boolean
  pageSize?: number
  pageSizeOptions?: number[]
  searchPlaceholder?: string
  emptyStateTitle?: string
  emptyStateMessage?: string
  keyField?: string
}

const props = withDefaults(defineProps<Props>(), {
  pagination: true,
  searchable: true,
  clickable: false,
  pageSize: 10,
  pageSizeOptions: () => [5, 10, 20, 50],
  searchPlaceholder: 'Tìm kiếm...',
  emptyStateTitle: 'Không có dữ liệu',
  emptyStateMessage: 'Chưa có dữ liệu để hiển thị',
  keyField: 'id'
})

const emit = defineEmits<{
  'row-click': [item: any]
  'action-click': [action: string, item: any]
  'search': [query: string]
  'page-change': [page: number]
  'page-size-change': [pageSize: number]
  'sort-change': [sortBy: string, sortOrder: 'asc' | 'desc']
}>()

// Reactive state
const viewMode = ref<'grid' | 'table'>('table')
const searchQuery = ref('')
const currentPage = ref(1)
const currentPageSize = ref(props.pageSize)
const sortBy = ref<string>('')
const sortOrder = ref<'asc' | 'desc'>('asc')

// Computed properties
const filteredData = computed(() => {
  if (!searchQuery.value) return props.data
  
  const query = searchQuery.value.toLowerCase()
  return props.data.filter(item => {
    return props.columns.some(column => {
      const value = getNestedValue(item, column.key)
      return value && value.toString().toLowerCase().includes(query)
    })
  })
})

const sortedData = computed(() => {
  if (!sortBy.value) return filteredData.value
  
  return [...filteredData.value].sort((a, b) => {
    const aValue = getNestedValue(a, sortBy.value)
    const bValue = getNestedValue(b, sortBy.value)
    
    if (aValue === bValue) return 0
    
    let result = 0
    if (typeof aValue === 'number' && typeof bValue === 'number') {
      result = aValue - bValue
    } else {
      result = String(aValue).localeCompare(String(bValue))
    }
    
    return sortOrder.value === 'desc' ? -result : result
  })
})

const totalItems = computed(() => sortedData.value.length)
const totalPages = computed(() => Math.ceil(totalItems.value / currentPageSize.value))

const startIndex = computed(() => (currentPage.value - 1) * currentPageSize.value)
const endIndex = computed(() => Math.min(startIndex.value + currentPageSize.value, totalItems.value))

const paginatedData = computed(() => {
  if (!props.pagination) return sortedData.value
  return sortedData.value.slice(startIndex.value, startIndex.value + currentPageSize.value)
})

// Methods
const getNestedValue = (obj: any, path: string): any => {
  return path.split('.').reduce((current, key) => current?.[key], obj)
}

const getItemKey = (item: any, index: number): string | number => {
  return getNestedValue(item, props.keyField) || index
}

const handleSearch = () => {
  currentPage.value = 1
  emit('search', searchQuery.value)
}

const handleSort = (columnKey: string) => {
  if (sortBy.value === columnKey) {
    sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortBy.value = columnKey
    sortOrder.value = 'asc'
  }
  
  emit('sort-change', sortBy.value, sortOrder.value)
}

const getSortIcon = (columnKey: string): string => {
  if (sortBy.value !== columnKey) return 'mdi:sort'
  return sortOrder.value === 'asc' ? 'mdi:sort-ascending' : 'mdi:sort-descending'
}

const handleRowClick = (item: any) => {
  emit('row-click', item)
}

const handleAction = (actionKey: string, item: any) => {
  emit('action-click', actionKey, item)
}

const goToPage = (page: number) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page
    emit('page-change', page)
  }
}

const handlePageSizeChange = () => {
  currentPage.value = 1
  emit('page-size-change', currentPageSize.value)
}

const formatDate = (date: string | Date): string => {
  if (!date) return ''
  const d = new Date(date)
  return d.toLocaleDateString('vi-VN')
}

const formatNumber = (num: number): string => {
  if (typeof num !== 'number') return ''
  return num.toLocaleString('vi-VN')
}

// Watch for data changes
watch(() => props.data, () => {
  currentPage.value = 1
})
</script>

<style scoped>
.data-grid {
  background-color: white;
  border-radius: 0.5rem;
  box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1);
  border: 1px solid #e5e7eb;
}

.grid-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
  border-bottom: 1px solid #e5e7eb;
}

.grid-controls {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.view-toggle {
  display: flex;
  background-color: #f3f4f6;
  border-radius: 0.5rem;
  padding: 0.25rem;
}

.view-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 0.75rem;
  border-radius: 0.375rem;
  font-size: 0.875rem;
  font-weight: 500;
  transition: all 0.2s;
  border: none;
  background: none;
  cursor: pointer;
}

.view-btn:not(.active) {
  color: #4b5563;
}

.view-btn:not(.active):hover {
  color: #111827;
  background-color: #e5e7eb;
}

.view-btn.active {
  color: #2563eb;
  background-color: white;
  box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1);
}

.page-size-selector {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
  color: #111827;
}

.page-size-selector label {
  color: #374151;
}

.page-size-selector select {
  border: 1px solid #d1d5db;
  border-radius: 0.375rem;
  padding: 0.25rem 0.5rem;
  font-size: 0.875rem;
}

.grid-search {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background-color: #f9fafb;
  border-radius: 0.5rem;
  padding: 0.5rem 0.75rem;
  border: 1px solid #e5e7eb;
  color: #111827;
}

.search-input {
  background: transparent;
  border: none;
  outline: none;
  font-size: 0.875rem;
  width: 16rem;
}

.search-input::placeholder {
  color: #6b7280;
}

/* Table View Styles */
.table-container {
  overflow-x: auto;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
}

.data-table th {
  padding: 0.75rem 1rem;
  text-align: left;
  font-size: 0.75rem;
  font-weight: 500;
  color: #6b7280;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  border-bottom: 1px solid #e5e7eb;
}

.data-table th.sortable {
  cursor: pointer;
}

.data-table th.sortable:hover {
  background-color: #f9fafb;
}

.th-content {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.sort-icon {
  color: #9ca3af;
}

.data-table td {
  padding: 0.75rem 1rem;
  font-size: 0.875rem;
  color: #111827;
  border-bottom: 1px solid #f3f4f6;
}

.data-table tr.clickable {
  cursor: pointer;
}

.data-table tr.clickable:hover {
  background-color: #f9fafb;
}

.actions-column {
  width: 5rem;
}

.actions-cell {
  text-align: right;
}

.action-buttons {
  display: flex;
  justify-content: flex-end;
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

.action-btn:hover {
  background-color: #f3f4f6;
}

.action-btn.primary {
  color: #2563eb;
}

.action-btn.primary:hover {
  background-color: #eff6ff;
}

.action-btn.danger {
  color: #dc2626;
}

.action-btn.danger:hover {
  background-color: #fef2f2;
}

.action-btn.success {
  color: #16a34a;
}

.action-btn.success:hover {
  background-color: #f0fdf4;
}

/* Grid View Styles */
.grid-container {
  display: grid;
  grid-template-columns: repeat(1, 1fr);
  gap: 1rem;
  padding: 1rem;
}

@media (min-width: 768px) {
  .grid-container {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (min-width: 1024px) {
  .grid-container {
    grid-template-columns: repeat(3, 1fr);
  }
}

.grid-item {
  background-color: white;
  border: 1px solid #e5e7eb;
  border-radius: 0.5rem;
  padding: 1rem;
  transition: all 0.2s;
}

.grid-item:hover {
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
}

.grid-item.clickable {
  cursor: pointer;
}

.grid-item.clickable:hover {
  border-color: #93c5fd;
}

.grid-item-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 0.75rem;
}

.grid-item-header h3 {
  font-weight: 600;
  color: #111827;
  font-size: 1.125rem;
  margin: 0;
}

.grid-actions {
  display: flex;
  gap: 0.25rem;
}

.grid-item-content {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.grid-field {
  font-size: 0.875rem;
}

.grid-field label {
  font-weight: 500;
  color: #4b5563;
  margin-right: 0.5rem;
}

.grid-field span {
  color: #111827;
}

/* Empty State */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 0;
  color: #6b7280;
}

.empty-state h3 {
  font-size: 1.125rem;
  font-weight: 500;
  color: #111827;
  margin: 0 0 0.25rem 0;
}

.empty-state p {
  margin: 0;
}

/* Pagination */
.pagination {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem 1rem;
  border-top: 1px solid #e5e7eb;
}

.pagination-info {
  font-size: 0.875rem;
  color: #374151;
}

.pagination-controls {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.pagination-btn {
  padding: 0.5rem;
  border-radius: 0.25rem;
  border: 1px solid #d1d5db;
  background-color: white;
  cursor: pointer;
  transition: all 0.2s;
}

.pagination-btn:hover:not(:disabled) {
  background-color: #f9fafb;
}

.pagination-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.pagination-current {
  padding: 0.25rem 0.75rem;
  font-size: 0.875rem;
  font-weight: 500;
  color: #374151;
}
</style>