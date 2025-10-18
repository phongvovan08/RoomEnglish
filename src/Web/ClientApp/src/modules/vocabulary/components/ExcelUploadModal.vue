<template>
  <div class="excel-upload-container">
    <div class="upload-card">
      <div class="upload-header">
        <h2>
          <i class="mdi mdi-file-excel"></i>
          Import Vocabulary from Excel
        </h2>
        <p>Upload an Excel file containing vocabulary words and examples</p>
      </div>

      <div class="upload-section">
        <!-- File Upload Area -->
        <div 
          class="upload-dropzone" 
          :class="{ 'drag-over': isDragOver, 'uploading': isUploading }"
          @drop="handleDrop"
          @dragover="handleDragOver"
          @dragenter="handleDragEnter"
          @dragleave="handleDragLeave"
          @click="triggerFileInput"
        >
          <input 
            ref="fileInput" 
            type="file" 
            accept=".xlsx,.xls" 
            @change="handleFileSelect" 
            style="display: none"
          />
          
          <div v-if="!selectedFile && !isUploading" class="upload-prompt">
            <i class="mdi mdi-cloud-upload upload-icon"></i>
            <h3>Drop your Excel file here</h3>
            <p>or <span class="upload-link">click to browse</span></p>
            <div class="file-requirements">
              <small>Supported formats: .xlsx, .xls</small>
            </div>
          </div>

          <div v-if="selectedFile && !isUploading" class="selected-file">
            <i class="mdi mdi-file-excel file-icon"></i>
            <div class="file-info">
              <h4>{{ selectedFile.name }}</h4>
              <p>{{ formatFileSize(selectedFile.size) }}</p>
            </div>
            <button @click.stop="removeFile" class="remove-file-btn">
              <i class="mdi mdi-close"></i>
            </button>
          </div>

          <div v-if="isUploading" class="uploading-state">
            <div class="upload-spinner"></div>
            <h3>Processing Excel file...</h3>
            <p>Please wait while we import your vocabulary data</p>
          </div>
        </div>

        <!-- Action Buttons -->
        <div class="upload-actions" v-if="selectedFile && !isUploading">
          <button @click="uploadFile" class="upload-btn" :disabled="!selectedFile">
            <i class="mdi mdi-upload"></i>
            Import Vocabulary
          </button>
          <button @click="downloadTemplate" class="template-btn">
            <i class="mdi mdi-download"></i>
            Download Template
          </button>
        </div>
      </div>

      <!-- Results Section -->
      <div v-if="uploadResult" class="results-section">
        <div class="result-header" :class="{ 'success': uploadResult.success, 'error': !uploadResult.success }">
          <i class="mdi" :class="uploadResult.success ? 'mdi-check-circle' : 'mdi-alert-circle'"></i>
          <h3>{{ uploadResult.success ? 'Import Completed' : 'Import Failed' }}</h3>
        </div>

        <div v-if="uploadResult.success" class="success-stats">
          <div class="stat-item">
            <span class="stat-number">{{ uploadResult.importedWords }}</span>
            <span class="stat-label">Words Imported</span>
          </div>
          <div class="stat-item">
            <span class="stat-number">{{ uploadResult.importedExamples }}</span>
            <span class="stat-label">Examples Imported</span>
          </div>
        </div>



        <div class="result-actions">
          <button @click="resetUpload" class="reset-btn">
            <i class="mdi mdi-refresh"></i>
            Upload Another File
          </button>
          <button @click="$emit('close')" class="close-btn">
            <i class="mdi mdi-close"></i>
            Close
          </button>
        </div>
      </div>
    </div>

    <!-- Excel Format Guide -->
    <div class="format-guide">
      <h3>📋 Excel Format Guide</h3>
      <p>Your Excel file should contain the following columns:</p>
      <div class="columns-grid">
        <div class="column-item required">
          <strong>Word</strong> - The English word
        </div>
        <div class="column-item">
          <strong>Phonetic</strong> - Pronunciation (optional)
        </div>
        <div class="column-item required">
          <strong>PartOfSpeech</strong> - noun, verb, adjective, etc.
        </div>
        <div class="column-item required">
          <strong>Meaning</strong> - Vietnamese meaning
        </div>
        <div class="column-item required">
          <strong>Definition</strong> - English definition
        </div>
        <div class="column-item">
          <strong>DifficultyLevel</strong> - 1, 2, or 3
        </div>
        <div class="column-item">
          <strong>Category</strong> - Word category
        </div>
        <div class="column-item">
          <strong>ExampleSentence</strong> - Example in English
        </div>
        <div class="column-item">
          <strong>ExampleTranslation</strong> - Example in Vietnamese
        </div>
        <div class="column-item">
          <strong>ExampleGrammar</strong> - Grammar explanation
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { createFileUploadHeaders } from '@/utils/auth'
import { useNotifications } from '@/utils/notifications'

