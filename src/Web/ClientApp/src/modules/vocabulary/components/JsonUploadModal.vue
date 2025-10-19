<template>
  <div v-if="isOpen" class="modal-overlay" @click="closeModal">
    <div class="modal-container" @click.stop>
      <div class="modal-header">
        <h3>
          <Icon icon="mdi:code-json" class="w-6 h-6 mr-2" />
          Import từ vựng từ JSON
        </h3>
        <button @click="closeModal" class="close-button">
          <Icon icon="mdi:close" class="w-5 h-5" />
        </button>
      </div>

      <div class="modal-content">
        <div class="upload-section">
         

          <div class="upload-area">
            <div class="upload-methods">
              <!-- File Upload -->
              <div class="upload-method">
                <label class="upload-label" :class="{ 'has-file': selectedFile }">
                  <Icon :icon="selectedFile ? 'mdi:file-check' : 'mdi:file-upload'" class="w-8 h-8 mb-2" />
                  <span v-if="!selectedFile">Chọn file JSON</span>
                  <div v-else class="selected-file-display">
                    <span class="file-name">{{ selectedFile.name }}</span>
                    <span class="file-size">({{ formatFileSize(selectedFile.size) }})</span>
                    <small class="change-file">Nhấn để thay đổi file</small>
                  </div>
                  <input 
                    type="file" 
                    accept=".json"
                    @change="handleFileSelect"
                    class="hidden"
                  />
                </label>
              </div>
 <div class="template-actions">
            <button @click="downloadTemplate" class="template-button">
              <Icon icon="mdi:download" class="w-4 h-4 mr-2" />
              Tải mẫu JSON
            </button>
          </div>
              <div class="divider">HOẶC</div>

              <!-- Direct JSON Input -->
              <div class="upload-method">
                <label class="json-input-label">
                  <Icon icon="mdi:code-braces" class="w-5 h-5 mr-2" />
                  Nhập JSON trực tiếp:
                </label>
                <textarea
                  v-model="jsonInput"
                  placeholder="Nhập dữ liệu JSON vào đây..."
                  class="json-textarea"
                  rows="10"
                ></textarea>
              </div>
            </div>
          </div>

          <div v-if="jsonInput" class="file-info">
            <div class="json-info">
              <Icon icon="mdi:code-json" class="w-5 h-5 mr-2" />
              <span>JSON data ready ({{ jsonInput.length }} characters)</span>
            </div>
          </div>

          <div v-if="validationErrors.length > 0" class="validation-errors">
            <div class="error-header">
              <Icon icon="mdi:alert-circle" class="w-5 h-5 mr-2" />
              Lỗi validation:
            </div>
            <ul>
              <li v-for="error in validationErrors" :key="error">{{ error }}</li>
            </ul>
          </div>
        </div>
      </div>

      <div class="modal-footer">
        <button @click="closeModal" class="cancel-button">
          Hủy
        </button>
        <button 
          @click="handleUpload" 
          :disabled="(!selectedFile && !jsonInput) || isUploading"
          class="upload-button"
        >
          <Icon 
            :icon="isUploading ? 'mdi:loading' : 'mdi:upload'" 
            :class="['w-4 h-4 mr-2', { 'animate-spin': isUploading }]" 
          />
          {{ isUploading ? 'Đang import...' : 'Import JSON' }}
        </button>
      </div>

      <!-- Success Message with Countdown -->
      <div v-if="showSuccess" class="success-overlay">
        <div class="success-message">
          <Icon icon="mdi:check-circle" class="w-8 h-8 text-green-500 mb-2" />
          <h4>Import thành công!</h4>
          <p>{{ successMessage }}</p>
          <p class="countdown">Tự động đóng sau {{ countdown }}s</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { Icon } from '@iconify/vue'

interface Props {
  isOpen: boolean
}

