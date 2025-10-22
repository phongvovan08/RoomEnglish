<template>
  <div v-if="show" class="modal-overlay" @click="$emit('close')">
    <div class="modal-content" @click.stop>
      <div class="modal-header">
        <h3>Cập nhật ảnh đại diện</h3>
        <button @click="$emit('close')" class="close-btn">
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
          <div @click="fileInput?.click()" class="upload-placeholder">
            <Icon icon="mdi:cloud-upload" class="w-12 h-12" />
            <p>Click để chọn ảnh hoặc kéo thả vào đây</p>
            <small>Hỗ trợ: JPG, PNG (tối đa 2MB)</small>
          </div>
          <div v-if="selectedFile" class="file-preview">
            <Icon icon="mdi:file-image" class="w-6 h-6" />
            <span>{{ selectedFile.name }}</span>
            <button @click="clearFile" class="clear-file-btn">
              <Icon icon="mdi:close" class="w-4 h-4" />
            </button>
          </div>
        </div>
        
        <div class="modal-actions">
          <button type="button" @click="$emit('close')" class="btn-secondary">
            Hủy
          </button>
          <button 
            @click="handleUpload" 
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
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { Icon } from '@iconify/vue'
import { useToast } from '@/composables/useToast'

interface Props {
  show: boolean
  isLoading: boolean
}

defineProps<Props>()

const emit = defineEmits<{
  close: []
  upload: [file: File]
}>()

const { showWarning } = useToast()
const fileInput = ref<HTMLInputElement>()
const selectedFile = ref<File | null>(null)

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

const clearFile = () => {
  selectedFile.value = null
  if (fileInput.value) {
    fileInput.value.value = ''
  }
}

const handleUpload = () => {
  if (selectedFile.value) {
    emit('upload', selectedFile.value)
    clearFile()
  }
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

.file-preview {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem;
  background: #f3f4f6;
  border-radius: 0.5rem;
  margin-top: 1rem;
}

.file-preview span {
  flex: 1;
  font-size: 0.875rem;
  color: #374151;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.clear-file-btn {
  padding: 0.25rem;
  color: #6b7280;
  background: none;
  border: none;
  cursor: pointer;
  border-radius: 0.25rem;
  transition: all 0.2s;
}

.clear-file-btn:hover {
  color: #374151;
  background: #e5e7eb;
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