interface UploadResult {
  success: boolean
  importedWords: number
  importedExamples: number
  errors: string[]
  warnings: string[]
}

const emit = defineEmits<{
  close: []
  success: [result: UploadResult]
}>()

const { showSuccess, showError } = useNotifications()

const selectedFile = ref<File | null>(null)
const isUploading = ref(false)
const isDragOver = ref(false)
const uploadResult = ref<UploadResult | null>(null)
const fileInput = ref<HTMLInputElement>()

const handleDragOver = (e: DragEvent) => {
  e.preventDefault()
  isDragOver.value = true
}

const handleDragEnter = (e: DragEvent) => {
  e.preventDefault()
  isDragOver.value = true
}

const handleDragLeave = (e: DragEvent) => {
  e.preventDefault()
  if (!e.relatedTarget || !(e.currentTarget as Element).contains(e.relatedTarget as Node)) {
    isDragOver.value = false
  }
}

const handleDrop = (e: DragEvent) => {
  e.preventDefault()
  isDragOver.value = false
  
  const files = e.dataTransfer?.files
  if (files && files.length > 0) {
    const file = files[0]
    if (isValidExcelFile(file)) {
      selectedFile.value = file
    }
  }
}

const triggerFileInput = () => {
  if (!isUploading.value) {
    fileInput.value?.click()
  }
}

const handleFileSelect = (e: Event) => {
  const target = e.target as HTMLInputElement
  const file = target.files?.[0]
  if (file && isValidExcelFile(file)) {
    selectedFile.value = file
  }
}

const isValidExcelFile = (file: File): boolean => {
  const validTypes = ['application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', 'application/vnd.ms-excel']
  const validExtensions = ['.xlsx', '.xls']
  
  return validTypes.includes(file.type) || validExtensions.some(ext => file.name.toLowerCase().endsWith(ext))
}

const removeFile = () => {
  selectedFile.value = null
  if (fileInput.value) {
    fileInput.value.value = ''
  }
}