interface Emits {
  (e: 'close'): void
  (e: 'upload-success'): void
  (e: 'download-template'): void
  (e: 'import-json', jsonData: string): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const selectedFile = ref<File | null>(null)
const jsonInput = ref('')
const validationErrors = ref<string[]>([])
const isUploading = ref(false)
const showSuccess = ref(false)
const successMessage = ref('')
const countdown = ref(3)

const handleFileSelect = (event: Event) => {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  
  if (file) {
    selectedFile.value = file
    jsonInput.value = '' // Clear direct input when file is selected
    validateFile(file)
  }
}

const validateFile = (file: File) => {
  validationErrors.value = []
  
  if (!file.type.includes('json') && !file.name.endsWith('.json')) {
    validationErrors.value.push('File phải có định dạng JSON')
  }
  
  if (file.size > 5 * 1024 * 1024) { // 5MB limit
    validationErrors.value.push('File không được vượt quá 5MB')
  }
}

const validateJson = (jsonString: string): boolean => {
  validationErrors.value = []
  
  try {
    const parsed = JSON.parse(jsonString)
    
    if (!Array.isArray(parsed)) {
      validationErrors.value.push('JSON phải là một mảng các object từ vựng')
      return false
    }
    
    if (parsed.length === 0) {
      validationErrors.value.push('JSON không được rỗng')
      return false
    }
    
    // Basic validation of required fields
    for (let i = 0; i < parsed.length; i++) {
      const item = parsed[i]
      if (!item.word || !item.meaning) {
        validationErrors.value.push(`Item ${i + 1}: Thiếu trường word hoặc meaning`)
      }
    }
    
    return validationErrors.value.length === 0
  } catch (error) {
    validationErrors.value.push('JSON không hợp lệ: ' + (error as Error).message)
    return false
  }
}

const handleUpload = async () => {
  let jsonData = ''
  
  if (selectedFile.value) {
    // Read file content
    try {
      const fileContent = await readFileAsText(selectedFile.value)
      jsonData = fileContent
    } catch (error) {
      validationErrors.value = ['Không thể đọc file: ' + (error as Error).message]
      return
    }
  } else if (jsonInput.value) {
    jsonData = jsonInput.value
  }
  
  if (!validateJson(jsonData)) {
    return
  }
  
  isUploading.value = true
  
  try {
    emit('import-json', jsonData)
  } finally {
    isUploading.value = false
  }
}

const readFileAsText = (file: File): Promise<string> => {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload = (e) => resolve(e.target?.result as string)
    reader.onerror = (e) => reject(new Error('Failed to read file'))
    reader.readAsText(file)
  })
}

const downloadTemplate = () => {
  emit('download-template')
}

const closeModal = () => {
  if (!isUploading.value) {
    emit('close')
  }
}

const formatFileSize = (bytes: number): string => {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

const startCountdown = () => {
  const timer = setInterval(() => {
    countdown.value--
    if (countdown.value <= 0) {
      clearInterval(timer)
      showSuccess.value = false
      emit('close')
      emit('upload-success')
    }
  }, 1000)
}

// Watch for successful upload (you would call this from parent)
const showSuccessMessage = (message: string) => {
  successMessage.value = message
  showSuccess.value = true
  countdown.value = 3
  startCountdown()
}

// Reset form when modal closes
watch(() => props.isOpen, (newValue) => {
  if (!newValue) {
    selectedFile.value = null
    jsonInput.value = ''
    validationErrors.value = []
    showSuccess.value = false
  }
})

// Expose method for parent to show success
defineExpose({
  showSuccessMessage
})
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 50;
}

.modal-container {
  background: white;
  border-radius: 8px;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
  width: 100%;
  max-width: 672px;
  margin: 0 16px;
  max-height: 90vh;
  overflow: hidden;
  position: relative;
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 24px;
  border-bottom: 1px solid #e5e7eb;
}

.modal-header h3 {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1f2937;
  display: flex;
  align-items: center;
}

