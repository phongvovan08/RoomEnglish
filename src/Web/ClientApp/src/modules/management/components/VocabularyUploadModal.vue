<template>
  <div class="vocabulary-upload-modal">
    <div class="modal-header">
      <h2>
        <Icon icon="mdi:upload" class="w-6 h-6 mr-3" />
        Upload Từ vựng từ Excel
      </h2>
      <button @click="$emit('close')" class="close-btn">
        <Icon icon="mdi:close" class="w-6 h-6" />
      </button>
    </div>

    <div class="modal-content">
      <!-- Upload Area -->
      <div 
        class="upload-area"
        :class="{ 'dragover': isDragover, 'uploading': isUploading }"
        @dragover.prevent="isDragover = true"
        @dragleave.prevent="isDragover = false"
        @drop.prevent="handleDrop"
      >
        <div v-if="!selectedFile && !isUploading" class="upload-placeholder">
          <Icon icon="mdi:cloud-upload" class="w-16 h-16 text-blue-400 mb-4" />
          <h3>Kéo thả file Excel hoặc click để chọn</h3>
          <p>Hỗ trợ file .xlsx, .xls</p>
          <input 
            ref="fileInput"
            type="file" 
            accept=".xlsx,.xls"
            @change="handleFileSelect"
            class="hidden"
          />
          <button @click="fileInput?.click()" class="select-file-btn">
            Chọn file Excel
          </button>
        </div>

        <div v-if="selectedFile && !isUploading" class="file-selected">
          <Icon icon="mdi:file-excel" class="w-12 h-12 text-green-400 mb-2" />
          <h4>{{ selectedFile.name }}</h4>
          <p>{{ formatFileSize(selectedFile.size) }}</p>
          <div class="file-actions">
            <button @click="clearFile" class="btn-secondary">
              <Icon icon="mdi:close" class="w-4 h-4 mr-1" />
              Hủy
            </button>
            <button @click="uploadFile" class="btn-primary">
              <Icon icon="mdi:upload" class="w-4 h-4 mr-1" />
              Upload
            </button>
          </div>
        </div>

        <div v-if="isUploading" class="uploading-state">
          <div class="loading-spinner"></div>
          <h3>Đang upload và xử lý file...</h3>
          <p>Vui lòng chờ trong giây lát</p>
          <div class="progress-bar">
            <div class="progress-fill" :style="{ width: uploadProgress + '%' }"></div>
          </div>
          <span class="progress-text">{{ uploadProgress }}%</span>
        </div>
      </div>

      <!-- Format Instructions -->
      <div class="format-instructions">
        <h3>
          <Icon icon="mdi:information" class="w-5 h-5 mr-2" />
          Định dạng file Excel
        </h3>
        <div class="columns-info">
          <div class="column-item required">
            <span class="column-name">Word</span>
            <span class="column-desc">Từ vựng (bắt buộc)</span>
          </div>
          <div class="column-item required">
            <span class="column-name">Definition</span>
            <span class="column-desc">Định nghĩa (bắt buộc)</span>
          </div>
          <div class="column-item optional">
            <span class="column-name">Pronunciation</span>
            <span class="column-desc">Phát âm (tùy chọn)</span>
          </div>
        </div>
      </div>

      <!-- Results -->
      <div v-if="uploadResults" class="upload-results">
        <div class="results-header">
          <Icon 
            :icon="uploadResults.success ? 'mdi:check-circle' : 'mdi:alert-circle'" 
            :class="uploadResults.success ? 'text-green-400' : 'text-red-400'"
            class="w-6 h-6 mr-2"
          />
          <h3>{{ uploadResults.success ? 'Upload thành công!' : 'Upload thất bại!' }}</h3>
        </div>
        
        <div v-if="uploadResults.success" class="success-stats">
          <div class="stat">
            <span class="stat-label">Đã thêm:</span>
            <span class="stat-value">{{ uploadResults.addedCount }} từ</span>
          </div>
          <div class="stat">
            <span class="stat-label">Đã cập nhật:</span>
            <span class="stat-value">{{ uploadResults.updatedCount }} từ</span>
          </div>
          <div class="stat">
            <span class="stat-label">Lỗi:</span>
            <span class="stat-value">{{ uploadResults.errors?.length || 0 }} từ</span>
          </div>
        </div>
        
        <div v-if="uploadResults.errors && uploadResults.errors.length > 0" class="error-list">
          <h4>Các lỗi gặp phải:</h4>
          <ul>
            <li v-for="(error, index) in uploadResults.errors.slice(0, 5)" :key="index">
              {{ error }}
            </li>
            <li v-if="uploadResults.errors.length > 5">
              ... và {{ uploadResults.errors.length - 5 }} lỗi khác
            </li>
          </ul>
        </div>

        <div class="results-actions">
          <button @click="$emit('close')" class="btn-secondary">
            Đóng
          </button>
          <button v-if="uploadResults.success" @click="$emit('success', uploadResults)" class="btn-primary">
            Hoàn tất
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { Icon } from '@iconify/vue'
import { createAuthHeaders } from '@/utils/auth'
import { useNotifications } from '@/utils/notifications'

