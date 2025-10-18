<template>
  <DataGrid
    :data="examples"
    :columns="columns"
    :actions="actions"
    :pagination="true"
    :searchable="true"
    :clickable="false"
    :server-side="true"
    :loading="loading"
    :page-size="pageSize"
    :search-placeholder="'Tìm kiếm ví dụ...'"
    :empty-state-title="'Chưa có ví dụ nào'"
    :empty-state-message="'Thêm ví dụ đầu tiên cho từ vựng này'"
    :current-page="currentPage"
    :total-items="totalItems"
    :total-pages="totalPages"
    @row-click="handleRowClick"
    @action-click="handleActionClick"
    @search="handleSearch"
    @page-change="handlePageChange"
    @page-size-change="handlePageSizeChange"
    @sort-change="handleSortChange"
  >
    <!-- Custom grid item for example -->
    <template #grid-item="{ item }">
      <div class="example-card">
        <div class="example-header">
          <div class="example-info">
            <h3>{{ item.sentence }}</h3>
            <span v-if="item.grammar" class="grammar-point">{{ item.grammar }}</span>
          </div>
          <div class="example-actions">
            <button @click.stop="$emit('example-click', item)" class="action-btn detail">
              <Icon icon="mdi:eye" class="w-4 h-4" />
            </button>
            <button @click.stop="$emit('edit-example', item)" class="action-btn edit">
              <Icon icon="mdi:pencil" class="w-4 h-4" />
            </button>
            <button @click.stop="$emit('delete-example', item)" class="action-btn delete">
              <Icon icon="mdi:delete" class="w-4 h-4" />
            </button>
          </div>
        </div>
        
        <div class="example-content">
          <p class="translation">{{ item.translation }}</p>
          
          <div class="example-stats">
            <div class="stat">
              <Icon icon="mdi:volume-high" class="w-4 h-4" />
              <span>{{ item.audioUrl ? 'Có âm thanh' : 'Không có âm thanh' }}</span>
            </div>
            <div class="stat">
              <Icon icon="mdi:star" class="w-4 h-4" />
              <span>Level {{ item.difficultyLevel || 1 }}</span>
            </div>
            <div class="stat">
              <Icon icon="mdi:calendar" class="w-4 h-4" />
              <span>{{ formatDate(item.createdAt) }}</span>
            </div>
          </div>
        </div>

        <div class="example-footer">
          <span class="view-details">Xem chi tiết →</span>
        </div>
      </div>
    </template>

    <!-- Custom cell for sentence in table view -->
    <template #cell-sentence="{ value }">
      <div class="sentence-cell">
        <span class="sentence-text">{{ value }}</span>
      </div>
    </template>

    <!-- Custom cell for translation -->
    <template #cell-translation="{ value }">
      <span class="translation-cell">{{ value }}</span>
    </template>

    <!-- Custom cell for grammar point -->
    <template #cell-grammar="{ value }">
      <span class="grammar-cell">{{ value || '—' }}</span>
    </template>

    <!-- Custom cell for audio -->
    <template #cell-audioUrl="{ value }">
      <div class="audio-cell">
        <Icon v-if="value" icon="mdi:volume-high" class="w-4 h-4 text-green-500" />
        <Icon v-else icon="mdi:volume-off" class="w-4 h-4 text-gray-400" />
      </div>
    </template>

    <!-- Custom cell for difficulty level -->
    <template #cell-difficultyLevel="{ value }">
      <div class="difficulty-cell">
        <span class="level-badge" :class="`level-${value || 1}`">
          Level {{ value || 1 }}
        </span>
      </div>
    </template>

    <!-- Custom cell for created date -->
    <template #cell-createdAt="{ value }">
      <span class="date-cell">{{ formatDate(value) }}</span>
    </template>

    <!-- Custom empty state -->
    <template #empty-state>
      <div class="example-empty-state">
        <Icon icon="mdi:format-quote-outline" class="w-16 h-16 text-gray-400 mb-4" />
        <h3>Chưa có ví dụ nào</h3>
        <p>Thêm ví dụ đầu tiên cho từ vựng này</p>
        <div class="empty-actions">
          <button @click="$emit('create-example')" class="btn-primary">
            <Icon icon="mdi:plus" class="w-5 h-5 mr-2" />
            Thêm ví dụ đầu tiên
          </button>
          <button @click="$emit('upload-example')" class="btn-upload">
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

