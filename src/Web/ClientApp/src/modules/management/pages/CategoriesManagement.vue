<template>
  <div class="manage-categories">
    <div class="page-header">
      <h1>
        <Icon icon="mdi:folder-multiple" class="w-8 h-8 mr-3" />
        Quản lý Danh mục
      </h1>
      <p>Quản lý tất cả danh mục từ vựng trong hệ thống</p>
    </div>

    <div class="action-bar">
      <button @click="showCreateModal = true" class="btn-primary">
        <Icon icon="mdi:plus" class="w-5 h-5 mr-2" />
        Tạo danh mục mới
      </button>
    </div>

    <!-- Category Data Grid -->
    <CategoryDataGrid
      :categories="categories"
      :page-size="pageSize"
      :current-page="currentPage"
      :total-items="totalItems"
      :total-pages="totalPages"
      @detail-category="(category) => navigateToVocabularies(category.id)"
      @edit-category="editCategory"
      @delete-category="(category) => deleteCategory(category.id)"
      @create-category="showCreateModal = true"
      @search="handleSearch"
      @page-change="goToPage"
      @page-size-change="changePageSize"
      @sort-change="handleSort"
    />

    <!-- Create/Edit Modal -->
    <div v-if="showCreateModal || editingCategory" class="modal-overlay" @click="closeModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>{{ editingCategory ? 'Sửa danh mục' : 'Tạo danh mục mới' }}</h2>
          <button @click="closeModal" class="close-btn">
            <Icon icon="mdi:close" class="w-6 h-6" />
          </button>
        </div>
        
        <form @submit.prevent="saveCategory" class="category-form">
          <div class="form-group">
            <label for="categoryName">Tên danh mục *</label>
            <input 
              id="categoryName"
              v-model="categoryForm.name" 
              type="text" 
              required
              placeholder="Nhập tên danh mục..."
              class="form-input"
            />
          </div>
          
          <div class="form-group">
            <label for="categoryDescription">Mô tả</label>
            <textarea 
              id="categoryDescription"
              v-model="categoryForm.description" 
              placeholder="Nhập mô tả danh mục..."
              rows="3"
              class="form-textarea"
            ></textarea>
          </div>
          
          <div class="form-actions">
            <button type="button" @click="closeModal" class="btn-secondary">
              Hủy bỏ
            </button>
            <button type="submit" class="btn-primary" :disabled="!categoryForm.name.trim()">
              {{ editingCategory ? 'Cập nhật' : 'Tạo mới' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Icon } from '@iconify/vue'
import { Routes } from '@/router/constants'
import { getAuthTokenWithFallback, createAuthHeaders } from '@/utils/auth'
import { useNotifications } from '@/utils/notifications'
import CategoryDataGrid from '@/components/ui/CategoryDataGrid.vue'

const router = useRouter()
const { showSuccess, showError } = useNotifications()

interface Category {
  id: number
  name: string
  description?: string
  vocabularyCount: number
  createdAt: string
}

interface CategoryForm {
  name: string
  description: string
}

// Reactive data
const categories = ref<Category[]>([])
const searchQuery = ref('')
const showCreateModal = ref(false)
const editingCategory = ref<Category | null>(null)
const categoryForm = ref<CategoryForm>({
  name: '',
  description: ''
})

// Pagination state
const currentPage = ref(1)
const pageSize = ref(10)
const totalItems = ref(0)
const totalPages = ref(0)
const includeInactive = ref(false)
const sortBy = ref<string>('')
const sortOrder = ref<'asc' | 'desc'>('asc')

// Computed
const filteredCategories = computed(() => {
  if (!searchQuery.value) return categories.value
  
  const query = searchQuery.value.toLowerCase()
  return categories.value.filter(category => 
    category.name.toLowerCase().includes(query) ||
    (category.description && category.description.toLowerCase().includes(query))
  )
})

const visiblePages = computed(() => {
  const pages = []
  const maxVisible = 5
  const halfVisible = Math.floor(maxVisible / 2)
  
  let start = Math.max(1, currentPage.value - halfVisible)
  let end = Math.min(totalPages.value, start + maxVisible - 1)
  
  if (end - start + 1 < maxVisible) {
    start = Math.max(1, end - maxVisible + 1)
  }
  
  for (let i = start; i <= end; i++) {
    pages.push(i)
  }
  
  return pages
})

// Methods
const loadCategories = async (page: number = currentPage.value) => {
  try {
    const token = await getAuthTokenWithFallback()
    console.log('Token:', token ? 'exists' : 'missing')
    
    // Build query parameters
    const params = new URLSearchParams({
      PageNumber: page.toString(),
      PageSize: pageSize.value.toString(),
      IncludeInactive: includeInactive.value.toString()
    })
    
    // Add search parameter
    if (searchQuery.value) {
      params.append('SearchTerm', searchQuery.value)
    }
    
    // Add sort parameters
    if (sortBy.value) {
      params.append('SortBy', sortBy.value)
      params.append('SortOrder', sortOrder.value)
    }
    
    const response = await fetch(`/api/vocabulary-categories?${params}`, {
      headers: {
        'Authorization': `Bearer ${token || ''}`,
        'Content-Type': 'application/json'
      }
    })
    
    console.log('Response status:', response.status)
    
    if (response.ok) {
      const data = await response.json()
      categories.value = data.items || []
      totalItems.value = data.totalCount || 0
      totalPages.value = data.totalPages || 0
      currentPage.value = data.pageNumber || 1
    } else {
      showError('Lỗi tải danh mục', `Không thể tải danh sách danh mục. Mã lỗi: ${response.status}`)
      console.error('Failed response:', response.status, response.statusText)
    }
  } catch (err) {
    showError('Lỗi kết nối', 'Không thể kết nối đến máy chủ. Vui lòng thử lại.')
    console.error('Failed to load categories:', err)
  }
}

const handleSearch = (query: string) => {
  searchQuery.value = query
  currentPage.value = 1
  loadCategories()
}

const handleSort = (sortByParam: string, sortOrderParam: 'asc' | 'desc') => {
  // Update sort state
  sortBy.value = sortByParam
  sortOrder.value = sortOrderParam
  currentPage.value = 1 // Reset to first page when sorting
  loadCategories()
}

const navigateToVocabularies = (categoryId: number) => {
  router.push(`/management/vocabularies/${categoryId}`)
}

// Pagination methods
const goToPage = (page: number) => {
  if (page >= 1 && page <= totalPages.value) {
    loadCategories(page)
  }
}

const nextPage = () => {
  if (currentPage.value < totalPages.value) {
    goToPage(currentPage.value + 1)
  }
}

const prevPage = () => {
  if (currentPage.value > 1) {
    goToPage(currentPage.value - 1)
  }
}

const changePageSize = (newSize: number | string) => {
  pageSize.value = Number(newSize)
  currentPage.value = 1
  loadCategories(1)
}

const toggleIncludeInactive = () => {
  includeInactive.value = !includeInactive.value
  currentPage.value = 1
  loadCategories(1)
}

const editCategory = (category: Category) => {
  editingCategory.value = category
  categoryForm.value = {
    name: category.name,
    description: category.description || ''
  }
}

const deleteCategory = async (categoryId: number) => {
  if (!confirm('Bạn có chắc chắn muốn xóa danh mục này?')) return
  
  try {
    const response = await fetch(`/api/vocabulary-categories/${categoryId}`, {
      method: 'DELETE',
      headers: createAuthHeaders()
    })
    
    if (response.ok) {
      showSuccess('Xóa thành công', 'Danh mục đã được xóa thành công')
      await loadCategories()
    } else {
      showError('Lỗi xóa danh mục', `Không thể xóa danh mục. Mã lỗi: ${response.status}`)
    }
  } catch (err) {
    showError('Lỗi kết nối', 'Không thể kết nối đến máy chủ. Vui lòng thử lại.')
    console.error('Failed to delete category:', err)
  }
}

const saveCategory = async () => {
  const isEdit = !!editingCategory.value
  const url = isEdit 
    ? `/api/vocabulary-categories/${editingCategory.value!.id}`
    : '/api/vocabulary-categories'
  
  const method = isEdit ? 'PUT' : 'POST'
  
  try {
    const response = await fetch(url, {
      method,
      headers: createAuthHeaders(),
      body: JSON.stringify(categoryForm.value)
    })
    
    if (response.ok) {
      showSuccess(
        isEdit ? 'Cập nhật thành công' : 'Tạo thành công',
        isEdit ? 'Danh mục đã được cập nhật' : 'Danh mục mới đã được tạo'
      )
      await loadCategories()
      closeModal()
    } else {
      showError(
        isEdit ? 'Lỗi cập nhật' : 'Lỗi tạo danh mục',
        `Không thể ${isEdit ? 'cập nhật' : 'tạo'} danh mục. Mã lỗi: ${response.status}`
      )
    }
  } catch (err) {
    showError('Lỗi kết nối', 'Không thể kết nối đến máy chủ. Vui lòng thử lại.')
    console.error('Failed to save category:', err)
  }
}

const closeModal = () => {
  showCreateModal.value = false
  editingCategory.value = null
  categoryForm.value = { name: '', description: '' }
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('vi-VN')
}

// Lifecycle
onMounted(() => {
  loadCategories()
})
</script>

<style scoped>
.manage-categories {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
  min-height: 100vh;
}

.page-header {
  text-align: center;
  margin-bottom: 3rem;
}

.page-header h1 {
  color: #74c0fc;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2.5rem;
  margin-bottom: 0.5rem;
}

.page-header p {
  color: rgba(255, 255, 255, 0.7);
  font-size: 1.1rem;
}

.action-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
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

.categories-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 2rem;
  margin-bottom: 2rem;
}

