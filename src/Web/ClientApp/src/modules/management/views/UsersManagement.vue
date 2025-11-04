<template>
  <div class="user-management-container">
    <div class="page-header">
      <div class="header-content">
        <h1>
          <Icon icon="mdi:account-multiple" class="w-8 h-8 mr-3" />
          Quản lý Tài khoản
        </h1>
        <p>Quản lý người dùng và phân quyền trong hệ thống</p>
      </div>
      <button @click="openCreateModal" class="create-btn">
        <Icon icon="mdi:plus" class="w-5 h-5 mr-2" />
        Tạo người dùng mới
      </button>
    </div>

    <!-- User Data Grid -->
    <UserDataGrid
      :users="users"
      :loading="loading"
      :page-size="pageSize"
      :current-page="currentPage"
      :total-items="totalCount"
      :total-pages="totalPages"
      :current-user-id="currentUserId"
      @edit-user="openEditRolesModal"
      @delete-user="openDeleteConfirmation"
      @search="handleSearch"
      @page-change="goToPage"
      @page-size-change="changePageSize"
      @sort-change="handleSort"
    />

    <!-- Edit Roles Modal -->
    <div v-if="showEditModal" class="modal-overlay" @click="closeEditModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>Chỉnh sửa vai trò</h2>
          <button class="close-btn" @click="closeEditModal">
            <Icon icon="mdi:close" class="w-6 h-6" />
          </button>
        </div>
        
        <div class="modal-body">
          <p class="user-email">
            <strong>Email:</strong> {{ editingUser?.email }}
          </p>
          
          <!-- Current Roles Display -->
          <div v-if="editingUser" class="current-roles-section">
            <h4>Vai trò hiện tại:</h4>
            <div class="current-roles">
              <span v-if="editingUser.roles && editingUser.roles.length > 0" 
                    v-for="role in editingUser.roles" 
                    :key="role" 
                    class="role-badge current">
                <Icon icon="mdi:shield-account" class="w-3 h-3" />
                {{ role }}
              </span>
              <span v-else class="no-role-badge">
                <Icon icon="mdi:account" class="w-3 h-3" />
                Không có quyền
              </span>
            </div>
          </div>
          
          <div class="roles-selection">
            <h3>Chọn vai trò mới:</h3>
            
            <!-- Loading state -->
            <div v-if="loadingRoles" class="roles-loading">
              <Icon icon="mdi:loading" class="w-5 h-5 animate-spin" />
              <span>Đang tải vai trò...</span>
            </div>
            
            <!-- Roles list -->
            <div v-else-if="availableRoles.length > 0">
              <div 
                v-for="role in availableRoles" 
                :key="role.name" 
                class="role-checkbox"
              >
                <label>
                  <input
                    type="checkbox"
                    :value="role.name"
                    v-model="selectedRoles"
                  />
                  <span class="role-name">{{ role.name }}</span>
                  <span class="role-desc">
                    ({{ role.description }})
                  </span>
                </label>
              </div>
            </div>
            
            <!-- No roles available -->
            <div v-else class="no-roles-warning">
              <Icon icon="mdi:alert" class="w-5 h-5" />
              <span>Không có vai trò nào</span>
            </div>
          </div>
          
          <!-- Changes Preview -->
          <div v-if="hasRoleChanges" class="changes-preview">
            <Icon icon="mdi:alert-circle" class="w-4 h-4" />
            <span>Có thay đổi vai trò</span>
          </div>
        </div>
        
        <div class="modal-footer">
          <button class="cancel-btn" @click="closeEditModal">
            Hủy
          </button>
          <button 
            class="save-btn" 
            @click="saveUserRoles"
            :disabled="savingRoles || !hasRoleChanges"
          >
            {{ savingRoles ? 'Đang lưu...' : 'Lưu thay đổi' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Create User Modal -->
    <div v-if="showCreateModal" class="modal-overlay" @click="closeCreateModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>Tạo người dùng mới</h2>
          <button class="close-btn" @click="closeCreateModal">
            <Icon icon="mdi:close" class="w-6 h-6" />
          </button>
        </div>
        
        <div class="modal-body">
          <form @submit.prevent="createNewUser">
            <div class="form-group">
              <label for="userEmail">
                Email <span class="required">*</span>
              </label>
              <input
                id="userEmail"
                v-model="newUserForm.email"
                type="email"
                required
                placeholder="user@example.com"
                :disabled="creatingUser"
                class="form-input"
              />
            </div>

            <div class="form-group">
              <label for="userName">Tên người dùng</label>
              <input
                id="userName"
                v-model="newUserForm.userName"
                type="text"
                placeholder="Tùy chọn"
                :disabled="creatingUser"
                class="form-input"
              />
            </div>

            <div class="form-group">
              <label for="userPassword">
                Mật khẩu <span class="required">*</span>
              </label>
              <input
                id="userPassword"
                v-model="newUserForm.password"
                type="password"
                required
                placeholder="Nhập mật khẩu"
                :disabled="creatingUser"
                class="form-input"
              />
            </div>

            <div class="roles-selection">
              <h3>Phân quyền:</h3>
              <div v-if="availableRoles.length > 0">
                <div v-for="role in availableRoles" :key="role.name" class="role-checkbox">
                  <label>
                    <input
                      type="checkbox"
                      :value="role.name"
                      v-model="newUserForm.roles"
                      :disabled="creatingUser"
                    />
                    <span class="role-name">{{ role.name }}</span>
                    <span class="role-desc">({{ role.description }})</span>
                  </label>
                </div>
              </div>
            </div>

            <div class="modal-footer">
              <button type="button" @click="closeCreateModal" :disabled="creatingUser" class="cancel-btn">
                Hủy
              </button>
              <button type="submit" :disabled="creatingUser || !isCreateFormValid" class="save-btn">
                <Icon v-if="creatingUser" icon="mdi:loading" class="w-5 h-5 mr-2 animate-spin" />
                <Icon v-else icon="mdi:check" class="w-5 h-5 mr-2" />
                {{ creatingUser ? 'Đang tạo...' : 'Tạo người dùng' }}
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
            <p>Bạn có chắc chắn muốn xóa người dùng <strong>{{ deletingUser?.email }}</strong>?</p>
            <p class="warning-text">Hành động này không thể hoàn tác!</p>
          </div>

          <div class="modal-footer">
            <button type="button" @click="closeDeleteModal" :disabled="deletingUser !== null && deleting" class="cancel-btn">
              Hủy
            </button>
            <button type="button" @click="confirmDeleteUser" :disabled="deleting" class="delete-btn">
              <Icon v-if="deleting" icon="mdi:loading" class="w-5 h-5 mr-2 animate-spin" />
              <Icon v-else icon="mdi:delete" class="w-5 h-5 mr-2" />
              Xóa người dùng
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
import { useUserManagementAPI, type User, type RoleDto, type CreateUserRequest } from '@/composables/useUserManagementAPI'
import { useAuthStore } from '@/stores/auth'
import { useNotifications } from '@/utils/notifications'
import UserDataGrid from '../components/UserDataGrid.vue'

const authStore = useAuthStore()
const { getUsers, getAvailableRoles, updateUserRoles, createUser, deleteUser } = useUserManagementAPI()
const { showSuccess, showError } = useNotifications()

// State
const users = ref<User[]>([])
const availableRoles = ref<RoleDto[]>([])
const loading = ref(false)
const savingRoles = ref(false)
const loadingRoles = ref(false)
const creatingUser = ref(false)
const deleting = ref(false)

// Filters
const searchTerm = ref('')

// Pagination
const currentPage = ref(1)
const pageSize = ref(10)
const totalCount = ref(0)

// Modal
const showEditModal = ref(false)
const showCreateModal = ref(false)
const showDeleteModal = ref(false)
const editingUser = ref<User | null>(null)
const deletingUser = ref<User | null>(null)
const selectedRoles = ref<string[]>([])
const newUserForm = ref<CreateUserRequest>({
  email: '',
  password: '',
  userName: '',
  roles: []
})

// Computed
const isCreateFormValid = computed(() => {
  return newUserForm.value.email.trim() !== '' &&
    (newUserForm.value.userName?.trim() || '') !== '' &&
    newUserForm.value.password.trim() !== '' &&
    newUserForm.value.password.length >= 6
})

// Current user ID (to prevent editing self)
const currentUserId = computed(() => authStore.user?.id || '')

const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value))