.close-button {
  padding: 4px;
  border-radius: 9999px;
  border: none;
  background: transparent;
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.close-button:hover {
  background-color: #f3f4f6;
}

.modal-content {
  padding: 24px;
  overflow-y: auto;
  max-height: 60vh;
}

.template-actions {
  display: flex;
  justify-content: center;
}

.template-button {
  display: flex;
  align-items: center;
  padding: 8px 16px;
  background-color: #dbeafe;
  color: #1d4ed8;
  border-radius: 8px;
  border: none;
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.template-button:hover {
  background-color: #bfdbfe;
}

.upload-area {
  border: 2px dashed #d1d5db;
  border-radius: 8px;
  padding: 24px;
}

.upload-methods {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.upload-method {
  text-align: center;
}

.upload-label {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  padding: 24px;
  color: #1f2937;
  border-radius: 8px;
  border: 1px solid #e5e7eb;
  transition: all 0.3s ease;
}

.upload-label:hover {
  border-color: #60a5fa;
  background-color: #eff6ff;
}

.upload-label span {
  color: #4b5563;
  font-weight: 500;
}

.upload-label input {
  display: none;
}

.upload-label.has-file {
  border-color: #10b981;
  background-color: #ecfdf5;
}

.selected-file-display {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
}

.file-name {
  color: #059669;
  font-weight: 600;
  font-size: 0.875rem;
}

.file-size {
  color: #6b7280;
  font-size: 0.75rem;
}

.change-file {
  color: #9ca3af;
  font-size: 0.75rem;
  margin-top: 4px;
}

.divider {
  text-align: center;
  color: #9ca3af;
  font-weight: 500;
  padding: 8px 0;
}

.json-input-label {
  display: flex;
  align-items: center;
  font-size: 0.875rem;
  font-weight: 500;
  color: #374151;
  margin-bottom: 8px;
}

.json-textarea {
  width: 100%;
  padding: 12px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-family: 'Courier New', monospace;
  font-size: 0.875rem;
  resize: vertical;
  min-height: 120px;
}

.json-textarea:focus {
  outline: none;
  box-shadow: 0 0 0 2px #3b82f6;
  border-color: transparent;
}

.file-info {
  margin-top: 16px;
  padding: 12px;
  background-color: #f9fafb;
  border-radius: 8px;
}

.selected-file, .json-info {
  display: flex;
  align-items: center;
  font-size: 0.875rem;
  color: #4b5563;
}

.file-size {
  color: #9ca3af;
  margin-left: 8px;
}

.validation-errors {
  margin-top: 16px;
  padding: 12px;
  background-color: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 8px;
}

.error-header {
  display: flex;
  align-items: center;
  color: #b91c1c;
  font-weight: 500;
  margin-bottom: 8px;
}

.validation-errors ul {
  list-style-type: disc;
  list-style-position: inside;
  color: #dc2626;
  font-size: 0.875rem;
}

.validation-errors li {
  margin-bottom: 4px;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 24px;
  border-top: 1px solid #e5e7eb;
}

.cancel-button {
  padding: 8px 16px;
  color: #4b5563;
  background-color: #f3f4f6;
  border-radius: 8px;
  border: none;
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.cancel-button:hover {
  background-color: #e5e7eb;
}

.upload-button {
  display: flex;
  align-items: center;
  padding: 8px 16px;
  background-color: #2563eb;
  color: white;
  border-radius: 8px;
  border: none;
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.upload-button:hover:not(:disabled) {
  background-color: #1d4ed8;
}

.upload-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.success-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(255, 255, 255, 0.95);
  display: flex;
  align-items: center;
  justify-content: center;
}

.success-message {
  text-align: center;
  padding: 24px;
}

.success-message h4 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1f2937;
  margin-bottom: 8px;
}

.success-message p {
  color: #4b5563;
  margin-bottom: 8px;
}

.countdown {
  font-size: 0.875rem;
  color: #6b7280;
}
</style>