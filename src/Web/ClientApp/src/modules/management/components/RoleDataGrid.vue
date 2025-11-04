<template>
  <DataGrid
    :data="roles"
    :columns="columns"
    :actions="actions"
    :pagination="true"
    :searchable="true"
    :clickable="false"
    :server-side="true"
    :loading="loading"
    :page-size="pageSize"
    :search-placeholder="'Tìm kiếm vai trò...'"
    :empty-state-title="'Chưa có vai trò nào'"
    :empty-state-message="'Không tìm thấy vai trò'"
    :current-page="currentPage"
    :total-items="totalItems"
    :total-pages="totalPages"
    :default-view-mode="'table'"
    @action-click="handleActionClick"
    @search="handleSearch"
    @page-change="handlePageChange"
    @page-size-change="handlePageSizeChange"
    @sort-change="handleSortChange"
  >
    <!-- Custom grid item for role -->
    <template #grid-item="{ item }">
      <div class="role-card">
        <div class="role-header">
          <div class="role-info">
            <div class="role-icon">
              <Icon icon="mdi:shield-account" class="w-12 h-12 text-blue-500" />
            </div>
            <div class="role-details">
              <h3>{{ item.name }}</h3>
              <span class="role-description">{{ item.description }}</span>
            </div>
          </div>
          <div class="role-actions">
            <button @click.stop="$emit('edit-role', item)" class="action-btn edit">
              <Icon icon="mdi:pencil" class="w-4 h-4" />
            </button>
            <button @click.stop="$emit('delete-role', item)" class="action-btn delete">
              <Icon icon="mdi:delete" class="w-4 h-4" />
            </button>
          </div>
        </div>
        
        <div class="role-content">
          <div class="role-stats">
            <div class="stat-item">
              <Icon icon="mdi:account-multiple" class="w-4 h-4" />
              <span>{{ item.userCount }} người dùng</span>
            </div>
          </div>
        </div>
      </div>
    </template>

    <!-- Custom cell for name -->
    <template #cell-name="{ value }">
      <div class="name-cell">
        <Icon icon="mdi:shield-account" class="w-5 h-5 text-blue-500" />
        <span class="font-semibold">{{ value }}</span>
      </div>
    </template>

    <!-- Custom cell for description -->
    <template #cell-description="{ value }">
      <span class="description-text">{{ value }}</span>
    </template>

    <!-- Custom cell for user count -->
    <template #cell-userCount="{ value }">
      <div class="user-count-cell">
        <Icon icon="mdi:account-multiple" class="w-4 h-4 text-gray-400" />
        <span>{{ value }} người dùng</span>
      </div>
    </template>

    <!-- Custom empty state -->
    <template #empty-state>
      <div class="role-empty-state">
        <Icon icon="mdi:shield-account-outline" class="w-16 h-16 text-gray-400 mb-4" />
        <h3>Không tìm thấy vai trò</h3>
        <p>Thử tìm kiếm với từ khóa khác</p>
      </div>
    </template>
  </DataGrid>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Icon } from '@iconify/vue'
import DataGrid, { type GridColumn, type GridAction } from '@/components/ui/DataGrid.vue'
import type { RoleDetail } from '@/composables/useRoleManagementAPI'

interface Props {
  roles: RoleDetail[]
  pageSize?: number
  loading?: boolean
  currentPage?: number
  totalItems?: number
  totalPages?: number
}

const props = withDefaults(defineProps<Props>(), {
  pageSize: 10,
  loading: false,
  currentPage: 1,
  totalItems: 0,
  totalPages: 1
})

const emit = defineEmits<{
  'edit-role': [role: RoleDetail]
  'delete-role': [role: RoleDetail]
  'search': [query: string]
  'page-change': [page: number]
  'page-size-change': [pageSize: number]
  'sort-change': [sortBy: string, sortOrder: 'asc' | 'desc']
}>()

// Define columns for table view
const columns = computed<GridColumn[]>(() => [
  {
    key: 'name',
    label: 'Tên vai trò',
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
    key: 'userCount',
    label: 'Số người dùng',
    sortable: true,
    type: 'number'
  }
])

// Define actions for table view
const actions = computed<GridAction[]>(() => [
  {
    key: 'edit',
    icon: 'mdi:pencil',
    tooltip: 'Chỉnh sửa',
    variant: 'primary'
  },
  {
    key: 'delete',
    icon: 'mdi:delete',
    tooltip: 'Xóa',
    variant: 'danger'
  }
])

// Event handlers
const handleActionClick = (action: string, role: RoleDetail) => {
  if (action === 'edit') {
    emit('edit-role', role)
  } else if (action === 'delete') {
    emit('delete-role', role)
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
</script>

<style scoped>
/* Role card styles for grid view */
.role-card {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 0.5rem;
  padding: 1rem;
  transition: all 0.2s;
}

.role-card:hover {
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  border-color: #93c5fd;
}

.role-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 0.75rem;
}

.role-info {
  display: flex;
  gap: 0.75rem;
  align-items: flex-start;
  flex: 1;
}

.role-icon {
  flex-shrink: 0;
}

.role-details {
  flex: 1;
}

.role-details h3 {
  font-size: 1rem;
  font-weight: 600;
  color: #111827;
  margin: 0 0 0.25rem 0;
}

.role-description {
  font-size: 0.875rem;
  color: #6b7280;
  display: block;
}

.role-actions {
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

.role-content {
  margin-top: 0.75rem;
  padding-top: 0.75rem;
  border-top: 1px solid #f3f4f6;
}

.role-stats {
  display: flex;
  gap: 1rem;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #6b7280;
  font-size: 0.875rem;
}

/* Table cell styles */
.name-cell {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.description-text {
  color: #6b7280;
  font-size: 0.875rem;
}

.user-count-cell {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #6b7280;
}

/* Empty state styles */
.role-empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 0;
  color: #6b7280;
}

.role-empty-state h3 {
  font-size: 1.125rem;
  font-weight: 500;
  color: #111827;
  margin: 0 0 0.25rem 0;
}

.role-empty-state p {
  margin: 0;
  text-align: center;
}
</style>