// Check if roles have changed
const hasRoleChanges = computed(() => {
  if (!editingUser.value) return false
  const currentRoles = editingUser.value.roles || []
  return JSON.stringify([...currentRoles].sort()) !== JSON.stringify([...selectedRoles.value].sort())
})

// Load users with filters
const loadUsers = async () => {
  loading.value = true
  try {
    const result = await getUsers(
      searchTerm.value,
      undefined, // role filter
      currentPage.value,
      pageSize.value
    )
    
    if (result) {
      users.value = result.users
      totalCount.value = result.totalCount
    }
  } catch (error) {
    console.error('Failed to load users:', error)
    showError('Lỗi tải dữ liệu', 'Không thể tải danh sách người dùng')
  } finally {
    loading.value = false
  }
}

// Load available roles
const loadAvailableRoles = async () => {
  loadingRoles.value = true
  try {
    console.log('🔄 Loading available roles from API...')
    const roles = await getAvailableRoles()
    availableRoles.value = roles
    console.log('✅ Available roles loaded:', roles)
    
    if (roles.length === 0) {
      showError('Cảnh báo', 'Không tìm thấy vai trò nào trong hệ thống')
    }
  } catch (error) {
    console.error('❌ Failed to load roles:', error)
    showError('Lỗi tải vai trò', 'Không thể tải danh sách vai trò từ server')
    // Fallback to default roles if API fails
    availableRoles.value = [
      { name: 'Administrator', description: 'Toàn quyền quản trị hệ thống' },
      { name: 'User', description: 'Người dùng thông thường' }
    ]
  } finally {
    loadingRoles.value = false
  }
}

