<template>
  <div class="manage-examples">
    <div class="page-header">
      <button @click="$router.back()" class="back-btn">
        <Icon icon="mdi:arrow-left" class="w-5 h-5 mr-2" />
        Quay lại
      </button>
      
      <h1>
        <Icon icon="mdi:format-quote-close" class="w-8 h-8 mr-3" />
        Quản lý Ví dụ
        <span v-if="currentVocabulary" class="vocabulary-word">- {{ currentVocabulary.word }}</span>
      </h1>
      <p>Quản lý ví dụ cho từ vựng "{{ currentVocabulary?.word || 'đã chọn' }}"</p>
    </div>

    <div class="action-bar">
      <div class="left-actions">
        <button @click="showCreateModal = true" class="btn-primary">
          <Icon icon="mdi:plus" class="w-5 h-5 mr-2" />
          Thêm ví dụ
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
          placeholder="Tìm kiếm ví dụ..."
          class="search-input"
        />
      </div>
    </div>

    <div class="examples-list">
      <div 
        v-for="example in filteredExamples" 
        :key="example.id"
        class="example-card"
      >
        <div class="example-header">
          <div class="example-info">
            <span class="example-number">#{{ example.id }}</span>
          </div>
          <div class="example-actions">
            <button @click="editExample(example)" class="action-btn edit">
              <Icon icon="mdi:pencil" class="w-4 h-4" />
            </button>
            <button @click="deleteExample(example.id)" class="action-btn delete">
              <Icon icon="mdi:delete" class="w-4 h-4" />
            </button>
          </div>
        </div>
        
        <div class="example-content">
          <div class="sentence-section">
            <label class="section-label">
              <Icon icon="mdi:format-quote-open" class="w-4 h-4" />
              Câu ví dụ (Tiếng Anh)
            </label>
            <p class="sentence english">{{ example.sentence }}</p>
          </div>
          
          <div class="translation-section">
            <label class="section-label">
              <Icon icon="mdi:translate" class="w-4 h-4" />
              Bản dịch (Tiếng Việt)
            </label>
            <p class="sentence vietnamese">{{ example.translation }}</p>
          </div>
          
          <div v-if="example.grammar" class="grammar-section">
            <label class="section-label">
              <Icon icon="mdi:school" class="w-4 h-4" />
              Giải thích ngữ pháp
            </label>
            <p class="grammar-explanation">{{ example.grammar }}</p>
          </div>
          
          <div class="example-footer">
            <span class="date-created">
              <Icon icon="mdi:calendar" class="w-4 h-4" />
              {{ formatDate(example.createdAt) }}
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-if="filteredExamples.length === 0" class="empty-state">
      <Icon icon="mdi:format-quote-outline" class="w-16 h-16 text-gray-400 mb-4" />
      <h3>Chưa có ví dụ nào</h3>
      <p>Thêm ví dụ đầu tiên cho từ vựng này</p>
      <div class="empty-actions">
        <button @click="showCreateModal = true" class="btn-primary">
          <Icon icon="mdi:plus" class="w-5 h-5 mr-2" />
          Thêm ví dụ đầu tiên
        </button>
        <button @click="showUploadModal = true" class="btn-upload ml-2">
          <Icon icon="mdi:upload" class="w-5 h-5 mr-2" />
          Upload từ Excel
        </button>
      </div>
    </div>

    <!-- Create/Edit Modal -->
    <div v-if="showCreateModal || editingExample" class="modal-overlay" @click="closeModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>{{ editingExample ? 'Sửa ví dụ' : 'Thêm ví dụ mới' }}</h2>
          <button @click="closeModal" class="close-btn">
            <Icon icon="mdi:close" class="w-6 h-6" />
          </button>
        </div>
        
        <form @submit.prevent="saveExample" class="example-form">
          <div class="form-group">
            <label for="sentence">Câu ví dụ (Tiếng Anh) *</label>
            <textarea 
              id="sentence"
              v-model="exampleForm.sentence" 
              required
              placeholder="Nhập câu ví dụ tiếng Anh..."
              rows="3"
              class="form-textarea"
            ></textarea>
          </div>
          
          <div class="form-group">
            <label for="translation">Bản dịch (Tiếng Việt) *</label>
            <textarea 
              id="translation"
              v-model="exampleForm.translation" 
              required
              placeholder="Nhập bản dịch tiếng Việt..."
              rows="3"
              class="form-textarea"
            ></textarea>
          </div>
          
          <div class="form-group">
            <label for="grammar">Giải thích ngữ pháp</label>
            <textarea 
              id="grammar"
              v-model="exampleForm.grammar" 
              placeholder="Nhập giải thích ngữ pháp..."
              rows="2"
              class="form-textarea"
            ></textarea>
          </div>
          
          <div class="form-actions">
            <button type="button" @click="closeModal" class="btn-secondary">
              Hủy bỏ
            </button>
            <button type="submit" class="btn-primary" :disabled="!exampleForm.sentence.trim() || !exampleForm.translation.trim()">
              {{ editingExample ? 'Cập nhật' : 'Thêm mới' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Upload Modal -->
    <div v-if="showUploadModal" class="modal-overlay" @click="closeUploadModal">
      <div class="modal-content" @click.stop>
        <ExampleUploadModal 
          :vocabularyId="vocabularyId"
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
import ExampleUploadModal from '../components/ExampleUploadModal.vue'

const router = useRouter()
const route = useRoute()
const { showSuccess, showError } = useNotifications()

interface Vocabulary {
  id: number
  word: string
  definition: string
}

interface Example {
  id: number
  sentence: string
  translation: string
  grammar?: string
  createdAt: string
}

interface ExampleForm {
  sentence: string
  translation: string
  grammar: string
}

// Props from route
const vocabularyId = computed(() => parseInt(route.params.vocabularyId as string))

// Reactive data
const currentVocabulary = ref<Vocabulary | null>(null)
const examples = ref<Example[]>([])
const searchQuery = ref('')
const showCreateModal = ref(false)
const showUploadModal = ref(false)
const editingExample = ref<Example | null>(null)
const exampleForm = ref<ExampleForm>({
  sentence: '',
  translation: '',
  grammar: ''
})

// Computed
const filteredExamples = computed(() => {
  if (!searchQuery.value) return examples.value
  
  const query = searchQuery.value.toLowerCase()
  return examples.value.filter(example => 
    example.sentence.toLowerCase().includes(query) ||
    example.translation.toLowerCase().includes(query) ||
    (example.grammar && example.grammar.toLowerCase().includes(query))
  )
})

// Methods
const loadVocabulary = async () => {
  try {
    const response = await fetch(`/api/vocabulary-words/${vocabularyId.value}`, {
      headers: createAuthHeaders()
    })
    
    if (response.ok) {
      currentVocabulary.value = await response.json()
    } else {
      showError('Lỗi tải từ vựng', `Không thể tải thông tin từ vựng. Mã lỗi: ${response.status}`)
    }
  } catch (error) {
    showError('Lỗi kết nối', 'Không thể kết nối đến máy chủ. Vui lòng thử lại.')
    console.error('Failed to load vocabulary:', error)
  }
}

const loadExamples = async () => {
  try {
    const response = await fetch(`/api/vocabulary-examples?vocabularyId=${vocabularyId.value}`, {
      headers: createAuthHeaders()
    })
    
    if (response.ok) {
      const data = await response.json()
      examples.value = data.items || []
    } else {
      showError('Lỗi tải ví dụ', `Không thể tải danh sách ví dụ. Mã lỗi: ${response.status}`)
    }
  } catch (error) {
    showError('Lỗi kết nối', 'Không thể kết nối đến máy chủ. Vui lòng thử lại.')
    console.error('Failed to load examples:', error)
  }
}

const editExample = (example: Example) => {
  editingExample.value = example
  exampleForm.value = {
    sentence: example.sentence,
    translation: example.translation,
    grammar: example.grammar || ''
  }
}

const deleteExample = async (exampleId: number) => {
  if (!confirm('Bạn có chắc chắn muốn xóa ví dụ này?')) return
  
  try {
    const response = await fetch(`/api/vocabulary-examples/${exampleId}`, {
      method: 'DELETE',
      headers: createAuthHeaders()
    })
    
    if (response.ok) {
      showSuccess('Xóa thành công', 'Ví dụ đã được xóa thành công')
      await loadExamples()
    } else {
      showError('Lỗi xóa ví dụ', `Không thể xóa ví dụ. Mã lỗi: ${response.status}`)
    }
  } catch (error) {
    showError('Lỗi kết nối', 'Không thể kết nối đến máy chủ. Vui lòng thử lại.')
    console.error('Failed to delete example:', error)
  }
}

const saveExample = async () => {
  const isEdit = !!editingExample.value
  const url = isEdit 
    ? `/api/vocabulary-examples/${editingExample.value!.id}`
    : '/api/vocabulary-examples'
  
  const method = isEdit ? 'PUT' : 'POST'
  const payload = {
    ...exampleForm.value,
    vocabularyWordId: vocabularyId.value
  }
  
  try {
    const response = await fetch(url, {
      method,
      headers: createAuthHeaders(),
      body: JSON.stringify(payload)
    })
    
    if (response.ok) {
      showSuccess(
        isEdit ? 'Cập nhật thành công' : 'Thêm thành công',
        isEdit ? 'Ví dụ đã được cập nhật' : 'Ví dụ mới đã được thêm'
      )
      await loadExamples()
      closeModal()
    } else {
      showError(
        isEdit ? 'Lỗi cập nhật' : 'Lỗi thêm ví dụ',
        `Không thể ${isEdit ? 'cập nhật' : 'thêm'} ví dụ. Mã lỗi: ${response.status}`
      )
    }
  } catch (error) {
    showError('Lỗi kết nối', 'Không thể kết nối đến máy chủ. Vui lòng thử lại.')
    console.error('Failed to save example:', error)
  }
}

const closeModal = () => {
  showCreateModal.value = false
  editingExample.value = null
  exampleForm.value = { sentence: '', translation: '', grammar: '' }
}

const closeUploadModal = () => {
  showUploadModal.value = false
}

const handleUploadSuccess = () => {
  showSuccess('Upload thành công', 'Các ví dụ đã được thêm từ file Excel')
  loadExamples()
  closeUploadModal()
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('vi-VN')
}

// Lifecycle
onMounted(() => {
  loadVocabulary()
  loadExamples()
})
</script>

<style scoped>
.manage-examples {
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

.vocabulary-word {
  color: #51cf66;
  font-weight: 700;
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

.examples-list {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.example-card {
  background: rgba(255, 255, 255, 0.08);
  border-radius: 16px;
  padding: 1.5rem;
  border: 1px solid rgba(255, 255, 255, 0.1);
  transition: all 0.3s ease;
  backdrop-filter: blur(10px);
}

.example-card:hover {
  transform: translateY(-2px);
  background: rgba(255, 255, 255, 0.12);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.3);
}

.example-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.example-number {
  background: linear-gradient(135deg, #74c0fc, #339af0);
  color: white;
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-weight: 600;
  font-size: 0.875rem;
}

.example-actions {
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

.example-content {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.section-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: rgba(255, 255, 255, 0.8);
  font-weight: 600;
  margin-bottom: 0.5rem;
  font-size: 0.875rem;
}

.sentence {
  margin: 0;
  padding: 1rem;
  border-radius: 12px;
  line-height: 1.6;
  font-size: 1rem;
}

.sentence.english {
  background: rgba(116, 192, 252, 0.1);
  border-left: 4px solid #74c0fc;
  color: #e3f2fd;
}

.sentence.vietnamese {
  background: rgba(81, 207, 102, 0.1);
  border-left: 4px solid #51cf66;
  color: #e8f5e8;
}

.grammar-explanation {
  margin: 0;
  padding: 1rem;
  background: rgba(255, 193, 7, 0.1);
  border-left: 4px solid #ffc107;
  border-radius: 12px;
  color: #fff3cd;
  line-height: 1.6;
  font-style: italic;
}

.example-footer {
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  padding-top: 1rem;
  margin-top: 1rem;
}

.date-created {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: rgba(255, 255, 255, 0.5);
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
  max-width: 600px;
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

.example-form {
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

.form-textarea {
  width: 100%;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 8px;
  padding: 0.75rem;
  color: white;
  font-size: 1rem;
  transition: all 0.3s ease;
  resize: vertical;
}

.form-textarea:focus {
  outline: none;
  border-color: #74c0fc;
  background: rgba(255, 255, 255, 0.15);
}

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
  .manage-examples {
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
  
  .example-card {
    padding: 1rem;
  }
  
  .page-header h1 {
    font-size: 2rem;
  }
  
  .modal-content {
    margin: 1rem;
    width: calc(100% - 2rem);
  }
  
  .example-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }
}
</style>