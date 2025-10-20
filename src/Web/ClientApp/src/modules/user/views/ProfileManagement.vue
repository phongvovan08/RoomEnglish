<template>
  <div class="profile-management">
    <!-- Page Header -->
    <div class="page-header">
      <div class="header-content">
        <h1>
          <Icon icon="mdi:account-circle" class="w-8 h-8" />
          Quản lý Profile
        </h1>
        <p>Quản lý thông tin cá nhân và cài đặt tài khoản</p>
      </div>
    </div>

    <!-- Profile Content -->
    <div class="profile-content">
      <!-- Profile Overview Card -->
      <div class="profile-overview">
        <div class="avatar-section">
          <div class="avatar-container">
            <img 
              v-if="userProfile.avatar" 
              :src="userProfile.avatar" 
              :alt="userProfile.displayName"
              class="avatar-image"
            />
            <div v-else class="avatar-placeholder">
              <Icon icon="mdi:account" class="w-16 h-16" />
            </div>
            <button @click="showAvatarUpload = true" class="avatar-edit-btn">
              <Icon icon="mdi:camera" class="w-4 h-4" />
            </button>
          </div>
        </div>
        
        <div class="profile-info">
          <h2>{{ userProfile.displayName || userProfile.email }}</h2>
          <p class="email">{{ userProfile.email }}</p>
          <div class="profile-stats">
            <div class="stat">
              <Icon icon="mdi:calendar" class="w-4 h-4" />
              <span>Tham gia: {{ formatDate(userProfile.createdAt) }}</span>
            </div>
            <div class="stat">
              <Icon icon="mdi:clock" class="w-4 h-4" />
              <span>Hoạt động: {{ formatDate(userProfile.lastLoginAt) }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Profile Forms -->
      <div class="profile-forms">
        <!-- Personal Information -->
        <div class="form-section">
          <div class="section-header">
            <h3>
              <Icon icon="mdi:account-edit" class="w-5 h-5" />
              Thông tin cá nhân
            </h3>
            <button 
              v-if="!isEditingPersonal"
              @click="startEditPersonal"
              class="btn-edit"
            >
              <Icon icon="mdi:pencil" class="w-4 h-4" />
              Chỉnh sửa
            </button>
          </div>
          
          <form @submit.prevent="savePersonalInfo" class="profile-form">
            <div class="form-grid">
              <div class="form-group">
                <label for="firstName">Họ</label>
                <input 
                  id="firstName"
                  v-model="personalForm.firstName"
                  type="text"
                  :readonly="!isEditingPersonal"
                  class="form-input"
                  placeholder="Nhập họ của bạn"
                />
              </div>
              
              <div class="form-group">
                <label for="lastName">Tên</label>
                <input 
                  id="lastName"
                  v-model="personalForm.lastName"
                  type="text"
                  :readonly="!isEditingPersonal"
                  class="form-input"
                  placeholder="Nhập tên của bạn"
                />
              </div>
              
              <div class="form-group">
                <label for="displayName">Tên hiển thị</label>
                <input 
                  id="displayName"
                  v-model="personalForm.displayName"
                  type="text"
                  :readonly="!isEditingPersonal"
                  class="form-input"
                  placeholder="Nhập tên hiển thị"
                />
              </div>
              
              <div class="form-group">
                <label for="phone">Số điện thoại</label>
                <input 
                  id="phone"
                  v-model="personalForm.phone"
                  type="tel"
                  :readonly="!isEditingPersonal"
                  class="form-input"
                  placeholder="Nhập số điện thoại"
                />
              </div>
              
              <div class="form-group full-width">
                <label for="bio">Giới thiệu bản thân</label>
                <textarea 
                  id="bio"
                  v-model="personalForm.bio"
                  :readonly="!isEditingPersonal"
                  class="form-textarea"
                  placeholder="Giới thiệu ngắn về bản thân..."
                  rows="3"
                ></textarea>
              </div>
            </div>
            
            <div v-if="isEditingPersonal" class="form-actions">
              <button type="button" @click="cancelEditPersonal" class="btn-secondary">
                <Icon icon="mdi:cancel" class="w-4 h-4" />
                Hủy
              </button>
              <button type="submit" :disabled="isLoading" class="btn-primary">
                <Icon icon="mdi:content-save" class="w-4 h-4" />
                Lưu thay đổi
              </button>
            </div>
          </form>
        </div>

        <!-- Security Settings -->
        <div class="form-section">
          <div class="section-header">
            <h3>
              <Icon icon="mdi:shield-account" class="w-5 h-5" />
              Bảo mật tài khoản
            </h3>
          </div>
          
          <div class="security-options">
            <div class="security-item">
              <div class="security-info">
                <h4>Đổi mật khẩu</h4>
                <p>Cập nhật mật khẩu để bảo vệ tài khoản</p>
              </div>
              <button @click="showChangePassword = true" class="btn-outline">
                <Icon icon="mdi:key-change" class="w-4 h-4" />
                Đổi mật khẩu
              </button>
            </div>
            
            <div class="security-item">
              <div class="security-info">
                <h4>Xác thực 2 bước</h4>
                <p>Tăng cường bảo mật với xác thực 2 bước</p>
              </div>
              <button class="btn-outline" disabled>
                <Icon icon="mdi:two-factor-authentication" class="w-4 h-4" />
                Sắp có
              </button>
            </div>
          </div>
        </div>

        <!-- Learning Preferences -->
        <div class="form-section">
          <div class="section-header">
            <h3>
              <Icon icon="mdi:school" class="w-5 h-5" />
              Tùy chọn học tập
            </h3>
          </div>
          
          <form @submit.prevent="savePreferences" class="preferences-form">
            <div class="preference-group">
              <div class="preference-item">
                <label class="switch-label">
                  <input 
                    type="checkbox" 
                    v-model="preferences.emailNotifications"
                    class="switch-input"
                  />
                  <span class="switch-slider"></span>
                  <span class="switch-text">Nhận thông báo qua email</span>
                </label>
              </div>
              
              <div class="preference-item">
                <label class="switch-label">
                  <input 
                    type="checkbox" 
                    v-model="preferences.dailyReminder"
                    class="switch-input"
                  />
                  <span class="switch-slider"></span>
                  <span class="switch-text">Nhắc nhở học hàng ngày</span>
                </label>
              </div>
              
              <div class="preference-item">
                <label class="switch-label">
                  <input 
                    type="checkbox" 
                    v-model="preferences.soundEffects"
                    class="switch-input"
                  />
                  <span class="switch-slider"></span>
                  <span class="switch-text">Hiệu ứng âm thanh</span>
                </label>
              </div>
            </div>
            
            <div class="language-preference">
              <label for="language">Ngôn ngữ giao diện</label>
              <select id="language" v-model="preferences.language" class="form-select">
                <option value="vi">Tiếng Việt</option>
                <option value="en">English</option>
              </select>
            </div>
            
            <button type="submit" :disabled="isLoading" class="btn-primary">
              <Icon icon="mdi:content-save" class="w-4 h-4" />
              Lưu tùy chọn
            </button>
          </form>
        </div>
      </div>
    </div>

    <!-- Change Password Modal -->
    <div v-if="showChangePassword" class="modal-overlay" @click="showChangePassword = false">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h3>Đổi mật khẩu</h3>
          <button @click="showChangePassword = false" class="close-btn">
            <Icon icon="mdi:close" class="w-5 h-5" />
          </button>
        </div>
        
        <form @submit.prevent="changePassword" class="modal-form">
          <div class="form-group">
            <label for="currentPassword">Mật khẩu hiện tại</label>
            <input 
              id="currentPassword"
              v-model="passwordForm.currentPassword"
              type="password"
              class="form-input"
              required
            />
          </div>
          
          <div class="form-group">
            <label for="newPassword">Mật khẩu mới</label>
            <input 
              id="newPassword"
              v-model="passwordForm.newPassword"
              type="password"
              class="form-input"
              required
              minlength="6"
            />
          </div>
          
          <div class="form-group">
            <label for="confirmPassword">Xác nhận mật khẩu mới</label>
            <input 
              id="confirmPassword"
              v-model="passwordForm.confirmPassword"
              type="password"
              class="form-input"
              required
            />
          </div>
          
          <div class="modal-actions">
            <button type="button" @click="showChangePassword = false" class="btn-secondary">
              Hủy
            </button>
            <button type="submit" :disabled="isLoading" class="btn-primary">
              <Icon icon="mdi:key-change" class="w-4 h-4" />
              Đổi mật khẩu
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Avatar Upload Modal -->
    <div v-if="showAvatarUpload" class="modal-overlay" @click="showAvatarUpload = false">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h3>Cập nhật ảnh đại diện</h3>
          <button @click="showAvatarUpload = false" class="close-btn">
            <Icon icon="mdi:close" class="w-5 h-5" />
          </button>
        </div>
        
        <div class="avatar-upload">
          <div class="upload-area">
            <input 
              type="file" 
              ref="fileInput"
              @change="handleFileSelect"
              accept="image/*"
              hidden
            />
            <div @click="$refs.fileInput?.click()" class="upload-placeholder">
              <Icon icon="mdi:cloud-upload" class="w-12 h-12" />
              <p>Click để chọn ảnh hoặc kéo thả vào đây</p>
              <small>Hỗ trợ: JPG, PNG (tối đa 2MB)</small>
            </div>
          </div>
          
          <div class="modal-actions">
            <button type="button" @click="showAvatarUpload = false" class="btn-secondary">
              Hủy
            </button>
            <button 
              @click="uploadAvatar" 
              :disabled="!selectedFile || isLoading" 
              class="btn-primary"
            >
              <Icon icon="mdi:upload" class="w-4 h-4" />
              Tải lên
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { Icon } from '@iconify/vue'
import { useAuthStore } from '@/stores/auth'
import { useToast } from '@/composables/useToast'

// Types
interface UserProfile {
  id: string
  email: string
  firstName: string
  lastName: string
  displayName: string
  phone: string
  bio: string
  avatar: string
  createdAt: string
  lastLoginAt: string
}

interface PersonalForm {
  firstName: string
  lastName: string
  displayName: string
  phone: string
  bio: string
}

interface PasswordForm {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}

interface Preferences {
  emailNotifications: boolean
  dailyReminder: boolean
  soundEffects: boolean
  language: string
}

// Composables
const authStore = useAuthStore()
const { showSuccess, showError, showWarning } = useToast()

// Reactive state
const isLoading = ref(false)
const isEditingPersonal = ref(false)
const showChangePassword = ref(false)
const showAvatarUpload = ref(false)
const selectedFile = ref<File | null>(null)
const fileInput = ref<HTMLInputElement>()

const userProfile = ref<UserProfile>({
  id: '',
  email: '',
  firstName: '',
  lastName: '',
  displayName: '',
  phone: '',
  bio: '',
  avatar: '',
  createdAt: '',
  lastLoginAt: ''
})

const personalForm = reactive<PersonalForm>({
  firstName: '',
  lastName: '',
  displayName: '',
  phone: '',
  bio: ''
})

const passwordForm = reactive<PasswordForm>({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const preferences = reactive<Preferences>({
  emailNotifications: true,
  dailyReminder: false,
  soundEffects: true,
  language: 'vi'
})

// Methods
const loadUserProfile = async () => {
  try {
    isLoading.value = true
    const response = await fetch('/api/users/me', {
      headers: {
        'Authorization': `Bearer ${authStore.token}`,
        'Content-Type': 'application/json'
      }
    })
    
    if (response.ok) {
      const data = await response.json()
      userProfile.value = data
      
      // Copy to form
      Object.assign(personalForm, {
        firstName: data.firstName || '',
        lastName: data.lastName || '',
        displayName: data.displayName || '',
        phone: data.phone || '',
        bio: data.bio || ''
      })
    } else {
      showError('Lỗi tải profile', 'Không thể tải thông tin profile')
    }
  } catch (error) {
    showError('Lỗi kết nối', 'Không thể kết nối đến máy chủ')
    console.error('Failed to load user profile:', error)
  } finally {
    isLoading.value = false
  }
}

const startEditPersonal = () => {
  isEditingPersonal.value = true
}

const cancelEditPersonal = () => {
  isEditingPersonal.value = false
  // Reset form
  Object.assign(personalForm, {
    firstName: userProfile.value.firstName || '',
    lastName: userProfile.value.lastName || '',
    displayName: userProfile.value.displayName || '',
    phone: userProfile.value.phone || '',
    bio: userProfile.value.bio || ''
  })
}

const savePersonalInfo = async () => {
  try {
    isLoading.value = true
    const response = await fetch('/api/users/me', {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${authStore.token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(personalForm)
    })
    
    if (response.ok) {
      const updatedProfile = await response.json()
      userProfile.value = { ...userProfile.value, ...updatedProfile }
      isEditingPersonal.value = false
      showSuccess('Cập nhật thành công!', 'Thông tin cá nhân đã được cập nhật')
    } else {
      showError('Lỗi cập nhật', 'Không thể cập nhật thông tin')
    }
  } catch (error) {
    showError('Lỗi kết nối', 'Không thể kết nối đến máy chủ')
    console.error('Failed to save personal info:', error)
  } finally {
    isLoading.value = false
  }
}

const changePassword = async () => {
  if (passwordForm.newPassword !== passwordForm.confirmPassword) {
    showWarning('Mật khẩu không khớp', 'Mật khẩu mới và xác nhận mật khẩu không giống nhau')
    return
  }
  
  try {
    isLoading.value = true
    const response = await fetch('/api/users/change-password', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${authStore.token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        currentPassword: passwordForm.currentPassword,
        newPassword: passwordForm.newPassword
      })
    })
    
    if (response.ok) {
      showChangePassword.value = false
      Object.assign(passwordForm, {
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
      })
      showSuccess('Đổi mật khẩu thành công!', 'Mật khẩu của bạn đã được cập nhật')
    } else {
      const error = await response.text()
      showError('Lỗi đổi mật khẩu', error || 'Không thể đổi mật khẩu')
    }
  } catch (error) {
    showError('Lỗi kết nối', 'Không thể kết nối đến máy chủ')
    console.error('Failed to change password:', error)
  } finally {
    isLoading.value = false
  }
}

const savePreferences = async () => {
  try {
    isLoading.value = true
    const response = await fetch('/api/users/preferences', {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${authStore.token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(preferences)
    })
    
    if (response.ok) {
      showSuccess('Lưu tùy chọn thành công!', 'Các tùy chọn học tập đã được cập nhật')
    } else {
      showError('Lỗi lưu tùy chọn', 'Không thể lưu tùy chọn')
    }
  } catch (error) {
    showError('Lỗi kết nối', 'Không thể kết nối đến máy chủ')
    console.error('Failed to save preferences:', error)
  } finally {
    isLoading.value = false
  }
}

const handleFileSelect = (event: Event) => {
  const file = (event.target as HTMLInputElement).files?.[0]
  if (file) {
    if (file.size > 2 * 1024 * 1024) {
      showWarning('File quá lớn', 'Vui lòng chọn file nhỏ hơn 2MB')
      return
    }
    selectedFile.value = file
  }
}

const uploadAvatar = async () => {
  if (!selectedFile.value) return
  
  try {
    isLoading.value = true
    const formData = new FormData()
    formData.append('avatar', selectedFile.value)
    
    const response = await fetch('/api/users/avatar', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${authStore.token}`
      },
      body: formData
    })
    
    if (response.ok) {
      const result = await response.json()
      userProfile.value.avatar = result.avatarUrl
      showAvatarUpload.value = false
      selectedFile.value = null
      showSuccess('Cập nhật ảnh đại diện thành công!')
    } else {
      showError('Lỗi tải ảnh', 'Không thể tải lên ảnh đại diện')
    }
  } catch (error) {
    showError('Lỗi kết nối', 'Không thể kết nối đến máy chủ')
    console.error('Failed to upload avatar:', error)
  } finally {
    isLoading.value = false
  }
}

const formatDate = (dateString: string): string => {
  if (!dateString) return ''
  return new Date(dateString).toLocaleDateString('vi-VN')
}

// Lifecycle
onMounted(() => {
  loadUserProfile()
})
</script>

<style scoped>
.profile-management {
  max-width: 1000px;
  margin: 0 auto;
  padding: 2rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  min-height: 100vh;
}

.page-header {
  margin-bottom: 2rem;
}

.header-content h1 {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  font-size: 2rem;
  font-weight: 700;
  color: white;
  margin: 0 0 0.5rem 0;
}

.header-content p {
  color: rgba(255, 255, 255, 0.8);
  font-size: 1.125rem;
  margin: 0;
}

.profile-content {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.profile-overview {
  background: white;
  border-radius: 1rem;
  padding: 2rem;
  display: flex;
  align-items: center;
  gap: 2rem;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
}

.avatar-section {
  flex-shrink: 0;
}

.avatar-container {
  position: relative;
  width: 120px;
  height: 120px;
}

.avatar-image {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  object-fit: cover;
  border: 4px solid #667eea;
}

.avatar-placeholder {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  background: #f3f4f6;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #9ca3af;
  border: 4px solid #e5e7eb;
}

.avatar-edit-btn {
  position: absolute;
  bottom: 0;
  right: 0;
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: #667eea;
  color: white;
  border: 3px solid white;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s;
}

.avatar-edit-btn:hover {
  background: #5a6fd8;
  transform: scale(1.1);
}

.profile-info h2 {
  font-size: 1.5rem;
  font-weight: 600;
  color: #111827;
  margin: 0 0 0.5rem 0;
}

.email {
  color: #6b7280;
  font-size: 1rem;
  margin: 0 0 1rem 0;
}

.profile-stats {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.stat {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #6b7280;
  font-size: 0.875rem;
}

.profile-forms {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.form-section {
  background: white;
  border-radius: 1rem;
  padding: 2rem;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
}

.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid #e5e7eb;
}

.section-header h3 {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 1.25rem;
  font-weight: 600;
  color: #111827;
  margin: 0;
}

.btn-edit {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  background: #f3f4f6;
  color: #374151;
  border: 1px solid #d1d5db;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-edit:hover {
  background: #e5e7eb;
}

.profile-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-group.full-width {
  grid-column: 1 / -1;
}

.form-group label {
  font-weight: 500;
  color: #374151;
  font-size: 0.875rem;
}

.form-input, .form-textarea, .form-select {
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  transition: all 0.2s;
}

.form-input:focus, .form-textarea:focus, .form-select:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.form-input:read-only {
  background: #f9fafb;
  color: #6b7280;
}

.form-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  padding-top: 1rem;
  border-top: 1px solid #e5e7eb;
}

.btn-primary, .btn-secondary, .btn-outline {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.5rem;
  border-radius: 0.5rem;
  font-weight: 500;
  font-size: 0.875rem;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid transparent;
}

.btn-primary {
  background: #667eea;
  color: white;
  border-color: #667eea;
}

.btn-primary:hover:not(:disabled) {
  background: #5a6fd8;
  border-color: #5a6fd8;
}

.btn-primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-secondary {
  background: #f3f4f6;
  color: #374151;
  border-color: #d1d5db;
}

.btn-secondary:hover {
  background: #e5e7eb;
}

.btn-outline {
  background: transparent;
  color: #667eea;
  border-color: #667eea;
}

.btn-outline:hover:not(:disabled) {
  background: #667eea;
  color: white;
}

.btn-outline:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.security-options {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.security-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem;
  border: 1px solid #e5e7eb;
  border-radius: 0.5rem;
}

.security-info h4 {
  font-size: 1rem;
  font-weight: 500;
  color: #111827;
  margin: 0 0 0.25rem 0;
}

.security-info p {
  font-size: 0.875rem;
  color: #6b7280;
  margin: 0;
}

.preferences-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.preference-group {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.preference-item {
  display: flex;
  align-items: center;
}

.switch-label {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  cursor: pointer;
}

.switch-input {
  display: none;
}

.switch-slider {
  position: relative;
  width: 44px;
  height: 24px;
  background: #d1d5db;
  border-radius: 24px;
  transition: all 0.2s;
}

.switch-slider::before {
  content: '';
  position: absolute;
  top: 2px;
  left: 2px;
  width: 20px;
  height: 20px;
  background: white;
  border-radius: 50%;
  transition: all 0.2s;
}

.switch-input:checked + .switch-slider {
  background: #667eea;
}

.switch-input:checked + .switch-slider::before {
  transform: translateX(20px);
}

.switch-text {
  font-weight: 500;
  color: #374151;
}

.language-preference {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.language-preference label {
  font-weight: 500;
  color: #374151;
  font-size: 0.875rem;
}

/* Modal Styles */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 50;
}

.modal-content {
  background: white;
  border-radius: 1rem;
  padding: 0;
  max-width: 500px;
  width: 90%;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1.5rem;
  border-bottom: 1px solid #e5e7eb;
}

.modal-header h3 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #111827;
  margin: 0;
}

.close-btn {
  padding: 0.5rem;
  color: #6b7280;
  cursor: pointer;
  border: none;
  background: none;
  border-radius: 0.25rem;
  transition: all 0.2s;
}

.close-btn:hover {
  color: #374151;
  background: #f3f4f6;
}

.modal-form {
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.modal-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  padding: 1.5rem;
  border-top: 1px solid #e5e7eb;
}

.avatar-upload {
  padding: 1.5rem;
}

.upload-area {
  margin-bottom: 1.5rem;
}

.upload-placeholder {
  border: 2px dashed #d1d5db;
  border-radius: 0.5rem;
  padding: 3rem 2rem;
  text-align: center;
  cursor: pointer;
  transition: all 0.2s;
}

.upload-placeholder:hover {
  border-color: #667eea;
  background: #f8faff;
}

.upload-placeholder p {
  margin: 0.5rem 0;
  color: #374151;
  font-weight: 500;
}

.upload-placeholder small {
  color: #6b7280;
}

@media (max-width: 768px) {
  .profile-management {
    padding: 1rem;
  }
  
  .profile-overview {
    flex-direction: column;
    text-align: center;
    gap: 1rem;
  }
  
  .form-grid {
    grid-template-columns: 1fr;
  }
  
  .security-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }
}
</style>