// Create User Modal handlers
const openCreateModal = () => {
  newUserForm.value = {
    email: '',
    password: '',
    userName: '',
    roles: []
  }
  showCreateModal.value = true
}

const closeCreateModal = () => {
  showCreateModal.value = false
  newUserForm.value = {
    email: '',
    password: '',
    userName: '',
    roles: []
  }
}

const createNewUser = async () => {
  if (!isCreateFormValid.value) return

  creatingUser.value = true
  try {
    await createUser(newUserForm.value)
    showSuccess('Thành công', 'Đã tạo người dùng mới')
    closeCreateModal()
    await loadUsers()
  } catch (error: any) {
    console.error('Failed to create user:', error)
    const errorMessage = error?.response?.data?.title || error?.message || 'Không thể tạo người dùng'
    showError('Lỗi tạo người dùng', errorMessage)
  } finally {
    creatingUser.value = false
  }
}

// Delete User Modal handlers
const openDeleteConfirmation = (user: User) => {
  deletingUser.value = user
  showDeleteModal.value = true
}

const closeDeleteModal = () => {
  showDeleteModal.value = false
  deletingUser.value = null
}

const confirmDeleteUser = async () => {
  if (!deletingUser.value) return

  deleting.value = true
  try {
    await deleteUser(deletingUser.value.id)
    showSuccess('Thành công', 'Đã xóa người dùng')
    closeDeleteModal()
    await loadUsers()
  } catch (error: any) {
    console.error('Failed to delete user:', error)
    const errorMessage = error?.response?.data?.title || error?.message || 'Không thể xóa người dùng'
    showError('Lỗi xóa người dùng', errorMessage)
  } finally {
    deleting.value = false
  }
}

// Edit User Modal handlers

// Debounced search
let searchTimeout: ReturnType<typeof setTimeout> | null = null
const handleSearch = (query: string) => {
  searchTerm.value = query
  if (searchTimeout) clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    currentPage.value = 1
    loadUsers()
  }, 300)
}

// Pagination handlers
const goToPage = (page: number) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page
    loadUsers()
  }
}

const changePageSize = (newSize: number) => {
  pageSize.value = newSize
  currentPage.value = 1
  loadUsers()
}

// Sort handler (placeholder - backend needs to support this)
const handleSort = (sortByParam: string, sortOrderParam: 'asc' | 'desc') => {
  console.log('Sort requested:', sortByParam, sortOrderParam)
  // TODO: Implement sorting when backend supports it
}

const openEditRolesModal = (user: User) => {
  editingUser.value = user
  selectedRoles.value = [...(user.roles || [])]
  showEditModal.value = true
  
  // Reload roles để đảm bảo data mới nhất
  if (availableRoles.value.length === 0) {
    loadAvailableRoles()
  }
}

const closeEditModal = () => {
  showEditModal.value = false
  editingUser.value = null
  selectedRoles.value = []
}

