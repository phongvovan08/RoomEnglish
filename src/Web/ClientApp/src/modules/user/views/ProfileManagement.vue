<template>
  <div class="profile-management">
    <div class="profile-header">
      <h1 class="profile-title">
        <i class="bi bi-person-circle"></i>
        Quản Lý Hồ Sơ
      </h1>
      <p class="profile-subtitle">Cập nhật thông tin cá nhân và cài đặt tài khoản</p>
    </div>

    <div v-if="isLoading" class="loading-spinner">
      <div class="spinner"></div>
      <p>Đang tải thông tin...</p>
    </div>

    <div v-else class="profile-content">
      <ProfileOverview 
        :user-profile="userProfile"
        :format-date="formatDate"
        @edit-avatar="showAvatarModal = true"
      />

      <PersonalInfoForm 
        v-model:personal-form="personalForm"
        :user-profile="userProfile"
        :is-loading="isLoading"
        @save="savePersonalInfo"
      />

      <SecuritySettings 
        :user-profile="userProfile"
        @change-password="showPasswordModal = true"
      />

      <LearningPreferences 
        v-model:preferences="preferences"
        :is-loading="isLoading"
        @save="savePreferences"
      />
    </div>

    <ChangePasswordModal 
      v-model:show="showPasswordModal"
      :password-form="passwordForm"
      :is-loading="isLoading"
      @submit="changePassword"
    />

    <AvatarUploadModal 
      v-model:show="showAvatarModal"
      :is-loading="isLoading"
      @upload="uploadAvatar"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useUserProfile } from '../composables/useUserProfile'
import ProfileOverview from '../components/profile-overview/ProfileOverview.vue'
import PersonalInfoForm from '../components/personal-info-form/PersonalInfoForm.vue'
import SecuritySettings from '../components/security-settings/SecuritySettings.vue'
import LearningPreferences from '../components/learning-preferences/LearningPreferences.vue'
import ChangePasswordModal from '../components/change-password-modal/ChangePasswordModal.vue'
import AvatarUploadModal from '../components/avatar-upload-modal/AvatarUploadModal.vue'

const {
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
} = useUserProfile()

const showPasswordModal = ref(false)
const showAvatarModal = ref(false)

onMounted(() => {
  loadUserProfile()
})
</script>

<style scoped>
.profile-management {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}

.profile-header {
  text-align: center;
  margin-bottom: 3rem;
  padding-bottom: 2rem;
  border-bottom: 2px solid rgba(59, 130, 246, 0.3);
}

.profile-title {
  font-size: 2.5rem;
  font-weight: 700;
  background: linear-gradient(135deg, #3b82f6 0%, #8b5cf6 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin-bottom: 0.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
}

.profile-title i {
  -webkit-text-fill-color: #3b82f6;
}

.profile-subtitle {
  font-size: 1.1rem;
  color: #9ca3af;
}

.loading-spinner {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 4rem 0;
  gap: 1rem;
}

.spinner {
  width: 50px;
  height: 50px;
  border: 4px solid rgba(59, 130, 246, 0.2);
  border-top-color: #3b82f6;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.profile-content {
  display: grid;
  gap: 2rem;
}

@media (max-width: 768px) {
  .profile-management {
    padding: 1rem;
  }

  .profile-title {
    font-size: 2rem;
  }

  .profile-content {
    gap: 1.5rem;
  }
}
</style>
