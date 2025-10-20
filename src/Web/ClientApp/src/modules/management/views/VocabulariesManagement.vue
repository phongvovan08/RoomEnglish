<template>
  <div class="manage-vocabularies">
    <div class="page-header">
      <button @click="$router.back()" class="back-btn">
        <Icon icon="mdi:arrow-left" class="w-5 h-5 mr-2" />
        Quay lại
      </button>
      
      <h1>
        <Icon icon="mdi:book-alphabet" class="w-8 h-8 mr-3" />
        Quản lý Từ vựng
        <span v-if="currentCategory" class="category-name">- {{ currentCategory.name }}</span>
      </h1>
      <p>Quản lý từ vựng trong danh mục {{ currentCategory?.name || 'đã chọn' }}</p>
    </div>

    <div class="action-bar">
      <div class="left-actions">
        <button @click="showCreateModal = true" class="btn-primary">
          <Icon icon="mdi:plus" class="w-5 h-5 mr-2" />
          Thêm từ mới
        </button>
        <button @click="showUploadModal = true" class="btn-upload">
          <Icon icon="mdi:upload" class="w-5 h-5 mr-2" />
          Upload Excel
        </button>
        <button @click="showJsonUploadModal = true" class="btn-json">
          <Icon icon="mdi:code-json" class="w-5 h-5 mr-2" />
          Import Words
        </button>
      </div>
      
      <div v-if="selectedVocabularies.length > 0" class="selection-actions">
        <span class="selection-count">
          Đã chọn {{ selectedVocabularies.length }} từ vựng
        </span>
        <button 
          @click="generateExamplesForSelected" 
          :disabled="isGeneratingExamples"
          class="btn-ai"
        >
          <Icon icon="mdi:lightbulb-on" class="w-5 h-5 mr-2" />
          {{ isGeneratingExamples ? 'Đang tạo...' : 'Tạo ví dụ AI' }}
        </button>
        <button @click="clearSelection" class="btn-secondary">
          <Icon icon="mdi:close" class="w-5 h-5 mr-2" />
          Hủy chọn
        </button>
      </div>
    </div>

    <!-- Vocabulary Data Grid -->
    <VocabularyDataGrid
      :vocabularies="vocabularies"
      :loading="isLoading"
      :page-size="pageSize"
      :current-page="currentPage"
      :total-items="totalItems"
      :total-pages="totalPages"
      :selectable="true"
      :selected-items="selectedVocabularies"
      :default-view-mode="'table'"
      @vocabulary-click="(vocab: Vocabulary) => navigateToExamples(vocab.id)"
      @edit-vocabulary="editVocabulary"
      @delete-vocabulary="(vocab: Vocabulary) => deleteVocabulary(vocab.id)"
      @create-vocabulary="showCreateModal = true"
      @upload-vocabulary="showUploadModal = true"
      @search="handleGridSearch"
      @page-change="handleGridPageChange"
      @page-size-change="handleGridPageSizeChange"
      @sort-change="handleSort"
      @selection-change="handleSelectionChange"
    />

    <!-- Create/Edit Modal -->
    <div v-if="showCreateModal || editingVocabulary" class="modal-overlay" @click="closeModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>{{ editingVocabulary ? 'Sửa từ vựng' : 'Thêm từ vựng mới' }}</h2>
          <button @click="closeModal" class="close-btn">
            <Icon icon="mdi:close" class="w-6 h-6" />
          </button>
        </div>
        
        <form @submit.prevent="saveVocabulary" class="vocabulary-form">
          <div class="form-group">
            <label for="word">Từ vựng *</label>
            <input 
              id="word"
              v-model="vocabularyForm.word" 
              type="text" 
              required
              placeholder="Nhập từ vựng..."
              class="form-input"
            />
          </div>
          
          <div class="form-group">
            <label for="definition">Định nghĩa *</label>
            <textarea 
              id="definition"
              v-model="vocabularyForm.definition" 
              required
              placeholder="Nhập định nghĩa..."
              rows="3"
              class="form-textarea"
            ></textarea>
          </div>
          
          <div class="form-group">
            <label for="pronunciation">Phát âm</label>
            <input 
              id="pronunciation"
              v-model="vocabularyForm.pronunciation" 
              type="text" 
              placeholder="/pronunciation/"
              class="form-input"
            />
          </div>
          
          <div class="form-actions">
            <button type="button" @click="closeModal" class="btn-secondary">
              Hủy bỏ
            </button>
            <button type="submit" class="btn-primary" :disabled="!vocabularyForm.word.trim() || !vocabularyForm.definition.trim()">
              {{ editingVocabulary ? 'Cập nhật' : 'Thêm mới' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Upload Modal -->
    <div v-if="showUploadModal" class="modal-overlay" @click="closeUploadModal">
      <div class="modal-content" @click.stop>
        <VocabularyUploadModal 
          :categoryId="categoryId"
          @close="closeUploadModal" 
          @success="handleUploadSuccess"
        />
      </div>
    </div>

    <!-- JSON Upload Modal -->
    <JsonUploadModal 
      :is-open="showJsonUploadModal"
      :is-importing="isImportingWords"
      @close="showJsonUploadModal = false"
      @upload-success="handleJsonUploadSuccess"
      @download-template="handleDownloadJsonTemplate"
      @import-json="handleImportJson"
      @import-words="handleImportWords"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { Icon } from '@iconify/vue'
import { createAuthHeaders } from '@/utils/auth'
import { useNotifications } from '@/utils/notifications'
import { useVocabulariesManagement } from '../composables/use-vocabularies-management'
import VocabularyUploadModal from '../components/VocabularyUploadModal.vue'
import JsonUploadModal from '@/modules/vocabulary/components/JsonUploadModal.vue'
import VocabularyDataGrid from '@/modules/vocabulary/components/VocabularyDataGrid.vue'

const router = useRouter()
const route = useRoute()
const { showSuccess, showError ,showWarning} = useNotifications()
const { importFromJson, importFromWords, downloadJsonTemplate, isImportingWords } = useVocabulariesManagement()

interface Category {
  id: number
  name: string
  description: string
}

interface Vocabulary {
  id: number
  word: string
  definition: string
  pronunciation?: string
  exampleCount: number
  createdAt: string
}

interface VocabularyForm {
  word: string
  definition: string
  pronunciation: string
}

// Props from route
const categoryId = computed(() => parseInt(route.params.categoryId as string))

// Reactive data
const currentCategory = ref<Category | null>(null)
const vocabularies = ref<Vocabulary[]>([])
const searchQuery = ref('')
const isLoading = ref(false)
const showCreateModal = ref(false)
const showUploadModal = ref(false)
const showJsonUploadModal = ref(false)
const editingVocabulary = ref<Vocabulary | null>(null)
const vocabularyForm = ref<VocabularyForm>({
  word: '',
  definition: '',
  pronunciation: ''
})

// Pagination state
const currentPage = ref(1)
const pageSize = ref(10)
const totalItems = ref(0)
const totalPages = ref(0)
const includeInactive = ref(false)
const includeExamples = ref(true)
const sortBy = ref<string>('')
const sortOrder = ref<'asc' | 'desc'>('asc')

// Selection state
const selectedVocabularies = ref<Vocabulary[]>([])
const isGeneratingExamples = ref(false)

// Methods
const loadCategory = async () => {
  try {
    const response = await fetch(`/api/vocabulary-categories/${categoryId.value}`, {
      headers: createAuthHeaders()
    })
    
    if (response.ok) {
      currentCategory.value = await response.json()
    } else {
      showError('Lỗi tải danh mục', `Không thể tải thông tin danh mục. Mã lỗi: ${response.status}`)
    }
  } catch (error) {
    showError('Lỗi kết nối', 'Không thể kết nối đến máy chủ. Vui lòng thử lại.')
    console.error('Failed to load category:', error)
  }
}

const loadVocabularies = async (page: number = currentPage.value) => {
  try {
    isLoading.value = true
    // Build query parameters matching GetVocabularyWordsQuery
    const params = new URLSearchParams({
      CategoryId: categoryId.value.toString(),
      PageNumber: page.toString(),
      PageSize: pageSize.value.toString(),
      IncludeInactive: includeInactive.value.toString(),
      IncludeExamples: includeExamples.value.toString(),
      IncludeUserProgress: 'false'
    })
    
    // Add search term if provided
    if (searchQuery.value) {
      params.append('SearchTerm', searchQuery.value)
    }
    
    // Add sort parameters
    if (sortBy.value) {
      params.append('SortBy', sortBy.value)
      params.append('SortOrder', sortOrder.value)
    }
    
    const response = await fetch(`/api/vocabulary-words?${params}`, {
      headers: createAuthHeaders()
    })
    
    if (response.ok) {
      const data = await response.json()
      vocabularies.value = data.items || []
      totalItems.value = data.totalCount || 0
      totalPages.value = data.totalPages || 0
      currentPage.value = data.pageNumber || 1
    } else {
      showError('Lỗi tải từ vựng', `Không thể tải danh sách từ vựng. Mã lỗi: ${response.status}`)
    }
  } catch (error) {
    showError('Lỗi kết nối', 'Không thể kết nối đến máy chủ. Vui lòng thử lại.')
    console.error('Failed to load vocabularies:', error)
  } finally {
    isLoading.value = false
  }
}

const navigateToExamples = (vocabularyId: number) => {
  router.push(`/management/examples/${vocabularyId}`)
}

const editVocabulary = (vocabulary: Vocabulary) => {
  editingVocabulary.value = vocabulary
  vocabularyForm.value = {
    word: vocabulary.word,
    definition: vocabulary.definition,
    pronunciation: vocabulary.pronunciation || ''
  }
}

const deleteVocabulary = async (vocabularyId: number) => {
  if (!confirm('Bạn có chắc chắn muốn xóa từ vựng này?')) return
  
  try {
    const response = await fetch(`/api/vocabulary-words/${vocabularyId}`, {
      method: 'DELETE',
      headers: createAuthHeaders()
    })
    
    if (response.ok) {
      await loadVocabularies()
      showSuccess('Xóa từ vựng thành công!')
    } else {
      showError('Xóa từ vựng thất bại: ' + response.statusText)
    }
  } catch (error) {
    console.error('Failed to delete vocabulary:', error)
    showError('Lỗi kết nối khi xóa từ vựng')
  }
}

const saveVocabulary = async () => {
  const isEdit = !!editingVocabulary.value
  const url = isEdit 
    ? `/api/vocabulary-words/${editingVocabulary.value!.id}`
    : '/api/vocabulary-words'
  
  const method = isEdit ? 'PUT' : 'POST'
  const payload = {
    ...vocabularyForm.value,
    categoryId: categoryId.value
  }
  
  try {
    const response = await fetch(url, {
      method,
      headers: createAuthHeaders(),
      body: JSON.stringify(payload)
    })
    
    if (response.ok) {
      await loadVocabularies()
      closeModal()
      showSuccess(isEdit ? 'Cập nhật từ vựng thành công!' : 'Thêm từ vựng thành công!')
    } else {
      showError(`${isEdit ? 'Cập nhật' : 'Thêm'} từ vựng thất bại: ` + response.statusText)
    }
  } catch (error) {
    console.error('Failed to save vocabulary:', error)
    showError(`Lỗi kết nối khi ${isEdit ? 'cập nhật' : 'thêm'} từ vựng`)
  }
}

const closeModal = () => {
  showCreateModal.value = false
  editingVocabulary.value = null
  vocabularyForm.value = { word: '', definition: '', pronunciation: '' }
}

const closeUploadModal = () => {
  showUploadModal.value = false
}

const handleUploadSuccess = () => {
  loadVocabularies()
  closeUploadModal()
}

const handleJsonUploadSuccess = () => {
  loadVocabularies()
  showJsonUploadModal.value = false
}

const handleDownloadJsonTemplate = async () => {
  try {
    await downloadJsonTemplate()
    showSuccess('JSON template downloaded successfully')
  } catch (error) {
    showError('Failed to download JSON template: ' + (error as Error).message)
  }
}

const handleImportJson = async (jsonData: string) => {
  try {
    console.log('TRY handleImportJson')
    const result = await importFromJson(jsonData)
    
    // Check if result exists and has expected properties
    if (result && typeof result.successCount !== 'undefined') {
      // Check if there were any errors
      if (result.errorCount > 0 && result.errors && Array.isArray(result.errors)) {
        // Show errors if any
        const errorTitle = `Import completed with ${result.successCount} success ${result.errorCount} errors`
        showWarning(errorTitle) // Longer duration for error messages
      } else {
        // Show success if no errors
        showSuccess(`Import successful: ${result.successCount} words imported`)
      }
      
      // Auto refresh after import (whether successful or with errors)
      setTimeout(() => {
        loadVocabularies()
      }, 1000)
    } else {
      console.warn('Import result missing expected properties:', result)
      showError('Import completed but response format was unexpected')
    }
  } catch (error) {
    console.log('catch error')
    console.log(error)
    // Error handling is now done by usePromiseWrapper
    // Just log for debugging - toast.error is already called by the wrapper
  }
}

const handleImportWords = async (words: string[]) => {
  try {
    console.log('TRY handleImportWords', words)
    const result = await importFromWords(words)
    
    // Check if result exists and has expected properties
    if (result && typeof result.successCount !== 'undefined') {
      // Check if there were any errors
      if (result.errorCount > 0 && result.errors && Array.isArray(result.errors)) {
        // Show errors if any
         const errorTitle = `Import completed with ${result.successCount} success ${result.errorCount} errors`
        showWarning(errorTitle) // Longer duration for error messages
      } else {
        // Show success if no errors
        showSuccess(`Import successful: ${result.successCount} words processed via ChatGPT`)
      }
      
      // Close modal after successful import
      showJsonUploadModal.value = false
      
      // Auto refresh after import (whether successful or with errors)
      setTimeout(() => {
        loadVocabularies()
      }, 1000)
    } else {
      console.warn('Import result missing expected properties:', result)
      showError('Import completed but response format was unexpected')
    }
  } catch (error) {
    console.log('catch error handleImportWords')
    console.log(error)
    // Error handling is now done by usePromiseWrapper
    // Just log for debugging - toast.error is already called by the wrapper
  }
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('vi-VN')
}

// Grid event handlers
const handleGridSearch = (query: string) => {
  searchQuery.value = query
}

const handleGridPageChange = (page: number) => {
  currentPage.value = page
  loadVocabularies(page)
}

const handleGridPageSizeChange = (newPageSize: number) => {
  pageSize.value = newPageSize
  currentPage.value = 1
  loadVocabularies(1)
}

const handleSort = (sortByParam: string, sortOrderParam: 'asc' | 'desc') => {
  // Update sort state
  sortBy.value = sortByParam
  sortOrder.value = sortOrderParam
  currentPage.value = 1 // Reset to first page when sorting
  loadVocabularies()
}

// Selection methods
const handleSelectionChange = (selected: Vocabulary[]) => {
  selectedVocabularies.value = selected
}

const clearSelection = () => {
  selectedVocabularies.value = []
}

const generateExamplesForSelected = async () => {
  if (selectedVocabularies.value.length === 0) {
    showWarning('Vui lòng chọn ít nhất một từ vựng để tạo ví dụ')
    return
  }

  const startTime = performance.now()
  console.log(`🚀 Started generating examples for ${selectedVocabularies.value.length} words:`, 
    selectedVocabularies.value.map(v => v.word))

  try {
    isGeneratingExamples.value = true
    
    // Extract just the words from selected vocabularies
    const words = selectedVocabularies.value.map(vocab => vocab.word)
    
    // Call the import examples API
    const response = await fetch('/api/vocabulary-examples/import-words', {
      method: 'POST',
      headers: {
        ...createAuthHeaders(),
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        Words: words,
        ExampleCount: 10,
        IncludeGrammar: true,
        IncludeContext: true,
        DifficultyLevel: 1 // Easy level by default
      })
    })

    if (response.ok) {
      const endTime = performance.now()
      const totalTime = Math.round(endTime - startTime)
      
      const result = await response.json()
      console.log(`✅ Generate examples completed in ${totalTime}ms:`, result)
      
      if (result.successCount > 0) {
        let message = `Đã tạo thành công ${result.successCount} ví dụ cho ${words.length} từ vựng trong ${(totalTime/1000).toFixed(1)}s`
        
        if (result.errorCount > 0) {
          message += `, ${result.errorCount} ví dụ bị trùng hoặc lỗi`
          showWarning(message)
        } else {
          showSuccess(message)
        }
        
        // Performance logging
        const avgTimePerWord = totalTime / words.length
        console.log(`📊 Performance metrics:
          - Total time: ${totalTime}ms (${(totalTime/1000).toFixed(1)}s)
          - Average per word: ${avgTimePerWord.toFixed(0)}ms
          - Success rate: ${((result.successCount / (result.successCount + result.errorCount)) * 100).toFixed(1)}%
          - Words processed: ${words.length}
        `)
      } else {
        if (result.errorCount > 0) {
          showWarning(`Không tạo được ví dụ mới. ${result.errorCount} ví dụ bị trùng hoặc lỗi`)
        } else {
          showWarning('Không thể tạo ví dụ cho các từ vựng đã chọn')
        }
      }
      
      // Clear selection after generating examples
      clearSelection()
      
      // Refresh the vocabulary list to show updated example counts
      setTimeout(() => {
        loadVocabularies()
      }, 1000)
    } else {
      const endTime = performance.now()
      const totalTime = Math.round(endTime - startTime)
      
      const errorText = await response.text()
      console.error(`❌ Generate examples failed after ${totalTime}ms:`, errorText)
      showError('Lỗi tạo ví dụ', `Không thể tạo ví dụ. ${errorText}`)
    }
  } catch (error) {
    const endTime = performance.now()
    const totalTime = Math.round(endTime - startTime)
    
    console.error(`❌ Generate examples error after ${totalTime}ms:`, error)
    showError('Lỗi', 'Có lỗi xảy ra khi tạo ví dụ')
  } finally {
    isGeneratingExamples.value = false
    console.log('🏁 Generate examples process completed')
  }
}

// Lifecycle
onMounted(() => {
  loadCategory()
  loadVocabularies()
})
</script>

<style scoped>
.manage-vocabularies {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
  min-height: 100vh;
}

.page-header {
  margin-bottom: 3rem;
}

.back-btn {
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  color: white;
  padding: 0.5rem 1rem;
  border-radius: 8px;
  cursor: pointer;
  display: flex;
  align-items: center;
  margin-bottom: 1rem;
  transition: all 0.3s ease;
}

.back-btn:hover {
  background: rgba(255, 255, 255, 0.2);
}

.page-header h1 {
  color: #74c0fc;
  display: flex;
  align-items: center;
  font-size: 2.5rem;
  margin-bottom: 0.5rem;
}

.category-name {
  color: #51cf66;
  font-weight: 600;
}

.page-header p {
  color: rgba(255, 255, 255, 0.7);
  font-size: 1.1rem;
  margin-left: 3.5rem;
}

.action-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  gap: 1rem;
}

