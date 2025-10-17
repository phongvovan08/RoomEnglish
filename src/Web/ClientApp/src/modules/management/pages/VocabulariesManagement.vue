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
      </div>
      
      <div class="search-box">
        <Icon icon="mdi:magnify" class="w-5 h-5" />
        <input 
          v-model="searchQuery" 
          type="text" 
          placeholder="Tìm kiếm từ vựng..."
          class="search-input"
        />
      </div>
    </div>

    <div class="vocabularies-grid">
      <div 
        v-for="vocabulary in filteredVocabularies" 
        :key="vocabulary.id"
        class="vocabulary-card"
        @click="navigateToExamples(vocabulary.id)"
      >
        <div class="vocabulary-header">
          <div class="word-info">
            <h3>{{ vocabulary.word }}</h3>
            <span v-if="vocabulary.pronunciation" class="pronunciation">{{ vocabulary.pronunciation }}</span>
          </div>
          <div class="vocabulary-actions">
            <button @click.stop="editVocabulary(vocabulary)" class="action-btn edit">
              <Icon icon="mdi:pencil" class="w-4 h-4" />
            </button>
            <button @click.stop="deleteVocabulary(vocabulary.id)" class="action-btn delete">
              <Icon icon="mdi:delete" class="w-4 h-4" />
            </button>
          </div>
        </div>
        
        <div class="vocabulary-content">
          <p class="definition">{{ vocabulary.definition }}</p>
          
          <div class="vocabulary-stats">
            <div class="stat">
              <Icon icon="mdi:format-quote-close" class="w-4 h-4" />
              <span>{{ vocabulary.exampleCount }} ví dụ</span>
            </div>
            <div class="stat">
              <Icon icon="mdi:calendar" class="w-4 h-4" />
              <span>{{ formatDate(vocabulary.createdAt) }}</span>
            </div>
          </div>
        </div>

        <div class="vocabulary-footer">
          <span class="view-examples">Xem ví dụ →</span>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-if="filteredVocabularies.length === 0" class="empty-state">
      <Icon icon="mdi:book-outline" class="w-16 h-16 text-gray-400 mb-4" />
      <h3>Chưa có từ vựng nào</h3>
      <p>Thêm từ vựng đầu tiên cho danh mục này</p>
      <div class="empty-actions">
        <button @click="showCreateModal = true" class="btn-primary">
          <Icon icon="mdi:plus" class="w-5 h-5 mr-2" />
          Thêm từ đầu tiên
        </button>
        <button @click="showUploadModal = true" class="btn-upload ml-2">
          <Icon icon="mdi:upload" class="w-5 h-5 mr-2" />
          Upload từ Excel
        </button>
      </div>
    </div>

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
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { Icon } from '@iconify/vue'
import { createAuthHeaders } from '@/utils/auth'
import { useNotifications } from '@/utils/notifications'
import VocabularyUploadModal from '../components/VocabularyUploadModal.vue'

const router = useRouter()
const route = useRoute()
const { showSuccess, showError } = useNotifications()

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
const showCreateModal = ref(false)
const showUploadModal = ref(false)
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

// Computed
const filteredVocabularies = computed(() => {
  if (!searchQuery.value) return vocabularies.value
  
  const query = searchQuery.value.toLowerCase()
  return vocabularies.value.filter(vocabulary => 
    vocabulary.word.toLowerCase().includes(query) ||
    vocabulary.definition.toLowerCase().includes(query)
  )
})

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

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('vi-VN')
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

.search-box {
  display: flex;
  align-items: center;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  padding: 0.75rem 1rem;
  gap: 0.5rem;
  color: rgba(255, 255, 255, 0.7);
}

.search-input {
  background: none;
  border: none;
  color: white;
  outline: none;
  width: 250px;
  font-size: 1rem;
}

.search-input::placeholder {
  color: rgba(255, 255, 255, 0.5);
}

.vocabularies-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(400px, 1fr));
  gap: 2rem;
  margin-bottom: 2rem;
}

.vocabulary-card {
  background: rgba(255, 255, 255, 0.08);
  border-radius: 16px;
  padding: 1.5rem;
  border: 1px solid rgba(255, 255, 255, 0.1);
  cursor: pointer;
  transition: all 0.3s ease;
  backdrop-filter: blur(10px);
}

.vocabulary-card:hover {
  transform: translateY(-5px);
  background: rgba(255, 255, 255, 0.12);
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
}

.vocabulary-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.word-info h3 {
  color: #74c0fc;
  margin: 0;
  font-size: 1.5rem;
  font-weight: 700;
}

.pronunciation {
  color: rgba(255, 255, 255, 0.6);
  font-style: italic;
  font-size: 0.9rem;
  margin-top: 0.25rem;
}

.vocabulary-actions {
  display: flex;
  gap: 0.5rem;
}

.action-btn {
  background: rgba(255, 255, 255, 0.1);
  border: none;
  padding: 0.5rem;
  border-radius: 8px;
  cursor: pointer;
  color: white;
  transition: all 0.2s ease;
}

.action-btn.edit:hover {
  background: rgba(255, 193, 7, 0.2);
  color: #ffc107;
}

.action-btn.delete:hover {
  background: rgba(220, 53, 69, 0.2);
  color: #dc3545;
}

.vocabulary-content {
  margin-bottom: 1rem;
}

.definition {
  color: rgba(255, 255, 255, 0.9);
  margin: 0 0 1rem 0;
  line-height: 1.5;
  font-size: 1rem;
}

.vocabulary-stats {
  display: flex;
  gap: 1rem;
  margin-bottom: 1rem;
}

.stat {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  color: rgba(255, 255, 255, 0.6);
  font-size: 0.875rem;
}

.vocabulary-footer {
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  padding-top: 1rem;
  text-align: right;
}

.view-examples {
  color: #51cf66;
  font-weight: 600;
  font-size: 0.875rem;
}

.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  color: rgba(255, 255, 255, 0.7);
}

.empty-state h3 {
  color: white;
  margin-bottom: 0.5rem;
}

.empty-actions {
  margin-top: 1.5rem;
}

.ml-2 {
  margin-left: 0.5rem;
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
    margin-bottom: 1rem;
  }
  
  .search-box {
    justify-content: center;
  }
  
  .search-input {
    width: 200px;
  }
  
  .vocabularies-grid {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
  
  .vocabulary-card {
    padding: 1rem;
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