const formatFileSize = (bytes: number): string => {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

const uploadFile = async () => {
  console.log('🚀 uploadFile called!')
  console.log('📁 Selected file:', selectedFile.value)
  console.log('🔧 useNotifications:', { showSuccess, showError })
  
  if (!selectedFile.value) {
    console.log('❌ No file selected, returning')
    return
  }

  console.log('✅ Starting upload process...')
  isUploading.value = true
  uploadResult.value = null

  try {
    // Validate file first
    if (!selectedFile.value) {
      throw new Error('No file selected')
    }
    
    if (!isValidExcelFile(selectedFile.value)) {
      throw new Error('Invalid file type. Please select an Excel file (.xlsx or .xls)')
    }

    console.log('Starting file upload process...')
    console.log('File validation passed')

    const formData = new FormData()
    formData.append('file', selectedFile.value)

    // Debug: Log FormData content
    console.log('FormData entries:', Array.from(formData.entries()))
    console.log('File details:', {
      name: selectedFile.value.name,
      type: selectedFile.value.type,
      size: selectedFile.value.size,
      lastModified: selectedFile.value.lastModified
    })
    
    // Test if FormData contains the file
    console.log('FormData has file:', formData.has('file'))
    console.log('FormData get file:', formData.get('file'))

    const uploadUrl = '/api/vocabulary-learning/upload-excel'
    console.log('About to send request to:', uploadUrl)

    console.log('Sending FormData to server...')
    console.log('🌐 Current URL:', window.location.href)
    console.log('📡 Upload URL:', uploadUrl)
    console.log('🔑 Headers:', createFileUploadHeaders())

    const response = await fetch(uploadUrl, {
      method: 'POST',
      body: formData,
      headers: createFileUploadHeaders()
    })
    
    console.log('📥 Fetch completed, checking response...')

    console.log('Response received!')
    console.log('Response status:', response.status)
    console.log('Response status text:', response.statusText)
    console.log('Response headers:', Object.fromEntries(response.headers.entries()))
    console.log('Response ok:', response.ok)

    if (!response.ok) {
      const errorText = await response.text()
      console.error('Error response body:', errorText)
      console.error('Full error details:', {
        status: response.status,
        statusText: response.statusText,
        url: response.url,
        headers: Object.fromEntries(response.headers.entries())
      })
      
      let errorMessage = ''
      if (response.status === 415) {
        errorMessage = `Không thể xử lý định dạng file. Chi tiết: ${errorText}`
      } else if (response.status === 404) {
        errorMessage = `Không tìm thấy API endpoint: ${uploadUrl}`
      } else if (response.status === 401) {
        errorMessage = `Không có quyền truy cập. Vui lòng đăng nhập lại.`
      } else {
        errorMessage = `Upload thất bại: ${response.status} ${response.statusText} - ${errorText}`
      }
      
      showError(errorMessage)
      throw new Error(errorMessage)
    }

    const result: UploadResult = await response.json()
    console.log('📋 Upload result:', result)
    uploadResult.value = result
    
    if (result.success) {
      console.log('✅ Upload successful!')
      showSuccess(`Import thành công! Đã thêm ${result.importedWords} từ vựng và ${result.importedExamples} ví dụ.`)
      emit('success', result)
    } else {
      console.log('❌ Upload failed in result:', result.errors)
      const errorMsg = result.errors.length > 0 ? result.errors[0] : 'Import thất bại'
      showError(`Import thất bại: ${errorMsg}`)
    }
  } catch (error) {
    const errorMessage = error instanceof Error ? error.message : 'Upload failed'
    showError(errorMessage)
    uploadResult.value = {
      success: false,
      importedWords: 0,
      importedExamples: 0,
      errors: [errorMessage],
      warnings: []
    }
  } finally {
    isUploading.value = false
  }
}

const downloadTemplate = async () => {
  try {
    const response = await fetch('/api/vocabulary-learning/template.xlsx')
    if (response.ok) {
      const blob = await response.blob()
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = 'vocabulary_template.xlsx'
      link.click()
      window.URL.revokeObjectURL(url)
    } else {
      // Fallback to CSV template
      const template = [
        ['Word', 'Phonetic', 'PartOfSpeech', 'Meaning', 'Definition', 'DifficultyLevel', 'Category', 'ExampleSentence', 'ExampleTranslation', 'ExampleGrammar'],
        ['hello', '/həˈloʊ/', 'interjection', 'xin chào', 'used as a greeting', '1', 'Greetings', 'Hello, how are you?', 'Xin chào, bạn khỏe không?', 'Basic greeting expression'],
        ['beautiful', '/ˈbjuːtɪfəl/', 'adjective', 'đẹp', 'pleasing the senses or mind aesthetically', '2', 'Appearance', 'She is a beautiful woman.', 'Cô ấy là một người phụ nữ đẹp.', 'Adjective describing physical appearance'],
        ['study', '/ˈstʌdi/', 'verb', 'học', 'to learn about something', '1', 'Education', 'I study English every day.', 'Tôi học tiếng Anh mỗi ngày.', 'Present simple tense with frequency adverb']
      ]
      
      const csvContent = template.map(row => row.map(cell => `"${cell}"`).join(',')).join('\\n')
      const blob = new Blob([csvContent], { type: 'text/csv' })
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = 'vocabulary_template.csv'
      link.click()
      window.URL.revokeObjectURL(url)
    }
  } catch (error) {
    console.error('Failed to download template:', error)
  }
}

const resetUpload = () => {
  selectedFile.value = null
  uploadResult.value = null
  if (fileInput.value) {
    fileInput.value.value = ''
  }
}
</script>

<style scoped>
.excel-upload-container {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
  min-height: 100vh;
}

.upload-card {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 20px;
  padding: 2rem;
  margin-bottom: 2rem;
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.upload-header {
  text-align: center;
  margin-bottom: 2rem;
}

.upload-header h2 {
  color: #74c0fc;
  margin-bottom: 0.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}

.upload-header p {
  color: rgba(255, 255, 255, 0.7);
}

.upload-dropzone {
  border: 2px dashed rgba(116, 192, 252, 0.5);
  border-radius: 15px;
  padding: 3rem;
  text-align: center;
  cursor: pointer;
  transition: all 0.3s ease;
  background: rgba(116, 192, 252, 0.05);
  margin-bottom: 2rem;
}

.upload-dropzone:hover,
.upload-dropzone.drag-over {
  border-color: #74c0fc;
  background: rgba(116, 192, 252, 0.1);
  transform: translateY(-2px);
}

.upload-dropzone.uploading {
  cursor: not-allowed;
  opacity: 0.7;
}

.upload-icon {
  font-size: 4rem;
  color: #74c0fc;
  margin-bottom: 1rem;
}

.upload-prompt h3 {
  color: white;
  margin-bottom: 0.5rem;
}

.upload-prompt p {
  color: rgba(255, 255, 255, 0.7);
}

.upload-link {
  color: #74c0fc;
  text-decoration: underline;
}

.file-requirements {
  margin-top: 1rem;
  color: rgba(255, 255, 255, 0.5);
}

.selected-file {
  display: flex;
  align-items: center;
  gap: 1rem;
  background: rgba(231, 94, 141, 0.1);
  border: 1px solid rgba(231, 94, 141, 0.3);
  border-radius: 10px;
  padding: 1rem;
  margin: 0 auto;
  max-width: 400px;
}

.file-icon {
  font-size: 2rem;
  color: #e75e8d;
}

.file-info h4 {
  color: white;
  margin: 0;
}

.file-info p {
  color: rgba(255, 255, 255, 0.7);
  margin: 0.25rem 0 0 0;
  font-size: 0.9rem;
}

.remove-file-btn {
  background: rgba(255, 255, 255, 0.1);
  border: none;
  border-radius: 50%;
  width: 30px;
  height: 30px;
  color: white;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-left: auto;
}

.remove-file-btn:hover {
  background: rgba(231, 94, 141, 0.3);
}

.uploading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}

.upload-spinner {
  width: 40px;
  height: 40px;
  border: 4px solid rgba(116, 192, 252, 0.3);
  border-top: 4px solid #74c0fc;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.upload-actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
}

.upload-btn,
.template-btn,
.reset-btn,
.close-btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 10px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 500;
  transition: all 0.3s ease;
}

