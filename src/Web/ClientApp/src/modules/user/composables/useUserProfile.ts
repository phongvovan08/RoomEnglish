import { ref, reactive } from 'vue'
import { AuthService } from '@/services/authService'
import { useToast } from '@/composables/useToast'
import type { UserProfile, PersonalForm, PasswordForm, Preferences } from '../types/user.types'

// Re-export types for convenience
export type { UserProfile, PersonalForm, PasswordForm, Preferences }

export function useUserProfile() {
  const { showSuccess, showError, showWarning } = useToast()
  const isLoading = ref(false)
  
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

  const loadUserProfile = async () => {
    try {
      isLoading.value = true
      const token = AuthService.getToken()
      
      if (!token) {
        showError('Chưa đăng nhập', 'Vui lòng đăng nhập để xem profile')
        return
      }
      
      const response = await fetch('/api/users/me', {
        headers: {
          'Authorization': `Bearer ${token}`,
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

  const savePersonalInfo = async () => {
    try {
      isLoading.value = true
      const token = AuthService.getToken()
      
      if (!token) {
        showError('Chưa đăng nhập', 'Vui lòng đăng nhập để cập nhật profile')
        return false
      }
      
      const response = await fetch('/api/users/me', {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(personalForm)
      })
      
      if (response.ok) {
        const updatedProfile = await response.json()
        userProfile.value = { ...userProfile.value, ...updatedProfile }
        showSuccess('Cập nhật thành công!', 'Thông tin cá nhân đã được cập nhật')
        return true
      } else {
        showError('Lỗi cập nhật', 'Không thể cập nhật thông tin')
        return false
      }
    } catch (error) {
      showError('Lỗi kết nối', 'Không thể kết nối đến máy chủ')
      console.error('Failed to save personal info:', error)
      return false
    } finally {
      isLoading.value = false
    }
  }

  const changePassword = async () => {
    if (passwordForm.newPassword !== passwordForm.confirmPassword) {
      showWarning('Mật khẩu không khớp', 'Mật khẩu mới và xác nhận mật khẩu không giống nhau')
      return false
    }
    
    try {
      isLoading.value = true
      const token = AuthService.getToken()
      
      if (!token) {
        showError('Chưa đăng nhập', 'Vui lòng đăng nhập để đổi mật khẩu')
        return false
      }
      
      const response = await fetch('/api/users/change-password', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          currentPassword: passwordForm.currentPassword,
          newPassword: passwordForm.newPassword
        })
      })
      
      if (response.ok) {
        Object.assign(passwordForm, {
          currentPassword: '',
          newPassword: '',
          confirmPassword: ''
        })
        showSuccess('Đổi mật khẩu thành công!', 'Mật khẩu của bạn đã được cập nhật')
        return true
      } else {
        const error = await response.text()
        showError('Lỗi đổi mật khẩu', error || 'Không thể đổi mật khẩu')
        return false
      }
    } catch (error) {
      showError('Lỗi kết nối', 'Không thể kết nối đến máy chủ')
      console.error('Failed to change password:', error)
      return false
    } finally {
      isLoading.value = false
    }
  }

  const savePreferences = async () => {
    try {
      isLoading.value = true
      const token = AuthService.getToken()
      
      if (!token) {
        showError('Chưa đăng nhập', 'Vui lòng đăng nhập để lưu tùy chọn')
        return false
      }
      
      const response = await fetch('/api/users/preferences', {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(preferences)
      })
      
      if (response.ok) {
        showSuccess('Lưu tùy chọn thành công!', 'Các tùy chọn học tập đã được cập nhật')
        return true
      } else {
        showError('Lỗi lưu tùy chọn', 'Không thể lưu tùy chọn')
        return false
      }
    } catch (error) {
      showError('Lỗi kết nối', 'Không thể kết nối đến máy chủ')
      console.error('Failed to save preferences:', error)
      return false
    } finally {
      isLoading.value = false
    }
  }

  const uploadAvatar = async (file: File) => {
    try {
      isLoading.value = true
      const token = AuthService.getToken()
      
      if (!token) {
        showError('Chưa đăng nhập', 'Vui lòng đăng nhập để tải ảnh đại diện')
        return false
      }
      
      const formData = new FormData()
      formData.append('avatar', file)
      
      const response = await fetch('/api/users/avatar', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`
        },
        body: formData
      })
      
      if (response.ok) {
        const result = await response.json()
        userProfile.value.avatar = result.avatarUrl
        showSuccess('Cập nhật ảnh đại diện thành công!')
        return true
      } else {
        showError('Lỗi tải ảnh', 'Không thể tải lên ảnh đại diện')
        return false
      }
    } catch (error) {
      showError('Lỗi kết nối', 'Không thể kết nối đến máy chủ')
      console.error('Failed to upload avatar:', error)
      return false
    } finally {
      isLoading.value = false
    }
  }

  const formatDate = (dateString: string): string => {
    if (!dateString) return ''
    return new Date(dateString).toLocaleDateString('vi-VN')
  }

  return {
    userProfile,
    personalForm,
    passwordForm,
    preferences,
    isLoading,
    loadUserProfile,
    savePersonalInfo,
    changePassword,
    savePreferences,
    uploadAvatar,
    formatDate
  }
}
