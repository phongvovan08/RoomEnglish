<template>
  <div v-if="show" class="modal-overlay" @click="$emit('close')">
    <div class="modal-content" @click.stop>
      <div class="modal-header">
        <h3>Đổi mật khẩu</h3>
        <button @click="$emit('close')" class="close-btn">
          <Icon icon="mdi:close" class="w-5 h-5" />
        </button>
      </div>
      
      <form @submit.prevent="handleSubmit" class="modal-form">
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
          <button type="button" @click="$emit('close')" class="btn-secondary">
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
</template>

<script setup lang="ts">
import { Icon } from '@iconify/vue'
import type { PasswordForm } from '../composables/useUserProfile'

interface Props {
  show: boolean
  passwordForm: PasswordForm
  isLoading: boolean
}

defineProps<Props>()

const emit = defineEmits<{
  close: []
  submit: []
}>()

const handleSubmit = () => {
  emit('submit')
}
</script>

<style scoped>
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

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-group label {
  font-weight: 500;
  color: #374151;
  font-size: 0.875rem;
}

.form-input {
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  transition: all 0.2s;
}

.form-input:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.modal-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  padding: 1.5rem;
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
</style>
