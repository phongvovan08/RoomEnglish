<template>
  <div class="role-management-container">
    <div class="page-header">
      <div class="header-content">
        <h1>
          <Icon icon="mdi:shield-account" class="w-8 h-8 mr-3" />
          Quản lý Vai trò
        </h1>
        <p>Quản lý vai trò và phân quyền trong hệ thống</p>
      </div>
      <button @click="openCreateModal" class="create-btn">
        <Icon icon="mdi:plus" class="w-5 h-5 mr-2" />
        Tạo vai trò mới
      </button>
    </div>

    <!-- Role Data Grid -->
    <RoleDataGrid
      :roles="roles"
      :loading="loading"
      :page-size="pageSize"
      :current-page="currentPage"
      :total-items="totalCount"
      :total-pages="totalPages"
      @edit-role="openEditModal"
      @delete-role="openDeleteConfirmation"
      @search="handleSearch"
      @page-change="goToPage"
      @page-size-change="changePageSize"
      @sort-change="handleSort"
    />

    <!-- Create/Edit Role Modal -->
    <div v-if="showFormModal" class="modal-overlay" @click="closeFormModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>{{ isEditMode ? 'Chỉnh sửa vai trò' : 'Tạo vai trò mới' }}</h2>
          <button class="close-btn" @click="closeFormModal">
            <Icon icon="mdi:close" class="w-6 h-6" />
          </button>
        </div>
        
        <div class="modal-body">
          <form @submit.prevent="handleSubmit">
            <div class="form-group">
              <label for="roleName">
                Tên vai trò <span class="required">*</span>
              </label>
              <input
                id="roleName"
                v-model="formData.name"
                type="text"
                placeholder="Nhập tên vai trò (VD: Teacher, Student)"
                required
                :disabled="saving"
                class="form-input"
              />
            </div>

            <div class="form-group">
              <label for="roleDescription">
                Mô tả <span class="required">*</span>
              </label>
              <textarea
                id="roleDescription"
                v-model="formData.description"
                placeholder="Nhập mô tả vai trò"
                rows="3"
                required
                :disabled="saving"
                class="form-input"
              />
            </div>

            <div class="modal-footer">
              <button type="button" @click="closeFormModal" :disabled="saving" class="cancel-btn">
                Hủy
              </button>
              <button type="submit" :disabled="saving || !isFormValid" class="save-btn">
                <Icon v-if="saving" icon="mdi:loading" class="w-5 h-5 mr-2 animate-spin" />
                <Icon v-else icon="mdi:check" class="w-5 h-5 mr-2" />
                {{ isEditMode ? 'Cập nhật' : 'Tạo vai trò' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Delete Confirmation Modal -->
    <div v-if="showDeleteModal" class="modal-overlay" @click="closeDeleteModal">
      <div class="modal-content modal-small" @click.stop>
        <div class="modal-header">
          <h2>Xác nhận xóa</h2>
          <button class="close-btn" @click="closeDeleteModal">
            <Icon icon="mdi:close" class="w-6 h-6" />
          </button>
        </div>
        
        <div class="modal-body">
          <div class="delete-warning">
            <Icon icon="mdi:alert" class="w-12 h-12 text-red-500 mb-4" />
            <p>Bạn có chắc chắn muốn xóa vai trò <strong>{{ deletingRole?.name }}</strong>?</p>
            <p class="warning-text">Hành động này không thể hoàn tác!</p>
          </div>

          <div class="modal-footer">
            <button type="button" @click="closeDeleteModal" :disabled="deleting" class="cancel-btn">
              Hủy
            </button>
            <button type="button" @click="handleDelete" :disabled="deleting" class="delete-btn">
              <Icon v-if="deleting" icon="mdi:loading" class="w-5 h-5 mr-2 animate-spin" />
              <Icon v-else icon="mdi:delete" class="w-5 h-5 mr-2" />
              Xóa vai trò
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { Icon } from '@iconify/vue'
import RoleDataGrid from '../components/RoleDataGrid.vue'
import { useRoleManagementAPI, type RoleDetail } from '@/composables/useRoleManagementAPI'
import { useNotifications } from '@/utils/notifications'

// Composables
const { getRoles, createRole, updateRole, deleteRole } = useRoleManagementAPI()
const { showSuccess, showError } = useNotifications()

// State
const roles = ref<RoleDetail[]>([])
const loading = ref(false)
const saving = ref(false)
const deleting = ref(false)

// Pagination
const currentPage = ref(1)
const pageSize = ref(10)
const totalCount = ref(0)
const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value))

