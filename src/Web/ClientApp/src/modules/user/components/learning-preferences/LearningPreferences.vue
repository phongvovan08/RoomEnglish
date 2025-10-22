<template>
  <div class="form-section">
    <div class="section-header">
      <h3>
        <Icon icon="mdi:school" class="w-5 h-5" />
        Tùy chọn học tập
      </h3>
    </div>
    
    <form @submit.prevent="$emit('save')" class="preferences-form">
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
</template>

<script setup lang="ts">
import { Icon } from '@iconify/vue'
import type { Preferences } from '../../composables/useUserProfile'

interface Props {
  preferences: Preferences
  isLoading: boolean
}

defineProps<Props>()
defineEmits<{
  save: []
}>()
</script>

<style scoped>
.form-section {
  background: white;
  border-radius: 1rem;
  padding: 2rem;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
}

.section-header {
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

.form-select {
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  transition: all 0.2s;
}

.form-select:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.btn-primary {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  justify-content: center;
  padding: 0.75rem 1.5rem;
  border-radius: 0.5rem;
  font-weight: 500;
  font-size: 0.875rem;
  cursor: pointer;
  transition: all 0.2s;
  background: #667eea;
  color: white;
  border: 1px solid #667eea;
}

.btn-primary:hover:not(:disabled) {
  background: #5a6fd8;
  border-color: #5a6fd8;
}

.btn-primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