interface Props {
  categoryId: number
}

interface UploadResults {
  success: boolean
  addedCount: number
  updatedCount: number
  errors: string[]
}

const props = defineProps<Props>()
const emit = defineEmits<{
  close: []
  success: [results: UploadResults]
}>()

const { showSuccess, showError } = useNotifications()

const fileInput = ref<HTMLInputElement>()
const isDragover = ref(false)
const selectedFile = ref<File | null>(null)
const isUploading = ref(false)
const uploadProgress = ref(0)
const uploadResults = ref<UploadResults | null>(null)

const handleDrop = (event: DragEvent) => {
  isDragover.value = false
  const files = event.dataTransfer?.files
  if (files && files.length > 0) {
    selectFile(files[0])
  }
}

const handleFileSelect = (event: Event) => {
  const target = event.target as HTMLInputElement
  if (target.files && target.files.length > 0) {
    selectFile(target.files[0])
  }
}

const selectFile = (file: File) => {
  if (file.type !== 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' && 
      file.type !== 'application/vnd.ms-excel') {
    alert('Vui lòng chọn file Excel (.xlsx hoặc .xls)')
    return
  }
  
  selectedFile.value = file
  uploadResults.value = null
}

const clearFile = () => {
  selectedFile.value = null
  uploadResults.value = null
}

const uploadFile = async () => {
  if (!selectedFile.value) return
  
  isUploading.value = true
  uploadProgress.value = 0
  
  // Simulate progress
  const progressInterval = setInterval(() => {
    if (uploadProgress.value < 90) {
      uploadProgress.value += Math.random() * 20
    }
  }, 200)
  
  try {
    const formData = new FormData()
    formData.append('file', selectedFile.value)
    formData.append('categoryId', props.categoryId.toString())
    
    const response = await fetch('/api/vocabulary-learning/upload-excel', {
      method: 'POST',
      headers: createAuthHeaders(),
      body: formData
    })
    
    clearInterval(progressInterval)
    uploadProgress.value = 100
    
    if (response.ok) {
      const result = await response.json()
      uploadResults.value = {
        success: true,
        addedCount: result.addedCount || 0,
        updatedCount: result.updatedCount || 0,
        errors: result.errors || []
      }
    } else {
      const error = await response.text()
      uploadResults.value = {
        success: false,
        addedCount: 0,
        updatedCount: 0,
        errors: [error || 'Upload thất bại']
      }
    }
  } catch (error) {
    clearInterval(progressInterval)
    uploadResults.value = {
      success: false,
      addedCount: 0,
      updatedCount: 0,
      errors: ['Lỗi kết nối: ' + (error as Error).message]
    }
  } finally {
    isUploading.value = false
  }
}

const formatFileSize = (bytes: number): string => {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}
</script>