interface Example {
  id: number
  sentence: string
  translation: string
  grammar?: string
  audioUrl?: string
  difficultyLevel: number
  isActive: boolean
  displayOrder: number
  wordId: number
  createdAt: string
}

interface Props {
  examples: Example[]
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
  'example-click': [example: Example]
  'edit-example': [example: Example]
  'delete-example': [example: Example]
  'create-example': []
  'upload-example': []
  'search': [query: string]
  'page-change': [page: number]
  'page-size-change': [pageSize: number]
  'sort-change': [sortBy: string, sortOrder: 'asc' | 'desc']
}>()

// Define columns for table view
const columns = computed<GridColumn[]>(() => [
  {
    key: 'sentence',
    label: 'Câu ví dụ',
    sortable: true,
    type: 'text'
  },
  {
    key: 'translation',
    label: 'Dịch nghĩa',
    sortable: false,
    type: 'text'
  },
  {
    key: 'grammar',
    label: 'Ngữ pháp',
    sortable: false,
    type: 'text'
  },
  {
    key: 'audioUrl',
    label: 'Âm thanh',
    sortable: true,
    type: 'text'
  },
  {
    key: 'difficultyLevel',
    label: 'Độ khó',
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
    tooltip: 'Sửa ví dụ',
    variant: 'primary'
  },
  {
    key: 'delete',
    icon: 'mdi:delete',
    tooltip: 'Xóa ví dụ',
    variant: 'danger'
  }
])

// Event handlers
const handleRowClick = (example: Example) => {
  emit('example-click', example)
}

const handleActionClick = (action: string, example: Example) => {
  if (action === 'detail') {
    emit('example-click', example)
  } else if (action === 'edit') {
    emit('edit-example', example)
  } else if (action === 'delete') {
    emit('delete-example', example)
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
/* Example card styles for grid view */
.example-card {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 0.5rem;
  padding: 1rem;
  cursor: pointer;
  transition: all 0.2s;
}

.example-card:hover {
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  border-color: #93c5fd;
}

.example-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 0.75rem;
}

.example-info h3 {
  font-size: 1rem;
  font-weight: 600;
  color: #111827;
  margin: 0 0 0.25rem 0;
  line-height: 1.4;
}

.grammar-point {
  font-size: 0.75rem;
  color: #7c3aed;
  background-color: #f3f4f6;
  padding: 0.125rem 0.5rem;
  border-radius: 0.25rem;
  font-weight: 500;
}

.example-actions {
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

.example-content {
  margin-bottom: 0.75rem;
}

.translation {
  color: #374151;
  font-size: 0.875rem;
  line-height: 1.5;
  margin: 0 0 0.75rem 0;
  font-style: italic;
}

.example-stats {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
}

.stat {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  font-size: 0.75rem;
  color: #6b7280;
}

.example-footer {
  padding-top: 0.5rem;
  border-top: 1px solid #f3f4f6;
}

.view-details {
  font-size: 0.875rem;
  color: #2563eb;
  font-weight: 500;
}

/* Table cell styles */
.sentence-cell {
  max-width: 300px;
}

.sentence-text {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.translation-cell {
  font-style: italic;
  color: #6b7280;
  font-size: 0.875rem;
  max-width: 200px;
  display: -webkit-box;
  -webkit-line-clamp: 1;
  line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.grammar-cell {
  font-size: 0.75rem;
  color: #7c3aed;
  background-color: #f3f4f6;
  padding: 0.125rem 0.5rem;
  border-radius: 0.25rem;
  font-weight: 500;
}

.audio-cell {
  display: flex;
  align-items: center;
  justify-content: center;
}

.difficulty-cell {
  display: flex;
  align-items: center;
  justify-content: center;
}

.level-badge {
  font-size: 0.75rem;
  font-weight: 500;
  padding: 0.125rem 0.5rem;
  border-radius: 0.25rem;
}

.level-1 {
  background-color: #dcfce7;
  color: #166534;
}

.level-2 {
  background-color: #fef3c7;
  color: #92400e;
}

.level-3 {
  background-color: #fed7d7;
  color: #c53030;
}

.date-cell {
  color: #6b7280;
  font-size: 0.875rem;
}

/* Empty state styles */
.example-empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 0;
  color: #6b7280;
}

.example-empty-state h3 {
  font-size: 1.125rem;
  font-weight: 500;
  color: #111827;
  margin: 0 0 0.25rem 0;
}

.example-empty-state p {
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