.category-card {
  background: rgba(255, 255, 255, 0.08);
  border-radius: 16px;
  padding: 1.5rem;
  border: 1px solid rgba(255, 255, 255, 0.1);
  cursor: pointer;
  transition: all 0.3s ease;
  backdrop-filter: blur(10px);
}

.category-card:hover {
  transform: translateY(-5px);
  background: rgba(255, 255, 255, 0.12);
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
}

.category-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.category-icon {
  color: #74c0fc;
  background: rgba(116, 192, 252, 0.1);
  padding: 0.75rem;
  border-radius: 12px;
}

.category-actions {
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

.category-content h3 {
  color: white;
  margin: 0 0 0.5rem 0;
  font-size: 1.25rem;
}

.category-content p {
  color: rgba(255, 255, 255, 0.7);
  margin: 0 0 1rem 0;
  line-height: 1.4;
}

.category-stats {
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

.category-footer {
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  padding-top: 1rem;
  text-align: right;
}

.view-details {
  color: #74c0fc;
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

.category-form {
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
  .manage-categories {
    padding: 1rem;
  }
  
  .action-bar {
    flex-direction: column;
    align-items: stretch;
  }
  
  .search-box {
    justify-content: center;
  }
  
  .search-input {
    width: 200px;
  }
  
  .categories-grid {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
  
  .category-card {
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