<style scoped>
.vocabulary-upload-modal {
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
  border-radius: 20px;
  border: 1px solid rgba(255, 255, 255, 0.1);
  max-width: 600px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
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
  display: flex;
  align-items: center;
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

.modal-content {
  padding: 1.5rem;
}

.upload-area {
  border: 2px dashed rgba(255, 255, 255, 0.3);
  border-radius: 12px;
  padding: 3rem 2rem;
  text-align: center;
  transition: all 0.3s ease;
  margin-bottom: 2rem;
}

.upload-area.dragover {
  border-color: #74c0fc;
  background: rgba(116, 192, 252, 0.1);
}

.upload-area.uploading {
  border-color: #51cf66;
  background: rgba(81, 207, 102, 0.1);
}

.upload-placeholder h3 {
  color: white;
  margin-bottom: 0.5rem;
}

.upload-placeholder p {
  color: rgba(255, 255, 255, 0.6);
  margin-bottom: 1.5rem;
}

.select-file-btn {
  background: linear-gradient(135deg, #74c0fc, #339af0);
  color: white;
  border: none;
  padding: 0.75rem 2rem;
  border-radius: 12px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.select-file-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(116, 192, 252, 0.4);
}

.hidden {
  display: none;
}

.file-selected {
  color: white;
}

.file-selected h4 {
  margin: 0.5rem 0 0.25rem 0;
  color: #51cf66;
}

.file-selected p {
  margin: 0 0 1.5rem 0;
  color: rgba(255, 255, 255, 0.6);
}

.file-actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
}

.btn-primary {
  background: linear-gradient(135deg, #74c0fc, #339af0);
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: all 0.3s ease;
}

.btn-primary:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 15px rgba(116, 192, 252, 0.4);
}

.btn-secondary {
  background: rgba(255, 255, 255, 0.1);
  color: white;
  border: 1px solid rgba(255, 255, 255, 0.2);
  padding: 0.5rem 1rem;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: all 0.3s ease;
}

.btn-secondary:hover {
  background: rgba(255, 255, 255, 0.2);
}

.uploading-state {
  color: white;
}

.uploading-state h3 {
  margin-bottom: 0.5rem;
}

.uploading-state p {
  color: rgba(255, 255, 255, 0.6);
  margin-bottom: 1.5rem;
}

.loading-spinner {
  width: 3rem;
  height: 3rem;
  margin: 0 auto 1rem;
  border: 3px solid rgba(255, 255, 255, 0.3);
  border-top: 3px solid #74c0fc;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.progress-bar {
  width: 100%;
  height: 8px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 4px;
  overflow: hidden;
  margin-bottom: 0.5rem;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, #74c0fc, #339af0);
  transition: width 0.3s ease;
}

.progress-text {
  color: #74c0fc;
  font-weight: 600;
}

.format-instructions {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 12px;
  padding: 1.5rem;
  margin-bottom: 2rem;
}

.format-instructions h3 {
  color: white;
  margin: 0 0 1rem 0;
  display: flex;
  align-items: center;
}

.columns-info {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.column-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem;
  border-radius: 8px;
  border-left: 4px solid;
}

.column-item.required {
  background: rgba(220, 53, 69, 0.1);
  border-left-color: #dc3545;
}

.column-item.optional {
  background: rgba(255, 193, 7, 0.1);
  border-left-color: #ffc107;
}

.column-name {
  color: white;
  font-weight: 600;
}

.column-desc {
  color: rgba(255, 255, 255, 0.7);
  font-size: 0.875rem;
}

.upload-results {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 12px;
  padding: 1.5rem;
}

.results-header {
  display: flex;
  align-items: center;
  margin-bottom: 1rem;
}

.results-header h3 {
  color: white;
  margin: 0;
}

.text-green-400 {
  color: #51cf66;
}

.text-red-400 {
  color: #ff6b6b;
}

.success-stats {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  margin-bottom: 1rem;
}

.stat {
  background: rgba(255, 255, 255, 0.1);
  padding: 0.75rem 1rem;
  border-radius: 8px;
  display: flex;
  flex-direction: column;
  align-items: center;
  min-width: 100px;
}

.stat-label {
  color: rgba(255, 255, 255, 0.7);
  font-size: 0.875rem;
  margin-bottom: 0.25rem;
}

.stat-value {
  color: white;
  font-weight: 600;
  font-size: 1.25rem;
}

.error-list {
  margin: 1rem 0;
}

.error-list h4 {
  color: #ff6b6b;
  margin: 0 0 0.5rem 0;
}

.error-list ul {
  color: rgba(255, 255, 255, 0.8);
  margin: 0;
  padding-left: 1.5rem;
}

.error-list li {
  margin-bottom: 0.25rem;
}

.results-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  margin-top: 1.5rem;
}

/* Mobile Responsiveness */
@media (max-width: 768px) {
  .vocabulary-upload-modal {
    margin: 1rem;
    width: calc(100% - 2rem);
    max-width: none;
  }
  
  .upload-area {
    padding: 2rem 1rem;
  }
  
  .success-stats {
    justify-content: center;
  }
  
  .file-actions,
  .results-actions {
    flex-direction: column;
  }
}
</style>