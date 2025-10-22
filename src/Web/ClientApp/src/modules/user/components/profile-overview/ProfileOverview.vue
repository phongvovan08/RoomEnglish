<template>
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
        <button @click="$emit('edit-avatar')" class="avatar-edit-btn">
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
</template>

<script setup lang="ts">
import { Icon } from '@iconify/vue'
import type { UserProfile } from '../../composables/useUserProfile'

interface Props {
  userProfile: UserProfile
  formatDate: (date: string) => string
}

defineProps<Props>()
defineEmits<{
  'edit-avatar': []
}>()
</script>

<style scoped>
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

@media (max-width: 768px) {
  .profile-overview {
    flex-direction: column;
    text-align: center;
    gap: 1rem;
  }
}
</style>
