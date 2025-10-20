<template>
  <div class="form-section">
    <div class="section-header">
      <h3>
        <Icon icon="mdi:account-edit" class="w-5 h-5" />
        Thông tin cá nhân
      </h3>
      <button 
        v-if="!isEditing"
        @click="startEdit"
        class="btn-edit"
      >
        <Icon icon="mdi:pencil" class="w-4 h-4" />
        Chỉnh sửa
      </button>
    </div>
    
    <form @submit.prevent="handleSubmit" class="profile-form">
      <div class="form-grid">
        <div class="form-group">
          <label for="firstName">Họ</label>
          <input 
            id="firstName"
            v-model="personalForm.firstName"
            type="text"
            :readonly="!isEditing"
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
            :readonly="!isEditing"
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
            :readonly="!isEditing"
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
            :readonly="!isEditing"
            class="form-input"
            placeholder="Nhập số điện thoại"
          />
        </div>
        
        <div class="form-group full-width">
          <label for="bio">Giới thiệu bản thân</label>
          <textarea 
            id="bio"
            v-model="personalForm.bio"
            :readonly="!isEditing"
            class="form-textarea"
            placeholder="Giới thiệu ngắn về bản thân..."
            rows="3"
          ></textarea>
        </div>
      </div>
      
      <div v-if="isEditing" class="form-actions">
        <button type="button" @click="cancelEdit" class="btn-secondary">
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
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { Icon } from '@iconify/vue'
import type { PersonalForm, UserProfile } from '../composables/useUserProfile'

interface Props {
  personalForm: PersonalForm
  userProfile: UserProfile
  isLoading: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  save: []
}>()

const isEditing = ref(false)
const originalForm = ref<PersonalForm>({} as PersonalForm)

const startEdit = () => {
  // Save original values for cancel
  originalForm.value = { ...props.personalForm }
  isEditing.value = true
}

const cancelEdit = () => {
  // Restore original values
  Object.assign(props.personalForm, originalForm.value)
  isEditing.value = false
}

const handleSubmit = async () => {
  emit('save')
  isEditing.value = false
}
</script>

<style scoped>
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

.form-input, .form-textarea {
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  transition: all 0.2s;
}

.form-input:focus, .form-textarea:focus {
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

.btn-primary, .btn-secondary {
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

@media (max-width: 768px) {
  .form-grid {
    grid-template-columns: 1fr;
  }
}
</style>
