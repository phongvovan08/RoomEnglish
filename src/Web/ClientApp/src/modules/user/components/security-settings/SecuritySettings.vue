<template>
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
        <button @click="$emit('change-password')" class="btn-outline">
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
</template>

<script setup lang="ts">
import { Icon } from '@iconify/vue'

defineEmits<{
  'change-password': []
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

.btn-outline {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.5rem;
  border-radius: 0.5rem;
  font-weight: 500;
  font-size: 0.875rem;
  cursor: pointer;
  transition: all 0.2s;
  background: transparent;
  color: #667eea;
  border: 1px solid #667eea;
}

.btn-outline:hover:not(:disabled) {
  background: #667eea;
  color: white;
}

.btn-outline:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

@media (max-width: 768px) {
  .security-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }
}
</style>