// Search
const searchTerm = ref('')

// Modals
const showFormModal = ref(false)
const showDeleteModal = ref(false)
const isEditMode = ref(false)
const editingRole = ref<RoleDetail | null>(null)
const deletingRole = ref<RoleDetail | null>(null)

// Form data
const formData = ref({
  name: '',
  description: ''
})

// Validation
const isFormValid = computed(() => {
  return formData.value.name.trim() !== '' && formData.value.description.trim() !== ''
})

// Load roles
const loadRoles = async () => {
  loading.value = true
  try {
    console.log('🔄 Loading roles...', { currentPage: currentPage.value, pageSize: pageSize.value, searchTerm: searchTerm.value })
    
    const response = await getRoles(currentPage.value, pageSize.value, searchTerm.value || undefined)
    
    roles.value = response.roles
    totalCount.value = response.totalCount
    
    console.log('✅ Roles loaded:', response)
  } catch (error) {
    console.error('❌ Failed to load roles:', error)
    showError('Lỗi tải dữ liệu', 'Không thể tải danh sách vai trò')
  } finally {
    loading.value = false
  }
}

// Create modal
const openCreateModal = () => {
  isEditMode.value = false
  editingRole.value = null
  formData.value = {
    name: '',
    description: ''
  }
  showFormModal.value = true
}

// Edit modal
const openEditModal = (role: RoleDetail) => {
  isEditMode.value = true
  editingRole.value = role
  formData.value = {
    name: role.name,
    description: role.description
  }
  showFormModal.value = true
}

// Close form modal
const closeFormModal = () => {
  showFormModal.value = false
  isEditMode.value = false
  editingRole.value = null
  formData.value = {
    name: '',
    description: ''
  }
}

// Submit form
const handleSubmit = async () => {
  if (!isFormValid.value) return

  saving.value = true
  try {
    if (isEditMode.value && editingRole.value) {
      // Update role
      console.log('🔄 Updating role:', editingRole.value.id, formData.value)
      
      await updateRole({
        roleId: editingRole.value.id,
        name: formData.value.name,
        description: formData.value.description
      })
      
      showSuccess('Thành công', 'Vai trò đã được cập nhật')
      console.log('✅ Role updated successfully')
    } else {
      // Create role
      console.log('🔄 Creating role:', formData.value)
      
      await createRole({
        name: formData.value.name,
        description: formData.value.description
      })
      
      showSuccess('Thành công', 'Vai trò mới đã được tạo')
      console.log('✅ Role created successfully')
    }

    closeFormModal()
    await loadRoles()
  } catch (error: any) {
    console.error('❌ Failed to save role:', error)
    showError('Lỗi', error.message || 'Không thể lưu vai trò')
  } finally {
    saving.value = false
  }
}

// Delete confirmation
const openDeleteConfirmation = (role: RoleDetail) => {
  deletingRole.value = role
  showDeleteModal.value = true
}

const closeDeleteModal = () => {
  showDeleteModal.value = false
  deletingRole.value = null
}

// Delete role
const handleDelete = async () => {
  if (!deletingRole.value) return

  deleting.value = true
  try {
    console.log('🔄 Deleting role:', deletingRole.value.id)
    
    await deleteRole(deletingRole.value.id)
    
    showSuccess('Thành công', 'Vai trò đã được xóa')
    console.log('✅ Role deleted successfully')
    
    closeDeleteModal()
    await loadRoles()
  } catch (error: any) {
    console.error('❌ Failed to delete role:', error)
    showError('Lỗi xóa vai trò', error.message || 'Không thể xóa vai trò')
  } finally {
    deleting.value = false
  }
}

