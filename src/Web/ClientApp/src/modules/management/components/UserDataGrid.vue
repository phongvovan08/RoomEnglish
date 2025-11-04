<template>
  <DataGrid
    :data="users"
    :columns="columns"
    :actions="actions"
    :pagination="true"
    :searchable="true"
    :clickable="false"
    :server-side="true"
    :loading="loading"
    :page-size="pageSize"
    :search-placeholder="'Tìm kiếm người dùng...'"
    :empty-state-title="'Chưa có người dùng nào'"
    :empty-state-message="'Không tìm thấy người dùng'"
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
    <!-- Custom grid item for user -->
    <template #grid-item="{ item }">
      <div class="user-card">
        <div class="user-header">
          <div class="user-info">
            <div class="user-avatar">
              <Icon icon="mdi:account-circle" class="w-12 h-12 text-blue-500" />
            </div>
            <div class="user-details">
              <h3>{{ item.email }}</h3>
              <span v-if="item.userName" class="username">@{{ item.userName }}</span>
            </div>
          </div>
          <div class="user-actions">
            <button @click.stop="$emit('edit-user', item)" class="action-btn edit">
              <Icon icon="mdi:account-edit" class="w-4 h-4" />
            </button>
            <button @click.stop="$emit('delete-user', item)" class="action-btn delete">
              <Icon icon="mdi:delete" class="w-4 h-4" />
            </button>
          </div>
        </div>
        
        <div class="user-content">
          <div class="user-roles">
            <span v-if="item.roles && item.roles.length > 0" v-for="role in item.roles" :key="role" class="role-badge">
              <Icon icon="mdi:shield-account" class="w-3 h-3" />
              {{ role }}
            </span>
            <span v-else class="role-badge no-role">
              <Icon icon="mdi:account" class="w-3 h-3" />
              Không có quyền
            </span>
          </div>
        </div>
      </div>
    </template>

    <!-- Custom cell for email -->
    <template #cell-email="{ value, item }">
      <div class="email-cell">
        <Icon icon="mdi:email" class="w-4 h-4 text-gray-400" />
        <span>{{ value }}</span>
        <span v-if="isCurrentUser(item.id)" class="current-user-badge">Bạn</span>
      </div>
    </template>

    <!-- Custom cell for roles -->
    <template #cell-roles="{ value }">
      <div class="roles-cell">
        <span v-if="value && value.length > 0" v-for="role in value" :key="role" class="role-badge-small">
          {{ role }}
        </span>
        <span v-else class="no-role-text">Không có quyền</span>
      </div>
    </template>

    <!-- Custom empty state -->
    <template #empty-state>
      <div class="user-empty-state">
        <Icon icon="mdi:account-multiple-outline" class="w-16 h-16 text-gray-400 mb-4" />
        <h3>Không tìm thấy người dùng</h3>
        <p>Thử tìm kiếm với từ khóa khác</p>
      </div>
    </template>
  </DataGrid>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Icon } from '@iconify/vue'
import DataGrid, { type GridColumn, type GridAction } from '@/components/ui/DataGrid.vue'
import type { User } from '@/composables/useUserManagementAPI'

interface Props {
  users: User[]
  pageSize?: number
  loading?: boolean
  currentPage?: number
  totalItems?: number
  totalPages?: number
  currentUserId?: string
}

const props = withDefaults(defineProps<Props>(), {
  pageSize: 10,
  loading: false,
  currentPage: 1,
  totalItems: 0,
  totalPages: 1
})

const emit = defineEmits<{
  'edit-user': [user: User]
  'delete-user': [user: User]
  'search': [query: string]
  'page-change': [page: number]
  'page-size-change': [pageSize: number]
  'sort-change': [sortBy: string, sortOrder: 'asc' | 'desc']
}>()

// Define columns for table view
const columns = computed<GridColumn[]>(() => [
  {
    key: 'email',
    label: 'Email',
    sortable: true,
    type: 'text'
  },
  {
    key: 'userName',
    label: 'Tên đăng nhập',
    sortable: true,
    type: 'text'
  },
  {
    key: 'roles',
    label: 'Quyền',
    sortable: false,
    type: 'text'
  }
])

// Define actions for table view
const actions = computed<GridAction[]>(() => [
  {
    key: 'edit',
    icon: 'mdi:account-edit',
    tooltip: 'Phân quyền',
    variant: 'primary'
  },
  {
    key: 'delete',
    icon: 'mdi:delete',
    tooltip: 'Xóa người dùng',
    variant: 'danger'
  }
])

// Event handlers
const handleActionClick = (action: string, user: User) => {
  if (action === 'edit') {
    emit('edit-user', user)
  } else if (action === 'delete') {
    emit('delete-user', user)
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
const isCurrentUser = (userId: string): boolean => {
  return props.currentUserId === userId
}
</script>

<style scoped>
/* User card styles for grid view */
.user-card {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 0.5rem;
  padding: 1rem;
  transition: all 0.2s;
}

.user-card:hover {
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  border-color: #93c5fd;
}

.user-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 0.75rem;
}

.user-info {
  display: flex;
  gap: 0.75rem;
  align-items: center;
}

.user-avatar {
  flex-shrink: 0;
}

.user-details h3 {
  font-size: 1rem;
  font-weight: 600;
  color: #111827;
  margin: 0 0 0.25rem 0;
}

.username {
  font-size: 0.875rem;
  color: #6b7280;
}

.user-actions {
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

.user-content {
  margin-top: 0.75rem;
}

.user-roles {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.role-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  padding: 0.25rem 0.75rem;
  background-color: #dbeafe;
  color: #1e40af;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
}

.role-badge.no-role {
  background-color: #f3f4f6;
  color: #6b7280;
}

/* Table cell styles */
.email-cell {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.current-user-badge {
  display: inline-block;
  padding: 0.125rem 0.5rem;
  background-color: #dcfce7;
  color: #166534;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
}

.roles-cell {
  display: flex;
  flex-wrap: wrap;
  gap: 0.25rem;
}

.role-badge-small {
  display: inline-block;
  padding: 0.125rem 0.5rem;
  background-color: #dbeafe;
  color: #1e40af;
  border-radius: 0.25rem;
  font-size: 0.75rem;
  font-weight: 500;
}

.no-role-text {
  color: #9ca3af;
  font-size: 0.875rem;
  font-style: italic;
}

/* Empty state styles */
.user-empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 0;
  color: #6b7280;
}

.user-empty-state h3 {
  font-size: 1.125rem;
  font-weight: 500;
  color: #111827;
  margin: 0 0 0.25rem 0;
}

.user-empty-state p {
  margin: 0;
  text-align: center;
}
</style>
