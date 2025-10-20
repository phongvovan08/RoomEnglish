<template>
  <DataGrid
    :data="vocabularies"
    :columns="columns"
    :actions="actions"
    :pagination="true"
    :searchable="true"
    :clickable="false"
    :server-side="true"
    :loading="loading"
    :page-size="pageSize"
    :search-placeholder="'Tìm kiếm từ vựng...'"
    :empty-state-title="'Chưa có từ vựng nào'"
    :empty-state-message="'Thêm từ vựng đầu tiên cho danh mục này'"
    :current-page="currentPage"
    :total-items="totalItems"
    :total-pages="totalPages"
    :selectable="selectable"
    :selected-items="selectedItems"
    :default-view-mode="defaultViewMode"
    @row-click="handleRowClick"
    @action-click="handleActionClick"
    @search="handleSearch"
    @page-change="handlePageChange"
    @page-size-change="handlePageSizeChange"
    @sort-change="handleSortChange"
    @selection-change="handleSelectionChange"
  >
    <!-- Custom grid item for vocabulary -->
    <template #grid-item="{ item }">
      <div class="vocabulary-card" >
        <div class="vocabulary-header">
          <div class="word-info">
            <div class="word-title">
              <input 
                v-if="selectable"
                type="checkbox" 
                :checked="selectedItems.some(selected => selected.id === item.id)"
                @change.stop="toggleSelection(item)"
                class="select-checkbox"
              />
              <h3>{{ item.word }}</h3>
            </div>
            <span v-if="item.pronunciation" class="pronunciation">{{ item.pronunciation }}</span>
          </div>
          <div class="vocabulary-actions">
            <button @click.stop="$emit('vocabulary-click', item)" class="action-btn detail">
              <Icon icon="mdi:eye" class="w-4 h-4" />
            </button>
            <button @click.stop="$emit('edit-vocabulary', item)" class="action-btn edit">
              <Icon icon="mdi:pencil" class="w-4 h-4" />
            </button>
            <button @click.stop="$emit('delete-vocabulary', item)" class="action-btn delete">
              <Icon icon="mdi:delete" class="w-4 h-4" />
            </button>
          </div>
        </div>
        
        <div class="vocabulary-content">
          <p class="definition">{{ item.definition }}</p>
          
          <div class="vocabulary-stats">
            <div class="stat">
              <Icon icon="mdi:format-quote-close" class="w-4 h-4" />
              <span>{{ item.exampleCount || 0 }} ví dụ</span>
            </div>
            <div class="stat">
              <Icon icon="mdi:calendar" class="w-4 h-4" />
              <span>{{ formatDate(item.createdAt) }}</span>
            </div>
          </div>
        </div>

        <div class="vocabulary-footer">
          <span class="view-examples">Xem ví dụ →</span>
        </div>
      </div>
    </template>

    <!-- Custom cell for pronunciation in table view -->
    <template #cell-pronunciation="{ value }">
      <span class="pronunciation-cell">{{ value || '—' }}</span>
    </template>

    <!-- Custom cell for example count -->
    <template #cell-exampleCount="{ value }">
      <div class="example-count">
        <Icon icon="mdi:format-quote-close" class="w-4 h-4" />
        <span>{{ value || 0 }}</span>
      </div>
    </template>

    <!-- Custom cell for created date -->
    <template #cell-createdAt="{ value }">
      <span class="date-cell">{{ formatDate(value) }}</span>
    </template>

    <!-- Custom empty state -->
    <template #empty-state>
      <div class="vocabulary-empty-state">
        <Icon icon="mdi:book-outline" class="w-16 h-16 text-gray-400 mb-4" />
        <h3>Chưa có từ vựng nào</h3>
        <p>Thêm từ vựng đầu tiên cho danh mục này</p>
        <div class="empty-actions">
          <button @click="$emit('create-vocabulary')" class="btn-primary">
            <Icon icon="mdi:plus" class="w-5 h-5 mr-2" />
            Thêm từ đầu tiên
          </button>
          <button @click="$emit('upload-vocabulary')" class="btn-upload">
            <Icon icon="mdi:upload" class="w-5 h-5 mr-2" />
            Upload từ Excel
          </button>
        </div>
      </div>
    </template>
  </DataGrid>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Icon } from '@iconify/vue'
import DataGrid, { type GridColumn, type GridAction } from '@/components/ui/DataGrid.vue'

interface Vocabulary {
  id: number
  word: string
  definition: string
  pronunciation?: string
  exampleCount: number
  createdAt: string
}