.upload-btn {
  background: linear-gradient(135deg, #74c0fc, #339af0);
  color: white;
}

.upload-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(116, 192, 252, 0.3);
}

.upload-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.template-btn {
  background: rgba(255, 255, 255, 0.1);
  color: white;
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.template-btn:hover {
  background: rgba(255, 255, 255, 0.2);
}

.results-section {
  margin-top: 2rem;
  padding-top: 2rem;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.result-header {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 2rem;
  justify-content: center;
}

.result-header.success {
  color: #51cf66;
}

.result-header.error {
  color: #ff6b6b;
}

.result-header i {
  font-size: 2rem;
}

.success-stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 1rem;
  margin-bottom: 2rem;
}

.stat-item {
  text-align: center;
  background: rgba(81, 207, 102, 0.1);
  border: 1px solid rgba(81, 207, 102, 0.3);
  border-radius: 10px;
  padding: 1rem;
}

.stat-number {
  display: block;
  font-size: 2rem;
  font-weight: bold;
  color: #51cf66;
}

.stat-label {
  color: rgba(255, 255, 255, 0.7);
  font-size: 0.9rem;
}



.result-actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
  margin-top: 2rem;
}

.reset-btn {
  background: rgba(116, 192, 252, 0.2);
  color: #74c0fc;
  border: 1px solid rgba(116, 192, 252, 0.3);
}

.reset-btn:hover {
  background: rgba(116, 192, 252, 0.3);
}

.close-btn {
  background: rgba(255, 255, 255, 0.1);
  color: white;
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.close-btn:hover {
  background: rgba(255, 255, 255, 0.2);
}

.format-guide {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 15px;
  padding: 2rem;
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.format-guide h3 {
  color: #74c0fc;
  margin-bottom: 1rem;
}

.format-guide p {
  color: rgba(255, 255, 255, 0.7);
  margin-bottom: 1.5rem;
}

.columns-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
}

.column-item {
  background: rgba(116, 192, 252, 0.1);
  border: 1px solid rgba(116, 192, 252, 0.3);
  border-radius: 8px;
  padding: 0.75rem;
  color: rgba(255, 255, 255, 0.9);
}

.column-item.required {
  background: rgba(231, 94, 141, 0.1);
  border-color: rgba(231, 94, 141, 0.3);
}

.column-item strong {
  color: white;
}
</style>