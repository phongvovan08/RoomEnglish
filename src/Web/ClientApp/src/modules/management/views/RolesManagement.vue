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
  max-width: 1400px;
  margin: 0 auto;
  padding: 2rem;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
  min-height: 100vh;
}

.page-header {
  text-align: center;
  margin-bottom: 3rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 1.5rem;
}

.header-content {
  flex: 1;
  text-align: left;
}

.header-content h1 {
  color: #74c0fc;
  display: flex;
  align-items: center;
  justify-content: flex-start;
  font-size: 2.5rem;
  margin-bottom: 0.5rem;
}

.header-content p {
  color: rgba(255, 255, 255, 0.7);
  font-size: 1.1rem;
}

.create-btn {
  background: linear-gradient(135deg, #74c0fc, #339af0);
  color: white;
  border: none;
  padding: 0.875rem 1.75rem;
  border-radius: 12px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  box-shadow: 0 4px 15px rgba(116, 192, 252, 0.3);
}

.create-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(116, 192, 252, 0.5);
}

/* Modal styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  backdrop-filter: blur(5px);
}

.modal-content {
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
  border-radius: 20px;
  max-width: 500px;
  width: 90%;
  max-height: 90vh;
  overflow-y: auto;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.modal-small {
  max-width: 400px;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.modal-header h2 {
  color: white;
  margin: 0;
}

.close-btn {
  background: none;
  border: none;
  color: rgba(255, 255, 255, 0.7);
  cursor: pointer;
  padding: 0.5rem;
  border-radius: 8px;
  transition: all 0.2s ease;
}

.close-btn:hover {
  background: rgba(255, 255, 255, 0.1);
  color: white;
}

.modal-body {
  padding: 1.5rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  color: rgba(255, 255, 255, 0.9);
  margin-bottom: 0.5rem;
  font-weight: 500;
}

.form-group .required {
  color: #ff6b6b;
  margin-left: 0.25rem;
}

.form-input {
  width: 100%;
  padding: 0.75rem 1rem;
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 8px;
  color: white;
  font-size: 1rem;
  transition: all 0.3s ease;
}

.form-input:focus {
  outline: none;
  border-color: #74c0fc;
  background: rgba(255, 255, 255, 0.08);
  box-shadow: 0 0 0 3px rgba(116, 192, 252, 0.1);
}

.form-input:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.form-input::placeholder {
  color: rgba(255, 255, 255, 0.4);
}

textarea.form-input {
  resize: vertical;
  min-height: 80px;
  font-family: inherit;
}

.delete-warning {
  text-align: center;
  color: rgba(255, 255, 255, 0.9);
  padding: 1rem;
}

.delete-warning p {
  margin: 0.5rem 0;
  line-height: 1.6;
}

.delete-warning strong {
  color: #74c0fc;
  font-weight: 600;
}

.warning-text {
  color: #ff6b6b !important;
  font-size: 0.875rem;
  margin-top: 1rem !important;
}

.modal-footer {
  padding: 1.5rem;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
}

.cancel-btn {
  background: rgba(255, 255, 255, 0.1);
  color: white;
  border: 1px solid rgba(255, 255, 255, 0.2);
  padding: 0.75rem 1.5rem;
  border-radius: 12px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.cancel-btn:hover {
  background: rgba(255, 255, 255, 0.2);
}

.save-btn {
  background: linear-gradient(135deg, #74c0fc, #339af0);
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 12px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
}

.save-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(116, 192, 252, 0.4);
}

.save-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.delete-btn {
  background: linear-gradient(135deg, #ff6b6b, #ee5a52);
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 12px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
}

.delete-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(255, 107, 107, 0.4);
}

.delete-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
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

/* Mobile Responsiveness */
@media (max-width: 768px) {
  .role-management-container {
    padding: 1rem;
  }

  .page-header h1 {
    font-size: 2rem;
  }

  .modal-content {
    margin: 1rem;
    width: calc(100% - 2rem);
  }
}
</style>