interface Props {
  vocabularies: Vocabulary[]
  pageSize?: number
  loading?: boolean
  // Server-side pagination props
  currentPage?: number
  totalItems?: number
  totalPages?: number
  // Selection props
  selectable?: boolean
  selectedItems?: Vocabulary[]
  defaultViewMode?: 'table' | 'grid'
}

const props = withDefaults(defineProps<Props>(), {
  pageSize: 12,
  loading: false,
  currentPage: 1,
  totalItems: 0,
  totalPages: 1,
  selectable: false,
  selectedItems: () => [],
  defaultViewMode: 'grid'
})

const emit = defineEmits<{
  'vocabulary-click': [vocabulary: Vocabulary]
  'edit-vocabulary': [vocabulary: Vocabulary]
  'delete-vocabulary': [vocabulary: Vocabulary]
  'create-vocabulary': []
  'upload-vocabulary': []
  'search': [query: string]
  'page-change': [page: number]
  'page-size-change': [pageSize: number]
  'sort-change': [sortBy: string, sortOrder: 'asc' | 'desc']
  'selection-change': [selectedVocabularies: Vocabulary[]]
}>()

// Define columns for table view
const columns = computed<GridColumn[]>(() => [
  {
    key: 'word',
    label: 'Từ vựng',
    sortable: true,
    type: 'text'
  },
  {
    key: 'definition',
    label: 'Định nghĩa',
    sortable: false,
    type: 'text'
  },
  {
    key: 'pronunciation',
    label: 'Phát âm',
    sortable: false,
    type: 'text'
  },
  {
    key: 'exampleCount',
    label: 'Ví dụ',
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
    tooltip: 'Sửa từ vựng',
    variant: 'primary'
  },
  {
    key: 'delete',
    icon: 'mdi:delete',
    tooltip: 'Xóa từ vựng',
    variant: 'danger'
  }
])

// Event handlers
const handleRowClick = (vocabulary: Vocabulary) => {
  emit('vocabulary-click', vocabulary)
}

const handleActionClick = (action: string, vocabulary: Vocabulary) => {
  if (action === 'detail') {
    emit('vocabulary-click', vocabulary)
  } else if (action === 'edit') {
    emit('edit-vocabulary', vocabulary)
  } else if (action === 'delete') {
    emit('delete-vocabulary', vocabulary)
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

const handleSelectionChange = (selectedItems: Vocabulary[]) => {
  emit('selection-change', selectedItems)
}

const toggleSelection = (vocabulary: Vocabulary) => {
  const currentlySelected = [...props.selectedItems]
  const index = currentlySelected.findIndex(item => item.id === vocabulary.id)
  
  if (index > -1) {
    currentlySelected.splice(index, 1)
  } else {
    currentlySelected.push(vocabulary)
  }
  
  emit('selection-change', currentlySelected)
}

// Utility functions
const formatDate = (date: string | Date): string => {
  if (!date) return ''
  const d = new Date(date)
  return d.toLocaleDateString('vi-VN')
}
</script>

<style scoped>
/* Vocabulary card styles for grid view */
.vocabulary-card {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 0.5rem;
  padding: 1rem;
  cursor: pointer;
  transition: all 0.2s;
}

.vocabulary-card:hover {
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  border-color: #93c5fd;
}

.vocabulary-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 0.75rem;
}

.word-title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.25rem;
}

.word-info h3 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #111827;
  margin: 0;
}

.pronunciation {
  font-size: 0.875rem;
  color: #6b7280;
  font-style: italic;
}

.vocabulary-actions {
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

.vocabulary-content {
  margin-bottom: 0.75rem;
}

.definition {
  color: #374151;
  font-size: 0.875rem;
  line-height: 1.5;
  margin: 0 0 0.75rem 0;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.vocabulary-stats {
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

.vocabulary-footer {
  padding-top: 0.5rem;
  border-top: 1px solid #f3f4f6;
}

.view-examples {
  font-size: 0.875rem;
  color: #2563eb;
  font-weight: 500;
}

/* Table cell styles */
.pronunciation-cell {
  font-style: italic;
  color: #6b7280;
}

.example-count {
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
.vocabulary-empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 0;
  color: #6b7280;
}

.vocabulary-empty-state h3 {
  font-size: 1.125rem;
  font-weight: 500;
  color: #111827;
  margin: 0 0 0.25rem 0;
}

.vocabulary-empty-state p {
  margin: 0 0 1.5rem 0;
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

.btn-upload {
  display: flex;
  align-items: center;
  padding: 0.5rem 1rem;
  background-color: #10b981;
  color: white;
  border: none;
  border-radius: 0.375rem;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-upload:hover {
  background-color: #059669;
}
</style>