.left-actions {
  display: flex;
  gap: 1rem;
}

.btn-primary {
  background: linear-gradient(135deg, #74c0fc, #339af0);
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 12px;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: all 0.3s ease;
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(116, 192, 252, 0.4);
}

.btn-upload {
  background: linear-gradient(135deg, #51cf66, #40c057);
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 12px;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: all 0.3s ease;
}

.btn-upload:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(81, 207, 102, 0.4);
}

.btn-json {
  background: linear-gradient(135deg, #ffd43b, #fab005);
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 12px;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: all 0.3s ease;
}

.btn-json:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(255, 212, 59, 0.4);
}

.btn-secondary {
  background: rgba(255, 255, 255, 0.1);
  color: white;
  border: 1px solid rgba(255, 255, 255, 0.2);
  padding: 0.75rem 1.5rem;
  border-radius: 12px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-secondary:hover {
  background: rgba(255, 255, 255, 0.2);
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  backdrop-filter: blur(5px);
}

.modal-content {
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
  border-radius: 20px;
  max-width: 500px;
  width: 90%;
  max-height: 90vh;
  overflow-y: auto;
  border: 1px solid rgba(255, 255, 255, 0.1);
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

.vocabulary-form {
  padding: 1.5rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  color: white;
  margin-bottom: 0.5rem;
  font-weight: 600;
}

.form-input,
.form-textarea {
  width: 100%;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 8px;
  padding: 0.75rem;
  color: white;
  font-size: 1rem;
  transition: all 0.3s ease;
}

.form-input:focus,
.form-textarea:focus {
  outline: none;
  border-color: #74c0fc;
  background: rgba(255, 255, 255, 0.15);
}

.form-input::placeholder,
.form-textarea::placeholder {
  color: rgba(255, 255, 255, 0.5);
}

.form-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  margin-top: 2rem;
}

.btn-primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-primary:disabled:hover {
  transform: none;
  box-shadow: none;
}

/* Selection Actions */
.selection-actions {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.selection-count {
  color: white;
  font-weight: 600;
  padding: 0.5rem 1rem;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 8px;
  font-size: 0.875rem;
}

.btn-ai {
  background: linear-gradient(135deg, #845ec2, #b39bc8);
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 12px;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: all 0.3s ease;
}

.btn-ai:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(132, 94, 194, 0.4);
}

.btn-ai:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}

.btn-secondary {
  background: rgba(255, 255, 255, 0.1);
  color: white;
  border: 1px solid rgba(255, 255, 255, 0.2);
  padding: 0.75rem 1.5rem;
  border-radius: 12px;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: all 0.3s ease;
}

.btn-secondary:hover {
  background: rgba(255, 255, 255, 0.2);
  transform: translateY(-2px);
}

/* Mobile Responsiveness */
@media (max-width: 768px) {
  .manage-vocabularies {
    padding: 1rem;
  }
  
  .action-bar {
    flex-direction: column;
    align-items: stretch;
  }
  
  .left-actions {
    justify-content: center;
  }
  
  .page-header h1 {
    font-size: 2rem;
  }
  
  .modal-content {
    margin: 1rem;
    width: calc(100% - 2rem);
  }
}
</style>