// Search handler
const handleSearch = (query: string) => {
  console.log('🔍 Search:', query)
  searchTerm.value = query
  currentPage.value = 1
  loadRoles()
}

// Pagination handlers
const goToPage = (page: number) => {
  console.log('📄 Go to page:', page)
  currentPage.value = page
  loadRoles()
}

const changePageSize = (size: number) => {
  console.log('📊 Change page size:', size)
  pageSize.value = size
  currentPage.value = 1
  loadRoles()
}

// Sort handler
const handleSort = (sortBy: string, sortOrder: 'asc' | 'desc') => {
  console.log('🔀 Sort:', sortBy, sortOrder)
  // TODO: Implement sorting if needed
}

// Initialize
onMounted(() => {
  loadRoles()
})
</script>

<style scoped>
.role-management-container {
  padding: 2rem;
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 2rem;
  flex-wrap: wrap;
  gap: 1rem;
}

.header-content h1 {
  display: flex;
  align-items: center;
  font-size: 2rem;
  font-weight: 700;
  color: #111827;
  margin: 0 0 0.5rem 0;
}

.header-content p {
  color: #6b7280;
  margin: 0;
}

.create-btn {
  display: flex;
  align-items: center;
  padding: 0.75rem 1.5rem;
  background-color: #2563eb;
  color: white;
  border: none;
  border-radius: 0.5rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.create-btn:hover {
  background-color: #1d4ed8;
}

/* Modal styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
}

.modal-content {
  background: white;
  border-radius: 0.75rem;
  width: 100%;
  max-width: 600px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
}

.modal-small {
  max-width: 450px;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid #e5e7eb;
}

.modal-header h2 {
  font-size: 1.5rem;
  font-weight: 600;
  color: #111827;
  margin: 0;
}

.close-btn {
  padding: 0.25rem;
  border: none;
  background: none;
  cursor: pointer;
  color: #6b7280;
  transition: color 0.2s;
}

.close-btn:hover {
  color: #111827;
}

.modal-body {
  padding: 1.5rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  font-weight: 500;
  color: #374151;
  margin-bottom: 0.5rem;
}

.required {
  color: #dc2626;
}

.form-input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 0.5rem;
  font-size: 1rem;
  transition: all 0.2s;
}

.form-input:focus {
  outline: none;
  border-color: #2563eb;
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}

.form-input:disabled {
  background-color: #f3f4f6;
  cursor: not-allowed;
}

textarea.form-input {
  resize: vertical;
  min-height: 80px;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  margin-top: 2rem;
}

.cancel-btn,
.save-btn,
.delete-btn {
  display: flex;
  align-items: center;
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 0.5rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.cancel-btn {
  background-color: #f3f4f6;
  color: #374151;
}

.cancel-btn:hover:not(:disabled) {
  background-color: #e5e7eb;
}

.save-btn {
  background-color: #2563eb;
  color: white;
}

.save-btn:hover:not(:disabled) {
  background-color: #1d4ed8;
}

.save-btn:disabled {
  background-color: #93c5fd;
  cursor: not-allowed;
}

.delete-btn {
  background-color: #dc2626;
  color: white;
}

.delete-btn:hover:not(:disabled) {
  background-color: #b91c1c;
}

.delete-btn:disabled {
  background-color: #fca5a5;
  cursor: not-allowed;
}

.delete-warning {
  text-align: center;
  padding: 1rem 0;
}

.delete-warning p {
  margin: 0.5rem 0;
  color: #374151;
}

.delete-warning strong {
  color: #dc2626;
}

.warning-text {
  font-size: 0.875rem;
  color: #ef4444;
  font-weight: 500;
}

.animate-spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}
</style>