const saveUserRoles = async () => {
  if (!editingUser.value) return
  
  // Validation: Check if roles changed
  const currentRoles = editingUser.value.roles || []
  const hasChanges = JSON.stringify([...currentRoles].sort()) !== JSON.stringify([...selectedRoles.value].sort())
  
  if (!hasChanges) {
    showError('Không có thay đổi', 'Vai trò của người dùng không thay đổi')
    return
  }
  
  savingRoles.value = true
  try {
    const success = await updateUserRoles(editingUser.value.id, selectedRoles.value)
    
    if (success) {
      // Update user in the list immediately for better UX
      const userIndex = users.value.findIndex(u => u.id === editingUser.value!.id)
      if (userIndex !== -1) {
        users.value[userIndex] = {
          ...users.value[userIndex],
          roles: [...selectedRoles.value]
        }
      }
      
      const roleNames = selectedRoles.value.length > 0 
        ? selectedRoles.value.join(', ') 
        : 'Không có quyền'
      
      showSuccess(
        'Cập nhật thành công', 
        `Vai trò của ${editingUser.value.email}: ${roleNames}`
      )
      closeEditModal()
      
      // Reload to ensure data consistency with server
      await loadUsers()
    } else {
      showError('Lỗi cập nhật', 'Không thể cập nhật vai trò. Vui lòng thử lại.')
    }
  } catch (error) {
    console.error('Error saving user roles:', error)
    showError('Lỗi hệ thống', 'Đã xảy ra lỗi khi cập nhật vai trò')
  } finally {
    savingRoles.value = false
  }
}

// Lifecycle
onMounted(async () => {
  await Promise.all([
    loadUsers(),
    loadAvailableRoles()
  ])
})
</script>

<style scoped>
.user-management-container {
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

.page-header h1 {
  color: #74c0fc;
  display: flex;
  align-items: center;
  justify-content: flex-start;
  font-size: 2.5rem;
  margin-bottom: 0.5rem;
}

.page-header p {
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

/* Modal Styles */
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

.user-email {
  color: white;
  margin-bottom: 1.5rem;
  padding: 1rem;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 8px;
}

.user-email strong {
  color: #74c0fc;
}

.current-roles-section {
  margin-bottom: 1.5rem;
  padding: 1rem;
  background: rgba(255, 255, 255, 0.03);
  border-radius: 8px;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.current-roles-section h4 {
  color: #74c0fc;
  margin: 0 0 0.75rem 0;
  font-size: 0.875rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.current-roles {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.role-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  padding: 0.375rem 0.75rem;
  background-color: rgba(116, 192, 252, 0.2);
  color: #74c0fc;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
  border: 1px solid rgba(116, 192, 252, 0.3);
}

.role-badge.current {
  background-color: rgba(34, 197, 94, 0.2);
  color: #4ade80;
  border-color: rgba(34, 197, 94, 0.3);
}

.no-role-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  padding: 0.375rem 0.75rem;
  background-color: rgba(156, 163, 175, 0.2);
  color: #9ca3af;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
  border: 1px solid rgba(156, 163, 175, 0.3);
  font-style: italic;
}

.roles-selection h3 {
  color: white;
  margin-bottom: 1rem;
}

.roles-loading {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1.5rem;
  color: rgba(255, 255, 255, 0.7);
  justify-content: center;
}

.roles-loading .animate-spin {
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

.no-roles-warning {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 1rem;
  background: rgba(251, 191, 36, 0.1);
  border: 1px solid rgba(251, 191, 36, 0.3);
  border-radius: 8px;
  color: #fbbf24;
  font-size: 0.875rem;
}

.role-checkbox {
  margin-bottom: 0.75rem;
}

.role-checkbox label {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  color: white;
  cursor: pointer;
  padding: 0.75rem;
  border-radius: 8px;
  transition: background 0.2s;
}

.role-checkbox label:hover {
  background: rgba(255, 255, 255, 0.05);
}

.role-checkbox input[type="checkbox"] {
  width: 1.25rem;
  height: 1.25rem;
  cursor: pointer;
}

.role-name {
  font-weight: 600;
}

.role-desc {
  color: rgba(255, 255, 255, 0.6);
  font-size: 0.875rem;
}

.changes-preview {
  margin-top: 1rem;
  padding: 0.75rem 1rem;
  background: rgba(251, 191, 36, 0.1);
  border: 1px solid rgba(251, 191, 36, 0.3);
  border-radius: 8px;
  color: #fbbf24;
  font-size: 0.875rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 500;
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

/* Mobile Responsiveness */
@media (max-width: 768px) {
  .user-management-container {
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
